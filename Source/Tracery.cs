namespace Tracery
{
	public static class Tracery
	{
		public static TraceryNode ParseRule(string rawRule)
		{
			return new Parser(rawRule).Parse();
		}
	}
}
