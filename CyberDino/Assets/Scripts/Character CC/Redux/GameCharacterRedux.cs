using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// This script holds, keeps track of, and sends delegates for a GameCharacter's vital statisics.
// It also handles the interpretation of input.
// If a GameCharacter does not also have a CharacterMotor component on it, it cannot move.
// If a GameCharacter does not have a CharacterGun and a Weapon, it cannot fire.
public class GameCharacterRedux : MonoBehaviour {

    private static float WEAPON_UPDATE_WAIT = 1 / 30f;
    private static float STAT_UPDATE_WAIT = 1 / 30f;

    public CharacterMotor motor;

    // Inputs
    [HideInInspector]
    public Vector3 moveInput = Vector3.zero;
    [HideInInspector]
    public bool sprintInput = false;
    [HideInInspector]
    public bool jumpInput = false;
    [HideInInspector]
    public Vector3 aimPoint = Vector3.zero;
    [HideInInspector]
    // You can only politely ask that you'd like the Dino to fire it's weapon.
    public bool fireInput = false;

    public bool IsSprinting
    {
        // We can't sprint if we're overheated or if we don't have any move input!
        get { return !heat.Overheated && sprintInput && moveInput != Vector3.zero; }
    }

    // Movement settings
    [SerializeField]
    private CharacterStat speed = new CharacterStat(10f);
    public CharacterStat Speed { get { return speed; } }
    public float sprintSpeedMultiplier = 2f;
    public float sprintHeatCostPerSecond = 10f;

    [SerializeField]
    private CharacterStat maxJumpHeight = new CharacterStat(5f);
    public CharacterStat MaxJumpHeight { get { return maxJumpHeight; } }
    public float minJumpHeight = 1f;

    [SerializeField]
    private CharacterStatMax health = new CharacterStatMax(2000f);
    public CharacterStatMax Health { get { return health;} }

    public event Action OnDeath;
    public static event Action<GameCharacterRedux> OnCharacterDeath;

    [SerializeField]
    private CharacterStatHeat heat = new CharacterStatHeat(200f, 20f, 0.66f, 2f);
    public CharacterStatHeat Heat { get { return heat; } }

    //Gun and weapon settings
    public CharacterGun gun;
    public Weapon weapon;

    void Awake() {
        health.OnChanged += CheckIfDead;
    }

    void OnEnable() {
        StartCoroutine(this.CoroutineUpdate(STAT_UPDATE_WAIT, UpdateStats));
        StartCoroutine(this.CoroutineUpdate(WEAPON_UPDATE_WAIT, UpdateWeapon));
    }

    void OnDisable() {
        StopAllCoroutines();
    }

    protected void CheckIfDead(CharacterStatMax stat) {
        if (stat.CurrentValue <= 0) {
            if (OnCharacterDeath != null) {
                OnCharacterDeath(this);
            }

            if (OnDeath != null) {
                OnDeath();
            }
        }
    }

    protected void UpdateStats() {
        speed.UpdateState();
        maxJumpHeight.UpdateState();

        health.UpdateState();
        heat.UpdateState();
    }

    protected void UpdateWeapon() {
        UpdateWeapon(fireInput);
    }

    protected virtual void UpdateWeapon(bool fireInput) {
        if (gun != null && weapon != null && weapon.isActiveAndEnabled) {
            float heatChange;
            bool weaponFired;
            //We can't shoot if we're overheated!
            bool firing = (heat.Overheated) ? false : fireInput;
            weapon.UpdateState(firing, gun.bulletSpawnPoints, out heatChange, out weaponFired);
            heat.CurrentValue += heatChange;
        }
    }
    
    void OnValidate() {
        speed.RecalulateValues();
        maxJumpHeight.RecalulateValues();

        health.RecalulateValues();
        heat.RecalulateValues();
    }

    //Debug information that only gets compiled if we're running this script in the editor.
#if UNITY_EDITOR
    public bool showDebugInfo = false;

    void OnGUI() {
        if (showDebugInfo) {
            GUILayout.Label("Health: " + health.CurrentValue + "/" + health.ModifiedMaxValue);
            GUILayout.Label("Heat: " + heat.CurrentValue + "/" + heat.ModifiedMaxValue);
            GUILayout.Label("Overheated: " + heat.Overheated);
            if (weapon != null) {
                GUILayout.Label("Weapon: " + weapon.weaponName + ((weapon.usesAmmo) ? " (" + weapon.currentAmmo + "/" + weapon.maxAmmo + ")" : ""));
            }
        }
    }
#endif
}
