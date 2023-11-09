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
	public class NexusModsCacheHandler : ReactiveObject, IExternalModCacheHandler<NexusModsCachedData>
	{
		public ModSourceType SourceType => ModSourceType.NEXUSMODS;
		public string FileName => "nexusmodsdata.json";
		public JsonSerializerSettings SerializerSettings => ModUpdateHandler.DefaultSerializerSettings;
		[Reactive] public bool IsEnabled { get; set; }
		public NexusModsCachedData CacheData { get; set; }

		public string APIKey { get; set; }
		public string AppName { get; set; }
		public string AppVersion { get; set; }

		public NexusModsCacheHandler()
		{
			CacheData = new NexusModsCachedData();
			IsEnabled = false;
		}

		public void OnCacheUpdated(NexusModsCachedData cachedData)
		{
			foreach (var entry in cachedData.Mods)
			{
				if (CacheData.Mods.TryGetValue(entry.Key, out var existing))
				{
					if (existing.UpdatedTimestamp < entry.Value.UpdatedTimestamp || !existing.IsUpdated)
					{
						CacheData.Mods[entry.Key] = entry.Value;
					}
				}
				else
				{
					CacheData.Mods[entry.Key] = entry.Value;
				}
			}
		}

		public async Task<bool> Update(IEnumerable<DivinityModData> mods, CancellationToken cts)
		{
			if (!NexusModsDataLoader.IsInitialized && !string.IsNullOrEmpty(APIKey))
			{
				NexusModsDataLoader.Init(APIKey, AppName, AppVersion);
			}

			if (NexusModsDataLoader.CanFetchData)
			{
				var result = await NexusModsDataLoader.LoadAllModsDataAsync(mods, cts);

				if (result.Success)
				{
					DivinityApp.Log($"Fetched NexusMods mod info for {result.UpdatedMods.Count} mod(s).");

					foreach (var mod in mods.Where(x => x.NexusModsData.ModId >= DivinityApp.NEXUSMODS_MOD_ID_START).Select(x => x.NexusModsData))
					{
						CacheData.Mods[mod.UUID] = mod;
					}

					return true;
				}
				else
				{
					DivinityApp.Log($"Failed to update NexusMods mod info:\n{result.FailureMessage}");
				}
			}
			else
			{
				DivinityApp.Log("NexusModsAPIKey not set, or daily/hourly limit reached. Skipping.");
			}
			return false;
		}
	}
}
