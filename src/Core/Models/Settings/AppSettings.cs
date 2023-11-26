using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DynamicData;
using ReactiveUI.Fody.Helpers;
using DivinityModManager.Models.App;

namespace DivinityModManager.Models.Settings
{
	public class AppSettings : ReactiveObject
	{
		[Reactive] public DefaultPathwayData DefaultPathways { get; set; }
		[Reactive] public AppFeatures Features { get; set; }

		public AppSettings()
		{
			DefaultPathways = new DefaultPathwayData();
			Features = new AppFeatures();
		}
	}
}
