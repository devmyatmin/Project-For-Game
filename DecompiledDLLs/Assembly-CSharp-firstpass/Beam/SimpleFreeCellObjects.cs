namespace Beam;

public class SimpleFreeCellObjects<TCellObject> : IFreeCellObjects<TCellObject> where TCellObject : ICellObject
{
	public void FreeCellObject(TCellObject obj)
	{
		obj.Destroy();
	}
}
