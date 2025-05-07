using UnityEngine;
using TMPro;
using Vamporium.VLanguage;

public class LanguageDocumentTMPro : MonoBehaviour
{
    [SerializeField, TextArea(3, 20)] private string _text = string.Empty;
    [SerializeField] private LanguageData _languageData = null;
    [SerializeField] private TMP_Text _textComponent = null;
    [SerializeField] private bool _translateOnEnable = true;
    [SerializeField] private bool _translateOnLearn = true;
    [SerializeField] private bool _bypassTranslation = false;

    private LanguageKnowledge _lk;

    private void Awake()
    {
        _lk = FindObjectOfType<LanguageKnowledge>();
        if (_lk) _lk.OnAnyChange += OnAnyChange;
        else Debug.LogError("No LanguageKnowledge component was found!");
    }

    private void OnDestroy()
    {
        if (_lk) _lk.OnAnyChange -= OnAnyChange;
    }

    private void OnAnyChange(LanguageData data)
    {
        if (!_translateOnLearn) return;
        if (data != _languageData) return;
        TranslateText();
    }

    private void OnEnable()
    {
        if (_translateOnEnable) TranslateText();
    }

    public void TranslateText()
    {
        _textComponent.text = _bypassTranslation ? _text : _lk.TranslateText(_languageData, _text);
    }
}
