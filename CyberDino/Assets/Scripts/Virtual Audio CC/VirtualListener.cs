using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Use: Attach to anything in the scene that should be listening to audio. Use only if there's more than one thing in a scene that should be listening to audio at the same time.
public class VirtualListener : MonoBehaviour {

    public static List<VirtualListener> activeListeners = new List<VirtualListener>();
    public static AudioListener realListener;
    public static Transform realLisenterTransform;
    public static Vector3 realListenerOffset = new Vector3(0, 0, 0);

    [Range(0.01f, 10f)]
    public float weight = 1f;
    [Range(0.01f, 10f)]
    public float changeAwayFromWeight = 1f;

    void Start() {
        if (realListener == null) {
            GameObject obj = new GameObject("Real Audio Listener");
            obj.transform.position = realListenerOffset;
            realListener = obj.AddComponent<AudioListener>();
            realLisenterTransform = obj.transform;
        }
    }

    void OnEnable() {
        activeListeners.Add(this);
    }
	
    void OnDisable() {
        activeListeners.Remove(this);
    }

    void OnValidate() {
        weight = Mathf.Max(0.01f, weight);
        changeAwayFromWeight = Mathf.Max(weight, changeAwayFromWeight);
    }
}
