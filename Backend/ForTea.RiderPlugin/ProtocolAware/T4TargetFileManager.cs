using System.Collections.Generic;
using GammaJul.ForTea.Core.TemplateProcessing.Managing;
using JetBrains.Annotations;
using JetBrains.Application.Threading;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Host.Features;
using JetBrains.ReSharper.Host.Features.Documents;
using JetBrains.Rider.Model;
using JetBrains.Util;

namespace JetBrains.ForTea.RiderPlugin.ProtocolAware
{
	[SolutionComponent]
	public sealed class T4TargetFileManager : TemplateProcessing.Managing.Impl.T4TargetFileManager
	{
		[NotNull]
		private DocumentHost Host { get; }

		public T4TargetFileManager(
			[NotNull] ISolution solution,
			IT4TargetFileChecker checker,
			[NotNull] DocumentHost host,
			[NotNull] IShellLocks locks
		) : base(solution, checker, locks) => Host = host;

		protected override void SyncDocuments(FileSystemPath destinationLocation) =>
			Host.SyncDocumentsWithFiles(destinationLocation);

		protected override void RefreshFiles(FileSystemPath destinationLocation) => Solution
			.GetProtocolSolution()
			.GetFileSystemModel()
			.RefreshPaths
			.Start(new RdRefreshRequest(new List<string> {destinationLocation.FullPath}, true));
	}
}