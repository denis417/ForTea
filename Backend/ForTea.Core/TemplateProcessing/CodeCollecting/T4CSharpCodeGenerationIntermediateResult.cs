using System.Collections.Generic;
using GammaJul.ForTea.Core.TemplateProcessing.CodeCollecting.Descriptions;
using GammaJul.ForTea.Core.TemplateProcessing.CodeCollecting.State;
using GammaJul.ForTea.Core.TemplateProcessing.CodeGeneration;
using GammaJul.ForTea.Core.Tree;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.Tree;

namespace GammaJul.ForTea.Core.TemplateProcessing.CodeCollecting
{
	public sealed class T4CSharpCodeGenerationIntermediateResult
	{
		[NotNull]
		public T4CSharpCodeGenerationResult CollectedBaseClass { get; }

		[NotNull, ItemNotNull]
		private List<T4AppendableElementDescriptionBase> MyTransformationDescriptions { get; }

		[NotNull, ItemNotNull]
		private List<T4AppendableElementDescriptionBase> MyFeatureDescriptions { get; }

		[NotNull, ItemNotNull]
		private List<T4ParameterDescription> MyParameterDescriptions { get; }

		[NotNull, ItemNotNull]
		private List<T4ImportDescription> MyImportDescriptions { get; }

		[NotNull, ItemNotNull]
		public IReadOnlyList<T4ParameterDescription> ParameterDescriptions => MyParameterDescriptions;

		[NotNull, ItemNotNull]
		public IReadOnlyList<T4ImportDescription> ImportDescriptions => MyImportDescriptions;

		[NotNull, ItemNotNull]
		public IReadOnlyList<T4AppendableElementDescriptionBase> FeatureDescriptions => MyFeatureDescriptions;

		[NotNull, ItemNotNull]
		public IReadOnlyList<T4AppendableElementDescriptionBase> TransformationDescriptions =>
			MyTransformationDescriptions;

		public IT4InfoCollectorState State { get; private set; }
		public bool FeatureStarted => State.FeatureStarted;
		public bool HasHost { get; private set; }
		public void AdvanceState([NotNull] ITreeNode element) => State = State.GetNextState(element);
		public void RequireHost() => HasHost = true;
		public bool HasBaseClass => !CollectedBaseClass.IsEmpty;

		public T4CSharpCodeGenerationIntermediateResult([NotNull] IT4File file)
		{
			CollectedBaseClass = new T4CSharpCodeGenerationResult(file);
			MyTransformationDescriptions = new List<T4AppendableElementDescriptionBase>();
			MyFeatureDescriptions = new List<T4AppendableElementDescriptionBase>();
			MyParameterDescriptions = new List<T4ParameterDescription>();
			MyImportDescriptions = new List<T4ImportDescription>();
			State = new T4InfoCollectorStateInitial();
			HasHost = false;
		}

		public void Append([NotNull] T4ParameterDescription description) => MyParameterDescriptions.Add(description);
		public void Append([NotNull] T4ImportDescription description) => MyImportDescriptions.Add(description);

		public void AppendFeature([NotNull] T4AppendableElementDescriptionBase description) =>
			MyFeatureDescriptions.Add(description);

		public void AppendFeature([NotNull] string message) =>
			MyFeatureDescriptions.Add(new T4TextDescription(message));

		public void AppendTransformation([NotNull] T4AppendableElementDescriptionBase description) =>
			MyTransformationDescriptions.Add(description);

		public void AppendTransformation([NotNull] string message) =>
			MyTransformationDescriptions.Add(new T4TextDescription(message));

		public void Append([NotNull] T4CSharpCodeGenerationIntermediateResult other)
		{
			AppendInvisible(MyImportDescriptions, other.ImportDescriptions);
			if (CollectedBaseClass.IsEmpty) CollectedBaseClass.Append(other.CollectedBaseClass);
			AppendInvisible(MyTransformationDescriptions, other.TransformationDescriptions);
			AppendInvisible(MyFeatureDescriptions, other.FeatureDescriptions);
			AppendInvisible(MyParameterDescriptions, other.ParameterDescriptions);
			// 'feature started' is intentionally ignored
			HasHost = HasHost || other.HasHost;
		}

		private void AppendInvisible<T>(
			[NotNull, ItemNotNull] ICollection<T> our,
			[NotNull, ItemNotNull] IEnumerable<T> their
		) where T : T4ElementDescriptionBase
		{
			foreach (var description in their)
			{
				description.MakeInvisible();
				our.Add(description);
			}
		}
	}
}
