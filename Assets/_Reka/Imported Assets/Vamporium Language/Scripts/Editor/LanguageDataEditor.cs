using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Vamporium.VLanguage;

namespace Vamporium.VLanguageEditor
{
    [CustomEditor(typeof(LanguageData), true)]
    public class LanguageDataEditor : Editor
    {
        private LanguageData data;
        private GUIStyle style;
        private GUIContent caseSensitive;
        private bool changed;
        private string oldTestTex;

        private string translated = string.Empty;

        private void OnEnable()
        {
            data = (LanguageData)target;

            if (data.AutoTranslate)
                translated = data.TestTranslate();
        }

        public override void OnInspectorGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(EditorStyles.label);
                style.wordWrap = true;
            }

            if (caseSensitive == null)
                caseSensitive = new GUIContent("", "Case sensitive");

            EditorGUI.BeginChangeCheck();
            {
                base.OnInspectorGUI();

                EditorGUILayout.Space();

                serializedObject.Update();

                changed = false;
                oldTestTex = data.TestText;

                GUILayout.Label("Testing", EditorStyles.boldLabel);

                GUILayout.BeginVertical("box");
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.TestText)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.Learnt)));

                    GUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginDisabledGroup(data.AutoTranslate);
                        {
                            if (GUILayout.Button("Translate"))
                                translated = data.TestTranslate();
                        }
                        EditorGUI.EndDisabledGroup();

                        data.AutoTranslate = EditorGUILayout.Toggle(data.AutoTranslate, EditorStyles.miniButton, GUILayout.Width(20));
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Translation", GUILayout.Height(20));

                        float w = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 100;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.ShowFormated)), GUILayout.Width(120));
                        EditorGUIUtility.labelWidth = w;

                        style.richText = data.ShowFormated;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        GUILayout.Label(translated, style);
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndVertical();

                }
                GUILayout.EndVertical();

                GUILayout.Space(20);
                GUILayout.Label("Database", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data._cacheDatabase)));

                if (data._cacheDatabase)
                    EditorGUILayout.HelpBox("A cached database is faster but will always be case-sensitive (ignores each entry's setting)!", MessageType.Info);
                else
                    EditorGUILayout.HelpBox("You can turn an entry case-sensitive with the checkbox next to the X button.", MessageType.Info);

                DrawSection("Words", data.words, ref data.ShowWords);
                DrawSection("Clusters", data.clusters, ref data.ShowClusters);
                DrawSection("Letters", data.letters, ref data.ShowLetters);

                serializedObject.ApplyModifiedProperties();
            }

            changed = string.Compare(oldTestTex, data.TestText) > 0;
            if (data.AutoTranslate && (EditorGUI.EndChangeCheck() || changed))
                translated = data.TestTranslate();
        }

        private void DrawSection(string label, List<LanguageEntry> entryList, ref bool show)
        {
            bool isClusterList = entryList == data.clusters;
            bool isLetterList = entryList == data.letters;

            GUILayout.BeginVertical("box");
            {
                EditorGUI.indentLevel++;
                show = EditorGUILayout.Foldout(show, label, true);
                EditorGUI.indentLevel--;

                if (show)
                {
                    if (entryList == null) entryList = new List<LanguageEntry>();

                    for (int i = 0; i < entryList.Count; i++)
                    {
                        var e = entryList[i];
                        if (string.IsNullOrEmpty(e.key)) e.key = "";

                        bool isRed = e.key.Length == 0 
                            || (isClusterList && e.key.Length < 2)
                            || (isLetterList && e.key.Length != 1);

                        if (isRed) GUI.color = Color.red;

                        GUILayout.BeginHorizontal();
                        {
                            e.key = EditorGUILayout.TextField(entryList[i].key);
                            e.value = EditorGUILayout.TextField(entryList[i].value);

                            EditorGUI.BeginDisabledGroup(data._cacheDatabase);
                            e.caseSensitive = EditorGUILayout.Toggle(caseSensitive, e.caseSensitive, GUILayout.Width(16));
                            EditorGUI.EndDisabledGroup();

                            entryList[i] = e;

                            if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(20)))
                            {
                                entryList.RemoveAt(i);
                                break;
                            }
                        }
                        GUILayout.EndHorizontal();

                        if (string.IsNullOrEmpty(e.key)) e.key = "";

                        if (isClusterList && e.key.Length < 2)
                            GUILayout.Label("Cluster keys must have at least two characters!");

                        if (isLetterList && e.key.Length != 1)
                            GUILayout.Label("The Letter key must be one character!");

                        if (isRed) GUI.color = Color.white;
                    }

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Add Entry"))
                            entryList.Add(new LanguageEntry());

                        if (GUILayout.Button("Sort", GUILayout.Width(60)))
                            Sort(entryList);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            EditorGUILayout.Space();
        }

        private void Sort(List<LanguageEntry> entry)
        {
            entry.Sort((a, b) =>
            {
                a.MakeSafe();
                b.MakeSafe();
                return a.key.CompareTo(b.key);
            });
        }
    }
}