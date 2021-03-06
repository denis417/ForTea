using GammaJul.ForTea.Core.Daemon.Highlightings;
using GammaJul.ForTea.Core.Daemon.ProblemAnalyzers;
using GammaJul.ForTea.Core.Psi.Directives;
using GammaJul.ForTea.Core.Psi.Directives.Attributes;
using GammaJul.ForTea.Core.Psi.Resolve.Assemblies;
using GammaJul.ForTea.Core.Tree;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Daemon;

namespace JetBrains.ForTea.RiderPlugin.Daemon.ProblemAnalyzers
{
	[ElementProblemAnalyzer(typeof(IT4AssemblyDirective), HighlightingTypes =
		new[] {typeof(UnresolvedAssemblyWarning)})]
	public sealed class T4UnresolvedPathAnalyzer : T4AttributeValueProblemAnalyzerBase<IT4AssemblyDirective>
	{
		[NotNull]
		private IT4AssemblyReferenceResolver AssemblyReferenceResolver { get; }

		[NotNull]
		private IT4ProjectReferenceResolver ProjectReferenceResolver { get; }

		public T4UnresolvedPathAnalyzer(
			[NotNull] IT4AssemblyReferenceResolver resolver,
			[NotNull] IT4ProjectReferenceResolver projectReferenceResolver
		)
		{
			AssemblyReferenceResolver = resolver;
			ProjectReferenceResolver = projectReferenceResolver;
		}

		protected override void DoRun(IT4AttributeValue element, IHighlightingConsumer consumer)
		{
			var attribute = DirectiveAttributeNavigator.GetByValue(element);
			if (!(DirectiveNavigator.GetByAttribute(attribute) is IT4AssemblyDirective assemblyDirective)) return;
			var project = ProjectReferenceResolver.TryResolveProject(assemblyDirective.Path.ResolvePath());
			if (project != null) return;
			var path = AssemblyReferenceResolver.Resolve(assemblyDirective);
			if (path != null && path.ExistsFile) return;
			consumer.AddHighlighting(new UnresolvedAssemblyWarning(assemblyDirective.Name));
		}

		protected override DirectiveAttributeInfo GetTargetAttribute() => T4DirectiveInfoManager.Assembly.NameAttribute;
	}
}
