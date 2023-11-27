using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DivinityModManager.AppServices
{
	/// <summary>
	/// This service allows shell commands opening BG3MM to communicate with the running instance, using pipes.
	/// </summary>
	public class BackgroundCommandService
	{
		private NamedPipeServerStream _pipe;
		private IDisposable _backgroundTask;

		private async Task WaitForCommandAsync(IScheduler sch, CancellationToken token)
		{
			try
			{
				await _pipe.WaitForConnectionAsync(token);

				if (token.IsCancellationRequested) return;

				using (var sr = new StreamReader(_pipe, Encoding.UTF8))
				{
					var message = await sr.ReadToEndAsync();
					if(!String.IsNullOrEmpty(message))
					{
						if(message.IndexOf("nxm://") > -1)
						{
							var nexusMods = Services.Get<INexusModsService>();
							nexusMods.ProcessNXMLinkBackground(message);
						}
					}
				}
			}
			catch(Exception ex)
			{
				DivinityApp.Log($"Error with server pipe:\n{ex}");
			}

			if (token.IsCancellationRequested) return;

			RxApp.TaskpoolScheduler.Schedule(Restart);
		}

		public void Restart()
		{
			_backgroundTask?.Dispose();
			_pipe?.Dispose();
			_pipe = new NamedPipeServerStream(DivinityApp.PIPE_ID, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
			_backgroundTask = RxApp.TaskpoolScheduler.ScheduleAsync(WaitForCommandAsync);
		}

		public BackgroundCommandService()
		{
			Restart();
		}
	}
}
