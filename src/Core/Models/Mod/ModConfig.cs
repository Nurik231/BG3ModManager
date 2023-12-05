using DivinityModManager.Json;

using Newtonsoft.Json;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Models.Mod
{
	public class ModConfig : ReactiveObject, IObjectWithId
	{
		[JsonIgnore] public static string FileName => "ModManagerConfig.json";
		[JsonIgnore] public string Id { get; set; }

		[Reactive] public GitHubModConfig GitHub { get; set; }
		[Reactive] public NexusModsModConfig NexusMods { get; set; }
		[Reactive] public SteamWorkshopModConfig SteamWorkshop { get; set; }
		[Reactive] public string Notes { get; set; }

		public ModConfig()
		{
			GitHub = new GitHubModConfig();
			NexusMods = new NexusModsModConfig();
			SteamWorkshop = new SteamWorkshopModConfig();
			Notes = "";
		}
	}
}
