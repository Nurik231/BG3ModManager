using AdonisUI;

using DivinityModManager.ViewModels;

using Microsoft.Windows.Themes;

using ReactiveUI;

using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DivinityModManager.Views;

public class ModUpdatesLayoutBase : ReactiveUserControl<ModUpdatesViewData> { }

public partial class ModUpdatesLayout : ModUpdatesLayoutBase
{
	private static readonly GridLength CollapsedLength = new(0, GridUnitType.Pixel);
	private static readonly GridLength StarLength = new(1, GridUnitType.Star);

	public ModUpdatesLayout()
	{
		InitializeComponent();

		Loaded += ModUpdatesLayout_Loaded;

		this.WhenActivated(d =>
		{
			this.OneWayBind(ViewModel, vm => vm.Unlocked, view => view.IsManipulationEnabled);
			this.OneWayBind(ViewModel, vm => vm.Unlocked, view => view.IsEnabled);
			this.OneWayBind(ViewModel, vm => vm.UpdatesHeaderVisiblity, view => view.UpdatesHeaderTextBlock.Visibility);

			this.OneWayBind(ViewModel, vm => vm.NewAvailable, view => view.NewFilesModListView.IsEnabled);
			this.OneWayBind(ViewModel, vm => vm.NewMods, view => view.NewFilesModListView.ItemsSource);

			this.OneWayBind(ViewModel, vm => vm.UpdatesAvailable, view => view.UpdatesModListView.IsEnabled);
			this.OneWayBind(ViewModel, vm => vm.UpdatedMods, view => view.UpdatesModListView.ItemsSource);

			this.BindCommand(ViewModel, vm => vm.UpdateModsCommand, view => view.UpdateButton);

			this.OneWayBind(ViewModel, vm => vm.AllNewModsSelected, view => view.NewFilesModListViewCheckboxHeader.IsChecked);
			this.BindCommand(ViewModel, vm => vm.SelectAllNewModsCommand, view => view.NewFilesModListViewCheckboxHeader);

			this.OneWayBind(ViewModel, vm => vm.AllModUpdatesSelected, view => view.ModUpdatesCheckboxHeader.IsChecked);
			this.BindCommand(ViewModel, vm => vm.SelectAllUpdatesCommand, view => view.ModUpdatesCheckboxHeader);

			ViewModel.WhenAnyValue(x => x.NewAvailable).Select(b => b ? StarLength : CollapsedLength).BindTo(NewModsGridRow, x => x.Height);
			ViewModel.WhenAnyValue(x => x.UpdatesAvailable).Select(b => b ? StarLength : CollapsedLength).BindTo(UpdatesGridRow, x => x.Height);
		});
	}

	private static readonly List<string> _ignoreColors = new() { "#FFEDEDED", "#00FFFFFF", "#FFFFFFFF", "#FFF4F4F4", "#FFE8E8E8", "#FF000000" };

	public void UpdateBackgroundColors()
	{
		//Fix for IsEnabled False ListView having a system color border background we can't change.
		foreach (var border in this.FindVisualChildren<ClassicBorderDecorator>())
		{
			border.SetResourceReference(BackgroundProperty, Brushes.Layer4BackgroundBrush);
		}
	}
	private void ModUpdatesLayout_Loaded(object sender, RoutedEventArgs e)
	{
		UpdateBackgroundColors();
	}

	GridViewColumnHeader _lastHeaderClicked = null;
	ListSortDirection _lastDirection = ListSortDirection.Ascending;

	private void Sort(string sortBy, ListSortDirection direction, object sender, bool modUpdatesGrid = false)
	{
		if (sortBy == "Version" || sortBy == "Current") sortBy = "Version.Version";
		if (sortBy == "New") sortBy = "UpdatedMod.Version.Version";
		if (sortBy == "#") sortBy = "Index";

		if (modUpdatesGrid && sortBy != "IsSelected" && sortBy != "UpdatedMod.Version.Version")
		{
			sortBy = "Mod." + sortBy;
		}

		if (sortBy != "")
		{
			try
			{
				ListView lv = sender as ListView;
				ICollectionView dataView =
					CollectionViewSource.GetDefaultView(lv.ItemsSource);

				dataView.SortDescriptions.Clear();
				SortDescription sd = new(sortBy, direction);
				dataView.SortDescriptions.Add(sd);
				dataView.Refresh();
			}
			catch (Exception ex)
			{
				DivinityApp.Log("Error sorting grid: " + ex.ToString());
			}
		}
	}

	private void SortGrid(object sender, RoutedEventArgs e, bool modUpdatesGrid = false)
	{
		ListSortDirection direction;

		if (e.OriginalSource is GridViewColumnHeader headerClicked)
		{
			if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
			{
				if (headerClicked != _lastHeaderClicked)
				{
					direction = ListSortDirection.Ascending;
				}
				else
				{
					if (_lastDirection == ListSortDirection.Ascending)
					{
						direction = ListSortDirection.Descending;
					}
					else
					{
						direction = ListSortDirection.Ascending;
					}
				}

				string header = "";

				if (headerClicked.Column.Header is TextBlock textBlock)
				{
					header = textBlock.Text;
				}
				else if (headerClicked.Column.Header is string gridHeader)
				{
					header = gridHeader;
				}
				else if (headerClicked.Column.Header is CheckBox)
				{
					header = "IsSelected";
				}
				else if (headerClicked.Column.Header is Control c && c.ToolTip is string toolTip)
				{
					header = toolTip;
				}

				Sort(header, direction, sender, modUpdatesGrid);

				_lastHeaderClicked = headerClicked;
				_lastDirection = direction;
			}
		}
	}

	private void SortNewModsGridView(object sender, RoutedEventArgs e)
	{
		SortGrid(sender, e);
	}

	private void SortModUpdatesGridView(object sender, RoutedEventArgs e)
	{
		SortGrid(sender, e, true);
	}
}
