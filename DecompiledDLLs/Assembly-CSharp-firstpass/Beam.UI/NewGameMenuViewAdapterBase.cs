using UnityEngine;

namespace Beam.UI;

public abstract class NewGameMenuViewAdapterBase : MenuViewAdapterBase
{
	public abstract IOptionButtonViewAdapter ModeOptionButton { get; }

	public abstract IOptionButtonViewAdapter DifficultyOptionButton { get; }

	public abstract IOptionButtonViewAdapter WorldOptionButton { get; }

	public abstract IOptionButtonViewAdapter GenderOptionButton { get; }

	public abstract IOptionButtonViewAdapter Gender2OptionButton { get; }

	public abstract IOptionButtonViewAdapter PermadeathOptionButton { get; }

	public abstract IOptionButtonViewAdapter WildlifeOptionButton { get; }

	public abstract IOptionButtonViewAdapter StartingCrateOptionButton { get; }

	public abstract ILabelButtonViewAdapter KickButton { get; }

	public abstract ILabelButtonViewAdapter StartButton { get; }

	public abstract ILabelButtonViewAdapter BackButton { get; }

	public abstract ILabelViewAdapter TitleLabel { get; }

	public abstract GameObject LobbyDetails { get; }

	public abstract IButtonViewAdapter CopyCodeButton { get; }

	public abstract ILabelViewAdapter LobbyNameLabel { get; }

	public abstract ILabelViewAdapter RegionLabel { get; }

	public abstract ILabelViewAdapter PingLabel { get; }

	public abstract ILabelViewAdapter CodeLabel { get; }

	public abstract void SetTooltip(string tooltip);
}
