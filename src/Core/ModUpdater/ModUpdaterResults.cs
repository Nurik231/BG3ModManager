using DivinityModManager.Models;
using DivinityModManager.Models.Github;
using DivinityModManager.Models.NexusMods;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.ModUpdater
{
	public class ModUpdaterResults
	{
		public Dictionary<string, GithubLatestReleaseData> Github { get; }
		public Dictionary<string, NexusModsModDownloadLink> NexusMods { get; }
		public Dictionary<string, DivinityModData> SteamWorkshop { get; }

		public ModUpdaterResults(Dictionary<string, GithubLatestReleaseData> github, Dictionary<string, NexusModsModDownloadLink> nexusMods, Dictionary<string, DivinityModData> steamWorkshop)
		{
			Github = github;
			NexusMods = nexusMods;
			SteamWorkshop = steamWorkshop;
		}
	}
}
