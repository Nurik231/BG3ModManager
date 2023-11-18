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

namespace DivinityModManager
{
	public interface IModUpdaterService
	{
		bool IsRefreshing { get; set; }
		NexusModsCacheHandler NexusMods { get; }
		SteamWorkshopCacheHandler SteamWorkshop { get; }
		GithubModsCacheHandler Github { get; }
		Task<bool> UpdateAsync(IEnumerable<DivinityModData> mods, CancellationToken token);
		Task<bool> LoadAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token);
		Task<bool> SaveAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token);
		Task<bool> GetGithubUpdatesAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token);
		Task<List<NexusModsModDownloadLink>> GetNexusModsUpdatesAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token);
		Task<bool> GetSteamWorkshopUpdatesAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token);

		bool DeleteCache();
	}
}

namespace DivinityModManager.AppServices
{
	public class ModUpdaterService : ReactiveObject, IModUpdaterService
	{
		private readonly NexusModsCacheHandler _nexus;
		public NexusModsCacheHandler NexusMods => _nexus;

		private readonly SteamWorkshopCacheHandler _workshop;
		public SteamWorkshopCacheHandler SteamWorkshop => _workshop;

		private readonly GithubModsCacheHandler _github;
		public GithubModsCacheHandler Github => _github;

		[Reactive] public bool IsRefreshing { get; set; }

		private readonly string _appVersion;

		private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			Formatting = Formatting.None
		};

		public async Task<bool> UpdateAsync(IEnumerable<DivinityModData> mods, CancellationToken token)
		{
			IsRefreshing = true;
			if (SteamWorkshop.IsEnabled) await SteamWorkshop.Update(mods, token);
			if (NexusMods.IsEnabled) await NexusMods.Update(mods, token);
			if (Github.IsEnabled) await Github.Update(mods, token);
			IsRefreshing = false;
			return false;
		}

		public async Task<bool> LoadAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token)
		{
			if (SteamWorkshop.IsEnabled)
			{
				if ((DateTimeOffset.Now.ToUnixTimeSeconds() - SteamWorkshop.CacheData.LastUpdated >= 3600))
				{
					await SteamWorkshop.LoadCacheAsync(currentAppVersion, token);
				}
			}
			if (NexusMods.IsEnabled)
			{
				await NexusMods.LoadCacheAsync(currentAppVersion, token);
			}
			if (Github.IsEnabled)
			{
				await Github.LoadCacheAsync(currentAppVersion, token);
			}

			await Observable.Start(() =>
			{
				foreach (var mod in mods)
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
						if (NexusMods.CacheData.Mods.TryGetValue(mod.UUID, out var nexusData))
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

		public async Task<bool> SaveAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token)
		{
			if (SteamWorkshop.IsEnabled)
			{
				await SteamWorkshop.SaveCacheAsync(true, currentAppVersion, token);
			}
			if (NexusMods.IsEnabled)
			{
				foreach (var mod in mods.Where(x => x.NexusModsData.ModId >= DivinityApp.NEXUSMODS_MOD_ID_START).Select(x => x.NexusModsData))
				{
					NexusMods.CacheData.Mods[mod.UUID] = mod;
				}
				await NexusMods.SaveCacheAsync(true, currentAppVersion, token);
			}
			if (Github.IsEnabled)
			{
				await Github.SaveCacheAsync(true, currentAppVersion, token);
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

		public async Task<bool> FetchUpdatesAsync(IEnumerable<DivinityModData> mods, CancellationToken token)
		{
			//TODO
			IsRefreshing = true;
			var githubResult = await GetGithubUpdatesAsync(mods, _appVersion, token);
			var nexusResult = await GetNexusModsUpdatesAsync(mods, _appVersion, token);
			var workshopResult = await GetSteamWorkshopUpdatesAsync(mods, _appVersion, token);
			IsRefreshing = false;
			return false;
		}

		public async Task<bool> GetGithubUpdatesAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token)
		{
			try
			{
				if (!Github.IsEnabled) return false;
				if(!Github.CacheData.CacheUpdated)
				{
					await Github.LoadCacheAsync(currentAppVersion, token);
					await Github.Update(mods, token);
					await Github.SaveCacheAsync(true, currentAppVersion, token);

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
				}
				//TODO
				return true;
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error fetching Github updates:\n{ex}");
			}
			return false;
		}

		public async Task<List<NexusModsModDownloadLink>> GetNexusModsUpdatesAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token)
		{
			try
			{
				if (!NexusMods.IsEnabled) return null;
				if (!NexusMods.CacheData.CacheUpdated)
				{
					await NexusMods.LoadCacheAsync(currentAppVersion, token);
					await NexusMods.Update(mods, token);
					await NexusMods.SaveCacheAsync(true, currentAppVersion, token);
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
				}
				var updates = await Services.Get<INexusModsService>().GetLatestDownloadsForModsAsync(mods, token);
				return updates;
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error fetching NexusMods updates:\n{ex}");
			}
			return null;
		}

		public async Task<bool> GetSteamWorkshopUpdatesAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token)
		{
			try
			{
				if (!SteamWorkshop.IsEnabled) return false;
				if (!SteamWorkshop.CacheData.CacheUpdated)
				{
					await SteamWorkshop.LoadCacheAsync(currentAppVersion, token);
					await SteamWorkshop.Update(mods, token);
					await SteamWorkshop.SaveCacheAsync(true, currentAppVersion, token);
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
				}
				//TODO
				return true;
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error fetching SteamWorkshop updates:\n{ex}");
			}
			return false;
		}

		public ModUpdaterService(string appVersion)
		{
			_appVersion = appVersion;

			_nexus = new NexusModsCacheHandler(DefaultSerializerSettings);
			_workshop = new SteamWorkshopCacheHandler(DefaultSerializerSettings);
			_github = new GithubModsCacheHandler();
		}
	}
}