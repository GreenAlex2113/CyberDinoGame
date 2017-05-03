using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add this script to any projectiles that will be moving so fast that there will be gaps in the space they collide with.
/// This sweeps a box collider forwards according to their speed in order to cover these gaps.
/// </summary>
public class SweepColliderForwards : MonoBehaviour {
    private static float updatesPerSecond = 20;
    public Transform transformToRotate;
    public Rigidbody rigidbodyToMeasure;
    public BoxCollider boxColliderToSweep;
    /// <summary>
    /// Diameter that the box collider will have in world space.
    /// </summary>
    public float diameter = 1f;
    /// <summary>
    /// In world space, how much extra size will be given to the collider. Used to make sure floating point errors don't make the collider too small.
    /// </summary>
    public float forwardBuffer = 0.05f;

    private float lastMagnitude = 0f;

#if UNITY_EDITOR
    public bool debugTest;
    public Vector3 debugVelocity = Vector3.zero;

    void OnEnable() {
        StartCoroutine(this.CoroutineUpdate(1f / updatesPerSecond, UpdateCollider));
    }

    void OnDisable() {
        StopAllCoroutines();
    }

    void OnValidate() {
        if (!Application.isPlaying && this.isActiveAndEnabled) {
            if (debugTest) {
                UpdateCollider();
            } else {
                debugVelocity = Vector3.zero;
                debugTest = true;
                UpdateCollider();
                debugTest = false;
            }
        }
    }
#endif

    void UpdateCollider() {
        //Get how far the rigidbody is going (to try) to move this next frame
#if UNITY_EDITOR
        Vector3 frameVelocity;
        if (debugTest) {
            frameVelocity = debugVelocity * Time.fixedDeltaTime;
        } else {
            frameVelocity = rigidbodyToMeasure.velocity * Time.fixedDeltaTime;
            debugVelocity = rigidbodyToMeasure.velocity;
        }
#else
        Vector3 frameVelocity = rigidbodyToMeasure.velocity * Time.fixedDeltaTime;
#endif
        float magnitude = frameVelocity.magnitude;

        //Check to see if the velocity has changed enough to allow for a recalculation.
        if (!Mathf.Approximately(magnitude, lastMagnitude)) {
            lastMagnitude = magnitude;
        } else {
            //If it's the same as it was last time, then there's no need to recalculate things.
            return;
        }

        //Create a rotation that faces towards the velocity.
        Quaternion rotation = Quaternion.identity;
        if (frameVelocity != Vector3.zero) {
            rotation = Quaternion.LookRotation(frameVelocity, Vector3.up);
        }
        transformToRotate.rotation = rotation;

        //Scale up our settings so they are what's needed to acheieve a certain size in world space.
        Vector3 scale = transformToRotate.lossyScale;
        Vector3 size = Vector3.zero;
        if (scale.x != 0) {
            //Inverse scaling is a go!
            size.x = diameter / scale.x;
        }
        if (scale.y != 0) {
            size.y = diameter / scale.y;
        }
        Vector3 center = Vector3.zero;
        if (scale.z != 0) {
            // Is the frame velocity + buffer bigger than the radius?
            size.z = (Mathf.Max(diameter / 2f, frameVelocity.magnitude + forwardBuffer) + diameter / 2f) / scale.z;
            center.z = (size.z - (diameter / scale.z)) / 2f;
        }
        boxColliderToSweep.size = size;
        boxColliderToSweep.center = center;
    }
}
