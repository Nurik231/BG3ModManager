using Alphaleonis.Win32.Filesystem;

using DivinityModManager.AppServices;
using DivinityModManager.Util;
using DivinityModManager.Views;

using ReactiveUI;

using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace DivinityModManager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public SplashScreen Splash { get; set; }

		public App()
		{
			Directory.SetCurrentDirectory(DivinityApp.GetAppDirectory());
			// Fix for loading C++ dlls from _Lib
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			var assembly = Assembly.GetExecutingAssembly();
			var appName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute), false)).Product;
			var version = assembly.GetName().Version.ToString();

			var productName = Regex.Replace(appName.Trim(), @"\s+", String.Empty);
			Services.RegisterSingleton<IFileWatcherService>(new FileWatcherService());
			Services.RegisterSingleton<IGitHubService>(new GitHubService(productName, version));
			Services.RegisterSingleton<INexusModsService>(new NexusModsService(productName, version));
			Services.RegisterSingleton<IModUpdaterService>(new ModUpdaterService(version));
			Services.RegisterSingleton(new BackgroundCommandService());

			// POCO type warning suppression
			Services.Register<ICreatesObservableForProperty>(() => new DivinityModManager.Util.CustomPropertyResolver());

			WebHelper.SetupClient();
#if DEBUG
			RxApp.SuppressViewCommandBindingMessage = false;
#else
			RxApp.DefaultExceptionHandler = new RxExceptionHandler();
			RxApp.SuppressViewCommandBindingMessage = true;
#endif
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			//For making date display use the current system's culture
			FrameworkElement.LanguageProperty.OverrideMetadata(
				typeof(FrameworkElement),
				new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

			EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown));

			var splashFade = new System.Threading.Thread(() =>
			{
				Splash.Close(TimeSpan.FromSeconds(1));
			});

			var mainWindow = new MainWindow();
			splashFade.Start();
			mainWindow.Show();
		}

		private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var assyName = new AssemblyName(args.Name);

			var newPath = Path.Combine("_Lib", assyName.Name);
			if (!newPath.EndsWith(".dll"))
			{
				newPath += ".dll";
			}

			if (File.Exists(newPath))
			{
				var assy = Assembly.LoadFile(newPath);
				return assy;
			}
			return null;
		}

		private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			DivinityApp.IsKeyboardNavigating = false;
		}
	}
}
