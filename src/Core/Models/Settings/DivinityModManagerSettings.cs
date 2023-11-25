using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Windows.Input;
using DivinityModManager.Util;
using System.Reactive.Disposables;
using System.Reflection;
using Alphaleonis.Win32.Filesystem;
using DivinityModManager.Models.App;
using System.Reactive;
using ReactiveUI.Fody.Helpers;
using System.ComponentModel;
using DivinityModManager.Models.Extender;
using System.Reactive.Concurrency;
using DivinityModManager.Extensions;

namespace DivinityModManager.Models
{
	[DataContract]
	public class DivinityModManagerSettings : ReactiveObject
	{
		[SettingsEntry("Game Data Path", "The path to the Data folder, for loading editor mods.\nExample: Baldur's Gate 3/Data")]
		[DataMember][Reactive] public string GameDataPath { get; set; }

		[SettingsEntry("Game Executable Path", "The path to bg3.exe")]
		[DataMember][Reactive] public string GameExecutablePath { get; set; }

		[DefaultValue("")]
		[SettingsEntry("NexusMods API Key", "Your personal NexusMods API key, which will allow the mod manager to fetch mod updates/information", HideFromUI = true)]
		[DataMember][Reactive] public string NexusModsAPIKey { get; set; }

		[DefaultValue(false)]
		[SettingsEntry("Enable Story Log", "When launching the game, enable the Osiris story log (osiris.log)")]
		[DataMember][Reactive] public bool GameStoryLogEnabled { get; set; }

		[DefaultValue(false)]
		[SettingsEntry("Disable Launcher Telemetry", "Disable the telemetry options in the launcher\nTelemetry is always disabled if mods are active")]
		[DataMember][Reactive] public bool DisableLauncherTelemetry { get; set; }

		[DefaultValue(false)]
		[SettingsEntry("Disable Launcher Warnings", "Disable the mod/data mismatch warnings in the launcher")]
		[DataMember][Reactive] public bool DisableLauncherModWarnings { get; set; }

		[DefaultValue(false)]
		[SettingsEntry("Enable DirectX 11 Mode", "If enabled, when launching the game, bg3_dx11.exe is used instead")]
		[DataMember][Reactive] public bool LaunchDX11 { get; set; }

		[DefaultValue(true)]
		[SettingsEntry("Skip Launcher", "Pass the --skip-launcher args when launching the game")]
		[DataMember][Reactive] public bool SkipLauncher { get; set; }

		[DefaultValue(false)]
		[SettingsEntry("Launch Through Steam", "Launch the game through steam, instead of by the exe directly")]
		[DataMember][Reactive] public bool LaunchThroughSteam { get; set; }

		//[SettingsEntry("Workshop Path", "The Steam Workshop folder for Baldur's Gate 3\nUsed for detecting mod updates and new mods to be copied into the local mods folder\nExample: Steam/steamapps/workshop/content/1086940")]
		[DataMember][Reactive] public string WorkshopPath { get; set; }

		[DefaultValue("Orders")]
		[SettingsEntry("Saved Load Orders Path", "The folder containing mod load orders")]
		[DataMember][Reactive] public string LoadOrderPath { get; set; }

		[DefaultValue(false)]
		[SettingsEntry("Enable Internal Log", "Enable the log for the mod manager", HideFromUI = true)]
		[DataMember][Reactive] public bool LogEnabled { get; set; }

		[DefaultValue(true)]
		[SettingsEntry("Auto Add Missing Dependencies When Exporting", "Automatically add dependency mods above their dependents in the exported load order, if omitted from the active order")]
		[DataMember][Reactive] public bool AutoAddDependenciesWhenExporting { get; set; }

		[DefaultValue(true)]
		[SettingsEntry("Enable Automatic Updates", "Automatically check for updates when the program starts")]
		[DataMember][Reactive] public bool CheckForUpdates { get; set; }

		[DefaultValue("")]
		[SettingsEntry("AppData Path Override", "[EXPERIMENTAL]\nOverride the default location to %LOCALAPPDATA%\\Larian Studios\\Baldur's Gate 3\nThis folder is used when exporting load orders, loading profiles, and loading mods.")]
		[DataMember][Reactive] public string DocumentsFolderPathOverride { get; set; }

		//[SettingsEntry("Automatically Load GM Campaign Mods", "When a GM campaign is selected, its dependency mods will automatically be loaded without needing to manually import them")]
		//[DataMember][Reactive] public bool AutomaticallyLoadGMCampaignMods { get; set; }
		//TODO - Waiting for DM mode
		public bool AutomaticallyLoadGMCampaignMods => false;

		[DataMember][Reactive] public long LastUpdateCheck { get; set; }

		[DataMember][Reactive] public string LastOrder { get; set; }

		[DataMember][Reactive] public string LastImportDirectoryPath { get; set; }
		[DataMember][Reactive] public string LastLoadedOrderFilePath { get; set; }
		[DataMember][Reactive] public string LastExtractOutputPath { get; set; }

		[DefaultValue(true)]
		[DataMember][Reactive] public bool DarkThemeEnabled { get; set; }

		[DefaultValue(true)]
		[SettingsEntry("Shift Focus on Swap", "When moving selected mods to the opposite list with Enter, move focus to that list as well")]
		[DataMember][Reactive] public bool ShiftListFocusOnSwap { get; set; }

		[DataMember] public ScriptExtenderSettings ExtenderSettings { get; set; }
		[DataMember] public ScriptExtenderUpdateConfig ExtenderUpdaterSettings { get; set; }

		public string DefaultExtenderLogDirectory { get; set; }

		public string ExtenderLogDirectory
		{
			get
			{
				if (ExtenderSettings == null || String.IsNullOrWhiteSpace(ExtenderSettings.LogDirectory))
				{
					return DefaultExtenderLogDirectory;
				}
				return ExtenderSettings.LogDirectory;
			}
		}

		[DefaultValue(DivinityGameLaunchWindowAction.None)]
		[SettingsEntry("On Game Launch", "When the game launches through the mod manager, this action will be performed")]
		[DataMember][Reactive]
		public DivinityGameLaunchWindowAction ActionOnGameLaunch { get; set; }

		[DefaultValue(false)]
		[SettingsEntry("Disable Missing Mod Warnings", "If a load order is missing mods, no warnings will be displayed")]
		[DataMember][Reactive] public bool DisableMissingModWarnings { get; set; }

		[DefaultValue(false)]
		[SettingsEntry("Disable Checking for Steam Workshop Tags", "The mod manager will try and find mod tags from the workshop by default")]
		[DataMember][Reactive] public bool DisableWorkshopTagCheck { get; set; }

		[DefaultValue(false)]
		[Reactive] public bool DisplayFileNames { get; set; }

		[DefaultValue(false)]
		[SettingsEntry("Mod Developer Mode", "This enables features for mod developers, such as being able to copy a mod's UUID in context menus, and additional Script Extender options", HideFromUI = true)]
		[Reactive][DataMember]
		public bool DebugModeEnabled { get; set; }

		[DefaultValue("")]
		[Reactive][DataMember] public string GameLaunchParams { get; set; }

		[DefaultValue(false)]
		[Reactive][DataMember] public bool GameMasterModeEnabled { get; set; }

		[DataMember] public WindowSettings Window { get; set; }

		[DefaultValue(false)]
		[SettingsEntry("Save Window Location", "Save and restore the window location when the application starts.")]
		[DataMember][Reactive] public bool SaveWindowLocation { get; set; }

		public bool Loaded { get; set; }

		private bool canSaveSettings = false;

		public bool CanSaveSettings
		{
			get => canSaveSettings;
			set { this.RaiseAndSetIfChanged(ref canSaveSettings, value); }
		}

		public bool SettingsWindowIsOpen { get; set; }

		public DivinityModManagerSettings()
		{
			Loaded = false;
			//Defaults
			ExtenderSettings = new ScriptExtenderSettings();
			ExtenderUpdaterSettings = new ScriptExtenderUpdateConfig();
			Window = new WindowSettings();

			var properties = typeof(DivinityModManagerSettings)
			.GetRuntimeProperties()
			.Where(prop => Attribute.IsDefined(prop, typeof(DataMemberAttribute)))
			.Select(prop => prop.Name)
			.ToArray();

			this.WhenAnyPropertyChanged(properties).Subscribe((c) =>
			{
				if (SettingsWindowIsOpen) CanSaveSettings = true;
			});

			var extenderProperties = typeof(ScriptExtenderSettings)
			.GetRuntimeProperties()
			.Where(prop => Attribute.IsDefined(prop, typeof(DataMemberAttribute)))
			.Select(prop => prop.Name)
			.ToArray();

			ExtenderSettings.WhenAnyPropertyChanged(extenderProperties).Subscribe((c) =>
			{
				if (SettingsWindowIsOpen) CanSaveSettings = true;
				this.RaisePropertyChanged("ExtenderLogDirectory");
			});

			var extenderUpdaterProperties = typeof(ScriptExtenderUpdateConfig)
			.GetRuntimeProperties()
			.Where(prop => Attribute.IsDefined(prop, typeof(DataMemberAttribute)))
			.Select(prop => prop.Name)
			.ToArray();

			ExtenderUpdaterSettings.WhenAnyPropertyChanged(extenderUpdaterProperties).Subscribe((c) =>
			{
				if (SettingsWindowIsOpen) CanSaveSettings = true;
			});

			this.WhenAnyValue(x => x.DebugModeEnabled).Subscribe(b => DivinityApp.DeveloperModeEnabled = b);

			this.SetToDefault();
		}
	}
}
