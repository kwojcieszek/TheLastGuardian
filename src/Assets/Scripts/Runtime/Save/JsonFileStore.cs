using System;
using System.IO;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Reads and atomically writes JSON files in the persistent data folder.
    /// </summary>
    public static class JsonFileStore
    {
        private const string TemporarySuffix = ".tmp";
        private const string BackupSuffix = ".bak";

        public static string GetPath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        public static bool Exists(string fileName)
        {
            return File.Exists(GetPath(fileName));
        }

        public static T Read<T>(string fileName) where T : class
        {
            string path = GetPath(fileName);

            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                string json = File.ReadAllText(path);

                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not read {path}: {exception.Message}");

                return null;
            }
        }

        public static bool Write<T>(string fileName, T data) where T : class
        {
            string path = GetPath(fileName);
            string temporaryPath = path + TemporarySuffix;

            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(temporaryPath, json);

                if (File.Exists(path))
                {
                    File.Replace(temporaryPath, path, path + BackupSuffix);
                }
                else
                {
                    File.Move(temporaryPath, path);
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not write {path}: {exception.Message}");

                return false;
            }
        }

        public static void Delete(string fileName)
        {
            string path = GetPath(fileName);

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (File.Exists(path + BackupSuffix))
                {
                    File.Delete(path + BackupSuffix);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not delete {path}: {exception.Message}");
            }
        }
    }
}
