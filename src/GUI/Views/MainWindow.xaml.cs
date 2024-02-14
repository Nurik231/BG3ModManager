using AdonisUI;
using AdonisUI.Controls;

using Alphaleonis.Win32.Filesystem;

using AutoUpdaterDotNET;

using DivinityModManager.Controls;
using DivinityModManager.Util;
using DivinityModManager.Util.ScreenReader;
using DivinityModManager.ViewModels;

using DynamicData;

using ReactiveUI;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace DivinityModManager.Views
{
	public partial class MainWindow : AdonisWindow, IViewFor<MainWindowViewModel>, INotifyPropertyChanged
	{
		private static MainWindow self;
		public static MainWindow Self => self;

		[DllImport("user32")] public static extern int FlashWindow(IntPtr hwnd, bool bInvert);

		public MainViewControl MainView { get; private set; }

		public SettingsWindow SettingsWindow { get; private set; }
		public ModPropertiesWindow ModPropertiesWindow { get; private set; }
		public AboutWindow AboutWindow { get; private set; }
		public VersionGeneratorWindow VersionGeneratorWindow { get; private set; }
		public AppUpdateWindow UpdateWindow { get; private set; }
		public HelpWindow HelpWindow { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;

		private MainWindowViewModel viewModel;
		public MainWindowViewModel ViewModel
		{
			get => viewModel;
			set
			{
				viewModel = value;
				// ViewModel is POCO type warning suppression
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ViewModel"));
			}
		}

		object IViewFor.ViewModel
		{
			get => ViewModel;
			set => ViewModel = (MainWindowViewModel)value;
		}

		private readonly System.Windows.Interop.WindowInteropHelper _hwnd;

		public LogTraceListener DebugLogListener { get; private set; }

		private readonly string _logsDir;
		private readonly string _logFileName;

		public AlertBar AlertBar => MainView.AlertBar;
		public Style MessageBoxStyle => MainView.MainWindowMessageBox_OK.Style;

		public void ToggleLogging(bool enabled)
		{
			if (enabled || ViewModel?.DebugMode == true)
			{
				if (DebugLogListener == null)
				{
					if (!Directory.Exists(_logsDir))
					{
						Directory.CreateDirectory(_logsDir);
						DivinityApp.Log($"Creating logs directory: {_logsDir}");
					}

					DebugLogListener = new LogTraceListener(_logFileName, "DebugLogListener");
					Trace.Listeners.Add(DebugLogListener);
					Trace.AutoFlush = true;
				}
			}
			else if (DebugLogListener != null && ViewModel?.DebugMode != true)
			{
				Trace.Listeners.Remove(DebugLogListener);
				DebugLogListener.Dispose();
				DebugLogListener = null;
				Trace.AutoFlush = false;
			}
		}

		public void DisplayError(string msg)
		{
			ToggleLogging(true);
			DivinityApp.Log(msg);
			var result = Xceed.Wpf.Toolkit.MessageBox.Show(msg,
				"Open the logs folder?",
				System.Windows.MessageBoxButton.YesNo,
				System.Windows.MessageBoxImage.Error,
				System.Windows.MessageBoxResult.No, MessageBoxStyle);
			if (result == System.Windows.MessageBoxResult.Yes)
			{
				DivinityFileUtils.TryOpenPath(DivinityApp.GetAppDirectory("_Logs"));
			}
		}

		public void DisplayError(string msg, string caption, bool showLog = false)
		{
			if(!showLog)
			{
				Xceed.Wpf.Toolkit.MessageBox.Show(msg, caption,
				System.Windows.MessageBoxButton.OK,
				System.Windows.MessageBoxImage.Warning,
				System.Windows.MessageBoxResult.OK, MessageBoxStyle);
			}
			else
			{
				DisplayError(msg);
			}
		}

		private void OnUIException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			e.Handled = true;
			ToggleLogging(true);
			var doShutdown = ViewModel?.IsInitialized != true;
			var shutdownText = doShutdown ? " The program will close." : "";
			DivinityApp.Log($"An exception in the UI occurred.{shutdownText}\n{e.Exception}");

			var result = Xceed.Wpf.Toolkit.MessageBox.Show($"An exception in the UI occurred.{shutdownText}\n{e.Exception}", 
				"Open the logs folder?",
				System.Windows.MessageBoxButton.YesNo,
				System.Windows.MessageBoxImage.Error,
				System.Windows.MessageBoxResult.No, MessageBoxStyle);
			if (result == System.Windows.MessageBoxResult.Yes)
			{
				DivinityFileUtils.TryOpenPath(DivinityApp.GetAppDirectory("_Logs"));
			}

			//Shutdown if we had an exception when loading.
			if (doShutdown)
			{
				App.Current.Shutdown(1);
			}
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			ToggleLogging(true);
			var doShutdown = ViewModel?.IsInitialized != true;
			var shutdownText = doShutdown ? " The program will close." : "";

			DivinityApp.Log($"An unhandled exception occurred.{shutdownText}\n{e.ExceptionObject}");
			var result = Xceed.Wpf.Toolkit.MessageBox.Show($"An unhandled exception occurred.{shutdownText}\n{e.ExceptionObject}",
				"Open the logs folder?",
				System.Windows.MessageBoxButton.YesNo,
				System.Windows.MessageBoxImage.Error,
				System.Windows.MessageBoxResult.No, MessageBoxStyle);
			if (result == System.Windows.MessageBoxResult.Yes)
			{
				DivinityFileUtils.TryOpenPath(DivinityApp.GetAppDirectory("_Logs"));
			}

			if(doShutdown)
			{
				App.Current.Shutdown(1);
			}
		}

		void OnStateChanged(object sender, EventArgs e)
		{
			if(ViewModel?.Settings?.Loaded == true)
			{
				var windowSettings = ViewModel.Settings.Window;
				windowSettings.Maximized = WindowState == WindowState.Maximized;
				var screen = System.Windows.Forms.Screen.FromHandle(_hwnd.Handle);
				windowSettings.Screen = System.Windows.Forms.Screen.AllScreens.IndexOf(screen);
				ViewModel.QueueSave();
			}
		}

		void OnLocationChanged(object sender, EventArgs e)
		{
			if (ViewModel?.Settings?.Loaded == true)
			{
				var windowSettings = ViewModel.Settings.Window;
				var screen = System.Windows.Forms.Screen.FromHandle(_hwnd.Handle);
				windowSettings.X = Left - screen.WorkingArea.Left;
				windowSettings.Y = Top - screen.WorkingArea.Top;
				windowSettings.Screen = System.Windows.Forms.Screen.AllScreens.IndexOf(screen);
				ViewModel.QueueSave();
			}
		}

		public void ToggleWindowPositionSaving(bool b)
		{
			if (b)
			{
				StateChanged += OnStateChanged;
				LocationChanged += OnLocationChanged;
			}
			else
			{
				StateChanged -= OnStateChanged;
				LocationChanged -= OnLocationChanged;
			}
		}

		public void OpenPreferences(bool switchToKeybindings = false, bool forceOpen = false)
		{
			if(SettingsWindow.ViewModel == null)
			{
				SettingsWindow.Init(ViewModel);
			}
			if (!SettingsWindow.IsVisible)
			{
				if (switchToKeybindings == true)
				{
					SettingsWindow.ViewModel.SelectedTabIndex = SettingsWindowTab.Keybindings;
				}
				SettingsWindow.Show();
				SettingsWindow.Owner = this;
				ViewModel.Settings.SettingsWindowIsOpen = true;
			}
			else if(!forceOpen)
			{
				SettingsWindow.Hide();
				ViewModel.Settings.SettingsWindowIsOpen = false;
			}
		}

		private void ToggleAboutWindow()
		{
			AboutWindow ??= new AboutWindow();

			if (!AboutWindow.IsVisible)
			{
				AboutWindow.DataContext = ViewModel;
				AboutWindow.Show();
				AboutWindow.Owner = this;
			}
			else
			{
				AboutWindow.Hide();
			}
		}

		public void ToggleUpdateWindow(bool visible, UpdateInfoEventArgs e = null)
		{
			UpdateWindow ??= new AppUpdateWindow();

			if (visible)
			{
				UpdateWindow.ViewModel.CheckArgs(e);
				if(!UpdateWindow.IsVisible)
				{
					UpdateWindow.Show();
					UpdateWindow.Owner = this;
				}
			}
			else if(UpdateWindow.IsVisible)
			{
				UpdateWindow.Hide();
			}
		}

		public void ShowHelpWindow(string title, string helpText)
		{
			HelpWindow ??= new HelpWindow();

			HelpWindow.ViewModel.HelpTitle = title;
			HelpWindow.ViewModel.HelpText = helpText;

			if (!HelpWindow.IsVisible)
			{
				HelpWindow.Show();
				HelpWindow.Owner = this;
			}
		}

		private static System.Windows.Shell.TaskbarItemProgressState BoolToTaskbarItemProgressState(bool b)
		{
			return b ? System.Windows.Shell.TaskbarItemProgressState.Normal : System.Windows.Shell.TaskbarItemProgressState.None;
		}

		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new CachedAutomationPeer(this);
		}

		public static readonly Uri LightTheme = new("pack://application:,,,/BG3ModManager;component/Themes/Light.xaml", UriKind.Absolute);
		public static readonly Uri DarkTheme = new("pack://application:,,,/BG3ModManager;component/Themes/Dark.xaml", UriKind.Absolute);

		public void UpdateColorTheme(bool darkMode)
		{
			ResourceLocator.SetColorScheme(this.Resources, !darkMode ? LightTheme : DarkTheme);
			ResourceLocator.SetColorScheme(SettingsWindow.Resources, !darkMode ? LightTheme : DarkTheme);
			if (AboutWindow != null)
			{
				ResourceLocator.SetColorScheme(AboutWindow.Resources, !darkMode ? LightTheme : DarkTheme);
			}
			if (VersionGeneratorWindow != null)
			{
				ResourceLocator.SetColorScheme(VersionGeneratorWindow.Resources, !darkMode ? LightTheme : DarkTheme);
			}
			if (UpdateWindow != null)
			{
				ResourceLocator.SetColorScheme(UpdateWindow.Resources, !darkMode ? LightTheme : DarkTheme);
			}
			if (HelpWindow != null)
			{
				ResourceLocator.SetColorScheme(HelpWindow.Resources, !darkMode ? LightTheme : DarkTheme);
			}
		}

		private void OnClosing()
		{
			ViewModel.SaveSettings();
			Application.Current.Shutdown();
		}

		private void AutoUpdater_OnClosing()
		{
			ViewModel.Settings.LastUpdateCheck = DateTimeOffset.Now.ToUnixTimeSeconds();
			OnClosing();
		}

		private WindowInteropHelper _wih;

		public void FlashTaskbar()
		{
			FlashWindow(_wih.Handle, true);
		}

		public MainWindow()
		{
			InitializeComponent();
			self = this;

			_hwnd = new System.Windows.Interop.WindowInteropHelper(this);

			_logsDir = DivinityApp.GetAppDirectory("_Logs");
			var sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/", "-");
#if DEBUG
			_logFileName = Path.Combine(_logsDir, "debug_" + DateTime.Now.ToString(sysFormat + "_HH-mm-ss") + ".log");
#else
			_logFileName = Path.Combine(_logsDir, "release_" + DateTime.Now.ToString(sysFormat + "_HH-mm-ss") + ".log");
#endif

			Application.Current.DispatcherUnhandledException += OnUIException;
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			DivinityApp.DateTimeColumnFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
			DivinityApp.DateTimeTooltipFormat = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;

			RxExceptionHandler.view = this;

			ViewModel = new MainWindowViewModel();
			MainView = new MainViewControl(this, ViewModel);
			MainGrid.Children.Add(MainView);

			SettingsWindow = new SettingsWindow();
			ModPropertiesWindow = new ModPropertiesWindow();
			ModPropertiesWindow.Hide();
			//TODO Replace with new updater service
			/*SettingsWindow.OnWorkshopPathChanged += delegate
			{
				RxApp.TaskpoolScheduler.ScheduleAsync(TimeSpan.FromMilliseconds(50), async (sch, cts) => await ViewModel.LoadWorkshopModsAsync(cts));
			};*/
			SettingsWindow.Closed += delegate
			{
				if (ViewModel?.Settings != null)
				{
					ViewModel.Settings.SettingsWindowIsOpen = false;
				}
			};
			SettingsWindow.Hide();

			if (File.Exists(Alphaleonis.Win32.Filesystem.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "debug")))
			{
				ViewModel.DebugMode = true;
				ToggleLogging(true);
				DivinityApp.Log("Enable logging due to the debug file next to the exe.");
			}

			this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;

			Closed += (o,e) => OnClosing();
			AutoUpdater.ApplicationExitEvent += AutoUpdater_OnClosing;
			AutoUpdater.HttpUserAgent = "DivinityModManagerUser";
			AutoUpdater.RunUpdateAsAdmin = false;

			DataContext = ViewModel;

			_wih = new WindowInteropHelper(this);

			DivinityInteractions.OpenModProperties.RegisterHandler(interaction =>
			{
				ModPropertiesWindow.ViewModel.SetMod(interaction.Input);
				if(!ModPropertiesWindow.IsVisible)
				{
					ModPropertiesWindow.Show();
					ModPropertiesWindow.Owner = this;
				}
				interaction.SetOutput(true);
			});

			this.WhenActivated(d =>
			{
				ViewModel.OnViewActivated(this, MainView);
				this.WhenAnyValue(x => x.ViewModel.Title).BindTo(this, view => view.Title);
				this.OneWayBind(ViewModel, vm => vm.MainProgressIsActive, view => view.TaskbarItemInfo.ProgressState, BoolToTaskbarItemProgressState);

				ViewModel.Keys.OpenPreferences.AddAction(() => OpenPreferences(false));
				ViewModel.Keys.OpenKeybindings.AddAction(() => OpenPreferences(true));
				ViewModel.Keys.OpenAboutWindow.AddAction(ToggleAboutWindow);

				ViewModel.Keys.ToggleVersionGeneratorWindow.AddAction(() =>
				{
					VersionGeneratorWindow ??= new VersionGeneratorWindow();

					if (!VersionGeneratorWindow.IsVisible)
					{
						VersionGeneratorWindow.Show();
						VersionGeneratorWindow.Owner = this;
					}
					else
					{
						VersionGeneratorWindow.Hide();
					}
				});

				this.WhenAnyValue(x => x.ViewModel.MainProgressValue).BindTo(this, view => view.TaskbarItemInfo.ProgressValue);

				MainView.OnActivated();

				//Allow launching the game if single instance mode is enabled, but the shift key is held
				Observable.Merge(
					Observable.FromEventPattern<KeyEventArgs>(this, nameof(KeyDown)),
					Observable.FromEventPattern<KeyEventArgs>(this, nameof(KeyUp))
				)
				.Select(e => (e.EventArgs.Key == Key.LeftShift || e.EventArgs.Key == Key.RightShift) && e.EventArgs.IsDown)
				.BindTo(ViewModel, x => x.CanForceLaunchGame);
			});
		}
	}
}
