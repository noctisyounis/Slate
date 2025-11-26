using System;
using System.Collections.Generic;
using UnityEngine;

namespace WindowCreator.Runtime
{
    public class DataModels
    {
        // =====================================================================
        // LayoutZone  -----------------  Un champ ou un groupe de champs
        // =====================================================================
        [Serializable]
        public class LayoutZone
        {
            public string m_guid = Guid.NewGuid().ToString();

            public string m_key;    // Nom (HP, Attack, Rarity...)
            public string m_type;   // Text, Int, Float, EnumCustom, ...

            public string m_value;  // Valeur (string)

            public float m_sliderMin = 0f;
            public float m_sliderMax = 1f;

            // Ce champ est la cause des CRASH
            // -> Il doit être NON sérialisé pour éviter JsonUtility loops
            [NonSerialized]
            public List<LayoutZone> m_fields;   // Sous-champs → runtime seulement
        }

        // =====================================================================
        // ColumnData  -----------------  Champs affichés dans une colonne
        // =====================================================================
        [Serializable]
        public class ColumnData
        {
            public List<LayoutZone> GeneralParameters = new();
            public List<LayoutZone> Fields = new();
        }


        // =====================================================================
        // RecordData  -----------------  Onglet (ex: Item, Weapon, Rarity...)
        // =====================================================================
        [Serializable]
        public class RecordData
        {
            public string m_name = "NewTab";
            public string m_guid = Guid.NewGuid().ToString();

            public int m_id;
            public int m_parentLayoutId;

            public ColumnData LeftColumn = new();
            public ColumnData RightColumn = new();

            public void EnsureInitialized()
            {
                LeftColumn ??= new ColumnData();
                RightColumn ??= new ColumnData();

                if (m_id == 0)
                    m_id = ImGuiLayoutManager.GetNextRecordIDForWindow(m_parentLayoutId);
            }
        }

        // =====================================================================
        // LayoutData ------------------  Une fenêtre (items, actors, enums…)
        // =====================================================================
        [Serializable]
        public class LayoutData
        {
            public int m_id;
            public string m_title;

            public bool m_open;
            public bool m_isEnumWindow;

            public List<RecordData> m_records = new();
        }

        // =====================================================================
        // LayoutContainer
        // =====================================================================
        [Serializable]
        public class LayoutContainer
        {
            public List<LayoutData> m_layouts = new();
            public int m_nextId = 0;
        }
    }
}
