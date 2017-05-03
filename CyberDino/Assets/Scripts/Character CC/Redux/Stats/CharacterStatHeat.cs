using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class CharacterStatHeat : CharacterStatMax {

    [SerializeField]
    [HideInInspector]
    private bool overheated = false;
    public bool Overheated { get { return overheated; } }

    public float overheatedWait;
    [Range(0f, 1f)]
    public float overheatedUntilThreshold;
    public float cooldownRate;

    [SerializeField]
    [HideInInspector]
    private float overheatedTime;
    private float lastUpdateTime;

    public override float CurrentValue {
        get { return base.CurrentValue; }
        set {
            currentValue = Mathf.Clamp(value, 0, ModifiedValue);
            CheckIfOverheated();
            SendOnChangedEvent();
        }
    }

    public new event Action<CharacterStatHeat> OnChanged;

    public CharacterStatHeat(float defaultValue, float defaultCooldownRate, float defaultOverheatedUntilThreshold, float defaultOverheatedWait)
        : base(defaultValue) {
        currentValue = 0f;
        overheatedWait = defaultOverheatedWait;
        overheatedUntilThreshold = defaultOverheatedUntilThreshold;
        cooldownRate = defaultCooldownRate;

        overheatedTime = 0f;
        lastUpdateTime = 0f;
    }

    protected override bool Internal_UpdateState() {
        return (base.Internal_UpdateState() || UpdateCooling());
    }

    public override void RecalulateValues() {
        base.RecalulateValues();
        CheckIfOverheated();
    }

    private void CheckIfOverheated() {
        //Check to see if we need to become overheated.
        if (!overheated) {
            if (currentValue >= ModifiedMaxValue) {
                overheatedTime = Time.time;
                overheated = true;
            }
        //Check to see if we need to become not overheated.
        } else {
            if (Percentage <= overheatedUntilThreshold) {
                overheated = false;
            }
        }
    } 

    private bool UpdateCooling() {
        if (lastUpdateTime == 0f) {
            lastUpdateTime = Time.time;

            return false;
        }

        float deltaTime = Time.time - lastUpdateTime;
        float prevValue = currentValue;

        // Update heat cooling
        // If we got overheated, then we need to wait before we can cool down again.
        if (!overheated || (overheated && (Time.time >= overheatedTime + overheatedWait))) {
            currentValue = Mathf.Max(0, currentValue - (cooldownRate * deltaTime));
        }

        lastUpdateTime = Time.time;

        CheckIfOverheated();

        return prevValue != currentValue;
    }

    protected override void Internal_SendOnChangedEvent() {
        base.Internal_SendOnChangedEvent();
        if (OnChanged != null) {
            OnChanged(this);
        }
    }
}