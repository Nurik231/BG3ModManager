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
	public class GitHubModsCacheHandler : ReactiveObject, IExternalModCacheHandler<GitHubModsCachedData>
	{
		public ModSourceType SourceType => ModSourceType.GITHUB;
		public string FileName => "GithubData.json";

		//Format GitHub data so people can more easily edit/add mods manually.
		public JsonSerializerSettings SerializerSettings => new()
		{
			NullValueHandling = NullValueHandling.Ignore,
			Formatting = Formatting.Indented,
		};

		[Reactive] public bool IsEnabled { get; set; }
		public GitHubModsCachedData CacheData { get; set; }

		public GitHubModsCacheHandler()
		{
			CacheData = new GitHubModsCachedData();
			IsEnabled = false;
		}

		public void OnCacheUpdated(GitHubModsCachedData cachedData)
		{

		}

		public async Task<bool> Update(IEnumerable<DivinityModData> mods, CancellationToken token)
		{
			DivinityApp.Log("Checking for GitHub mod updates.");
			var success = false;
			try
			{
				var github = Services.Get<IGitHubService>();

				foreach (var mod in mods)
				{
					if (mod.GitHubData != null && !String.IsNullOrEmpty(mod.GitHubData.Author) && !String.IsNullOrEmpty(mod.GitHubData.Repository))
					{
						var latestRelease = await github.GetLatestReleaseAsync(mod.GitHubData.Author, mod.GitHubData.Repository);
						if (latestRelease != null)
						{
							mod.GitHubData.LatestRelease.Version = latestRelease.Version;
							mod.GitHubData.LatestRelease.Date = latestRelease.Date;
							mod.GitHubData.LatestRelease.Description = latestRelease.Description;
							mod.GitHubData.LatestRelease.BrowserDownloadLink = latestRelease.BrowserDownloadLink;
							success = true;
						}
						CacheData.Mods[mod.UUID] = mod.GitHubData;
					}
				}
			}
			catch(Exception ex)
			{
				DivinityApp.Log($"Error fetching updates: {ex}");
			}
			return success;
		}
	}
}
