using System.Collections.Generic;

using UnityEngine;

namespace Vamporium.VLanguage
{
    [CreateAssetMenu(menuName = "Vamporium/Language/Language Data")]
    public partial class LanguageData : ScriptableObject
    {
        public enum EntryType { Word, Cluster, Letter }

        [Header("Basic Info")]
        public string Name = "New Pseudo Language";
        public string Description = string.Empty;
        public int Difficulty = 5;

        [Header("Formatting")]
        [SerializeField] private string _formattingPrefix = string.Empty;
        [SerializeField] private string _formattingSuffix = string.Empty;
        [SerializeField] private bool _simplifyFormatting = true;

        [HideInInspector] public bool _cacheDatabase;

        [HideInInspector] public List<LanguageEntry> words = new List<LanguageEntry>();
        [HideInInspector] public List<LanguageEntry> clusters = new List<LanguageEntry>();
        [HideInInspector] public List<LanguageEntry> letters = new List<LanguageEntry>();

        private Dictionary<string, LanguageEntry> words_cached = null;
        private Dictionary<string, LanguageEntry> clusters_cached = null;
        private Dictionary<string, LanguageEntry> letters_cached = null;

        #region Get Entry List/Dictionary

        private List<LanguageEntry> GetEntryList(EntryType type)
        {
            switch (type)
            {
                case EntryType.Word: return words;
                case EntryType.Cluster: return clusters;
                case EntryType.Letter: return letters;
            }
            return null;
        }

        private Dictionary<string, LanguageEntry> GetEntryDictionary(EntryType type)
        {
            switch (type)
            {
                case EntryType.Word:
                    if (words_cached == null) words_cached = CacheListToDictionary(words);
                    return words_cached;
                case EntryType.Cluster:
                    if (clusters_cached == null) clusters_cached = CacheListToDictionary(clusters);
                    return clusters_cached;
                case EntryType.Letter:
                    if (letters_cached == null) letters_cached = CacheListToDictionary(letters);
                    return letters_cached;
            }
            return null;
        }

        #endregion

        #region Chache List

        private Dictionary<string, LanguageEntry> CacheListToDictionary(List<LanguageEntry> list)
        {
            Dictionary<string, LanguageEntry> d = new Dictionary<string, LanguageEntry>();
            for (int i = 0; i < list.Count; i++)
                if (!d.ContainsKey(list[i].key))
                    d.Add(list[i].key, list[i]);

            return d;
        }

        #endregion

        public bool ContainsKey(string key, EntryType type, out LanguageEntry entry)
        {
            if (_cacheDatabase)
                return ContainsKeyInDictionary(key, type, out entry);
            else
                return ContainsKeyInList(key, type, out entry);
        }

        #region Contains Key In List / Dictionary

        private bool ContainsKeyInList(string key, EntryType type, out LanguageEntry entry)
        {
            var list = GetEntryList(type);

            entry = new LanguageEntry();
            if (list == null) return false;

            for (int i = 0; i < list.Count; i++)
            {
                string k = key;
                if (!string.IsNullOrEmpty(list[i].key))
                {
                    if (!list[i].caseSensitive) k = k.ToLower();
                    if (list[i].key == k)
                    {
                        entry = list[i];
                        break;
                    }
                }
            }

            return !string.IsNullOrEmpty(entry.key);
        }

        private bool ContainsKeyInDictionary(string key, EntryType type, out LanguageEntry entry)
        {
            var d = GetEntryDictionary(type);
            return d.TryGetValue(key, out entry);
        }

        #endregion

        #region Handle Text After ranslation

        public virtual string AfterTranslatingOne(string text)
        {
            if (string.IsNullOrEmpty(_formattingPrefix) || string.IsNullOrEmpty(_formattingSuffix)) return text;
            return _formattingPrefix + text + _formattingSuffix;
        }

        public virtual string AfterTranslatingAll(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            if (_simplifyFormatting)
                text = text.Replace(_formattingSuffix + _formattingPrefix, "");

            text = text.Replace("  ", " ");

            return text;
        }

        #endregion

        #region Utils

        public float GetLearntPercentage(int knownLevels)
        {
            return knownLevels / (float)Difficulty;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        [HideInInspector] public bool ShowWords, ShowClusters, ShowLetters;
        [HideInInspector] public bool DebugMode, AutoTranslate, ShowFormated;

        [SerializeField, HideInInspector, TextArea] public string TestText = "";
        [SerializeField, HideInInspector, Range(0, 1)] public float Learnt = 0;

        public string TestTranslate()
        {
            return PseudoLanguage.Translate(this, TestText, Learnt);
        }
#endif

        #endregion

    }
}