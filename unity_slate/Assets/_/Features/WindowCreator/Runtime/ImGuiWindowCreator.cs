using System.Collections.Generic;
using UnityEngine;
using Foundation.Runtime;
using ImGuiNET;
using UImGui;

namespace WindowCreator.Runtime
{
    public class ImGuiWindowCreator : FBehaviour
    {
        #region Publics

        public string m_windowTitle = "Fenêtre de Création";
        public CameraPanSettings m_cameraPanSettings ;

        #endregion
        
        
        #region Api Unity

        private void OnEnable()
        {
            UImGuiUtility.Layout += OnImGuiLayout;
        }

        private void Start()
        {
            // Pré-charger une fenêtre d'exemple
            CreateNewWindow("Bienvenue");
            var w = _window["Bienvenue"];
            w.m_selectedType = _types[0];
            w.m_extraButton = new List<string> {"OK", "Cancel"};
            w.m_textBuffer = "Bienvenue dans la fenêtre ImGui.";
            _window["Bienvenue"] = w;
        }

        private void OnDisable()
        {
            UImGuiUtility.Layout -= OnImGuiLayout;
        }

        #endregion
        
        
        #region Utils
        
        public void CreateNewWindow(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = $"Fenetre_{_nextId++}";
            
            if (_window.ContainsKey(name))
                return;

            var data = new WindowData
            {
                m_open = true,
                m_title = name,
                m_size = new Vector2(300, 200),
                m_floatValue = 0.5f,
                m_textBuffer = name + " contenu ...",
                m_selectedType = _types[0],
                m_extraButton = new List<string> (),
                m_lastAction = ""
            };
            
            _window.Add(name, data);
            _order.Add(name);
        }
        
        // Ajoute du contenu exemple à la fenêtre (exposé publiquement pour tests)
        public void AddButtonToWindow(string name, string buttonLabel)
        {
            if (!_window.TryGetValue(name, out var w)) return;
            w.m_extraButton.Add(buttonLabel);
            _window[name] = w;
        }
        
        #endregion
        
        
        #region Main Methods

        private void OnImGuiLayout(UImGui.UImGui uImGui)
        {
            DrawMainWindow();
            DrawChildWindows();
            CleanupClosed();
            
            ImGui.End();
        }

        private void DrawMainWindow()
        {
            ImGui.Begin(m_windowTitle);
            
            ImGui.Text("Créer une nouvelle fenêtre ImGui runtime");
            ImGui.SameLine();
            ImGui.InputText("##newWindowName", ref _newWindowNames, 128);
            
            ImGui.SameLine();
            if (ImGui.Button("Créer"))
                CreateNewWindow(_newWindowNames);
            
            ImGui.Separator();
            ImGui.Text($"Fenêtre actives : {_order.Count}");
            for (int i = 0; i < _order.Count; i++)
                ImGui.Text($" - {_order[i]}");
            
            ImGui.Separator();
            ImGui.Text("Exemples rapides");
            if (ImGui.Button("Créer 'Exemple'"))
                CreateNewWindow("Exemple");

            if (ImGui.Button("Ouvrir Fenêtre Camera Settings"))
            {
                if (_cameraSettingsWindow == null)
                    _cameraSettingsWindow = new CameraSettingsWindow(m_cameraPanSettings);
                
                _showCameraWindow = true;
            }

            if (_showCameraWindow && _cameraSettingsWindow != null)
            {
                ImGui.Begin("Camera Settings", ref _showCameraWindow);
                _cameraSettingsWindow.Draw();
                ImGui.End();
            }

            ImGui.End();
        }

        private void DrawChildWindows()
        {
            // Itérer sur la copie de la liste d'ordre pour respecter l'ordre d'ouverture.
            foreach (var name in new List<string>(_order))
            {
                if (!_window.TryGetValue(name, out var data))
                    continue;
                
                // Assure qu'il y a une entrée bool clonable pour ImGui.Begin(ref open)
                bool open = data.m_open;
                
                // Passe le ref bool vers ImGui. Si utilisateur clique la croix open devient false
                if (ImGui.Begin(data.m_title, ref open, ImGuiWindowFlags.None))
                {
                    // exemple : header + texte
                    ImGui.Text($"Titre :  {data.m_title}");
                    
                    // InputText simple (modifie le buffer stocké)
                    ImGui.InputText("Text", ref data.m_textBuffer, 512);
                    
                    // SliderFloat exemple
                    ImGui.SliderFloat("Taille", ref data.m_floatValue, 0.1f, 5f);
                    
                    // Checkbox
                    ImGui.Checkbox("Actif", ref data.m_boolFlag);
                    
                    // Combo / Dropdow (liste fixe)
                    if (ImGui.BeginCombo("Type", data.m_selectedType))
                    {
                        foreach (var t in _types)
                        {
                            bool isSelected = data.m_selectedType == t;
                            if (ImGui.Selectable(t, isSelected))
                                data.m_selectedType = t;
                            if (isSelected) ImGui.SetItemDefaultFocus();
                        }
                        
                        ImGui.EndCombo();
                    }
                    
                    // Child region Scrollable
                    ImGui.BeginChild("child_area", new Vector2(0, 80), ImGuiChildFlags.None);
                    ImGui.Text("Zone scrollable :");
                    ImGui.Text("Lignes d'exemple 1");
                    ImGui.Text("Lignes d'exemple 2");
                    ImGui.EndChild();
                    
                    // Tabs
                    if (ImGui.BeginTabBar("Tabs"))
                    {
                        if (ImGui.BeginTabItem("Controls"))
                        {
                            ImGui.Text("Boutons et actions :");
                            if (ImGui.Button("Action 1"))
                            {
                                // Exemple d'action stockée
                                data.m_lastAction = "Action 1";
                            }
                            ImGui.SameLine();
                            if (ImGui.Button("Action 2"))
                                data.m_lastAction = "Action 2";
                            
                            // Bouton dynamiques ajoutés via utils
                            foreach (var b in data.m_extraButton)
                            {
                                if (ImGui.Button(b))
                                    data.m_lastAction = $"Pressed {b}";
                            }
                            
                            ImGui.EndTabItem();
                        }

                        if (ImGui.BeginTabItem("Debug"))
                        {
                            ImGui.Text($"FloatValue = {data.m_floatValue:F2}");
                            ImGui.Text($"LastAction = {data.m_lastAction}");
                            ImGui.EndTabItem();
                        }
                        
                        ImGui.EndTabBar();
                    }
                    
                    // Fin du contenu de la fenêtre
                }
                
                ImGui.End();
                
                // Sauvegarde de l'état mis à jour (open peut avoir changé)
                data.m_open = open;
                _window[name] = data;
                
                // Si la fenêtre a été fermée par l'utilisateur, marque pour suppression
                if (!open && !_toRemove.Contains(name))
                    _toRemove.Add(name);
            }
            
        }

        private void CleanupClosed()
        {
            if (_toRemove.Count == 0) return;

            foreach (var name in _toRemove)
            {
                _window.Remove(name);
                _order.Remove(name);
            }

            _toRemove.Clear();
        }


        #endregion
        
        
        #region Private and Protected

        private struct WindowData
        {
            public bool m_open;
            public string m_title;
            public Vector2 m_size;
            public float m_floatValue;
            public bool m_boolFlag;
            public string m_textBuffer;
            public string m_selectedType;
            public string m_lastAction;
            public List<string> m_extraButton;
        }
        
        private readonly Dictionary<string, WindowData> _window = new();
        private readonly List<string> _order = new();
        private readonly List<string> _toRemove = new();
        private string _newWindowNames = "Nouvelle Fenêtre";
        private int _nextId = 1;
        private readonly string[] _types = new[] { "Default", "Form", "Tool" };
        
        private CameraSettingsWindow _cameraSettingsWindow;
        private bool _showCameraWindow;

        #endregion
    }
}
