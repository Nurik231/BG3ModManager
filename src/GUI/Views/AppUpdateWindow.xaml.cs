using AutoUpdaterDotNET;

using DivinityModManager.Controls;
using DivinityModManager.Util;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;

namespace DivinityModManager.Views
{
	public class AppUpdateWindowBase : HideWindowBase<AppUpdateWindowViewModel> { }

	public class AppUpdateWindowViewModel : ReactiveObject
	{
		private readonly AppUpdateWindow _view;

		public UpdateInfoEventArgs UpdateArgs { get; set; }

		[Reactive] public bool CanConfirm { get; set; }
		[Reactive] public bool CanSkip { get; set; }
		[Reactive] public string SkipButtonText { get; set; }
		[Reactive] public string UpdateDescription { get; set; }
		[Reactive] public string UpdateChangelogView { get; set; }

		public ICommand ConfirmCommand { get; private set; }
		public ICommand SkipCommand { get; private set; }

		public void CheckArgs(UpdateInfoEventArgs args)
		{
			if (args == null) return;
			UpdateArgs = args;
			//Title = $"{AutoUpdater.AppTitle} {args.CurrentVersion}";

			string markdownText;

			if (!args.ChangelogURL.EndsWith(".md"))
			{
				markdownText = WebHelper.DownloadUrlAsString(DivinityApp.URL_CHANGELOG_RAW);
			}
			else
			{
				markdownText = WebHelper.DownloadUrlAsString(args.ChangelogURL);
			}
			if (!String.IsNullOrEmpty(markdownText))
			{
				markdownText = Regex.Replace(markdownText, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
				UpdateChangelogView = markdownText;
			}

			if (args.IsUpdateAvailable)
			{
				UpdateDescription = $"{AutoUpdater.AppTitle} {args.CurrentVersion} is now available.{Environment.NewLine}You have version {args.InstalledVersion} installed.";

				CanConfirm = true;
				SkipButtonText = "Skip";
				CanSkip = args.Mandatory?.Value != true;
			}
			else
			{
				UpdateDescription = $"{AutoUpdater.AppTitle} is up-to-date.";
				CanConfirm = false;
				CanSkip = true;
				SkipButtonText = "Close";
			}
		}

		public AppUpdateWindowViewModel(AppUpdateWindow view)
		{
			_view = view;

			var canConfirm = this.WhenAnyValue(x => x.CanConfirm);
			ConfirmCommand = ReactiveCommand.Create(() =>
			{
				try
				{
					if (AutoUpdater.DownloadUpdate(UpdateArgs))
					{
						System.Windows.Application.Current.Shutdown();
					}
				}
				catch (Exception ex)
				{
					MainWindow.Self.DisplayError($"Error occurred while updating:\n{ex}");
					_view.Hide();
				}
			}, canConfirm, RxApp.MainThreadScheduler);

			var canSkip = this.WhenAnyValue(x => x.CanSkip);
			SkipCommand = ReactiveCommand.Create(() => _view.Hide(), canSkip);
		}
	}

	public partial class AppUpdateWindow : AppUpdateWindowBase
	{
		private readonly Lazy<Markdown> _fallbackMarkdown = new(() => new Markdown());
		private readonly Markdown _defaultMarkdown;

		private FlowDocument StringToMarkdown(string text)
		{
			var markdown = _defaultMarkdown ?? _fallbackMarkdown.Value;
			var doc = markdown.Transform(text);
			return doc;
		}

		public AppUpdateWindow()
		{
			InitializeComponent();

			ViewModel = new AppUpdateWindowViewModel(this);

			var obj = TryFindResource("DefaultMarkdown");
			if (obj != null && obj is Markdown markdown)
			{
				_defaultMarkdown = markdown;
			}

			this.WhenActivated(d =>
			{
				d(this.BindCommand(ViewModel, vm => vm.ConfirmCommand, v => v.ConfirmButton));
				d(this.BindCommand(ViewModel, vm => vm.SkipCommand, v => v.SkipButton));
				d(this.OneWayBind(ViewModel, vm => vm.SkipButtonText, v => v.SkipButton.Content));
				d(this.OneWayBind(ViewModel, vm => vm.UpdateDescription, v => v.UpdateDescription.Text));
				d(this.OneWayBind(ViewModel, vm => vm.UpdateChangelogView, v => v.UpdateChangelogView.Document, StringToMarkdown));
			});
		}
	}
}
