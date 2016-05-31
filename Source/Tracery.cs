namespace Tracery
{
	public static class Tracery
	{
		public static System.Random Rng = new System.Random();

		public static TraceryNode ParseRule(string rawRule)
		{
			return new Parser(rawRule).Parse();
		}
	}
}
