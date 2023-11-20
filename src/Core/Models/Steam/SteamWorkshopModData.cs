using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Models.Steam
{
	public class SteamWorkshopModData : ReactiveObject
	{
		[Reactive] public long ModId { get; set; }
		[Reactive] public DateTime CreatedDate { get; set; }
		[Reactive] public DateTime UpdatedDate { get; set; }

		public List<string> Tags { get; set; }

		public void Update(SteamWorkshopModData otherData)
		{
			ModId = otherData.ModId;
			CreatedDate = otherData.CreatedDate;
			UpdatedDate = otherData.UpdatedDate;
			Tags = otherData.Tags;
		}
	}
}
