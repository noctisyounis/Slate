using System;
using Foundation.Runtime;
using ImGuiNET;
using UImGui;
using UnityEngine;
using Random = System.Random;

public class ConfigurableWindowBehaviour : FBehaviour
{
    #region Public

        [Header("Debug")]
        public GameObject _test;
        public int _number;
        public int inputnumber;
        public Vector2 pos;
        
    
    #endregion
    
   
    #region Unity API

        private void Awake()
        {
          
        }

        private void Start()
        {
            _renderer = _test.GetComponent<Renderer>();
            Load();

            if (FactExists("backgroundColor", out _backgroundColor));
            if (FactExists("gameObjectTestColor", out _gameObjectTestColor));
            if (FactExists("number", out _number));
            if (FactExists("color", out _color))
            {
                _renderer.material.color = _color;
            }
            else _renderer.material.color = Color.white;
        }
        
        private void OnEnable()
        {
            UImGuiUtility.Layout += OnLayout;
        }

        
        private void OnDisable()
        {
            UImGuiUtility.Layout -= OnLayout;
            UImGuiUtility.OnInitialize -= OnInitialize;
            UImGuiUtility.OnDeinitialize -= OnDeinitialize;
        }
        
    #endregion


    #region ImGUI

        private void OnLayout(UImGui.UImGui obj)
        {
            if (!showWindow) return;
            
            ImGui.SetNextWindowSize(new Vector2(400, 300), ImGuiCond.Once);
            ImGui.Begin("Configurable Window");

            string[] tabs = { "Debug commands", "SlateConfigs" };
            if (ImGui.BeginTabBar("##tabs"))
            {
                if (ImGui.BeginTabItem(tabs[0]))
                {
                    DrawDebugCommands();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(tabs[1]))
                {
                    DrawSlateConfigs();
                    ImGui.Spacing();

                    if (ImGui.Button("Save"))
                    {
                        SaveColor();
                        Save();
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            if (ImGui.Button("Fermer"))
            {
                showWindow = false;
            }

            ImGui.End();
        }
        
        private void OnInitialize(UImGui.UImGui obj)
        {
            // runs after UImGui.OnEnable();
        
        }

        private void OnDeinitialize(UImGui.UImGui obj)
        {
            // runs after UImGui.OnDisable();
        }

    #endregion



    #region Context Menus
        
        [ContextMenu("Set blue")]
        public void SetBlue()
        {
            _number = inputnumber;
            SetFact("number", _number,true);
            
            var color = Color.blue;
            _renderer.material.color = color; 
            
            SetFact("color", color, true);
            Save();

        }
        
        [ContextMenu("Set Red")]
        public void SetRed()
        {
            _number = inputnumber;
            SetFact("number", _number,true);
            
            var color = Color.red;
            _renderer.material.color = color; 
            
            SetFact("color", color, true);
            Save();

        }
        
        [ContextMenu("Set Yellow")]
        public void SetYellow()
        {
            _number = inputnumber;
            SetFact("number", _number,true);
            
            var color = Color.yellow;
            _renderer.material.color = color; 
            
            SetFact("color", color, true);
            Save();

        }

        private void DrawDebugCommands()
        {
            ImGui.Text("Show borders : B");
            ImGui.Text("Hide borders : H");
            ImGui.Text("Lock Window : L");
            ImGui.Text("Unlock Window : U");
        }

        private void DrawSlateConfigs()
        {
            ImGui.Text("Configure la couleur ici :");
            ImGui.ColorEdit4("Color Picker", ref colorValue);
        }

        private void SaveColor()
        {
            if (_test != null)
            {
                var renderer = _test.GetComponent<Renderer>();
                if (renderer != null)
                {
                    _color = new Color(colorValue.x, colorValue.y, colorValue.z, colorValue.w);
                    renderer.material.color = _color;
                }
            }
                        
            SetFact("color", _color, true);
        }
        private void HandleZoom()
        {
            var io = ImGui.GetIO();
            if (io.MouseWheel != 0)
            {
                zoom += io.MouseWheel * 0.1f;
                zoom = Mathf.Clamp(zoom, 0.5f, 3.0f);
                ImGui.GetStyle().ScaleAllSizes(zoom);
            }

            ImGui.Begin("Ma Fenêtre zoomable");

            ImGui.Text($"Zoom actuel : {zoom}");
            var rnd = new Random();
            ImGui.End();
        }
        
    #endregion
    
    
    #region Privates and Protected
    
    
        private Color _gameObjectTestColor;
        private Color _backgroundColor;
        private Renderer _renderer;
        private Color _color;
        private Vector4 colorValue = new Vector4(1, 1, 1, 1);
        
        private bool showWindow = true;
        private int currentTab = 0; 
        private float zoom = 1f;
        
    #endregion
}