using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;

namespace Beam.UI;

public class IndicatorPresenter : ControllerBase, INeedPlayerComponent
{
	[SerializeField]
	private IndicatorViewBase _view;

	private List<IndicatorElementBase> _indicators = new List<IndicatorElementBase>();

	private IPlayer _player;

	IPlayer INeedPlayerComponent.Player => _player;

	private void Start()
	{
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"IndicatorPresenter:: No view component assigned!");
			_view = NullIndicatorViewBase.Instance;
		}
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as IndicatorViewBase;
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"IndicatorPresenter:: No view component assigned!");
			_view = NullIndicatorViewBase.Instance;
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.Statistics.DirectionalDamage += PlayerStatistics_DirectionalDamage;
	}

	private void OnDestroy()
	{
		if (_player != null && (Object)(object)_player.Statistics != (Object)null)
		{
			_player.Statistics.DirectionalDamage -= PlayerStatistics_DirectionalDamage;
		}
	}

	public void NewIndicator(IndicatorDataBase data)
	{
		if (data != null)
		{
			IndicatorElementBase indicatorElementBase = Object.Instantiate<DamageIndicatorElement>(_view.IndicatorPrefab);
			indicatorElementBase.Initialize(data);
			AddIndicator(indicatorElementBase);
		}
	}

	private void Update()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if (_player == null)
		{
			return;
		}
		for (int i = 0; i < _indicators.Count; i++)
		{
			IndicatorElementBase indicatorElementBase = _indicators[i];
			if (BeamTeam.IsNullOrDestroyed(indicatorElementBase))
			{
				_indicators.Remove(indicatorElementBase);
				break;
			}
			Vector3 forward = _player.transform.forward;
			Vector3 val = indicatorElementBase.Data.Direction - _player.transform.position;
			val.y = 0f;
			val.Normalize();
			float dot = 0f - Vector3.Dot(Vector3.Cross(forward, val), _player.transform.up);
			float angle = Vector3.Angle(val, forward);
			angle = AngleCircumference(dot, angle);
			((Component)indicatorElementBase).transform.localRotation = Quaternion.Slerp(((Component)indicatorElementBase).transform.localRotation, Quaternion.AngleAxis(angle, new Vector3(0f, 0f, 1f)), 10f * Time.deltaTime);
		}
	}

	public void AddIndicator(IndicatorElementBase indicator)
	{
		if (!_indicators.Contains(indicator))
		{
			((Component)indicator).transform.SetParent(_view.IndicatorContainer.Transform, false);
			_indicators.Add(indicator);
		}
	}

	public void RemoveIndicator(IndicatorElementBase indicator)
	{
		_indicators.Remove(indicator);
	}

	public void DestroyIndicator(IndicatorElementBase indicator)
	{
		RemoveIndicator(indicator);
		BeamTeam.Destroy<GameObject>(((Component)indicator).gameObject);
	}

	private void PlayerStatistics_DirectionalDamage(IndicatorDataBase data)
	{
		NewIndicator(data);
	}

	public float AngleCircumference(float dot, float angle)
	{
		float num = angle;
		float num2 = 360f;
		num = angle;
		if (dot < 0f)
		{
			num = num2 - angle;
		}
		return num;
	}
}
