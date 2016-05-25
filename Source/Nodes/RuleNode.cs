using System.Text;

namespace Tracery
{
	public class RuleNode : TraceryNode
	{
		private TraceryNode[] sections;

		public RuleNode(TraceryNode[] sections)
		{
			this.sections = sections;
		}

		public string Flatten(Grammar grammar)
		{
			StringBuilder builder = new StringBuilder();
			foreach (TraceryNode section in sections)
			{
				builder.Append(section.Flatten(grammar));
			}
			return builder.ToString();
		}
	}
}
