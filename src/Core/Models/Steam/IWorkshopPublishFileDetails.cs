using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Models.Steam
{
	public interface IWorkshopPublishFileDetails
	{
		long PublishedFileId { get; set; }
		long TimeCreated { get; set; }
		long TimeUpdated { get; set; }

		List<WorkshopTag> Tags { get; set; }
	}
}
