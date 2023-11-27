using Alphaleonis.Win32.Filesystem;

using DivinityModManager.Util;

using DynamicData;
using DynamicData.Binding;

using LSLib.LS;

using Newtonsoft.Json;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Input;

namespace DivinityModManager.Models
{
	public interface IDivinityModData
	{
		string UUID { get; set; }
		string Name { get; set; }
		string Folder { get; set; }
		string MD5 { get; set; }
		DivinityModVersion2 Version { get; set; }
	}

	[DataContract]
	public class DivinityBaseModData : ReactiveObject, IDivinityModData
	{
		[Reactive] public string FilePath { get; set; }
		[Reactive][DataMember] public string UUID { get; set; }
		[Reactive][DataMember] public string Folder { get; set; }
		[Reactive][DataMember] public string Name { get; set; }
		[Reactive][DataMember] public string Description { get; set; }
		[Reactive][DataMember] public string Author { get; set; }
		[Reactive] public string MD5 { get; set; }
		[Reactive][DataMember] public DivinityModVersion2 Version { get; set; }
		[Reactive] public DivinityModVersion2 HeaderVersion { get; set; }
		[Reactive] public DivinityModVersion2 PublishVersion { get; set; }
		[Reactive] public DateTime? LastModified { get; set; }

		[Reactive] public bool DisplayFileForName { get; set; }
		[Reactive] public bool IsHidden { get; set; }

		/// <summary>True if this mod is in DivinityApp.IgnoredMods, or the author is Larian. Larian mods are hidden from the load order.</summary>
		[Reactive] public bool IsLarianMod { get; set; }

		/// <summary>Whether the mod was loaded from the user's mods directory.</summary>
		[Reactive] public bool IsUserMod { get; set; }

		/// <summary>
		/// True if the mod has a meta.lsx.
		/// </summary>
		[Reactive] public bool HasMetadata { get; set; }

		/// <summary>True if the mod has a base game mod directory. This data is always loaded regardless if the mod is enabled or not.</summary>
		[Reactive] public bool IsForceLoaded { get; set; }
		/// <summary>
		/// Whether the mod has files of its own (i.e. it overrides Gustav, but it has Public/ModFolder/Assets files etc).
		/// </summary>
		[Reactive] public bool IsForceLoadedMergedMod { get; set; }

		/// <summary>
		/// For situations where an override pak has a meta.lsx with no original files, but it needs to be allowed in the load order anyway.
		/// </summary>
		[Reactive] public bool ForceAllowInLoadOrder { get; set; }
		[Reactive] public string BuiltinOverrideModsText { get; set; }

		[Reactive] public string HelpText { get; set; }

		public List<string> Tags { get; set; } = new List<string>();

		[Reactive] public Visibility Visibility { get; set; }

		[Reactive] public string FileName { get; private set; }
		[ObservableAsProperty] public string DisplayName { get; }
		[ObservableAsProperty] public Visibility DescriptionVisibility { get; }
		[ObservableAsProperty] public Visibility AuthorVisibility { get; }

		public virtual string GetDisplayName()
		{
			return !DisplayFileForName ? Name : FileName;
		}

		public virtual string GetHelpText()
		{
			return "";
		}

		public void AddTag(string tag)
		{
			if (!String.IsNullOrWhiteSpace(tag) && !Tags.Contains(tag))
			{
				Tags.Add(tag);
				Tags.Sort((x, y) => string.Compare(x, y, true));
			}
		}

		public void AddTags(IEnumerable<string> tags)
		{
			if (tags == null)
			{
				return;
			}
			bool addedTags = false;
			foreach (var tag in tags)
			{
				if (!String.IsNullOrWhiteSpace(tag) && !Tags.Contains(tag))
				{
					Tags.Add(tag);
					addedTags = true;
				}
			}
			Tags.Sort((x, y) => string.Compare(x, y, true));
			if (addedTags)
			{
				this.RaisePropertyChanged("Tags");
			}
		}

		public bool PakEquals(string fileName, StringComparison comparison = StringComparison.Ordinal)
		{
			string outputPackage = Path.ChangeExtension(Folder, "pak");
			//Imported Classic Projects
			if (!Folder.Contains(UUID))
			{
				outputPackage = Path.ChangeExtension(Path.Combine(Folder + "_" + UUID), "pak");
			}
			return outputPackage.Equals(fileName, comparison);
		}

		public bool IsNewerThan(DateTime date)
		{
			if (LastModified.HasValue)
			{
				return LastModified.Value > date;
			}
			return false;
		}

		public bool IsNewerThan(DivinityBaseModData mod)
		{
			if (LastModified.HasValue && mod.LastModified.HasValue)
			{
				return LastModified.Value > mod.LastModified.Value;
			}
			return false;
		}

		public DivinityBaseModData()
		{
			Version = DivinityModVersion2.Empty;
			HeaderVersion = DivinityModVersion2.Empty;
			PublishVersion = DivinityModVersion2.Empty;
			MD5 = "";
			Author = "";
			Folder = "";
			UUID = "";
			Name = "";

			HelpText = "";

			this.WhenAnyValue(x => x.FilePath).Select(f => Path.GetFileName(f)).BindTo(this, x => x.FileName);
			this.WhenAnyValue(x => x.Name, x => x.FilePath, x => x.DisplayFileForName).Select(x => this.GetDisplayName()).ToUIProperty(this, x => x.DisplayName);
			this.WhenAnyValue(x => x.Description).Select(PropertyConverters.StringToVisibility).ToUIProperty(this, x => x.DescriptionVisibility);
			this.WhenAnyValue(x => x.Author).Select(PropertyConverters.StringToVisibility).ToUIProperty(this, x => x.AuthorVisibility);
		}
	}
}
