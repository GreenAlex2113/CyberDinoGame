//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;

//class WeaponList : MonoBehaviour{

//    [SerializeField]
//    [Tooltip("The weapon the character is currently using.")]
//    private Weapon activeWeapon = null;
//    public Weapon ActiveWeapon { get { return activeWeapon; } }
//    [SerializeField]
//    [Tooltip("If set, this is the weapon the character will spawn with.")]
//    private Weapon defaultWeapon = null;
//    public Weapon DefaultWeapon { get { return defaultWeapon; } }
//    [SerializeField]
//    private List<Weapon> weaponList = new List<Weapon>();
//    public Weapon[] Weapons { get { return weaponList.ToArray(); } }
//    [HideInInspector]
//    [SerializeField]
//    private GameObject weaponsGroupObject;
//    [HideInInspector]
//    [SerializeField]
//    private List<Weapon> removedWeaponList = new List<Weapon>();

//    #region Weapon Functions
//    /// <summary>
//    /// Set the GameCharacter's default weapon, which cannot be removed except by calling this function with a null parameter.<para></para>
//    /// If the weapon is not already in the GameCharacter's list of weapons, it will be added to the GameCharacter.
//    /// </summary>
//    /// <param name="weapon">The weapon to be set as default.</param>
//    /// <returns>The copy of the weapon that is unique to this GameCharacter.</returns>
//    public Weapon SetDefaultWeapon(Weapon weapon) {
//        // Are we setting a new default weapon?
//        if (weapon != null) {
//            if (weapon != defaultWeapon || weaponList.Count == 0) {
//                Weapon prevDefault = defaultWeapon;
//                defaultWeapon = AddWeapon(weapon);

//                if (prevDefault != null) {
//                    // If we were using the previous default weapon, then swtich to the new one.
//                    if (activeWeapon == prevDefault) {
//                        activeWeapon = defaultWeapon;
//                    }

//                    // Remove the weapon that was previously the default.
//                    weaponList.Remove(prevDefault);

//                    prevDefault.enabled = false;
//                    removedWeaponList.Add(prevDefault);

//                    if (OnChanged != null) {
//                        OnChanged(weaponList.ToArray());
//                    }
//                }
//            }
//            // If this function was called with null, then remove the default weapon.
//        } else if (defaultWeapon != null) {
//            // If we were previously using the default weapon, we now don't have an active weapon.
//            if (activeWeapon == defaultWeapon) {
//                SetActiveWeapon(null);
//            }

//            // Remove the default weapon.
//            weaponList.Remove(defaultWeapon);

//            defaultWeapon.enabled = false;
//            removedWeaponList.Add(defaultWeapon);

//            if (OnChanged != null) {
//                OnChanged(weaponList.ToArray());
//            }
//        }

//        return defaultWeapon;
//    }

//    /// <summary>
//    /// <para>Add a copy of a weapon to the GameCharacter's weapon list.</para>
//    /// If the GameCharacter already has a weapon with the same weaponID, then OnPickupCopy([the new weapon]) is called on the existing weapon.
//    /// </summary>
//    /// <param name="weapon">The weapon to be copied.</param>
//    /// <returns>The copy of the weapon that is unique to the GameCharacter and is also in it's weapon list.</returns>
//    public Weapon AddWeapon(Weapon weapon) {
//        Weapon wpn = FindWeaponWithID(weapon.weaponID);
//        // Do we already have a weapon with the same ID?
//        if (wpn == null) {
//            wpn = FindRemovedWeaponWithID(weapon.weaponID);
//            if (wpn != null) {
//                // Reinitalize the previously removed weapon.
//                wpn.CopyDataFrom(weapon);
//                removedWeaponList.Remove(wpn);
//            } else {
//                // Create a new copy of the weapon passed in.
//                wpn = weaponsGroupObject.AddCopyOfComponent(weapon);
//            }
//            // Configure the weapon to have the correct owner.
//            wpn.gameCharacter = this;
//            // and put it in our weapon list.
//            weaponList.Add(wpn);
//            if (OnChanged != null) {
//                OnChanged(weaponList.ToArray());
//            }

//            // If didn't have an active weapon before, we now have one.
//            if (activeWeapon == null) {
//                SetActiveWeapon(wpn);
//            }

//            return wpn;
//        } else {
//            wpn.OnPickupCopy(weapon);
//            return wpn;
//        }
//    }

//    /// <summary>
//    /// Remove a weapon (or a weapon with the same weaponID) from the GameCharacter's list of weapons.<para></para>
//    /// If the weapon to be removed is the GameCharacter's default weapon, the function will not remove it.
//    /// </summary>
//    /// <param name="weapon"></param>
//    public void RemoveWeapon(Weapon weapon) {
//        Weapon wpn = null;
//        if (weapon.gameObject == weaponsGroupObject) {
//            wpn = weapon;
//        } else {
//            wpn = FindWeaponWithID(weapon.weaponID);
//        }

//        if (wpn != null && wpn != defaultWeapon) {
//            // If we're removing our activeWeapon, then we need to switch to another weapon if we have one.
//            if (activeWeapon == wpn) {
//                if (defaultWeapon != null) {
//                    // We have a default weapon we can switch to.
//                    SetActiveWeapon(defaultWeapon);
//                } else if (weaponList.Count > 1) {
//                    // We have at least one other weapon we can switch to.
//                    CycleWeaponPrev();
//                } else {
//                    // We have no other weapons to switch to.
//                    SetActiveWeapon(null);
//                }
//            }

//            // Add the weapon to a list of removed weapons to allow for caching
//            weaponList.Remove(wpn);

//            wpn.enabled = false;
//            removedWeaponList.Add(wpn);

//            if (OnChanged != null) {
//                OnChanged(weaponList.ToArray());
//            }
//        }
//    }

//    /// <summary>
//    /// Add a new weapon to a GameCharacter and make the the active weapon.
//    /// See <seealso cref="AddWeapon(Weapon)"/> and <see cref="SetActiveWeapon(Weapon)"/>
//    /// </summary>
//    /// <param name="weapon">The weapon to be added and set active</param>
//    /// <returns>The copy of the weapon that is unique to the GameCharacter and is also in it's weapon list.</returns>
//    public Weapon AddWeaponAndSetActive(Weapon weapon) {
//        Weapon copy = AddWeapon(weapon);
//        SetActiveWeapon(copy);
//        return copy;
//    }

//    /// <summary>
//    /// Finds a weapon with the specified weaponID inside the GameCharacter's list of weapons.
//    /// </summary>
//    /// <param name="weaponID">The weaponID to look for.</param>
//    /// <returns>The weapon found, if there is one. Otherwise, returns null.</returns>
//    public Weapon FindWeaponWithID(string weaponID) {
//        return weaponList.Find(wpn => wpn.weaponID == weaponID);
//    }

//    /// <summary>
//    /// Finds a weapon with a weaponID inside the cached list of previously removed weapons.
//    /// </summary>
//    /// <param name="weaponID"></param>
//    /// <returns></returns>
//    private Weapon FindRemovedWeaponWithID(string weaponID) {
//        return removedWeaponList.Find(wpn => wpn.weaponID == weaponID);
//    }

//    /// <summary>
//    /// Sets the GameCharacter's active weapon (or a weapon with the same weaponID if the weapon passed in is not in the GameCharacter's list of weapons) and disables all other weapons.<para></para>
//    /// Does nothing if the GameCharacter does not have that weapon (or a weapon with the same ID) in it's weapon list. Though calling this with null will set the GameCharacter's active weapon to their default weapon (if they have one).
//    /// </summary>
//    /// <param name="weapon"></param>
//    public void SetActiveWeapon(Weapon weapon) {
//        Weapon wpn = null;
//        // Are we resetting the active weapon?
//        if (weapon != null) {
//            //Don't do anything unless we're actually changing weapons.
//            if (weapon != activeWeapon) {
//                if (weaponList.Contains(weapon)) {
//                    wpn = weapon;
//                } else {
//                    wpn = FindWeaponWithID(weapon.weaponID);
//                }

//                // Only do something if we have that weapon in our list.
//                if (wpn != null) {
//                    // Update our active weapon one last time before switching away from it.
//                    // This stops shenanigans such as charging a weapon and then storing that charge by swtiching away from it.
//                    // Perhaps this would be better with special code for that case, but the a "ChargingWeapon" class doesn't exist when I'm writing this!
//                    UpdateWeapon(false);

//                    //Disable all other weapons in our list and enable the one that was made active.
//                    foreach (Weapon w in weaponList) {
//                        w.enabled = w == wpn;
//                    }

//                    activeWeapon = wpn;

//                    if (ActiveWeaponChanged != null) {
//                        ActiveWeaponChanged(activeWeapon, weaponList.IndexOf(activeWeapon));
//                    }
//                }
//            }
//        } else {
//            //Only do something if the weapon is actually being changed.
//            if (activeWeapon != defaultWeapon) {
//                // Update our active weapon one last time before switching away from it.
//                // This stops shenanigans such as charging a weapon and then storing that charge by swtiching away from it.
//                // Perhaps this would be better with special code for that case, but the a "ChargingWeapon" class doesn't exist when I'm writing this!
//                UpdateWeapon(false);

//                // We are resetting the active weapon.
//                // If default weapon is null, then all weapons are disabled.
//                weaponList.ForEach(w => w.enabled = w == defaultWeapon);
//                activeWeapon = defaultWeapon;

//                if (ActiveWeaponChanged != null) {
//                    ActiveWeaponChanged(activeWeapon, weaponList.IndexOf(activeWeapon));
//                }
//            }
//        }
//    }

//    /// <summary>
//    /// Sets the GameCharacter's active weapon to the one that comes before it in it's weapon list.
//    /// </summary>
//    public void CycleWeaponPrev() {
//        CycleWeapon(false);
//    }

//    /// <summary>
//    /// Sets the GameCharacter's active weapon to the one that comes after it in it's weapon list.
//    /// </summary>
//    public void CycleWeaponNext() {
//        CycleWeapon(true);
//    }

//    private void CycleWeapon(bool next) {
//        // Do we actually /have/ any weapons?
//        if (weaponList.Count > 0) {
//            if (activeWeapon != null) {
//                int index = weaponList.IndexOf(activeWeapon);
//                // Go fowards or backwards based on which way we're cycling.
//                index = (index + ((next) ? 1 : -1)) % weaponList.Count;
//                SetActiveWeapon(weaponList[index]);
//            } else {
//                // If we don't have an active weapon, set it to the start or the end of the weapon list based on which way we're cycling.
//                SetActiveWeapon((next) ? weaponList[0] : weaponList[weaponList.Count - 1]);
//            }
//        }
//    }
//}
