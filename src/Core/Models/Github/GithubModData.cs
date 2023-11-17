using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Models.Github
{
	public class GithubModData : ReactiveObject
	{
		[Reactive] public string Author { get; set; }
		[Reactive] public string Repository { get; set; }
		[Reactive] public GithubLatestReleaseData LatestRelease { get; set; }

		public void Update(GithubModData data)
		{
			Author = data.Author;
			Repository = data.Repository;
			if(data.LatestRelease != null)
			{
				LatestRelease.Version =	data.LatestRelease.Version;
				LatestRelease.Description =	data.LatestRelease.Description;
				LatestRelease.Date =	data.LatestRelease.Date;
				LatestRelease.BrowserDownloadLink = data.LatestRelease.BrowserDownloadLink;
			}
		}

		public GithubModData()
		{
			LatestRelease = new GithubLatestReleaseData();
		}
	}
}
