using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class HealthPackManager_KM : MonoBehaviour {

	/* Note: This script is attached to an object whose children are named 'Spawn Point' and numbered. These 'spawn points' are,
	 * in fact, the health packs, but are named such to clarify how they are to be used within the editor. To wit, each is to be
	 * placed within the scene where a health pack would spawn.
	*/

	public const float SPAWN_DELAY = 15;

	public static Action<GameObject> SpawnHealthPack;
	public static Action<GameObject> TriggerHealthPack;

	public List<GameObject> activeHealthPacks;
	public List<GameObject> inactiveHealthPacks;

	// Use this for initialization
	void Start () {
		SpawnHealthPack += MoveToActive;
		TriggerHealthPack += MoveToInactive;

		//Populate the health pack lists. Ensure that all health packs begin disabled.
		for (int i = 0; i < transform.childCount; i++) {
			inactiveHealthPacks.Add (transform.GetChild(i).gameObject);
			inactiveHealthPacks [i].SetActive (false);
		}

		StartCoroutine (Spawn());
	}

	/// <summary>
	/// Activates a health pack.
	/// </summary>
	/// <param name="healthPack">Health pack.</param>
	void MoveToActive(GameObject healthPack)
	{
		healthPack.SetActive (true);

		activeHealthPacks.Add (healthPack);
		inactiveHealthPacks.Remove (healthPack);
	}

	/// <summary>
	/// Deactivates a health pack.
	/// </summary>
	/// <param name="healthPack">Health pack.</param>
	void MoveToInactive(GameObject healthPack)
	{
		healthPack.SetActive (false);

		inactiveHealthPacks.Add (healthPack);
		activeHealthPacks.Remove (healthPack);
	}

	/// <summary>
	/// Spawn a random health pack every so often.
	/// </summary>
	IEnumerator Spawn()
	{
		while (true) {
			if (inactiveHealthPacks.Count != 0) {
				int rand = UnityEngine.Random.Range (0, inactiveHealthPacks.Count);
				SpawnHealthPack (inactiveHealthPacks [rand]);
			}

			yield return new WaitForSeconds (SPAWN_DELAY);
		}
	}
}
