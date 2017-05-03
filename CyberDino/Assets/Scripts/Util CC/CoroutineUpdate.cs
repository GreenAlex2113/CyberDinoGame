using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

public static class UpdateInCoroutine {
    public static IEnumerator CoroutineUpdate(this MonoBehaviour script, float wait, Action<float> func) {
        float lastUpdate = Time.time;
        if (func != null) {
            while (true) {
                yield return new WaitForSeconds(wait);
                //Debug.Log(Time.time - lastUpdate);
                func(Time.time - lastUpdate);
                lastUpdate = Time.time;
            }
        }
    }

    public static IEnumerator CoroutineUpdate(this MonoBehaviour script, float wait, Action func) {
        if (func != null) {
            while (true) {
                yield return new WaitForSeconds(wait);
                //Debug.Log(Time.time - lastUpdate);
                func();
            }
        }
    }
}

