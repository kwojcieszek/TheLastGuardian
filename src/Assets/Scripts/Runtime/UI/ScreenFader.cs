using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Persistent black overlay that fades between scenes.
    /// </summary>
    public sealed class ScreenFader : MonoBehaviour
    {
        private const float FadeSeconds = 0.35f;
        private const int SortingOrder = 1000;

        private static ScreenFader instance;

        private CanvasGroup group;
        private bool busy;

        public static bool IsBusy => instance != null && instance.busy;

        public static void LoadScene(string sceneName)
        {
            ScreenFader fader = EnsureInstance();

            if (fader.busy)
            {
                return;
            }

            fader.StartCoroutine(fader.FadeAndLoad(sceneName));
        }

        private static ScreenFader EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            Canvas canvas = UiFactory.CreateCanvas("ScreenFader", SortingOrder);
            DontDestroyOnLoad(canvas.gameObject);

            instance = canvas.gameObject.AddComponent<ScreenFader>();
            instance.Build(canvas);

            return instance;
        }

        private void Build(Canvas canvas)
        {
            Image curtain = UiFactory.CreatePanel(canvas.transform, "Curtain", Color.black);
            UiFactory.Stretch(curtain.rectTransform);

            group = canvas.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        private IEnumerator FadeAndLoad(string sceneName)
        {
            busy = true;
            group.blocksRaycasts = true;

            yield return Fade(0f, 1f);

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

            while (operation != null && !operation.isDone)
            {
                yield return null;
            }

            yield return null;
            yield return Fade(1f, 0f);

            group.blocksRaycasts = false;
            busy = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            float elapsed = 0f;

            while (elapsed < FadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, elapsed / FadeSeconds);

                yield return null;
            }

            group.alpha = to;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
