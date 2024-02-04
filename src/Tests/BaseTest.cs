using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit.Abstractions;

namespace DivinityModManager.Tests
{
	public abstract class BaseTest : IDisposable
	{
		private readonly ITestOutputHelper _output;

		public ITestOutputHelper Output => _output;

		public BaseTest(ITestOutputHelper output)
		{
			_output = output;
			DivinityApp.LogMethod = _output.WriteLine;
			Console.SetOut(new ConsoleTextRedirecter(_output));
		}

		public virtual void Dispose()
		{
			DivinityApp.LogMethod = null;
		}
	}
}
