using System;
using System.Collections.Generic;
using System.Linq;
using Database.Runtime.Interfaces;

namespace Database.Runtime
{
    public class FactDictionary
    {
        #region Public
        // Start of the Public region
        public FactDictionary(string saveDirectoryPath)
        {
            _saveDirectoryPath = saveDirectoryPath;
        }
        public Dictionary<string, IFact> AllFacts => _facts;
        public string SaveDirectoryPath => _saveDirectoryPath;
        
        public enum FactPersistence
        {
            Normal,
            Persistent,
        }
        // End of the Public region
        #endregion
        
        #region Main Methods
        public bool FactExists<T>(string key, out T value)
        {
            if (_facts.TryGetValue(key, out var fact) && fact is Fact<T> typedFact)
            {
                value = typedFact.Value;
                return true;
            }
            
            value = default;
            return false;
        }

        public void SetFact<T>(string key, T value, FactPersistence persistence)
        {
            
            if (_facts.TryGetValue(key, out var existingFact))
            {
                if (existingFact is Fact<T> typedFact)
                {
                    typedFact.Value = value;
                    typedFact.IsPersistent = persistence == FactPersistence.Persistent;
                }
                else throw new InvalidCastException("Fact exists but is a wrong type");
            }
            else
            {
                bool isPersistent = persistence == FactPersistence.Persistent;
                _facts[key] = new Fact<T>(value, isPersistent);
                // _facts.Add(key, new Fact<T>(value, isPersistent));
                //Debug.Log($"Added fact: {_facts[key]}");
            }
        }

        public T GetFact<T>(string key)
        {
            if (!_facts.TryGetValue(key, out var fact)) throw new KeyNotFoundException($"Fact {key} does not exist");

            if (_facts[key] is not Fact<T> typedFact) throw new InvalidCastException($"Fact {key} is not of type {typeof(T)}");
            
            return typedFact.Value;
        }

        public Dictionary<string, IFact> GetPersistentFacts()
        {
            var persistentFacts = _facts.Where(fact => fact.Value.IsPersistent)
                .ToDictionary(fact => fact.Key, fact => fact.Value);

            return persistentFacts;
        }

        public void RemoveFact<T>(string key)
        {
            _facts.Remove(key);
        }
        
        #endregion
        
        #region Private and Protected
        // Start of the Private region
        private Dictionary<string, IFact> _facts = new Dictionary<string, IFact>();
        private string _saveDirectoryPath;
        
        // End of the Private region
        #endregion
    }
}