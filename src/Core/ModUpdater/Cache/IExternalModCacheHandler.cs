﻿using Alphaleonis.Win32.Filesystem;

using DivinityModManager.Models;
using DivinityModManager.Models.Cache;
using DivinityModManager.Util;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DivinityModManager.ModUpdater.Cache
{
	public interface IExternalModCacheHandler<T> where T : IModCacheData
	{
		ModSourceType SourceType { get; }
		string FileName { get; }
		JsonSerializerSettings SerializerSettings { get; }

		bool IsEnabled { get; set; }
		T CacheData { get; set; }
		void OnCacheUpdated(T cachedData);
		Task<bool> Update(IEnumerable<DivinityModData> mods, CancellationToken token);
	}

	public static class IExternalModCacheDataExtensions
	{
		public static async Task<T> LoadCacheAsync<T>(this IExternalModCacheHandler<T> handler, string currentAppVersion, CancellationToken token) where T : IModCacheData
		{
			var filePath = DivinityApp.GetAppDirectory("Data", handler.FileName);

			if (File.Exists(filePath))
			{
				var cachedData = await DivinityJsonUtils.DeserializeFromPathAsync<T>(filePath, token);
				if (cachedData != null)
				{
					if (string.IsNullOrEmpty(cachedData.LastVersion) || cachedData.LastVersion != currentAppVersion)
					{
						cachedData.LastUpdated = -1;
					}
					cachedData.CacheUpdated = true;
					handler.OnCacheUpdated(cachedData);
					return cachedData;
				}
			}
			return default;
		}

		public static async Task<bool> SaveCacheAsync<T>(this IExternalModCacheHandler<T> handler, bool updateLastTimestamp, string currentAppVersion, CancellationToken token) where T : IModCacheData
		{
			try
			{
				var parentDir = DivinityApp.GetAppDirectory("Data");
				var filePath = Path.Combine(parentDir, handler.FileName);
				if (!Directory.Exists(parentDir)) Directory.CreateDirectory(parentDir);

				if (updateLastTimestamp)
				{
					handler.CacheData.LastUpdated = DateTimeOffset.Now.ToUnixTimeSeconds();
				}
				handler.CacheData.LastVersion = currentAppVersion;

				string contents = JsonConvert.SerializeObject(handler.CacheData, handler.SerializerSettings);

				var buffer = Encoding.UTF8.GetBytes(contents);
				using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create,
					System.IO.FileAccess.Write, System.IO.FileShare.None, buffer.Length, true))
				{
					await fs.WriteAsync(buffer, 0, buffer.Length, token);
				}

				return true;
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error saving cache:\n{ex}");
			}
			return false;
		}

		public static bool DeleteCache<T>(this IExternalModCacheHandler<T> handler, bool permanent = false) where T : IModCacheData
		{
			try
			{
				var parentDir = DivinityApp.GetAppDirectory("Data");
				var filePath = Path.Combine(parentDir, handler.FileName);
				if (File.Exists(filePath))
				{
					RecycleBinHelper.DeleteFile(filePath, false, permanent);
					return true;
				}
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error saving cache:\n{ex}");
			}
			return false;
		}
	}
}
