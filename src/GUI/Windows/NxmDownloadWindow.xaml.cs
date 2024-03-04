using DivinityModManager.ViewModels;
using DivinityModManager.Windows;
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
using System.Windows.Shapes;

namespace DivinityModManager.Windows
{
	public class NxmDownloadWindowBase : HideWindowBase<NxmDownloadWindowViewModel> { }
	/// <summary>
	/// Interaction logic for NxmDownloadWindow.xaml
	/// </summary>
	public partial class NxmDownloadWindow : NxmDownloadWindowBase
	{
		public NxmDownloadWindow()
		{
			InitializeComponent();

			ViewModel = new();

			this.WhenActivated(d =>
			{
				this.Bind(ViewModel, vm => vm.Url, view => view.UrlTextBox.Text);
				this.BindCommand(ViewModel, vm => vm.DownloadCommand, view => view.DownloadButton);
			});
		}
	}
}
