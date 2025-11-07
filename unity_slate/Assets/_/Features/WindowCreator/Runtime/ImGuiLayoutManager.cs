using System;
using System.Collections.Generic;
using Foundation.Runtime;
using ImGuiNET;
using UImGui;
using UnityEditor.Overlays;
using UnityEngine;

namespace WindowCreator.Runtime
{
    [Serializable]
    public class LayoutData
    {
        public int m_id;
        public string m_name; // Nom interne / identifiant unique
        public string m_title; // Titre affiché dans la fenêtre
        public bool m_open;
        public string m_descrition;
        public string m_lastAction;
        public List<string> m_extraButtons;
    }

    [Serializable]
    public class LayoutCoutainer
    {
        public List<LayoutData> m_layouts;
        public int m_nextId;
    }
    
    public class ImGuiLayoutManager : FBehaviour
    {
        #region Public

        public string m_windowTitle = "Gestion des Layouts";
        public string m_newLayoutName = "Nouveau Layout";

        #endregion


        #region Api Unity

        private void OnEnable()
        {
            UImGuiUtility.Layout += OnImGuiLayout;
            LoadLayoutsFromFacts();
        }

        private void OnDisable()
        {
            UImGuiUtility.Layout -= OnImGuiLayout;
        }

        #endregion
        
        
        #region Utils

        public void CreateNewLayout(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = $"Layout_{_nextId++}";

            if (_layouts.Exists(l => l.m_name == name))
            {
                Warning($"Un layout nommé '{name}' existe déjà.");
            }

            var layout = new LayoutData
            {
                m_id = _nextId++,
                m_name = name,
                m_title = name,
                m_open = true,
                m_descrition = "",
                m_lastAction = "",
                m_extraButtons = new List<string>()
            };
                
                _layouts.Add(layout);
                SaveLayoutsToFacts();
                InfoDone($"Layout créé : {layout.m_name}");
                
                string debugJson = JsonUtility.ToJson(_container, true);
                Debug.Log($"[ImGuiLayoutManager] JSON sauvegardé : \n {debugJson}");
        }
        
        #endregion


        #region Main Methods

        private void SaveLayoutsToFacts()
        {
            _container.m_layouts = _layouts;
            _container.m_nextId = _nextId;
            
            SetFact("Layout_List", _layouts, true);
            Save();
            InfoDone("Layout persistés dans GameFacs.");
        }

        private void LoadLayoutsFromFacts()
        {
            if (FactExists("Layout_Countainer", out LayoutCoutainer saved))
            {
                _container = saved;
                _layouts = _container.m_layouts ?? new List<LayoutData>();
                _nextId = _container.m_nextId;
                InfoDone($"Layouts chargés depuis GameFacts : {_layouts.Count}");
            }

            else
            {
                _layouts = new List<LayoutData>();
                _nextId = 1;
                InfoInProgress("Aucun layout trouvé - initialisation vide");
            }
        }

        private int CalculateNextId()
        {
            int max = 0;
            foreach (var l in _layouts)
                if (l.m_id > max) max = l.m_id;
            return max + 1;
        }

        private void ReopenLayouts(string name)
        {
            for (int i = 0; i < _layouts.Count; i++)
            {
                if (_layouts[i].m_name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    var layout = _layouts[i];
                    layout.m_open = true;
                    _layouts[i] = layout;
                    Info($"Layout rouvert : {layout.m_name}");
                    SaveLayoutsToFacts();
                    return;
                }
            }
            
            Warning($"Aucun layout trouvé avec le nom '{name}'.");
        }

        private void OnImGuiLayout(UImGui.UImGui uImGui)
        {
            DrawMainWindow();
            DrawLayouts();
            CleanupClosed();
        }

        private void DrawMainWindow()
        {
            ImGui.Begin(m_windowTitle);
            
            ImGui.Text("Créer un nouveau Layout :");
            ImGui.InputText("##NewLayoutName", ref m_newLayoutName, 64);
            
            if (ImGui.Button("Créer"))
                CreateNewLayout(m_newLayoutName);
            
            ImGui.SameLine();
            if (ImGui.Button("Sauvegarder tous les layouts"))
                SaveLayoutsToFacts();
            
            ImGui.SameLine();
            if (ImGui.Button("Recharger"))
                LoadLayoutsFromFacts();
            
            ImGui.Separator();
            foreach (var layout in _layouts )
            {
                ImGui.Text($"{layout.m_title}");

                if (layout.m_open)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled("[Ouvert]");
                }
                else
                {
                    ImGui.SameLine();
                    if (ImGui.SmallButton($"Ouvrir##{layout.m_name}"))
                       _toReopen.Add(layout.m_name);
                }
            }
            ImGui.End();

            if (_toReopen.Count > 0)
            {
                foreach (var name in _toReopen)
                {
                    ReopenLayouts(name);
                }
                
                _toReopen.Clear();
            }
        }
        
        private void DrawLayouts()
        {
            for (int i = 0; i < _layouts.Count; i++)
            {
                var layout = _layouts[i];
                bool open =  layout.m_open;

                if (open && ImGui.Begin(layout.m_title, ref open))
                {
                    ImGui.Text("Titre :");
                    ImGui.InputText("##Title", ref layout.m_title, 64);
                    
                    ImGui.Text("Description :");
                    ImGui.InputTextMultiline("##desc", ref layout.m_descrition, 512, new Vector2(-1, 80));

                    if (ImGui.Button("Sauvegarder"))
                    {
                        layout.m_lastAction = "Sauvegarder";
                        SaveLayoutsToFacts();
                    }
                    
                    ImGui.SameLine();
                    if (ImGui.Button("Modifier"))
                        layout.m_lastAction = "Modification";
                    
                    ImGui.SameLine();
                    if (ImGui.Button("Fermer"))
                        open = false;
                    
                    ImGui.SameLine();
                    if (ImGui.Button("Supprimer"))
                        _toRemove.Add(layout.m_name);
                    
                    ImGui.Text($"Dernière action : {layout.m_lastAction}");
                    ImGui.End();
                }
                
                layout.m_open = open;
                _layouts[i] = layout;
            }
        }

        private void CleanupClosed()
        {
            if (_toRemove.Count == 0) return;

            _layouts.RemoveAll(l => _toRemove.Contains(l.m_name));
            _toRemove.Clear();
            SaveLayoutsToFacts();
        }

        #endregion


        #region Private and Protected

        private List<LayoutData> _layouts = new ();
        private LayoutCoutainer _container = new();
        private List<string> _toRemove = new();
        private List<string> _toReopen = new();
        private int _nextId = 1;

        #endregion
        


    }
}
