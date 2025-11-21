using System;
using System.Collections.Generic;
using UnityEngine;

namespace WindowCreator.Runtime
{
    public class DataModels
    {
        [Serializable]
        public class LayoutZone
        {
            public string m_key; // Data name
            public string m_type; // Value Type(int, float, string, bool ...)
            
            public string m_value; // Value stored as a string for simple serialization
            
            public float m_sliderMin;
            public float m_sliderMax  = 1f;
            
            public string m_guid = Guid.NewGuid().ToString();
            
            public List<LayoutZone> m_fields = new List<LayoutZone>();
        }

        [Serializable]
        public class ColumnData
        {
            public List<LayoutZone> GeneralParameters = new();
            public List<LayoutZone> Fields = new();

            public void AddGeneral(LayoutZone zone) => GeneralParameters.Add(zone);
            public void AddField(LayoutZone zone) => Fields.Add(zone);

            public void RemoveGeneral(LayoutZone zone) => GeneralParameters.Remove(zone);
            public void RemoveField(LayoutZone zone) => Fields.Remove(zone);
        }
        
        [Serializable]
        public class GeneralParameters
        {
            [SerializeField] private List<LayoutZone> _leftFields = new();
            [SerializeField] private List<LayoutZone> _rightFields = new();

            public IReadOnlyList<LayoutZone> LeftFields => _leftFields ??= new List<LayoutZone>();
            public IReadOnlyList<LayoutZone> RightFields => _rightFields ??= new List<LayoutZone>();

            public void AddLeft(LayoutZone zone) => _leftFields.Add(zone);
            public void AddRight(LayoutZone zone) => _rightFields.Add(zone);

            public void RemoveLeft(LayoutZone zone) => _leftFields?.Remove(zone);
            public void RemoveRight(LayoutZone zone) => _rightFields?.Remove(zone);

            public void RemoveLeftAt(int index)
            {
                if (_leftFields == null || index < 0 || index >= _leftFields.Count) return;
                _leftFields.RemoveAt(index);
            }

            public void RemoveRightAt(int index)
            {
                if (_rightFields == null || index < 0 || index >= _rightFields.Count) return;
                _rightFields.RemoveAt(index);
            }
        }


        [Serializable]
        public class RecordData
        {
            public string m_name = "NewTab";
            public string m_guid = Guid.NewGuid().ToString();

            public ColumnData LeftColumn = new ColumnData();
            public ColumnData RightColumn = new ColumnData();

            public void EnsureInitialized()
            {
                LeftColumn ??= new ColumnData();
                RightColumn ??= new ColumnData();
            }
        }


        [Serializable]
        public class LayoutData
        {
            public int m_id;
            public string m_name; // Internal name / unique identifier
            public string m_title; // Title displayed in the window
            public bool m_open;
            public string m_description;
            public string m_lastAction;
            public List<RecordData> m_records = new List<RecordData>();
            
            public LayoutData() => m_id = Guid.NewGuid().GetHashCode();

        }

        [Serializable]
        public class LayoutContainer
        {
            public List<LayoutData> m_layouts = new List<LayoutData>();
            public int m_nextId = 0;
        }
    }
}
