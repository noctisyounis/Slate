using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WindowCreator.Runtime
{
    /// <summary>
    /// EnumRegistry
    /// ------------
    /// Registre global de listes dynamiques façon RPG Maker MV :
    /// - Actors
    /// - Monsters
    /// - Items
    /// - Skills
    /// - etc.
    ///
    /// Persistance :
    /// Latest/data/database/EnumRegistry.json
    /// Compatible Editor + Build.
    /// </summary>
    public static class EnumRegistry
    {
        #region Publics

        /// <summary>
        /// Indique si le registre a été initialisé (Load effectué).
        /// </summary>
        public static bool m_isInitialized => _isInitialized;

        /// <summary>
        /// Clés disponibles.
        /// </summary>
        public static IEnumerable<string> m_keys
        {
            get
            {
                EnsureInitialized();
                return _lists.Keys;
            }
        }

        #endregion


        #region API Unity (Awake, Start, Update, etc.)

        // Static service : rien ici.

        #endregion


        #region Utils (méthodes publics)

        /// <summary>
        /// Force l'initialisation (Load).
        /// Sans danger si appelé plusieurs fois.
        /// </summary>
        public static void Initialize()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Retourne la liste associée à une clé (ou null si inexistante).
        /// </summary>
        public static List<string> Get(string key)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(key))
                return null;

            return _lists.TryGetValue(key, out var list) ? list : null;
        }

        /// <summary>
        /// Crée ou remplace une liste complète.
        /// </summary>
        public static void Set(string key, List<string> values)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(key))
                return;

            values ??= new List<string>();
            _lists[key] = values;
            MarkDirty();
        }

        /// <summary>
        /// Ajoute une liste vide si elle n'existe pas déjà.
        /// </summary>
        public static bool AddList(string key)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(key))
                return false;

            if (_lists.ContainsKey(key))
                return false;

            _lists.Add(key, new List<string>());
            MarkDirty();
            return true;
        }

        /// <summary>
        /// Supprime une liste.
        /// </summary>
        public static bool RemoveList(string key)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(key))
                return false;

            bool removed = _lists.Remove(key);
            if (removed)
                MarkDirty();

            return removed;
        }

        /// <summary>
        /// Renomme une liste.
        /// </summary>
        public static bool RenameList(string oldKey, string newKey)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(oldKey) || string.IsNullOrWhiteSpace(newKey))
                return false;

            if (!_lists.ContainsKey(oldKey) || _lists.ContainsKey(newKey))
                return false;

            var values = _lists[oldKey];
            _lists.Remove(oldKey);
            _lists.Add(newKey, values);

            MarkDirty();
            return true;
        }

        /// <summary>
        /// Ajoute une valeur à une liste.
        /// </summary>
        public static bool AddValue(string key, string value)
        {
            EnsureInitialized();

            if (!_lists.TryGetValue(key, out var list))
                return false;

            value ??= string.Empty;

            if (list.Contains(value))
                return false;

            list.Add(value);
            MarkDirty();
            return true;
        }

        /// <summary>
        /// Supprime une valeur d'une liste.
        /// </summary>
        public static bool RemoveValue(string key, string value)
        {
            EnsureInitialized();

            if (!_lists.TryGetValue(key, out var list))
                return false;

            bool removed = list.Remove(value);
            if (removed)
                MarkDirty();

            return removed;
        }

        /// <summary>
        /// Sauvegarde immédiate sur disque si dirty.
        /// </summary>
        public static void SaveIfDirty()
        {
            EnsureInitialized();

            if (!_isDirty)
                return;

            SaveInternal();
        }

        /// <summary>
        /// Sauvegarde immédiate explicite.
        /// </summary>
        public static void Save()
        {
            EnsureInitialized();
            SaveInternal();
        }

        #endregion


        #region Main Methods (méthodes private)

        private static void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            _saveFolder = GetDatabaseFolder();
            _savePath = Path.Combine(_saveFolder, _fileName);

            LoadInternal();

            // Si aucune liste, on en crée une par défaut.
            if (_lists.Count == 0)
            {
                _lists["Default"] = new List<string> { "OptionA", "OptionB", "OptionC" };
                _isDirty = true;
                SaveInternal();
            }

            _isInitialized = true;
        }

        private static void MarkDirty()
        {
            _isDirty = true;
        }

        private static void LoadInternal()
        {
            _lists.Clear();

            if (!File.Exists(_savePath))
                return;

            try
            {
                string json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<EnumRegistryData>(json);

                if (data?.m_entries == null)
                    return;

                foreach (var entry in data.m_entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.m_key))
                        continue;

                    entry.m_values ??= new List<string>();
                    _lists[entry.m_key] = entry.m_values;
                }
            }
            catch (Exception)
            {
                // En cas de corruption JSON, on repart vide.
                _lists.Clear();
            }
        }

        private static void SaveInternal()
        {
            try
            {
                var data = new EnumRegistryData
                {
                    m_entries = _lists
                        .Select(kvp => new EnumRegistryEntry
                        {
                            m_key = kvp.Key,
                            m_values = kvp.Value ?? new List<string>()
                        })
                        .ToList()
                };

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(_savePath, json);

                _isDirty = false;
            }
            catch (Exception)
            {
                // On laisse dirty pour retenter plus tard.
                _isDirty = true;
            }
        }

        private static string GetDatabaseFolder()
        {
#if UNITY_EDITOR
            string root = Application.dataPath;
            root = Directory.GetParent(root).FullName;
            root = Directory.GetParent(root).FullName;
#else
            string root = Application.persistentDataPath;
#endif
            string targetDir = Path.Combine(root, "Latest", "data", "database");
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            return targetDir;
        }

        #endregion


        #region Private and Protected

        [Serializable]
        private class EnumRegistryData
        {
            public List<EnumRegistryEntry> m_entries = new();
        }

        [Serializable]
        private class EnumRegistryEntry
        {
            public string m_key;
            public List<string> m_values = new();
        }

        private const string _fileName = "EnumRegistry.json";

        private static bool _isInitialized;
        private static bool _isDirty;

        private static string _saveFolder;
        private static string _savePath;

        private static readonly Dictionary<string, List<string>> _lists = new();

        #endregion
    }
}
