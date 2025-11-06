using System;
using System.Collections.Generic;
using System.IO;
using Database.Runtime._.Features.Framework.Database.Runtime.Enums;
using Database.Runtime.Interfaces;
using UnityEngine;
using UnityEngine.Assertions;

namespace Database.Runtime
{
    public class JsonSaveSystem
    {
        
        #region Public

        public static string SaveDirectoryPath { get; private set; }
        
        #endregion
        
        #region Main Methods

        //todo: add way to manage different slots with a SaveSlot class/struct
        //todo: make readme documentation with new save system: explain different serializable classes
       
        public string Save(Dictionary<string, IFact> persistentFacts, SaveSlot slot)
        {
            Assert.IsNotNull(persistentFacts);
            var path = GetSavePath(slot);
            // string jsonString = JsonConvert.SerializeObject(persistentFacts, Formatting.Indented);
            var serializedSave = new SerializableSave();
            foreach (var kvp in persistentFacts)
            {
                var fact = kvp.Value;
                var factValue = fact.GetObjectValue;
                var serializableFact = new SerializableFact
                {
                    IsPersistent = fact.IsPersistent,
                    Value = (factValue.GetType().IsPrimitive || factValue is string) ? 
                        factValue.ToString() : JsonUtility.ToJson(fact.GetObjectValue),
                    // ValueType = kvp.Value.GetObjectValue.GetType().AssemblyQualifiedName
                    ValueType = fact.ValueType.AssemblyQualifiedName
                };
                serializedSave.AddFact(kvp.Key, serializableFact);
            }
            Assert.IsNotNull(serializedSave);
            Assert.IsNotNull(serializedSave.Facts);
            Assert.IsTrue(serializedSave.Facts.Count > 0);
            string jsonString = JsonUtility.ToJson(serializedSave, true);
            File.WriteAllText(path, jsonString);
            return jsonString;
            // Info($"Game saved! \n SaveDirectoryPath: {_filePath}");
        }
       
        public Dictionary<string, IFact> Load(FactDictionary factStore, SaveSlot slot)
        {
            var savePath = GetSavePath(slot);
            // if (!File.Exists(savePath))
                // throw new FileNotFoundException($"Save {slot} not found.");

                // TODO: handle this situation properly
                if (!File.Exists(savePath))
                {
                    Debug.LogWarning($"Save {slot} not found. Creating new save.");
                    factStore.SetFact("",string.Empty, FactDictionary.FactPersistence.Persistent);
                    Save(factStore.AllFacts, slot);
                }

                var json = File.ReadAllText(savePath);
            // 
            // var saveFile = JsonConvert.DeserializeObject<Dictionary<string, SerializableFact>>(json);
            var saveFile = JsonUtility.FromJson<SerializableSave>(json);
            Debug.Log(json);
            var facts = saveFile.ToDictionary();
            // var saveFile = JsonConvert.DeserializeObject<Dictionary<string, IFact>>(json);
            factStore.AllFacts.Clear();

            foreach (var kvp in facts)
            {
                // get type from type string
                var type = Type.GetType(kvp.Value.ValueType);
                // deserialize value from json string with proper value type
                // var value = JsonConvert.DeserializeObject(kvp.Value.Value, type);
                var value = (type.IsPrimitive || type is string) ? 
                    Convert.ChangeType(kvp.Value.Value, type) : JsonUtility.FromJson(kvp.Value.Value, type);
                // create generic Fact with proper type
                var factType = typeof(Fact<>).MakeGenericType(type);
                // create Fact instance with deserialized value
                var fact = (IFact)Activator.CreateInstance(factType, value, kvp.Value.IsPersistent);
                
                //factStore.SetFact(kvp.Key, fact, FactDictionary);
                factStore.AllFacts[kvp.Key] = fact;
            }
            return factStore.AllFacts;
        }

        /*
        Cherif's method: uses a SerializableSaveData class with a SerializableFact in order to
            deserialize Facts from the Json database
            Save file contains a dictionary of <key: string, value: SerializableFact>
            SerializableFact only contains Fact.Value and Fact.Value.Type
            SerializableFact could be a struct

        public static void Load(FactDictionary factStore, SaveSlot slot, string stateId)
        {
            var path = GetSavePath(slot, stateId);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Save state '{stateId}' not found in {slot}");

            var json = File.ReadAllText(path);
            var saveFile = JsonConvert.DeserializeObject<SerializableSaveData>(json);

            factStore.AllFacts.Clear();

            foreach (var kvp in saveFile.Facts)
            {
                var type = Type.GetType(kvp.Value.Type);
                var value = JsonConvert.DeserializeObject(kvp.Value.ValueType, type);
                var factType = typeof(Fact<>).MakeGenericType(type);
                var fact = (IFact)Activator.CreateInstance(factType, value, kvp.Value.IsPersistent);
                factStore.AllFacts[kvp.Key] = fact;
            }
        }*/

        #endregion
        
        #region Utils

        private static string GetSavePath(SaveSlot slot)
        {
            Directory.CreateDirectory(SaveDirectoryPath);
            return $"{SaveDirectoryPath}/Save_{slot}.json";
        }
        public void SetPath(string directoryPath)
        {
            SaveDirectoryPath = directoryPath;
        }
        #endregion
    }
}