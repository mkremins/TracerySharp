using System.Linq;

namespace Tracery
{
	public class TagNode : TraceryNode
	{
		private string key;
		private string[] modifiers;
		private NodeAction[] preActions;
		private NodeAction[] postActions;

		public TagNode(string key, string[] modifiers, NodeAction[] preActions)
		{
			this.key = key;
			this.modifiers = modifiers;
			this.preActions = preActions;
			this.postActions = preActions
				.Select((action) => action.CreateUndo())
				.Where((action) => action != null)
				.ToArray();
		}

		public string Flatten(Grammar grammar)
		{
			foreach (NodeAction preAction in preActions)
			{
				preAction.Activate(grammar);
			}

			string text = grammar.SelectRule(key).Flatten(grammar);
			foreach (string modifierName in modifiers)
			{
				text = grammar.ApplyModifier(modifierName, text);
			}

			foreach (NodeAction postAction in postActions)
			{
				postAction.Activate(grammar);
			}

			return text;
		} 
	}
}
