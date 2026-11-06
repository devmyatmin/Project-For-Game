using System;

namespace Beam.UI;

public class NullPlayerPreviewView : PlayerPreviewViewBase
{
	public static NullPlayerPreviewView Instance { get; } = Activator.CreateInstance<NullPlayerPreviewView>();

	public override void SetState(PlayerModelPreviewFlags flags)
	{
	}
}
