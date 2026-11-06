using System.Collections.Generic;

namespace Beam.UI;

public interface IInputMenuViewAdapter : IOptionsPanelView, IViewAdapter
{
	IScrollviewViewAdapter Scrollview { get; }

	ILabelButtonViewAdapter DefaultButton { get; }

	IList<IOptionButtonViewAdapter> Buttons { get; }

	IOptionButtonViewAdapter CreateInputOptionButton(IOptionCategoryHeader header);

	IOptionSliderViewAdapter CreateInputOptionSlider(IOptionCategoryHeader header);

	IInputMapCategoryHeader CreateInputCategorySubheader(IOptionCategoryHeader header);

	IOptionInputFieldButtonViewAdapter CreateInputField(IOptionCategoryHeader header);

	ISelectableViewAdapter CreateControllerLayoutImage(IOptionCategoryHeader header);

	void RemoveButton(IOptionButtonViewAdapter button);

	void DestroyInputOptionButtons();

	void DestroyInputOptionSliders();

	void DestroyInputFields();

	void SetNavigation(IList<IOptionCategoryHeader> categories);
}
