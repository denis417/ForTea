using System.Text;
using GammaJul.ForTea.Core.Tree;
using GammaJul.ForTea.Core.Tree.Impl;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.Tree;

namespace GammaJul.ForTea.Core.TemplateProcessing.CodeCollecting.State
{
	public sealed class T4InfoCollectorStateInitial : T4InfoCollectorStateBase
	{
		[NotNull]
		private StringBuilder Builder { get; }

		public T4InfoCollectorStateInitial() : this(new StringBuilder())
		{
		}

		public T4InfoCollectorStateInitial([NotNull] StringBuilder builder) => Builder = builder;

		protected override IT4InfoCollectorState GetNextStateSafe(ITreeNode element)
		{
			switch (element)
			{
				case T4FeatureBlock _:
					Die();
					return new T4InfoCollectorStateSeenFeature();
				case IT4Directive _:
				case T4StatementBlock _:
					Die();
					return new T4InfoCollectorSateSeenDirectiveOrStatementBlock();
				case T4ExpressionBlock _:
					Die();
					return new T4InfoCollectorStateInitial();
				default: return this;
			}
		}

		protected override bool FeatureStartedSafe => false;
		protected override void ConsumeTokenSafe(IT4Token token) => Builder.Append(Convert(token));
		protected override string ProduceSafe(ITreeNode lookahead) => Builder.ToString();
		protected override string ProduceBeforeEofSafe() => Builder.ToString();
	}
}