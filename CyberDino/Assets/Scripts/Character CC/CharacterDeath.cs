using UnityEngine;
using System.Collections;

//Script that lisens for a gameCharacter's death.
//Mean to be attached to the object with the Game Character on it.
//Right now it just makes an explosion prefab and disables it's game object.
public class CharacterDeath : MonoBehaviour {

    public GameCharacter gameCharacter;
    public GameObject explosionPrefab;

	// Use this for initialization
	void OnEnable () {
        gameCharacter.Death += IDied;
    }
	
	void OnDisable() {
        gameCharacter.Death -= IDied;
    }

    void IDied() {
        //Spawn Health Pack here!
        Destroy(Instantiate(explosionPrefab, gameCharacter.GetComponent<Collider>().bounds.center, Quaternion.identity), 5f);

        gameCharacter.enabled = false;
        gameCharacter.GetComponent<CharacterController>().enabled = false;

        PlayerControl control = gameCharacter.GetComponent<PlayerControl>();
        if (control) {
            control.enabled = false;
        }

        gameObject.SetActive(false);
    }
}
