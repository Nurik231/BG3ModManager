using DivinityModManager.Models;
using DivinityModManager.Models.Cache;

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

		public async Task<bool> Update(IEnumerable<DivinityModData> mods, CancellationToken cts)
		{
			DivinityApp.Log("Checking for Github mod updates.");
			return false;
		}
	}
}
