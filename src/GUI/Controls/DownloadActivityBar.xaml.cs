using DivinityModManager.ViewModels;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DivinityModManager.Controls
{
	public class DownloadActivityBarBase : ReactiveUserControl<DownloadActivityBarViewModel> { }

	public partial class DownloadActivityBar : DownloadActivityBarBase
	{
		public DownloadActivityBar()
		{
			InitializeComponent();

			this.WhenActivated(d =>
			{
				if(ViewModel != null)
				{
					this.OneWayBind(ViewModel, vm => vm.CurrentValue, view => view.TaskProgressBar.Value);
					this.OneWayBind(ViewModel, vm => vm.CurrentText, view => view.TaskProgressWorkText.Text);
					this.OneWayBind(ViewModel, vm => vm.IsVisible, view => view.Visibility);
					this.BindCommand(ViewModel, vm => vm.CancelCommand, view => view.CancelButton);
				}
			});
		}
	}
}
