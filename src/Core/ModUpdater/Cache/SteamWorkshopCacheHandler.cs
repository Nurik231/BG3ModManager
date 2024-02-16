using DivinityModManager.Models;
using DivinityModManager.Models.Cache;
using DivinityModManager.Util;

using Newtonsoft.Json;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DivinityModManager.ModUpdater.Cache
{
	public class SteamWorkshopCacheHandler : ReactiveObject, IExternalModCacheHandler<SteamWorkshopCachedData>
	{
		public ModSourceType SourceType => ModSourceType.STEAM;
		public string FileName => "steamworkshopdata.json";
		public JsonSerializerSettings SerializerSettings { get; }
		public SteamWorkshopCachedData CacheData { get; set; }
		[Reactive] public bool IsEnabled { get; set; }

		public string SteamAppID { get; set; }

		public SteamWorkshopCacheHandler(JsonSerializerSettings serializerSettings)
		{
			SerializerSettings = serializerSettings;
			CacheData = new SteamWorkshopCachedData();
			IsEnabled = false;
		}

		public void OnCacheUpdated(SteamWorkshopCachedData cachedData)
		{

		}

		public async Task<bool> Update(IEnumerable<DivinityModData> mods, CancellationToken token)
		{
			DivinityApp.Log("Checking for Steam Workshop updates.");
			var success = await WorkshopDataLoader.GetAllWorkshopDataAsync(CacheData, SteamAppID, token);
			if (success)
			{
				var cachedGUIDs = CacheData.Mods.Keys.ToHashSet();
				var nonWorkshopMods = mods.Where(x => !cachedGUIDs.Contains(x.UUID)).ToList();
				if (nonWorkshopMods.Count > 0)
				{
					foreach (var m in nonWorkshopMods)
					{
						CacheData.AddNonWorkshopMod(m.UUID);
					}
				}
			}
			return false;
		}
	}
}
