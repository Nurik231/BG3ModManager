using Alphaleonis.Win32.Filesystem;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DivinityModManager.Util
{
	public class TempFile : IDisposable
	{
		private readonly System.IO.FileStream _stream;
		private readonly string _path;
		private readonly string _sourcePath;

		public System.IO.FileStream Stream => _stream;
		public string FilePath => _path;
		public string SourceFilePath => _sourcePath;

		private TempFile(string sourcePath)
		{
			var tempDir = DivinityApp.GetAppDirectory("Temp");
			Directory.CreateDirectory(tempDir);
			_path = Path.Combine(tempDir, Path.GetFileName(sourcePath));
			_sourcePath = sourcePath;
			_stream = File.Create(_path, 4096, System.IO.FileOptions.Asynchronous | System.IO.FileOptions.DeleteOnClose, PathFormat.LongFullPath);
		}

		public static async Task<TempFile> CreateAsync(string sourcePath, CancellationToken token)
		{
			var temp = new TempFile(sourcePath);
			await temp.CopyAsync(token);
			return temp;
		}

		public static async Task<TempFile> CreateAsync(string sourcePath, System.IO.Stream sourceStream, CancellationToken token)
		{
			var temp = new TempFile(sourcePath);
			await temp.CopyAsync(sourceStream, token);
			return temp;
		}

		private async Task CopyAsync(CancellationToken token)
		{
			using var sourceStream = File.Open(_path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read, 4096, true);
			await sourceStream.CopyToAsync(_stream, 4096, token);
		}

		private async Task CopyAsync(System.IO.Stream sourceStream, CancellationToken token)
		{
			await sourceStream.CopyToAsync(_stream, 4096, token);
		}

		public void Dispose()
		{
			_stream?.Dispose();
		}
	}
}
