using UnityEngine;
using System.Collections;

public class HealthPackBehaviour_KM : MonoBehaviour {

	public const float HEALTH_RESTORE_PERCENTAGE = .2f;

	void OnTriggerEnter(Collider col)
	{
		//Get the component on the triggering dino that holds health information,
		//and add a percentage of its maximum health to its current health.
		if (col.GetComponent<GameCharacter> () != null) {
			GameCharacter dino = col.GetComponent<GameCharacter> ();
			dino.CurrentHealth += dino.MaxHealth * HEALTH_RESTORE_PERCENTAGE;

			HealthPackManager_KM.TriggerHealthPack (gameObject);
		}
	}
}
