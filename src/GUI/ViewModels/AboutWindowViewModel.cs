using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.ViewModels
{
	public class AboutWindowViewModel : ReactiveObject
	{
		[Reactive] public string Title { get; set; }

		public AboutWindowViewModel()
		{
			Title = "About";
		}
	}
}
