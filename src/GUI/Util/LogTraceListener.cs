using Alphaleonis.Win32.Filesystem;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Util
{
    public class LogTraceListener : TextWriterTraceListener
	{
		public LogTraceListener(string fileName, string name) : base(fileName, name)
		{
			
		}

		public override void Write(string message)
		{
			base.Write(StringUtils.ReplaceSpecialPathways(message));
		}

		public override void WriteLine(string message)
		{
			base.WriteLine(StringUtils.ReplaceSpecialPathways(message));
		}
	}
}
