using System;
using UnityEngine.Events;

namespace Beam.Crafting;

[Serializable]
public class GameActionEvent : UnityEvent<IBase>
{
}
