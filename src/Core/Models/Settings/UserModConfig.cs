using DivinityModManager.Json;
using DivinityModManager.Models.Mod;

using DynamicData;

using Newtonsoft.Json;

using ReactiveUI.Fody.Helpers;
using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DivinityModManager.Models.Settings
{
	public class UserModConfig : BaseSettings<UserModConfig>, ISerializableSettings
	{
		[JsonConverter(typeof(DictionaryToSourceCacheConverter<ModConfig>))]
		public SourceCache<ModConfig, string> Mods { get; set; }
		public Dictionary<string, long> LastUpdated { get; set; }

		private ICommand AutosaveCommand { get; set; }

		private void TrySave()
		{
			this.Save(out _);
		}

		public UserModConfig() : base("UserModConfig.json")
		{
			Mods = new SourceCache<ModConfig, string>(x => x.Id);
			LastUpdated = new Dictionary<string, long>();

			var props = typeof(ModConfig)
			.GetRuntimeProperties()
			.Where(prop => Attribute.IsDefined(prop, typeof(ReactiveAttribute)))
			.Select(prop => prop.Name)
			.ToArray();

			AutosaveCommand = ReactiveCommand.Create(TrySave);

			Mods.Connect().WhenAnyPropertyChanged(props).Throttle(TimeSpan.FromMilliseconds(25)).InvokeCommand(AutosaveCommand);
		}
	}
}
