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

		private static Version PathToVersion(string path)
		{
			if(Version.TryParse(Path.GetFileName(path), out var version))
			{
				return version;
			}
			return null;
		}

		public static bool NetCoreRuntimeGreaterThanOrEqualTo(int majorVersion)
		{
			if(Directory.Exists(NET_CORE_DIR))
			{
				try
				{
					var versions = Directory.EnumerateDirectories(NET_CORE_DIR, DirectoryEnumerationOptions.Folders).Select(PathToVersion);
					foreach (var version in versions)
					{
						if (version != null && version.Major >= majorVersion)
						{
							return true;
						}
					}
				}
				catch(Exception ex)
				{
					DivinityApp.Log($"Error checking directories for .NET:\n{ex}");
				}
			}
			return false;
		}
	}
}
