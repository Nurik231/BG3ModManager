using DivinityModManager.Json;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System.Runtime.Serialization;

namespace DivinityModManager.Models.Mod;

[DataContract]
public class ModConfig : ReactiveObject, IObjectWithId
{
	public static string FileName => "ModManagerConfig.json";
	/// <summary>
	/// The mod UUID or FileName (override paks) associated with this config.
	/// </summary>
	public bool IsLoaded { get; set; }
	public string Id { get; set; }

	[Reactive, DataMember] public string Notes { get; set; }

	[Reactive, DataMember] public string GitHubAuthor { get; set; }
	[Reactive, DataMember] public string GitHubRepository { get; set; }
	[Reactive, DataMember] public long NexusModsId { get; set; }
	[Reactive, DataMember] public long SteamWorkshopId { get; set; }

	public ModConfig()
	{

	}
}
