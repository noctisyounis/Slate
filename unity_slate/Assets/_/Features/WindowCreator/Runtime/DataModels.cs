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
            public string Key; // Data name
            public string Type; // Value Type(int, float, string, bool ...)

            public LayoutType.LayoutValueType TypeEnum = LayoutType.LayoutValueType.String;

            public string Value; // Value stored as a string for simple serialization

            public void ResolveTypeFromString()
            {
                if (!string.IsNullOrEmpty(Type))
                {
                    if (Enum.TryParse(typeof(LayoutType.LayoutValueType), Type, true, out var value))
                        TypeEnum = (LayoutType.LayoutValueType)value;
                    else
                    {
                        var t = Type.ToLowerInvariant();
                        if (t.Contains("int")) TypeEnum = LayoutType.LayoutValueType.Int;
                        else if (t.Contains("float") || t.Contains("double") || t.Contains("single"))
                            TypeEnum = LayoutType.LayoutValueType.Float;
                        else if (t.Contains("bool")) TypeEnum = LayoutType.LayoutValueType.Bool;
                        else TypeEnum = LayoutType.LayoutValueType.String;
                    }
                }
            }

            public float SliderMin  = 0;
            public float SliderMax  = 1f;
        }

        [Serializable]
        public class RecordData
        {
            public string m_name = "NewTab"; // Record name
            public string m_guid = Guid.NewGuid().ToString();

            [SerializeField] private List<LayoutZone> _fields = new List<LayoutZone>();
            public IReadOnlyList<LayoutZone> Fields => _fields ??= new List<LayoutZone>();

            public int FieldCount => (_fields != null) ? _fields.Count : 0;

            public LayoutZone GetField(int index)
            {
                if (_fields == null || index < 0 || index >= _fields.Count)
                    return null;
                return _fields[index];
            }

            public void AddField(LayoutZone zone)
            {
                _fields ??= new List<LayoutZone>();
                _fields.Add(zone);
            }

            public void RemoveField(LayoutZone zone)
            {
                if (_fields == null) return;
                _fields.Remove(zone);
            }

            public void RemoveFieldAt(int index)
            {
                if (_fields == null) return;
                if (index >= 0 && index < _fields.Count)
                    _fields.RemoveAt(index);
            }
        }

        [Serializable]
        public class LayoutData
        {
            public int m_id;
            public string m_name; // Internal name / unique identifier
            public string m_title; // Title displayed in the window
            public bool m_open;
            public string m_descrition;
            public string m_lastAction;
            public List<RecordData> m_records = new List<RecordData>();

        }

        [Serializable]
        public class LayoutContainer
        {
            public List<LayoutData> m_layouts;
            public int m_nextId;
        }

        [Serializable]
        public class RuntimeWindow
        {
            public int m_windowID;
            public int m_layoutId;
            public string m_recordGuid;
            public string m_title;
            public bool m_open = true;
            public List<InfoWindow> m_infoWindows = new List<InfoWindow>();
        }

        [Serializable]
        public class InfoWindowField
        {
            public string m_key;
            public string m_value;
            public LayoutType.LayoutValueType TypeEnum = LayoutType.LayoutValueType.String;
        }

        [Serializable]
        public class InfoWindow
        {
            public int m_id;
            public string m_title;
            public List<string> m_values = new List<string>();
            public List<string> m_columns = new List<string>();
            public bool m_open = true;
        }
    }
}
