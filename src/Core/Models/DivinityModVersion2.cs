using Newtonsoft.Json;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Reactive.Linq;

namespace DivinityModManager.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class DivinityModVersion2 : ReactiveObject
	{
		[Reactive] public ulong Major { get; set; }
		[Reactive] public ulong Minor { get; set; }
		[Reactive] public ulong Revision { get; set; }
		[Reactive] public ulong Build { get; set; }

		[JsonProperty][Reactive] public string Version { get; set; }

		private ulong versionInt = 0;

		[JsonProperty]
		public ulong VersionInt
		{
			get { return versionInt; }
			set => ParseInt(value);
		}

		private void UpdateVersion()
		{
			Version = $"{Major}.{Minor}.{Revision}.{Build}";
		}

		public ulong ToInt()
		{
			return (Major << 55) + (Minor << 47) + (Revision << 31) + Build;
		}

		public override string ToString()
		{
			return String.Format("{0}.{1}.{2}.{3}", Major, Minor, Revision, Build);
		}

		public void ParseInt(ulong nextVersionInt)
		{
			nextVersionInt = Math.Max(ulong.MinValue, Math.Min(nextVersionInt, ulong.MaxValue));
			if (versionInt != nextVersionInt)
			{
				versionInt = nextVersionInt;
				if (versionInt != 0)
				{
					Major = versionInt >> 55;
					Minor = (versionInt >> 47) & 0xFF;
					Revision = (versionInt >> 31) & 0xFFFF;
					Build = versionInt & 0x7FFFFFFFUL;
				}
				else
				{
					Major = Minor = Revision = Build = 0;
				}
				this.RaisePropertyChanged("VersionInt");
			}
		}

		public void ParseString(string nextVersion)
		{
			var values = nextVersion.Split('.');
			if (values.Length > 0)
			{
				if (ulong.TryParse(values[0], out var major)) Major = major;
				if (values.Length > 1 && ulong.TryParse(values[1], out var minor)) Minor = minor;
				if (values.Length > 2 && ulong.TryParse(values[2], out var revision)) Revision = revision;
				if (values.Length > 3 && ulong.TryParse(values[3], out var build)) Build = build;
				versionInt = ToInt();
				this.RaisePropertyChanged("VersionInt");
			}
		}

		public static DivinityModVersion2 FromInt(ulong vInt)
		{
			if (vInt == 1 || vInt == 268435456)
			{
				// 1.0.0.0
				vInt = 36028797018963968;
			}
			return new DivinityModVersion2(vInt);
		}

		public static bool operator >(DivinityModVersion2 a, DivinityModVersion2 b)
		{
			return a.VersionInt > b.VersionInt;
		}

		public static bool operator <(DivinityModVersion2 a, DivinityModVersion2 b)
		{
			return a.VersionInt < b.VersionInt;
		}

		public static bool operator >=(DivinityModVersion2 a, DivinityModVersion2 b)
		{
			return a.VersionInt >= b.VersionInt;
		}

		public static bool operator <=(DivinityModVersion2 a, DivinityModVersion2 b)
		{
			return a.VersionInt <= b.VersionInt;
		}

		public static bool operator >(DivinityModVersion2 a, string b)
		{
			return a.VersionInt > new DivinityModVersion2(b).VersionInt;
		}

		public static bool operator <(DivinityModVersion2 a, string b)
		{
			return a.VersionInt < new DivinityModVersion2(b).VersionInt;
		}

		public static bool operator >=(DivinityModVersion2 a, string b)
		{
			return a.VersionInt >= new DivinityModVersion2(b).VersionInt;
		}

		public static bool operator <=(DivinityModVersion2 a, string b)
		{
			return a.VersionInt <= new DivinityModVersion2(b).VersionInt;
		}

		public DivinityModVersion2()
		{
			this.WhenAnyValue(x => x.VersionInt).Subscribe((x) =>
			{
				UpdateVersion();
			});
		}

		public DivinityModVersion2(ulong vInt) : this()
		{
			ParseInt(vInt);
		}

		public DivinityModVersion2(string versionStr) : this()
		{
			ParseString(versionStr);
		}

		public DivinityModVersion2(ulong headerMajor, ulong headerMinor, ulong headerRevision, ulong headerBuild) : this()
		{
			Major = headerMajor;
			Minor = headerMinor;
			Revision = headerRevision;
			Build = headerBuild;
			versionInt = ToInt();
			UpdateVersion();
		}

		public static readonly DivinityModVersion2 Empty = new DivinityModVersion2(0);
	}
}
