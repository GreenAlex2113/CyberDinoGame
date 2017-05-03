using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class CharacterStat  {

    [SerializeField]
    protected float baseValue;
    [SerializeField]
    private float modifiedValue;

    private bool sendingOnChangedEvent = false;

    public float BaseValue
    {
        get { return baseValue; }
        set {
            if (baseValue != value) {
                baseValue = Mathf.Max(0, value);
                RecalulateValues();
                SendOnChangedEvent();
            }
        }
    }

    public float ModifiedValue {
        get {
            return modifiedValue;
        }
    }

    public event Action<CharacterStat> OnChanged;
    [SerializeField]
    private List<CharacterStatModifier> modifiers = new List<CharacterStatModifier>();

    public CharacterStat(float defaultValue) {
        baseValue = defaultValue;
    }

    public void AddModifier(CharacterStatModifier modifier) {
        modifiers.Add(modifier);
        RecalulateValues();
        SendOnChangedEvent();
    }

    public void RemoveModifier(CharacterStatModifier modifier) {
        modifiers.Remove(modifier);
        RecalulateValues();
        SendOnChangedEvent();
    }

    public IEnumerator<CharacterStatModifier> GetModifiers() {
        foreach (var m in modifiers) {
            yield return m;
        }
    }

    public void UpdateState() {
        if (Internal_UpdateState()) {
            SendOnChangedEvent();
        }
    }

    /// <summary>
    /// Updates the internal state of the stat. Returning true results in OnChangeEvent being called.
    /// </summary>
    /// <returns>true if there was changes made, false if there wasn't.</returns>
    protected virtual bool Internal_UpdateState() {
        return FindAndRemoveExpiredModifiers();
    }

    private bool FindAndRemoveExpiredModifiers() {
        if (modifiers.RemoveAll(m => m.IsExpired()) > 0) {
            RecalulateValues();
            return true;
        }

        return false;
    }

    public virtual void RecalulateValues() {
        modifiedValue = baseValue;

        modifiers.Sort();
        modifiers.ForEach(m => modifiedValue = m.ModifyStat(modifiedValue));

        modifiedValue = Mathf.Max(0, modifiedValue);
    }

    protected void SendOnChangedEvent() {
        if (!sendingOnChangedEvent) {
            sendingOnChangedEvent = true;
            Internal_SendOnChangedEvent();
            sendingOnChangedEvent = false;
        }
    }

    protected virtual void Internal_SendOnChangedEvent() {
        if (OnChanged != null) {
            OnChanged(this);
        }
    }
}
