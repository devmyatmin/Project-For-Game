using System;
using UnityEngine.Events;

namespace Beam;

[Serializable]
public class MissionStartDetailsUnityEvent : UnityEvent<IMissionDetailsProvider, bool>
{
}
