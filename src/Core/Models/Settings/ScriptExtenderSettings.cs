using DivinityModManager.Extensions;
using Newtonsoft.Json;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Models.Settings
{
	[DataContract]
	public class ScriptExtenderSettings : ReactiveObject
	{
		[Reactive] public bool ExtenderIsAvailable { get; set; }
		[Reactive] public bool ExtenderUpdaterIsAvailable { get; set; }
		[Reactive] public string ExtenderVersion { get; set; }
		[Reactive] public int ExtenderMajorVersion { get; set; }

		[SettingsEntry("Export Default Values", "Export all values, even if it matches a default extender value")]
		[Reactive]
		[DataMember]
		[JsonIgnore] // This isn't an actual extender setting, so omit it from the exported json
		[DefaultValue(false)]
		public bool ExportDefaultExtenderSettings { get; set; }

		[SettingsEntry("Enable Developer Mode", "Enables various debug functionality for development purposes\nThis can be checked by mods to enable additional log messages and more")]
		[Reactive]
		[DataMember]
		[DefaultValue(false)]
		public bool DeveloperMode { get; set; }

		[SettingsEntry("Custom Profile", "Use a profile other than Public\nThis should be the profile folder name")]
		[Reactive]
		[DataMember]
		[DefaultValue("")]
		public string CustomProfile { get; set; }

		[SettingsEntry("Create Console", "Creates a console window that logs extender internals\nMainly useful for debugging")]
		[Reactive]
		[DataMember]
		[DefaultValue(false)]
		public bool CreateConsole { get; set; }

		[SettingsEntry("Log Working Story Errors", "Log errors during Osiris story compilation to a log file (LogFailedCompile)")]
		[Reactive]
		[DataMember]
		[DefaultValue(true)]
		public bool LogFailedCompile { get; set; }

		[SettingsEntry("Enable Osiris Logging", "Enable logging of Osiris activity (rule evaluation, queries, etc.) to a log file")]
		[Reactive]
		[DataMember]
		[DefaultValue(false)]
		public bool EnableLogging { get; set; }

		[SettingsEntry("Log Script Compilation", "Log Osiris story compilation to a log file")]
		[Reactive]
		[DataMember]
		[DefaultValue(false)]
		public bool LogCompile { get; set; }

		[SettingsEntry("Log Directory", "Directory where the generated Osiris logs will be stored\nDefault is Documents\\OsirisLogs")]
		[Reactive]
		[DataMember]
		[DefaultValue("")]
		public string LogDirectory { get; set; }

		[SettingsEntry("Log Runtime", "Log extender console and script output to a log file")]
		[Reactive]
		[DataMember]
		[DefaultValue(false)]
		public bool LogRuntime { get; set; }

		[SettingsEntry("Disable Launcher", "Prevents the exe from force-opening the launcher\nMay not work correctly if extender auto-updating is enabled, or the --skip-launcher launch param is set", true)]
		[Reactive]
		[DataMember]
		[DefaultValue(false)]
		public bool DisableLauncher { get; set; }

		[SettingsEntry("Disable Story Merge", "Prevents story.div.osi merging, which automatically happens when mods are present\nMay only occur when loading a save", true)]
		[Reactive]
		[DataMember]
		[DefaultValue(true)]
		public bool DisableStoryMerge { get; set; }

		[SettingsEntry("Disable Story Patching", "Prevents patching story.bin with story.div.osi when loading saves, effectively preventing the Osiris scripts in the save from updating", true)]
		[Reactive]
		[DataMember]
		[DefaultValue(false)]
		public bool DisableStoryPatching { get; set; }

		[SettingsEntry("Disable Mod Validation", "Disable module hashing when loading mods\nSpeeds up mod loading with no drawbacks")]
		[Reactive]
		[DataMember]
		[DefaultValue(true)]
		public bool DisableModValidation { get; set; }

		[SettingsEntry("Enable Achievements", "Re-enable achievements for modded games")]
		[Reactive]
		[DataMember]
		[DefaultValue(true)]
		public bool EnableAchievements { get; set; }

		[SettingsEntry("Enable Extensions", "Enables or disables extender API functionality", true)]
		[Reactive]
		[DataMember]
		[DefaultValue(true)]
		public bool EnableExtensions { get; set; }

		[SettingsEntry("Send Crash Reports", "Upload minidumps to the crash report collection server after a game crash")]
		[Reactive]
		[DataMember]
		[DefaultValue(true)]
		public bool SendCrashReports { get; set; }

		[SettingsEntry("Enable Osiris Debugger", "Enables the Osiris debugger interface (vscode extension)", true)]
		[Reactive]
		[DataMember]
		[DefaultValue(false)]
		public bool EnableDebugger { get; set; }

		[SettingsEntry("Osiris Debugger Port", "Port number the Osiris debugger will listen on\nDefault: 9999", true)]
		[Reactive]
		[DataMember]
		[DefaultValue(9999)]
		public int DebuggerPort { get; set; }

		[SettingsEntry("Dump Network Strings", "Dumps the NetworkFixedString table to LogDirectory\nMainly useful for debugging desync issues", true)]
		[Reactive]
		[DataMember]
		[DefaultValue(false)]
		public bool DumpNetworkStrings { get; set; }

		[SettingsEntry("Osiris Debugger Flags", "Debugger flags to set\nDefault: 0")]
		[Reactive]
		[DataMember]
		[DefaultValue(0)]
		public int DebuggerFlags { get; set; }

		[SettingsEntry("Enable Lua Debugger", "Enables the Lua debugger interface (vscode extension)", true)]
		[Reactive]
		[DataMember]
		[DefaultValue(false)]
		public bool EnableLuaDebugger { get; set; }

		[SettingsEntry("Lua Builtin Directory", "An additional directory where the Script Extender will check for builtin scripts\nThis setting is meant for developers, to make it easier to test builtin script changes", true)]
		[Reactive]
		[DataMember]
		[DefaultValue("")]
		public string LuaBuiltinResourceDirectory { get; set; }

		[SettingsEntry("Clear Console On Reset", "Clears the extender console when the reset command is used", true)]
		[Reactive]
		[DataMember]
		[DefaultValue(true)]
		public bool ClearOnReset { get; set; }

		[SettingsEntry("Default to Client Side", "Defaults the extender console to the client-side\nThis is setting is intended for developers", true)]
		[Reactive]
		[DataMember]
		[DefaultValue(false)]
		public bool DefaultToClientConsole { get; set; }

		[SettingsEntry("Show Performance Warnings", "Print warnings to the extender console window, which indicates when the server-side part of the game lags behind (a.k.a. warnings about ticks taking too long).", true)]
		[Reactive]
		[DataMember]
		[DefaultValue(false)]
		public bool ShowPerfWarnings { get; set; }

		public ScriptExtenderSettings()
		{
			this.SetToDefault();
			ExtenderVersion = string.Empty;
			ExtenderMajorVersion = -1;
		}
	}
}
