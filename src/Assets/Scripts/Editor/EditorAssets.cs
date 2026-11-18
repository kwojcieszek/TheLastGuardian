using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Asset loading helpers that fail loudly when something is missing.
    /// </summary>
    public static class EditorAssets
    {
        public static Sprite LoadSprite(string assetPath)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            if (sprite == null)
            {
                throw new InvalidOperationException("Missing sprite: " + assetPath);
            }

            return sprite;
        }

        public static Sprite LoadArt(string relativePath)
        {
            return LoadSprite(GameAssetPaths.Sprite(relativePath));
        }

        public static GameObject LoadPrefab(string assetPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null)
            {
                throw new InvalidOperationException("Missing prefab: " + assetPath);
            }

            return prefab;
        }

        public static void EnsureFolder(string assetFolder)
        {
            string trimmed = assetFolder.TrimEnd('/');

            if (AssetDatabase.IsValidFolder(trimmed))
            {
                return;
            }

            string parent = Path.GetDirectoryName(trimmed)?.Replace('\\', '/');
            string name = Path.GetFileName(trimmed);

            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }

        public static int RequireLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);

            if (layer < 0)
            {
                throw new InvalidOperationException("Missing physics layer: " + layerName);
            }

            return layer;
        }
    }
}
