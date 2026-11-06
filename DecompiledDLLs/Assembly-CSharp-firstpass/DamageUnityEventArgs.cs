using System;
using Beam;

[Serializable]
public class DamageUnityEventArgs
{
	public float Damage;

	public IBase Damager;

	public DamageUnityEventArgs(float damage, IBase damager)
	{
		Damage = damage;
		Damager = damager;
	}
}
