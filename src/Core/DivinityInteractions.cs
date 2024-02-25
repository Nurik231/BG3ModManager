using DivinityModManager.Models;

using ReactiveUI;

using System.Threading;

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
	}
}
