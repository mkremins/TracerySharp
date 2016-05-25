namespace Tracery
{
	public class PopRulesAction : NodeAction
	{
		private string key;

		public PopRulesAction(string key)
		{
			this.key = key;
		}

		public NodeAction CreateUndo()
		{
			return null;
		}

		public void Activate(Grammar grammar)
		{
			grammar.PopRules(key);
		}
	}
}
