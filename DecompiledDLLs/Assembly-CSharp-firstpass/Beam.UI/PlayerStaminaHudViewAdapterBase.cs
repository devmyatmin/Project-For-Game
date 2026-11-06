namespace Beam.UI;

public abstract class PlayerStaminaHudViewAdapterBase : ViewAdapterBase
{
	public abstract IGameObjectViewAdapter Panel { get; }

	public abstract void UpdateStamina(float value);

	public abstract void UpdateRunning(bool running);

	public abstract void OutOfBreath(bool outOfBreath);

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
