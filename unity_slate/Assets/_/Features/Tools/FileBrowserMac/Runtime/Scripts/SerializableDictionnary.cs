using System;
using System.Collections.Generic;
using UnityEngine;

namespace FileBrowserMac.Runtime
{
    [Serializable]
    public class SerializableDictionary
    {
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
                dict[keys[i]] = values[i];
            return dict;
        }
    }
    
}
