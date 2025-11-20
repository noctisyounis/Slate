#if UNITY_EDITOR

    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using System.IO;

    namespace Style.Editor
    {
        public static class BuildDataExporter
        {
            private const string FontsSourceRelative = "_/Content/Fonts";
            private const string ThemesSourceRelative = "_/Content/Styles";
            
            [PostProcessBuild]
            public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
            {
                try
                {
                    var buildDir = Path.GetDirectoryName(pathToBuiltProject);
                    var latestDir = Directory.GetParent(buildDir)?.FullName;

                    if (latestDir == null)
                    {
                        Debug.LogWarning($"[BuildDataExporter] Cannot resolve 'latest' from build path: {pathToBuiltProject}");
                        return;
                    }

                    var dataDir = Path.Combine(latestDir, "data");
                    var fontsDir = Path.Combine(dataDir, "fonts");
                    var themesDir = Path.Combine(dataDir, "themes");

                    Directory.CreateDirectory(dataDir);
                    Directory.CreateDirectory(fontsDir);
                    Directory.CreateDirectory(themesDir);

                    CopyFontsIfEmpty(fontsDir);

                    CopyPresetsIfMissing(themesDir);
                }
                catch (System.SystemException ex)
                {
                    Debug.LogWarning($"[BuildDataExporter] Exception during export: {ex}");
                }
            }

            private static void CopyFontsIfEmpty(string fontsDir)
            {
                var existing = Directory.GetFiles(fontsDir, "*.ttf", SearchOption.TopDirectoryOnly);
                if (existing.Length > 0)
                {
                    Debug.Log($"[BuildDataExporter] Fonts already present in '{fontsDir}', skip copy.");
                    return;
                }

                var src = Path.Combine(Application.dataPath, FontsSourceRelative);
                if (!Directory.Exists(src))
                {
                    Debug.LogWarning($"[BuildDataExporter] Fonts source folder not found: {src}");
                    return;
                }

                foreach (var file in Directory.GetFiles(src, "*.ttf", SearchOption.TopDirectoryOnly))
                {
                    var name = Path.GetFileName(file);
                    var dst = Path.Combine(fontsDir, name);
                    File.Copy(file, dst, overwrite: true);
                }
            }

            private static void CopyPresetsIfMissing(string themesDir)
            {
                var src = Path.Combine(Application.dataPath, ThemesSourceRelative);
                if (!Directory.Exists(src))
                {
                    Debug.LogWarning($"[BuildDataExporter] Themes source folder not found: {src}");
                    return;
                }

                CopyPresetIfMissing(src, themesDir, "fonts.json");
                CopyPresetIfMissing(src, themesDir, "sizes.json");
                CopyPresetIfMissing(src, themesDir, "colors.json");
            }

            private static void CopyPresetIfMissing(string srcRoot, string dstRoot, string fileName)
            {
                var dst = Path.Combine(dstRoot, fileName);
                if (File.Exists(dst))
                {
                    Debug.Log($"[BuildDataExporter] Preset '{fileName}' already exists in '{dstRoot}', skip.");
                    return;
                }

                var src = Path.Combine(srcRoot, fileName);
                if (!File.Exists(src))
                {
                    Debug.LogWarning($"[BuildDataExporter] Preset '{fileName}' not found in '{srcRoot}'");
                    return;
                }

                File.Copy(src, dst, overwrite: true);
            }
        }
    }

#endif
