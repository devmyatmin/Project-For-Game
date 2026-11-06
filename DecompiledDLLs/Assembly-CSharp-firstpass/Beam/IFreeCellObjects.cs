namespace Beam;

public interface IFreeCellObjects<in T> where T : ICellObject
{
	void FreeCellObject(T obj);
}
