using System.IO;
using UnityEditor;
using UnityEngine;

namespace Validators.Editor
{
    public class TextureValidator : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath.Contains("é"))
            {
                Debug.LogError($"Texture name contains special character é : {assetPath}");
                File.Move("../../", assetPath);
                Debug.Log(assetPath);
            }
        }

        private void OnPreprocessAsset()
        {
            
        }
    }
}
