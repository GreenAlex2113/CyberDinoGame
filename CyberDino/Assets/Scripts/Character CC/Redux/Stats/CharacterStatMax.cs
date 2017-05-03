using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class CharacterStatMax : CharacterStat {

    [SerializeField]
    protected float currentValue = 0;
    public virtual float CurrentValue {
        get { return currentValue; }
        set {
            currentValue = Mathf.Clamp(value, 0, ModifiedValue);
            SendOnChangedEvent();
        }
    }

    public float BaseMaxValue
    {
        get { return BaseValue; }
        set { BaseValue = value; }
    }

    public float ModifiedMaxValue
    {
        get { return ModifiedValue; }
    }

    public float Percentage
    {
        get { return currentValue / ModifiedValue; }
        set { CurrentValue = ModifiedValue * value; }
    }

    public new event Action<CharacterStatMax> OnChanged;

    public CharacterStatMax(float defaultValue)
        : base(defaultValue) {
        currentValue = defaultValue;
    }

    public override void RecalulateValues() {
        base.RecalulateValues();

        if (currentValue > ModifiedMaxValue) {
            currentValue = ModifiedMaxValue;
        }
    }

    protected override void Internal_SendOnChangedEvent() {
        base.Internal_SendOnChangedEvent();
        if (OnChanged != null) {
            OnChanged(this);
        }
    }
}
