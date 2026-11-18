using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Read-only access to level dialogues.
    /// </summary>
    public static class DialogueCatalog
    {
        private const string ResourcePath = "Data/dialogues";

        private static DialogueEntry[] entries;

        public static DialogueLine[] GetLines(int levelId)
        {
            DialogueEntry entry = Array.Find(GetEntries(), item => item.Level == levelId);

            return entry != null ? entry.Lines : Array.Empty<DialogueLine>();
        }

        public static DialogueLine[] GetIntroLines(int levelId, int finalLevelIntroCount)
        {
            DialogueLine[] lines = GetLines(levelId);

            if (!LevelCatalog.IsFinalLevel(levelId) || lines.Length <= finalLevelIntroCount)
            {
                return lines;
            }

            DialogueLine[] intro = new DialogueLine[finalLevelIntroCount];
            Array.Copy(lines, intro, finalLevelIntroCount);

            return intro;
        }

        public static DialogueLine[] GetOutroLines(int levelId, int finalLevelIntroCount)
        {
            DialogueLine[] lines = GetLines(levelId);

            if (!LevelCatalog.IsFinalLevel(levelId) || lines.Length <= finalLevelIntroCount)
            {
                return Array.Empty<DialogueLine>();
            }

            DialogueLine[] outro = new DialogueLine[lines.Length - finalLevelIntroCount];
            Array.Copy(lines, finalLevelIntroCount, outro, 0, outro.Length);

            return outro;
        }

        private static DialogueEntry[] GetEntries()
        {
            if (entries != null)
            {
                return entries;
            }

            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);

            if (asset == null)
            {
                Debug.LogError("Missing dialogues: Resources/" + ResourcePath);
                entries = Array.Empty<DialogueEntry>();

                return entries;
            }

            string json = "{\"items\":" + asset.text + "}";
            DialogueCollection collection = JsonUtility.FromJson<DialogueCollection>(json);
            entries = collection != null ? collection.Items : Array.Empty<DialogueEntry>();

            return entries;
        }
    }
}
