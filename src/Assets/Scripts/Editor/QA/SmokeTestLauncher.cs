using System;
using PiterGame.QA;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PiterGame.EditorTools.QA
{
    /// <summary>
    /// Starts the automated play-mode smoke test from the menu or from batch mode.
    /// </summary>
    [InitializeOnLoad]
    public static class SmokeTestLauncher
    {
        public const string OutputArgument = "-smokeOutput";

        private const string ActiveKey = "PiterGame.SmokeTest.Active";
        private const string BatchKey = "PiterGame.SmokeTest.Batch";

        public static bool IsBatch => SessionState.GetBool(BatchKey, false);

        static SmokeTestLauncher()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem("The Last Guardian/Run Smoke Test", priority = 60)]
        public static void Run()
        {
            Start(false);
        }

        public static void BatchRun()
        {
            Start(true);
        }

        public static string ReadOutputFolder()
        {
            string[] arguments = Environment.GetCommandLineArgs();

            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (arguments[index] == OutputArgument)
                {
                    return arguments[index + 1];
                }
            }

            return "Logs/SmokeTest";
        }

        public static void Finish(int exitCode)
        {
            SessionState.SetBool(ActiveKey, false);

            if (IsBatch)
            {
                EditorApplication.Exit(exitCode);

                return;
            }

            EditorApplication.isPlaying = false;
        }

        private static void Start(bool batch)
        {
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetBool(BatchKey, batch);

            EditorSceneManager.OpenScene(GameAssetPaths.MainMenuScene);
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            GameObject host = new GameObject("SmokeTestRunner");
            UnityEngine.Object.DontDestroyOnLoad(host);
            SmokeTestRunner.OutputFolder = ReadOutputFolder();
            SmokeTestRunner.Finished = Finish;
            host.AddComponent<SmokeTestRunner>();
        }
    }
}
