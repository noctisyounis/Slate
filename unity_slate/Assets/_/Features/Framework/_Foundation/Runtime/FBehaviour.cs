using Database.Runtime;
using Database.Runtime._.Features.Framework.Database.Runtime.Enums;
using UnityEngine;

namespace Foundation.Runtime
{
    public class FBehaviour : MonoBehaviour
    {
        #region Public
        public bool m_debug {get => _debug; set => _debug = value; }
        public bool m_warning {get => _warning; set => _warning = value; }
        public bool m_error {get => _error; set => _error = value; }
        #endregion
   
        #region Fact Dictionary

        protected bool FactExists<T>(string key, out T value)
        {
            GameFacts.FactExists<T>(key, out value);
            return value != null;
        }

        protected void SetFact<T>(string key, T value, bool isPersistent)
        {
            GameFacts.SetFact<T>(key, value, isPersistent ? FactDictionary.FactPersistence.Persistent : FactDictionary.FactPersistence.Normal);
        }

        protected T GetFact<T>(string key)
        {
            return GameFacts.GetFact<T>(key);
        }

        protected void RemoveFact<T>(string key)
        {
            GameFacts.RemoveFact<T>(key);
        }

        protected void SetFactPersistence(bool isPersistent)
        {
            _persistence = isPersistent ? FactDictionary.FactPersistence.Persistent : FactDictionary.FactPersistence.Normal;
        }
   
        #endregion 
   
        #region Save/Load

        protected void Save()
        {
            InfoInProgress($"Saving Game");
            var saved = SaveSystem.Save(GameFacts.GetPersistentFacts(), SaveSlot.First_Slot);
            InfoDone($"Game saved: {saved}");
        }

        protected void Load()
        {
            InfoInProgress($"Loading Game");
            var loaded = SaveSystem.Load(GameFacts, SaveSlot.First_Slot);
            foreach (var fact in loaded)
            {
                InfoDone($"Fact: {fact.Key} = {fact.Value.GetObjectValue}");
            }
            //InfoDone($"Game loaded: {loaded}");
        }

        #endregion
   
        #region Debug

        protected void Info(string message, Object context = null)
        {
            if (!_debug) return;
            Debug.Log($"<color=cyan> FROM: {this} | INFO: {message} </color>", context);
        }
   
        protected void InfoInProgress(string message, Object context = null)
        {
            if (!_debug) return;
            Debug.Log($"<color=orange> FROM: {this} | IN_PROGRESS: {message} </color>", context);
        }
        
        protected void InfoDone(string message, Object context = null)
        {
            if (!_debug) return;
            Debug.Log($"<color=green> FROM: {this} | DONE: {message} </color>", context);
        }

        protected void Warning(string message, Object context = null)
        {
            if (!_warning) return;
            Debug.LogWarning($"<color=yellow> FROM: {this} | WARNING: {message} </color>", context);
        }

        protected void Error(string message, Object context = null)
        {
            if (!_error) return;
            Debug.LogError($"<color=red> FROM: {this} | ERROR: {message} </color>", context);
        }
   
        #endregion
   
        #region Private and Protected
        
        // [Header("Console Debug")]
        [SerializeField, HideInInspector]
        protected bool _debug;
        [SerializeField, HideInInspector]
        protected bool _warning;
        [SerializeField, HideInInspector]
        protected bool _error;
        
        protected static FactDictionary GameFacts => GameSystem.m_gameFacts;
        protected static JsonSaveSystem SaveSystem => GameSystem.m_jsonSaveSystem;
        
        protected static string _savePath = "SaveData.json";
        protected FactDictionary.FactPersistence _persistence = FactDictionary.FactPersistence.Normal;
   
        // protected static GameSystem System => GameSystem.Instance;

        #endregion
    }
}
