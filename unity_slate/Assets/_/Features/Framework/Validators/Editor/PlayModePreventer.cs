using UnityEditor;
using UnityEngine;

namespace Validators.Editor
{
    [InitializeOnLoad]
    public static class PlayModePreventer
    {
        static PlayModePreventer()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /*
         * Needs polishing, this script will read ALL files inside the project
         * TODO: add the errors to a scriptable object in order to stock all errors with OnPostProcessAssets -> read scriptable object -> if no errors -> playGame
         * todo: use .NET file system to delete the file that doesn't respect our conventions
         * todo: make it so that files(assets) need a prefix, in order to move said assets into their respective paths
         */
        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.ExitingEditMode)
            {
                string[] guids = AssetDatabase.FindAssets("t:Texture");
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (path.Contains("é"))
                    {
                        Debug.LogError($"NO");
                        EditorApplication.isPlaying = false;
                    }
                }
            }
        }
    }
}
