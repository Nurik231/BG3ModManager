using DivinityModManager.Models.NexusMods;
using DivinityModManager.Models.Updates;
using DivinityModManager.Util;
using DivinityModManager.Views;

using DynamicData;
using DynamicData.Binding;

using NexusModsNET.DataModels.GraphQL.Types;

using Ookii.Dialogs.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DivinityModManager.ViewModels
{
	public class CollectionDownloadWindowViewModel : ReactiveObject
	{
		[Reactive] public NexusModsCollectionData Data { get; private set; }

		[ObservableAsProperty] public string Name { get; }
		[ObservableAsProperty] public Visibility AuthorAvatarVisibility { get; }
		[ObservableAsProperty] public BitmapImage AuthorAvatar { get; }

		public void Load(NexusGraphCollectionRevision collectionRevision)
		{
			Data = NexusModsCollectionData.FromCollectionRevision(collectionRevision);
		}

		public CollectionDownloadWindowViewModel()
		{
			this.WhenAnyValue(x => x.Data.Name).ToUIProperty(this, x => x.Name);
			var whenAvatar = this.WhenAnyValue(x => x.Data.AuthorAvatarUrl);
			whenAvatar.Select(x => x != null ? Visibility.Visible : Visibility.Collapsed).ToUIProperty(this, x => x.AuthorAvatarVisibility);
			whenAvatar.Select(PropertyHelpers.UriToImage).ToUIProperty(this, x => x.AuthorAvatar);
		}
	}
}
