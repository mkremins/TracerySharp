using UnityEditor;
using UnityEngine;
using Tracery;

[CustomEditor(typeof(TraceryGrammar))]
public class GrammarEditor : Editor
{
	private string testValue = "";

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		SerializedProperty symbols = serializedObject.FindProperty("symbols");

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

		if (GUILayout.Button("Test Grammar"))
		{
			Grammar grammar = ((TraceryGrammar)target).Grammar;
			testValue = grammar.Flatten("#origin#");
		}

		GUILayout.Label(testValue);

		serializedObject.ApplyModifiedProperties();
	}
}
