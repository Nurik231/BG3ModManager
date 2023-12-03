using DivinityModManager.Models.Mod;

using DynamicData;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Models.Settings
{
	public class UserModConfig : BaseSettings<UserModConfig>, ISerializableSettings
	{
		public Dictionary<string, ModConfig> Mods { get; set; }
		public Dictionary<string, ModConfig> Files { get; set; }

		public UserModConfig() : base("usermodconfig.json")
		{
			Mods = new Dictionary<string, ModConfig>();
			Files = new Dictionary<string, ModConfig>();
		}
	}
}
