using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// One-click generation of prefabs, scenes and build settings, plus batch-mode entry points.
    /// </summary>
    public static class GameBuildPipeline
    {
        private const string MenuRoot = "The Last Guardian/";
        private const string BuildOutputArgument = "-buildOutput";

        [MenuItem(MenuRoot + "Build Everything", priority = 0)]
        public static void BuildEverything()
        {
            AssetDatabase.Refresh();
            ConfigureProjectSettings();

            PrefabBuilder.BuildAll();
            ArtLibraryBuilder.Build();

            List<string> levelScenes = LevelSceneBuilder.BuildAll();
            string menuScene = MainMenuSceneBuilder.Build();

            ConfigureBuildSettings(menuScene, levelScenes);
            AssetDatabase.SaveAssets();

            EditorSceneManager.OpenScene(menuScene);
            Debug.Log($"PITER_BUILD_READY: main menu + {levelScenes.Count} levels registered in Build Settings.");
        }

        [MenuItem(MenuRoot + "Reimport Game Sprites", priority = 20)]
        public static void ReimportSprites()
        {
            AssetDatabase.ImportAsset(GameAssetPaths.ArtRoot.TrimEnd('/'), ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        }

        [MenuItem(MenuRoot + "Delete Save Data", priority = 40)]
        public static void DeleteSaveData()
        {
            SaveSystem.ResetProgress();
            Debug.Log("Save data deleted: " + JsonFileStore.GetPath(SaveSystem.FileName));
        }

        [MenuItem(MenuRoot + "Unlock All Stages", priority = 41)]
        public static void UnlockAllStages()
        {
            SaveSystem.Current.UnlockAllLevels(LevelCatalog.Count);
            SaveSystem.Save();
            Debug.Log("All stages unlocked.");
        }

        public static void BatchBuildEverything()
        {
            RunBatch(BuildEverything);
        }

        public static void BatchBuildWindowsPlayer()
        {
            RunBatch(() =>
            {
                string output = ReadArgument(BuildOutputArgument) ?? "Builds/Windows/Piter.exe";
                BuildPlayerOptions options = new BuildPlayerOptions
                {
                    scenes = Array.ConvertAll(EditorBuildSettings.scenes, scene => scene.path),
                    locationPathName = output,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.None
                };

                BuildReport report = BuildPipeline.BuildPlayer(options);

                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException("Player build failed: " + report.summary.result);
                }

                Debug.Log($"PITER_PLAYER_BUILD_OK: {output} ({report.summary.totalSize / (1024 * 1024)} MB)");
            });
        }

        private static void RunBatch(Action action)
        {
            try
            {
                action();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static string ReadArgument(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();

            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (arguments[index] == name)
                {
                    return arguments[index + 1];
                }
            }

            return null;
        }

        private static void ConfigureProjectSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.runInBackground = false;
        }

        private static void ConfigureBuildSettings(string menuScene, List<string> levelScenes)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(menuScene, true)
            };

            foreach (string path in levelScenes)
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
