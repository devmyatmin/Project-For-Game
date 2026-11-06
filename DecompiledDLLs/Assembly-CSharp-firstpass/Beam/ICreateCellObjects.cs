namespace Beam;

public interface ICreateCellObjects<T> where T : ICellObject
{
	T CreateCellObject(T prefab);
}
