using UnityEngine;

namespace LS_LevelStreaming;

public interface LS_ITriggerByUpdatedPosition : LS_ITrigger
{
	void Update(Vector3 p_newPosition);
}
