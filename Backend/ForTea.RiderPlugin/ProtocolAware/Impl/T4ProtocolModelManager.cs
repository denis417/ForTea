using System;
using System.Linq;
using GammaJul.ForTea.Core.Psi;
using GammaJul.ForTea.Core.Tree;
using JetBrains.Annotations;
using JetBrains.Core;
using JetBrains.ForTea.RiderPlugin.TemplateProcessing.Managing;
using JetBrains.ForTea.RiderPlugin.TemplateProcessing.Managing.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Host.Features;
using JetBrains.ReSharper.Host.Features.ProjectModel.View;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Rider.Model;
using JetBrains.Util;

namespace JetBrains.ForTea.RiderPlugin.ProtocolAware.Impl
{
	[SolutionComponent]
	public sealed class T4ProtocolModelManager
	{
		[NotNull]
		private ILogger Logger { get; }

		[NotNull]
		private T4ProtocolModel Model { get; }

		[NotNull]
		private ISolution Solution { get; }

		[NotNull]
		private IT4TemplateCompiler Compiler { get; }

		[NotNull]
		private IT4TargetFileManager TargetFileManager { get; }

		[NotNull]
		private IT4BuildMessageConverter Converter { get; }

		[NotNull]
		private ProjectModelViewHost Host { get; }

		public T4ProtocolModelManager(
			[NotNull] ISolution solution,
			[NotNull] IT4TargetFileManager targetFileManager,
			[NotNull] IT4TemplateCompiler compiler,
			[NotNull] ILogger logger,
			[NotNull] T4BuildMessageConverter converter,
			[NotNull] ProjectModelViewHost host
		)
		{
			Solution = solution;
			TargetFileManager = targetFileManager;
			Compiler = compiler;
			Logger = logger;
			Converter = converter;
			Host = host;
			Model = solution.GetProtocolSolution().GetT4ProtocolModel();
			RegisterCallbacks();
		}

		private void RegisterCallbacks()
		{
			Model.RequestCompilation.Set(Wrap(Compile, Converter.FatalError()));
			Model.ExecutionSucceeded.Set(Wrap(HandleSuccess, Unit.Instance));
			Model.GetConfiguration.Set(Wrap(CalculateConfiguration, new T4ConfigurationModel("", "")));
		}

		private T4ConfigurationModel CalculateConfiguration([NotNull] IT4File file) => new T4ConfigurationModel(
			TargetFileManager.GetTemporaryExecutableLocation(file).FullPath.Replace("\\", "/"),
			TargetFileManager.GetExpectedTemporaryTargetFileLocation(file).FullPath.Replace("\\", "/")
		);

		private Func<T4FileLocation, T> Wrap<T>(Func<IT4File, T> wrappee, [NotNull] T defaultValue) where T : class =>
			location =>
			{
				var result = Logger.Catch(() =>
				{
					using (ReadLockCookie.Create())
					{
						var file = Host
							.GetItemById<IProjectFile>(location.Id)
							?.ToSourceFile()
							?.GetPsiFiles(T4Language.Instance)
							.OfType<IT4File>()
							.SingleItem();
						return file == null ? null : wrappee(file);
					}
				});
				return result ?? defaultValue;
			};

		private T4BuildResult Compile([NotNull] IT4File t4File)
		{
			using (WriteLockCookie.Create())
			{
				// Interrupt template execution, if any
			}

			return Compiler.Compile(Solution.GetLifetime(), t4File);
		}

		[CanBeNull]
		private Unit HandleSuccess([NotNull] IT4File file)
		{
			var destination = TargetFileManager.CopyExecutionResults(file);
			using (WriteLockCookie.Create())
			{
				TargetFileManager.UpdateProjectModel(file, destination);
			}

			return Unit.Instance;
		}
	}
}