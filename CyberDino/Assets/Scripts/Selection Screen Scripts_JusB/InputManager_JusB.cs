using UnityEngine;
using System.Collections;

public class InputManager_JusB: MonoBehaviour {
	[SerializeField]
	private KeyCode TabKey;
	[SerializeField]
	private KeyCode UpKey;
	[SerializeField]
	private KeyCode DownKey;
	[SerializeField]
	private KeyCode LeftKey;
	[SerializeField]
	private KeyCode RightKey;

	public delegate void keyHandler(KeyCode keyPressed);
	public event keyHandler keyPressed;

	// Use this for initialization
	void Start () {
		
	}

	void Update(){
			if (Input.GetKeyDown (TabKey)) {
				Debug.Log ("Whoa");
				keyPressed (TabKey);
			} else if (Input.GetKeyDown (UpKey)) {
				keyPressed (UpKey);
			} else if (Input.GetKeyDown (DownKey)) {
				keyPressed (DownKey);
			} else if (Input.GetKeyDown (LeftKey)) {
				keyPressed (LeftKey);
			} else if (Input.GetKeyDown (RightKey)) {
				keyPressed (RightKey);
			}
	}
	public KeyCode GetTabKey()
	{
		return TabKey;
	}
	public KeyCode GetUpKey()
	{
		return UpKey;
	}
	public KeyCode GetDownKey()
	{
		return DownKey;
	}
	public KeyCode GetLeftKey()
	{
		return LeftKey;
	}
	public KeyCode GetRightKey()
	{
		return RightKey;
	}
}
