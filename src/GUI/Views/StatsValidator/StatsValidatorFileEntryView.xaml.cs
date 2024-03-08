using DivinityModManager.Models.View;
using ReactiveUI;

using System.Windows;
using System.Windows.Media;

namespace DivinityModManager.Views.StatsValidator;

public class StatsValidatorFileEntryViewBase : ReactiveUserControl<StatsValidatorFileResults> { }
/// <summary>
/// Interaction logic for StatsValidatorFileEntryView.xaml
/// </summary>
public partial class StatsValidatorFileEntryView : StatsValidatorFileEntryViewBase
{
	private Brush HasErrorToForeground(bool isError)
	{
		if(!isError)
		{
			return Application.Current.TryFindResource(AdonisUI.Brushes.ForegroundBrush) as Brush;
		}
		return Brushes.Red;
	}

	public StatsValidatorFileEntryView()
	{
		InitializeComponent();

		this.OneWayBind(ViewModel, vm => vm.DisplayName, view => view.NameTextBlock.Text);
		this.OneWayBind(ViewModel, vm => vm.ToolTip, view => view.NameTextBlock.ToolTip);
		this.OneWayBind(ViewModel, vm => vm.HasErrors, view => view.NameTextBlock.Foreground, HasErrorToForeground);
	}
}
