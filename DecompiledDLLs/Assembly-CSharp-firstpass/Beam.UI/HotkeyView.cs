using System.Collections.Generic;

namespace Beam.UI;

public abstract class HotkeyView : ViewAdapterBase
{
	public abstract void OnAssignmentCreated();

	public abstract void OnAssignmentRemoved();

	public abstract void OnAssignmentFailed();

	public abstract void Initialize(IEnumerable<HotkeyData> hotkeys);
}
