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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DivinityModManager.ViewModels
{
	public class CollectionDownloadWindowViewModel : ReactiveObject
	{
		[Reactive] public NexusModsCollectionData Data { get; private set; }
		[Reactive] public bool IsCardView { get; set; }

		public ObservableCollectionExtended<NexusModsCollectionModData> Mods { get; }

		[ObservableAsProperty] public string Title { get; }
		[ObservableAsProperty] public Visibility AuthorAvatarVisibility { get; }
		[ObservableAsProperty] public BitmapImage AuthorAvatar { get; }
		[ObservableAsProperty] public Visibility GridViewVisibility { get; }
		[ObservableAsProperty] public Visibility CardViewVisibility { get; }

		public ICommand SelectAllCommand { get; }
		public ICommand SetGridViewCommand { get; }
		public ICommand SetCardViewCommand { get; }
		public ICommand ConfirmCommand { get; set; }
		public ICommand CancelCommand { get; set; }

		public void Load(NexusGraphCollectionRevision collectionRevision)
		{
			Data = NexusModsCollectionData.FromCollectionRevision(collectionRevision);

			Mods.Clear();

			if (Data?.Mods?.Count > 0)
			{
				Mods.AddRange(Data.Mods.Items);
			}

			this.RaisePropertyChanged("Mods");
		}

		private static string ToTitleText(ValueTuple<string, string> x)
		{
			var text = x.Item1;
			var author = x.Item2;
			if (!String.IsNullOrEmpty(author))
			{
				text += " by " + author;
			}
			return text;
		}

		public CollectionDownloadWindowViewModel()
		{
			Mods = new ObservableCollectionExtended<NexusModsCollectionModData>();

			this.WhenAnyValue(x => x.Data.Name, x => x.Data.Author).Select(ToTitleText).ToUIProperty(this, x => x.Title);

			var whenAvatar = this.WhenAnyValue(x => x.Data.AuthorAvatarUrl);
			whenAvatar.Select(x => x != null ? Visibility.Visible : Visibility.Collapsed).ToUIProperty(this, x => x.AuthorAvatarVisibility);
			whenAvatar.Select(PropertyHelpers.UriToImage).ToUIProperty(this, x => x.AuthorAvatar);

			this.WhenAnyValue(x => x.IsCardView).Select(PropertyConverters.BoolToVisibilityReversed).ToUIProperty(this, x => x.GridViewVisibility, Visibility.Visible);
			this.WhenAnyValue(x => x.IsCardView).Select(PropertyConverters.BoolToVisibility).ToUIProperty(this, x => x.CardViewVisibility, Visibility.Collapsed);

			SelectAllCommand = ReactiveCommand.Create<bool>(b =>
			{
				foreach (var mod in Data.Mods.Items)
				{
					mod.IsSelected = b;
				}
			});

			SetGridViewCommand = ReactiveCommand.Create(() => IsCardView = false);
			SetCardViewCommand = ReactiveCommand.Create(() => IsCardView = true);
		}
	}

	public class CollectionDownloadWindowDesignViewModel : CollectionDownloadWindowViewModel
	{
		public CollectionDownloadWindowDesignViewModel() : base()
		{
			var mod1 = new NexusGraphMod()
			{
				PictureUrl = "https://staticdelivery.nexusmods.com/mods/3474/images/746/746-1691682009-457832810.png",
				CreatedAt = DateTimeOffset.Now,
				UpdatedAt = DateTimeOffset.Now,
				Version = "1.0.0",
				Category = "General"
			};
			var mod2 = new NexusGraphMod()
			{
				PictureUrl = "https://staticdelivery.nexusmods.com/mods/3474/images/832/832-1691856555-1641696071.png",
				CreatedAt = DateTimeOffset.Now,
				UpdatedAt = DateTimeOffset.Now,
				Version = "1.0.0",
				Category = "Spells"
			};
			var mod3 = new NexusGraphMod()
			{
				PictureUrl = "https://staticdelivery.nexusmods.com/mods/3474/images/691/691-1691538521-579579604.jpeg",
				CreatedAt = DateTimeOffset.Now,
				UpdatedAt = DateTimeOffset.Now,
				Version = "6.0",
				Category = "Classes"
			};
			var user = new NexusGraphUser()
			{
				Name = "LaughingLeader",
				Avatar = "https://avatars.nexusmods.com/8743560/100",
			};
			var designData = new NexusGraphCollectionRevision()
			{
				AdultContent = false,
				CreatedAt = DateTimeOffset.Now,
				UpdatedAt = DateTimeOffset.Now,
				Collection = new NexusGraphCollection()
				{
					Name = "Test Collection",
					Summary = "A collection of various mods",
					TileImage = new NexusGraphCollectionImage()
					{
						Url = "https://media.nexusmods.com/d/0/d01c8b3d-4849-457f-8754-71ce4ee27b8b.webp",
						ThumbnailUrl = "https://media.nexusmods.com/d/0/t/small/d01c8b3d-4849-457f-8754-71ce4ee27b8b.webp"
					},
					User = user
				},
				ModFiles = new NexusGraphCollectionRevisionMod[3] {
					new(){ Optional = true, File = new NexusGraphModFile(){ SizeInBytes = 2002, Name = "Better Bags", Description = "Mod desc", Owner = user, Mod = mod1}},
					new(){ File = new NexusGraphModFile(){ SizeInBytes = 2276, Name = "No concentration Shield Of Faith with 3 target AOE", Description = "No concentration Shield Of Faith", Owner = user, Mod = mod2}},
					new(){ File = new NexusGraphModFile(){ SizeInBytes = 1366, Name = "Paladin Unleashed - The Divine Warrior - Lay on Hands Restored and Auras Buffed", Description = "Channeling, smiting and laying on hands now that is what the unleashed divine Paladin does best! Don't worry, they are trained to be completely professional while carrying out their divine duties! In fact, they are ever watchful of her allies and ready to protect them from would be evildoers!", Owner = user, Mod = mod3}},
				}
			};
			RxApp.MainThreadScheduler.Schedule(() =>
			{
				Load(designData);
				DivinityApp.Log($"Mods: {Mods.Count} / {Data?.Mods?.Count}");
			});
		}
	}
}
