using DivinityModManager.Models;

using NexusModsNET.DataModels.GraphQL.Types;

using ReactiveUI;

namespace DivinityModManager
{
	public struct DeleteFilesViewConfirmationData
	{
		public int Total;
		public bool PermanentlyDelete;
		public CancellationToken Token;
	}

	public static class DivinityInteractions
	{
		public static readonly Interaction<DeleteFilesViewConfirmationData, bool> ConfirmModDeletion = new();
		public static readonly Interaction<DivinityModData, bool> OpenModProperties = new();
		public static readonly Interaction<NexusGraphCollectionRevision, bool> OpenDownloadCollectionView = new();
	}
}
