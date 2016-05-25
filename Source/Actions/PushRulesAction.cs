using System.Linq;

namespace Tracery
{
	public class PushRulesAction : NodeAction
	{
		private string key;
		private TraceryNode[] rules;

		public PushRulesAction(string key, TraceryNode[] rules)
		{
			this.key = key;
			this.rules = rules;
		}

		public NodeAction CreateUndo()
		{
			return new PopRulesAction(key);
		}

		public void Activate(Grammar grammar)
		{
			// eagerly flatten the rules to plaintext
			TraceryNode[] textRules = rules
				.Select((node) => new PlaintextNode(node.Flatten(grammar)))
				.ToArray();
			// push the flattened rules to the grammar
			grammar.PushRules(key, textRules);
		}
	}
}
