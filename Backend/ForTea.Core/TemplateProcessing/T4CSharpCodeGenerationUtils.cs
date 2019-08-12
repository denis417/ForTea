using System;
using System.Diagnostics;
using System.Linq;
using GammaJul.ForTea.Core.Psi.Directives;
using GammaJul.ForTea.Core.Tree;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.Util;

namespace GammaJul.ForTea.Core.TemplateProcessing
{
	public static class T4CSharpCodeGenerationUtils
	{
		[NotNull] public const string DefaultTargetExtension = "cs";

		[NotNull]
		// name is NOT supposed to contain extension
		public static string WithExtension([NotNull] this string name, [NotNull] string extension)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (extension == null) throw new ArgumentNullException(nameof(extension));
			return name + '.' + extension.WithoutLeadingDot();
		}

		[NotNull]
		public static string WithoutLeadingDot([NotNull] this string extension)
		{
			if (extension == null) throw new ArgumentNullException(nameof(extension));
			if (!extension.StartsWith(".", StringComparison.Ordinal)) return extension;
			return extension.Substring(1);
		}

		[NotNull]
		// name is supposed to contain extension
		public static string WithOtherExtension([NotNull] this string name, [NotNull] string newExtension)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (newExtension == null) throw new ArgumentNullException(nameof(newExtension));
			return name.WithoutExtension().WithExtension(newExtension);
		}

		[NotNull]
		public static string WithoutExtension([NotNull] this string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			int dotIndex = name.LastIndexOf('.');
			return dotIndex < 0 ? name : name.Substring(0, dotIndex);
		}

		[NotNull]
		public static string EscapeKeyword([NotNull] this string s)
		{
			if (!CSharpLexer.IsKeyword(s)) return s;
			return '@' + s;
		}

		/// <returns>
		/// Target extension. Leading dot, if any, is removed.
		/// </returns>
		[NotNull]
		public static string GetTargetExtension(
			[NotNull] this IT4File file,
			[NotNull] T4DirectiveInfoManager manager
		)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			if (manager == null) throw new ArgumentNullException(nameof(manager));

			OutputDirectiveInfo output = manager.Output;
			var attributes = file
				.GetDirectives(output)
				.SelectMany(outputDirective => outputDirective.GetAttributes())
				.Where(attribute => string.Equals(attribute.GetName(), output.ExtensionAttribute.Name));
			var query = attributes.Select(attribute => attribute.GetValue());

			string targetExtension = query.FirstOrDefault();

			if (targetExtension == null) return DefaultTargetExtension;

			return targetExtension.StartsWith(".", StringComparison.Ordinal)
				? targetExtension.Substring(1)
				: targetExtension;
		}

		public static int WaitForExitSpinning(
			[NotNull] this Process process,
			int interval,
			[CanBeNull] IProgressIndicator indicator
		)
		{
			if (process == null) throw new ArgumentNullException(nameof(process));
			while (!process.WaitForExit(interval))
			{
				InterruptableActivityCookie.CheckAndThrow();
				if (indicator?.IsCanceled != true) continue;
				process.KillTree();
				throw new OperationCanceledException();
			}

			return process.ExitCode;
		}
	}
}
