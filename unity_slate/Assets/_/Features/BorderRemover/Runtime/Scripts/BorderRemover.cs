using Foundation.Runtime;
using Slate.Runtime;
using UnityEngine;

namespace BorderRemover.Runtime
{
    public class BorderRemover : FBehaviour 
    {
        #region Unity API

        private void Awake()
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

        private void Update()
        {
            
        }

        #endregion

        #region Privates & Protected

        private GameObject _borderRemover;
        
        #endregion
    }
}