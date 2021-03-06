using System.Collections.Generic;
using GammaJul.ForTea.Core.Parsing.Ranges;
using GammaJul.ForTea.Core.TemplateProcessing.CodeCollecting;
using GammaJul.ForTea.Core.TemplateProcessing.CodeCollecting.Descriptions;
using GammaJul.ForTea.Core.Tree;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Util;

namespace GammaJul.ForTea.Core.TemplateProcessing.CodeGeneration.Converters
{
	public class T4CSharpIntermediateConverter : T4CSharpIntermediateConverterBase
	{
		public T4CSharpIntermediateConverter(
			[NotNull] T4CSharpCodeGenerationIntermediateResult intermediateResult,
			[NotNull] IT4File file
		) : base(intermediateResult, file)
		{
		}

		protected sealed override string BaseClassResourceName => "GammaJul.ForTea.Core.Resources.TemplateBaseFull.cs";

		protected sealed override void AppendSyntheticAttribute()
		{
			// Synthetic attribute is only used for avoiding completion.
			// It is not valid during compilation,
			// so it should not be inserted in code
		}

		protected sealed override void AppendParameterInitialization(
			IReadOnlyCollection<T4ParameterDescription> descriptions
		)
		{
			AppendIndent();
			Result.AppendLine("if (Errors.HasErrors) return;");
			foreach (var description in descriptions)
			{
				AppendIndent();
				Result.Append("if (Session.ContainsKey(nameof(");
				Result.Append(description.FieldNameString);
				Result.AppendLine(")))");
				AppendIndent();
				Result.AppendLine("{");
				PushIndent();
				AppendIndent();
				Result.Append(description.FieldNameString);
				Result.Append(" = (");
				Result.Append(description.TypeString);
				Result.Append(") Session[nameof(");
				Result.Append(description.FieldNameString);
				Result.AppendLine(")];");
				PopIndent();
				AppendIndent();
				Result.AppendLine("}");
				AppendIndent();
				Result.AppendLine("else");
				AppendIndent();
				Result.AppendLine("{");
				PushIndent();
				AppendIndent();
				Result.Append(
					"object data = global::System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(nameof(");
				Result.Append(description.FieldNameString);
				Result.AppendLine("));");
				AppendIndent();
				Result.AppendLine("if (data != null)");
				AppendIndent();
				Result.AppendLine("{");
				PushIndent();
				AppendIndent();
				Result.Append(description.FieldNameString);
				Result.Append(" = (");
				Result.Append(description.TypeString);
				Result.AppendLine(") data;");
				PopIndent();
				AppendIndent();
				Result.AppendLine("}");
				PopIndent();
				AppendIndent();
				Result.AppendLine("}");
			}
		}

		protected override void AppendClass()
		{
			AppendIndent();
			Result.AppendLine();
			AppendClassSummary();
			AppendIndent();
			Result.AppendLine();
			AppendIndent();
			Result.AppendLine($"#line 1 \"{File.GetSourceFile().GetLocation()}\"");
			AppendIndent();
			Result.AppendLine(
				"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"JetBrains.ForTea.TextTemplating\", \"42.42.42.42\")]");
			base.AppendClass();
		}

		protected override void AppendTransformMethod()
		{
			Result.AppendLine("#line hidden");
			AppendIndent();
			Result.AppendLine("/// <summary>");
			AppendIndent();
			Result.AppendLine("/// Create the template output");
			AppendIndent();
			Result.AppendLine("/// </summary>");
			base.AppendTransformMethod();
		}

		private void AppendClassSummary()
		{
			AppendIndent();
			Result.AppendLine("/// <summary>");
			AppendIndent();
			Result.AppendLine("/// Class to produce the template output");
			AppendIndent();
			Result.AppendLine("/// </summary>");
		}

		protected override void AppendGeneratedMessage() =>
			Result.Append(@"// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by JetBrains T4 Processor
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
");

		protected override void AppendParameterDeclaration(T4ParameterDescription description)
		{
			// Range maps of this converter are ignored, so it's safe to use Append instead of AppendMapped
			AppendIndent();
			Result.Append("private ");
			Result.Append(description.TypeString);
			Result.Append(" ");
			Result.Append(description.NameString);
			Result.Append(" => ");
			Result.Append(description.FieldNameString);
			Result.AppendLine(";");
		}

		protected override void AppendHost()
		{
			// Host directive does not work for runtime templates
		}

		protected override string GeneratedClassName
		{
			get
			{
				File.AssertContainsNoIncludeContext();
				string fileName = File.LogicalPsiSourceFile.Name.WithoutExtension();
				if (ValidityChecker.IsValidIdentifier(fileName)) return fileName;
				return GeneratedClassNameString;
			}
		}

		protected override string GeneratedBaseClassName => GeneratedClassName + "Base";

		protected override void AppendIndent(int size)
		{
			// TODO: use user indents?
			for (int index = 0; index < size; index += 1)
			{
				Result.Append("    ");
			}
		}

		#region IT4ElementAppendFormatProvider
		public override string CodeCommentStart => "";
		public override string CodeCommentEnd => "";
		public override string ExpressionCommentStart => "";
		public override string ExpressionCommentEnd => "";
		public override string Indent => new string(' ', CurrentIndent * 4); // TODO: use user indents?
		public override bool ShouldBreakExpressionWithLineDirective => false;

		public override void AppendCompilationOffset(T4CSharpCodeGenerationResult destination, IT4TreeNode node)
		{
			// In preprocessed file, behave like VS
		}

		public override void AppendLineDirective(T4CSharpCodeGenerationResult destination, IT4TreeNode node)
		{
			var sourceFile = node.FindLogicalPsiSourceFile();
			int offset = T4UnsafeManualRangeTranslationUtil.GetDocumentStartOffset(node).Offset;
			int line = (int) sourceFile.Document.GetCoordsByOffset(offset).Line;
			destination.AppendLine($"#line {line + 1} \"{sourceFile.GetLocation()}\"");
		}

		public override void AppendMappedIfNeeded(T4CSharpCodeGenerationResult destination, IT4Code code) =>
			destination.Append(code.GetText().Trim());
		#endregion IT4ElementAppendFormatProvider
	}
}
