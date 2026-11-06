using UnityEngine;

[ExecuteInEditMode]
public class AudioMixerFixer : MonoBehaviour
{
	private void Awake()
	{
		AudioSource[] componentsInChildren = ((Component)((Component)this).transform).GetComponentsInChildren<AudioSource>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			AudioTools.FixMixerGroups(componentsInChildren[i]);
		}
	}
}
