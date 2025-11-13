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

        [Header("Debug")]
        public GameObject _test;
        public int _number;
        public int inputnumber;
        public Vector2 pos;
        public TextAsset jsonFile;

    #endregion

    #region Unity API

   

    private void Start()
        {
            if (_test != null)
                _renderer = _test.GetComponent<Renderer>();
            LoadFactsOnStart();
        }
    /*
        private void OnEnable()
        {
            UImGuiUtility.Layout += OnLayout;
        }
*/
        /*
        private void OnDisable()
        {
            UImGuiUtility.Layout -= OnLayout;
            UImGuiUtility.OnInitialize -= OnInitialize;
            UImGuiUtility.OnDeinitialize -= OnDeinitialize;
        }
        */
    #endregion

    #region ImGUI Main

        protected override void WindowLayout()
        {
            if (!showWindow) return;

            
            //ImGui.SetNextWindowSize(new Vector2(700, 520), ImGuiCond.Once);
            //if (ImGui.Begin("Configurable Window", ImGuiWindowFlags.NoCollapse))
            //{

                string[] tabs = { "Debug commands", "SlateConfigs", "Json Editor", "Localisation" };
                foreach (var tab in tabs)
                {
                    WindowPosManager.RegisterWindow(tab);
                }
                
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
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem(tabs[2]))
                    {
                        jsonLoader.DrawUI();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem(tabs[3]))
                    {
                        _localisation.DrawUI();
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            //}

            if (ImGui.Button("Fermer"))
            {
                showWindow = false;
            }

            ImGui.End();
            
            DrawOpenJsonEditors();
        }

    #endregion

    #region Debug / Slate

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
            if (ImGui.Button("Appliquer couleur"))
            {
                SaveColor();
            }
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

        private Localisation _localisation = new Localisation();
        private Color _gameObjectTestColor;
        private Color _backgroundColor;
        private Renderer _renderer;
        private Color _color;
        private Vector4 colorValue = new Vector4(1, 1, 1, 1);

        private bool showWindow = true;
        private float zoom = 1f;
        // private Dictionary<string, string> jsonData = new Dictionary<string, string>();

    #endregion

    #region Helpers (optionnel)

        private void LoadFactsOnStart()
        {
            // if (FactExists("color", out _color)) { if (_renderer != null) _renderer.material.color = _color; }
        }

    #endregion
}

public class Localisation
{
    private bool _loaded = false;
    public void DrawUI()
    {
        InitializeDefault();
        InitializeGroups();
        if (!_loaded)
        {
            _loaded = true;
            LoadLocalizationData();
        }
        
        WindowPosManager.RegisterWindow("Localization Window###LocalizationUniqueId");
        
        
        if (ImGui.Begin("Localization Window###LocalizationUniqueId", ImGuiWindowFlags.NoCollapse))
        {
            WindowPosManager.SyncWindowPosition("Localization Window###LocalizationUniqueId");
            ImGui.Text("Localisation");
            float leftWidth = 200f;

            ImGui.BeginChild("LeftPanel", new Vector2(leftWidth, 0));
            DrawHierarchyPanel();
            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("RightPanel", new Vector2(0, 0));
            DrawContentPanel();
            ImGui.EndChild();

            ImGui.Separator();

        }

        ImGui.End();
    }


    private void DrawHierarchyPanel()
    {
        ImGui.Text("Groups");
        ImGui.Separator();
        for (int i = 0; i < _groups.Count; i++)
        {
            ImGui.PushID(i);
            if (ImGui.Selectable(_groups[i], _selectedGroup == i))
            {
                _selectedGroup = i;
            }
            ImGui.PopID();
        }

        if (ImGui.Button("New group"))
        {
            string newGroupName = $"New group {_groups.Count}";
            _groups.Add(newGroupName);

            // Ajoute le nouveau groupe à toutes les langues
            foreach (var lang in _languages)
            {
                if (!_localisationByLanguage.ContainsKey(lang))
                    _localisationByLanguage[lang] = new Dictionary<string, List<LocalisationData>>();
        
                if (!_localisationByLanguage[lang].ContainsKey(newGroupName))
                    _localisationByLanguage[lang][newGroupName] = new List<LocalisationData>();
            }
        }


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
        

        ImGui.SetCursorPos(new Vector2(rightStartX-100, cur.y));
        ImGui.SetNextItemWidth(100);
        DrawLanguagesCombo();
        
        ImGui.Separator();

        if (ImGui.BeginTable("LocalizationTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            
            string groupName = _groups[_selectedGroup];

            //List<LocalisationData> datas = _localisationByGroup[groupName];
            Debug.Log($"<color='purple'>'_locaBL count : {_localisationByLanguage.Count}</color>");
            
            List<LocalisationData> datas = _localisationByLanguage[_languages[_selectedLanguage]][groupName];
            
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


    public void DrawLocalizationTable(List<LocalisationData> datas)
    {
        for (int i = 0; i < datas.Count; i++)
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.PushID(i * 4 + 0);
            ImGui.Text(i.ToString());
            //ImGui.InputText("##line", ref datas[i].m_line, 100);
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


    private void InitializeDefault()
    {
        foreach (var group in _defaultGroups)
        {
            if(!_groups.Contains(group)) _groups.Add(group);
        }

        foreach (var lang in _defaultLanguages)
        {
            if(!_languages.Contains(lang)) _languages.Add(lang);
            if(!_localisationByLanguage.ContainsKey(lang)) _localisationByLanguage.Add(lang, new Dictionary<string, List<LocalisationData>>());
        }
    }
    
    
    private void InitializeGroups()
    {
        foreach (var group in _defaultGroups)
        {
            if (!_groups.Contains(group))
                _groups.Add(group);

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
        SynchronizeUUIDAcrossLanguages(groupName);
        
        foreach (var lang in _localisationByLanguage)
        {
            string fileName = $"LocalisationData.{lang.Key}.json";
            LocalisationFile file = new LocalisationFile();
            Debug.Log($"<color='cyan'>Language KVP: {lang.Key}</color>");
        
            foreach (var group in lang.Value)
            {
                Debug.Log($"<color='yellow'>Group: {group.Key}</color>");

                var copy = new List<LocalisationData>(group.Value); // Copie de la liste
            
                // Ajout d'un groupe avec ses entrées à la sauvegarde
                file.groups.Add(new GroupData
                {
                    groupName = group.Key,
                    entries = copy
                });
            }

            string json = JsonConvert.SerializeObject(file, Formatting.Indented);
            File.WriteAllText(fileName, json);

            Debug.Log($"Saved localisation data to {fileName} for language {lang.Key}");
        }
    }

    private void SynchronizeUUIDAcrossLanguages(string groupName)
    {
        // On parcourt chaque langue
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

            // Synchronisation sur la longueur minimale
            int count = Math.Min(referenceList.Count, currentList.Count);

            for (int i = 0; i < count; i++)
            {
                // On copie l'UUID de la langue sélectionnée vers toutes les autres langues
                currentList[i].m_uuid = referenceList[i].m_uuid;
            }
        }
    }
    
    /*private void LoadLocalizationData()
    {
        string lang = _languages[_selectedLanguage];
        string fileName = $"LocalisationData.{lang}.json";

        if (_localisationByLanguage.ContainsKey(lang))
        {
            Debug.Log($"Using cached localisation data for {lang}");
            _localisationByGroup = _localisationByLanguage[lang];
            return;
        }
        if (!File.Exists(fileName))
        {
            Debug.Log($"No file found for {lang}, skipping load.");
            return;
        }

        string json = File.ReadAllText(fileName);
        LocalisationFile file = JsonConvert.DeserializeObject<LocalisationFile>(json);

        _localisationByGroup.Clear();
        foreach (var group in file.groups)
        {
            Debug.Log($"<color='cyan'>Found group {group.groupName} and entries {group.entries.Count}</color>");
            if(!_groups.Contains(group.groupName)) _groups.Add(group.groupName);
            _localisationByGroup[group.groupName] = group.entries;
        }

        Debug.Log($"Loaded localisation data from {fileName}");
    }
    */
    
    private void LoadLocalizationData()
    {
        //_localisationByLanguage.Clear();
        //_groups.Clear();

        foreach (var lang in _languages)
        {
            string fileName = $"LocalisationData.{lang}.json";

            if (!File.Exists(fileName))
            {
                Debug.Log($"No file found for {lang}, skipping load.");
                continue;
            }

            string json = File.ReadAllText(fileName);
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

        // Initialise la vue locale _localisationByGroup avec la langue sélectionnée
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

    
    private List<LocalisationData> _localisationDatas = new List<LocalisationData>();
    private int _selectedGroup = -1;
    private int _selectedLanguage = 0;
    private List<string> _groups = new List<string>();
    private List<string> _languages = new List<string>();
    private string[] _defaultGroups = {"Player","Quest","Weapons"};
    private string[] _defaultLanguages = {"French","English","Spanish"};
    
    private Dictionary<string, List<LocalisationData>> _localisationByGroup 
        = new Dictionary<string, List<LocalisationData>>();
    
    private Dictionary<string, Dictionary<string, List<LocalisationData>>> _localisationByLanguage =  new Dictionary<string, Dictionary<string, List<LocalisationData>>>();

}

public class LocalisationData
{
    public string m_folder = "";
    public string m_line = "";
    public string m_uuid = "";
    public string m_expression = "";
    public string m_country = "";
    public override string ToString()
    {
        return $"Folder: {m_folder}, Line: {m_line}, UUID: {m_uuid}, Expression: {m_expression}, Country: {m_country}";
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



