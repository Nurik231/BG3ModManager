using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Humanizer;

namespace DivinityModManager.Converters
{
	public class TimeSpanToReadableStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(value is TimeSpan time)
			{
				if(time <= TimeSpan.Zero)
				{
					return "0 minutes (Disabled)";
				}
				return time.Humanize(2);
			}
			return String.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return TimeSpan.Zero;
		}
	}
}
