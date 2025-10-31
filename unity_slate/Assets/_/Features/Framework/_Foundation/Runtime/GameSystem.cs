using Database.Runtime;
using UnityEngine;

namespace Foundation.Runtime
{
    public class GameSystem : FBehaviour
    {
        #region Public
        // Start of the Public region
        public static string GameFactsDirectoryPath => _gameFactsDirectoryPath;
        public static FactDictionary m_gameFacts
        {
            get
            {
                if (_gameFacts != null) return _gameFacts;
                _gameFacts = new FactDictionary(_gameFactsDirectoryPath);
                return _gameFacts;
            }
        }

        public static JsonSaveSystem m_jsonSaveSystem
        {
            get
            {
                if (_jsonSaveSystem != null) return _jsonSaveSystem;
                _jsonSaveSystem = new JsonSaveSystem();
                _jsonSaveSystem.SetPath(_gameFacts.SaveDirectoryPath);
                return _jsonSaveSystem;
            }
        }
        
        // End of the Public region
        #endregion
        
        #region Private
        // Start of the Private region
        
        private static FactDictionary _gameFacts;
        private static JsonSaveSystem _jsonSaveSystem;
        
        private static readonly string _gameFactsDirectoryPath = $"{Application.persistentDataPath}/Data/GameFacts/";

        // End of the Private region
        #endregion
    }
}