using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vamporium.VLanguage
{
    public class LanguageKnowledge : MonoBehaviour
    {
        public event Action<LanguageData> OnAnyChange;
        public event Action<PseudoLanguage.Knowledge> OnLanguageLearn, OnLanguageUnlearn;
        public event Action<LanguageData> OnLanguagePerfected, OnLanguageForget;

        [SerializeField] private Dictionary<LanguageData, int> _languages = new Dictionary<LanguageData, int>();

        public string TranslateText(LanguageData data, string text)
        {
            int known = 0;
            _languages.TryGetValue(data, out known);
            return PseudoLanguage.Translate(data, text, known);
        }

        public void LearnLanguage(LanguageData data, int amount)
        {
            if (data == null || amount == 0) return;
            if (!_languages.ContainsKey(data)) _languages.Add(data, 0);

            var value = Mathf.Clamp(_languages[data] + amount, 0, data.Difficulty);
            _languages[data] = value;

            bool learning = amount > 0;
            if (learning)
            {
                if (IsLanguagePerfected(data, value))
                {
                    if (OnLanguagePerfected != null)
                        OnLanguagePerfected.Invoke(data);
                }
                else
                {
                    if (OnLanguageLearn != null)
                        OnLanguageLearn.Invoke(GetKnowledge(data, value));
                }
            }
            else
            {
                if (value <= 1)
                {
                    if (OnLanguageForget != null)
                        OnLanguageForget.Invoke(data);
                }
                else
                { 
                    if (OnLanguageUnlearn != null)
                        OnLanguageUnlearn.Invoke(GetKnowledge(data, value));
                }
            }

            if (OnAnyChange != null)
                OnAnyChange.Invoke(data);
        }

        private bool IsLanguagePerfected(LanguageData data, int amount)
        {
            return amount >= data.Difficulty;
        }

        private PseudoLanguage.Knowledge GetKnowledge(LanguageData data, int value)
        {
            return new PseudoLanguage.Knowledge(data, data.GetLearntPercentage(value));
        }
    }
}