using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImGuiNET;
using Slate.Runtime;
using UnityEngine;

namespace WindowCreator.Runtime
{
    public class ImGuiLayoutManager : WindowBaseBehaviour
    {
        #region Public

        public string m_windowTitle = "Layout Management";
        public string m_newLayoutName = "New Layout";
        public List<DataModels.LayoutZone> m_layoutZones = new List<DataModels.LayoutZone>();

        #endregion
        

        #region Api Unity

        private void Awake()
        {
            _saveFolder = GetDatabaseFolder();

            _container ??= new DataModels.LayoutContainer();

            // Charger les layouts depuis les fichiers
            LoadLayoutsFromFiles();

            // Assurer que le container contient tous les layouts
            _container.m_layouts = _layouts ??= new List<DataModels.LayoutData>();

            // Assigner des IDs uniques aux layouts manquants et incrémenter nextId
            foreach (var layout in _container.m_layouts)
            {
                if (layout.m_id == 0)
                    layout.m_id = _container.m_nextId++;
                else
                    _container.m_nextId = Math.Max(_container.m_nextId, layout.m_id + 1);
            }

            // Focus sur le premier layout si aucun n'est sélectionné
            if (_container.m_layouts.Count > 0 && !_container.m_layouts.Any(l => l.m_id == _focusLayoutId))
                _focusLayoutId = _container.m_layouts[0].m_id;

            // Initialiser les records
            foreach (var layout in _container.m_layouts)
            foreach (var rec in layout.m_records)
                rec.EnsureInitialized();
        }

        protected override void OnDisable()
        {
            if (_layoutsDirty)
                SaveAllLayouts();
        }

        private void OnApplicationQuit()
        {
            if (_layoutsDirty)
                SaveAllLayouts();
        }

        protected override void WindowLayout()
        {
            ProcessPendingRemovals();

            m_newLayoutName ??= string.Empty;

            ImGui.SetNextWindowSize(new Vector2(800, 800), ImGuiCond.FirstUseEver);
            ImGui.Begin(m_windowTitle);

            DrawLeftPanel();
            ImGui.SameLine();
            DrawRightPanel();

            ImGui.End();
        }

        #endregion

        
        #region Utils
        
        private void DrawLeftPanel()
        {
            ImGui.BeginChild("LeftPanel", new Vector2(400, 0), ImGuiChildFlags.Border);
            ImGui.Text("Window :");
            ImGui.Separator();

            foreach (var layout in _layouts)
            {
                foreach (var rec in layout.m_records)
                    rec.EnsureInitialized();
                
                bool isSelected = (_focusLayoutId == layout.m_id);
                layout.m_title ??= string.Empty;
                
                ImGui.PushID(layout.m_id); // garantit unicité des widgets

                // Bouton delete
                if (ImGui.SmallButton("X"))
                {
                    RemoveWindow(layout.m_id);
                    ImGui.PopID();
                    ImGui.EndChild();
                    _layoutsDirty = true;
                    return;
                }

                ImGui.SameLine();
                ImGui.PopID();

                if (ImGui.Selectable($"{layout.m_title}", isSelected))
                    _focusLayoutId = layout.m_id;
            }

            ImGui.Spacing();
            ImGui.Separator();
            Layout("Window Name : ", 150,150);
            ImGui.InputText("##New_Window", ref m_newLayoutName, 64);
            if (ImGui.Button(" + Create Window"))
                CreateNewLayout(m_newLayoutName);
            
            ImGui.SameLine();
            if (ImGui.Button("Save"))
                SaveAllLayouts();

            ImGui.EndChild();
            ImGui.SameLine();
        }

        private void DrawRightPanel()
        {
            ImGui.BeginChild("RightPanel", new Vector2(0, 0), ImGuiChildFlags.Border);

            var selectedLayout = _layouts.Find(l => l.m_id == _focusLayoutId);
            if (selectedLayout == null)
            {
                ImGui.TextDisabled("Select a window on the left");
                ImGui.EndChild();
                return;
            }

            float panelWidth = ImGui.GetContentRegionAvail().x;
            float minWidthLeft = 150f;
            float minWidthCenter = 200f;
            float minWidthRight = 200f;
            
            // Clamp les colonnes à la largeur du panel
            float totalWidth = _colLeftWidth + _colCenterWidth + _colRightWidth + _splitterSize * 2;
            if (totalWidth > panelWidth)
            {
                float scale = (panelWidth - 2 * _splitterSize) / (_colLeftWidth + _colCenterWidth + _colRightWidth);
                _colLeftWidth *= scale;
                _colCenterWidth *= scale;
                _colRightWidth *= scale;
            }

            // ---------- Record Selector ----------
            ImGui.BeginChild("RecordSelector", new Vector2(_colLeftWidth, 0), ImGuiChildFlags.Border);
            DrawRecordSelector(selectedLayout);
            ImGui.EndChild();

            ImGui.SameLine();
            Splitter(ref _colLeftWidth, ref _colCenterWidth);

            // ---------- Record Editor Left ----------
            ImGui.SameLine();
            ImGui.BeginChild("RecordEditorLeft", new Vector2(_colCenterWidth, 0), ImGuiChildFlags.Border);
            var recordLeft = selectedLayout.m_records.FirstOrDefault(r => r.m_guid == _focusRecordsGuidLeft);
            if (recordLeft != null)
                DrawRecordEditor(recordLeft, true);
            ImGui.EndChild();

            ImGui.SameLine();
            Splitter(ref _colCenterWidth, ref _colRightWidth);

            // ---------- Record Editor Right ----------
            ImGui.SameLine();
            ImGui.BeginChild("RecordEditorRight", new Vector2(_colRightWidth, 0), ImGuiChildFlags.Border);
            var recordRight = selectedLayout.m_records.FirstOrDefault(r => r.m_guid == _focusRecordsGuidRight);
            if (recordRight != null)
                DrawRecordEditor(recordRight, false);
            ImGui.EndChild();

            ImGui.EndChild();
        }

        private void DrawRecordSelector(DataModels.LayoutData selectedLayout)
        {
            ImGui.Text("Records");
            ImGui.Separator();

            for (int i = 0; i < selectedLayout.m_records.Count; i++)
            {
                var rec = selectedLayout.m_records[i];
                bool selected = _focusRecordsGuidLeft == rec.m_guid;
                
                // Bouton delete
                if (ImGui.SmallButton("X"))
                {
                    selectedLayout.m_records.RemoveAt(i);

                    if (_focusRecordsGuidLeft == rec.m_guid)
                        _focusRecordsGuidLeft = selectedLayout.m_records.Count > 0 ? selectedLayout.m_records[0].m_guid : null;

                    if (_focusRecordsGuidRight == rec.m_guid)
                        _focusRecordsGuidRight = selectedLayout.m_records.Count > 0 ? selectedLayout.m_records[0].m_guid : null;

                    _layoutsDirty = true;
                    ImGui.PopID();
                    break; // sortir de la boucle pour éviter l'erreur d'itération
                }
                
                ImGui.SameLine();
                ImGui.PopID();

                if (ImGui.Selectable($"{rec.m_name}", selected))
                {
                    _focusRecordsGuidLeft = rec.m_guid;
                    _focusRecordsGuidRight = rec.m_guid;
                }
            }

            ImGui.Separator();
            ImGui.Text("New Record Name");
            _newRecordName ??= "";
            ImGui.InputText("", ref _newRecordName, 32);

            if (ImGui.Button("+ Add Record"))
            {
                if (string.IsNullOrWhiteSpace(_newRecordName))
                    _newRecordName = "New Record";

                var newRec = new DataModels.RecordData { m_name = _newRecordName };
                selectedLayout.m_records.Add(newRec);

                _focusRecordsGuidLeft = newRec.m_guid;
                _focusRecordsGuidRight = newRec.m_guid;
                _newRecordName = "";
                _layoutsDirty = true;
            }
        }

        private void DrawRecordEditor(DataModels.RecordData record, bool isLeft)
        {
            if (record == null) return;
            string recId = $"rec_{record.m_guid}";
            record.m_name ??= "";

            var colData = isLeft ? record.LeftColumn : record.RightColumn;

            ImGui.Separator();

            foreach (var param in colData.GeneralParameters.ToList())
            {
                param.m_key ??= "";
                param.m_value ??= "";
                param.m_type ??= "String";

                // ---------- General Parameter Name ----------
                ImGui.Text("General Parameter Name:");
                ImGui.InputText($"##ParamName_{param.m_guid}_{recId}", ref param.m_key, 32);

                ImGui.SameLine();
                if (ImGui.Button($"X##DelParam_{param.m_guid}_{recId}"))
                {
                    colData.GeneralParameters.Remove(param);
                    _layoutsDirty = true;
                    break;
                }

                ImGui.Separator();

                // ---------- Fields ----------
                foreach (var field in param.m_fields.ToList())
                {
                    field.m_key ??= "";
                    field.m_value ??= "";
                    field.m_type ??= "String";

                    Layout("Field Name:", 100, 150);
                    ImGui.InputText($"##FieldKey_{field.m_guid}_{recId}", ref field.m_key, 32);

                    // Type dropdown
                    ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(0.2f,0.2f,0.3f,1f));
                    string[] types = Enum.GetNames(typeof(LayoutType.LayoutValueType));
                    int currentTypeIndex = Array.IndexOf(types, field.m_type);
                    Layout("Type Name:", 100, 150);
                    if (ImGui.Combo($"##FieldType_{field.m_guid}_{recId}", ref currentTypeIndex, types, types.Length))
                        field.m_type = types[currentTypeIndex];
                    
                    ImGui.PopStyleColor();

                    // Value input depending on type
                    if (!Enum.TryParse(field.m_type, out LayoutType.LayoutValueType typeEnum))
                        typeEnum = LayoutType.LayoutValueType.String;

                    switch (typeEnum)
                    {
                        case LayoutType.LayoutValueType.String:
                            Layout("Value:", 100, 150);
                            ImGui.InputText($"##FieldValue_{field.m_guid}_{recId}", ref field.m_value, 32);
                            break;
                        case LayoutType.LayoutValueType.Int:
                            int intVal = 0;
                            int.TryParse(field.m_value, out intVal);
                            Layout("Value:", 100, 150);
                            ImGui.InputInt($"##FieldValue_{field.m_guid}_{recId}", ref intVal);
                            field.m_value = intVal.ToString();
                            break;
                        case LayoutType.LayoutValueType.Float:
                            float floatVal = 0f;
                            float.TryParse(field.m_value, out floatVal);
                            Layout("Value:", 100, 150);
                            ImGui.InputFloat($"##FieldValue_{field.m_guid}_{recId}", ref floatVal, 0f, 100f);
                            field.m_value = floatVal.ToString();
                            break;
                        case LayoutType.LayoutValueType.Bool:
                            bool boolVal = field.m_value == "true";
                            ImGui.Checkbox($"##FieldValue_{field.m_guid}_{recId}", ref boolVal);
                            field.m_value = boolVal ? "true" : "false";
                            break;
                        case LayoutType.LayoutValueType.Slider:
                            DrawSlider($"{field.m_guid}_{recId}", field);
                            float currentVal = 0f;
                            float.TryParse(field.m_value, out currentVal);
                            currentVal = Mathf.Clamp(currentVal, field.m_sliderMin, field.m_sliderMax);
                            ImGui.SetNextItemWidth(200);
                            ImGui.SliderFloat($"##SliderFloat_{field.m_guid}_{recId}", ref currentVal, field.m_sliderMin, field.m_sliderMax);
                            field.m_value = currentVal.ToString();
                            break;
                    }

                    ImGui.SameLine();
                    if (ImGui.Button($"X##removeField_{field.m_guid}_{recId}"))
                    {
                        param.m_fields.Remove(field);
                        _layoutsDirty = true;
                        break;
                    }
                }

                ImGui.Separator();
                if (ImGui.Button($"+ Add Field##AddField_{param.m_guid}_{recId}"))
                {
                    param.m_fields.Add(new DataModels.LayoutZone
                    {
                        m_guid = Guid.NewGuid().ToString(),
                        m_key = "NewField",
                        m_value = "",
                        m_type = "String"
                    });
                    _layoutsDirty = true;
                }
            }

            ImGui.Separator();
            if (ImGui.Button($"+ Add General Parameter##AddGen_{recId}_{(isLeft ? "L" : "R")}"))
            {
                colData.GeneralParameters.Add(new DataModels.LayoutZone
                {
                    m_guid = Guid.NewGuid().ToString(),
                    m_key = "NewParam",
                    m_value = "",
                    m_type = "String",
                    m_fields = new List<DataModels.LayoutZone>()
                });
                _layoutsDirty = true;
            }
        }


        private void DrawSlider(string zoneId, DataModels.LayoutZone zone)
        {
            ImGui.BeginGroup();

            ImGui.Text("Min :");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            ImGui.InputFloat($"##Min_{zoneId}", ref zone.m_sliderMin, 0.1f);
            
            ImGui.Text("Max :");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            ImGui.InputFloat($"##Max_{zoneId}", ref zone.m_sliderMax, 0.1f);

            ImGui.EndGroup();

            // Correction automatique si Max < Min
            if (zone.m_sliderMax < zone.m_sliderMin)
                zone.m_sliderMax = zone.m_sliderMin + 0.0001f;
        }
        private void CreateNewLayout(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "New Layout";

            var newLayout = new DataModels.LayoutData
            {
                m_id = _container.m_nextId++, // Assigner un ID unique
                m_title = name,
                m_name = name // Optionnel : pour avoir un identifiant interne cohérent
            };
            _layouts.Add(newLayout);
            _focusLayoutId = newLayout.m_id;
            _layoutsDirty = true;
            
            SaveLayoutToFile(newLayout);
        }

        private void ProcessPendingRemovals()
        {
            if (_toRemoveRecords.Count == 0) return;

            foreach (var entry in _toRemoveRecords)
                entry.layout?.m_records?.Remove(entry.record);

            _toRemoveRecords.Clear();
            _layoutsDirty = true;
        }

        private void Splitter(ref float left, ref float right)
        {
            ImGui.InvisibleButton("##splitter", new Vector2(_splitterSize, -1f));
            if (ImGui.IsItemActive())
            {
                float delta = ImGui.GetIO().MouseDelta.x;
                left += delta;
                right -= delta;

                if (left < 120) { right += (left - 120); left = 120; }
                if (right < 120) { left += (right - 120); right = 120; }
            }
        }

        private string GetDatabaseFolder()
        {
#if UNITY_EDITOR
            string root;
            root = Application.dataPath;
            root = Directory.GetParent(root).FullName;
            root = Directory.GetParent(root).FullName;
#else
            string root = Application.persistentDataPath;
#endif
            string targetDir = Path.Combine(root, "Latest", "data", "database");
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);
            return targetDir;
        }

        private string GetLayoutPath(string layoutName)
        {
            string safeName = string.Concat(layoutName.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_saveFolder, safeName + ".json");
        }

        private void RemoveWindow(int layoutId)
        {
            var toDelete = _layouts.Find(l => l.m_id == layoutId);
            if (toDelete == null)
                return;
            
            // Supprimer le fichier JSON associé
            string path = GetLayoutPath(toDelete.m_title);
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    Info($"Failed to delete layout file {path}: {e}");
                }
            }
            
            _layouts.Remove(toDelete);
            
            if (_focusLayoutId == layoutId)
                _focusLayoutId = _layouts.Count > 0 ? _layouts[0].m_id : -1;
        }

        #endregion
        

        #region Helpers

        private static void Layout(string name, float labelWidth, float inputWidth)
        {
            ImGui.Text($"{name}");
            ImGui.SameLine(labelWidth);
            ImGui.SetNextItemWidth(inputWidth);
        }

        private void LoadLayoutsFromFiles()
        {
            _layouts = new List<DataModels.LayoutData>();

            if (!Directory.Exists(_saveFolder)) return;

            foreach (var file in Directory.GetFiles(_saveFolder, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var layout = JsonUtility.FromJson<DataModels.LayoutData>(json);
                    if (layout != null)
                        _layouts.Add(layout);
                }
                catch (Exception e)
                {
                    InfoDone($"Failed to load layout file {file}: {e}");
                }
            }
        }
        
        private void SaveLayoutToFile(DataModels.LayoutData layout)
        {
            string path = GetLayoutPath(layout.m_title);
            try
            {
                string json = JsonUtility.ToJson(layout, true);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Info($"Failed to save layout {layout.m_title}: {e}");
            }
        }

        private void SaveAllLayouts()
        {
            foreach (var layout in _layouts)
                SaveLayoutToFile(layout);

            _layoutsDirty = false;
        }

        #endregion
        

        #region Private and Protected

        private DataModels.LayoutContainer _container = new();
        private List<DataModels.LayoutData> _layouts = new();
        private List<(DataModels.LayoutData layout, DataModels.RecordData record)> _toRemoveRecords = new();
        private bool _layoutsDirty;
        private string _savePath;
        private int _focusLayoutId = -1;
        private string _saveFolder;
        private string[] _enumTypeNames = Enum.GetNames(typeof(LayoutType.LayoutValueType));

        // Record Fields
        private string _focusRecordsGuidLeft;
        private string _focusRecordsGuidRight;
        private string _newRecordName;

        // Column size
        private float _colLeftWidth = 200f;
        private float _colCenterWidth = 350f;
        private float _colRightWidth = 350f;
        private const float _splitterSize = 5f;

        #endregion


    }
}
