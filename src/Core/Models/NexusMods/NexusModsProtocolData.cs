using System;
using System.Text.RegularExpressions;
using System.Web;

namespace DivinityModManager.Models.NexusMods
{
	public class NexusModsProtocolData
	{
		private static Regex _nxmPattern = new Regex("nxm://(?<game>[^/]+)/mods/(?<mod>[^/]+)/files/(?<file>[^?]+)", RegexOptions.IgnoreCase);

		public string GameId { get; set; }
		public int ModId { get; set; }
		public int FileId { get; set; }
		public int UserId { get; set; }
		public string Key { get; set; }
		public int Expires { get; set; }

		public bool IsValid => !String.IsNullOrEmpty(GameId) && ModId > 0 && FileId > 0 && UserId > 0 && !String.IsNullOrEmpty(Key) && Expires > 0;

		public string AsUrl => $"nxm://{GameId}/mods/{ModId}/files/{FileId}?key={Key}&expires={Expires}&user_id={UserId}";

		public override string ToString()
		{
			return $"[NexusModsProtocolData] IsValid({IsValid}) GameId({GameId}) ModId({ModId}) FileId({FileId}) UserId({UserId}) Key({Key}) Expires({Expires})";
		}

		private static int TryParseGroup(Group group)
		{
			if(int.TryParse(group?.Value, out var value))
			{
				return value;
			}
			return 0;
		}

		public static NexusModsProtocolData FromUrl(string url)
		{
			var data = new NexusModsProtocolData();
			if (!String.IsNullOrEmpty(url))
			{
				var urlData = _nxmPattern.Match(url);
				if (urlData.Success)
				{
					data.GameId = urlData.Groups["game"]?.Value;
					data.ModId = TryParseGroup(urlData.Groups["mod"]);
					data.FileId = TryParseGroup(urlData.Groups["file"]);
					//Replace ? with &, otherwise the ?key part will be included as the first key, aint with the url
					var queryData = HttpUtility.ParseQueryString(url.Replace("?", "&"));
					if(queryData != null)
					{
						data.Key = queryData.Get("key");
						if(int.TryParse(queryData.Get("expires"), out var expires))
						{
							data.Expires = expires;
						}
						if(int.TryParse(queryData.Get("user_id"), out var userId))
						{
							data.UserId = userId;
						}
					}
				}
			}
			return data;
		}
	}
}
