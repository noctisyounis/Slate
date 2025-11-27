
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
                if (!_loaded) return;
                ImGui.Begin("Localisation Debug");
                ImGui.Text($"Current directory : {dir}");
                ImGui.Text($"Parent directory : {parentDir}");
                ImGui.Text($"Parent directory + : {parentDirPlus}");
                ImGui.Text($"File directory : {fileDir}");
                ImGui.Text($"File Name : {fileName}");
                //ImGui.Text($"Selected Language : {_languages[_selectedLanguage]}");
                ImGui.Text($"Json Localisations : {json}");
                ImGui.End();
            }
            public void DrawUI()
            {
                if (!_loaded)
                {
                    _loaded = true;
                    LoadLocalizationData();
                    InitializeGroups();
                }
                
                WindowPosManager.RegisterWindow("Localisation");
                
                DrawLocalisationWindow();
                
                if (_showAddLanguage)
                { 
                    CenterPopup();
                    DrawShowAddLanguagePopup();
                }
                if (_showConfirmDeleteGroup)
                {
                    CenterPopup();
                    DrawDeleteGroupButton();
                }
            }
        #endregion

        #region Draw ImGui

            private bool shouldStopEditing = false;

            private void DrawLocalisationWindow()
            {
                ImGui.Begin("Localisation", ImGuiWindowFlags.NoDocking);
                
                WindowPosManager.SyncWindowPosition("Localisation");

                //Left pannel
                ImGui.BeginChild("LeftPanel", new Vector2(leftWidth, 0));
                DrawHierarchyPanel();
                ImGui.EndChild();
                ImGui.SameLine();
                
                //Splitter
                DrawSplitter();
                ImGui.SameLine();
                
                //Right pannel
                ImGui.BeginChild("RightPanel", new Vector2(0, 0));
                DrawContentPanel();
                ImGui.EndChild();
                
                ImGui.End();  
                
            }

            private void DrawSplitter()
            {
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
            }
            
            private void DrawHierarchyPanel()
            {
                ImGui.BeginChild("HierarchyPanel", new Vector2(leftWidth, 0));
                ImGui.Text("Groups");
                ImGui.Separator();

                for (int i = 0; i < _groups.Count; i++)
                {
                    ImGui.PushID(i);
                    _index = i;
                    
                    if (editingIndex == i)
                    {
                        float deleteWidth = CalcButtonSize("Delete");
                        ImGui.SetNextItemWidth(leftWidth - deleteWidth - 20);

                        ImGui.InputText("##Rename", ref renameBuffer, 256,
                            ImGuiInputTextFlags.EnterReturnsTrue);

                        if (ImGui.IsItemDeactivatedAfterEdit())
                        {
                            ValidateRename(_index);
                        }

                        ImGui.SameLine();
                        if(ImGui.Button("Delete", new Vector2(deleteWidth,0)))
                        {
                            _showConfirmDeleteGroup = true;
                        }
                        
                        ImGui.PopID();
                        continue;
                    }
                    
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
                DrawNewGroupButton();
                ImGui.EndChild();
            }

            private void CenterPopup()
            {
                var io = ImGui.GetIO();
                var center = io.DisplaySize * 0.5f;

                ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            }

            private float CalcButtonSize(string label)
            {
                float textWidth = ImGui.CalcTextSize(label).x;

                float padding = ImGui.GetStyle().FramePadding.x;

                float buttonWidth = textWidth + padding * 2f;
                
                return buttonWidth;
            }
            
            private void DrawNewGroupButton()
            {
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
            }

            private void DrawDeleteGroupButton()
            {
                string oldName = _groups[_index];
                    
                ImGui.Begin("Delete group");

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
            private void ValidateRename(int index)
            {
                string oldName = _groups[index];
                string newName = renameBuffer.Trim();

                if (!string.IsNullOrEmpty(newName) && newName != oldName)
                {
                    if (_groups.Contains(newName)) return;
                    
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
                
                editingIndex = -1;
                
            }

            private string GetLongestLanguageLabel()
            {
                string longestLabel = "";
                foreach (string lang in _languages)
                {
                    if(lang.Length > longestLabel.Length) longestLabel = lang;
                }
                return longestLabel;
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
                ImGui.SameLine();
                if (ImGui.Button("+"))
                {
                    _showAddLanguage = true;
                    CenterPopup();
                    DrawShowAddLanguagePopup();
                }
            }

            private void DrawShowAddLanguagePopup()
            {
                ImGui.Begin("Add Language");
                var windowSize = ImGui.GetWindowSize();
                
                ImGui.Text("Add Language");
                ImGui.InputText("##AddLanguage", ref addLanguageBuffer, 256,
                    ImGuiInputTextFlags.EnterReturnsTrue);
                if (ImGui.Button("Confirm", new Vector2(windowSize.x,0)))
                {
                    if (_localisationByLanguage.ContainsKey(addLanguageBuffer)) return;

                    string newLang = addLanguageBuffer;
                    string referenceLang = _languages.Count > 0 ? _languages[0] : null;

                    _localisationByLanguage[newLang] = new Dictionary<string, List<LocalisationData>>();
                    _languages.Add(newLang);

                    foreach (var group in _groups)
                    {
                        _localisationByLanguage[newLang][group] = new List<LocalisationData>();

                        if (referenceLang != null && _localisationByLanguage.ContainsKey(referenceLang))
                        {
                            var refList = _localisationByLanguage[referenceLang][group];

                            foreach (var entry in refList)
                            {
                                _localisationByLanguage[newLang][group].Add(new LocalisationData
                                {
                                    m_uuid = entry.m_uuid,
                                    m_expression = ""      
                                });
                            }
                        }
                    }

                    SaveLocalizationData();
                    ImGui.CloseCurrentPopup();
                    _showAddLanguage = false;
                }

                if(ImGui.Button("Cancel", new Vector2(windowSize.x,0)))
                {
                    ImGui.CloseCurrentPopup();
                    _showAddLanguage = false;
                    
                }
                ImGui.End();
            }
            private void DrawRefreshConfirmPopup()
            {
                if (ImGui.Begin("RefreshConfirmPopup"))
                {
                    ImGui.Text("Confirm Refresh ?");

                    if (ImGui.Button("OK"))
                    {
                        _languages.Clear();
                        _groups.Clear();
                        _localisationByLanguage.Clear();
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
            private void DrawContentPanel()
            {
                
                float refreshWidth = CalcButtonSize("Refresh");
                float saveWidth = CalcButtonSize("Save");
                string longestLanguage = GetLongestLanguageLabel();
                float languageWidth = CalcButtonSize(longestLanguage) + 20;
                if (_selectedGroup == -1)
                {
                    ImGui.Text("Select a group on the left.");
                    return;
                }
                
                var winSize = ImGui.GetWindowSize();
                
                ImGui.SetNextItemWidth(winSize.x - refreshWidth - languageWidth);
                ImGui.Text($"Reading: {_groups[_selectedGroup]}");
                ImGui.SameLine();
                
                ImGui.SetNextItemWidth(refreshWidth);
                
                if (ImGui.Button("Refresh"))
                {   
                    _showConfirmRefresh = true;
                }
                
                ImGui.SameLine();
                ImGui.SetNextItemWidth(saveWidth);
                
                if (ImGui.Button("Save"))
                {
                    SaveLocalizationData(_groups[_selectedGroup]);
                }
                if (_showConfirmRefresh)
                {
                    CenterPopup();
                    DrawRefreshConfirmPopup();
                }
                
                ImGui.SameLine();
                
                ImGui.SetNextItemWidth(languageWidth);
                DrawLanguagesCombo();
                
                ImGui.Separator();

                ImGui.SetNextItemWidth(winSize.x);
                   
                DrawNewDataButton("0");
                
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
                    
                    ImGui.TableSetupColumn("Index",ImGuiTableColumnFlags.WidthFixed, CalcButtonSize("#999999"));
                    ImGui.TableSetupColumn("Key");
                    ImGui.TableSetupColumn("Value");
                    ImGui.TableHeadersRow();
                    DrawLocalizationTable(datas);
                    ImGui.EndTable();
                    ImGui.SetNextItemWidth(winSize.x);
                   
                    DrawNewDataButton("1");
                }
            }


            private void DrawNewDataButton(string uniqueId)
            {
                var winSize = ImGui.GetWindowSize();
                
                if (ImGui.Button($"New Data##{uniqueId}",new Vector2(winSize.x, 0)))
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

        private void InitializeLanguages()
        {

            var localizationFiles = Directory.GetFiles(fileDir, "LocalisationData.*.json");

            foreach (var file in localizationFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string lang = fileName.Substring("LocalisationData.".Length);
                _languages.Add(lang);
            }

            if (_languages.Count == 0)
            {
                foreach (var lang in _defaultLanguages)
                {
                    _languages.Add(lang);
                }
            }
        }
        

        private void InitializeGroups()
        {
            if (_groups.Count == 0)
            {
                foreach (var language in _localisationByLanguage)
                {
                    foreach (var group in language.Value)
                    {
                        _groups.Add(group.Key);
                    }
                }
            }
            

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
            parentDir = Directory.GetParent(dir).FullName;
            parentDirPlus = Directory.GetParent(parentDir).FullName;
            fileDir = Path.Combine(parentDir, "data", "localisation");
            if(!Directory.Exists(fileDir)) Directory.CreateDirectory(fileDir);
            
            foreach (var lang in _localisationByLanguage)
            {
                string fileName = Path.Combine(fileDir,$"LocalisationData.{lang.Key}.json");
                LocalisationFile file = new LocalisationFile();
            
                foreach (var group in lang.Value)
                {
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
            {
                return;
            }

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
            
            dir = Directory.GetCurrentDirectory();
            parentDir = Directory.GetParent(dir).FullName;
            parentDirPlus = Directory.GetParent(parentDir).FullName;
            
            
            fileDir = Path.Combine(parentDir, "data","localisation");
            if(!Directory.Exists(fileDir)) Directory.CreateDirectory(fileDir);
            
            InitializeLanguages();
            
            foreach (var lang in _languages)
            {
                fileName = Path.Combine(fileDir,$"LocalisationData.{lang}.json");
                
                if (!File.Exists(fileName))
                    continue;

                json = File.ReadAllText(fileName);
                LocalisationFile file = JsonConvert.DeserializeObject<LocalisationFile>(json);

                var groupDict = new Dictionary<string, List<LocalisationData>>();

                foreach (var group in file.groups)
                {
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
        }

        
        private int editingIndex = -1;
        private string renameBuffer = "";
        private string addLanguageBuffer = "";
        
        float leftWidth = 200f;                 
        float splitterWidth = 10f;                 
        float minLeftWidth = 30f;               
        float maxLeftWidth = 600f;

        private string dir = "";
        private string parentDir = "";
        private string parentDirPlus = "";
        private string fileDir = "";
        private string json = "";
        private string fileName = "";
        private int _index = 0;
        
        private bool open = true;
        private bool _showAddLanguage = false;
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

