using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager
{
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
}
