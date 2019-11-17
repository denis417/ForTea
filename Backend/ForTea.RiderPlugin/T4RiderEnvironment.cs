using System;
using System.Collections.Generic;
using System.Linq;
using GammaJul.ForTea.Core;
using JetBrains.Application;
using JetBrains.Application.platforms;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.Util;
using JetBrains.Util.Dotnet.TargetFrameworkIds;
using Microsoft.VisualStudio.TextTemplating;

namespace JetBrains.ForTea.RiderPlugin
{
	[ShellComponent]
	public sealed class T4RiderEnvironment : T4DefaultEnvironment
	{
		public override TargetFrameworkId TargetFrameworkId { get; } =
			TargetFrameworkId.Create(FrameworkIdentifier.NetFramework, new Version(4, 7, 2));

		public override CSharpLanguageLevel CSharpLanguageLevel => CSharpLanguageLevel.Latest;

		public override IEnumerable<string> DefaultAssemblyNames =>
			base.DefaultAssemblyNames.Concat(typeof(ITextTemplatingEngineHost).Assembly.Location);


		public override bool IsSupported => true;
	}
}
