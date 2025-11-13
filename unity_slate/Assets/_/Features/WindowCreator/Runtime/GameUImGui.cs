using System.Collections.Generic;
using ImGuiNET;
using UnityEngine;

namespace WindowCreator.Runtime
{
    public static class GameUImGui
    {
        public static bool m_questsWindowOpen = false;
        public static bool m_charactersWindowOpen = false;
        public static bool m_monstersWindowOpen = false;
        public static bool m_itemsWindowOpen = false;
        
        public static List<QuestData> m_quests = new ();
        public static List<CharacterData> m_characters = new ();
        public static List<MonsterData> m_monsters = new ();
        public static List<ItemData> m_items = new ();

        public static int m_selectedQuest = -1;
        public static int m_selectedCharacter = -1;
        public static int m_selectedMonster = -1;
        public static int m_selectedItem = -1;
        
        public static void DrawGameWindows()
        {
            // --- QUÊTES ---
            if (m_questsWindowOpen)
            {
                ImGui.Begin("Quêtes");

                // Bouton créer
                if (ImGui.Button("Nouvelle quête"))
                {
                    var q = new QuestData { m_title = "Nouvelle quête", m_description = "", m_completed = false };
                    m_quests.Add(q);
                    m_selectedQuest = m_quests.Count - 1; // sélectionner la nouvelle
                }

                ImGui.Separator();

                // protection simple
                if (m_quests == null) m_quests = new List<QuestData>();

                for (int i = 0; i < m_quests.Count; i++)
                {
                    string title = string.IsNullOrEmpty(m_quests[i].m_title) ? $"Quête {i+1}" : m_quests[i].m_title;
                    bool selected = (m_selectedQuest == i);
                    if (ImGui.Selectable(title, selected))
                        m_selectedQuest = i;
                }

                // affiche l'éditeur de la quête sélectionnée
                if (m_selectedQuest >= 0 && m_selectedQuest < m_quests.Count)
                {
                    ImGui.Separator();
                    ImGui.Text("Détails de la quête :");
                
                    var quest = m_quests[m_selectedQuest];
                    QuestWindow(ref quest);

                    ImGui.Separator();
                    if (ImGui.Button("Sauvegarder la quête"))
                    { 

                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Supprimer la quête"))
                    {
                        m_quests.RemoveAt(m_selectedQuest);
                        m_selectedQuest = Mathf.Clamp(m_selectedQuest - 1, -1, m_quests.Count - 1);
                    }

                    
                }

                if (ImGui.Button("Fermer la fenêtre"))
                    m_questsWindowOpen = false;
                
                ImGui.End();
            }
            

            // --- PERSONNAGES ---
            if (m_charactersWindowOpen)
            {
                ImGui.Begin("Personnages");

                if (m_characters == null) m_characters = new List<CharacterData>();

                if (ImGui.Button("Nouveau personnage"))
                {
                    m_characters.Add(new CharacterData { m_name = "Nouveau personnage", m_level = 1 });
                    m_selectedCharacter = m_characters.Count - 1;
                }
                ImGui.Separator();

                for (int i = 0; i < m_characters.Count; i++)
                {
                    string name = string.IsNullOrEmpty(m_characters[i].m_name) ? $"Perso {i+1}" : m_characters[i].m_name;
                    if (ImGui.Selectable(name, m_selectedCharacter == i))
                        m_selectedCharacter = i;
                }

                if (m_selectedCharacter >= 0 && m_selectedCharacter < m_characters.Count)
                {
                    ImGui.Separator();
                    var character = m_characters[m_selectedCharacter];
                    CharacterWindow(ref character);
                    ImGui.Separator();
                    if (ImGui.Button("Supprimer personnage"))
                    {
                        m_characters.RemoveAt(m_selectedCharacter);
                        m_selectedCharacter = Mathf.Clamp(m_selectedCharacter - 1, -1, m_characters.Count - 1);
                    }

                    
                }
                
                if (ImGui.Button("Fermer la fenêtre"))
                    m_charactersWindowOpen = false;

                ImGui.End();
            }
            

            // --- MONSTRES ---

            if (m_monstersWindowOpen)
            {
                ImGui.Begin("Monstres");

                if (m_monsters == null) m_monsters = new List<MonsterData>();

                if (ImGui.Button("Nouveau monstre"))
                {
                    m_monsters.Add(new MonsterData { m_name = "Nouveau monstre", m_level = 1 });
                    m_selectedMonster = m_monsters.Count - 1;
                }
                ImGui.Separator();

                for (int i = 0; i < m_monsters.Count; i++)
                {
                    string name = string.IsNullOrEmpty(m_monsters[i].m_name) ? $"Monstre {i+1}" : m_monsters[i].m_name;
                    if (ImGui.Selectable(name, m_selectedMonster == i))
                        m_selectedMonster = i;
                }

                if (m_selectedMonster >= 0 && m_selectedMonster < m_monsters.Count)
                {
                    ImGui.Separator();
                    var monster = m_monsters[m_selectedMonster];
                    MonsterWindow(ref monster);
                    ImGui.Separator();
                    if (ImGui.Button("Supprimer monstre"))
                    {
                        m_monsters.RemoveAt(m_selectedMonster);
                        m_selectedMonster = Mathf.Clamp(m_selectedMonster - 1, -1, m_monsters.Count - 1);
                    }
                    
                }

                if (ImGui.Button("Fermer la fenêtre"))
                    m_monstersWindowOpen = false;
                
                ImGui.End();
            }
            

            // --- INVENTAIRE ---
            if (m_itemsWindowOpen)
            {
                ImGui.Begin("Inventaire");

                if (m_items == null) m_items = new List<ItemData>();

                if (ImGui.Button("Nouvel objet"))
                {
                    m_items.Add(new ItemData { m_name = "Nouvel objet", m_quantity = 1 });
                    m_selectedItem = m_items.Count - 1;
                }
                ImGui.Separator();

                for (int i = 0; i < m_items.Count; i++)
                {
                    string name = string.IsNullOrEmpty(m_items[i].m_name) ? $"Objet {i+1}" : m_items[i].m_name;
                    if (ImGui.Selectable(name, m_selectedItem == i))
                        m_selectedItem = i;
                }

                if (m_selectedItem >= 0 && m_selectedItem < m_items.Count)
                {
                    ImGui.Separator();
                    var  item = m_items[m_selectedItem];
                    ItemWindow(ref item);
                    ImGui.Separator();
                    if (ImGui.Button("Supprimer objet"))
                    {
                        m_items.RemoveAt(m_selectedItem);
                        m_selectedItem = Mathf.Clamp(m_selectedItem - 1, -1, m_items.Count - 1);
                    }
                    
                }

                if (ImGui.Button("Fermer la fenêtre"))
                    m_itemsWindowOpen = false;
                ImGui.End();
            }
            
        }
        
        
        #region Quest
        public class QuestData
        {
            public string m_title;
            public string m_description;
            public bool m_completed;
            public bool m_open = true;
            public List<QuestData> m_subQuests = new List<QuestData>();
        }
        
        // Afficher une fenêtre ou section pour une quête
        public static void QuestWindow(ref QuestData quest)
        {
            if (!quest.m_open)  return;
            
            if (quest.m_title == null) quest.m_title = "";
            if (quest.m_description == null) quest.m_description = "";
            if (quest.m_subQuests == null) quest.m_subQuests = new List<QuestData>();
            
            ImGui.Text("Titre de la quête :");
            ImGui.InputText("##QuestTitle", ref quest.m_title, 128);
            
            ImGui.Text("Description :");
            ImGui.InputTextMultiline("##QuestDesc", ref quest.m_description, 1024, new Vector2(-1,80));
            
            ImGui.Checkbox("Quête terminée ?", ref quest.m_completed);

            if (quest.m_subQuests.Count > 0)
            {
                ImGui.Separator();
                ImGui.Text("Sous-quêtes :");
                for (int i = 0; i < quest.m_subQuests.Count; i++)
                {
                    var sub = quest.m_subQuests[i];
                    QuestWindow(ref sub);
                }
            }

            if (ImGui.Button("Ajouter sous-quête"))
            {
                quest.m_subQuests.Add(new QuestData { m_title = "Nouvelle sous-quête", m_description = "", m_completed = false });
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Fermer la fenêtre"))
            {
                quest.m_open = false;
            }
        }
        
        #endregion
        
        
        #region Character
        
        public class CharacterData
        {
            public string m_name;
            public int m_level;
            public float m_health;
            public float m_mana;
            public int m_experience;
        }

        public static void CharacterWindow(ref CharacterData character)
        {
            ImGui.InputText("Nom", ref character.m_name, 128);
            ImGui.SliderInt("Niveau", ref character.m_level, 0, 100);
            ImGui.SliderFloat("PV", ref character.m_health, 0, 500f);
            ImGui.SliderFloat("Mana", ref character.m_mana, 0, 500f);
            ImGui.SliderInt("XP", ref character.m_experience, 0, 10000);
        }
        
        #endregion
        
        
        #region Monsters

        public class MonsterData
        {
            public string m_name;
            public int m_level;
            public float m_health;
            public float m_damage;
        }

        public static void MonsterWindow(ref MonsterData monster)
        {
            ImGui.InputText("Nom", ref monster.m_name, 128);
            ImGui.SliderInt("Niveau", ref monster.m_level, 1, 100);
            ImGui.SliderFloat("PV", ref monster.m_health, 0, 500f);
            ImGui.SliderFloat("Dégâts", ref monster.m_damage, 0, 200f);
        }
        
        #endregion
        
        
        #region Inventory / Object

        public class ItemData
        {
            public string m_name;
            public string m_description;
            public int m_quantity;
        }

        public static void ItemWindow(ref ItemData item)
        {
            ImGui.InputText("Nom", ref item.m_name, 128);
            ImGui.InputTextMultiline("Description", ref item.m_description, 512, new Vector2(-1,60));
            ImGui.SliderInt("Quantity", ref item.m_quantity, 0, 999);
        }
        
        #endregion
        
        
        #region Fight / Actions Player

        public static void ActionList(ref List<string> actions, ref int selectedAction)
        {
            if (actions.Count == 0) return;
            ImGui.Combo("Actions", ref selectedAction, actions.ToArray(), actions.Count);
        }
        
        #endregion


        #region Booléens dynamiques

        public static void BollActionList(Dictionary<string, bool> actions)
        {
            foreach (var key in new List<string>(actions.Keys))
            {
                bool value = actions[key];
                if (ImGui.Checkbox(key, ref value))
                {
                    actions[key] = value;
                }
            }
        }

        #endregion
    }
}
