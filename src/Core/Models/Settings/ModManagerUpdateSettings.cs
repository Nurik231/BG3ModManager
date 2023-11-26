using DivinityModManager.Extensions;
using DivinityModManager.Util;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

		private readonly bool _isAssociatedWithNXM;
		public bool IsAssociatedWithNXM => _isAssociatedWithNXM;

		public ModManagerUpdateSettings()
		{
			_isAssociatedWithNXM = DivinityRegistryHelper.IsAssociatedWithNXMProtocol();
			this.RaisePropertyChanged(nameof(IsAssociatedWithNXM));
			this.SetToDefault();
		}
	}
}
