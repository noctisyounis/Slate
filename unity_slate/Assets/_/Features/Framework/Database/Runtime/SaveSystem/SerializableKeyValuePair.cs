using System;

namespace Database.Runtime
{
    [Serializable]
    public class SerializableKeyValuePair
    {
        public string Key;
        public SerializableFact Value;
    }
}