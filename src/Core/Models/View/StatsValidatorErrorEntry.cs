using LSLib.LS.Stats;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System.IO;

namespace DivinityModManager.Models.View;
public class StatsValidatorErrorEntry : TreeViewEntry
{
	public override object ViewModel => this;

	public StatLoadingError Error { get; }

	[Reactive] public string Message { get; set; }
	[Reactive] public string Code { get; set; }

	[ObservableAsProperty] public bool IsError { get; }

	private static string FormatMessage(StatLoadingError message) => $"{message.Location.StartLine}: {message.Message} [{message.Code}]";

	public StatsValidatorErrorEntry(StatLoadingError error)
	{
		Error = error;

		Message = FormatMessage(Error);
		Code = Error.Code;
		this.WhenAnyValue(x => x.Code, code => code == DiagnosticCode.StatSyntaxError).ToUIProperty(this, x => x.IsError);
	}
}
