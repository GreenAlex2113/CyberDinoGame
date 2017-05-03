using UnityEngine;
using System.Collections;

public class EnableDisable : MonoBehaviour {
	[SerializeField]
	private bool startEnabled;
	[SerializeField]
	private GameObject player;
	private InputManager_JusB playerManager;
	[SerializeField]
	private KeyCode ToggleKey;

	// Use this for initialization
	void Start () {
		gameObject.SetActive (startEnabled);
		playerManager = player.GetComponent<InputManager_JusB>();
		playerManager.keyPressed += toggleTab;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void toggleTab(KeyCode input)
	{
		if (input == playerManager.GetTabKey ()) {
			gameObject.SetActive (!gameObject.activeSelf);
		}
	}
}
