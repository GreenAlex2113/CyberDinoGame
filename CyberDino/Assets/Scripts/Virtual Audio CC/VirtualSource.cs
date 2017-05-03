using UnityEngine;
using System.Collections;

// Use: Attach to any object that has a Audio Source if you have any Virtual Listeners in the scene.
public class VirtualSource : MonoBehaviour {

    public static float UPDATE_FREQUENCY = 1 / 50f;

    public enum Mode {
        Fixed, Closest, Average
    }

    public GameObject realSourceObject;
    public AudioSource realSource;

    public Mode mode = Mode.Closest;

    private float startLerpTime = 0f;
    public float listenerChangeLerpTime = 0.5f;

    private Transform tr;

    [SerializeField]
    private VirtualListener listener;
    public VirtualListener Listener {
        get { return listener; }
        set { SetListener(value); }
    }
    private VirtualListener prevListener;
    private float dopplerLevelTemp;

    private Coroutine updatePositionCoroutine;

	// Use this for initialization
	void Start () {
        tr = transform;

        //Copy the audio source to another object that this script will be controlling the position of.
        AudioSource audioSource = GetComponent<AudioSource>();
        realSourceObject = new GameObject(gameObject.name + " (Real Audio Source)");

        //Configure the copy object. Set it to false to start with so that the copied AudioSource doesn't try to automatically start before we're ready. (Allows "play on awake" to work properly)
        realSourceObject.SetActive(false);
        //Copy the audio source component.
        realSource = realSourceObject.AddCopyOfComponent(audioSource);
        //If the virtual souce is enabled, then the real source better be enabled too.
        realSource.enabled = true;
        //Get rid of the audio source that we just copied. No need to have two of the same thing causing confusion.
        Destroy(audioSource);
        realSourceObject.SetActive(true);


        // If we don't have a listener set in the inspector, try and grab the closest one.
        if (listener == null) {
            listener = FindClosestLisenter();
        }

        updatePositionCoroutine = StartCoroutine(UpdatePosition());
	}

    void OnEnable() {
        if (realSource != null) {
            realSource.enabled = true;

            if (updatePositionCoroutine == null) {
                updatePositionCoroutine = StartCoroutine(UpdatePosition());
            }
        }
    }

    void OnDisable() {
        if (realSource != null) {
            realSource.enabled = false;
        }

        if (updatePositionCoroutine != null) {
            StopCoroutine(updatePositionCoroutine);
            updatePositionCoroutine = null;
        }
    }
	
	// Update is called once per frame
	IEnumerator UpdatePosition () {
        //Only run the code if there's listeners in the scene.
        while (true) {
            yield return new WaitForSeconds(UPDATE_FREQUENCY);
            if (VirtualListener.activeListeners.Count > 0) {
                if (mode == Mode.Fixed || mode == Mode.Closest) {
                    // If we're in cloest mode, then we need to find the closest listener every update.
                    // Otherwise if we don't have a listener (or it became disabled), then we need to get another one.
                    if (mode == Mode.Closest || listener == null || !listener.isActiveAndEnabled) {
                        VirtualListener l = FindClosestLisenter();
                        if (l != listener) {
                            SetListener(l);
                        }
                    }

                    //Find the position of ourselves in the local space of the listener. Use InverseTransformDirectionto ignore scaling of the listener.
                    Vector3 offset = tr.position - listener.transform.position;
                    float distance = offset.magnitude;
                    offset = listener.transform.InverseTransformDirection(offset.normalized) * distance;
                    //Get the rotation if terms of the listener's local space as well.
                    Vector3 forward = listener.transform.InverseTransformDirection(tr.forward).normalized;

                    //If we have a previous listener, then we need to be lerping between where it says we should and where our current listener says we should be.
                    if (prevListener != null) {
                        float t = Mathf.Clamp01((Time.time - startLerpTime) / listenerChangeLerpTime);
                        if (t < 1) {
                            //Find the positoin of oursevles in the local space of the previous listener.
                            Vector3 prevOffset = tr.position - prevListener.transform.position;
                            float prevDistance = prevOffset.magnitude;
                            prevOffset = prevListener.transform.InverseTransformDirection(prevOffset.normalized) * prevDistance;
                            Vector3 prevForward = prevListener.transform.InverseTransformDirection(tr.forward).normalized;

                            //Lerp the values from the prev listener.
                            offset = Vector3.Lerp(prevOffset, offset, t);
                            distance = Mathf.Lerp(prevDistance, distance, t);
                            forward = Vector3.Lerp(prevForward, forward, t).normalized;
                            if (forward == Vector3.zero) {
                                forward = prevForward;
                            }
                        } else {
                            //The lerping is done.
                            prevListener = null;
                            realSource.dopplerLevel = dopplerLevelTemp;
                            dopplerLevelTemp = 0;
                        }
                    }
                    realSourceObject.transform.position = (offset.normalized * distance) + VirtualListener.realLisenterTransform.position;
                    //This introduces random tilt, but for a 3D audio source we just want it pointing in the right direction.
                    realSourceObject.transform.rotation = Quaternion.LookRotation(forward);
                } else if (mode == Mode.Average) {
                    Vector3 averageOffset = Vector3.zero;
                    //Measure the average distance as well so that we can be always at least that far away because the offsets can cancel eachother out.
                    float averageDistance = 0;
                    Vector3 averageForward = Vector3.zero;
                    float totalWeight = 0f;
                    foreach (var l in VirtualListener.activeListeners) {
                        Vector3 offset = tr.position - l.transform.position;
                        offset = l.transform.InverseTransformDirection(offset.normalized) * offset.magnitude;
                        averageOffset += offset * l.weight;
                        averageDistance += offset.magnitude * l.weight;
                        averageForward += l.transform.InverseTransformDirection(tr.forward).normalized * l.weight;
                        totalWeight += l.weight;
                    }
                    //Find the average offset and the average distance.
                    averageOffset /= totalWeight;
                    averageDistance /= totalWeight;
                    averageForward /= totalWeight;
                    if (averageForward == Vector3.zero) {
                        averageForward = VirtualListener.activeListeners[0].transform.InverseTransformDirection(tr.forward).normalized;
                    }
                    realSourceObject.transform.position = (averageOffset.normalized * averageDistance) + VirtualListener.realLisenterTransform.position;
                    realSourceObject.transform.rotation = Quaternion.LookRotation((averageForward / totalWeight).normalized);
                }
            } else {
                realSourceObject.transform.position = tr.position;
                realSourceObject.transform.rotation = tr.rotation;
            }
        }
	}

    private void SetListener(VirtualListener listener) {
        prevListener = this.listener;
        this.listener = listener;
        startLerpTime = Time.time;
        //We don't want the doppler effect to happen while the source is lerping between two listeners.
        //Though we only want to disable it if it's not already disabled or else we will lose the doppler level data.
        if (dopplerLevelTemp == 0 && listenerChangeLerpTime > 0.1f && prevListener != null && (mode == Mode.Closest || mode == Mode.Fixed)) {
            dopplerLevelTemp = realSource.dopplerLevel;
            realSource.dopplerLevel = 0;
        }
    }

    private VirtualListener FindClosestLisenter() {
        //If there's no active virtual listeners in the scene, then just return null.
        if (VirtualListener.activeListeners.Count == 0) {
            return null;
        }

        //Get initail comparison variables;
        float sqrDistance = 0;
        VirtualListener best = null;

        //If we currently have a set listener, start the comparison by measuring the distance to it.
        if (listener != null) {
            sqrDistance = (listener.transform.position - tr.position).sqrMagnitude / Mathf.Max(listener.changeAwayFromWeight, listener.weight);
            best = listener;
        }

        //Set if any other listeners are closer.
        foreach (var other in VirtualListener.activeListeners) {
            if (other == best) {
                //No need to compare the same listener twice.
                continue;
            }

            float newDist = (other.transform.position - tr.position).sqrMagnitude / other.weight;
            // If we didn't have a listener, just use the first one we grab.
            if (best == null || newDist < sqrDistance) {
                best = other;
                sqrDistance = newDist;
            }
        }

        return best;
    }

    void OnValidate() {
        //Don't allow realSourceObject and realSouce to be set from the inspectior during edit mode.
        if (!Application.isPlaying) {
            realSourceObject = null;
            realSource = null;
        }
    }
}
