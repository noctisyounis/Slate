using UnityEngine;
using System.IO;

namespace Style.Runtime
{
    public static class SettingsPath
    {
        public static readonly string DataFolder;
        public static readonly string ThemesFolder;
        public static readonly string FontsFolder;
        
        public static readonly string FontsPresetPath;
        public static readonly string SizesPresetPath;
        public static readonly string ColorsPresetPath;

        static SettingsPath()
        {
            
#if UNITY_EDITOR
            
            var assetsDir = Application.dataPath;
            var unityProject = Directory.GetParent(assetsDir)?.FullName;
            var rootFolder = Directory.GetParent(unityProject ?? assetsDir)?.FullName 
                             ?? unityProject
                             ?? assetsDir;

            var latestDir = Path.Combine(rootFolder, "latest");
            
#else
            
            var dataPath = Application.dataPath;
            var slateDir = Directory.GetParent(dataPath)?.FullName
                            ?? dataPath;
            var latestDir = Directory.GetParent(slateDir)?.FullName
                            ?? slateDir;
            
#endif
            
            DataFolder = Path.Combine(latestDir, "data");
            ThemesFolder = Path.Combine(DataFolder, "themes");
            FontsFolder = Path.Combine(DataFolder, "fonts");

            FontsPresetPath = Path.Combine(ThemesFolder, "fonts.json");
            SizesPresetPath = Path.Combine(ThemesFolder, "sizes.json");
            ColorsPresetPath = Path.Combine(ThemesFolder, "colors.json");
            
            TryCreateDirectory(DataFolder);
            TryCreateDirectory(ThemesFolder);
            TryCreateDirectory(FontsFolder);
        }
        
        private static void TryCreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                Directory.CreateDirectory(path);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SettingsPath] Failed to create '{path}': {ex.Message}");
            }
        }
    }
}