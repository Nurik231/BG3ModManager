using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace DivinityModManager
{
	/// <summary>
	/// Currently used to monitor when the game is launched, in order to prevent launching multiple instances of the game.
	/// </summary>
	public interface IGameUtilitiesService
	{
		bool IsActive { get; }
		bool GameIsRunning { get; }
		TimeSpan ProcessCheckInterval { get; set; }
		void AddGameProcessName(string name);
		void CheckForGameProcess();
	}
}

namespace DivinityModManager.AppServices
{
	public class GameUtilitiesService : ReactiveObject, IGameUtilitiesService
	{
		[Reactive] public bool GameIsRunning { get; private set; }
		[Reactive] public TimeSpan ProcessCheckInterval { get; set; }
		[ObservableAsProperty] public bool IsActive { get; }

		private IDisposable _backgroundCheckTask;

		private static readonly HashSet<string> GameExeNames = new HashSet<string>() { "bg3", "bg3_dx11" };

		public void CheckForGameProcess()
		{
			var processes = GameExeNames.SelectMany(x => Process.GetProcessesByName(x)).ToArray();
			GameIsRunning = processes.Length > 0;
		}

		public void AddGameProcessName(string name)
		{
			GameExeNames.Add(name);
		}

		public GameUtilitiesService()
		{
			var whenInterval = this.WhenAnyValue(x => x.ProcessCheckInterval);
			whenInterval.Select(x => x.Ticks > 0).ToPropertyEx(this, x => x.IsActive);
			whenInterval.Subscribe(interval =>
			{
				_backgroundCheckTask?.Dispose();
				if (interval.Ticks > 0)
				{
					//Run once to update the bool
					CheckForGameProcess();
					_backgroundCheckTask = RxApp.TaskpoolScheduler.SchedulePeriodic(interval, () => CheckForGameProcess());
				}
			});

			ProcessCheckInterval = TimeSpan.FromSeconds(30);
		}
	}
}