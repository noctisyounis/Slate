using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Foundation.Runtime;
using ImGuiNET;
using UImGui;
using UnityEngine;

namespace WindowCreator.Runtime
{
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
                var json = File.ReadAllText(_savePath);
                _container = JsonUtility.FromJson<DataModels.LayoutContainer>(json);
    
                if (_container == null)
                    _container = new DataModels.LayoutContainer
                    {
                        m_layouts = new List<DataModels.LayoutData>(),
                        m_nextId = 1
                    };
    
                _layouts = new List<DataModels.LayoutData>(_container.m_layouts);

                // Résolution des types
                foreach (var layout in _layouts)
                {
                    layout.m_records ??= new List<DataModels.RecordData>();
                    layout.m_title ??= layout.m_name ?? "Untitled";
                    layout.m_descrition ??= "";
                    layout.m_lastAction ??= "";

                    foreach (var record in layout.m_records)
                    {
                        foreach (var zone in record.Fields)
                            zone?.ResolveTypeFromString();
                    }
                    InfoDone("Layouts loaded from JSON");
                }
            }
            catch (Exception e)
            {
                InfoInProgress("No backup file found. Creating an empty container.");
                _container = new DataModels.LayoutContainer { m_layouts = new List<DataModels.LayoutData>(), m_nextId = 1 };
                _layouts = new List<DataModels.LayoutData>();
                _layoutsDirty = true;
            }
            // --- Initialisation de la mémoire runtime ---
            _container.m_layouts ??= new List<DataModels.LayoutData>();
            _layouts ??= new List<DataModels.LayoutData>();
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

            var layout = new DataModels.LayoutData
            {
                m_id = _container.m_nextId,
                m_name = name,
                m_title = name,
                m_open = true,
                m_descrition = "",
                m_lastAction = "",
                m_records = new List<DataModels.RecordData>()
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
            // Synchroniser l’état ouvert/fermé
            foreach (var layout in _layouts)
            {
                var containerLayout = _container.m_layouts.Find(l => l.m_id == layout.m_id);
                if (containerLayout != null) containerLayout.m_open = layout.m_open;
            }

            // Sauvegarde JSON
            try
            {
                string json = JsonUtility.ToJson(_container, true);
                File.WriteAllText(_savePath, json);
                InfoDone("Layouts saved to JSON.");
            }
            catch (Exception e)
            {
                Warning($"Failed to save JSON: {e.Message}");
            }

            // Sauvegarde Facts
            SetFact("WindowCreator.LayoutContainer", _container, true);

            _layoutsDirty = false;
        }

        private void LoadLayoutsFromFacts()
        {

            DataModels.LayoutContainer saved = null;

            // --- Essayer de charger depuis Facts ---
            if (!FactExists("WindowCreator.LayoutContainer", out saved))
            {
                // Si aucune fact, charger depuis JSON
                if (File.Exists(_savePath))
                {
                    try
                    {
                        string json = File.ReadAllText(_savePath);
                        saved = JsonUtility.FromJson<DataModels.LayoutContainer>(json);
                    }
                    catch
                    {
                        InfoInProgress("Failed to load layouts.json. Creating empty container.");
                    }
                }
            }

            // --- Initialisation si aucun layout trouvé ---
            if (saved == null)
            {
                _container = new DataModels.LayoutContainer
                {
                    m_layouts = new List<DataModels.LayoutData>(),
                    m_nextId = 1
                };
                _layouts = new List<DataModels.LayoutData>();
                _layoutsDirty = true;
                InfoInProgress("No layout found. Empty container created.");
                return;
            }

            // --- Charger les layouts ---
            saved.m_layouts ??= new List<DataModels.LayoutData>();
            if (saved.m_nextId <= 0) saved.m_nextId = saved.m_layouts.Count + 1;

            _container = saved;
            _layouts = new List<DataModels.LayoutData>(_container.m_layouts);

            // --- Préparer records et zones ---
            foreach (var layout in _layouts)
            {
                layout.m_records ??= new List<DataModels.RecordData>();
                layout.m_title ??= layout.m_name ?? "Untitled";
                layout.m_descrition ??= "";
                layout.m_lastAction ??= "";

                foreach (var record in layout.m_records)
                {
                    foreach (var zone in record.Fields)
                        zone?.ResolveTypeFromString();
                }
            }

            InfoDone("Layouts loaded from Facts/JSON.");
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
            DrawRuntimeWindows();

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

            // Left / right panels
            ImGui.BeginChild("LeftPanel", new Vector2(250, 0), ImGuiChildFlags.Border);

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

            ImGui.SameLine();

            ImGui.BeginChild("RightPanel", new Vector2(0, 0), ImGuiChildFlags.Border);

            var selectedLayout = _layouts.Find(l => l.m_id == _focusLayoutId);
            if (selectedLayout != null)
            {
                ImGui.Text($"Layout: {selectedLayout.m_title ?? "Untitled"}");
                selectedLayout.m_title ??= "";
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
                        float width = ImGui.GetContentRegionAvail().x / 3f;
                        ImGui.SetNextItemWidth(width);
                        ImGui.InputText($"Key##{zoneId}", ref zone.Key, 32);

                        var types = Enum.GetNames(typeof(LayoutType.LayoutValueType));
                        int currentTypeIndex = (int)zone.TypeEnum;
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(width);
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0, 0, 0, 1));
                        ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(0.08f, 0.08f, 0.08f, 1));
                        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
                        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6, 4));

                        float width2 = ImGui.GetContentRegionAvail().x;
                        ImGui.SetNextItemWidth(width2);
                        if (ImGui.Combo($"Type##{zoneId}", ref currentTypeIndex, types, types.Length))
                        {
                            zone.TypeEnum = (LayoutType.LayoutValueType)currentTypeIndex;
                            zone.Type = zone.TypeEnum.ToString();
                            if (zone.TypeEnum == LayoutType.LayoutValueType.Bool && string.IsNullOrEmpty(zone.Value))
                                zone.Value = "false";

                            if (zone.TypeEnum == LayoutType.LayoutValueType.Slider)
                            {
                                zone.SliderMin = 0f;
                                zone.SliderMax = 1f;
                            }
                            zone.Type = zone.TypeEnum.ToString();
                        }
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(width);

                        if (zone.TypeEnum == LayoutType.LayoutValueType.Slider)
                        {
                            ImGui.SetNextItemWidth(100);
                            ImGui.DragFloat($"Min##{zoneId}", ref zone.SliderMin, 0.1f);
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(100);
                            ImGui.DragFloat($"Max##{zoneId}", ref zone.SliderMax, 0.1f);

                            float value = 0f;
                            float.TryParse(zone.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
                            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x);
                            if (ImGui.SliderFloat($"{zone.Key}##{zoneId}", ref value, zone.SliderMin, zone.SliderMax))
                            {
                                zone.Value = value.ToString(CultureInfo.InvariantCulture);
                                _layoutsDirty = true;
                            }
                        }

                        else
                        {
                            ImGui.InputText($"value##{zoneId}", ref zone.Value, 32);
                        }

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
                        record.AddField(new DataModels.LayoutZone { Key = "NewZone", Type = "string", Value = "" });
                        _layoutsDirty = true;
                    }

                    ImGui.SameLine();
                    if (ImGui.Button($"DeleteTab##{recId}"))
                    {
                        _toRemoveRecords.Add((selectedLayout, record));
                        _layoutsDirty = true;
                    }

                    ImGui.SameLine();
                    if (ImGui.Button($"Open Window##open_{selectedLayout.m_id}_{record.m_guid}"))
                    {
                        var existing = _runtimeWindows.Find(w => w.m_layoutId == selectedLayout.m_id && w.m_recordGuid == record.m_guid);
                        if (existing == null)
                        {
                            var win = new DataModels.RuntimeWindow
                            {
                                m_windowID = _nextRuntimeWindowId++,
                                m_layoutId = selectedLayout.m_id,
                                m_recordGuid = record.m_guid,
                                m_title = $"{selectedLayout.m_title} - {record.m_name}",
                                m_open = true
                            };
                            _runtimeWindows.Add(win);
                            
                        }
                        else
                        {
                            existing.m_open = true;
                        }
                    }
                }

                ImGui.EndChild();

                if (ImGui.Button($"AddTab##addrec_{selectedLayout.m_id}"))
                {
                    var newrec = new DataModels.RecordData { m_name = "NewTab" };
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

            // re-open requests
            if (_toReopen.Count > 0)
            {
                foreach (var name in _toReopen)
                    ReopenLayouts(name);
                _toReopen.Clear();
            }
        }
        
        private void DrawLayoutButton(DataModels.LayoutData layout)
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

        private void DrawRuntimeWindows()
        {
            foreach (var win in _runtimeWindows.ToArray())
            {
                if (!win.m_open) continue;
                
                var layout = _layouts.Find(l => l.m_id == win.m_layoutId);
                if (layout == null) 
                { 
                    win.m_open = false;
                    continue;
                }
                var record = layout.m_records.Find(r => r.m_guid == win.m_recordGuid);
                if (record == null)
                {
                    win.m_open = false;
                    continue;
                }

                string windowId = $"{win.m_title}##runtime_{win.m_windowID}";

                if (!ImGui.Begin(windowId, ref win.m_open, ImGuiWindowFlags.None))
                {
                    ImGui.End();
                    continue;
                }
                
                ImGui.Text($"{layout.m_title} - {record.m_name}");

                for (int z = 0; z < record.FieldCount; z++)
                {
                    var zone = record.GetField(z);
                    if (zone == null) continue;
                    string ctlId = $"##runtime_{win.m_windowID}_f{z}";

                    FieldRenderer.DrawField(zone, ctlId);
                }
                
                ImGui.Separator();
                if (ImGui.Button($"Save##runtime_save_{win.m_windowID}"))
                {
                    _layoutsDirty = true;
                }
                ImGui.SameLine();
                if (ImGui.Button($"Close##runtime_close_{win.m_windowID}"))
                {
                    win.m_open = false;
                }
                
                ImGui.End();
                
                if (!win.m_open)
                    _runtimeWindows.RemoveAll(w => w.m_windowID ==  win.m_windowID);
            }
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
        
        #endregion


        #region Private and Protected

        private DataModels.LayoutContainer _container = new();
        private List<DataModels.LayoutData> _layouts = new ();
        private List<(DataModels.LayoutData layout, DataModels.RecordData record)> _toRemoveRecords = new();
        private List<DataModels.LayoutData> _toRemoveLayouts = new ();
        private List<string> _toRemove = new();
        private List<string> _toReopen = new();
        private List<DataModels.RuntimeWindow> _runtimeWindows = new ();
        private List<DataModels.InfoWindow> _infoWindows = new();
        private bool _layoutsDirty  = false;
        private string _savePath;
        private int _focusLayoutId = -1;
        private int _nextRuntimeWindowId = 1;
        private int _nextInfoWindowId = 1;

        #endregion
    }
}
