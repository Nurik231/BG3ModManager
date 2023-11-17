using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Octokit;

namespace DivinityModManager
{
	public interface IGithubService
	{
		Task<Release> GetLatestReleaseAsync(string owner, string repo);
	}
}

namespace DivinityModManager.AppServices
{
	public class GithubService : IGithubService
	{
		private readonly GitHubClient _client;

		public async Task<Release> GetLatestReleaseAsync(string owner, string repo)
		{
			var result = await _client.Repository.Release.GetLatest(owner, repo);
			return result;
		}

		public GithubService(string appName, string appVersion)
		{
			_client = new GitHubClient(new ProductHeaderValue(appName, appVersion));
		}
	}
}
