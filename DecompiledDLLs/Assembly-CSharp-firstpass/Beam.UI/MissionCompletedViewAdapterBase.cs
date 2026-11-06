namespace Beam.UI;

public abstract class MissionCompletedViewAdapterBase : ViewAdapterBase
{
	public abstract IGameObjectViewAdapter Panel { get; }

	public abstract IMissionRewardElement CreateRewardElement();

	public abstract ActionHudElementViewAdapterBase CreateActionLabel();

	public abstract void DestroyRewardElements();

	public abstract void DestroyActionLabels();

	public override void Show()
	{
		if (!Panel.Active)
		{
			Panel.Active = true;
		}
		base.Show();
	}

	public override void Hide()
	{
		if (Panel.Active)
		{
			Panel.Active = false;
		}
		base.Hide();
	}
}
