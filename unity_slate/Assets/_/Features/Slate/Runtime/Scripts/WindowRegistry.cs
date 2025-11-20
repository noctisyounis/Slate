using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Slate.Runtime
{
    public static class WindowRegistry
    {
        #region Types

            public sealed class WindowInfo
            {
                public string Category;
                public string Entry;
                public Type   Type;
            }

        #endregion

        #region Fields

            private static readonly List<WindowInfo> _windows = new List<WindowInfo>();
            private static bool _initialized;

        #endregion

        #region Public API

            public static IReadOnlyList<WindowInfo> Windows
            {
                get
                {
                    EnsureInitialized();
                    return _windows;
                }
            }

            public static IEnumerable<IGrouping<string, WindowInfo>> GroupedByCategory()
            {
                EnsureInitialized();
                return _windows
                    .GroupBy(w => w.Category)
                    .OrderBy(g => g.Key, StringComparer.Ordinal);
            }

            public static void ToggleWindow(WindowInfo info)
            {
                if (info == null || info.Type == null)
                    return;

                var existing = UnityEngine.Object.FindObjectOfType(info.Type) as MonoBehaviour;

                if (existing == null)
                {
                    var go = new GameObject(info.Type.Name);
                    existing = (MonoBehaviour)go.AddComponent(info.Type);
                }

                var toggle = info.Type.GetMethod(
                    "ToggleVisible",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    Type.EmptyTypes,
                    null
                );

                if (toggle != null)
                {
                    toggle.Invoke(existing, null);
                    return;
                }

                Debug.LogWarning($"[WindowRegistry] {info.Type.Name} has no ToggleVisible() method.");
            }

        #endregion

        #region Init

            private static void EnsureInitialized()
            {
                if (_initialized)
                    return;

                _initialized = true;
                _windows.Clear();

                var attrType = typeof(SlateWindowAttribute);

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try
                    {
                        types = asm.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        types = ex.Types;
                    }

                    if (types == null)
                        continue;

                    foreach (var t in types)
                    {
                        if (t == null || !typeof(MonoBehaviour).IsAssignableFrom(t) || t.IsAbstract)
                            continue;

                        var attr = t.GetCustomAttribute(attrType) as SlateWindowAttribute;
                        if (attr == null)
                            continue;

                        var category = string.IsNullOrEmpty(attr.categoryName)
                            ? "Windows"
                            : attr.categoryName;

                        var entry = string.IsNullOrEmpty(attr.entry)
                            ? t.Name
                            : attr.entry;

                        _windows.Add(new WindowInfo
                        {
                            Category = category,
                            Entry    = entry,
                            Type     = t
                        });
                    }
                }

                _windows.Sort((a, b) =>
                {
                    var cat = string.CompareOrdinal(a.Category, b.Category);
                    return cat != 0 ? cat : string.CompareOrdinal(a.Entry, b.Entry);
                });
            }

        #endregion
    }
}