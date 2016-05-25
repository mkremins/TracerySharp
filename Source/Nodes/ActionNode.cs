namespace Tracery
{
	public class ActionNode : TraceryNode
	{
		public NodeAction action;

		public ActionNode(NodeAction action)
		{
			this.action = action;
		}

		public string Flatten(Grammar grammar)
		{
			// execute the action
			action.Activate(grammar);
			// since this is an action node, we don't need to return any text
			return "";
		}
	}
}
