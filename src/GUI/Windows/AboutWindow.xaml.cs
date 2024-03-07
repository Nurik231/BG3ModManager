using DivinityModManager.ViewModels;

using ReactiveUI;

namespace DivinityModManager.Windows;

public class AboutWindowBase : HideWindowBase<AboutWindowViewModel> { }

/// <summary>
/// Interaction logic for AboutWindow.xaml
/// </summary>
public partial class AboutWindow : AboutWindowBase
{
	public AboutWindow()
	{
		InitializeComponent();

		ViewModel = new AboutWindowViewModel();

		this.WhenActivated(d =>
		{
			d(this.OneWayBind(ViewModel, vm => vm.Title, v => v.TitleText.Text));
			d(this.OneWayBind(ViewModel, vm => vm.Title, v => v.Title));
		});
	}
}
