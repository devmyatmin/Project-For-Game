using System;
using System.Collections.Generic;

public interface ILoader
{
	bool IsLoading { get; set; }

	IList<Action> DeferredLoading { get; }

	IList<Action> PostLoading { get; }

	IList<Action> PhysicalLoading { get; }
}
