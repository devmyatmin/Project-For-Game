using System;
using UnityEngine.Events;

namespace Beam;

[Serializable]
public class MissionDetailsUnityEvent : UnityEvent<IMissionDetailsProvider>
{
}
