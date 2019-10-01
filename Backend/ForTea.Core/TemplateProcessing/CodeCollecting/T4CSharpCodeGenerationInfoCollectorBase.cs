using System;
using System.Collections.Generic;
using GammaJul.ForTea.Core.Psi.Directives;
using GammaJul.ForTea.Core.Psi.Resolve;
using GammaJul.ForTea.Core.Psi.Utils;
using GammaJul.ForTea.Core.TemplateProcessing.CodeCollecting.Descriptions;
using GammaJul.ForTea.Core.TemplateProcessing.CodeCollecting.Interrupt;
using GammaJul.ForTea.Core.Tree;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace GammaJul.ForTea.Core.TemplateProcessing.CodeCollecting
{
	public abstract class T4CSharpCodeGenerationInfoCollectorBase : TreeNodeVisitor, IRecursiveElementProcessor
	{
		#region Properties
		[NotNull]
		private IT4File File { get; }

		[NotNull]
		private T4EncodingsManager EncodingsManager { get; }

		[NotNull]
		private T4IncludeGuard Guard { get; }

		[NotNull, ItemNotNull]
		private Stack<T4CSharpCodeGenerationIntermediateResult> Results { get; }

		private bool HasSeenTemplateDirective { get; set; }

		[NotNull]
		protected T4CSharpCodeGenerationIntermediateResult Result => Results.Peek();
		#endregion Properties

		protected T4CSharpCodeGenerationInfoCollectorBase(
			[NotNull] IT4File file,
			[NotNull] ISolution solution
		)
		{
			File = file;
			Results = new Stack<T4CSharpCodeGenerationIntermediateResult>();
			Guard = new T4IncludeGuard();
			EncodingsManager = solution.GetComponent<T4EncodingsManager>();
		}

		[NotNull]
		public T4CSharpCodeGenerationIntermediateResult Collect()
		{
			var projectFile = File.GetSourceFile()?.ToProjectFile();
			if (projectFile == null) return new T4CSharpCodeGenerationIntermediateResult(File, Interrupter);
			using (T4MacroResolveContextCookie.Create(projectFile))
			{
				Results.Push(new T4CSharpCodeGenerationIntermediateResult(File, Interrupter));
				Guard.StartProcessing(File.GetSourceFile().NotNull());
				File.ProcessDescendants(this);
				string suffix = Result.State.ProduceBeforeEof();
				if (!string.IsNullOrEmpty(suffix)) AppendTransformation(suffix);
				Guard.EndProcessing();
				return Results.Pop();
			}
		}

		#region IRecirsiveElementProcessor
		public bool InteriorShouldBeProcessed(ITreeNode element) => false;

		public void ProcessBeforeInterior(ITreeNode element)
		{
			AppendRemainingMessage(element);
			if (!(element is IT4IncludeDirective include)) return;
			Results.Push(new T4CSharpCodeGenerationIntermediateResult(File, Interrupter));
			var sourceFile = include.Path.Resolve();
			if (sourceFile == null)
			{
				var target = include.GetFirstAttribute(T4DirectiveInfoManager.Include.FileAttribute)?.Value ?? element;
				var data = T4FailureRawData.FromElement(target, $"Unresolved include: {target.GetText()}");
				Interrupter.InterruptAfterProblem(data);
				Guard.StartProcessing(null);
				return;
			}

			if (include.Once && Guard.HasSeenFile(sourceFile)) return;
			if (!Guard.CanProcess(sourceFile))
			{
				var target = include.GetFirstAttribute(T4DirectiveInfoManager.Include.FileAttribute)?.Value ?? element;
				var data = T4FailureRawData.FromElement(target, "Recursion in includes");
				Interrupter.InterruptAfterProblem(data);
				Guard.StartProcessing(sourceFile);
				return;
			}

			var resolved = include.Path.ResolveT4File(Guard).NotNull();
			Guard.StartProcessing(sourceFile);
			var projectFile = sourceFile.ToProjectFile();
			if (projectFile == null) resolved.ProcessDescendants(this);
			else
			{
				using (T4MacroResolveContextCookie.Create(projectFile))
				{
					resolved.ProcessDescendants(this);
				}
			}
		}

		public void ProcessAfterInterior(ITreeNode element)
		{
			if (!(element is IT4TreeNode t4Element)) return;
			t4Element.Accept(this);
			if (t4Element is IT4Token token) Result.State.ConsumeToken(token);
			Result.AdvanceState(t4Element);
		}

		public bool ProcessingIsFinished
		{
			get
			{
				InterruptableActivityCookie.CheckAndThrow();
				return false;
			}
		}
		#endregion IRecirsiveElementProcessor

		#region TreeNodeVisitor
		public override void VisitIncludeDirectiveNode(IT4IncludeDirective includeDirectiveParam)
		{
			string suffix = Result.State.ProduceBeforeEof();
			if (!string.IsNullOrEmpty(suffix)) AppendTransformation(suffix);
			Guard.TryEndProcessing(includeDirectiveParam.Path.Resolve());
			var intermediateResults = Results.Pop();
			Result.Append(intermediateResults);
		}

		public override void VisitAssemblyDirectiveNode(IT4AssemblyDirective assemblyDirectiveParam)
		{
			// TODO: resolve assemblies here
		}

		public override void VisitCleanupBehaviorDirectiveNode(
			IT4CleanupBehaviorDirective cleanupBehaviorDirectiveParam)
		{
			// TODO: should anything be done here?
		}

		public override void VisitImportDirectiveNode(IT4ImportDirective importDirectiveParam)
		{
			var description = T4ImportDescription.FromDirective(importDirectiveParam);
			if (description == null) return;
			Result.Append(description);
		}

		public override void VisitOutputDirectiveNode(IT4OutputDirective outputDirectiveParam) =>
			Result.Encoding = EncodingsManager.FindEncoding(outputDirectiveParam, Interrupter);

		public override void VisitParameterDirectiveNode(IT4ParameterDirective parameterDirectiveParam)
		{
			var description = T4ParameterDescription.FromDirective(parameterDirectiveParam);
			if (description == null) return;
			Result.Append(description);
		}

		public override void VisitTemplateDirectiveNode(IT4TemplateDirective templateDirectiveParam)
		{
			if (HasSeenTemplateDirective) return;
			HasSeenTemplateDirective = true;
			string hostSpecific = templateDirectiveParam
				.GetAttributeValueByName(T4DirectiveInfoManager.Template.HostSpecificAttribute.Name);
			if (bool.TrueString.Equals(hostSpecific, StringComparison.OrdinalIgnoreCase)) Result.RequireHost();

			(ITreeNode classNameToken, string className) = templateDirectiveParam
				.GetAttributeValueIgnoreOnlyWhitespace(T4DirectiveInfoManager.Template.InheritsAttribute.Name);
			if (classNameToken != null && className != null)
				Result.CollectedBaseClass.AppendMapped(className, classNameToken.GetTreeTextRange());
		}

		public override void VisitUnknownDirectiveNode(IT4UnknownDirective unknownDirectiveParam)
		{
			var data = T4FailureRawData.FromElement(unknownDirectiveParam, "Custom directives are not supported");
			Interrupter.InterruptAfterProblem(data);
		}

		public override void VisitExpressionBlockNode(IT4ExpressionBlock expressionBlockParam)
		{
			var code = expressionBlockParam.Code;
			if (code == null) return;
			if (Result.FeatureStarted) Result.AppendFeature(new T4ExpressionDescription(code));
			else Result.AppendTransformation(new T4ExpressionDescription(code));
		}

		public override void VisitFeatureBlockNode(IT4FeatureBlock featureBlockParam)
		{
			var code = featureBlockParam.Code;
			if (code == null) return;
			Result.AppendFeature(new T4CodeDescription(code));
		}

		public override void VisitStatementBlockNode(IT4StatementBlock statementBlockParam)
		{
			var code = statementBlockParam.Code;
			if (code == null) return;
			Result.AppendTransformation(new T4CodeDescription(code));
		}
		#endregion TreeNodeVisitor

		private void AppendRemainingMessage([NotNull] ITreeNode lookahead)
		{
			if (lookahead is IT4Token) return;
			string produced = Result.State.Produce(lookahead);
			if (string.IsNullOrEmpty(produced)) return;
			AppendTransformation(produced);
		}

		protected abstract void AppendTransformation([NotNull] string message);
		protected abstract IT4CodeGenerationInterrupter Interrupter { get; }
	}
}
