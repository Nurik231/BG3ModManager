using DivinityModManager.Models;
using DivinityModManager.Models.View;
using DynamicData.Binding;

using LSLib.LS.Stats;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace DivinityModManager.ViewModels;
public class StatsValidatorWindowViewModel : ReactiveObject
{
	[Reactive] public DivinityModData Mod { get; set; }
	[Reactive] public string OutputText { get; private set; }

	public ObservableCollectionExtended<StatsValidatorFileResults> Entries { get; }

	[ObservableAsProperty] public string ModName { get; }

	private static string FormatMessage(StatLoadingError message)
	{
		string result = "";
		if (message.Code == DiagnosticCode.StatSyntaxError)
		{
			result += "[ERR] ";
		}
		else
		{
			result += "[WARN] ";
		}

		if (!String.IsNullOrEmpty(message.Location?.FileName))
		{
			var baseName = Path.GetFileName(message.Location.FileName);
			result += $"{baseName}:{message.Location.StartLine}: ";
		}

		result += $"[{message.Code}] {message.Message}";
		return result;
	}

	public void Load(ValidateModStatsResults result)
	{
		RxApp.MainThreadScheduler.Schedule(() =>
		{
			Mod = result.Mods.FirstOrDefault();
			Entries.Clear();

			if (result.Errors.Count == 0)
			{
				OutputText = "No issues found!";
			}
			else
			{
				OutputText = $"{result.Errors.Count} issue(s):";
			}

			var entries = result.Errors.GroupBy(x => x.Location?.FileName);
			foreach(var fileGroup in entries)
			{
				var name = fileGroup.Key;
				if (String.IsNullOrEmpty(name)) name = "Unknown";
				StatsValidatorFileResults fileResults = new() { FilePath = name };
				foreach (var entry in fileGroup)
				{
					fileResults.Children.Add(new StatsValidatorErrorEntry(entry));
				}
				Entries.Add(fileResults);
			}
		});
	}

	public StatsValidatorWindowViewModel()
	{
		Entries = [];

		this.WhenAnyValue(x => x.Mod).WhereNotNull().Select(x => x.DisplayName).ToUIProperty(this, x => x.ModName);
	}
}
