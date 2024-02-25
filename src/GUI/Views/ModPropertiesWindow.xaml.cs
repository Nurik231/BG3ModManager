using Alphaleonis.Win32.Filesystem;

using DivinityModManager.Models;
using DivinityModManager.Util;

using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace DivinityModManager.Views
{
	public class ModPropertiesWindowBase : HideWindowBase<ModConfigPropertiesViewModel> { }

	public class ModConfigPropertiesViewModel : ReactiveObject
	{
		[Reactive] public string Title { get; set; }
		[Reactive] public bool IsActive { get; set; }
		[Reactive] public bool Locked { get; set; }
		[Reactive] public bool HasChanges { get; private set; }
		[Reactive] public DivinityModData Mod { get; set; }
		[Reactive] public string Notes { get; set; }
		[Reactive] public string GitHubAuthor { get; set; }
		[Reactive] public string GitHubRepository { get; set; }
		[Reactive] public long NexusModsId { get; set; }
		[Reactive] public long SteamWorkshopId { get; set; }

		[ObservableAsProperty] public string ModType { get; }
		[ObservableAsProperty] public string ModSizeText { get; }
		[ObservableAsProperty] public string ModFilePath { get; }
		[ObservableAsProperty] public bool IsEditorMod { get; }
		[ObservableAsProperty] public Visibility AuthorLabelVisibility { get; }
		[ObservableAsProperty] public Visibility RepoLabelVisibility { get; }

		public ICommand OKCommand { get; set; }
		public ICommand CancelCommand { get; set; }
		public ICommand ApplyCommand { get; }

		public void SetMod(DivinityModData mod)
		{
			Mod = mod;
			HasChanges = false;
		}

		private void LoadConfigProperties(DivinityModData mod)
		{
			Locked = true;
			//var disp = this.SuppressChangeNotifications();
			if (mod != null)
			{
				if (mod.ModManagerConfig != null && mod.ModManagerConfig.IsLoaded)
				{
					GitHubAuthor = mod.ModManagerConfig.GitHubAuthor;
					GitHubRepository = mod.ModManagerConfig.GitHubRepository;
					NexusModsId = mod.ModManagerConfig.NexusModsId;
					SteamWorkshopId = mod.ModManagerConfig.SteamWorkshopId;
					Notes = mod.ModManagerConfig.Notes;
				}
				else
				{
					GitHubAuthor = mod.GitHubData.Author;
					GitHubRepository = mod.GitHubData.Repository;
					NexusModsId = mod.NexusModsData.ModId;
					SteamWorkshopId = mod.WorkshopData.ModId;
					Notes = "";
				}
			}
			Locked = HasChanges = false;
			//disp.Dispose();
		}

		public void Apply()
		{
			if (Mod.ModManagerConfig == null) throw new NullReferenceException($"ModManagerConfig is null for mod ({Mod})");
			var modConfigService = Services.Get<ISettingsService>().ModConfig;

			if (String.IsNullOrEmpty(Mod.ModManagerConfig.Id)) Mod.ModManagerConfig.Id = Mod.UUID;

			modConfigService.Mods.AddOrUpdate(Mod.ModManagerConfig);

			Mod.ModManagerConfig.GitHubAuthor = GitHubAuthor;
			Mod.ModManagerConfig.GitHubRepository = GitHubRepository;
			Mod.ModManagerConfig.NexusModsId = NexusModsId;
			Mod.ModManagerConfig.SteamWorkshopId = SteamWorkshopId;
			Mod.ModManagerConfig.Notes = Notes;
			Mod.ApplyModConfig(Mod.ModManagerConfig);

			//Should be called automatically when the mod config is updated
			//Services.Get<ISettingsService>().ModConfig.TrySave();
		}

		public void OnClose()
		{
			HasChanges = false;
			Mod = null;
		}

		private static string ModToTitle(DivinityModData mod) => mod != null ? $"{mod.DisplayName} Properties" : "Mod Properties";
		private static string GetModType(DivinityModData mod) => mod?.IsEditorMod == true ? "Editor Project" : "Pak";
		private static string GetModFilePath(DivinityModData mod) => StringUtils.ReplaceSpecialPathways(mod.FilePath);

		private static string GetModSize(DivinityModData mod)
		{
			if (mod == null) return "0 bytes";

			try
			{
				if (mod != null && File.Exists(mod.FilePath))
				{
					if (mod.IsEditorMod)
					{
						var dir = new DirectoryInfo(mod.FilePath);
						var length = dir.EnumerateFiles("*.*", System.IO.SearchOption.AllDirectories).Sum(file => file.Length);
						return StringUtils.BytesToString(length);
					}
					else
					{
						return StringUtils.BytesToString(new FileInfo(mod.FilePath).Length);
					}
				}
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error checking mod file size at path '{mod?.FilePath}':\n{ex}");
			}
			return "0 bytes";
		}

		public static Visibility LabelVisibility(string str) => String.IsNullOrEmpty(str) ? Visibility.Visible : Visibility.Hidden;

		public ModConfigPropertiesViewModel()
		{
			Title = "Mod Properties";

			var whenModSet = this.WhenAnyValue(x => x.Mod).WhereNotNull();
			whenModSet.Select(ModToTitle).ObserveOn(RxApp.MainThreadScheduler).BindTo(this, x => x.Title);

			whenModSet.Subscribe(LoadConfigProperties);

			whenModSet.Select(GetModType).ToUIProperty(this, x => x.ModType);
			whenModSet.Select(GetModSize).ToUIProperty(this, x => x.ModSizeText);
			whenModSet.Select(GetModFilePath).ToUIProperty(this, x => x.ModFilePath);
			whenModSet.Select(x => x.IsEditorMod).ToUIProperty(this, x => x.IsEditorMod);

			var whenNotLocked = this.WhenAnyValue(x => x.Locked, x => x.IsActive).Select(x => !x.Item1 && x.Item2);
			var whenConfig = Observable.FromEventPattern<PropertyChangedEventArgs>(this, nameof(ReactiveObject.PropertyChanged));
			var autoSaveProperties = new HashSet<string>()
			{
				nameof(GitHubAuthor),
				nameof(GitHubRepository),
				nameof(NexusModsId),
				nameof(SteamWorkshopId),
				nameof(Notes),
			};

			whenConfig.Where(e => autoSaveProperties.Contains(e.EventArgs.PropertyName)).Subscribe(e =>
			{
				if (IsActive && !Locked) HasChanges = true;
			});


			this.WhenAnyValue(x => x.GitHubAuthor).Select(LabelVisibility).ToUIProperty(this, x => x.AuthorLabelVisibility, Visibility.Visible);
			this.WhenAnyValue(x => x.GitHubRepository).Select(LabelVisibility).ToUIProperty(this, x => x.RepoLabelVisibility, Visibility.Visible);

			ApplyCommand = ReactiveCommand.Create(Apply, this.WhenAnyValue(x => x.HasChanges));
			/*whenConfig.Where(e => autoSaveProperties.Contains(e.EventArgs.PropertyName))
			.Throttle(TimeSpan.FromMilliseconds(100))
			.Select(x => Unit.Default)
			.InvokeCommand(ApplyConfigCommand);*/
		}
	}

	/// <summary>
	/// Interaction logic for SingleModConfigWindow.xaml
	/// </summary>
	public partial class ModPropertiesWindow : ModPropertiesWindowBase
	{
		private void ConfirmAndClose()
		{
			ViewModel.Apply();
			Hide();
		}

		private void CancelAndClose()
		{
			ViewModel.OnClose();
			Hide();
		}

		private readonly object LargeFileIcon;
		private readonly object LargeFolderIcon;

		private object GetModTypeIcon(bool isEditorMod) => isEditorMod ? LargeFolderIcon : LargeFileIcon;

		public ModPropertiesWindow()
		{
			InitializeComponent();

			ViewModel = new ModConfigPropertiesViewModel()
			{
				OKCommand = ReactiveCommand.Create(ConfirmAndClose),
				CancelCommand = ReactiveCommand.Create(CancelAndClose)
			};

			LargeFileIcon = FindResource("LargeFileIcon");
			LargeFolderIcon = FindResource("LargeFolderIcon");

			/*ConfigAutoGrid.Loaded += (o, e) =>
			{
				ConfigAutoGrid.Rows = String.Join(",", Enumerable.Repeat("auto", ConfigAutoGrid.RowCount));
			};*/

			ModNexusModsIDUpDown.Minimum = DivinityApp.NEXUSMODS_MOD_ID_START;

			this.Activated += (o, e) => ViewModel.IsActive = true;
			this.Deactivated += (o, e) => ViewModel.IsActive = false;

			this.WhenActivated(d =>
			{
				this.OneWayBind(ViewModel, vm => vm.Title, v => v.Title);

				this.OneWayBind(ViewModel, vm => vm.Mod.FileName, v => v.ModFileNameText.Text);
				this.OneWayBind(ViewModel, vm => vm.Mod.Name, v => v.ModNameText.Text);
				this.OneWayBind(ViewModel, vm => vm.Mod.Description, v => v.ModDescriptionText.Text);
				this.OneWayBind(ViewModel, vm => vm.ModFilePath, v => v.ModPathText.Text);

				this.Bind(ViewModel, vm => vm.NexusModsId, v => v.ModNexusModsIDUpDown.Value);
				this.Bind(ViewModel, vm => vm.GitHubAuthor, v => v.ModGitHubAuthorText.Text);
				this.Bind(ViewModel, vm => vm.GitHubRepository, v => v.ModGitHubRepositoryText.Text);

				this.Bind(ViewModel, vm => vm.Notes, v => v.ModNotesTextBox.Text);

				this.OneWayBind(ViewModel, vm => vm.ModType, v => v.ModTypeText.Text);
				this.OneWayBind(ViewModel, vm => vm.ModSizeText, v => v.ModSizeText.Text);
				this.OneWayBind(ViewModel, vm => vm.AuthorLabelVisibility, v => v.AuthorLabel.Visibility);
				this.OneWayBind(ViewModel, vm => vm.RepoLabelVisibility, v => v.RepoLabel.Visibility);

				this.OneWayBind(ViewModel, vm => vm.Mod.IsEditorMod, v => v.ModTypeIconControl.Content, GetModTypeIcon);

				this.BindCommand(ViewModel, vm => vm.ApplyCommand, v => v.ApplyButton);
				this.BindCommand(ViewModel, vm => vm.OKCommand, v => v.OKButton);
				this.BindCommand(ViewModel, vm => vm.CancelCommand, v => v.CancelButton);

				HelpStackPanel.ToolTip = "Set the NexusMods ID to allow auto-updating (provided a NexusMods API key is set)\n\nSetting a valid GitHub Author/Repository will also allow auto-updating from GitHub";
			});
		}
	}
}
