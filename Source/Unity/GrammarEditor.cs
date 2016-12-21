using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Tracery;

[CustomEditor(typeof(TraceryGrammar))]
public class GrammarEditor : Editor
{
	private int selected = 0;
	private string testValue = null;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		SerializedProperty symbols = serializedObject.FindProperty("symbols");

		selected = GUILayout.Toolbar(selected, new string[]{"Rules", "JSON"});
		GUILayout.Space(5);

		if (selected == 0)
		{
			for (int i = 0; i < symbols.arraySize; i++)
			{
				SerializedProperty symbol = symbols.GetArrayElementAtIndex(i);
				SerializedProperty key = symbol.FindPropertyRelative("key");
				SerializedProperty rules = symbol.FindPropertyRelative("rules");

				GUILayout.BeginHorizontal();
				key.stringValue = GUILayout.TextField(key.stringValue, GUILayout.Width(120f));
				bool remove = GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(60f));
				GUILayout.EndHorizontal();

				if (remove)
				{
					symbols.DeleteArrayElementAtIndex(i);
				}
				else
				{
					rules.stringValue = GUILayout.TextArea(rules.stringValue);
				}

				GUILayout.Space(5);
			}

			if (GUILayout.Button("Add Rule"))
			{
				symbols.InsertArrayElementAtIndex(symbols.arraySize);

				// set default values
				SerializedProperty symbol = symbols.GetArrayElementAtIndex(symbols.arraySize - 1);
				SerializedProperty key = symbol.FindPropertyRelative("key");
				SerializedProperty rules = symbol.FindPropertyRelative("rules");
				key.stringValue = "ruleName";
				rules.stringValue = "rules";
			}

			GUILayout.Space(5);

			if (GUILayout.Button("Test Grammar") || testValue == null)
			{
				Grammar grammar = ((TraceryGrammar)target).Grammar;
				testValue = grammar.Flatten("#origin#");
			}

			GUILayout.Label(testValue);
		}

		else if (selected == 1)
		{
			GUILayout.Label("Paste JSON here", EditorStyles.boldLabel);
			string jsonInput = EditorGUILayout.TextArea("");
			if (jsonInput != "")
			{
				try
				{
					LoadFromJSON(jsonInput, symbols);
					selected = 0; // switch back to the Rules tab
					testValue = null; // force recalculation of the test value
				}
				catch (Exception cause)
				{
					Exception wrapper = new Exception("Couldn't parse JSON string", cause);
					Debug.LogException(wrapper);
				}
			}

			GUILayout.Space(5);

			GUILayout.Label("Copy JSON here", EditorStyles.boldLabel);
			EditorGUILayout.SelectableLabel(ToJSONString(symbols), EditorStyles.miniLabel);
		}

		serializedObject.ApplyModifiedProperties();
	}

	private void LoadFromJSON(string jsonString, SerializedProperty symbols)
	{
		symbols.ClearArray();

		JSONClass root = JSON.Parse(jsonString).AsObject;
		if (root == null)
		{
			throw new Exception("JSON-serialized grammar must be object");
		}

		foreach (KeyValuePair<string,JSONNode> pair in root)
		{
			JSONArray val = pair.Value.AsArray;
			if (val == null)
			{
				// TODO support non-array values (e.g. strings, objects) for top-level keys?
				throw new Exception("Value for top-level key must be array");
			}
			// TODO throw if one of the array items isn't a string?
			IEnumerable<string> rules = val.Children.Select((json) => json.Value);

			symbols.InsertArrayElementAtIndex(symbols.arraySize);
			SerializedProperty symbol = symbols.GetArrayElementAtIndex(symbols.arraySize - 1);
			symbol.FindPropertyRelative("key").stringValue = pair.Key;
			symbol.FindPropertyRelative("rules").stringValue = string.Join("\n", rules.ToArray());
		}
	}

	private string ToJSONString(SerializedProperty symbols)
	{
		JSONClass root = new JSONClass();
		for (int i = 0; i < symbols.arraySize; i++)
		{
			SerializedProperty symbol = symbols.GetArrayElementAtIndex(i);
			string key = symbol.FindPropertyRelative("key").stringValue;
			string rulesString = symbol.FindPropertyRelative("rules").stringValue;
			JSONArray rules = new JSONArray();
			foreach (string rule in rulesString.Split('\n'))
			{
				rules.Add(new JSONData(rule));
			}
			root.Add(key, rules);
		}
		return root.ToJSON(0);
	}
}
