using UnityEngine;

namespace Beam;

public interface ISpearable
{
	InteractiveType Type { get; }

	bool CanSpear { get; }

	bool IsLocalFauna { get; }

	uint LocalFaunaSpearedInstancePrefabId { get; }

	ISpearedInstance GetSpearedInstance(Vector3 position);

	bool LocalFaunaCheckHit(Vector3 postion);

	void LocalFaunaRelease();
}
