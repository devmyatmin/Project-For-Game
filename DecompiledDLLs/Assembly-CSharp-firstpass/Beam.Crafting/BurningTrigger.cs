using UnityEngine;

namespace Beam.Crafting;

public class BurningTrigger : MonoBehaviour
{
	[SerializeField]
	private Construction_CAMPFIRE _fire;

	private void OnTriggerEnter(Collider collided)
	{
		if (!LevelLoader.IsLoading() || !_fire.IgnoreCookableCollisionsWhileLoading)
		{
			_fire.IgnoreCookableCollisionsWhileLoading = false;
			Cooking component = ((Component)collided).GetComponent<Cooking>();
			if (Object.op_Implicit((Object)(object)component) && _fire.AddCookable(component, component.Food.PreviousOwnerId) && component.Food.CraftingType.InteractiveType == InteractiveType.ANIMALS_CRAB)
			{
				component.Food.Kill();
			}
		}
	}

	private void OnTriggerExit(Collider collided)
	{
		if (!LevelLoader.IsLoading() || !_fire.IgnoreCookableCollisionsWhileLoading)
		{
			_fire.IgnoreCookableCollisionsWhileLoading = false;
			Cooking component = ((Component)collided).GetComponent<Cooking>();
			if (Object.op_Implicit((Object)(object)component))
			{
				_fire.RemoveCookable(component);
			}
		}
	}
}
