using DivinityModManager.Models;

using LSLib.LS;
using LSLib.LS.Stats;

using System.IO;

namespace DivinityModManager.Util;

public static class ModUtils
{
	private static void LoadModStats(Dictionary<string, ModInfo> mods, VFS vfs, StatLoader loader, string folderName)
	{
		if (mods.TryGetValue(folderName, out var mod))
		{
			foreach (var file in mod.Stats)
			{
				using var statStream = vfs.Open(file);
				loader.LoadStatsFromStream(file, statStream);
			}
		}
	}

	public static void ValidateStats(IEnumerable<DivinityModData> mods, string gameDataPath)
	{
		var context = new StatLoadingContext();
		var loader = new StatLoader(context);

		var vfs = new VFS();
		vfs.AttachGameDirectory(gameDataPath, true);
		foreach (var mod in mods)
		{
			if (!mod.IsEditorMod && File.Exists(mod.FilePath))
			{
				vfs.AttachPackage(mod.FilePath);
			}
		}
		vfs.FinishBuild();

		var modResources = new ModResources();
		var modHelper = new ModPathVisitor(modResources, vfs)
		{
			Game = DivinityApp.GAME_COMPILER,
			CollectGlobals = false,
			CollectLevels = false,
			CollectStoryGoals = false,
			CollectStats = true,
			CollectGuidResources = false,
		};

		modHelper.Discover();

		var definitions = new StatDefinitionRepository();
		try
		{
			if (modResources.Mods.TryGetValue("Shared", out var shared))
			{
				definitions.LoadDefinitions(vfs.Open(shared.ValueListsFile));
				definitions.LoadDefinitions(vfs.Open(shared.ModifiersFile));
			}
			else
			{
				throw new Exception("The 'Shared' base mod appears to be missing. This is not normal.");
			}
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error loading definitions:\n{ex}");
		}

		context.Definitions = definitions;

		var dependencies = mods.SelectMany(x => x.Dependencies.Items.Select(x => x.Folder)).Distinct();
		foreach (var dependency in dependencies)
		{
			LoadModStats(modResources.Mods, vfs, loader, dependency);
		}

		loader.ResolveUsageRef();
		loader.ValidateEntries();

		context.Errors.Clear();
	}
}
