using UnityEngine;
using UnityEngine.UI;
using Vamporium.VLanguage;

public class LanguageDocumentText : MonoBehaviour
{
    [SerializeField, TextArea(3, 20)] private string _text = string.Empty;
    [SerializeField] private LanguageData _languageData = null;
    [SerializeField] private Text _textComponent = null;
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
