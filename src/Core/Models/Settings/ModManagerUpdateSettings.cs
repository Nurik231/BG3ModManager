using DivinityModManager.Extensions;
using DivinityModManager.Util;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DivinityModManager.Models.Settings
{
	[DataContract]
	public class ModManagerUpdateSettings : ReactiveObject
	{
		[DefaultValue(true)]
		[SettingsEntry("Update the Script Extender", "If the Script Extender updater is installed (DXGI.dll), automatically update it via Tools/Toolbox.exe")]
		[DataMember, Reactive] public bool UpdateScriptExtender { get; set; }

		[DefaultValue(true)]
		[SettingsEntry("Update GitHub Mods", "Automatically check for mod updates for mods configured with GitHub repository releases")]
		[DataMember, Reactive] public bool UpdateGitHubMods { get; set; }

		[DefaultValue(true)]
		[SettingsEntry("Update NexusMods Mods", "Automatically check for mod updates for mods configured with NexusMods releases")]
		[DataMember, Reactive] public bool UpdateNexusMods { get; set; }

		[DefaultValue(true)]
		[SettingsEntry("Update Steam Workshop Mods", "Automatically check for mod updates for mods configured with Steam Workshop releases", HideFromUI=true)]
		[DataMember, Reactive] public bool UpdateSteamWorkshopMods { get; set; }

		[DefaultValue("")]
		[SettingsEntry("NexusMods API Key", "Your personal NexusMods API key, which will allow the mod manager to fetch mod updates/information", HideFromUI = true)]
		[DataMember, Reactive] public string NexusModsAPIKey { get; set; }

		[DefaultValue(typeof(TimeSpan), "30mm")] // 30 minutes, or 2hh for 2 hours etc
		[SettingsEntry("Minimum Update Time Period", "Prevent checking for updates for mods until this amount of time has passed\nThis is to prevent hitting API limits too quickly")]
		[DataMember, Reactive] public TimeSpan MinimumUpdateTimePeriod { get; set; }

		[Reactive] public bool IsAssociatedWithNXM { get; set; }

		public ModManagerUpdateSettings()
		{
			IsAssociatedWithNXM = DivinityRegistryHelper.IsAssociatedWithNXMProtocol(DivinityApp.GetExePath());
			this.SetToDefault();
		}
	}
}
