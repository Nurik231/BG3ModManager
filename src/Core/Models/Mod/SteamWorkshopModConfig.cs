using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DivinityModManager.Models.Mod
{
	public class SteamWorkshopModConfig : ReactiveObject
	{
		[Reactive] public long ModId { get; set; }
	}
}
