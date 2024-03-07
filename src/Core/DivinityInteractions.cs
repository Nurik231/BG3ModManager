using DivinityModManager.Models;

using LSLib.LS.Stats;

using NexusModsNET.DataModels.GraphQL.Types;

using ReactiveUI;

namespace DivinityModManager;

public record struct DeleteFilesViewConfirmationData(int Total, bool PermanentlyDelete, CancellationToken Token);
public record struct ValidateModStatsResults(List<DivinityModData> Mods, List<StatLoadingError> Errors);


public static class DivinityInteractions
{
	public static readonly Interaction<DeleteFilesViewConfirmationData, bool> ConfirmModDeletion = new();
	public static readonly Interaction<DivinityModData, bool> OpenModProperties = new();
	public static readonly Interaction<NexusGraphCollectionRevision, bool> OpenDownloadCollectionView = new();
	public static readonly Interaction<ValidateModStatsResults, bool> OpenValidateStatsResults = new();
}
