namespace Vamporium.VLanguage
{
    [System.Serializable]
    public struct LanguageEntry
    {
        public string key, value;
        public bool caseSensitive;

        public LanguageEntry(string key, string value, bool caseSensitive)
        {
            this.key = key;
            this.value = value;
            this.caseSensitive = caseSensitive;
        }

        public LanguageEntry(string key, string value)
        {
            this.key = key;
            this.value = value;
            this.caseSensitive = false;
        }

        public void MakeSafe()
        {
            if (key == null) key = "";
            if (value == null) value = "";
        }
    }
}