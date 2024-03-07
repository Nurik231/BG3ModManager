using LSLib.LS.Stats;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System.IO;

namespace DivinityModManager.ViewModels;
public class StatsValidatorWindowViewModel : ReactiveObject
{
	[Reactive] public string OutputText { get; private set; }

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
		if(result.Errors.Count > 0)
		{
			OutputText = String.Join(Environment.NewLine, result.Errors.Select(FormatMessage));
		}
		else
		{
			OutputText = "No errors found!";
		}
	}
}
