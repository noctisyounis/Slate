using System;
using System.Collections.Generic;
using System.Linq;

namespace Database.Runtime
{
    [Serializable]
    public class SerializableSave
    {
        public List<SerializableKeyValuePair> Facts = new();

        // add fact as if this was a dictionary
        public void AddFact(string key, SerializableFact fact)
        {
            Facts.Add(new SerializableKeyValuePair
            {
                Key = key,
                Value = fact
            });
        }

        // Helper method to convert to dictionary
        public Dictionary<string, SerializableFact> ToDictionary() => Facts.ToDictionary(
            kvp => kvp.Key, 
            kvp => kvp.Value
        );
    }
}