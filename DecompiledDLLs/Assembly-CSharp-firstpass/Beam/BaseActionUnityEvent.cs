using System;
using UnityEngine.Events;

namespace Beam;

[Serializable]
public class BaseActionUnityEvent : UnityEvent<IBase, IBaseActionEventData>
{
}
