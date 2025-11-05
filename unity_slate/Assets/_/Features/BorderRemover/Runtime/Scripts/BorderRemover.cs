using Foundation.Runtime;
using Slate.Runtime;
using UnityEngine;

namespace BorderRemover.Runtime
{
    public class BorderRemover : FBehaviour 
    {
        #region Unity API

        private void Start()
        {
            #if !UNITY_EDITOR
            
            _borderRemover = Instantiate(new GameObject("BorderRemover"));
            _borderRemover.transform.SetParent(transform);
            #if UNITY_STANDALONE_WIN
            _borderRemover.AddComponent<NoBorderWin>();
            #endif
            #if UNITY_STANDALONE_OSX
            _borderRemover.AddComponent<WindowUtilMacBehaviour>();
            #endif

            #endif
        }

        #endregion

        #region Privates & Protected
        
        private GameObject _borderRemover;
        
        #endregion
    }
}