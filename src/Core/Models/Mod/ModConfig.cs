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
		/// <summary>
		/// The mod UUID or FileName (override paks) associated with this config.
		/// </summary>
		[JsonIgnore] public string Id { get; set; }

		[Reactive] public string Notes { get; set; }

		[Reactive] public string GitHubAuthor { get; set; }
		[Reactive] public string GitHubRepository { get; set; }
		[Reactive] public long NexusModsId { get; set; }
		[Reactive] public long SteamWorkshopId { get; set; }

		public ModConfig()
		{
			Notes = "";
		}
	}
}
