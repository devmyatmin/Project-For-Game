namespace Beam.UI;

public abstract class DisplayMissionNameViewAdapterBase : ViewAdapterBase
{
	public abstract IGameObjectViewAdapter Panel { get; }

	public abstract ILabelViewAdapter MissionNameLabel { get; }

	public abstract ICanvasGroupViewAdapter MissionNameGroup { get; }

	public override void Show()
	{
		if (!Panel.Active)
		{
			Panel.Active = true;
			base.Show();
		}
	}

	public override void Hide()
	{
		if (Panel.Active)
		{
			Panel.Active = false;
			base.Hide();
		}
	}
}
