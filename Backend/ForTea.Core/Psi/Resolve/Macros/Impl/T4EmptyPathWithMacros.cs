using GammaJul.ForTea.Core.Tree;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;

namespace GammaJul.ForTea.Core.Psi.Resolve.Macros.Impl
{
	public sealed class T4EmptyPathWithMacros : IT4PathWithMacros
	{
		private T4EmptyPathWithMacros()
		{
		}

		public static IT4PathWithMacros Instance { get; } = new T4EmptyPathWithMacros();
		public IPsiSourceFile Resolve() => null;
		public IT4File ResolveT4File(T4IncludeRecursionGuard guard) => null;
		public FileSystemPath ResolvePath() => FileSystemPath.Empty;
		public bool IsEmpty => true;
	}
}
