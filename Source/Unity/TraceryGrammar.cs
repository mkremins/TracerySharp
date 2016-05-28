using UnityEngine;
using Tracery;

[System.Serializable]
public class TraceryGrammar : MonoBehaviour
{
	public Symbol[] symbols;

	public Grammar Grammar
	{
		get
		{
			Grammar grammar = new Grammar();
			foreach (Symbol s in symbols)
			{
				grammar.PushRules(s.key, s.rules.Split('\n'));
			}
			return grammar;
		}
	}

	[System.Serializable]
	public class Symbol
	{
		public string key;
		public string rules;
	}
}
