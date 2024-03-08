using DynamicData.Binding;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DivinityModManager.Models.View;
public abstract class TreeViewEntry : ReactiveObject
{
	[Reactive] public bool IsExpanded { get; set; }
	[Reactive] public bool IsSelected { get; set; }

	public ObservableCollectionExtended<TreeViewEntry> Children { get; }

	public abstract object ViewModel { get; }

	public TreeViewEntry()
	{
		Children = [];
	}
}
