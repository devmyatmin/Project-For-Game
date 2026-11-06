using Beam.UI;

namespace Beam;

public interface IGameOverView : IViewAdapter
{
	ILabelViewAdapter CauseOfDeathLabel { get; }

	ILabelViewAdapter DaysSurvivedLabel { get; }

	ILabelViewAdapter PermadeathLabel { get; }
}
