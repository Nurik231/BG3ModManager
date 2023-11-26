using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Models.GitHub
{
	public class GitHubModData : ReactiveObject
	{
		[Reactive] public string Author { get; set; }
		[Reactive] public string Repository { get; set; }
		[Reactive] public GitHubLatestReleaseData LatestRelease { get; set; }

		[Reactive] public bool IsEnabled { get; private set; }

		public void Update(GitHubModData data)
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

		public GitHubModData()
		{
			LatestRelease = new GitHubLatestReleaseData();

			this.WhenAnyValue(x => x.Author, x => x.Repository)
				.Select(x => !String.IsNullOrEmpty(x.Item1) && !String.IsNullOrEmpty(x.Item2))
				.ObserveOn(RxApp.MainThreadScheduler)
				.BindTo(this, x => x.IsEnabled);
		}
	}
}
