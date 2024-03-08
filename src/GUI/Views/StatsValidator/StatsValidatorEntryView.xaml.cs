using DivinityModManager.Models.View;
using ReactiveUI;

using System.Windows.Media;

namespace DivinityModManager.Views.StatsValidator;

public class StatsValidatorEntryViewBase : ReactiveUserControl<StatsValidatorErrorEntry> { }

/// <summary>
/// Interaction logic for StatsValidatorFileResultsView.xaml
/// </summary>
public partial class StatsValidatorEntryView : StatsValidatorEntryViewBase
{
	private static Brush IsErrorToForeground(bool isError) => isError ? Brushes.Red : Brushes.Yellow;

	public StatsValidatorEntryView()
	{
		InitializeComponent();

		this.OneWayBind(ViewModel, vm => vm.Message, view => view.MessageTextBlock.Text);
		this.OneWayBind(ViewModel, vm => vm.IsError, view => view.MessageTextBlock.Foreground, IsErrorToForeground);
	}
}
