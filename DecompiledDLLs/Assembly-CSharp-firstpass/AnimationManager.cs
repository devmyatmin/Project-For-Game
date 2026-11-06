using System;
using System.Collections.Generic;
using Beam;
using Beam.Utilities;
using UnityEngine;

public class AnimationManager : Singleton<AnimationManager>
{
	[Serializable]
	public class AnimationCombo
	{
		[SerializeField]
		private InteractiveType _type;

		[SerializeField]
		private string _typeId;

		[SerializeField]
		private AnimationClip _idleAnimation;

		[SerializeField]
		private List<AnimationTypeCombo> _combos;

		public InteractiveType Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		public string TypeId
		{
			get
			{
				return _typeId;
			}
			set
			{
				_typeId = value;
			}
		}

		public AnimationClip IdleAnimation
		{
			get
			{
				return _idleAnimation;
			}
			set
			{
				_idleAnimation = value;
			}
		}

		public List<AnimationTypeCombo> Combos
		{
			get
			{
				return _combos;
			}
			set
			{
				_combos = value;
			}
		}

		public AnimationCombo()
		{
			_combos = new List<AnimationTypeCombo>
			{
				new AnimationTypeCombo
				{
					Type = InteractiveType.None,
					TypeId = InteractiveType.None.ToString()
				}
			};
		}
	}

	[Serializable]
	public class AnimationTypeCombo
	{
		[SerializeField]
		private InteractiveType _type;

		[SerializeField]
		private string _typeId;

		[SerializeField]
		private AnimationClip _value;

		public InteractiveType Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		public string TypeId
		{
			get
			{
				return _typeId;
			}
			set
			{
				_typeId = value;
			}
		}

		public AnimationClip Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}
	}

	[SerializeField]
	[HideInInspector]
	private List<AnimationCombo> _combos = new List<AnimationCombo>();

	public List<AnimationCombo> Combos => _combos;

	public AnimationClip GetAnimation(InteractiveType primary, InteractiveType secondary)
	{
		AnimationCombo animationCombo = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_combos, (AnimationCombo cmb, InteractiveType typ) => cmb.Type == typ, primary);
		if (animationCombo != null)
		{
			AnimationTypeCombo animationTypeCombo = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(animationCombo.Combos, (AnimationTypeCombo cmb, InteractiveType typ) => cmb.Type == typ, secondary);
			if (animationTypeCombo != null && (Object)(object)animationTypeCombo.Value != (Object)null)
			{
				return animationTypeCombo.Value;
			}
			animationTypeCombo = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(animationCombo.Combos, (AnimationTypeCombo cmb, InteractiveType typ) => cmb.Type == typ, InteractiveType.None);
			if (animationTypeCombo != null && (Object)(object)animationTypeCombo.Value != (Object)null)
			{
				return animationTypeCombo.Value;
			}
		}
		Debug.LogError((object)("AnimationManager::GetAnimation(): No animation combo found for [" + primary.ToString() + "] [" + secondary.ToString() + "]"));
		return null;
	}

	public AnimationClip GetIdleAnimation(InteractiveType primary)
	{
		AnimationCombo animationCombo = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_combos, (AnimationCombo cmb, InteractiveType typ) => cmb.Type == typ, primary);
		if (animationCombo != null && (Object)(object)animationCombo.IdleAnimation != (Object)null)
		{
			return animationCombo.IdleAnimation;
		}
		Debug.LogError((object)("AnimationManager::GetIdleAnimation(): No idle animation found for [" + primary.ToString() + "]"));
		return null;
	}
}
