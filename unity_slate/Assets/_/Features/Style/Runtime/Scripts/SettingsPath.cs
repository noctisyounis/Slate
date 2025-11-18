using UnityEngine;
using System.IO;

namespace Style.Runtime
{
    public static class SettingsPath
    {
        public static readonly string RootFolder;
        public static readonly string FontsPresetPath;
        public static readonly string SizesPresetPath;
        public static readonly string ColorsPresetPath;

        static SettingsPath()
        {
            // On veut : <projectRoot>/Build/Latest/data/themes/ en Editor
            // et dans le build : <BuildRoot>/data/themes/
            string root;

#if UNITY_EDITOR
            // .../MyProject/Assets -> .../MyProject
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            root = Path.Combine(projectRoot ?? Application.dataPath,
                "Build", "Latest", "data", "themes");
#else
            // Dans un build standalone, Application.dataPath pointe sur
            // "<Game>_Data". On remonte d'un cran pour avoir le dossier de build.
            var buildRoot = Directory.GetParent(Application.dataPath)?.FullName;
            root = Path.Combine(buildRoot ?? Application.dataPath,
                                "data", "themes");
#endif

            RootFolder = root;
            FontsPresetPath = Path.Combine(RootFolder, "fonts.json");
            SizesPresetPath = Path.Combine(RootFolder, "sizes.json");
            ColorsPresetPath = Path.Combine(RootFolder, "colors.json");

            try
            {
                Directory.CreateDirectory(RootFolder);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ThemePaths] Failed to create '{RootFolder}': {ex.Message}");
            }
        }
    }
}