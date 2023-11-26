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
	public class ModConfig
	{
		[JsonIgnore] public static string FileName => "ModManagerConfig.json";

		public GitHubModConfig GitHub { get; set; }
		public NexusModsModConfig NexusMods { get; set; }
		public SteamWorkshopModConfig SteamWorkshop { get; set; }
	}
}
