using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthBar_KO : MonoBehaviour
{
    //This script decreases the health bar when the player is damaged
    //Variable for the health bar that is displayed to everyone
    public Image[] healthBar;
    public Image[] emptyHealthBar;
    public Text[] percentageEveryone;

    int percentageDecrease = 5;
    int percentHealth = 100;

    //This is the amount the healthBar is decreased by
    float decreasingAmount = .01f;

    //This decreases the health bar a certain amount over a certain amount of time;
    public IEnumerator HealthBarDecreasing()
    {
        float i = 100;
        percentHealth -= percentageDecrease;
        while (i > 0)
        {
            healthBar[0].fillAmount -= decreasingAmount;
            healthBar[1].fillAmount -= decreasingAmount;
            healthBar[2].fillAmount -= decreasingAmount;
            healthBar[3].fillAmount -= decreasingAmount;
            emptyHealthBar[0].fillAmount += decreasingAmount;
            emptyHealthBar[1].fillAmount += decreasingAmount;
            emptyHealthBar[2].fillAmount += decreasingAmount;
            emptyHealthBar[3].fillAmount += decreasingAmount;
            yield return new WaitForSeconds(.1f);
            i -= 20;
        }
    }

    void Start ()
    {
        emptyHealthBar[0].fillAmount = 0;
        emptyHealthBar[1].fillAmount = 0;
        emptyHealthBar[2].fillAmount = 0;
        emptyHealthBar[3].fillAmount = 0;
    }
    void OnTriggerEnter()
    {
        StartCoroutine(HealthBarDecreasing());
        percentageEveryone[0].text = percentHealth.ToString() + "%";
        percentageEveryone[1].text = percentHealth.ToString() + "%";
        percentageEveryone[2].text = percentHealth.ToString() + "%";
        percentageEveryone[3].text = percentHealth.ToString() + "%";
    }

}

/* Instructions:
    I have a scene set up with how this all works called "Multiplayer HUD". It is in the Scenes Folder Under "Scenes_KO" -> "InterfaceScene_KO"
    1. For player 1 add the prefabs Display Health to Player 2, Display Health to Player 3, Display Health to Player 4, and Player Health Display.(Do not add the "Display Health to Player 1" prefab)
    (These prefabs can be found in the prefab folder -> Prefabs_KO -> InterfacePrefabs_KO)
    2. Zero out all of the rotations and positions on the Display Health to Player prefabs, except for the Y position.
    3. Add this script to the player.
    4. In the inspector, on this script, set the size of the two lists (Percentage Everyone and Health Bar) to 3.
    5. Add the 4 Images called "Health Bar" (it's under each of the "Display Health to Player" prefabs) to the Health Bar List in the inspector.
    6. Add the 4 Texts called "Health Percentage" (Display Health to Player prefab -> Health Bar -> Health Percentage) to the Percentage Everyone List in the inspector.
    7. Add the 4 Images called "Empty Health Bar" to the empty Health Bar List in the inspector.
    8. The Player 1 camera culling mask should have all of the "Player _ Display" layers deselected except for the "PLayer 1 Display" layer.
    9. The "Display Health to Player 2" prefab (and all the children) should be in the "Player 2 Display" Layer, "Display Health to Player 3" prefab 
    (and all the children) should be in the "Player 3 Display" Layer, etc.
    10. Repeat this for the other 3 players. 
    */

