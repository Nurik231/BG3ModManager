using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DivinityModManager.Models.Mod
{
	public class GitHubModConfig : ReactiveObject
	{
		[Reactive] public string Author { get; set; }
		[Reactive] public string Repository { get; set; }
	}
}
