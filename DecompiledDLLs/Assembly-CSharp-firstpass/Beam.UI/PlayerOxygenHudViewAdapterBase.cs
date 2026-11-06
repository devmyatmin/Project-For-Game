namespace Beam.UI;

public abstract class PlayerOxygenHudViewAdapterBase : ViewAdapterBase
{
	public abstract IGameObjectViewAdapter Panel { get; }

	public abstract void UpdateOxygen(float value);

	public abstract void UpdateUnderwater(bool underwater);

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
