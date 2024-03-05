using DivinityModManager.Controls;
using DivinityModManager.Models;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Windows;

namespace DivinityModManager.ViewModels
{
	public class VersionGeneratorViewModel : ReactiveObject
	{
		[Reactive] public DivinityModVersion2 Version { get; set; }
		[Reactive] public string Text { get; set; }

		public ICommand CopyCommand { get; private set; }
		public ICommand ResetCommand { get; private set; }
		public ReactiveCommand<KeyboardFocusChangedEventArgs, Unit> UpdateVersionFromTextCommand { get; private set; }

		public VersionGeneratorViewModel(AlertBar alert)
		{
			Version = new DivinityModVersion2(36028797018963968);

			CopyCommand = ReactiveCommand.Create(() =>
			{
				Clipboard.SetText(Version.VersionInt.ToString());
				alert.SetSuccessAlert($"Copied {Version.VersionInt} to the clipboard.");
			});

			ResetCommand = ReactiveCommand.Create(() =>
			{
				Version.VersionInt = 36028797018963968;
				alert.SetWarningAlert($"Reset version number.");
			});

			UpdateVersionFromTextCommand = ReactiveCommand.Create<KeyboardFocusChangedEventArgs, Unit>(e =>
			{
				if (ulong.TryParse(Text, out var version))
				{
					Version.ParseInt(version);
				}
				else
				{
					Version.ParseInt(36028797018963968);
				}
				return Unit.Default;
			});

			Version.WhenAnyValue(x => x.VersionInt).Throttle(TimeSpan.FromMilliseconds(50)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(v =>
			{
				Text = v.ToString();
			});

			Version.WhenAnyValue(x => x.Major, x => x.Minor, x => x.Revision, x => x.Build).Throttle(TimeSpan.FromMilliseconds(50)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(v =>
			{
				Version.VersionInt = Version.ToInt();
			});
		}
	}
}
