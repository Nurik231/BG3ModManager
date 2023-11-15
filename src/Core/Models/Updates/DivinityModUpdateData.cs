using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace DivinityModManager.Models.Updates
{
	public class DivinityModUpdateData : ReactiveObject, ISelectable
	{
		[Reactive] public DivinityModData Mod { get; set; }
		[Reactive] public ModDownloadData DownloadData { get; set; }
		[Reactive] public bool IsSelected { get; set; }
		[Reactive] public bool IsNewMod { get; set; }
		[Reactive] public bool CanDrag { get; set; }
		[Reactive] public Visibility Visibility { get; set; }

		[ObservableAsProperty] public ModSourceType Source { get; }
		[ObservableAsProperty] public bool IsEditorMod { get; }
		[ObservableAsProperty] public string Author { get; }
		[ObservableAsProperty] public string CurrentVersion { get; }
		[ObservableAsProperty] public string UpdateVersion { get; }
		[ObservableAsProperty] public string SourceText { get; }
		[ObservableAsProperty] public Uri UpdateLink { get; }
		[ObservableAsProperty] public string LocalFilePath { get; }
		[ObservableAsProperty] public string LocalFileDateText { get; }
		[ObservableAsProperty] public string UpdateFilePath { get; }
		[ObservableAsProperty] public string UpdateDateText { get; }

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

		private string DateToString(DateTime? date)
		{
			if(date.HasValue)
			{
				return date.Value.ToString(DivinityApp.DateTimeColumnFormat, CultureInfo.InstalledUICulture);
			}
			return "";
		}

		public DivinityModUpdateData()
		{
			CanDrag = true;
			Visibility = Visibility.Visible;

			this.WhenAnyValue(x => x.Mod.IsEditorMod).ToPropertyEx(this, x => x.IsEditorMod, true, RxApp.MainThreadScheduler);
			this.WhenAnyValue(x => x.Mod.Author).ToPropertyEx(this, x => x.Author, true, RxApp.MainThreadScheduler);
			this.WhenAnyValue(x => x.Mod.Version.Version).ToPropertyEx(this, x => x.CurrentVersion, true, RxApp.MainThreadScheduler);
			this.WhenAnyValue(x => x.Mod.FilePath).ToPropertyEx(this, x => x.LocalFilePath, true, RxApp.MainThreadScheduler);
			this.WhenAnyValue(x => x.Mod.LastModified).Select(DateToString).ToPropertyEx(this, x => x.LocalFileDateText, true, RxApp.MainThreadScheduler);

			var whenSource = this.WhenAnyValue(x => x.DownloadData.DownloadSourceType);
			whenSource.ToPropertyEx(this, x => x.Source, true, RxApp.MainThreadScheduler);
			whenSource.Select(x => x.GetDescription()).ToPropertyEx(this, x => x.SourceText, true, RxApp.MainThreadScheduler);

			this.WhenAnyValue(x => x.DownloadData.DownloadPath).ToPropertyEx(this, x => x.UpdateFilePath, true, RxApp.MainThreadScheduler);
			this.WhenAnyValue(x => x.DownloadData.Date).Select(DateToString).ToPropertyEx(this, x => x.UpdateDateText, true, RxApp.MainThreadScheduler);
			this.WhenAnyValue(x => x.DownloadData.Version).ToPropertyEx(this, x => x.UpdateVersion, true, RxApp.MainThreadScheduler);

			this.WhenAnyValue(x => x.Mod, x => x.Source).Select(SourceToLink).ToPropertyEx(this, x => x.UpdateLink, true, RxApp.MainThreadScheduler);
		}
	}
}
