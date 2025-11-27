using System;
using System.Collections.Generic;
using System.IO;
using ImGuiNET;
using SharedData.Runtime;
using Slate.Runtime;
using UnityEngine;

namespace WindowCreator.Runtime
{
    /// <summary>
    /// ImGuiLayoutManager
    /// ------------------
    /// Outil runtime/build permettant de créer et éditer des Layouts (fenêtres),
    /// Records (onglets), colonnes (gauche/droite), paramètres généraux et champs.
    /// Les données sont sérialisées en JSON dans un dossier "database".
    ///
    /// Dépend de WindowBaseBehaviour pour :
    /// - déplacement libre des fenêtres
    /// - masquage/affichage (tools)
    /// - logging (Info / InfoDone)
    /// </summary>
    
    [SlateWindow(categoryName = "Tools", entry = "WindowCreator")]
    public class ImGuiLayoutManager : WindowBaseBehaviour
    {
        #region Publics

        [Header("Window")]
        public string m_windowTitle = "Layout Management";

        [Header("Creation")]
        public string m_newLayoutName = "New Layout";
        
        #endregion


        #region API Unity (Awake, Start, Update, etc.)

        private void Awake()
        {
            // Singleton simple pour LayoutType (EnumCustom, DatabaseRef, etc.)
            _instance = this;

            _saveFolder = GetDatabaseFolder();

            if (_container == null)
                _container = new DataModels.LayoutContainer();

            LoadLayoutsFromFiles();

            if (_layouts == null)
                _layouts = new List<DataModels.LayoutData>();

            _container.m_layouts = _layouts;

            // Assure la cohérence des IDs de fenêtres et des records
            for (int i = 0; i < _container.m_layouts.Count; i++)
            {
                var layout = _container.m_layouts[i];
                if (layout == null)
                    continue;

                // ID de fenêtre
                if (layout.m_id == 0)
                    layout.m_id = _container.m_nextId++;
                else
                    _container.m_nextId = Mathf.Max(_container.m_nextId, layout.m_id + 1);

                // Records
                if (layout.m_records == null)
                    layout.m_records = new List<DataModels.RecordData>();

                // Nettoyage des null et initialisation
                var cleanRecords = new List<DataModels.RecordData>();
                for (int r = 0; r < layout.m_records.Count; r++)
                {
                    var rec = layout.m_records[r];
                    if (rec == null)
                        continue;

                    // S'assure que le record connaît sa fenêtre parente
                    if (rec.m_parentLayoutId == 0)
                        rec.m_parentLayoutId = layout.m_id;

                    rec.EnsureInitialized();
                    cleanRecords.Add(rec);
                }

                layout.m_records = cleanRecords;
            }

            // Focus par défaut sur une fenêtre valide
            if (_container.m_layouts.Count > 0)
            {
                bool idStillValid = false;
                for (int i = 0; i < _container.m_layouts.Count; i++)
                {
                    var layout = _container.m_layouts[i];
                    if (layout != null && layout.m_id == _focusLayoutId)
                    {
                        idStillValid = true;
                        break;
                    }
                }

                if (!idStillValid)
                    _focusLayoutId = _container.m_layouts[0].m_id;
            }

            // Focus record par défaut si possible
            DataModels.LayoutData firstLayout = null;
            for (int i = 0; i < _layouts.Count; i++)
            {
                if (_layouts[i] != null && _layouts[i].m_id == _focusLayoutId)
                {
                    firstLayout = _layouts[i];
                    break;
                }
            }

            if (firstLayout != null && firstLayout.m_records != null && firstLayout.m_records.Count > 0)
            {
                _focusRecordGuidLeft = firstLayout.m_records[0].m_guid;
                _focusRecordGuidRight = firstLayout.m_records[0].m_guid;
            }
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
            // Préparation / nettoyage
            m_newLayoutName = m_newLayoutName ?? string.Empty;

            ImGui.SetNextWindowSize(new Vector2(1200, 900), ImGuiCond.FirstUseEver);
            if (!ImGui.Begin(m_windowTitle))
            {
                ImGui.End();
                return;
            }

            // ----- Colonne 1 : Windows -----
            DrawLeftPanel();
            ImGui.SameLine();
            Splitter("split_windows", ref _colWindowsWidth, ref _colSelectorWidth, _minWindowsWidth, _minSelectorWidth);

            // Pré-calcul des largeurs dispos pour colonnes 2-3-4
            EnsureFourColumnsWidthsFitWindow();

            // ----- Colonne 2 : Record Selector -----
            ImGui.SameLine();
            DrawRecordSelectorPanel();
            ImGui.SameLine();
            Splitter("split_selector_left", ref _colSelectorWidth, ref _colEditorLeftWidth, _minSelectorWidth, _minEditorWidth);

            // ----- Colonne 3 : Editor Left -----
            ImGui.SameLine();
            DrawEditorLeft();
            ImGui.SameLine();
            Splitter("split_left_right", ref _colEditorLeftWidth, ref _colEditorRightWidth, _minEditorWidth, _minEditorWidth);

            // ----- Colonne 4 : Editor Right (flex) -----
            ImGui.SameLine();
            DrawEditorRight();

            ImGui.End();
        }

        #endregion


        #region Utils (méthodes publics)

        /// <summary>
        /// Donne un ID unique pour un Record dans une fenêtre donnée.
        /// Utilisé par DataModels.RecordData.EnsureInitialized().
        /// </summary>
        public static int GetNextRecordIDForWindow(int layoutId)
        {
            if (_instance == null)
                return 0;

            if (_instance._nextRecordIdPerLayout == null)
                _instance._nextRecordIdPerLayout = new Dictionary<int, int>();

            if (!_instance._nextRecordIdPerLayout.ContainsKey(layoutId))
                _instance._nextRecordIdPerLayout[layoutId] = 1;

            int id = _instance._nextRecordIdPerLayout[layoutId];
            _instance._nextRecordIdPerLayout[layoutId]++;

            return id;
        }

        /// <summary>
        /// Accès singleton (utilisé par LayoutType).
        /// </summary>
        public static ImGuiLayoutManager Instance()
        {
            return _instance;
        }

        /// <summary>
        /// Retourne la liste des titres de fenêtres existantes.
        /// </summary>
        public List<string> GetAllLayoutTitles()
        {
            var result = new List<string>();

            if (_layouts == null)
                return result;

            for (int i = 0; i < _layouts.Count; i++)
            {
                var layout = _layouts[i];
                if (layout == null)
                    continue;

                if (!string.IsNullOrEmpty(layout.m_title) && !result.Contains(layout.m_title))
                    result.Add(layout.m_title);
            }

            return result;
        }

        /// <summary>
        /// Retourne les records d'une fenêtre par son titre.
        /// </summary>
        public List<DataModels.RecordData> GetRecordsForWindow(string windowTitle)
        {
            var result = new List<DataModels.RecordData>();

            if (string.IsNullOrEmpty(windowTitle) || _layouts == null)
                return result;

            for (int i = 0; i < _layouts.Count; i++)
            {
                var layout = _layouts[i];
                if (layout == null || layout.m_title != windowTitle)
                    continue;

                if (layout.m_records != null)
                    result.AddRange(layout.m_records);

                break;
            }

            return result;
        }

        /// <summary>
        /// Pour EnumCustom : retourne la liste des fenêtres utilisables comme Enum Window.
        /// Actuellement : toutes les fenêtres ayant m_isEnumWindow == true.
        /// </summary>
        public List<string> GetEnumWindowTitles()
        {
            var result = new List<string>();

            if (_layouts == null)
                return result;

            for (int i = 0; i < _layouts.Count; i++)
            {
                var layout = _layouts[i];
                if (layout == null)
                    continue;

                if (layout.m_isEnumWindow && !string.IsNullOrEmpty(layout.m_title) && !result.Contains(layout.m_title))
                    result.Add(layout.m_title);
            }

            return result;
        }

        /// <summary>
        /// Pour EnumCustom : retourne les records d’une fenêtre Enum.
        /// </summary>
        public List<DataModels.RecordData> GetEnumRecords(string enumWindowTitle)
        {
            var result = new List<DataModels.RecordData>();

            if (string.IsNullOrEmpty(enumWindowTitle) || _layouts == null)
                return result;

            DataModels.LayoutData layout = null;
            for (int i = 0; i < _layouts.Count; i++)
            {
                var l = _layouts[i];
                if (l != null && l.m_title == enumWindowTitle)
                {
                    layout = l;
                    break;
                }
            }

            if (layout == null || layout.m_records == null)
                return result;

            // Nettoyage et EnsureInitialized
            for (int i = 0; i < layout.m_records.Count; i++)
            {
                var rec = layout.m_records[i];
                if (rec == null)
                    continue;

                // S'assure du parent pour GetNextRecordIDForWindow si besoin
                if (rec.m_parentLayoutId == 0)
                    rec.m_parentLayoutId = layout.m_id;

                rec.EnsureInitialized();
                result.Add(rec);
            }

            return result;
        }

        /// <summary>
        /// Pour EnumCustom :
        /// - Dans une Enum Window
        /// - record = un enum (Rarity, Classe, ...)
        /// - options = Standalone Fields (Left + Right)
        ///   On prend m_key si non vide, sinon m_value.
        /// </summary>
        public List<string> GetEnumOptions(string enumWindowTitle, string enumRecordName)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(enumWindowTitle) ||
                string.IsNullOrEmpty(enumRecordName) ||
                _layouts == null)
                return result;

            DataModels.LayoutData layout = null;
            for (int i = 0; i < _layouts.Count; i++)
            {
                var l = _layouts[i];
                if (l != null && l.m_title == enumWindowTitle)
                {
                    layout = l;
                    break;
                }
            }

            if (layout == null || layout.m_records == null)
                return result;

            DataModels.RecordData record = null;
            for (int i = 0; i < layout.m_records.Count; i++)
            {
                var r = layout.m_records[i];
                if (r != null && r.m_name == enumRecordName)
                {
                    record = r;
                    break;
                }
            }

            if (record == null)
                return result;

            Action<DataModels.ColumnData> collect = delegate(DataModels.ColumnData col)
            {
                if (col == null || col.Fields == null)
                    return;

                for (int i = 0; i < col.Fields.Count; i++)
                {
                    var f = col.Fields[i];
                    if (f == null)
                        continue;

                    string label = string.IsNullOrEmpty(f.m_key) ? f.m_value : f.m_key;
                    if (!string.IsNullOrEmpty(label) && !result.Contains(label))
                        result.Add(label);
                }
            };

            collect(record.LeftColumn);
            collect(record.RightColumn);

            return result;
        }

        /// <summary>
        /// Permet à un collègue de montrer / cacher la fenêtre.
        /// </summary>
        public void SetWindowVisible(bool visible)
        {
            enabled = visible;
        }

        #endregion


        #region Main Methods (méthodes private)

        private void DrawLeftPanel()
        {
            ImGui.BeginChild("LeftPanel", new Vector2(_colWindowsWidth, 0f), ImGuiChildFlags.Border);
            ImGui.Text("Layouts / Windows");
            ImGui.Separator();

            if (_layouts == null)
                _layouts = new List<DataModels.LayoutData>();

            // Liste des fenêtres
            for (int i = 0; i < _layouts.Count; i++)
            {
                var layout = _layouts[i];
                if (layout == null)
                    continue;

                if (layout.m_title == null)
                    layout.m_title = string.Empty;

                bool isSelected = (_focusLayoutId == layout.m_id);

                ImGui.PushID(layout.m_id);

                // Bouton delete
                if (ImGui.SmallButton("X"))
                {
                    RemoveWindow(layout.m_id);
                    _layoutsDirty = true;
                    ImGui.PopID();
                    ImGui.EndChild();
                    return;
                }

                ImGui.SameLine();

                if (ImGui.Selectable(layout.m_title, isSelected))
                {
                    _focusLayoutId = layout.m_id;

                    if (layout.m_records == null)
                        layout.m_records = new List<DataModels.RecordData>();

                    if (layout.m_records.Count > 0 && layout.m_records[0] != null)
                    {
                        _focusRecordGuidLeft = layout.m_records[0].m_guid;
                        _focusRecordGuidRight = layout.m_records[0].m_guid;
                    }
                    else
                    {
                        _focusRecordGuidLeft = null;
                        _focusRecordGuidRight = null;
                    }
                }

                ImGui.PopID();
            }

            ImGui.Spacing();
            ImGui.Separator();

            // Création d'une nouvelle fenêtre
            Layout("Window Name:", 140, 180);
            ImGui.InputText("##NewLayoutName", ref m_newLayoutName, 64);

            if (ImGui.Button("+ Create Window"))
                CreateNewLayout(m_newLayoutName);

            ImGui.SameLine();
            if (ImGui.Button("Save All"))
                SaveAllLayouts();

            // Section Enum Window pour la fenêtre sélectionnée
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Text("Enum Settings");

            DataModels.LayoutData current = null;
            for (int i = 0; i < _layouts.Count; i++)
            {
                var layout = _layouts[i];
                if (layout != null && layout.m_id == _focusLayoutId)
                {
                    current = layout;
                    break;
                }
            }

            if (current != null)
            {
                bool isEnum = current.m_isEnumWindow;
                if (ImGui.Checkbox("Is Enum Window", ref isEnum))
                {
                    current.m_isEnumWindow = isEnum;
                    _layoutsDirty = true;
                }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("If enabled, this window can be used by EnumCustom fields\nas a source of custom enums.");
            }
            else
            {
                ImGui.TextDisabled("No selected window.");
            }

            ImGui.EndChild();
        }

        private void DrawRecordSelectorPanel()
        {
            DataModels.LayoutData selectedLayout = null;
            for (int i = 0; i < _layouts.Count; i++)
            {
                var layout = _layouts[i];
                if (layout != null && layout.m_id == _focusLayoutId)
                {
                    selectedLayout = layout;
                    break;
                }
            }

            ImGui.BeginChild("RecordSelectorPanel", new Vector2(_colSelectorWidth, 0f), ImGuiChildFlags.Border);

            if (selectedLayout == null)
            {
                ImGui.TextDisabled("Select a Layout.");
                ImGui.EndChild();
                return;
            }

            DrawRecordSelector(selectedLayout);
            ImGui.EndChild();
        }

        private void DrawEditorLeft()
        {
            DataModels.LayoutData selectedLayout = null;
            for (int i = 0; i < _layouts.Count; i++)
            {
                var layout = _layouts[i];
                if (layout != null && layout.m_id == _focusLayoutId)
                {
                    selectedLayout = layout;
                    break;
                }
            }

            ImGui.BeginChild("EditorLeft", new Vector2(_colEditorLeftWidth, 0f), ImGuiChildFlags.Border);

            if (selectedLayout == null)
            {
                ImGui.TextDisabled("No Layout selected.");
                ImGui.EndChild();
                return;
            }

            if (selectedLayout.m_records == null)
                selectedLayout.m_records = new List<DataModels.RecordData>();

            DataModels.RecordData record = null;
            for (int i = 0; i < selectedLayout.m_records.Count; i++)
            {
                var r = selectedLayout.m_records[i];
                if (r != null && r.m_guid == _focusRecordGuidLeft)
                {
                    record = r;
                    break;
                }
            }

            if (record == null)
            {
                ImGui.TextDisabled("Select a Record.");
                ImGui.EndChild();
                return;
            }

            DrawRecordEditor(record, true);
            ImGui.EndChild();
        }

        private void DrawEditorRight()
        {
            DataModels.LayoutData selectedLayout = null;
            for (int i = 0; i < _layouts.Count; i++)
            {
                var layout = _layouts[i];
                if (layout != null && layout.m_id == _focusLayoutId)
                {
                    selectedLayout = layout;
                    break;
                }
            }

            ImGui.BeginChild("EditorRight", new Vector2(_colEditorRightWidth, 0f), ImGuiChildFlags.Border);

            if (selectedLayout == null)
            {
                ImGui.TextDisabled("No Layout selected.");
                ImGui.EndChild();
                return;
            }

            if (selectedLayout.m_records == null)
                selectedLayout.m_records = new List<DataModels.RecordData>();

            DataModels.RecordData record = null;
            for (int i = 0; i < selectedLayout.m_records.Count; i++)
            {
                var r = selectedLayout.m_records[i];
                if (r != null && r.m_guid == _focusRecordGuidRight)
                {
                    record = r;
                    break;
                }
            }

            if (record == null)
            {
                ImGui.TextDisabled("Select a Record.");
                ImGui.EndChild();
                return;
            }

            DrawRecordEditor(record, false);
            ImGui.EndChild();
        }

        private void DrawRecordSelector(DataModels.LayoutData selectedLayout)
        {
            ImGui.Text("Records / Tabs");
            ImGui.Separator();

            if (selectedLayout.m_records == null)
                selectedLayout.m_records = new List<DataModels.RecordData>();

            for (int i = 0; i < selectedLayout.m_records.Count; i++)
            {
                var rec = selectedLayout.m_records[i];
                if (rec == null)
                    continue;

                rec.EnsureInitialized();

                bool selected = (_focusRecordGuidLeft == rec.m_guid);

                ImGui.PushID(rec.m_guid);

                if (ImGui.SmallButton("X"))
                {
                    selectedLayout.m_records.RemoveAt(i);

                    if (selectedLayout.m_records.Count > 0 && selectedLayout.m_records[0] != null)
                    {
                        if (_focusRecordGuidLeft == rec.m_guid)
                            _focusRecordGuidLeft = selectedLayout.m_records[0].m_guid;

                        if (_focusRecordGuidRight == rec.m_guid)
                            _focusRecordGuidRight = selectedLayout.m_records[0].m_guid;
                    }
                    else
                    {
                        if (_focusRecordGuidLeft == rec.m_guid)
                            _focusRecordGuidLeft = null;

                        if (_focusRecordGuidRight == rec.m_guid)
                            _focusRecordGuidRight = null;
                    }

                    _layoutsDirty = true;
                    ImGui.PopID();
                    break;
                }

                ImGui.SameLine();

                if (ImGui.Selectable(rec.m_name, selected))
                {
                    _focusRecordGuidLeft = rec.m_guid;
                    _focusRecordGuidRight = rec.m_guid;
                }

                ImGui.PopID();
            }

            ImGui.Separator();
            ImGui.Text("New Actor Name");

            if (_newRecordName == null)
                _newRecordName = string.Empty;

            ImGui.InputText("##NewRecordName", ref _newRecordName, 32);

            if (ImGui.Button("+ Add Record"))
            {
                if (string.IsNullOrWhiteSpace(_newRecordName))
                    _newRecordName = "New Record";

                var newRec = new DataModels.RecordData();
                newRec.m_name = _newRecordName;
                newRec.m_parentLayoutId = selectedLayout.m_id;
                newRec.EnsureInitialized();

                selectedLayout.m_records.Add(newRec);

                _focusRecordGuidLeft = newRec.m_guid;
                _focusRecordGuidRight = newRec.m_guid;

                _newRecordName = string.Empty;
                _layoutsDirty = true;
            }
        }

        private void DrawRecordEditor(DataModels.RecordData record, bool isLeft)
        {
            if (record == null)
                return;

            if (record.m_name == null)
                record.m_name = string.Empty;

            DataModels.ColumnData colData = isLeft ? record.LeftColumn : record.RightColumn;
            if (colData == null)
                colData = new DataModels.ColumnData();

            if (colData.GeneralParameters == null)
                colData.GeneralParameters = new List<DataModels.LayoutZone>();
            if (colData.Fields == null)
                colData.Fields = new List<DataModels.LayoutZone>();

            // -------- General Parameters --------
            ImGui.Text("Category");
            ImGui.Separator();

            // Copie locale pour éviter modification pendant l'itération
            var listParams = new List<DataModels.LayoutZone>(colData.GeneralParameters);

            for (int i = 0; i < listParams.Count; i++)
            {
                var param = listParams[i];
                if (param == null)
                    continue;

                EnsureZoneDefaults(param);
                ImGui.PushID(param.m_guid);

                bool collapsed = GetCollapsedState(param.m_guid);

                // ▶ / ▼ + nom du paramètre
                if (ImGui.SmallButton((collapsed ? "<##col" : ">##col") + param.m_key))
                    SetCollapsedState(param.m_guid, !collapsed);

                // Nom éditable
                ImGui.SameLine();
                ImGui.SetNextItemWidth(180);
                string prevParamKey = param.m_key;
                ImGui.InputText("##ParamName", ref param.m_key, 64);
                if (prevParamKey != param.m_key)
                    _layoutsDirty = true;

                ImGui.SameLine();
                if (ImGui.Button("Remove"))
                {
                    colData.GeneralParameters.Remove(param);
                    _layoutsDirty = true;
                    ImGui.PopID();
                    continue;
                }

                ImGui.Separator();

                if (!collapsed)
                {
                    DrawFieldsList(param);

                    if (ImGui.Button("+ Add Field"))
                    {
                        if (param.m_fields == null)
                            param.m_fields = new List<DataModels.LayoutZone>();

                        param.m_fields.Add(CreateDefaultField());
                        _layoutsDirty = true;
                    }

                    ImGui.Separator();
                }

                ImGui.PopID();
            }

            if (ImGui.Button("+ Add Category"))
            {
                colData.GeneralParameters.Add(CreateDefaultParameter());
                _layoutsDirty = true;
            }

            ImGui.Spacing();
            ImGui.Separator();

            // -------- Standalone Fields Column --------
            ImGui.Text("Standalone Fields");
            ImGui.Separator();

            var listFields = new List<DataModels.LayoutZone>(colData.Fields);

            for (int i = 0; i < listFields.Count; i++)
            {
                var field = listFields[i];
                if (field == null)
                    continue;

                EnsureZoneDefaults(field);
                ImGui.PushID(field.m_guid);

                DrawFieldUI(field);

                ImGui.SameLine();
                if (ImGui.Button("X##RemoveStandaloneField"))
                {
                    colData.Fields.Remove(field);
                    _layoutsDirty = true;
                    ImGui.PopID();
                    continue;
                }

                ImGui.Separator();
                ImGui.PopID();
            }

            if (ImGui.Button("+ Add Standalone Field"))
            {
                colData.Fields.Add(CreateDefaultField());
                _layoutsDirty = true;
            }

            // Sauvegarde des modifications dans la bonne colonne
            if (isLeft)
                record.LeftColumn = colData;
            else
                record.RightColumn = colData;
        }

        private void DrawFieldsList(DataModels.LayoutZone parentParam)
        {
            if (parentParam.m_fields == null)
                parentParam.m_fields = new List<DataModels.LayoutZone>();

            var listFields = new List<DataModels.LayoutZone>(parentParam.m_fields);

            for (int i = 0; i < listFields.Count; i++)
            {
                var field = listFields[i];
                if (field == null)
                    continue;

                EnsureZoneDefaults(field);
                ImGui.PushID(field.m_guid);

                DrawFieldUI(field);

                ImGui.SameLine();
                if (ImGui.Button("X##RemoveField"))
                {
                    parentParam.m_fields.Remove(field);
                    _layoutsDirty = true;
                    ImGui.PopID();
                    continue;
                }

                ImGui.Separator();
                ImGui.PopID();
            }
        }

        /// <summary>
        /// UI complet d’un champ :
        /// - Nom (manager)
        /// - Type + Valeur (LayoutType)
        /// - Dirty check + sécurités slider
        /// </summary>
        private void DrawFieldUI(DataModels.LayoutZone field)
        {
            // Nom du champ
            Layout("Field Name:", 120, 240);
            string prevKey = field.m_key;
            ImGui.InputText("##FieldName", ref field.m_key, 64);
            if (prevKey != field.m_key)
                _layoutsDirty = true;

            // Snapshot avant DrawField
            string prevType = field.m_type;
            string prevValue = field.m_value;
            float prevMin = field.m_sliderMin;
            float prevMax = field.m_sliderMax;

            Layout("Type / Value:", 120, 320);
            LayoutType.DrawField(field);

            // Dirty check + sécurités sur le slider
            if (prevType != field.m_type ||
                prevValue != field.m_value ||
                !Mathf.Approximately(prevMin, field.m_sliderMin) ||
                !Mathf.Approximately(prevMax, field.m_sliderMax))
            {
                if (float.IsNaN(field.m_sliderMin)) field.m_sliderMin = 0f;
                if (float.IsNaN(field.m_sliderMax)) field.m_sliderMax = 1f;
                if (field.m_sliderMax <= field.m_sliderMin)
                    field.m_sliderMax = field.m_sliderMin + 0.001f;

                _layoutsDirty = true;
            }
        }

        private void CreateNewLayout(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "New Layout";

            var newLayout = new DataModels.LayoutData();
            newLayout.m_id = _container.m_nextId++;
            newLayout.m_title = name;
            newLayout.m_open = true;

            if (newLayout.m_records == null)
                newLayout.m_records = new List<DataModels.RecordData>();

            _layouts.Add(newLayout);

            _focusLayoutId = newLayout.m_id;
            _layoutsDirty = true;

            SaveLayoutToFile(newLayout);
        }

        private void Splitter(string id, ref float left, ref float right, float minLeft, float minRight)
        {
            ImGui.PushID(id);

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.35f, 0.35f, 0.35f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.45f, 0.45f, 0.45f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.55f, 0.55f, 0.55f, 1.0f));

            ImGui.Button("##splitter", new Vector2(_splitterSize, -1f));

            ImGui.PopStyleColor(3);

            if (ImGui.IsItemActive())
            {
                float delta = ImGui.GetIO().MouseDelta.x;
                left += delta;
                right -= delta;

                if (left < minLeft)
                {
                    right -= (minLeft - left);
                    left = minLeft;
                }
                if (right < minRight)
                {
                    left -= (minRight - right);
                    right = minRight;
                }
            }

            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                float total = left + right;
                left = total * 0.5f;
                right = total * 0.5f;
            }

            ImGui.PopID();
        }

        private void EnsureFourColumnsWidthsFitWindow()
        {
            float remainingWidth = ImGui.GetContentRegionAvail().x;

            float totalWanted =
                _colSelectorWidth +
                _colEditorLeftWidth +
                _colEditorRightWidth +
                _splitterSize * 2;

            if (totalWanted > remainingWidth)
            {
                float usable = remainingWidth - _splitterSize * 2;
                float denom = _colSelectorWidth + _colEditorLeftWidth + _colEditorRightWidth;
                if (denom < 1f) denom = 1f;

                float scale = usable / denom;

                _colSelectorWidth = Mathf.Max(_minSelectorWidth, _colSelectorWidth * scale);
                _colEditorLeftWidth = Mathf.Max(_minEditorWidth, _colEditorLeftWidth * scale);
                _colEditorRightWidth = Mathf.Max(_minEditorWidth, _colEditorRightWidth * scale);
            }

            float usedWithoutRight =
                _colSelectorWidth +
                _colEditorLeftWidth +
                _splitterSize * 2;

            float rightFlex = remainingWidth - usedWithoutRight;
            _colEditorRightWidth = Mathf.Max(_minEditorWidth, rightFlex);
        }

        private void EnsureZoneDefaults(DataModels.LayoutZone zone)
        {
            if (zone == null)
                return;

            if (zone.m_key == null)
                zone.m_key = string.Empty;
            if (zone.m_value == null)
                zone.m_value = string.Empty;
            if (string.IsNullOrEmpty(zone.m_type))
                zone.m_type = LayoutType.LayoutValueType.Text.ToString();
            if (zone.m_fields == null)
                zone.m_fields = new List<DataModels.LayoutZone>();

            // slider safe
            if (float.IsNaN(zone.m_sliderMin)) zone.m_sliderMin = 0f;
            if (float.IsNaN(zone.m_sliderMax)) zone.m_sliderMax = 1f;
            if (zone.m_sliderMax <= zone.m_sliderMin)
                zone.m_sliderMax = zone.m_sliderMin + 0.001f;
        }

        private DataModels.LayoutZone CreateDefaultParameter()
        {
            var zone = new DataModels.LayoutZone();
            zone.m_guid = Guid.NewGuid().ToString();
            zone.m_key = "Category";
            zone.m_type = LayoutType.LayoutValueType.Text.ToString();
            zone.m_value = string.Empty;
            zone.m_fields = new List<DataModels.LayoutZone>();
            zone.m_sliderMin = 0f;
            zone.m_sliderMax = 1f;
            return zone;
        }

        private DataModels.LayoutZone CreateDefaultField()
        {
            var zone = new DataModels.LayoutZone();
            zone.m_guid = Guid.NewGuid().ToString();
            zone.m_key = "NewField";
            zone.m_type = LayoutType.LayoutValueType.Text.ToString();
            zone.m_value = string.Empty;
            zone.m_sliderMin = 0f;
            zone.m_sliderMax = 1f;
            zone.m_fields = new List<DataModels.LayoutZone>();
            return zone;
        }

        private string GetDatabaseFolder()
        {
            string root = Directory.GetParent(Application.dataPath).Parent.FullName;
            
            string dataDir   = Path.Combine(root, "data");
            string dbDir     = Path.Combine(dataDir, "database");

            if (!Directory.Exists(dbDir))
                Directory.CreateDirectory(dbDir);

            return dbDir;
        }

        private string GetLayoutPath(string layoutName)
        {
            if (string.IsNullOrEmpty(layoutName))
                layoutName = "Layout";

            string safeName = string.Concat(layoutName.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_saveFolder, safeName + ".json");
        }

        private void RemoveWindow(int layoutId)
        {
            if (_layouts == null)
                return;

            DataModels.LayoutData toDelete = null;
            for (int i = 0; i < _layouts.Count; i++)
            {
                var layout = _layouts[i];
                if (layout != null && layout.m_id == layoutId)
                {
                    toDelete = layout;
                    break;
                }
            }

            if (toDelete == null)
                return;

            string path = GetLayoutPath(toDelete.m_title);
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    Info("Failed to delete layout file " + path + ": " + e);
                }
            }

            _layouts.Remove(toDelete);

            if (_focusLayoutId == layoutId)
            {
                if (_layouts.Count > 0 && _layouts[0] != null)
                    _focusLayoutId = _layouts[0].m_id;
                else
                    _focusLayoutId = -1;
            }

            _layoutsDirty = true;
        }

        private void LoadLayoutsFromFiles()
        {
            _layouts = new List<DataModels.LayoutData>();

            if (!Directory.Exists(_saveFolder))
                return;

            string[] files = Directory.GetFiles(_saveFolder, "*.json");
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                try
                {
                    string json = File.ReadAllText(file);
                    var layout = JsonUtility.FromJson<DataModels.LayoutData>(json);
                    if (layout != null)
                    {
                        if (layout.m_records == null)
                            layout.m_records = new List<DataModels.RecordData>();

                        _layouts.Add(layout);
                    }
                }
                catch (Exception e)
                {
                    InfoDone("Failed to load layout file " + file + ": " + e);
                }
            }
        }

        private void SaveLayoutToFile(DataModels.LayoutData layout)
        {
            if (layout == null)
                return;

            string path = GetLayoutPath(layout.m_title);
            try
            {
                string json = JsonUtility.ToJson(layout, true);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Info("Failed to save layout " + layout.m_title + ": " + e);
            }
        }

        private void SaveAllLayouts()
        {
            if (_layouts == null)
                return;

            for (int i = 0; i < _layouts.Count; i++)
            {
                SaveLayoutToFile(_layouts[i]);
            }

            _layoutsDirty = false;
        }

        private bool GetCollapsedState(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return false;

            bool value;
            if (_collapsedParameters.TryGetValue(guid, out value))
                return value;

            _collapsedParameters.Add(guid, false);
            return false;
        }

        private void SetCollapsedState(string guid, bool collapsed)
        {
            if (string.IsNullOrEmpty(guid))
                return;

            _collapsedParameters[guid] = collapsed;
            _layoutsDirty = true;
        }

        #endregion


        #region Private and Protected

        private DataModels.LayoutContainer _container = new DataModels.LayoutContainer();
        private List<DataModels.LayoutData> _layouts = new List<DataModels.LayoutData>();

        private bool _layoutsDirty;
        private int _focusLayoutId = -1;
        private string _saveFolder;

        // Record focus
        private string _focusRecordGuidLeft;
        private string _focusRecordGuidRight;
        private string _newRecordName;

        // Column widths + min widths (4 colonnes)
        private float _colWindowsWidth = 300f;
        private float _colSelectorWidth = 220f;
        private float _colEditorLeftWidth = 360f;
        private float _colEditorRightWidth = 360f; // flex au runtime

        private const float _splitterSize = 6f;

        private const float _minWindowsWidth = 180f;
        private const float _minSelectorWidth = 160f;
        private const float _minEditorWidth = 220f;

        // Collapse state local (par GUID paramètre)
        private readonly Dictionary<string, bool> _collapsedParameters = new Dictionary<string, bool>();

        // Gestion des IDs de records par fenêtre (pour DatabaseRef + EnumCustom)
        private Dictionary<int, int> _nextRecordIdPerLayout = new Dictionary<int, int>();

        // Singleton
        private static ImGuiLayoutManager _instance;

        /// <summary>
        /// Helpers pour aligner label + input sur la même ligne.
        /// </summary>
        private static void Layout(string name, float labelWidth, float inputWidth)
        {
            ImGui.Text(name);
            ImGui.SameLine(labelWidth);
            ImGui.SetNextItemWidth(inputWidth);
        }

        #endregion
    }
}
