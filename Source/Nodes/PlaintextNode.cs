namespace Tracery
{
	public class PlaintextNode : TraceryNode
	{
		private string text;

		public PlaintextNode(string text)
		{
			this.text = text;
		}

		public string Flatten(Grammar grammar)
		{
			return text;
		}
	}
}
