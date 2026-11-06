namespace Beam.UI;

public abstract class PlayerFishingHudViewAdapterBase : ViewAdapterBase
{
	public abstract IGameObjectViewAdapter Panel { get; }

	public abstract void UpdateReel(float value);

	public abstract void UpdateReelShow(bool show);

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
