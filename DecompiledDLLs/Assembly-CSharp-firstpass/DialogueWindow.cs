using Beam.UI;
using Beam.Utilities;
using UnityEngine;

public class DialogueWindow : ViewAdapterBase
{
	public UButtonViewAdapter YesButton;

	public UButtonViewAdapter NoButton;

	public ULabelViewAdapter TitleText;

	public ULabelViewAdapter MessageText;

	protected virtual void Awake()
	{
		if (Object.op_Implicit((Object)(object)YesButton))
		{
			YesButton.Click += (object sender) =>
			{
				BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
			};
		}
		if (Object.op_Implicit((Object)(object)NoButton))
		{
			NoButton.Click += (object sender) =>
			{
				BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
			};
		}
	}

	protected virtual void Start()
	{
		Show();
	}

	public void SetTitle(string title)
	{
		if (Object.op_Implicit((Object)(object)TitleText))
		{
			TitleText.Text = title.ToUpper();
		}
	}

	public void SetMessage(string message)
	{
		if (Object.op_Implicit((Object)(object)MessageText))
		{
			MessageText.Text = message;
		}
	}
}
