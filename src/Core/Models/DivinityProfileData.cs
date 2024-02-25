using Alphaleonis.Win32.Filesystem;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace DivinityModManager.Models
{
	public class DivinityProfileData : ReactiveObject
	{
		[Reactive] public string Name { get; set; }

		/// <summary>
		/// The stored name in the profile.lsb or profile5.lsb file.
		/// </summary>
		[Reactive] public string ProfileName { get; set; }
		[Reactive] public string UUID { get; set; }
		[Reactive] public string FilePath { get; set; }
		[Reactive] public string ModSettingsFile { get; private set; }

		/// <summary>
		/// The saved load order from modsettings.lsx
		/// </summary>
		public List<string> ModOrder { get; set; } = new List<string>();

		/// <summary>
		/// The mod data under the Mods node, from modsettings.lsx.
		/// </summary>
		public List<DivinityProfileActiveModData> ActiveMods { get; set; } = new List<DivinityProfileActiveModData>();

		/// <summary>
		/// The ModOrder transformed into a DivinityLoadOrder. This is the "Current" order.
		/// </summary>
		public DivinityLoadOrder SavedLoadOrder { get; set; }

		public DivinityProfileData()
		{
			this.WhenAnyValue(x => x.FilePath).Select(x => !String.IsNullOrEmpty(x) ? Path.Combine(x, "modsettings.lsx") : "").BindTo(this, x => x.ModSettingsFile);
		}
	}
}
