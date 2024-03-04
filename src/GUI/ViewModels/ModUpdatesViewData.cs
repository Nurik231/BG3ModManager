using DivinityModManager.Models.Updates;
using DivinityModManager.Windows;

using DynamicData;
using DynamicData.Binding;

using Ookii.Dialogs.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

		public class UpdateTaskResult
		{
			public string ModId { get; set; }
			public bool Success { get; set; }
		}

		private readonly SourceList<DivinityModUpdateData> Mods = new SourceList<DivinityModUpdateData>();

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
		[ObservableAsProperty] public Visibility UpdatesHeaderVisiblity { get; }

		public ICommand UpdateModsCommand { get; }
		public ICommand SelectAllNewModsCommand { get; }
		public ICommand SelectAllUpdatesCommand { get; }

		public Action<bool> CloseView { get; set; }

		private readonly MainWindowViewModel _mainWindowViewModel;

		public void Add(DivinityModUpdateData mod) => Mods.Add(mod);

		public void Add(IEnumerable<DivinityModUpdateData> mods) => Mods.AddRange(mods);

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

		public void UpdateSelectedMods()
		{
			var documentsFolder = _mainWindowViewModel.PathwayData.AppDataGameFolder;
			var modPakFolder = _mainWindowViewModel.PathwayData.AppDataModsPath;

			using var dialog = new TaskDialog()
			{
				Buttons = {
					new TaskDialogButton(ButtonType.Yes),
					new TaskDialogButton(ButtonType.No)
				},
				WindowTitle = "Update Mods?",
				Content = "Download / copy updates? Previous pak files will be moved to the Recycle Bin.",
				MainIcon = TaskDialogIcon.Warning
			};
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

		private async Task<UpdateTaskResult> AwaitDownloadPartition(IEnumerator<DivinityModUpdateData> partition, int progressIncrement, string outputFolder, CancellationToken token)
		{
			var result = new UpdateTaskResult();
			using (partition)
			{
				while (partition.MoveNext())
				{
					result.ModId = partition.Current.Mod.UUID;
					if (token.IsCancellationRequested) return result;
					await Task.Yield(); // prevents a sync/hot thread hangup
					var downloadResult = await partition.Current.DownloadData.DownloadAsync(partition.Current.LocalFilePath, outputFolder, token);
					result.Success = downloadResult.Success;
					await _mainWindowViewModel.IncreaseMainProgressValueAsync(progressIncrement);
				}
			}
			return result;
		}

		private async Task<Unit> ProcessUpdatesAsync(CopyModUpdatesTask taskData, IScheduler sch, CancellationToken token)
		{
			await _mainWindowViewModel.StartMainProgressAsync("Processing updates...");
			var currentTime = DateTime.Now;
			var partitionAmount = Environment.ProcessorCount;
			var progressIncrement = (int)Math.Ceiling(100d / taskData.Updates.Count);
			var results = await Task.WhenAll(Partitioner.Create(taskData.Updates).GetPartitions(partitionAmount).AsParallel().Select(p => AwaitDownloadPartition(p, progressIncrement, taskData.ModPakFolder, token)));
			UpdateLastUpdated(results);
			await Observable.Start(FinishUpdating, RxApp.MainThreadScheduler);
			return Unit.Default;
		}

		private void StartUpdating(CopyModUpdatesTask taskData)
		{
			RxApp.MainThreadScheduler.ScheduleAsync(async (sch, token) => await ProcessUpdatesAsync(taskData, sch, token));
		}

		private void UpdateLastUpdated(UpdateTaskResult[] results)
		{
			var settings = Services.Get<ISettingsService>();
			settings.UpdateLastUpdated(results.Where(x => x.Success == true).Select(x => x.ModId).ToList());
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
			AllNewModsSelected = AllModUpdatesSelected = true;

			_mainWindowViewModel = mainWindowViewModel;

			Mods.CountChanged.ToUIProperty(this, x => x.TotalUpdates);

			var modsConnection = Mods.Connect();
			var splitList = modsConnection.AutoRefresh(x => x.IsNewMod);
			var newModsConnection = splitList.Filter(x => x.IsNewMod);
			var updatedModsConnection = modsConnection.Filter(x => !x.IsNewMod);

			newModsConnection.Bind(out _newMods).Subscribe();
			updatedModsConnection.Bind(out _updatedMods).Subscribe();

			var hasNewMods = newModsConnection.CountChanged().Select(_ => _newMods.Count > 0);
			var hasUpdatedMods = updatedModsConnection.CountChanged().Select(_ => _updatedMods.Count > 0);
			hasNewMods.ToUIProperty(this, x => x.NewAvailable);
			hasUpdatedMods.ToUIProperty(this, x => x.UpdatesAvailable);

			var selectedMods = modsConnection.AutoRefresh(x => x.IsSelected).ToCollection();
			selectedMods.Select(x => x.Any(y => y.IsSelected)).ToUIProperty(this, x => x.AnySelected);

			var newModsChangeSet = NewMods.ToObservableChangeSet().AutoRefresh(x => x.IsSelected).ToCollection();
			var modUpdatesChangeSet = UpdatedMods.ToObservableChangeSet().AutoRefresh(x => x.IsSelected).ToCollection();

			splitList.Filter(x => x.IsNewMod).ToCollection().Select(x => x.All(y => y.IsSelected)).ToUIPropertyImmediate(this, x => x.AllNewModsSelected);
			splitList.Filter(x => !x.IsNewMod).ToCollection().Select(x => x.All(y => y.IsSelected)).ToUIPropertyImmediate(this, x => x.AllModUpdatesSelected);

			var anySelectedObservable = this.WhenAnyValue(x => x.AnySelected);

			this.WhenAnyValue(x => x.NewAvailable, x => x.UpdatesAvailable).Select(x => x.Item1 && x.Item2 ? Visibility.Visible : Visibility.Collapsed)
				.ToUIProperty(this, x => x.UpdatesHeaderVisiblity, Visibility.Collapsed);

			UpdateModsCommand = ReactiveCommand.Create(UpdateSelectedMods, anySelectedObservable);

			SelectAllNewModsCommand = ReactiveCommand.Create<bool>(b =>
			{
				foreach (var x in NewMods)
				{
					x.IsSelected = b;
				}
			}, hasNewMods);

			SelectAllUpdatesCommand = ReactiveCommand.Create<bool>(b =>
			{
				foreach (var x in UpdatedMods)
				{
					x.IsSelected = b;
				}
			}, hasUpdatedMods);
		}
	}
}
