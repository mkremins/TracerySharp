namespace Tracery
{
	public abstract class TraceryNode
	{
		public readonly string Raw;

		public TraceryNode(string raw)
		{
			this.Raw = raw;
		}

		public abstract string Flatten(Grammar grammar);
	}
}
