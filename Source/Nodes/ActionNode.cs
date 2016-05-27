namespace Tracery
{
	public class ActionNode : TraceryNode
	{
		public readonly NodeAction action;

		public ActionNode(NodeAction action, string raw) : base(raw)
		{
			this.action = action;
		}

		public override string Flatten(Grammar grammar)
		{
			// execute the action
			action.Activate(grammar);
			// since this is an action node, we don't need to return any text
			return "";
		}
	}
}
