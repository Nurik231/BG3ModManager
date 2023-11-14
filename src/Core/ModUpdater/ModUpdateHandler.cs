using DivinityModManager.Models;
using DivinityModManager.Models.NexusMods;
using DivinityModManager.ModUpdater.Cache;
using DivinityModManager.Util;

using Newtonsoft.Json;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DivinityModManager.ModUpdater
{
	public class ModUpdateHandler : ReactiveObject
	{
		private readonly NexusModsCacheHandler _nexus;
		public NexusModsCacheHandler NexusMods => _nexus;

		private readonly SteamWorkshopCacheHandler _workshop;
		public SteamWorkshopCacheHandler SteamWorkshop => _workshop;

		private readonly GithubModsCacheHandler _github;
		public GithubModsCacheHandler Github => _github;

		[Reactive] public bool IsRefreshing { get; set; }

		public static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			Formatting = Formatting.None
		};

		public async Task<bool> UpdateAsync(IEnumerable<DivinityModData> mods, CancellationToken cts)
		{
			IsRefreshing = true;
			if (SteamWorkshop.IsEnabled) await SteamWorkshop.Update(mods, cts);
			if (NexusMods.IsEnabled) await NexusMods.Update(mods, cts);
			if (Github.IsEnabled) await Github.Update(mods, cts);
			IsRefreshing = false;
			return false;
		}

		public async Task<bool> LoadAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken cts)
		{
			if(SteamWorkshop.IsEnabled)
			{
				if((DateTimeOffset.Now.ToUnixTimeSeconds() - SteamWorkshop.CacheData.LastUpdated >= 3600))
				{
					await SteamWorkshop.LoadCacheAsync(currentAppVersion, cts);
				}
			}
			if(NexusMods.IsEnabled)
			{
				await NexusMods.LoadCacheAsync(currentAppVersion, cts);
			}
			if(Github.IsEnabled)
			{
				await Github.LoadCacheAsync(currentAppVersion, cts);
			}

			await Observable.Start(() =>
			{
				foreach(var mod in mods)
				{
					if (SteamWorkshop.IsEnabled)
					{
						if (SteamWorkshop.CacheData.Mods.TryGetValue(mod.UUID, out var workshopData))
						{
							if (string.IsNullOrEmpty(mod.WorkshopData.ID) || mod.WorkshopData.ID == workshopData.WorkshopID)
							{
								mod.WorkshopData.ID = workshopData.WorkshopID;
								mod.WorkshopData.CreatedDate = DateUtils.UnixTimeStampToDateTime(workshopData.Created);
								mod.WorkshopData.UpdatedDate = DateUtils.UnixTimeStampToDateTime(workshopData.LastUpdated);
								mod.WorkshopData.Tags = workshopData.Tags;
								mod.AddTags(workshopData.Tags);
								if (workshopData.LastUpdated > 0)
								{
									mod.LastUpdated = mod.WorkshopData.UpdatedDate;
								}
							}
						}
					}
					if (NexusMods.IsEnabled)
					{
						if(NexusMods.CacheData.Mods.TryGetValue(mod.UUID, out var nexusData))
						{
							mod.NexusModsData.Update(nexusData);
						}
					}
					if (Github.IsEnabled)
					{
						if (Github.CacheData.Mods.TryGetValue(mod.UUID, out var githubData))
						{
							mod.GithubData.Update(githubData);
						}
					}
				}
				return Unit.Default;
			}, RxApp.MainThreadScheduler);

			return false;
		}

		public async Task<bool> SaveAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken cts)
		{
			if(SteamWorkshop.IsEnabled)
			{
				await SteamWorkshop.SaveCacheAsync(true, currentAppVersion, cts);
			}
			if(NexusMods.IsEnabled)
			{
				foreach (var mod in mods.Where(x => x.NexusModsData.ModId >= DivinityApp.NEXUSMODS_MOD_ID_START).Select(x => x.NexusModsData))
				{
					NexusMods.CacheData.Mods[mod.UUID] = mod;
				}
				await NexusMods.SaveCacheAsync(true, currentAppVersion, cts);
			}
			if(Github.IsEnabled)
			{
				await Github.SaveCacheAsync(true, currentAppVersion, cts);
			}
			return false;
		}

		public bool DeleteCache()
		{
			var b1 = NexusMods.DeleteCache();
			var b2 = SteamWorkshop.DeleteCache();
			var b3 = Github.DeleteCache();
			return b1 || b2 || b3;
		}

		public async Task<bool> RefreshGithubAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken cts)
		{
			try
			{
				await Github.LoadCacheAsync(currentAppVersion, cts);
				await Github.Update(mods, cts);
				await Github.SaveCacheAsync(true, currentAppVersion, cts);

				await Observable.Start(() =>
				{
					foreach (var mod in mods)
					{
						if (Github.CacheData.Mods.TryGetValue(mod.UUID, out var githubData))
						{
							mod.GithubData.Update(githubData);
						}
					}
					return Unit.Default;
				}, RxApp.MainThreadScheduler);
				return true;
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error fetching Github updates:\n{ex}");
			}
			return false;
		}

		public async Task<List<NexusModsModDownloadLink>> RefreshNexusModsAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken cts)
		{
			try
			{
				await NexusMods.LoadCacheAsync(currentAppVersion, cts);
				await NexusMods.Update(mods, cts);
				await NexusMods.SaveCacheAsync(true, currentAppVersion, cts);
				var updates = await NexusModsDataLoader.GetLatestDownloadsForModsAsync(mods, cts);
				await Observable.Start(() =>
				{
					foreach (var mod in mods)
					{
						if (NexusMods.CacheData.Mods.TryGetValue(mod.UUID, out var nexusData))
						{
							mod.NexusModsData.Update(nexusData);
						}
					}
					return Unit.Default;
				}, RxApp.MainThreadScheduler);
				return updates;
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error fetching NexusMods updates:\n{ex}");
			}
			return null;
		}

		public async Task<bool> RefreshSteamWorkshopAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken cts)
		{
			try
			{
				await SteamWorkshop.LoadCacheAsync(currentAppVersion, cts);
				await SteamWorkshop.Update(mods, cts);
				await SteamWorkshop.SaveCacheAsync(true, currentAppVersion, cts);
				await Observable.Start(() =>
				{
					foreach (var mod in mods)
					{
						if (SteamWorkshop.CacheData.Mods.TryGetValue(mod.UUID, out var workshopData))
						{
							if (string.IsNullOrEmpty(mod.WorkshopData.ID) || mod.WorkshopData.ID == workshopData.WorkshopID)
							{
								mod.WorkshopData.ID = workshopData.WorkshopID;
								mod.WorkshopData.CreatedDate = DateUtils.UnixTimeStampToDateTime(workshopData.Created);
								mod.WorkshopData.UpdatedDate = DateUtils.UnixTimeStampToDateTime(workshopData.LastUpdated);
								mod.WorkshopData.Tags = workshopData.Tags;
								mod.AddTags(workshopData.Tags);
								if (workshopData.LastUpdated > 0)
								{
									mod.LastUpdated = mod.WorkshopData.UpdatedDate;
								}
							}
						}
					}
					return Unit.Default;
				}, RxApp.MainThreadScheduler);
				return true;
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error fetching SteamWorkshop updates:\n{ex}");
			}
			return false;
		}

		public ModUpdateHandler()
		{
			_nexus = new NexusModsCacheHandler();
			_workshop = new SteamWorkshopCacheHandler();
			_github = new GithubModsCacheHandler();
		}
	}
}
