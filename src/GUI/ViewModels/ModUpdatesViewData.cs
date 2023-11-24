using Alphaleonis.Win32.Filesystem;

using DivinityModManager.Models.Updates;
using DivinityModManager.Views;

using DynamicData;
using DynamicData.Binding;
using DynamicData.Aggregation;

using Ookii.Dialogs.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Threading;
using System.Reactive;
using System.Collections.Concurrent;
using System.Windows;

namespace DivinityModManager.ViewModels
{
	public class CopyModUpdatesTask
	{
		public List<DivinityModUpdateData> Updates { get; set; }
		public string DocumentsFolder { get; set; }
		public string ModPakFolder { get; set; }
		public int TotalProcessed { get; set; }
	}

	public class ModUpdatesViewData : ReactiveObject
	{
		[Reactive] public bool Unlocked { get; set; }
		[Reactive] public bool JustUpdated { get; set; }

		public SourceList<DivinityModUpdateData> Mods { get; private set; } = new SourceList<DivinityModUpdateData>();

		private readonly ReadOnlyObservableCollection<DivinityModUpdateData> _newMods;
		public ReadOnlyObservableCollection<DivinityModUpdateData> NewMods => _newMods;

		private readonly ReadOnlyObservableCollection<DivinityModUpdateData> _updatedMods;
		public ReadOnlyObservableCollection<DivinityModUpdateData> UpdatedMods => _updatedMods;

		[ObservableAsProperty] public bool AnySelected { get; }
		[ObservableAsProperty] public bool AllNewModsSelected { get; }
		[ObservableAsProperty] public bool AllModUpdatesSelected { get; }
		[ObservableAsProperty] public bool NewAvailable { get; }
		[ObservableAsProperty] public bool UpdatesAvailable { get; }
		[ObservableAsProperty] public int TotalUpdates { get; }

		public ICommand CopySelectedModsCommand { get; private set; }
		public ICommand SelectAllNewModsCommand { get; private set; }
		public ICommand SelectAllUpdatesCommand { get; private set; }

		public Action<bool> CloseView { get; set; }

		private readonly MainWindowViewModel _mainWindowViewModel;

		public void Clear()
		{
			Mods.Clear();
			Unlocked = true;
		}

		public void SelectAll(bool select = true)
		{
			foreach (var x in Mods.Items)
			{
				x.IsSelected = select;
			}
		}

		public void CopySelectedMods()
		{
			var documentsFolder = _mainWindowViewModel.PathwayData.AppDataGameFolder;
			var modPakFolder = _mainWindowViewModel.PathwayData.AppDataModsPath;

			using (var dialog = new TaskDialog()
			{
				Buttons = {
					new TaskDialogButton(ButtonType.Yes),
					new TaskDialogButton(ButtonType.No)
				},
				WindowTitle = "Update Mods?",
				Content = "Download / copy updates? Previous pak files will be moved to the Recycle Bin.",
				MainIcon = TaskDialogIcon.Warning
			})
			{
				var result = dialog.ShowDialog(MainWindow.Self);
				if (result.ButtonType == ButtonType.Yes)
				{
					var updates = Mods.Items.Where(x => x.IsSelected).ToList();

					Unlocked = false;

					StartUpdating(new CopyModUpdatesTask()
					{
						DocumentsFolder = documentsFolder,
						ModPakFolder = modPakFolder,
						Updates = Mods.Items.Where(x => x.IsSelected).ToList(),
						TotalProcessed = 0
					});
				}
			}
		}

		private async Task AwaitDownloadPartition(IEnumerator<DivinityModUpdateData> partition, int progressIncrement, string outputFolder, CancellationToken token)
		{
			using (partition)
			{
				while (partition.MoveNext())
				{
					if (token.IsCancellationRequested) return;
					await Task.Yield(); // prevents a sync/hot thread hangup
					await partition.Current.DownloadData.DownloadAsync(partition.Current.LocalFilePath, outputFolder, token);
					await _mainWindowViewModel.IncreaseMainProgressValueAsync(progressIncrement);
				}
			}
		}

		private async Task<Unit> ProcessUpdatesAsync(CopyModUpdatesTask taskData, IScheduler sch, CancellationToken token)
		{
			await _mainWindowViewModel.StartMainProgressAsync("Processing updates...");
			var currentTime = DateTime.Now;
			var partitionAmount = Environment.ProcessorCount;
			var progressIncrement = (int)Math.Ceiling(100d/taskData.Updates.Count);
			await Task.WhenAll(Partitioner.Create(taskData.Updates).GetPartitions(partitionAmount).AsParallel().Select(p => AwaitDownloadPartition(p, progressIncrement, taskData.ModPakFolder, token)));
			await Observable.Start(FinishUpdating, RxApp.MainThreadScheduler);
			return Unit.Default;
		}

		private void StartUpdating(CopyModUpdatesTask taskData)
		{
			RxApp.MainThreadScheduler.ScheduleAsync(async (sch,token) => await ProcessUpdatesAsync(taskData,sch,token));
		}

		private void FinishUpdating()
		{
			Unlocked = true;
			JustUpdated = true;
			CloseView?.Invoke(true);
		}

		public ModUpdatesViewData(MainWindowViewModel mainWindowViewModel)
		{
			Unlocked = true;

			_mainWindowViewModel = mainWindowViewModel;

			var modsConnection = Mods.Connect();

			modsConnection.CountChanged().Select(x => x.Count).ToUIProperty(this, x => x.TotalUpdates);

			var splitList = modsConnection.AutoRefresh(x => x.IsNewMod);
			var newModsConnection = splitList.Filter(x => x.IsNewMod);
			var updatedModsConnection = splitList.Filter(x => !x.IsNewMod);

			newModsConnection.Bind(out _newMods).Subscribe();
			updatedModsConnection.Bind(out _updatedMods).Subscribe();

			var hasNewMods = newModsConnection.CountChanged().Select(x => x.Count > 0);
			var hasUpdatedMods = updatedModsConnection.CountChanged().Select(x => x.Count > 0);
			hasNewMods.ToUIProperty(this, x => x.NewAvailable);
			hasUpdatedMods.ToUIProperty(this, x => x.UpdatesAvailable);

			var selectedMods = modsConnection.AutoRefresh(x => x.IsSelected).ToCollection();
			selectedMods.Select(x => x.Any(y => y.IsSelected)).ToUIProperty(this, x => x.AnySelected);

			var newModsChangeSet = NewMods.ToObservableChangeSet().AutoRefresh(x => x.IsSelected).ToCollection();
			var modUpdatesChangeSet = UpdatedMods.ToObservableChangeSet().AutoRefresh(x => x.IsSelected).ToCollection();

			splitList.Filter(x => x.IsNewMod).ToCollection().Select(x => x.All(y => y.IsSelected)).ToUIProperty(this, x => x.AllNewModsSelected);
			splitList.Filter(x => !x.IsNewMod).ToCollection().Select(x => x.All(y => y.IsSelected)).ToUIProperty(this, x => x.AllModUpdatesSelected);

			var anySelectedObservable = this.WhenAnyValue(x => x.AnySelected);

			CopySelectedModsCommand = ReactiveCommand.Create(CopySelectedMods, anySelectedObservable);

			SelectAllNewModsCommand = ReactiveCommand.Create<bool>((b) =>
			{
				foreach (var x in NewMods)
				{
					x.IsSelected = b;
				}
			}, hasNewMods);
			SelectAllUpdatesCommand = ReactiveCommand.Create<bool>((b) =>
			{
				foreach (var x in UpdatedMods)
				{
					x.IsSelected = b;
				}
			}, hasUpdatedMods);
		}
	}
}
