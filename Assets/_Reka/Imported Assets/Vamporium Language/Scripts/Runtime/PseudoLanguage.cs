using System.Collections.Generic;

using UnityEngine;

namespace Vamporium.VLanguage
{
    public static class PseudoLanguage
    {
        #region Sub-Class Knowledge

        public class Knowledge
        {
            public LanguageData Data;
            public float Learnt;

            public bool IsLearnt { get { return Data == null ? false : Learnt > 0.999f; } }

            public Knowledge(LanguageData data)
            {
                this.Data = data;
                this.Learnt = 0;
            }

            public Knowledge(LanguageData data, float learnt)
            {
                this.Data = data;
                this.Learnt = learnt;
            }
        }

        #endregion

        public static System.Random random;
        private const string splitters = "([-–.,;:?!\n ()])";

        #region Translate

        public static string Translate(LanguageData data, string text, int knownLevels)
        {
            if (data == null) return text;
            return Translate(data, text, data.GetLearntPercentage(knownLevels));
        }

        public static string Translate(LanguageData data, string text, float learnt = 0)
        {
            if (data == null) return text;
            return Translate(new Knowledge(data, learnt), text);
        }

        public static string Translate(Knowledge knowledge, string text)
        {

            // just return original text if no translation is needed
            if (string.IsNullOrEmpty(text)) return text;
            if (knowledge.Data == null) return text;
            if (knowledge.IsLearnt || knowledge.Learnt < 0) return text;

            // split text into words
            string[] words = text.Split(' ', '/');
            words = System.Text.RegularExpressions.Regex.Split(text, splitters);

            // cycle through all words
            for (int i = 0; i < words.Length; i++)
            {
                // get seed based on the word's hash code to have consistent random result
                random = new System.Random(words[i].GetHashCode());

                // translate as word or clusters or individual letters
                if (TranslateWord(knowledge, ref words[i])) continue;
                if (TranslateClusters(knowledge, ref words[i])) continue;
                words[i] = string.Join("", TranslateLetters(knowledge, words[i].ToStringArray()));
            }

            // return the joined back result
            return knowledge.Data.AfterTranslatingAll(string.Join("", words));
        }

        #endregion

        #region Translate : Words

        private static bool TranslateWord(Knowledge knowledge, ref string word)
        {
            // no such word found in the words dictionary
            LanguageEntry entry;
            if (!knowledge.Data.ContainsKey(word, LanguageData.EntryType.Word, out entry))
                return false;

            // get probability of translation
            float lerp = 1;// Mathf.InverseLerp(1, 10, word.Length);
            float rnd = (float)random.NextDouble() * lerp;

            // translate if knowledge and probability is high enough
            if (rnd > knowledge.Learnt)
            {
                string translated = word.CopyCaseFrom(entry.value);
                word = knowledge.Data.AfterTranslatingOne(translated);
            }

            return true;
        }

        #endregion

        #region Translate : Clusters

        private static bool TranslateClusters(Knowledge knowledge, ref string word)
        {
            // prepare clusters and letters lists
            Dictionary<int, string> clusters = new Dictionary<int, string>();
            List<string> letters = new List<string>(word.ToStringArray());

            int start = 0;
            int indexOffset = 0;
            int keyLength;

            // cycle through known clusters
            for (int i = 0; i < knowledge.Data.clusters.Count; i++)
            {
                // cache key and value for easy use
                string key = knowledge.Data.clusters[i].key;
                string value = knowledge.Data.clusters[i].value;
                if (string.IsNullOrEmpty(key)) continue;

                keyLength = key.Length;
                if (keyLength < 2) continue;

                // handle case sensitivity
                var comparison = System.StringComparison.CurrentCultureIgnoreCase;
                if (knowledge.Data.clusters[i].caseSensitive)
                    comparison = System.StringComparison.CurrentCulture;

                for (int j = 0; j < word.Length; j++)
                {
                    // check if cluster is found in word
                    int index = word.IndexOf(key, start, comparison);
                    if (index < 0) break;

                    start += keyLength;

                    int trueIndex = index + indexOffset;
                    int count = letters.Count - trueIndex;
                    if (keyLength > count) keyLength = count;

                    // transform letters of cluster into null strings in the word
                    letters.RemoveRange(trueIndex, keyLength);
                    if (!string.IsNullOrEmpty(value))
                        letters.InsertRange(trueIndex, new string[value.Length]);

                    // translate if knowledge and probability is high enough
                    float rnd = (float)random.NextDouble();
                    if (rnd < knowledge.Learnt)
                    {
                        if (!clusters.ContainsKey(index))
                            clusters.Add(index, key);
                    }
                    else
                    {
                        value = word.Substring(index, keyLength).CopyCaseFrom(value);
                        if (!clusters.ContainsKey(index))
                            clusters.Add(index, knowledge.Data.AfterTranslatingOne(value));
                        indexOffset += value.Length - keyLength;
                    }
                }
            }

            // if no cluster was found, return and translate each individual letter
            if (clusters.Count == 0) return false;

            word = "";

            // cycle through all remaining letters
            for (int i = 0; i < letters.Count; i++)
            {
                // already translated as part of a cluster and it is
                // its first letter => replace with translated cluster
                if (string.IsNullOrEmpty(letters[i]))
                {
                    string translation = string.Empty;
                    if (clusters.TryGetValue(i, out translation))
                        word += translation;
                }

                // translate as individual letter
                else word += TranslateLetter(knowledge, letters[i]);
            }

            return true;
        }

        #endregion

        #region Translate : Letters

        private static string[] TranslateLetters(Knowledge knowledge, string[] letters)
        {
            // cycle through all letters
            for (int i = 0; i < letters.Length; i++)
                letters[i] = TranslateLetter(knowledge, letters[i]);

            return letters;
        }

        private static string TranslateLetter(Knowledge knowledge, string letter)
        {
            // check if the dictionary contains this letter
            LanguageEntry entry;
            if (knowledge.Data.ContainsKey(letter, LanguageData.EntryType.Letter, out entry))
            {
                // translate if knowledge and probability is high enough
                float rnd = (float)random.NextDouble();
                if (rnd < knowledge.Learnt) return letter;

                // save case
                bool upper = letter.IsCapital();

                // translate
                letter = entry.value;

                // apply case

                if (upper)
                {
                    if (!string.IsNullOrEmpty(letter))
                    {
                        if (letter.Length == 1) letter = letter.ToUpper();
                        else letter = letter[0].ToString().ToUpper() + letter.Substring(1);
                    }
                }

                letter = knowledge.Data.AfterTranslatingOne(letter);
            }

            // return translated letter
            return letter;
        }

        #endregion

        #region Utility

        private static string CopyCaseFrom(this string original, string translated)
        {
            // return if check is not needed
            if (string.IsNullOrEmpty(original) || original.Length == 0) return translated;
            if (string.IsNullOrEmpty(translated)) return translated;

            // will check if two consecutive letters have upper case
            // to decide if it is only a name (first letter is upper case)
            // or is a full upper case word

            bool oneLetter = original.Length == 1;

            string letter0 = original[0].ToString();
            string letter1 = oneLetter ? "" : original[1].ToString();

            bool capital0 = letter0.IsCapital();
            bool capital1 = oneLetter ? false : letter1.IsCapital();

            if (capital0 && capital1)
                return translated.ToUpper();
            else if (capital0)
                return translated[0].ToString().ToUpper() + translated.Substring(1);

            return translated;
        }

        private static bool IsCapital(this string letter)
        {
            return letter == letter.ToUpper();
        }

        private static string[] ToStringArray(this string value)
        {
            string[] output = new string[value.Length];
            for (int i = 0; i < value.Length; i++)
                output[i] = value[i].ToString();

            return output;
        }

        #endregion
    }
}