using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using FileBrowserMac.Runtime;
using UnityEngine;
using ImGuiNET;
using Manager.Runtime;
using Newtonsoft.Json;
using Slate.Runtime;

public class ConfigurableWindowBehaviour : WindowBaseBehaviour
{
    #region Publics
    
    
    #endregion

    #region Unity API

        private void Start()
        {
            LoadFactsOnStart();
        }
   
    #endregion

    #region ImGUI Main

        protected override void WindowLayout()
        {
            if (!showWindow) return;
            
            DrawWindowTabs();

            if (ImGui.Button("Fermer"))
            {
                showWindow = false;
            }
            
            DrawOpenJsonEditors();
        }

    #endregion

    #region Debug / Slate

        private void DrawWindowTabs()
        {
            foreach (var tab in _tabs)
            {
                WindowPosManager.RegisterWindow(tab);
                WindowPosManager.SyncWindowPosition(tab);
            }
            
            if (ImGui.BeginTabBar("##tabs"))
            {
                if (ImGui.BeginTabItem(_tabs[0]))
                {
                    DrawDebugCommands();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(_tabs[1]))
                {
                    DrawColorPicker();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(_tabs[2]))
                {
                    jsonLoader.DrawUI();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(_tabs[3]))
                {
                    _localisation.DrawUI();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
        private void DrawDebugCommands()
        {
            ImGui.Text("Show borders : B");
            ImGui.Text("Hide borders : H");
            ImGui.Text("Lock Window : L");
            ImGui.Text("Quit FullScreen : J");
            ImGui.Text("Unlock Window : U");
        }

        private void DrawColorPicker()
        {
            ImGui.Text("Pick Color:");
            ImGui.ColorEdit4("Color Picker", ref colorValue);
            if (ImGui.Button("Save color"))
            {
                SaveColor();
            }
        }

        private void SaveColor()
        {
            _color = new Color(colorValue.x, colorValue.y, colorValue.z, colorValue.w);
        }

    #endregion

    #region Json Loader UI & Logic
        
        private JsonLoader jsonLoader = new JsonLoader();
        
        private void DrawOpenJsonEditors()
        {
            foreach (var editor in jsonLoader.OpenEditors.ToList())
            {
                if (editor.ShowWindow)
                {
                    editor.Draw();
                }
                else
                {
                    jsonLoader.OpenEditors.Remove(editor);
                }
            }
        }
    
    #endregion
    

    #region Privates and Protected (conservés)
    
        private Localisation.Runtime.Localisation _localisation = new Localisation.Runtime.Localisation();
        private Color _backgroundColor;
        private Color _color;
        private Vector4 colorValue = new Vector4(1, 1, 1, 1);

        private string[] _tabs = { "Debug commands", "Color Picker", "Json Editor(test)", "Localisation" };
        
        private bool showWindow = true;

    #endregion

    
    #region Helpers (optionnel)

    
        private void LoadFactsOnStart()
        {
            // if (FactExists("color", out _color)) { if (_renderer != null) _renderer.material.color = _color; }
        }

    #endregion
}



