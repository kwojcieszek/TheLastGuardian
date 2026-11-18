using UnityEngine;
using UnityEngine.InputSystem;

namespace PiterGame
{
    /// <summary>
    /// Loads, applies and persists player preferences.
    /// </summary>
    public static class SettingsStore
    {
        public const string FileName = "piter_settings.json";

        private static GameSettings current;

        public static GameSettings Current
        {
            get
            {
                if (current == null)
                {
                    current = JsonFileStore.Read<GameSettings>(FileName) ?? new GameSettings();
                }

                return current;
            }
        }

        public static bool SupportsFullscreenToggle => !Application.isMobilePlatform && Application.platform != RuntimePlatform.WebGLPlayer;

        public static bool TouchControlsVisible
        {
            get
            {
                switch (Current.TouchControls)
                {
                    case TouchControlsMode.AlwaysOn:
                        return true;
                    case TouchControlsMode.Off:
                        return false;
                    default:
                        return Application.isMobilePlatform || Touchscreen.current != null;
                }
            }
        }

        public static void Save()
        {
            JsonFileStore.Write(FileName, Current);
        }

        public static void Apply()
        {
            AudioListener.volume = Current.SoundVolume;

            if (SupportsFullscreenToggle && !Application.isEditor && Screen.fullScreen != Current.Fullscreen)
            {
                Screen.fullScreen = Current.Fullscreen;
            }
        }
    }
}
