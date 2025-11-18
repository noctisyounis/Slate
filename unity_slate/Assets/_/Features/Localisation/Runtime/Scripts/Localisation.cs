
using System;
using System.Collections.Generic;
using System.IO;
using Foundation.Runtime;
using ImGuiNET;
using Manager.Runtime;
using Newtonsoft.Json;
using UnityEngine;

namespace Localisation.Runtime
{
    public class Localisation
    {
        #region Main Methods

        public void DrawDebug()
        {
            ImGui.Begin("Debug");
            ImGui.Text($"Current directory : {dir}");
            ImGui.Text($"File directory : {fileDir}");
            ImGui.Text($"File Name : {fileName}");
            ImGui.Text($"Json Localisations : {json}");
            ImGui.End();
        }
            public void DrawUI()
            {
                if (!_loaded)
                {
                    _loaded = true;
                    InitializeDefault();
                    LoadLocalizationData();
                    InitializeGroups();
                }
                
                WindowPosManager.RegisterWindow("Localisation");
                
                if (ImGui.Begin("Localisation", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.DockNodeHost))
                {
                    WindowPosManager.SyncWindowPosition("Localization Window###LocalizationUniqueId");

                    ImGui.BeginChild("LeftPanel", new Vector2(leftWidth, 0));
                    DrawHierarchyPanel();
                    ImGui.EndChild();

                    ImGui.SameLine();
                    
                    Vector2 splitterMin = ImGui.GetCursorScreenPos();
                    Vector2 splitterMax = new Vector2(splitterMin.x + splitterWidth, splitterMin.y + ImGui.GetContentRegionAvail().y);

                    ImGui.InvisibleButton("Splitter", new Vector2(splitterWidth, -1));
                    if (ImGui.IsItemActive())
                    {
                        leftWidth += ImGui.GetIO().MouseDelta.x;
                        leftWidth = Math.Clamp(leftWidth, minLeftWidth, maxLeftWidth);
                    }

                    var drawList = ImGui.GetWindowDrawList();
                    uint color = ImGui.GetColorU32(new Vector4(1, 1, 1, 0.3f));

                    drawList.AddLine(
                        new Vector2(splitterMin.x + splitterWidth * 0.5f, splitterMin.y),
                        new Vector2(splitterMin.x + splitterWidth * 0.5f, splitterMax.y),
                        color,
                        1.0f 
                    );

                    
                    ImGui.SameLine();

                    ImGui.BeginChild("RightPanel", new Vector2(0, 0));
                    DrawContentPanel();
                    ImGui.EndChild();

                    ImGui.Separator();

                }

                
                ImGui.End();    
            }
        #endregion

        #region Draw ImGui

            private bool shouldStopEditing = false;

            private void DrawHierarchyPanel()
            {
                ImGui.BeginChild("HierarchyPanel", new Vector2(leftWidth, 0));
                ImGui.Text("Groups");
                ImGui.Separator();

                for (int i = 0; i < _groups.Count; i++)
                {
                    ImGui.PushID(i);
                    _index = i;
                    // MODE RENAMING
                    if (editingIndex == i)
                    {
                        ImGui.SetNextItemWidth(leftWidth - 20);

                        bool validated = ImGui.InputText("##Rename", ref renameBuffer, 256,
                            ImGuiInputTextFlags.EnterReturnsTrue);

                        // Enter → valider
                        if (validated)
                        {
                            ValidateRename(_index);
                        }

                        // Si clic à l'extérieur → valider
                        if (ImGui.IsMouseClicked(0) && !ImGui.IsItemHovered())
                        {
                            ValidateRename(_index);
                        }

                        ImGui.PopID();
                        continue;
                    }

                    // MODE NORMAL : SELECTABLE
                    bool selected = (_selectedGroup == i);
                    if (ImGui.Selectable(_groups[i], selected))
                    {
                        _selectedGroup = i;
                    }
                    
                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
                    {
                        editingIndex = i;
                        renameBuffer = _groups[i];
                        ImGui.SetKeyboardFocusHere();
                    }

                    ImGui.PopID();
                }

                ImGui.Separator();

                if (ImGui.Button("New group"))
                {
                    string newGroupName = $"New group {_groups.Count}";
                    _groups.Add(newGroupName);
                    
                    foreach (var lang in _languages)
                    {
                        if (!_localisationByLanguage.ContainsKey(lang))
                            _localisationByLanguage[lang] = new Dictionary<string, List<LocalisationData>>();

                        if (!_localisationByLanguage[lang].ContainsKey(newGroupName))
                            _localisationByLanguage[lang][newGroupName] = new List<LocalisationData>();
                    }
                }

                if (_showConfirmDeleteGroup)
                {
                    string oldName = _groups[_index];
                    
                    ImGui.Begin("Delete group ?");

                    if (ImGui.Button("Delete"))
                    {
                        
                        _groups.RemoveAt(_selectedGroup);
                        _selectedGroup = _groups.Count - 1;
                        foreach (var lang in _languages)
                        {
                            if (_localisationByLanguage.ContainsKey(lang) &&
                                _localisationByLanguage[lang].ContainsKey(oldName))
                            {
                                Debug.Log("WHAT IS IT ?" + _localisationByLanguage[lang][oldName]);
                                _localisationByLanguage[lang].Remove(oldName);
                            }
                        }
                        
                        _showConfirmDeleteGroup = false;
                        SaveLocalizationData();
                        ImGui.CloseCurrentPopup();
                    }

                    if (ImGui.Button("Keep group"))
                    {
                        _showConfirmDeleteGroup = false;
                        ImGui.CloseCurrentPopup();
                    }
                    
                    ImGui.End();
                }
                ImGui.EndChild();
            }

            
            private void ValidateRename(int index)
            {
                foreach (var group in _groups)
                {
                 Debug.Log("GROUP" + group);
                    
                }
                string oldName = _groups[index];
                string newName = renameBuffer.Trim();

                if (!string.IsNullOrEmpty(newName) && newName != oldName)
                {
                    _groups[index] = newName;
                    
                    foreach (var lang in _languages)
                    {
                        if (_localisationByLanguage.ContainsKey(lang) &&
                            _localisationByLanguage[lang].ContainsKey(oldName))
                        {
                            var data = _localisationByLanguage[lang][oldName];
                            _localisationByLanguage[lang].Remove(oldName);
                            _localisationByLanguage[lang].Add(newName, data);
                        }
                    }
                    SaveLocalizationData();
                }
                
                if (string.IsNullOrEmpty(newName))
                {
                    _showConfirmDeleteGroup = true;
                }
                
                editingIndex = -1;
                
            }



            private void DrawLanguagesCombo()
            {
                if (ImGui.BeginCombo("##LanguageCombo", _languages[_selectedLanguage]))
                {
                    for (int i = 0; i < _languages.Count; i++)
                    {
                        bool isSelected = (_selectedLanguage == i);
                        if (ImGui.Selectable(_languages[i], isSelected))
                        {
                            _selectedLanguage = i;
                            //LoadLocalizationData();
                        }

                        if (isSelected)
                            ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }
            }
            
            private void DrawContentPanel()
            {
                if (_selectedGroup == -1)
                {
                    ImGui.Text("Select a group on the left.");
                    return;
                }
                
                var winSize = ImGui.GetWindowSize();
                var rightStartX = winSize.x;

                var cur = ImGui.GetCursorPos();
                
                ImGui.Text($"Content for: {_groups[_selectedGroup]}");
                ImGui.SameLine();
                
                cur = ImGui.GetCursorPos();
                
                ImGui.SetCursorPos(new Vector2(rightStartX-cur.x, cur.y));
                ImGui.SetNextItemWidth(100);
                if (ImGui.Button("Refresh"))
                {   
                    _showConfirmRefresh = true;
                }

                if (_showConfirmRefresh)
                {
                    if (ImGui.Begin("RefreshConfirmPopup"))
                    {
                        ImGui.Text("Confirm Refresh ?");

                        if (ImGui.Button("OK"))
                        {
                            LoadLocalizationData();
                            ImGui.CloseCurrentPopup();
                            _showConfirmRefresh = false;
                        }

                        ImGui.SameLine();

                        if (ImGui.Button("Cancel"))
                        {
                            ImGui.CloseCurrentPopup();
                            _showConfirmRefresh = false;
                            
                        }

                        ImGui.End();
                    }
               
                }

                
                ImGui.SameLine();
                
                ImGui.SetCursorPos(new Vector2(rightStartX-100, cur.y));
                ImGui.SetNextItemWidth(100);
                DrawLanguagesCombo();
                
                ImGui.Separator();

                if (ImGui.BeginTable("LocalizationTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
                {
                    string groupName = _groups[_selectedGroup];
                    
                    Debug.Log($"<color='purple'>'_locaBL count : {_localisationByLanguage.Count}</color>");

                    var datas = new List<LocalisationData>();
                    if (_localisationByLanguage[_languages[_selectedLanguage]].ContainsKey(groupName))
                    { 
                        datas = _localisationByLanguage[_languages[_selectedLanguage]][groupName];
                    }
                    else
                    {
                        return;
                    }
                    
                    Debug.Log($"<color='orange'>'Group count : {_localisationByGroup.Count}</color>");
                    
                    ImGui.TableSetupColumn("#");
                    ImGui.TableSetupColumn("ID");
                    ImGui.TableSetupColumn("Expression");
                    ImGui.TableHeadersRow();
                    DrawLocalizationTable(datas);
                    ImGui.EndTable();
                    if (ImGui.Button("New Data"))
                    {
                        string groupN = _groups[_selectedGroup];
                        Debug.Log($"<color='magenta'>LocByLangCount : {_localisationByLanguage.Count}</color> ");

                        foreach (var lang in _languages)
                        {
                            if (!_localisationByLanguage.ContainsKey(lang))
                                _localisationByLanguage[lang] = new Dictionary<string, List<LocalisationData>>();

                            if (!_localisationByLanguage[lang].ContainsKey(groupN))
                                _localisationByLanguage[lang][groupN] = new List<LocalisationData>();

                            _localisationByLanguage[lang][groupN].Add(new LocalisationData());
                        }
                        
                        SaveLocalizationData(_groups[_selectedGroup]);
                    }
                    winSize = ImGui.GetWindowSize();
                    rightStartX = winSize.x;

                    ImGui.SetCursorPos(new Vector2(rightStartX-100, cur.y));
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.Button("Save"))
                    {
                        SaveLocalizationData(_groups[_selectedGroup]);
                    }
                }
            }


            private void DrawLocalizationTable(List<LocalisationData> datas)
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.PushID(i * 4 + 0);
                    ImGui.Text(i.ToString());
                    ImGui.PopID();

                    ImGui.TableSetColumnIndex(1);
                    ImGui.PushID(i * 4 + 1);
                    ImGui.InputText("##uuid", ref datas[i].m_uuid, 256);
                    ImGui.PopID();

                    ImGui.TableSetColumnIndex(2);
                    ImGui.PushID(i * 4 + 2);
                    ImGui.InputText("##expression", ref datas[i].m_expression, 256);
                    ImGui.PopID();
                }
            }

        #endregion

        private void InitializeDefault()
        {
            if (_groups.Count == 0)
            {
                foreach (var group in _defaultGroups)
                {
                    //_groups.Add(group);
                }
            }
            

            foreach (var lang in _defaultLanguages)
            {
                if(!_languages.Contains(lang)) _languages.Add(lang);
                if(!_localisationByLanguage.ContainsKey(lang)) _localisationByLanguage.Add(lang, new Dictionary<string, List<LocalisationData>>());
            }
        }
        
        


        private void InitializeGroups()
        {
            foreach (var language in _localisationByLanguage)
            {
                foreach (var group in language.Value)
                {
                    //_groups.Add(group);
                    Debug.Log("Added : " + group);
                }
            }

            Debug.Log("_localisationByLanguage Count " + _localisationByLanguage.Count);
            Debug.Log("_groups Count " + _groups.Count);
            foreach (var group in _groups){
                
                foreach (var lang in _languages)
                {
                    if (!_localisationByLanguage.ContainsKey(lang))
                        _localisationByLanguage[lang] = new Dictionary<string, List<LocalisationData>>();

                    if (!_localisationByLanguage[lang].ContainsKey(group))
                        _localisationByLanguage[lang][group] = new List<LocalisationData>();
                }
            }
        }
        

        
        private void SaveLocalizationData(string groupName = "")
        {
            if(!string.IsNullOrEmpty(groupName)) SynchronizeUUIDAcrossLanguages(groupName);
            
            dir = Directory.GetCurrentDirectory();
            var fileDir = Path.Combine(dir, "data/localisation");
            if(!Directory.Exists(fileDir)) Directory.CreateDirectory(fileDir);
            
            foreach (var lang in _localisationByLanguage)
            {
                string fileName = Path.Combine(fileDir,$"LocalisationData.{lang.Key}.json");
                LocalisationFile file = new LocalisationFile();
                Debug.Log($"<color='cyan'>Language KVP: {lang.Key}</color>");
            
                foreach (var group in lang.Value)
                {
                    Debug.Log($"<color='yellow'>Group: {group.Key}</color>");

                    var copy = new List<LocalisationData>(group.Value);
                
                    file.groups.Add(new GroupData
                    {
                        groupName = group.Key,
                        entries = copy
                    });
                }

                string json = JsonConvert.SerializeObject(file, Formatting.Indented);
                File.WriteAllText(fileName, json);

            }
        }

        private void SynchronizeUUIDAcrossLanguages(string groupName)
        {
            if (!_localisationByLanguage.ContainsKey(_languages[_selectedLanguage])) 
                return;

            var referenceList = _localisationByLanguage[_languages[_selectedLanguage]][groupName];

            foreach (var lang in _languages)
            {
                if (!_localisationByLanguage.ContainsKey(lang))
                    _localisationByLanguage[lang] = new Dictionary<string, List<LocalisationData>>();

                if (!_localisationByLanguage[lang].ContainsKey(groupName))
                    _localisationByLanguage[lang][groupName] = new List<LocalisationData>();

                var currentList = _localisationByLanguage[lang][groupName];

                int count = Math.Min(referenceList.Count, currentList.Count);

                for (int i = 0; i < count; i++)
                {
                    currentList[i].m_uuid = referenceList[i].m_uuid;
                }
            }
        }
        
        
        private void LoadLocalizationData()
        {
            //_localisationByLanguage.Clear();
            //_groups.Clear();
            dir = Directory.GetCurrentDirectory();
            fileDir = Path.Combine(dir,"data/localisation");
            
            foreach (var lang in _languages)
            {
                fileName = Path.Combine(fileDir,$"LocalisationData.{lang}.json");
                
                if (!File.Exists(fileName))
                {
                    Debug.Log($"No file found for {lang}, skipping load.");
                    continue;
                }

                json = File.ReadAllText(fileName);
                LocalisationFile file = JsonConvert.DeserializeObject<LocalisationFile>(json);

                var groupDict = new Dictionary<string, List<LocalisationData>>();

                foreach (var group in file.groups)
                {
                    Debug.Log($"<color='cyan'>Found group {group.groupName} and entries {group.entries.Count} for language {lang}</color>");

                    if (!_groups.Contains(group.groupName))
                        _groups.Add(group.groupName);

                    groupDict[group.groupName] = group.entries;
                }

                _localisationByLanguage[lang] = groupDict;
            }

            if (_selectedLanguage >= 0 && _selectedLanguage < _languages.Count)
            {
                string selectedLang = _languages[_selectedLanguage];
                if (_localisationByLanguage.ContainsKey(selectedLang))
                {
                    _localisationByGroup = _localisationByLanguage[selectedLang];
                }
                else
                {
                    _localisationByGroup = new Dictionary<string, List<LocalisationData>>();
                }
            }
            else
            {
                _localisationByGroup = new Dictionary<string, List<LocalisationData>>();
            }

            Debug.Log($"Loaded localisation data for languages: {string.Join(", ", _localisationByLanguage.Keys)}");
        }

        
        private int editingIndex = -1;
        private string renameBuffer = "";
        
        float leftWidth = 200f;                 
        float splitterWidth = 10f;                 
        float minLeftWidth = 30f;               
        float maxLeftWidth = 600f;

        private string dir = "";
        private string fileDir = "";
        private string json = "";
        private string fileName = "";
        private int _index = 0;
        
        private bool _showConfirmRefresh = false;
        private bool _showConfirmDeleteGroup = false;
        private bool _loaded = false;
        private int _selectedGroup = -1;
        private int _selectedLanguage = 0;
        private string[] _defaultGroups = {"Player","Quest","Weapons"};
        private string[] _defaultLanguages = {"French","English","Spanish"};
        
        private List<string> _groups = new List<string>();
        private List<string> _languages = new List<string>();
        private Dictionary<string, List<LocalisationData>> _localisationByGroup 
            = new Dictionary<string, List<LocalisationData>>();
        private Dictionary<string, Dictionary<string, List<LocalisationData>>> _localisationByLanguage =  new Dictionary<string, Dictionary<string, List<LocalisationData>>>();

    }
    
   
    
    public class LocalisationData
    {
        public string m_uuid = "";
        public string m_expression = "";
        public override string ToString()
        {
            return  $"UUID: {m_uuid}, Expression: {m_expression}";
        }

    }


    [System.Serializable]
    public class LocalisationFile
    {
        public List<GroupData> groups = new List<GroupData>();
    }

    [System.Serializable]
    public class GroupData
    {
        public string groupName;
        public List<LocalisationData> entries = new List<LocalisationData>();
    }


}

