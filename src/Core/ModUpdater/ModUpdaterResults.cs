using DivinityModManager.Models;
using DivinityModManager.Models.GitHub;
using DivinityModManager.Models.NexusMods;

namespace DivinityModManager.ModUpdater
{
	public class ModUpdaterResults
	{
		public Dictionary<string, GitHubLatestReleaseData> GitHub { get; }
		public Dictionary<string, NexusModsModDownloadLink> NexusMods { get; }
		public Dictionary<string, DivinityModData> SteamWorkshop { get; }

		public ModUpdaterResults(Dictionary<string, GitHubLatestReleaseData> github, Dictionary<string, NexusModsModDownloadLink> nexusMods, Dictionary<string, DivinityModData> steamWorkshop)
		{
			GitHub = github;
			NexusMods = nexusMods;
			SteamWorkshop = steamWorkshop;
		}
	}
}
