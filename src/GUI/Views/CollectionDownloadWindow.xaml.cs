using AdonisUI;

using DivinityModManager.Models.NexusMods;
using DivinityModManager.ViewModels;

using Microsoft.Windows.Themes;

using NexusModsNET.DataModels.GraphQL.Types;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DivinityModManager.Views
{
	public class CollectionDownloadWindowBase : ReactiveUserControl<CollectionDownloadWindowViewModel> { }

	public partial class CollectionDownloadWindow : CollectionDownloadWindowBase
	{
		private void OpenWindow(IInteractionContext<NexusGraphCollectionRevision, bool> context)
		{
			ViewModel.Load(context.Input);
			context.SetOutput(false);
		}

		public CollectionDownloadWindow()
		{
			InitializeComponent();

			DivinityInteractions.OpenDownloadCollectionView.RegisterHandler(OpenWindow);

			this.WhenActivated(d =>
			{
				
			});
		}
	}
}
