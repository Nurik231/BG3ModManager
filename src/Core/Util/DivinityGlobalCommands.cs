using Alphaleonis.Win32.Filesystem;

using DivinityModManager.Models;
using DivinityModManager.ViewModels;

using ReactiveUI;

using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Windows;

namespace DivinityModManager.Util
{
	public class DivinityGlobalCommands : ReactiveObject
	{
		private IDivinityAppViewModel _viewModel;

		public IDivinityAppViewModel ViewModel => _viewModel;

		public void SetViewModel(IDivinityAppViewModel vm)
		{
			_viewModel = vm;
			this.RaisePropertyChanged(nameof(ViewModel));
		}

		public ReactiveCommand<string, Unit> OpenFileCommand { get; private set; }
		public ReactiveCommand<string, Unit> OpenInFileExplorerCommand { get; private set; }
		public ReactiveCommand<Unit, Unit> ClearMissingModsCommand { get; private set; }
		public ReactiveCommand<DivinityModData, Unit> ToggleNameDisplayCommand { get; private set; }
		public ReactiveCommand<string, Unit> CopyToClipboardCommand { get; private set; }
		public ReactiveCommand<DivinityModData, Unit> DeleteModCommand { get; private set; }
		public ReactiveCommand<DivinityModData, Unit> OpenSteamWorkshopPageCommand { get; private set; }
		public ReactiveCommand<DivinityModData, Unit> OpenSteamWorkshopPageInSteamCommand { get; private set; }
		public ReactiveCommand<DivinityModData, Unit> OpenNexusModsPageCommand { get; private set; }
		public ReactiveCommand<object, Unit> OpenURLCommand { get; private set; }
		public ReactiveCommand<DivinityModData, Unit> ToggleForceAllowInLoadOrderCommand { get; private set; }

		public void OpenFile(string path)
		{
			if (File.Exists(path))
			{
				try
				{
					Process.Start(Path.GetFullPath(path));
				}
				catch (System.ComponentModel.Win32Exception) // No File Association
				{
					Process.Start("explorer.exe", $"\"{Path.GetFullPath(path)}\"");
				}
			}
			else if (Directory.Exists(path))
			{
				Process.Start("explorer.exe", $"\"{Path.GetFullPath(path)}\"");
			}
			else
			{
				_viewModel.ShowAlert($"Error opening '{path}': File does not exist!", AlertType.Danger, 10);
			}
		}

		public void OpenInFileExplorer(string path)
		{
			if (File.Exists(path))
			{
				Process.Start("explorer.exe", $"/select, \"{Path.GetFullPath(path)}\"");
			}
			else if (Directory.Exists(path))
			{
				Process.Start("explorer.exe", $"\"{Path.GetFullPath(path)}\"");
			}
			else
			{
				_viewModel.ShowAlert($"Error opening '{path}': File does not exist!", AlertType.Danger, 10);
			}
		}

		public void CopyToClipboard(string text)
		{
			try
			{
				Clipboard.SetText(text);
				_viewModel.ShowAlert("Copied text to clipboard.", 0, 10);
			}
			catch (Exception ex)
			{
				_viewModel.ShowAlert($"Error copying text to clipboard: {ex}", AlertType.Danger, 10);
			}
		}

		public void OpenURL(string url)
		{
			if (!String.IsNullOrEmpty(url))
			{
				DivinityFileUtils.TryOpenPath(url);
			}
		}

		public void OpenSteamWorkshopPage(DivinityModData mod)
		{
			var url = mod.GetURL(ModSourceType.STEAM);
			if (!String.IsNullOrEmpty(url))
			{
				DivinityFileUtils.TryOpenPath(url);
			}
		}

		public void OpenSteamWorkshopPageInSteam(DivinityModData mod)
		{
			var url = mod.GetURL(ModSourceType.STEAM, true);
			if (!String.IsNullOrEmpty(url))
			{
				DivinityFileUtils.TryOpenPath(url);
			}
		}

		public void OpenNexusModsPage(DivinityModData mod)
		{
			var url = mod.GetURL(ModSourceType.NEXUSMODS);
			if (!String.IsNullOrEmpty(url))
			{
				DivinityFileUtils.TryOpenPath(url);
			}
		}

		public void OpenRepositoryPage(DivinityModData mod)
		{
			var url = mod.GetURL(ModSourceType.GITHUB);
			if (!String.IsNullOrEmpty(url))
			{
				DivinityFileUtils.TryOpenPath(url);
			}
		}

		public void ToggleForceAllowInLoadOrder(DivinityModData mod)
		{
			RxApp.MainThreadScheduler.Schedule(() =>
			{
				mod.ForceAllowInLoadOrder = !mod.ForceAllowInLoadOrder;
				if (mod.ForceAllowInLoadOrder)
				{
					ViewModel.AddActiveMod(mod);
				}
				else
				{
					ViewModel.RemoveActiveMod(mod);
				}
			});
		}

		public void ClearMissingMods()
		{
			_viewModel.ClearMissingMods();
		}

		public DivinityGlobalCommands()
		{
			var canExecuteViewModelCommands = this.WhenAnyValue(x => x.ViewModel, x => x.ViewModel.IsLocked, (vm, b) => vm != null && !b);

			OpenFileCommand = ReactiveCommand.Create<string>(OpenFile, canExecuteViewModelCommands);
			OpenInFileExplorerCommand = ReactiveCommand.Create<string>(OpenInFileExplorer, canExecuteViewModelCommands);
			ClearMissingModsCommand = ReactiveCommand.Create(ClearMissingMods, canExecuteViewModelCommands);

			ToggleNameDisplayCommand = ReactiveCommand.Create<DivinityModData>((mod) =>
			{
				mod.DisplayFileForName = !mod.DisplayFileForName;
				var b = mod.DisplayFileForName;
				foreach (var m in _viewModel.Mods)
				{
					if (m.IsSelected)
					{
						m.DisplayFileForName = b;
					}
				}
			}, canExecuteViewModelCommands);

			CopyToClipboardCommand = ReactiveCommand.Create<string>(CopyToClipboard, canExecuteViewModelCommands);

			DeleteModCommand = ReactiveCommand.Create<DivinityModData>((mod) =>
			{
				if (mod.CanDelete && _viewModel != null)
				{
					_viewModel.DeleteMod(mod);
				}
			}, canExecuteViewModelCommands);

			OpenURLCommand = ReactiveCommand.Create<object>(x => OpenURL(x.ToString()), canExecuteViewModelCommands);
			OpenSteamWorkshopPageCommand = ReactiveCommand.Create<DivinityModData>(OpenSteamWorkshopPage, canExecuteViewModelCommands);
			OpenSteamWorkshopPageInSteamCommand = ReactiveCommand.Create<DivinityModData>(OpenSteamWorkshopPageInSteam, canExecuteViewModelCommands);
			OpenNexusModsPageCommand = ReactiveCommand.Create<DivinityModData>(OpenNexusModsPage, canExecuteViewModelCommands);
			ToggleForceAllowInLoadOrderCommand = ReactiveCommand.Create<DivinityModData>(ToggleForceAllowInLoadOrder, canExecuteViewModelCommands);
		}
	}
}
