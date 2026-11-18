using UnityEditor.SceneManagement;
using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Generates the main menu scene.
    /// </summary>
    public static class MainMenuSceneBuilder
    {
        public static string Build()
        {
            EditorAssets.EnsureFolder(GameAssetPaths.ScenesRoot);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;

            cameraObject.AddComponent<AudioListener>();

            GameObject menu = new GameObject("MainMenu");
            menu.AddComponent<MainMenuController>();

            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), GameAssetPaths.MainMenuScene);

            return GameAssetPaths.MainMenuScene;
        }
    }
}
