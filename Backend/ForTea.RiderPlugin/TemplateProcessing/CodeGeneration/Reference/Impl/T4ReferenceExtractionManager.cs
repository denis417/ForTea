using System.Collections.Generic;
using System.Linq;
using Debugger.Common.MetadataAndPdb;
using GammaJul.ForTea.Core.Psi.Modules;
using GammaJul.ForTea.Core.Psi.Resolve;
using GammaJul.ForTea.Core.Psi.Resolve.Assemblies;
using GammaJul.ForTea.Core.TemplateProcessing.CodeCollecting.Interrupt;
using GammaJul.ForTea.Core.TemplateProcessing.CodeGeneration.Reference;
using GammaJul.ForTea.Core.Tree;
using JetBrains.Annotations;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Model2.Assemblies.Interfaces;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using JetBrains.Util.dataStructures;
using Microsoft.CodeAnalysis;

namespace JetBrains.ForTea.RiderPlugin.TemplateProcessing.CodeGeneration.Reference.Impl
{
	[SolutionComponent]
	public sealed class T4ReferenceExtractionManager : IT4ReferenceExtractionManager
	{
		[NotNull]
		private RoslynMetadataReferenceCache Cache { get; }

		[NotNull]
		private IPsiModules PsiModules { get; }

		[NotNull]
		private IT4AssemblyReferenceResolver AssemblyReferenceResolver { get; }

		public T4ReferenceExtractionManager(
			Lifetime lifetime,
			[NotNull] IPsiModules psiModules,
			[NotNull] IT4AssemblyReferenceResolver assemblyReferenceResolver
		)
		{
			PsiModules = psiModules;
			AssemblyReferenceResolver = assemblyReferenceResolver;
			Cache = new RoslynMetadataReferenceCache(lifetime);
		}

		public IEnumerable<MetadataReference> ExtractPortableReferencesTransitive(Lifetime lifetime, IT4File file)
		{
			Assertion.Assert(file.PhysicalPsiSourceFile == file.LogicalPsiSourceFile,
				"file.PhysicalPsiSourceFile == file.LogicalPsiSourceFile");
			var sourceFile = file.PhysicalPsiSourceFile.NotNull();
			var projectFile = sourceFile.ToProjectFile().NotNull();
			var directives = file.Children<IT4AssemblyDirective>();
			var errors = new FrugalLocalList<T4FailureRawData>();
			using (T4MacroResolveContextCookie.GetOrCreate(projectFile))
			{
				var directDependencies = directives.SelectNotNull(directive =>
				{
					var resolved = AssemblyReferenceResolver.Resolve(directive);
					if (resolved == null)
					{
						errors.Add(T4FailureRawData.FromElement(directive, "Unresolved assembly reference"));
					}

					return resolved;
				}).AsList();

				if (!errors.IsEmpty) throw new T4OutputGenerationException(errors);
				var result = AssemblyReferenceResolver.ResolveTransitiveDependencies(
					directDependencies,
					projectFile.SelectResolveContext()
				).Select(path => Cache.GetMetadataReference(lifetime, path)).AsList<MetadataReference>();
				AddBaseReferences(lifetime, result, sourceFile);
				return result;
			}
		}

		private void AddBaseReferences(
			Lifetime lifetime,
			[NotNull, ItemNotNull] List<MetadataReference> result,
			[NotNull] IPsiSourceFile sourceFile
		)
		{
			TryAddReference(lifetime, result, sourceFile, "mscorlib");
			TryAddReference(lifetime, result, sourceFile, "System");
		}

		private void TryAddReference(
			Lifetime lifetime,
			[NotNull, ItemNotNull] List<MetadataReference> result,
			[NotNull] IPsiSourceFile sourceFile,
			[NotNull] string assemblyName
		)
		{
			var resolved = AssemblyReferenceResolver.Resolve(assemblyName, sourceFile);
			if (resolved == null) return;
			var metadataReference = Cache.GetMetadataReference(lifetime, resolved);
			if (metadataReference == null) return;
			result.Add(metadataReference);
		}

		public IEnumerable<T4AssemblyReferenceInfo> ExtractReferenceLocationsTransitive(IT4File file)
		{
			var directReferences = ExtractRawAssemblyReferences(file).Select(assemblyFile =>
				new T4AssemblyReferenceInfo(assemblyFile.AssemblyName?.FullName ?? "", assemblyFile.Location)
			);
			var sourceFile = file.GetSourceFile().NotNull();
			var projectFile = sourceFile.ToProjectFile().NotNull();
			var resolveContext = projectFile.SelectResolveContext();
			return AssemblyReferenceResolver
				.ResolveTransitiveDependencies(directReferences, resolveContext)
				.AsList();
		}

		[NotNull, ItemNotNull]
		private static IEnumerable<IAssemblyFile> ExtractRawAssemblyReferences([NotNull] IT4File file)
		{
			var sourceFile = file.GetSourceFile().NotNull();
			var projectFile = sourceFile.ToProjectFile().NotNull();
			if (!(sourceFile.PsiModule is IT4FilePsiModule psiModule)) return EmptyList<IAssemblyFile>.Enumerable;
			var resolveContext = projectFile.SelectResolveContext();
			using (CompilationContextCookie.GetOrCreate(resolveContext))
			{
				return psiModule.RawReferences.SelectMany(it => it.GetFiles()).AsList();
			}
		}

		public IEnumerable<IProject> GetProjectDependencies(IT4File file)
		{
			var sourceFile = file.GetSourceFile().NotNull();
			var projectFile = sourceFile.ToProjectFile().NotNull();
			var psiModule = sourceFile.PsiModule;
			var resolveContext = projectFile.SelectResolveContext();
			using (CompilationContextCookie.GetOrCreate(resolveContext))
			{
				return PsiModules
					.GetModuleReferences(psiModule)
					.Select(it => it.Module)
					.OfType<IProjectPsiModule>()
					.Select(it => it.Project)
					.AsList();
			}
		}
	}
}
