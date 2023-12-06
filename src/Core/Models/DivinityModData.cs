using Alphaleonis.Win32.Filesystem;

using DivinityModManager.Models.GitHub;
using DivinityModManager.Models.Mod;
using DivinityModManager.Models.NexusMods;
using DivinityModManager.Models.Steam;
using DivinityModManager.Util;

using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace DivinityModManager.Models
{
	[ScreenReaderHelper(Name = "DisplayName", HelpText = "HelpText")]
	public class DivinityModData : DivinityBaseModData, ISelectable
	{
		[Reactive] public int Index { get; set; }

		public string OutputPakName
		{
			get
			{
				if (!Folder.Contains(UUID))
				{
					return Path.ChangeExtension($"{Folder}_{UUID}", "pak");
				}
				else
				{
					return Path.ChangeExtension($"{FileName}", "pak");
				}
			}
		}

		[Reactive] public string ModType { get; set; }

		public ObservableCollectionExtended<string> Modes { get; private set; } = new ObservableCollectionExtended<string>();

		public string Targets { get; set; }
		[Reactive] public DateTime? LastUpdated { get; set; }

		[Reactive] public DivinityExtenderModStatus ExtenderModStatus { get; set; }
		[Reactive] public DivinityOsirisModStatus OsirisModStatus { get; set; }

		[Reactive] public int CurrentExtenderVersion { get; set; }

		private string ExtenderStatusToToolTipText(DivinityExtenderModStatus status, int requiredVersion, int currentVersion)
		{
			var result = "";
			switch (status)
			{
				case DivinityExtenderModStatus.REQUIRED:
				case DivinityExtenderModStatus.REQUIRED_MISSING:
				case DivinityExtenderModStatus.REQUIRED_DISABLED:
				case DivinityExtenderModStatus.REQUIRED_OLD:
				case DivinityExtenderModStatus.REQUIRED_MISSING_UPDATER:
					if (status == DivinityExtenderModStatus.REQUIRED_MISSING)
					{
						result = "[MISSING] ";
					}
					else if (status == DivinityExtenderModStatus.REQUIRED_MISSING_UPDATER)
					{
						result = "[SE DISABLED] ";
					}
					else if (status == DivinityExtenderModStatus.REQUIRED_DISABLED)
					{
						result = "[EXTENDER DISABLED] ";
					}
					else if (status == DivinityExtenderModStatus.REQUIRED_OLD)
					{
						result = "[OLD] ";
					}
					if (requiredVersion > -1)
					{
						result += $"Requires Script Extender v{requiredVersion} or Higher";
					}
					else
					{
						result += "Requires the Script Extender";
					}
					if (status == DivinityExtenderModStatus.REQUIRED_DISABLED)
					{
						result += "\n(Enable Extensions in the Script Extender Settings)";
					}
					else if (status == DivinityExtenderModStatus.REQUIRED_MISSING_UPDATER)
					{
						result += "\n(Missing DWrite.dll)";
					}
					else if (status == DivinityExtenderModStatus.REQUIRED_OLD)
					{
						result += "\n(The installed SE version is older)";
					}
					break;
				case DivinityExtenderModStatus.SUPPORTS:
					if (requiredVersion > -1)
					{
						result = $"Uses Script Extender v{requiredVersion} or Higher (Optional)";
					}
					else
					{
						result = "Uses the Script Extender (Optional)";
					}
					break;
				case DivinityExtenderModStatus.NONE:
				default:
					result = "";
					break;
			}
			if (result != "")
			{
				result += Environment.NewLine;
			}
			if (currentVersion > -1)
			{
				result += $"Currently installed version is v{currentVersion}";
			}
			else
			{
				result += "No installed extender version found";
			}
			return result;
		}

		[Reactive] public DivinityModScriptExtenderConfig ScriptExtenderData { get; set; }
		public SourceList<DivinityModDependencyData> Dependencies { get; set; } = new SourceList<DivinityModDependencyData>();

		protected ReadOnlyObservableCollection<DivinityModDependencyData> displayedDependencies;
		public ReadOnlyObservableCollection<DivinityModDependencyData> DisplayedDependencies => displayedDependencies;

		public override string GetDisplayName()
		{
			if (DisplayFileForName)
			{
				if (!IsEditorMod)
				{
					return FileName;
				}
				else
				{
					return Folder + " [Editor Project]";
				}
			}
			else
			{
				if (!DivinityApp.DeveloperModeEnabled && UUID == DivinityApp.MAIN_CAMPAIGN_UUID)
				{
					return "Main";
				}
				return Name;
			}
		}

		[ObservableAsProperty] public bool HasToolTip { get; }
		[ObservableAsProperty] public int TotalDependencies { get; }
		[ObservableAsProperty] public bool HasDependencies { get; }

		[Reactive] public bool HasScriptExtenderSettings { get; set; }

		[Reactive] public bool IsEditorMod { get; set; }

		[Reactive] public bool IsActive { get; set; }

		private bool isSelected = false;

		public bool IsSelected
		{
			get => isSelected;
			set
			{
				if (value && Visibility != Visibility.Visible)
				{
					value = false;
				}
				this.RaiseAndSetIfChanged(ref isSelected, value);
			}
		}

		[ObservableAsProperty] public bool CanDelete { get; }
		[ObservableAsProperty] public bool CanAddToLoadOrder { get; }
		[ObservableAsProperty] public string ScriptExtenderSupportToolTipText { get; }
		[ObservableAsProperty] public string OsirisStatusToolTipText { get; }
		[ObservableAsProperty] public string LastModifiedDateText { get; }
		[ObservableAsProperty] public string DisplayVersion { get; }
		[ObservableAsProperty] public string Notes { get; }
		[ObservableAsProperty] public Visibility DependencyVisibility { get; }
		[ObservableAsProperty] public Visibility OpenGitHubLinkVisibility { get; }
		[ObservableAsProperty] public Visibility OpenNexusModsLinkVisibility { get; }
		[ObservableAsProperty] public Visibility OpenWorkshopLinkVisibility { get; }
		[ObservableAsProperty] public Visibility ToggleForceAllowInLoadOrderVisibility { get; }
		[ObservableAsProperty] public Visibility ExtenderStatusVisibility { get; }
		[ObservableAsProperty] public Visibility OsirisStatusVisibility { get; }
		[ObservableAsProperty] public Visibility HasFilePathVisibility { get; }

		#region NexusMods Properties
		[ObservableAsProperty] public Visibility NexusImageVisibility { get; }
		[ObservableAsProperty] public Visibility NexusModsInformationVisibility { get; }
		[ObservableAsProperty] public DateTime NexusModsCreatedDate { get; }
		[ObservableAsProperty] public DateTime NexusModsUpdatedDate { get; }
		[ObservableAsProperty] public string NexusModsTooltipInfo { get; }

		#endregion

		[Reactive] public bool GitHubEnabled { get; set; }
		[Reactive] public bool NexusModsEnabled { get; set; }
		[Reactive] public bool SteamWorkshopEnabled { get; set; }
		[Reactive] public bool CanDrag { get; set; }
		[Reactive] public bool DeveloperMode { get; set; }
		[Reactive] public bool HasColorOverride { get; set; }
		[Reactive] public string SelectedColor { get; set; }
		[Reactive] public string ListColor { get; set; }

		public HashSet<string> Files { get; set; }

		[Reactive] public SteamWorkshopModData WorkshopData { get; set; }
		[Reactive] public NexusModsModData NexusModsData { get; set; }
		[Reactive] public GitHubModData GitHubData { get; set; }
		[Reactive] public ModConfig ModManagerConfig { get; set; }

		public string GetURL(ModSourceType modSourceType, bool asProtocol = false)
		{
			switch (modSourceType)
			{
				case ModSourceType.STEAM:
					if (WorkshopData != null && WorkshopData.ModId > DivinityApp.WORKSHOP_MOD_ID_START)
					{
						if (!asProtocol)
						{
							return $"https://steamcommunity.com/sharedfiles/filedetails/?id={WorkshopData.ModId}";
						}
						else
						{
							return $"steam://url/CommunityFilePage/{WorkshopData.ModId}";
						}
					}
					break;
				case ModSourceType.NEXUSMODS:
					if (NexusModsData != null && NexusModsData.ModId >= DivinityApp.NEXUSMODS_MOD_ID_START)
					{
						return String.Format(DivinityApp.NEXUSMODS_MOD_URL, NexusModsData.ModId);
					}
					break;
				case ModSourceType.GITHUB:
					if (GitHubData != null)
					{
						return $"https://github.com/{GitHubData.Author}/{GitHubData.Repository}";
					}
					break;
			}
			return "";
		}

		public List<string> GetAllURLs(bool asProtocol = false)
		{
			var urls = new List<string>();
			var steamUrl = GetURL(ModSourceType.STEAM, asProtocol);
			if (!String.IsNullOrEmpty(steamUrl))
			{
				urls.Add(steamUrl);
			}
			var nexusUrl = GetURL(ModSourceType.NEXUSMODS, asProtocol);
			if (!String.IsNullOrEmpty(nexusUrl))
			{
				urls.Add(nexusUrl);
			}
			var githubUrl = GetURL(ModSourceType.GITHUB, asProtocol);
			if (!String.IsNullOrEmpty(githubUrl))
			{
				urls.Add(githubUrl);
			}
			return urls;
		}

		public override string ToString()
		{
			return $"Name({Name}) Version({Version?.Version}) Author({Author}) UUID({UUID})";
		}

		public DivinityLoadOrderEntry ToOrderEntry()
		{
			return new DivinityLoadOrderEntry
			{
				UUID = this.UUID,
				Name = this.Name
			};
		}

		public DivinityProfileActiveModData ToProfileModData()
		{
			return new DivinityProfileActiveModData()
			{
				Folder = Folder,
				MD5 = MD5,
				Name = Name,
				UUID = UUID,
				Version = Version.VersionInt
			};
		}

		public void AllowInLoadOrder(bool b)
		{
			ForceAllowInLoadOrder = b;
			IsActive = b && IsForceLoaded;
		}

		private string OsirisStatusToTooltipText(DivinityOsirisModStatus status)
		{
			switch(status)
			{
				case DivinityOsirisModStatus.SCRIPTS:
					return "Has Osiris Scripting";
				case DivinityOsirisModStatus.MODFIXER:
					return "Has Mod Fixer";
				case DivinityOsirisModStatus.NONE:
				default:
					return "";
			}
		}

		private bool CanOpenWorkshopBoolCheck(bool enabled, bool isHidden, bool isLarianMod, long workshopID)
		{
			return enabled && !isHidden & !isLarianMod & workshopID > DivinityApp.WORKSHOP_MOD_ID_START;
		}

		private string NexusModsInfoToTooltip(DateTime createdDate, DateTime updatedDate, long endorsements)
		{
			var lines = new List<string>();

			if (endorsements > 0)
			{
				lines.Add($"Endorsements: {endorsements}");
			}

			if (createdDate != DateTime.MinValue)
			{
				lines.Add($"Created on {createdDate.ToString(DivinityApp.DateTimeColumnFormat, CultureInfo.InstalledUICulture)}");
			}

			if(updatedDate != DateTime.MinValue)
			{
				lines.Add($"Last updated on {createdDate.ToString(DivinityApp.DateTimeColumnFormat, CultureInfo.InstalledUICulture)}");
			}

			return String.Join("\n", lines);
		}

		public void ApplyModConfig(ModConfig config)
		{
			ModManagerConfig = config;
			//if (config.NexusMods.ModId > DivinityApp.NEXUSMODS_MOD_ID_START) ModManagerConfig.NexusMods.ModId = config.NexusMods.ModId;
			//if (config.SteamWorkshop.ModId > DivinityApp.WORKSHOP_MOD_ID_START) ModManagerConfig.SteamWorkshop.ModId = config.SteamWorkshop.ModId;
			//if (!String.IsNullOrWhiteSpace(config.GitHub.Author)) ModManagerConfig.GitHub.Author = config.GitHub.Author;
			//if (!String.IsNullOrWhiteSpace(config.GitHub.Repository)) ModManagerConfig.GitHub.Repository = config.GitHub.Repository;
		}

		public DivinityModData(bool isBaseGameMod = false) : base()
		{
			Targets = "";
			Index = -1;
			CanDrag = true;

			WorkshopData = new SteamWorkshopModData();
			NexusModsData = new NexusModsModData();
			GitHubData = new GitHubModData();

			this.WhenAnyValue(x => x.UUID).BindTo(NexusModsData, x => x.UUID);

			this.WhenAnyValue(x => x.NexusModsData.PictureUrl)
				.Select(uri => uri != null && !String.IsNullOrEmpty(uri.AbsolutePath) ? Visibility.Visible : Visibility.Collapsed)
				.ToUIProperty(this, x => x.NexusImageVisibility, Visibility.Collapsed);

			this.WhenAnyValue(x => x.NexusModsData.IsUpdated)
				.Select(PropertyConverters.BoolToVisibility)
				.ToUIProperty(this, x => x.NexusModsInformationVisibility, Visibility.Collapsed);

			this.WhenAnyValue(x => x.NexusModsData.CreatedTimestamp).SkipWhile(x => x <= 0).Select(x => DateUtils.UnixTimeStampToDateTime(x)).ToUIProperty(this, x => x.NexusModsCreatedDate);
			this.WhenAnyValue(x => x.NexusModsData.UpdatedTimestamp).SkipWhile(x => x <= 0).Select(x => DateUtils.UnixTimeStampToDateTime(x)).ToUIProperty(this, x => x.NexusModsUpdatedDate);

			this.WhenAnyValue(x => x.NexusModsCreatedDate, x => x.NexusModsUpdatedDate, x => x.NexusModsData.EndorsementCount)
				.Select(x => NexusModsInfoToTooltip(x.Item1, x.Item2, x.Item3)).ToUIProperty(this, x => x.NexusModsTooltipInfo);

			this.WhenAnyValue(x => x.IsForceLoaded, x => x.HasMetadata, x => x.IsForceLoadedMergedMod)
				.Select(b => b.Item1 && b.Item2 && !b.Item3 ? Visibility.Visible : Visibility.Collapsed)
				.ToUIProperty(this, x => x.ToggleForceAllowInLoadOrderVisibility, Visibility.Collapsed);

			this.WhenAnyValue(x => x.GitHubEnabled, x => x.GitHubData.IsEnabled, (b1, b2) => b1 && b2)
				.Select(PropertyConverters.BoolToVisibility)
				.ToUIProperty(this, x => x.OpenGitHubLinkVisibility, Visibility.Collapsed);

			this.WhenAnyValue(x => x.NexusModsEnabled, x => x.NexusModsData.ModId, (b, id) => b && id >= DivinityApp.NEXUSMODS_MOD_ID_START)
				.Select(PropertyConverters.BoolToVisibility)
				.ToUIProperty(this, x => x.OpenNexusModsLinkVisibility, Visibility.Collapsed);

			this.WhenAnyValue(x => x.SteamWorkshopEnabled, x => x.IsHidden, x => x.IsLarianMod, x => x.WorkshopData.ModId, CanOpenWorkshopBoolCheck)
				.Select(PropertyConverters.BoolToVisibility)
				.ToUIProperty(this, x => x.OpenWorkshopLinkVisibility, Visibility.Collapsed);

			var connection = this.Dependencies.Connect().ObserveOn(RxApp.MainThreadScheduler);
			connection.Bind(out displayedDependencies).DisposeMany().Subscribe();
			connection.CountChanged().Select(x => x.Count).ToUIProperty(this, x => x.TotalDependencies);
			this.WhenAnyValue(x => x.TotalDependencies, c => c > 0).ToUIProperty(this, x => x.HasDependencies);
			this.WhenAnyValue(x => x.HasDependencies).Select(PropertyConverters.BoolToVisibility).ToUIProperty(this, x => x.DependencyVisibility, Visibility.Collapsed);
			this.WhenAnyValue(x => x.IsActive, x => x.IsForceLoaded, x => x.IsForceLoadedMergedMod,
				x => x.ForceAllowInLoadOrder).Subscribe((b) =>
			{
				var isActive = b.Item1;
				var isForceLoaded = b.Item2;
				var isForceLoadedMergedMod = b.Item3;
				var forceAllowInLoadOrder = b.Item4;

				if(forceAllowInLoadOrder || isActive)
				{
					CanDrag = true;
				}
				else
				{
					CanDrag = !isForceLoaded || isForceLoadedMergedMod;
				}
			});

			this.WhenAnyValue(x => x.IsForceLoaded, x => x.IsEditorMod).Subscribe((b) =>
			{
				var isForceLoaded = b.Item1;
				var isEditorMod = b.Item2;

				if (isForceLoaded)
				{
					this.SelectedColor = "#64F38F00";
					this.ListColor = "#32C17200";
					HasColorOverride = true;
				}
				else if (isEditorMod)
				{
					this.SelectedColor = "#6400ED48";
					this.ListColor = "#0C00FF4D";
					HasColorOverride = true;
				}
				else
				{
					HasColorOverride = false;
				}
			});

			if (isBaseGameMod)
			{
				this.IsHidden = true;
				this.IsLarianMod = true;
			}

			// If a screen reader is active, don't bother making tooltips for the mod item entry
			this.WhenAnyValue(x => x.Description, x => x.HasDependencies, x => x.UUID).
				Select(x => !DivinityApp.IsScreenReaderActive() && (!String.IsNullOrEmpty(x.Item1) || x.Item2 || !String.IsNullOrEmpty(x.Item3)))
				.ToUIProperty(this, x => x.HasToolTip, true);

			this.WhenAnyValue(x => x.IsEditorMod, x => x.IsLarianMod, x => x.FilePath,
				(isEditorMod, isLarianMod, path) => !isEditorMod && !isLarianMod && File.Exists(path)).ToUIProperty(this, x => x.CanDelete);
			this.WhenAnyValue(x => x.ModType, x => x.IsLarianMod, x => x.IsForceLoaded, x => x.IsForceLoadedMergedMod, x => x.ForceAllowInLoadOrder,
				(modType, isLarianMod, isForceLoaded, isMergedMod, forceAllowInLoadOrder) => 
				modType != "Adventure" && !isLarianMod && (!isForceLoaded || isMergedMod) || forceAllowInLoadOrder)
				.ToUIProperty(this, x => x.CanAddToLoadOrder, true);

			var whenExtenderProp = this.WhenAnyValue(x => x.ExtenderModStatus, x => x.ScriptExtenderData.RequiredVersion, x => x.CurrentExtenderVersion);
			whenExtenderProp.Select(x => ExtenderStatusToToolTipText(x.Item1, x.Item2, x.Item3)).ToUIProperty(this, x => x.ScriptExtenderSupportToolTipText);
			this.WhenAnyValue(x => x.ExtenderModStatus).Select(x => x != DivinityExtenderModStatus.NONE ? Visibility.Visible : Visibility.Collapsed)
				.ToUIProperty(this, x => x.ExtenderStatusVisibility, Visibility.Collapsed);

			var whenOsirisStatusChanges = this.WhenAnyValue(x => x.OsirisModStatus);
			whenOsirisStatusChanges.Select(x => x != DivinityOsirisModStatus.NONE ? Visibility.Visible : Visibility.Collapsed).ToUIProperty(this, x => x.OsirisStatusVisibility);
			whenOsirisStatusChanges.Select(OsirisStatusToTooltipText).ToUIProperty(this, x => x.OsirisStatusToolTipText);

			this.WhenAnyValue(x => x.LastUpdated).SkipWhile(x => !x.HasValue)
				.Select(x => $"Last Modified on {x.Value.ToString(DivinityApp.DateTimeColumnFormat, CultureInfo.InstalledUICulture)}")
				.ToUIProperty(this, x => x.LastModifiedDateText, "");

			this.WhenAnyValue(x => x.FilePath).Select(PropertyConverters.StringToVisibility).ToUIProperty(this, x => x.HasFilePathVisibility, Visibility.Collapsed);
			this.WhenAnyValue(x => x.Version.Version).ToUIProperty(this, x => x.DisplayVersion, "0.0.0.0");

			if(!isBaseGameMod)
			{
				ModManagerConfig = new ModConfig
				{
					Id = this.UUID
				};

				this.WhenAnyValue(x => x.ModManagerConfig.Notes).ToUIProperty(this, x => x.Notes, "");

				this.WhenAnyValue(x => x.NexusModsData.ModId).BindTo(this, x => x.ModManagerConfig.NexusMods.ModId);
				this.WhenAnyValue(x => x.WorkshopData.ModId).BindTo(this, x => x.ModManagerConfig.SteamWorkshop.ModId);
				this.WhenAnyValue(x => x.GitHubData.Author).BindTo(this, x => x.ModManagerConfig.GitHub.Author);
				this.WhenAnyValue(x => x.GitHubData.Repository).BindTo(this, x => x.ModManagerConfig.GitHub.Repository);
			}
		}

		public static DivinityModData Clone(DivinityModData mod)
		{
			var cloneMod = new DivinityModData()
			{
				HasMetadata = mod.HasMetadata,
				UUID = mod.UUID,
				Name = mod.Name,
				Author = mod.Author,
				Version = new DivinityModVersion2(mod.Version.VersionInt),
				HeaderVersion = new DivinityModVersion2(mod.HeaderVersion.VersionInt),
				PublishVersion = new DivinityModVersion2(mod.PublishVersion.VersionInt),
				Folder = mod.Folder,
				Description = mod.Description,
				MD5 = mod.MD5,
				ModType = mod.ModType,
				Tags = mod.Tags.ToList(),
				Targets = mod.Targets,
			};
			cloneMod.Dependencies.AddRange(mod.Dependencies.Items);
			cloneMod.NexusModsData.Update(mod.NexusModsData);
			cloneMod.WorkshopData.Update(mod.WorkshopData);
			cloneMod.GitHubData.Update(mod.GitHubData);
			cloneMod.ApplyModConfig(mod.ModManagerConfig);
			return cloneMod;
		}
	}
}
