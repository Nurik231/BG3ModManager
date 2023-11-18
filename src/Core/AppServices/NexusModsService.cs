using DivinityModManager.Models;
using DivinityModManager.Models.NexusMods;
using DivinityModManager.Models.Updates;

using DynamicData.Binding;

using NexusModsNET;
using NexusModsNET.DataModels;
using NexusModsNET.Inquirers;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DivinityModManager
{
	public interface INexusModsService
	{
		string ApiKey { get; set; }
		bool IsInitialized { get; }
		bool LimitExceeded { get; }
		bool CanFetchData { get; }
		bool IsPremium { get; }
		Uri ProfileAvatarUrl { get; }

		NexusModsObservableApiLimits ApiLimits { get; }

		IObservable<NexusModsObservableApiLimits> WhenLimitsChange { get; }

		Task<List<NexusModsModDownloadLink>> GetLatestDownloadsForModsAsync(IEnumerable<DivinityModData> mods, CancellationToken t);
		Task<UpdateResult> LoadAllModsDataAsync(IEnumerable<DivinityModData> mods, CancellationToken t);
	}
}

namespace DivinityModManager.AppServices
{
	public class NexusModsService : ReactiveObject, INexusModsService
	{
		private INexusModsClient _client;
		private InfosInquirer _dataLoader;

		[Reactive] public string ApiKey { get; set; }
		[Reactive] public bool IsPremium { get; private set; }
		[Reactive] public Uri ProfileAvatarUrl { get; private set; }

		private readonly NexusModsObservableApiLimits _apiLimits;
		public NexusModsObservableApiLimits ApiLimits => _apiLimits;

		public bool IsInitialized => _client != null;
		public bool LimitExceeded => LimitExceededCheck();
		public bool CanFetchData => IsInitialized && !LimitExceeded;

		private readonly IObservable<NexusModsObservableApiLimits> _whenLimitsChange;
		public IObservable<NexusModsObservableApiLimits> WhenLimitsChange => _whenLimitsChange;

		private bool LimitExceededCheck()
		{
			if (_client != null)
			{
				var daily = _client.RateLimitsManagement.ApiDailyLimitExceeded();
				var hourly = _client.RateLimitsManagement.ApiHourlyLimitExceeded();

				if (daily)
				{
					DivinityApp.Log($"Daily limit exceeded ({_client.RateLimitsManagement.APILimits.DailyLimit})");
					return true;
				}
				else if (hourly)
				{
					DivinityApp.Log($"Hourly limit exceeded ({_client.RateLimitsManagement.APILimits.HourlyLimit})");
					return true;
				}
			}
			return false;
		}

		public bool CanDoTask(int apiCalls)
		{
			if (_client != null)
			{
				var currentLimit = Math.Min(_client.RateLimitsManagement.APILimits.HourlyRemaining, _client.RateLimitsManagement.APILimits.DailyRemaining);
				if (currentLimit > apiCalls)
				{
					return true;
				}
			}
			return false;
		}

		public async Task<NexusUser> GetUserAsync(CancellationToken t)
		{
			if (!CanFetchData) return null;
			return await _dataLoader.User.GetUserAsync(t);
		}

		public async Task<List<NexusModsModDownloadLink>> GetLatestDownloadsForModsAsync(IEnumerable<DivinityModData> mods, CancellationToken t)
		{
			var links = new List<NexusModsModDownloadLink>();
			if (!CanFetchData) return links;
			
			try
			{
				//1 call for the mod files, 1 call to get a mod file link
				var apiCallAmount = mods.Count(x => x.NexusModsData.ModId >= DivinityApp.NEXUSMODS_MOD_ID_START) & 2;
				if (!CanDoTask(apiCallAmount))
				{
					DivinityApp.Log($"Task would exceed hourly or daily API limits. ExpectedCalls({apiCallAmount}) HourlyRemaining({ApiLimits.HourlyRemaining}/{ApiLimits.HourlyLimit}) DailyRemaining({ApiLimits.DailyRemaining}/{ApiLimits.DailyLimit})");
					return links;
				}
				foreach (var mod in mods)
				{
					if (mod.NexusModsData.ModId >= DivinityApp.NEXUSMODS_MOD_ID_START)
					{
						var result = await _dataLoader.ModFiles.GetModFilesAsync(DivinityApp.NEXUSMODS_GAME_DOMAIN, mod.NexusModsData.ModId, t);
						if (result != null)
						{
							var file = result.ModFiles.FirstOrDefault(x => x.IsPrimary || x.Category == NexusModFileCategory.Main);
							if (file != null)
							{
								var fileId = file.FileId;
								var linkResult = await _dataLoader.ModFiles.GetModFileDownloadLinksAsync(DivinityApp.NEXUSMODS_GAME_DOMAIN, mod.NexusModsData.ModId, fileId, t);
								if (linkResult != null && linkResult.Count() > 0)
								{
									var primaryLink = linkResult.FirstOrDefault();
									links.Add(new NexusModsModDownloadLink(mod, primaryLink, file));
								}
							}
						}
					}

					if (t.IsCancellationRequested) break;
				}
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error fetching NexusMods data:\n{ex}");
			}

			return links;
		}

		public async Task<UpdateResult> LoadAllModsDataAsync(IEnumerable<DivinityModData> mods, CancellationToken t)
		{
			var taskResult = new UpdateResult();
			if (!CanFetchData)
			{
				taskResult.Success = false;
				if (_client == null)
				{
					taskResult.FailureMessage = "API Client not initialized.";
				}
				else
				{
					var rateLimits = _client.RateLimitsManagement.APILimits;
					taskResult.FailureMessage = $"API limit exceeded. Hourly({rateLimits.HourlyRemaining}/{rateLimits.HourlyLimit}) Daily({rateLimits.DailyRemaining}/{rateLimits.DailyLimit})";
				}
				return taskResult;
			}
			var totalLoaded = 0;

			try
			{
				var targetMods = mods.Where(mod => mod.NexusModsData.ModId >= DivinityApp.NEXUSMODS_MOD_ID_START).ToList();
				var total = targetMods.Count;
				if (total == 0)
				{
					taskResult.Success = false;
					taskResult.FailureMessage = "Skipping. No mods to check (no NexusMods ID set in the loaded mods).";
					return taskResult;
				}

				var apiCallAmount = total; // 1 call for 1 mod
				if (!CanDoTask(total))
				{
					DivinityApp.Log($"Task would exceed hourly or daily API limits. ExpectedCalls({apiCallAmount}) HourlyRemaining({ApiLimits.HourlyRemaining}/{ApiLimits.HourlyLimit}) DailyRemaining({ApiLimits.DailyRemaining}/{ApiLimits.DailyLimit})");
					return taskResult;
				}

				DivinityApp.Log($"Using NexusMods API to update {total} mods");

				var dataLoader = new InfosInquirer(_client);
				foreach (var mod in targetMods)
				{
					var result = await dataLoader.Mods.GetMod(DivinityApp.NEXUSMODS_GAME_DOMAIN, mod.NexusModsData.ModId, t);
					if (result != null)
					{
						mod.NexusModsData.Update(result);
						taskResult.UpdatedMods.Add(mod);
						totalLoaded++;
					}

					if (t.IsCancellationRequested) break;
				}
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error fetching NexusMods data:\n{ex}");
			}

			return taskResult;
		}

		public NexusModsService(string appName, string appVersion)
		{
			_apiLimits = new NexusModsObservableApiLimits();
			_whenLimitsChange = _apiLimits.WhenAnyPropertyChanged();
			this.WhenAnyValue(x => x.ApiKey).Subscribe(key =>
			{
				_client?.Dispose();
				_dataLoader?.Dispose();

				if (!String.IsNullOrEmpty(key))
				{
					_client = NexusModsClient.Create(key, appName, appVersion, _apiLimits);
					_dataLoader = new InfosInquirer(_client);

					if (ProfileAvatarUrl == null)
					{
						RxApp.TaskpoolScheduler.ScheduleAsync(async (sch, cts) =>
						{
							var user = await GetUserAsync(cts);
							if (user != null)
							{
								IsPremium = user.IsPremium;
								ProfileAvatarUrl = user.ProfileAvatarUrl;
							}
						});
					}
				}
				else
				{
					_apiLimits.Reset();
				}
			});
		}
	}
}