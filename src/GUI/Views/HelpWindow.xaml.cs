using DivinityModManager.Controls;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

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

namespace DivinityModManager.Views
{
	public class HelpWindowBase : HideWindowBase<HelpWindowViewModel> { }

	public class HelpWindowViewModel : ReactiveObject
	{
		[Reactive] public string WindowTitle { get; set; }
		[Reactive] public string HelpTitle { get; set; }
		[Reactive] public string HelpText { get; set; }

		public HelpWindowViewModel()
		{
			WindowTitle = "Help";
			HelpTitle = "";
			HelpText = "";
		}
	}

	public partial class HelpWindow : HelpWindowBase
	{
		private readonly Lazy<Markdown> _fallbackMarkdown = new(() => new Markdown());
		private Markdown _defaultMarkdown;

		private FlowDocument StringToMarkdown(string text)
		{
			var markdown = _defaultMarkdown ?? _fallbackMarkdown.Value;
			var doc = markdown.Transform(text);
			return doc;
		}

		public HelpWindow()
        {
            InitializeComponent();

			ViewModel = new HelpWindowViewModel();

			this.WhenActivated(d =>
			{
				var obj = TryFindResource("DefaultMarkdown");
				if (obj != null && obj is Markdown markdown)
				{
					_defaultMarkdown = markdown;
				}

				d(this.OneWayBind(ViewModel, vm => vm.WindowTitle, v => v.Title));
				d(this.OneWayBind(ViewModel, vm => vm.HelpTitle, v => v.HelpTitleText.Text));
				d(this.OneWayBind(ViewModel, vm => vm.HelpText, v => v.MarkdownViewer.Document, StringToMarkdown));
			});
		}
    }
}
