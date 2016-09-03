using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace Tracery
{
	public class Grammar
	{
		private IDictionary<string,Func<string,string>> modifiers;
		private IDictionary<string,Stack<TraceryNode[]>> symbols;

		public Grammar()
		{
			modifiers = new Dictionary<string,Func<string,string>>();
			AddModifiers(Modifiers.BASE_ENG_MODIFIERS);
			symbols = new Dictionary<string,Stack<TraceryNode[]>>();
		}

		public static Grammar LoadFromJSON(string jsonString)
		{
			JSONClass root = JSON.Parse(jsonString).AsObject;
			if (root == null)
			{
				throw new Exception("JSON-serialized grammar must be object");
			}

			Grammar grammar = new Grammar();

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
				grammar.PushRules(pair.Key, rules);
			}

			return grammar;
		}

		public static Grammar LoadFromJSON(TextAsset jsonFile)
		{
			return LoadFromJSON(jsonFile.text);
		}

		public void AddModifiers(IDictionary<string,Func<string,string>> modifiers)
		{
			foreach (string modifierName in modifiers.Keys)
			{
				this.modifiers.Add(modifierName, modifiers[modifierName]);
			}
		}

		public string ApplyModifier(string modifierName, string text)
		{
			Func<string,string> modifier;
			if (!modifiers.TryGetValue(modifierName, out modifier))
			{
				throw new Exception("No registered modifier named " + modifierName);
			}
			return modifier.Invoke(text);
		}

		public void PushRules(string key, IEnumerable<TraceryNode> rules)
		{
			Stack<TraceryNode[]> stack;
			if (!symbols.TryGetValue(key, out stack))
			{
				stack = new Stack<TraceryNode[]>();
				symbols.Add(key, stack);
			}
			stack.Push(rules.ToArray());
		}

		public void PushRules(string key, IEnumerable<string> rawRules)
		{
			TraceryNode[] rules = rawRules.Select((raw) => Tracery.ParseRule(raw)).ToArray();
			PushRules(key, rules);
		}

		public void PopRules(string key)
		{
			GetRulesStack(key).Pop();
		}

		public TraceryNode SelectRule(string key)
		{
			return RandItem(GetRulesStack(key).Peek());
		}

		public string Flatten(string rawRule)
		{
			TraceryNode rule = Tracery.ParseRule(rawRule);
			return rule.Flatten(this);
		}

		private Stack<TraceryNode[]> GetRulesStack(string key)
		{
			Stack<TraceryNode[]> stack;
			if (!symbols.TryGetValue(key, out stack))
			{
				throw new Exception("No registered symbol named " + key);
			}
			return stack;
		}

		private T RandItem<T>(T[] items)
		{
			return items[Tracery.Rng.Next(items.Length)];
		}
	}
}
