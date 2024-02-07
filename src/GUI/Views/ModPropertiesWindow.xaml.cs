using Alphaleonis.Win32.Filesystem;

using DivinityModManager.Models;
using DivinityModManager.Models.Mod;
using DivinityModManager.Util;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using DynamicData;

namespace DivinityModManager.Views
{
	public class ModPropertiesWindowBase : HideWindowBase<ModConfigPropertiesViewModel> { }

	public class ModConfigPropertiesViewModel : ReactiveObject
	{
		[Reactive] public string Title { get; set; }
		[Reactive] public bool Locked { get; set; }
		[Reactive] public DivinityModData Mod { get; set; }
		[ObservableAsProperty] public string ModType { get; }
		[ObservableAsProperty] public string ModSizeText { get; }

		[Reactive] public string Notes { get; set; }
		[Reactive] public string GitHubAuthor { get; set; }
		[Reactive] public string GitHubRepository { get; set; }
		[Reactive] public long NexusModsId { get; set; }
		[Reactive] public long SteamWorkshopId { get; set; }

		public ReactiveCommand<Unit, Unit> ApplyConfigCommand { get; private set; }

		private static string ModToTitle(DivinityModData mod)
		{
			if(mod == null) return "Mod Properties";

			return $"{mod.DisplayName} Properties";
		}

		private static string GetModType(DivinityModData mod)
		{
			if (mod?.IsEditorMod == true) return "Editor Project";
			return "Pak";
		}

		private static string GetModSize(DivinityModData mod)
		{
			if (mod == null) return "0 bytes";

			try
			{
				if(mod != null && File.Exists(mod.FilePath))
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
			catch(Exception ex)
			{
				DivinityApp.Log($"Error checking mod file size at path '{mod?.FilePath}':\n{ex}");
			}
			return "0 bytes";
		}

		private void LoadConfigProperties(DivinityModData mod)
		{
			Locked = true;
			if(mod != null)
			{
				if(mod.ModManagerConfig != null && mod.ModManagerConfig.IsLoaded)
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
			RxApp.TaskpoolScheduler.Schedule(TimeSpan.FromMilliseconds(10), () => { Locked = false; });
		}

		public void ApplyConfigSettings()
		{
			var modConfigService = Services.Get<ISettingsService>().ModConfig;

			modConfigService.Mods.AddOrUpdate(Mod.ModManagerConfig);

			Mod.ModManagerConfig.GitHubAuthor = GitHubAuthor;
			Mod.ModManagerConfig.GitHubRepository = GitHubRepository;
			Mod.ModManagerConfig.NexusModsId = NexusModsId;
			Mod.ModManagerConfig.SteamWorkshopId = SteamWorkshopId;
			Mod.ModManagerConfig.Notes = Notes;

			//Should be called automatically when the mod config is updated
			//Services.Get<ISettingsService>().ModConfig.TrySave();
		}

		public ModConfigPropertiesViewModel()
		{
			Title = "Mod Properties";

			var whenModSet = this.WhenAnyValue(x => x.Mod);
			whenModSet.Select(ModToTitle).ObserveOn(RxApp.MainThreadScheduler).BindTo(this, x => x.Title);

			whenModSet.Subscribe(LoadConfigProperties);

			whenModSet.Select(GetModType).ToUIProperty(this, x => x.ModType);
			whenModSet.Select(GetModSize).ToUIProperty(this, x => x.ModSizeText);

			var whenNotLocked = this.WhenAnyValue(x => x.Locked, b => !b);
			var propertyChanged = nameof(ReactiveObject.PropertyChanged);
			var whenConfig = Observable.FromEventPattern<PropertyChangedEventArgs>(this, propertyChanged);
			var autoSaveProperties = new HashSet<string>()
			{ 
				nameof(GitHubAuthor),
				nameof(GitHubRepository),
				nameof(NexusModsId),
				nameof(SteamWorkshopId),
				nameof(Notes),
			};

			ApplyConfigCommand = ReactiveCommand.Create(ApplyConfigSettings, whenNotLocked);
			whenConfig.Where(e => autoSaveProperties.Contains(e.EventArgs.PropertyName))
			.Throttle(TimeSpan.FromMilliseconds(100))
			.Select(x => Unit.Default)
			.InvokeCommand(ApplyConfigCommand);
		}
	}

	/// <summary>
	/// Interaction logic for SingleModConfigWindow.xaml
	/// </summary>
	public partial class ModPropertiesWindow : ModPropertiesWindowBase
	{
		public ModPropertiesWindow()
		{
			InitializeComponent();

			ViewModel = new ModConfigPropertiesViewModel();

			/*ConfigAutoGrid.Loaded += (o, e) =>
			{
				ConfigAutoGrid.Rows = String.Join(",", Enumerable.Repeat("auto", ConfigAutoGrid.RowCount));
			};*/

			ModNexusModsIDUpDown.Minimum = DivinityApp.NEXUSMODS_MOD_ID_START;

			this.WhenActivated(d =>
			{
				d(this.OneWayBind(ViewModel, vm => vm.Title, v => v.Title));

				d(this.OneWayBind(ViewModel, vm => vm.Mod.Name, v => v.ModNameText.Text));
				d(this.OneWayBind(ViewModel, vm => vm.Mod.Description, v => v.ModDescriptionText.Text));
				d(this.OneWayBind(ViewModel, vm => vm.Mod.FilePath, v => v.ModPathText.Text));

				d(this.Bind(ViewModel, vm => vm.NexusModsId, v => v.ModNexusModsIDUpDown.Value));
				d(this.Bind(ViewModel, vm => vm.GitHubAuthor, v => v.ModGitHubAuthorText.Text));
				d(this.Bind(ViewModel, vm => vm.GitHubRepository, v => v.ModGitHubRepositoryText.Text));

				d(this.OneWayBind(ViewModel, vm => vm.ModType, v => v.ModTypeText.Text));
				d(this.OneWayBind(ViewModel, vm => vm.ModSizeText, v => v.ModSizeText.Text));
			});
		}
	}
}
