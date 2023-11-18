using Alphaleonis.Win32.Filesystem;

using DivinityModManager.Models;
using DivinityModManager.Models.Github;
using DivinityModManager.Models.NexusMods;
using DivinityModManager.ModUpdater;
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
		Task<bool> UpdateInfoAsync(IEnumerable<DivinityModData> mods, CancellationToken token);
		Task<bool> LoadCacheAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token);
		Task<bool> SaveCacheAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token);

		Task<ModUpdaterResults> FetchUpdatesAsync(DivinityModManagerSettings settings, IEnumerable<DivinityModData> mods, CancellationToken token);
		Task<Dictionary<string, GithubLatestReleaseData>> GetGithubUpdatesAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token);
		Task<Dictionary<string, NexusModsModDownloadLink>> GetNexusModsUpdatesAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token);
		Task<Dictionary<string, DivinityModData>> GetSteamWorkshopUpdatesAsync(DivinityModManagerSettings settings, IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token);

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

		public async Task<bool> UpdateInfoAsync(IEnumerable<DivinityModData> mods, CancellationToken token)
		{
			IsRefreshing = true;
			if (SteamWorkshop.IsEnabled) await SteamWorkshop.Update(mods, token);
			if (NexusMods.IsEnabled) await NexusMods.Update(mods, token);
			if (Github.IsEnabled) await Github.Update(mods, token);
			IsRefreshing = false;
			return false;
		}

		public async Task<bool> LoadCacheAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token)
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

		public async Task<bool> SaveCacheAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token)
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

		public async Task<ModUpdaterResults> FetchUpdatesAsync(DivinityModManagerSettings settings, IEnumerable<DivinityModData> mods, CancellationToken token)
		{
			//TODO
			IsRefreshing = true;
			var githubResults = await GetGithubUpdatesAsync(mods, _appVersion, token);
			var nexusResults = await GetNexusModsUpdatesAsync(mods, _appVersion, token);
			var workshopResults = await GetSteamWorkshopUpdatesAsync(settings, mods, _appVersion, token);
			IsRefreshing = false;
			return new ModUpdaterResults(githubResults, nexusResults, workshopResults);
		}

		public async Task<Dictionary<string, GithubLatestReleaseData>> GetGithubUpdatesAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token)
		{
			var results = new Dictionary<string, GithubLatestReleaseData>();
			try
			{
				if (!Github.IsEnabled) return results;
				if (!Github.CacheData.CacheUpdated)
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
								results.Add(mod.UUID, githubData.LatestRelease);
								mod.GithubData.Update(githubData);
							}
						}
						return Unit.Default;
					}, RxApp.MainThreadScheduler);
				}
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error fetching Github updates:\n{ex}");
			}
			return results;
		}

		public async Task<Dictionary<string, NexusModsModDownloadLink>> GetNexusModsUpdatesAsync(IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token)
		{
			var results = new Dictionary<string, NexusModsModDownloadLink>();
			try
			{
				if (!NexusMods.IsEnabled) return results;
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
				results = await Services.Get<INexusModsService>().GetLatestDownloadsForModsAsync(mods, token);
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error fetching NexusMods updates:\n{ex}");
			}
			return results;
		}

		public async Task<Dictionary<string, DivinityModData>> GetSteamWorkshopUpdatesAsync(DivinityModManagerSettings settings, IEnumerable<DivinityModData> mods, string currentAppVersion, CancellationToken token)
		{
			var results = new Dictionary<string, DivinityModData>();
			try
			{
				if (!SteamWorkshop.IsEnabled) return results;
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

				var workshopMods = await DivinityModDataLoader.LoadModPackageDataAsync(settings.WorkshopPath, token);
				foreach (var workshopMod in workshopMods.Mods)
				{
					string workshopID = Directory.GetParent(workshopMod.FilePath)?.Name;
					if (!String.IsNullOrEmpty(workshopID))
					{
						workshopMod.WorkshopData.ID = workshopID;
					}
				}

				foreach (var mod in workshopMods.Mods)
				{
					results.Add(mod.UUID, mod);
				}
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error fetching SteamWorkshop updates:\n{ex}");
			}
			return results;
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