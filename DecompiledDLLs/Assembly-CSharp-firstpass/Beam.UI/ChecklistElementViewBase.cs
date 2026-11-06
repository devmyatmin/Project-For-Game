using UnityEngine;

namespace Beam.UI;

public abstract class ChecklistElementViewBase : MonoBehaviour
{
	public abstract bool IsBusy { get; }

	public abstract void SetDescription(string description);

	public abstract void SetComplete();
}
