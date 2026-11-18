using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Player preferences stored separately from campaign progress.
    /// </summary>
    [Serializable]
    public sealed class GameSettings
    {
        [SerializeField] private float soundVolume = 0.8f;
        [SerializeField] private bool fullscreen = true;
        [SerializeField] private int touchControls = (int)TouchControlsMode.Auto;
        [SerializeField] private bool skipSeenDialogues;

        public float SoundVolume
        {
            get => soundVolume;
            set => soundVolume = Mathf.Clamp01(value);
        }

        public bool Fullscreen
        {
            get => fullscreen;
            set => fullscreen = value;
        }

        public TouchControlsMode TouchControls
        {
            get => (TouchControlsMode)touchControls;
            set => touchControls = (int)value;
        }

        public bool SkipSeenDialogues
        {
            get => skipSeenDialogues;
            set => skipSeenDialogues = value;
        }
    }
}
