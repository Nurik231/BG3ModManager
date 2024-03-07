using DivinityModManager.ViewModels;

using ReactiveUI;

using System.Reactive.Linq;

namespace DivinityModManager.Windows;

public class StatsValidatorWindowBase : HideWindowBase<StatsValidatorWindowViewModel> { }

public partial class StatsValidatorWindow : StatsValidatorWindowBase
{
	private async Task OpenWindow(IInteractionContext<ValidateModStatsResults, bool> context)
	{
		await Observable.Start(() =>
		{
			ViewModel.Load(context.Input);
			App.WM.StatsValidator.Toggle(true);
		}, RxApp.MainThreadScheduler);
		context.SetOutput(true);
	}

	public StatsValidatorWindow()
	{
		InitializeComponent();

		ViewModel = new StatsValidatorWindowViewModel();

		DivinityInteractions.OpenValidateStatsResults.RegisterHandler(OpenWindow);

		this.WhenActivated(d =>
		{
			this.OneWayBind(ViewModel, vm => vm.OutputText, view => view.ResultsTextBlock.Text);
		});
	}
}
