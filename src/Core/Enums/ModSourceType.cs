using System.ComponentModel;

namespace DivinityModManager;

public enum ModSourceType
{
	[Description("None")]
	NONE,
	[Description("GitHub")]
	GITHUB,
	[Description("Nexus Mods")]
	NEXUSMODS,
	[Description("Steam Workshop")]
	STEAM
}
