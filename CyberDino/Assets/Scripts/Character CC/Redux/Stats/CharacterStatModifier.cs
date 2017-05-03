using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class CharacterStatModifier : IComparable<CharacterStatModifier> {

    public float flat;
    public float multiplier;
    public float priority;

    public float duration;
    [SerializeField]
    private float startTime;

    CharacterStatModifier(float flat, float multiplier, float priority, float duration) {
        this.flat = flat;
        this.multiplier = multiplier;
        this.priority = priority;

        this.duration = duration;
        startTime = Time.time;
    }

    public bool IsExpired() {
        return duration > 0 && (Time.time >= duration + startTime);
    }

    public float ModifyStat(float stat) {
        stat += flat;
        stat *= multiplier;
        return stat;
    }

    public int CompareTo(CharacterStatModifier other) {
        return priority.CompareTo(other.priority);
    }
}
