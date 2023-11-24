using DivinityModManager.Util;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DivinityModManager.Models.Updates
{
	public enum ModDownloadPathType
	{
		FILE,
		URL
	}

	public class ModDownloadData : ReactiveObject
	{
		[Reactive] public string DownloadPath { get; set; }
		[Reactive] public ModDownloadPathType DownloadPathType { get; set; }
		[Reactive] public ModSourceType DownloadSourceType { get; set; }
		[Reactive] public DateTime? Date { get; set; }
		[Reactive] public string Version { get; set; }
		[Reactive] public string Description { get; set; }
		[Reactive] public bool IsIndirectDownload { get; set; }

		private bool FileNamesMatch(string localFilePath, string newFilePath) => Path.GetFileName(localFilePath).Equals(Path.GetFileName(newFilePath), StringComparison.OrdinalIgnoreCase);

		private void MoveOldPakToRecycleBin(string previousFilePath, string newFilePath)
		{
			if (!String.IsNullOrEmpty(previousFilePath) && FileNamesMatch(previousFilePath, newFilePath))
			{
				RecycleBinHelper.DeleteFile(previousFilePath, false, false);
			}
		}

		private static System.IO.Stream MakeFileStream(string path) => File.Open(path, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write, System.IO.FileShare.None, 4096, true);

		public async Task<bool> DownloadAsync(string previousFilePath, string outputDirectory, CancellationToken token)
		{
			try
			{
				DivinityApp.Log($"Downloading {DownloadPath} - DownloadPathType({DownloadPathType}) DownloadSourceType({DownloadSourceType})");
				if (DownloadPathType == ModDownloadPathType.FILE)
				{
					var outputFilePath = Path.Combine(outputDirectory, DownloadPath);
					//This covers when an update changes the pak name
					MoveOldPakToRecycleBin(previousFilePath, outputFilePath);
					await DivinityFileUtils.CopyFileAsync(DownloadPath, outputFilePath, token);
					return true;
				}
				else if (DownloadPathType == ModDownloadPathType.URL)
				{
					var success = false;
					if(IsIndirectDownload)
					{
						//Nexus non-premium users need to go to the website and get a nxm:// link to have download authorization.
						DivinityFileUtils.TryOpenPath(DownloadPath);
						return true;
					}
					else
					{
						using (var webStream = await WebHelper.DownloadFileAsStreamAsync(DownloadPath, token))
						{
							if (webStream == null) return false;

							if (DownloadPath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
							{
								var outputFilePath = Path.Combine(outputDirectory, Path.GetFileName(DownloadPath));
								MoveOldPakToRecycleBin(previousFilePath, outputFilePath);
								using (var outputFile = MakeFileStream(outputFilePath))
								{
									await webStream.CopyToAsync(outputFile, 4096, token);
									success = true;
								}
							}
							else
							{
								ZipArchive archive = new ZipArchive(webStream);
								foreach (ZipArchiveEntry entry in archive.Entries)
								{
									if (entry.Name.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
									{
										using (var entryStream = entry.Open())
										{
											var outputFilePath = Path.Combine(outputDirectory, Path.GetFileName(entry.Name));
											MoveOldPakToRecycleBin(previousFilePath, outputFilePath);
											using (var outputFile = MakeFileStream(outputFilePath))
											{
												await entryStream.CopyToAsync(outputFile, 4096, token);
												success = true;
											}
										}
									}
								}
							}
						}
					}
					return success;
				}
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error downloading update ({DownloadPath}): {ex}");
			}
			return false;
		}
	}
}
