namespace Beam;

public interface IMultiplayerStorage<TStoredType> : IStorage<TStoredType>, IMultiplayerStorageReference
{
	void ReplicatedOpen(int playerId);

	void ReplicatedClose();

	void ReplicatedPush(TStoredType obj);

	void ReplicatedPop(TStoredType obj);

	void ReplicatedSelect(TStoredType obj);
}
