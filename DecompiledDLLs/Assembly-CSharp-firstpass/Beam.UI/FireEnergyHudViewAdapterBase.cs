namespace Beam.UI;

public abstract class FireEnergyHudViewAdapterBase : ViewAdapterBase
{
	public abstract IGameObjectViewAdapter Panel { get; }

	public abstract void UpdateEnergy(float value);

	public abstract void UpdateFiring(bool firing);

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
