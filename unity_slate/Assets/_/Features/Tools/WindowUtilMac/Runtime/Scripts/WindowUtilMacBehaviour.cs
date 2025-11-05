using System;
using System.Runtime.InteropServices;
using Foundation.Runtime;
using UnityEditor;
using UnityEngine;

public class WindowUtilMacBehaviour : FBehaviour
{
    
    private bool showTooltip = false;

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 150, 20), "Commands: TAB");

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
        {
            showTooltip = !showTooltip;
        }

        if (showTooltip)
        {
            GUI.Box(new Rect(10, 40, 220, 110), "Debug Controls");
            GUI.Label(new Rect(20, 70, 200, 20), "- S : Lock build Window");
            GUI.Label(new Rect(20, 90, 200, 20), "- T : Unlock build Window");
            GUI.Label(new Rect(20, 110, 200, 20), "- B : Recover Borders (Might crash Build if on fullscreen WIP)");
            GUI.Label(new Rect(20, 130, 200, 20), "- K : Hide borders (Might crash Build if on fullscreen WIP)");
        }
    }
    public bool _isMovable = false;
    
    [DllImport("no_border_mac")]
    private static extern void MakeWindowBorderless();
    
    
    [DllImport("no_border_mac")]
    private static extern void MakeWindowMovable();
    
    [DllImport("no_border_mac")]
    private static extern void MakeWindowNotMovable();

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
        if (Input.GetKey(KeyCode.K))
        {
            MakeWindowBorderless();
        }

        if (Input.GetKey(KeyCode.S))
        {
            _isMovable = false;
        }
        
        if (Input.GetKey(KeyCode.T))
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
    
    #if UNITY_STANDALONE_OSX
    
    [DllImport("no_border_mac")]
    private static extern void ResetWindowStyle();

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
}


