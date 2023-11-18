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
	public class GithubModsCacheHandler : ReactiveObject, IExternalModCacheHandler<GithubModsCachedData>
	{
		public ModSourceType SourceType => ModSourceType.GITHUB;
		public string FileName => "githubdata.json";

		//Format Github data so people can more easily edit/add mods manually.
		public JsonSerializerSettings SerializerSettings => new JsonSerializerSettings()
		{
			NullValueHandling = NullValueHandling.Ignore,
			Formatting = Formatting.Indented,
		};

		[Reactive] public bool IsEnabled { get; set; }
		public GithubModsCachedData CacheData { get; set; }

		public GithubModsCacheHandler()
		{
			CacheData = new GithubModsCachedData();
			IsEnabled = false;
		}

		public void OnCacheUpdated(GithubModsCachedData cachedData)
		{

		}

		public async Task<bool> Update(IEnumerable<DivinityModData> mods, CancellationToken token)
		{
			DivinityApp.Log("Checking for Github mod updates.");
			var success = false;
			try
			{
				var github = Services.Get<IGithubService>();

				foreach (var mod in mods)
				{
					if (mod.GithubData != null && !String.IsNullOrEmpty(mod.GithubData.Author) && !String.IsNullOrEmpty(mod.GithubData.Repository))
					{
						var latestRelease = await github.GetLatestReleaseAsync(mod.GithubData.Author, mod.GithubData.Repository);
						if (latestRelease != null)
						{
							var releaseAsset = latestRelease.Assets.FirstOrDefault();
							if(releaseAsset != null)
							{
								mod.GithubData.LatestRelease.Version = latestRelease.TagName;
								mod.GithubData.LatestRelease.Date = latestRelease.CreatedAt.Ticks;
								mod.GithubData.LatestRelease.Description = latestRelease.Body;
								mod.GithubData.LatestRelease.BrowserDownloadLink = releaseAsset.BrowserDownloadUrl;
								success = true;
							}
						}
						CacheData.Mods[mod.UUID] = mod.GithubData;
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
