using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Octokit;

namespace DivinityModManager
{
	public interface IGitHubService
	{
		Task<Release> GetLatestReleaseAsync(string owner, string repo);
	}
}

namespace DivinityModManager.AppServices
{
	public class GitHubService : IGitHubService
	{
		private readonly GitHubClient _client;

		public async Task<Release> GetLatestReleaseAsync(string owner, string repo)
		{
			var result = await _client.Repository.Release.GetLatest(owner, repo);
			return result;
		}

		public GitHubService(string appName, string appVersion)
		{
			_client = new GitHubClient(new ProductHeaderValue(appName, appVersion));
		}
	}
}
