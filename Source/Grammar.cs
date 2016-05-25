using System;
using System.Collections.Generic;
using System.Linq;

namespace Tracery
{
	public class Grammar
	{
		private IDictionary<string,Func<string,string>> modifiers;
		private IDictionary<string,Stack<TraceryNode[]>> symbols;
		private System.Random rand;

		public Grammar()
		{
			modifiers = new Dictionary<string,Func<string,string>>();
			AddModifiers(BaseEngModifiers.BASE_ENG_MODIFIERS);
			symbols = new Dictionary<string,Stack<TraceryNode[]>>();
			rand = new System.Random();
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

		public void PushRules(string key, TraceryNode[] rules)
		{
			Stack<TraceryNode[]> stack;
			if (!symbols.TryGetValue(key, out stack))
			{
				stack = new Stack<TraceryNode[]>();
				symbols.Add(key, stack);
			}
			stack.Push(rules);
		}

		public void PushRules(string key, string[] rawRules)
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
			return items[rand.Next(items.Length)];
		}
	}
}
