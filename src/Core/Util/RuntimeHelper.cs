using Alphaleonis.Win32.Filesystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Util
{
	public static class RuntimeHelper
	{
		private static readonly string NET_CORE_DIR = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App";

		public static bool NetCoreRuntimeGreaterThan(int majorVersion)
		{
			if(Directory.Exists(NET_CORE_DIR))
			{
				var versions = Directory.EnumerateDirectories(NET_CORE_DIR, DirectoryEnumerationOptions.Folders).Select(x => Version.Parse(Path.GetFileName(x)));
				foreach(var version in versions)
				{
					if(version != null && version.Major >= majorVersion)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
