using System.Collections.Generic;
using System.IO;
using Database.Runtime.Utils;

namespace Database.Runtime.LanguageLocalization
{
    public class LocalizationSystem
    {
        #region Public
        // Start of the Public region

        public LocalizationSystem(string directoryPath)
        {
            _directoryPath = directoryPath;
            _localizedStrings = new Dictionary<string, string>();
        }
        public Dictionary<string, string> LocalizedStrings => _localizedStrings;
        
        // End of the Public region
        #endregion
        
        #region Main Methods
        // Start of the Main Methods region

        public string GetLocalizedString(string key)
        {
            return LocalizedStrings.TryGetValue(key, out var value) ? value : key;
        }

        public void SaveLanguage(string newLanguage, FactDictionary settingsFactStore, string languageKey)
        {
            // if (settingsFactStore.FactExists<string>("Language", out var languageFact))
            // {
                // settingsFactStore.SetFact("Language", language, FactDictionary.FactPersistence.Persistent);
            // }
            settingsFactStore.SetFact(languageKey, newLanguage, FactDictionary.FactPersistence.Persistent);
        }
        public void LoadLanguage(FactDictionary settingsFactStore, string languageKey)
        {
            var langFile = string.Empty;
            if (settingsFactStore.FactExists<string>(languageKey, out var languageFact))
                langFile = GetLanguageFile(languageFact.ToLower());
            
            if (string.IsNullOrEmpty(langFile)) 
                throw new FileNotFoundException($"Language file for {languageFact} not found.");

            _localizedStrings = XmlLoader.Load(langFile);
        }
        
        // End of the Main Methods region
        #endregion
        
        #region Utils
        // Start of the Utils region
        private string GetLanguageFile(string language)
        {
            var languageFile = $"{_directoryPath}/{language}.xml";
            return File.Exists(languageFile) ? languageFile : string.Empty;
        }
        // End of the Utils region
        #endregion
        
        #region Private
        // Start of the Private region
        private string _directoryPath;

        private Dictionary<string, string> _localizedStrings;
        // End of the Private region
        #endregion
    }
}