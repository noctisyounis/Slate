using System;
using System.Runtime.InteropServices;
using Foundation.Runtime;
using Inputs.Runtime;
using UnityEditor;
using UnityEngine;

public class WindowUtilMacBehaviour : FBehaviour
{
    #region Publics

        public bool _isMovable = false;

    #endregion
    

    #region Unity API

        private void Start()
        {
            #if UNITY_STANDALONE_OSX
                try
                {
                    MakeWindowBorderless();
                    Info("Fenêtre borderless appliquée");
                    Debug.Log("Fenêtre borderless appliquée");
                }
                catch (DllNotFoundException e)
                {
                    Error($"Plugin not found: {e.Message}");
                }
                catch (EntryPointNotFoundException e)
                {
                    Error($"Function not found in plugin: {e.Message}");
                }
            #endif
        }


        private void Update()
        {
            #if UNITY_STANDALONE_OSX
                if (Input.GetKey(KeyCode.B))
                {
                    ResetWindowStyle();
                }
                if (Input.GetKey(KeyCode.H))
                {
                    MakeWindowBorderless();
                }

                if (Input.GetKey(KeyCode.L))
                {
                    _isMovable = false;
                }
                
                if (Input.GetKey(KeyCode.U))
                {
                    _isMovable = true;
                }
                
                if (_isMovable)
                {
                    MakeWindowMovable();
                    InfoInProgress("Window Can move");
                    Debug.Log("Window Can move");
                    
                    
                }
                else
                {
                    MakeWindowNotMovable();
                    InfoDone("Window Cant move");
                    Debug.Log("Window Cant move");
                    
                }
            #endif
        }

    #endregion
    
    
    
    #region Utils
    
        #if UNITY_STANDALONE_OSX
            
            [DllImport("no_border_mac")]
            private static extern void ResetWindowStyle();
            [SerializeField] private InputsHandler _inputsHandler;

            #if UNITY_EDITOR
                [InitializeOnLoad]
                public class PlayModeStateListener
                {
                    static PlayModeStateListener()
                    {
                        EditorApplication.playModeStateChanged += state =>
                        {
                            if (state == PlayModeStateChange.ExitingPlayMode)
                            {
                                ResetWindowStyle();
                            }
                        };
                    }
                }
            #endif
        #endif

    #endregion
    
    
    #region Privates
    
        private bool showTooltip = false;
        
        [DllImport("no_border_mac")]
        private static extern void MakeWindowBorderless();
        
        [DllImport("no_border_mac")]
        private static extern void MakeWindowMovable();
    
        [DllImport("no_border_mac")]
        private static extern void MakeWindowNotMovable();
        
    #endregion
    
}


