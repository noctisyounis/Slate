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
        public string Key;      // Nom de la donnée
        public string Type;     // Type de valeur (int, float, string, bool ...)
        public string Value;    // Valeur stockée en string pour sérialisation simple
    }

    [Serializable]
    public class RecordData
    {
        public string m_name; // Nom de l'enregistrement
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
        public string m_name; // Nom interne / identifiant unique
        public string m_title; // Titre affiché dans la fenêtre
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

        public string m_windowTitle = "Gestion des Layouts";
        public string m_newLayoutName = "Nouveau Layout";

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

            // Initialisation du SaveSystem
            SaveSystem.SetPath(saveFolder);

            // Chargement des layouts
            try
            {
                Load(); 
                LoadLayoutsFromFacts();
            }
            catch (FileNotFoundException)
            {
                InfoInProgress("Aucun fichier de sauvegarde trouvé. Création d'un container vide.");
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
                Warning($"Un layout nommé '{name}' existe déjà.");
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
                
            InfoDone($"Layout créé : {layout.m_name}");
                
        }
        
        #endregion


        #region Main Methods

        private void SaveLayoutsToFacts()
        {
            SyncContainerOpenState();
            SetFact("WindowCreator.LayoutContainer", _container, true);
            Save();
            _layoutsDirty = false;
            InfoDone("Layout persistés dans GameFacs.");
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
                        Info($"Réouverture automatique du layout '{layout.m_name}'");
                    }
                }
            }

            else
            {
                _container = new LayoutContainer{ m_layouts = new List<LayoutData>(), m_nextId = 1 }; 
                _layouts = new List<LayoutData>();
                InfoInProgress("Aucun layout trouvé - initialisation vide");
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
            DrawLayouts();

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
            // GameUImGui.DrawGameWindows();
        }

        private void DrawMainWindow()
        {
            ImGui.Begin(m_windowTitle);
            
            ImGui.Text("Créer un nouveau Layout :");
            ImGui.InputText("##NewLayoutName", ref m_newLayoutName, 64);
            
            if (ImGui.Button("Créer"))
                CreateNewLayout(m_newLayoutName);
            
            ImGui.SameLine();
            if (ImGui.Button("Sauvegarder tous"))
                SaveLayoutsToFacts();
            
            ImGui.SameLine();
            if (ImGui.Button("Recharger"))
                LoadLayoutsFromFacts();

            // if (ImGui.Button("Quêtes")) GameUImGui.m_questsWindowOpen = true;
            // ImGui.SameLine();
            // if (ImGui.Button("Personnages")) GameUImGui.m_charactersWindowOpen = true;
            // ImGui.SameLine();
            // if (ImGui.Button("Monsters")) GameUImGui.m_monstersWindowOpen = true;
            // ImGui.SameLine();
            // if (ImGui.Button("Inventaire")) GameUImGui.m_itemsWindowOpen = true;
            
            ImGui.Separator();
            
            ImGui.BeginChild("LayoutList", new Vector2(0, 200), ImGuiChildFlags.None);
            
            foreach (var layout in _layouts )
            {
                ImGui.Text($"{layout.m_title}");

                if (layout.m_open)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled("[Ouvert]");
                }
                else if (ImGui.SmallButton($"Ouvrir##{layout.m_name}"))
                            _toReopen.Add(layout.m_name);
            }
            ImGui.EndChild();
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
            foreach (var layout in _layouts)
            {
                bool open = layout.m_open;
                string layoutWindowId = $"LayoutWindow##layout_{layout.m_id}";

                if (open && ImGui.Begin(layoutWindowId, ref open, ImGuiWindowFlags.AlwaysVerticalScrollbar))
                {
                    ImGui.Text("Titre :");

                    if (_focusLayoutId == layout.m_id)
                    {
                        ImGui.SetKeyboardFocusHere();
                        _focusLayoutId = -1;
                    }
                    ImGui.InputText($"##Title_{layout.m_id}", ref layout.m_title, 64);

                    ImGui.Text("Description :");
                    ImGui.InputTextMultiline($"##desc_{layout.m_id}", ref layout.m_descrition, 512, new Vector2(-1, 80));

                    ImGui.Separator();
                    ImGui.Text("Enregistrements :");

                    ImGui.BeginChild($"Records_{layout.m_id}", new Vector2(0, 200), ImGuiChildFlags.None);

                    foreach (var record in layout.m_records)
                    {
                        string recId = $"rec_{layout.m_id}_{record.m_guid}";
                        ImGui.Separator();

                        ImGui.InputText($"Nom##{recId}", ref record.m_name, 64);

                        for (int z = 0; z < record.Fields.Count; z++)
                        {
                            var zone = record.GetField(z);
                            if (zone == null) continue;

                            string zoneId = $"{recId}_field_{z}";

                            ImGui.InputText($"Clé##{zoneId}", ref zone.Key, 64);

                            string[] types = { "string", "int", "float", "bool" };
                            int currentTypeIndex = Mathf.Max(0, Array.IndexOf(types, zone.Type));
                            if (ImGui.Combo($"Type##{zoneId}", ref currentTypeIndex, types, types.Length))
                                zone.Type = types[currentTypeIndex];

                            ImGui.InputText($"Valeur##{zoneId}", ref zone.Value, 64);

                            ImGui.SameLine();
                            if (ImGui.SmallButton($"X##{zoneId}"))
                            {
                                record.RemoveFieldAt(z);
                                _layoutsDirty = true;
                                break;
                            }
                        }

                        if (ImGui.Button($"+ Ajouter Zone##{recId}"))
                        {
                            record.AddField(new LayoutZone { Key = "NouvelleZone", Type = "string", Value = "" });
                            _layoutsDirty = true;
                        }

                        ImGui.SameLine();
                        if (ImGui.Button($"Supprimer Enregistrement##{recId}"))
                        {
                            _toRemoveRecords.Add((layout, record));
                            _layoutsDirty = true;
                        }
                    }

                    ImGui.EndChild();

                    if (ImGui.Button($"Ajouter Enregistrement##addrec_{layout.m_id}"))
                    {
                        var newrec = new RecordData { m_name = "NouvelEnregistrement" };
                        layout.m_records.Add(newrec);
                        _layoutsDirty = true;
                    }

                    DrawLayoutButton(layout);
                    ImGui.End();
                }

                layout.m_open = open;
            }
        }
        
        private void DrawLayoutButton(LayoutData layout)
        {
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
            {
                layout.m_open = false;
                layout.m_lastAction = "Fermeture";
                _layoutsDirty = true;
            }
                    
            ImGui.SameLine();
            if (ImGui.Button("Supprimer"))
                _toRemove.Add(layout.m_name);
                    
            ImGui.Text($"Dernière action : {layout.m_lastAction}");
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

        private void CleanupRecords()
        {
            if (_toRemoveRecords.Count > 0)
            {
                foreach (var (layout, record) in  _toRemoveRecords )
                {
                    layout.m_records.Remove(record);
                }

                _toRemoveRecords.Clear();
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
        // private int _nextId = 1;

        #endregion



    }
}
