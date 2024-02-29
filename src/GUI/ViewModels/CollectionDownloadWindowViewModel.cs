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

		private readonly ReadOnlyObservableCollection<NexusModsCollectionModData> _mods;
		public ReadOnlyObservableCollection<NexusModsCollectionModData> Mods => _mods;

		[ObservableAsProperty] public string Title { get; }
		[ObservableAsProperty] public string CreatedByText { get; }
		[ObservableAsProperty] public Visibility AuthorAvatarVisibility { get; }
		[ObservableAsProperty] public BitmapImage AuthorAvatar { get; }

		public ICommand SelectAllCommand { get; }
		public ICommand ConfirmCommand { get; set; }
		public ICommand CancelCommand { get; set; }

		public void Load(NexusGraphCollectionRevision collectionRevision)
		{
			Data = NexusModsCollectionData.FromCollectionRevision(collectionRevision);
		}

		private static string AuthorToCreatedByText(string author)
		{
			var text = "";
			if (!String.IsNullOrEmpty(author))
			{
				text = "Created by " + author;
			}
			return text;
		}

		public CollectionDownloadWindowViewModel()
		{
			Data.Mods.Connect().ObserveOn(RxApp.MainThreadScheduler).Bind(out _mods).Subscribe();

			this.WhenAnyValue(x => x.Data.Name).ToUIProperty(this, x => x.Title);
			this.WhenAnyValue(x.Data.Author).Select(AuthorToCreatedByText).ToUIProperty(this, x => x.CreatedByText);

			var whenAvatar = this.WhenAnyValue(x => x.Data.AuthorAvatarUrl);
			whenAvatar.Select(x => x != null ? Visibility.Visible : Visibility.Collapsed).ToUIProperty(this, x => x.AuthorAvatarVisibility);
			whenAvatar.Select(PropertyHelpers.UriToImage).ToUIProperty(this, x => x.AuthorAvatar);

			SelectAllCommand = ReactiveCommand.Create<bool>(b =>
			{
				foreach(var mod in Data.Mods.Items)
				{
					mod.IsSelected = b;
				}
			});
		}
	}
}
