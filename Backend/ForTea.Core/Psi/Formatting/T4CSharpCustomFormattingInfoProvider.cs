using GammaJul.ForTea.Core.TemplateProcessing.CodeCollecting.Format;
using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Psi.CSharp.CodeStyle.FormatSettings;
using JetBrains.ReSharper.Psi.CSharp.Impl.CodeStyle;
using JetBrains.ReSharper.Psi.Impl.CodeStyle;
using JetBrains.ReSharper.Psi.Tree;

namespace GammaJul.ForTea.Core.Psi.Formatting
{
	[ShellComponent]
	public class T4CSharpCustomFormattingInfoProvider : DummyCSharpCustomFormattingInfoProvider
	{
		public override FmtSettings<CSharpFormatSettingsKey> AdjustFormattingSettings(
			FmtSettings<CSharpFormatSettingsKey> settings,
			ISettingsOptimization settingsOptimization
		)
		{
			var cSharpFormatSettings = settings.Settings.Clone();
			cSharpFormatSettings.INDENT_SIZE = 4; // TODO: remove!
			cSharpFormatSettings.OLD_ENGINE = true;
			return settings.ChangeMainSettings(cSharpFormatSettings, true);
		}

		public override SpaceType GetBlockSpaceType(CSharpFmtStageContext ctx, CSharpCodeFormattingContext context)
		{
			// Do not break one-line code blocks
			var leftChild = ctx.LeftChild;
			var rightChild = ctx.RightChild;
			if (IsCodeStartComment(leftChild) && !leftChild.HasLineFeedsTo(rightChild, context.CodeFormatter))
				return SpaceType.Horizontal;
			if (IsCodeEndComment(rightChild) && !leftChild.HasLineFeedsTo(rightChild, context.CodeFormatter))
				return SpaceType.Horizontal;
			return SpaceType.Default;
		}

		private static bool IsCodeEndComment(ITreeNode rightChild)
		{
			if (!(rightChild is ICommentNode)) return false;
			return rightChild.GetText() == T4CodeBehindFormatProvider.Instance.CodeCommentEnd;
		}

		private static bool IsCodeStartComment(ITreeNode candidate)
		{
			if (!(candidate is ICommentNode)) return false;
			return candidate.GetText() == T4CodeBehindFormatProvider.Instance.CodeCommentStart;
		}
	}
}
