using System;
using System.Collections.Generic;
using System.IO;
using Foundation.Runtime;
using ImGuiNET;
using UImGui;
using UnityEngine;

namespace WindowCreator.Runtime
{

    [Serializable]
    public class LayoutZone
    {
        public string Key;      // Data name
        public string Type;     // Value Type(int, float, string, bool ...)
        public string Value;    // Value stored as a string for simple serialization
    }

    [Serializable]
    public class RecordData
    {
        public string m_name; // Record name
        public string m_guid = Guid.NewGuid().ToString();
        
        [SerializeField] private List<LayoutZone> _fields = new List<LayoutZone>();
        public IReadOnlyList<LayoutZone> Fields => _fields;
        
        public int FieldCount => _fields.Count;
        public LayoutZone GetField(int index)
        {
            if (index < 0 || index >= _fields.Count)
                return null;
            return _fields[index];
        }

        public void AddField(LayoutZone zone) => _fields.Add(zone);
        public void RemoveField(LayoutZone zone) => _fields.Remove(zone);
        public void RemoveFieldAt(int index)
        {
            if (index < 0 || index >= _fields.Count)
                _fields.RemoveAt(index);
        }
    }
    [Serializable]
    public class LayoutData
    {
        public int m_id;
        public string m_name; // Internal name / unique identifier
        public string m_title; // Title displayed in the window
        public bool m_open;
        public string m_descrition;
        public string m_lastAction;
        public List<RecordData> m_records = new List<RecordData>() ;

    }

    [Serializable]
    public class LayoutContainer
    {
        public List<LayoutData> m_layouts;
        public int m_nextId;
    }
    
    public class ImGuiLayoutManager : FBehaviour
    {
        #region Public

        public string m_windowTitle = "layout Management";
        public string m_newLayoutName = "New Layout";

        #endregion


        #region Api Unity

        private void Awake()
        {
            string saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            _savePath = Path.Combine(saveFolder, "layouts.json");
            if (!File.Exists(_savePath))
                File.WriteAllText(_savePath, "{}");

            // Initialize SaveSystem
            SaveSystem.SetPath(saveFolder);

            // loading layouts
            try
            {
                Load(); 
                LoadLayoutsFromFacts();
            }
            catch (FileNotFoundException)
            {
                InfoInProgress("No backup file found. Creating an empty container.");
                _container = new LayoutContainer { m_layouts = new List<LayoutData>(), m_nextId = 1 };
                _layouts = new List<LayoutData>(_container.m_layouts ??  new List<LayoutData>());
            }
        }

        private void OnEnable()
        {
            UImGuiUtility.Layout += OnImGuiLayout;
        }
        
        private void OnDisable()
        {
            UImGuiUtility.Layout -= OnImGuiLayout;
            if (_layoutsDirty) SaveLayoutsToFacts();
        }

        #endregion
        
        
        #region Utils

        public void CreateNewLayout(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = $"Layout{_container.m_layouts.Count}";
   

            if (_container.m_layouts.Exists(l => l.m_name == name))
            {
                Warning($"A layour named '{name}' already exists.");
                return;
            }

            var layout = new LayoutData
            {
                m_id = _container.m_nextId,
                m_name = name,
                m_title = name,
                m_open = true,
                m_descrition = "",
                m_lastAction = "",
                m_records = new List<RecordData>()
            };
                
            _container.m_layouts.Add(layout);
            _layouts.Add(layout);
            _container.m_nextId++;
            _layoutsDirty = true;
            
            _focusLayoutId =  layout.m_id;
                
            InfoDone($"Layout created : {layout.m_name}");
        }
        
        #endregion


        #region Main Methods

        private void SaveLayoutsToFacts()
        {
            SyncContainerOpenState();
            SetFact("WindowCreator.LayoutContainer", _container, true);
            Save();
            _layoutsDirty = false;
            InfoDone("Layout persisted in GameFacs.");
        }

        private void LoadLayoutsFromFacts()
        {
            
            if (FactExists("WindowCreator.LayoutContainer", out LayoutContainer saved))
            {
                _container = saved;
                _layouts = new List<LayoutData>(_container.m_layouts ?? new List<LayoutData>());

                foreach (var layout in _layouts)
                {
                    if (layout.m_open)
                    {
                        Info($"Automatic reopening of the layout '{layout.m_name}'");
                    }
                }
            }

            else
            {
                _container = new LayoutContainer{ m_layouts = new List<LayoutData>(), m_nextId = 1 }; 
                _layouts = new List<LayoutData>();
                InfoInProgress("No layout found - empty initialization");
            }
        }
        
        private void ReopenLayouts(string name)
        {
            var layout = _layouts.Find(l => l.m_name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (layout != null)
            {
                if (!layout.m_open)
                {
                    layout.m_open = true;
                    _layoutsDirty = true;
                    Info($"Layout rouvert : {layout.m_name}");
                    
                }
            }
            else
            {
                Warning($"Aucun layout trouvé avec le nom '{name}'.");
            }
        }

        private void OnImGuiLayout(UImGui.UImGui uImGui)
        {
            DrawMainWindow();
            //DrawLayouts();

            if (_toRemoveRecords.Count > 0)
            {
                foreach (var (layout, record) in _toRemoveRecords)
                    layout.m_records.Remove(record);
                _toRemoveRecords.Clear();
                _layoutsDirty = true;
            }

            if (_toRemoveLayouts.Count > 0)
            {
                foreach (var layout in _toRemoveLayouts)
                {
                    _layouts.Remove(layout);
                    _container.m_layouts?.RemoveAll(l => l.m_id == layout.m_id);
                }
                
                _toRemoveLayouts.Clear();
                _layoutsDirty  = true;
            }

            if (_layoutsDirty)
            {
                SaveLayoutsToFacts();
                _layoutsDirty = false;
            }

            if (_toReopen.Count > 0)
            {
                foreach (var name in _toReopen )
                {
                    ReopenLayouts(name);
                }
                _toReopen.Clear();
            }
            
            CleanupClosed();
        }

        private void DrawMainWindow()
        {
            ImGui.Begin(m_windowTitle);

            // Barre d’outils
            ImGui.Text("Create a new Layout:");
            ImGui.InputText("##NewLayoutName", ref m_newLayoutName, 64);
            if (ImGui.Button("Create"))
                CreateNewLayout(m_newLayoutName);
            ImGui.SameLine();
            if (ImGui.Button("Save All"))
                SaveLayoutsToFacts();
            ImGui.SameLine();
            if (ImGui.Button("Reload"))
                LoadLayoutsFromFacts();

            ImGui.Separator();

            // --- Zone principale avec deux colonnes ---
            ImGui.BeginChild("LeftPanel", new Vector2(250, 0), ImGuiChildFlags.Border);

            // Liste des layouts
            ImGui.Text("Layouts:");
            ImGui.Separator();
            foreach (var layout in _layouts)
            {
                bool isSelected = (_focusLayoutId == layout.m_id);

                if (ImGui.Selectable($"{layout.m_title}", isSelected))
                {
                    _focusLayoutId = layout.m_id;
                }
            }
            ImGui.EndChild();

            ImGui.SameLine(); // <-- Affiche la zone de droite sur la même ligne

            // --- Détails du layout sélectionné ---
            ImGui.BeginChild("RightPanel", new Vector2(0, 0), ImGuiChildFlags.Border);

            var selectedLayout = _layouts.Find(l => l.m_id == _focusLayoutId);
            if (selectedLayout != null)
            {
                ImGui.Text($"Layout: {selectedLayout.m_title}");
                ImGui.InputText($"##Title_{selectedLayout.m_id}", ref selectedLayout.m_title, 64);

                ImGui.Text("Description:");
                ImGui.InputTextMultiline($"##desc_{selectedLayout.m_id}", ref selectedLayout.m_descrition, 512, new Vector2(-1, 80));
                ImGui.Separator();

                ImGui.Text("Records:");
                ImGui.BeginChild($"Records_{selectedLayout.m_id}", new Vector2(0, 200), ImGuiChildFlags.None);

                foreach (var record in selectedLayout.m_records)
                {
                    string recId = $"rec_{selectedLayout.m_id}_{record.m_guid}";
                    ImGui.Separator();

                    ImGui.InputText($"Nom##{recId}", ref record.m_name, 32);

                    for (int z = 0; z < record.Fields.Count; z++)
                    {
                        var zone = record.GetField(z);
                        if (zone == null) continue;

                        string zoneId = $"{recId}_field_{z}";
                        float width = ImGui.GetContentRegionAvail().x/3f;
                        ImGui.SetNextItemWidth(width);
                        ImGui.InputText($"Key##{zoneId}", ref zone.Key, 32);

                        string[] types = { "string", "int", "float", "bool" };
                        int currentTypeIndex = Mathf.Max(0, Array.IndexOf(types, zone.Type));
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(width);
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0,0,0,1));
                        ImGui.PushStyleColor(ImGuiCol.PopupBg,new Vector4(0.08f,0.08f,0.08f,1));
                        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,6f);
                        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6,4));
                        
                        float width2 = ImGui.GetContentRegionAvail().x;
                        ImGui.SetNextItemWidth(width2);
                        if (ImGui.Combo($"Type##{zoneId}", ref currentTypeIndex, types, types.Length))
                            zone.Type = types[currentTypeIndex];
                        ImGui.PopStyleVar(2);
                        ImGui.PopStyleColor(4);
                        
                        ImGui.SetNextItemWidth(width);
                        ImGui.InputText($"Value##{zoneId}", ref zone.Value, 32);
                        
                        ImGui.SameLine();
                        if (ImGui.Button($"Delete##{zoneId}"))
                        {
                            record.RemoveField(zone);
                            _layoutsDirty = true;
                            break;
                        }
                    }

                    if (ImGui.Button($"+ Add Zone##{recId}"))
                    {
                        record.AddField(new LayoutZone { Key = "NewZone", Type = "string", Value = "" });
                        _layoutsDirty = true;
                    }

                    ImGui.SameLine();
                    if (ImGui.Button($"DeleteTab##{recId}"))
                    {
                        _toRemoveRecords.Add((selectedLayout, record));
                        _layoutsDirty = true;
                    }
                }

                ImGui.EndChild();

                if (ImGui.Button($"AddTab##addrec_{selectedLayout.m_id}"))
                {
                    var newrec = new RecordData { m_name = "NewTab" };
                    selectedLayout.m_records.Add(newrec);
                    _layoutsDirty = true;
                }

                DrawLayoutButton(selectedLayout);
            }
            else
            {
                ImGui.TextDisabled("Select a layout on the left to view details.");
            }

            ImGui.EndChild();
            ImGui.End();

            // Réouverture éventuelle
            if (_toReopen.Count > 0)
            {
                foreach (var name in _toReopen)
                    ReopenLayouts(name);
                _toReopen.Clear();
            }
        }
        
        private void DrawLayoutButton(LayoutData layout)
        {
            if (ImGui.Button("Save"))
            {
                layout.m_lastAction = "Save";
                SaveLayoutsToFacts();
            }
                    
            ImGui.SameLine();
            if (ImGui.Button("Modify"))
                layout.m_lastAction = "Modify";
                    
            ImGui.SameLine();
            if (ImGui.Button("Delete"))
                _toRemove.Add(layout.m_name);
                    
            ImGui.Text($"Last action : {layout.m_lastAction}");
        }

        private void CleanupClosed()
        {
            if (_toRemove.Count > 0)
            {
                _layouts.RemoveAll(l => _toRemove.Contains(l.m_name));
                _toRemove.Clear();
                _layoutsDirty = true;
            }
        }

        private void SyncContainerOpenState()
        {
            foreach (var layout in _layouts)
            {
                var containerLayout = _container.m_layouts.Find(l => l.m_id == layout.m_id);
                if (containerLayout != null) containerLayout.m_open = layout.m_open;
            }
        }
        
        #endregion


        #region Private and Protected

        private List<LayoutData> _layouts = new ();
        private List<LayoutData> _toRemoveLayouts = new ();
        private LayoutContainer _container = new();
        private List<string> _toRemove = new();
        private List<string> _toReopen = new();
        private List<(LayoutData layout, RecordData record)> _toRemoveRecords = new();
        private bool _layoutsDirty  = false;
        private string _savePath;
        private bool _dataChanged;
        private int _focusLayoutId = -1;

        #endregion



    }
}
