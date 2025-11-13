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
            Debug.Log("WindowUtilMacBehaviour Start");
            
                //MakeWindowBorderless();
                //SetWindowStyleSafe();
        }


        private void Update()
        {
            //Debug.Log("Is fullscreen ? :" + IsWindowFullScreen());
            #if UNITY_STANDALONE_OSX
                if (Input.GetKey(KeyCode.B))
                {
                    ResetWindowStyle();
                }
                if (Input.GetKey(KeyCode.J))
                {
                    SetWindowStyleSafe();
                    //Debug.Log("Is FS ? :" + IsWindowFullScreen());
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
        public static extern void MakeWindowBorderless();
        
        [DllImport("no_border_mac")]
        public static extern void MakeWindowMovable();
    
        [DllImport("no_border_mac")]
        public static extern void MakeWindowNotMovable();
        
        [DllImport("no_border_mac")]
        public static extern bool IsWindowFullScreen();

        [DllImport("no_border_mac")]
        public static extern void SetWindowStyleSafe();
    
        [DllImport("no_border_mac")]
        public static extern void SetWindowFullScreen();
    
        [DllImport("no_border_mac")]
        public static extern void ToggleFullScreen();
        
        
    #endregion
    
}


