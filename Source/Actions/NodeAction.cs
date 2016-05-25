namespace Tracery
{
	public interface NodeAction
	{
		NodeAction CreateUndo();
		void Activate(Grammar grammar);
	}
}
