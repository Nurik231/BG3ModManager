using System.Globalization;
using System.Windows;

namespace DivinityModManager.Util
{
	public static class PropertyConverters
	{
		public static Visibility BoolToVisibility(bool b) => b ? Visibility.Visible : Visibility.Collapsed;
		public static Visibility BoolToVisibilityReversed(bool b) => !b ? Visibility.Visible : Visibility.Collapsed;
		public static Visibility BoolTupleToVisibility(ValueTuple<bool, bool, bool, bool, bool> b) => b.Item1 || b.Item2 || b.Item3 || b.Item4 || b.Item5 ? Visibility.Visible : Visibility.Collapsed;
		/// <summary>
		/// Visible if not null or empty, otherwise collapsed.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static Visibility StringToVisibility(string str) => !String.IsNullOrEmpty(str) ? Visibility.Visible : Visibility.Collapsed;
		public static Visibility UriToVisibility(Uri uri) => !String.IsNullOrEmpty(uri?.ToString()) ? Visibility.Visible : Visibility.Collapsed;
		public static bool StringToBool(string str) => !String.IsNullOrEmpty(str);
		public static string DateToString(DateTimeOffset date) => date.ToString(DivinityApp.DateTimeColumnFormat, CultureInfo.InstalledUICulture);
	}
}
