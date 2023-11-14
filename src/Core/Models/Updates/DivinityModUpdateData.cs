using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace DivinityModManager.Models.Updates
{
	public class DivinityModUpdateData : ReactiveObject, ISelectable
	{
		[Reactive] public DivinityModData LocalMod { get; set; }
		[Reactive] public DivinityModData UpdatedMod { get; set; }
		[Reactive] public bool IsSelected { get; set; }
		[Reactive] public bool IsNewMod { get; set; }
		[Reactive] public bool CanDrag { get; set; }
		[Reactive] public Visibility Visibility { get; set; }
		[Reactive] public ModSourceType Source { get; set; }

		[Reactive] public DivinityModData PrimaryModData { get; private set; }

		[ObservableAsProperty] public bool IsEditorMod { get; }
		[ObservableAsProperty] public string Author { get; }
		[ObservableAsProperty] public string CurrentVersion { get; }
		[ObservableAsProperty] public string UpdateVersion { get; }
		[ObservableAsProperty] public string SourceText { get; }
		[ObservableAsProperty] public Uri UpdateLink { get; }
		[ObservableAsProperty] public string LocalFilePath { get; }
		[ObservableAsProperty] public string UpdateFilePath { get; }
		[ObservableAsProperty] public DateTime? LastModified { get; }

		private DivinityModData GetNonNull(ValueTuple<DivinityModData, DivinityModData> items)
		{
			return items.Item1 ?? items.Item2;
		}

		private Uri SourceToLink(ValueTuple<DivinityModData, ModSourceType> data)
		{
			if(data.Item1 != null)
			{
				var url = data.Item1.GetURL(data.Item2);
				if(!String.IsNullOrEmpty(url))
				{
					return new Uri(url);
				}
			}
			return null;
		}

		public DivinityModUpdateData()
		{
			Source = ModSourceType.NONE;
			CanDrag = true;
			Visibility = Visibility.Visible;

			//Get whichever mod data isn't null, prioritizing LocalMod
			this.WhenAnyValue(x => x.LocalMod, x => x.UpdatedMod).Select(GetNonNull).BindTo(this, x => x.PrimaryModData);

			this.WhenAnyValue(x => x.Source).Select(x => x.GetDescription()).ToPropertyEx(this, x => x.SourceText, true, RxApp.MainThreadScheduler);
			this.WhenAnyValue(x => x.UpdatedMod, x => x.Source).Select(SourceToLink).ToPropertyEx(this, x => x.UpdateLink, true, RxApp.MainThreadScheduler);

			this.WhenAnyValue(x => x.PrimaryModData.IsEditorMod).ToPropertyEx(this, x => x.IsEditorMod, true, RxApp.MainThreadScheduler);
			this.WhenAnyValue(x => x.PrimaryModData.Author).ToPropertyEx(this, x => x.Author, true, RxApp.MainThreadScheduler);
			this.WhenAnyValue(x => x.PrimaryModData.Version.Version).ToPropertyEx(this, x => x.CurrentVersion, true, RxApp.MainThreadScheduler);
			this.WhenAnyValue(x => x.PrimaryModData.FilePath).ToPropertyEx(this, x => x.LocalFilePath, true, RxApp.MainThreadScheduler);

			this.WhenAnyValue(x => x.UpdatedMod.LastModified).ToPropertyEx(this, x => x.LastModified, true, RxApp.MainThreadScheduler);
			this.WhenAnyValue(x => x.UpdatedMod.Version.Version).ToPropertyEx(this, x => x.UpdateVersion, true, RxApp.MainThreadScheduler);

			this.WhenAnyValue(x => x.UpdatedMod.FilePath).ToPropertyEx(this, x => x.UpdateFilePath, true, RxApp.MainThreadScheduler);
		}
	}
}
