using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//script for applying upgrades for the player, when an upgrade is grabbed this script will activate a certain upgrade method
//script also gives the playerControls certain upgrade values that can be increase by upgrade objects
//all public values are derived on player controls
public class PlayerUpgrades : MonoBehaviour
{
    //list of major stats
    public float strength, intellect, agility, vitality;
    //list of minor stats
    public float meleeDamage, knockbackResist, meleeVamp, spellDamage, fireballCapacity, cooldownReduction, moveSpeed, baseDamage, attackSpeed, debuffReduction, health, abilityDMGBonus, debuffBonusDuration;

    //list of permanant power trackers and how many the player pickedup
    public int bombTotal, splitRoundTotal, bleedClawTotal, abilityHealTotal, meleeBoosterTotal, incendiaryMagicTotal, oilClawTotal, clarityTotal;
    public float bombSize, meleeBoosterDMGScale;

    public List<string> activePrimaryEffects = new List<string>();//special combat effects that only the primary attack can activate
    public List<string> activeMeleeEffects = new List<string>();//special combat effects that only the melee attack can activate
    public List<string> activeAbilityEffects = new List<string>();//special combat effects that any ability can activate

    public PlayerControls PlayerMain;
    private PlayerHealth PlayerHealthHolder;
    private PlayerAbilitySwapMenu abilityMenuOBJ;
    public GameStateController stateControllerOBJ;

    //temp int and string are used for the coroutines to properly apply an upgrade and are hidden from inspector
    [HideInInspector]
    public float TempInt;
    [HideInInspector]
    public string tempString;
    //below values are used for UI of stackable powers
    public List<int> designatedIntValue;
    [HideInInspector]
    public List<int> targetPoints;
    public List<int> positionInUI;
    private void Start()
    {
        GameObject Player = GameObject.FindWithTag("Player");
        PlayerMain = Player.GetComponent<PlayerControls>();
        PlayerHealthHolder = Player.GetComponentInChildren<PlayerHealth>();
        GameObject stateGameobject = GameObject.FindWithTag("MainSceneController");
        stateControllerOBJ = stateGameobject.GetComponentInChildren<GameStateController>();
    }

    //upgradeEffect applies stat boost upgrades and takes in the upgrade value and the string name of the upgrade
    public void UpgradeEffect(int upgradeAmount, string statToUpgrade)
    {
        StartCoroutine(IncreaseStat(upgradeAmount, statToUpgrade));
    }

    //unlockPower will provide a power for the player based on the strings passed in, starts a coroutine based on the upgradeName and uses upgradeTitle to track what power it being unlocked
    public void UnlockPower(string upgradeTitle ,string upgradeName)
    {
        tempString = upgradeTitle;
        StartCoroutine(upgradeName);
    }
    //power booster with run when the power picked up is already owned by the player, this allows it to stack power effects 
    public void UnlockPowerBooster(string upgradeName)
    {
        StartCoroutine(PermanantPowerBooster(upgradeName));
    }
    //unlockAbility will add a new player ability when activated, values taken in are sent to a coroutine
    public void UnlockAbility(string upgradeTitle, string upgradeName, float cooldown, Sprite abilityIMG)
    {
        StartCoroutine(UnlockPlayerAbility(upgradeName, upgradeTitle, cooldown, abilityIMG));
    }

    public IEnumerator IncreaseStat(int upgradeAmount,string statToUpgrade)//all player stats are increased using this coroutine, based on the string taken in it will increase the player stat accordingly and update any other data
    {
        //function is used to increase various player stats and effect that stat in a additive or multiplicative way

        //get access to health and player scripts
        GameObject Player = GameObject.FindWithTag("Player");
        PlayerMain = Player.GetComponent<PlayerControls>();
        PlayerHealthHolder = Player.GetComponentInChildren<PlayerHealth>();

        if(statToUpgrade == "STR")
        {
            PlayerMain.strength += upgradeAmount;
            PlayerMain.meleeDamage = (.4f * PlayerMain.strength + 1) * .15f + 1;//increase melee and melee power damage by 15% base and +6% per STR-- multiplicative
            PlayerMain.knockbackResist = (.3f * PlayerMain.strength + 1) * .2f;//multiplier to reduce the effect of any knockbacks-- unused

            //health gained from STR is slightly under the value of the STR counter
            PlayerMain.health = (((10 + PlayerMain.vitality / 2) * PlayerMain.vitality) / 100 + .07f * PlayerMain.strength) + 1;//increases health based on VIT and STR-- multiplicative
            PlayerHealthHolder.health = PlayerMain.health;//update health script to equal stored health data of playermain
            PlayerMain.meleeVamp = PlayerMain.strength / 40;//increase vampire value by an additional 2.5%-- multiplicative
            PlayerMain.leechText.text = "" + Mathf.RoundToInt(((1.15f + PlayerMain.meleeBoosterDMGScale) * (PlayerMain.baseDamage * PlayerMain.baseDMG * PlayerMain.meleeDamage) * (.1f + PlayerMain.meleeVamp) / 1.33f + 3));//update the vampire text by new values
            PlayerHealthHolder.HealthUp();//heal the player a small amount and update the UI for new health value
            PlayerHealthHolder.Heal(.05f);//added bonus to VIT is a heal for 5% max health
            PlayerMain.abilityDMGBonus = ((.5f * PlayerMain.strength + 1) * .05f) + ((.5f * PlayerMain.intellect + 1) * .05f) + 1;//increase ability damage by 5% base and 2.5% per STR-- multiplicative
            PlayerMain.upgradeUIScript.IncreaseStatCounter(0, PlayerMain.strength);
        }

        if (statToUpgrade == "INT")
        {
            PlayerMain.intellect += upgradeAmount;
            PlayerMain.spellDamage = (.5f * PlayerMain.intellect + 1) * .15f + 1;//increase fire bolt damage by 15% base and 7.5% per INT-- multiplicative
            PlayerMain.cooldownReduction = (Mathf.Sqrt(85 * PlayerMain.intellect) + 10) / 100 + 1;//reduces all ability cooldowns by 10% with an added multiplier the scales down overtime-- multiplicative
            PlayerMain.fireballCapacity = PlayerMain.intellect;//increase total fireball count by X-- additive
            PlayerMain.abilityDMGBonus = ((.5f * PlayerMain.strength + 1) * .05f) + ((.5f * PlayerMain.intellect + 1) * .05f) + 1;//increase ability damage by 5% base and 2.5% per INT-- multiplicative
            if (PlayerMain.fireCooling == false)
            {
                PlayerMain.StartCoroutine(PlayerMain.FireballCooldown());
            }
            PlayerMain.upgradeUIScript.IncreaseStatCounter(1, PlayerMain.intellect);
        }

        if (statToUpgrade == "AGL")
        {
            PlayerMain.agility += upgradeAmount;
            PlayerMain.moveSpeed = (Mathf.Sqrt(175 * PlayerMain.agility) + 2f) / 25;//speed scaling the weakens over time with a base bonus of .4-- additive
            PlayerMain.baseDamage = .055f * PlayerMain.agility + 1;//increases a base damage mulitplier by +5.5%-- multiplicative
            PlayerMain.debuffBonusDuration = (.05f * PlayerMain.agility) + (.08f * PlayerMain.vitality);//increase duration of debuffs by .05s per AGL-- additive
            PlayerMain.attackSpeed = (1f * PlayerMain.agility + 1) * .05f + 1;//increase all attack speeds by 5% + %5 per X-- multiplicative
            PlayerMain.upgradeUIScript.IncreaseStatCounter(2, PlayerMain.agility);
            PlayerMain.ReturnSpeed();
        }

        if (statToUpgrade == "VIT")
        {
            PlayerMain.vitality += upgradeAmount;
            PlayerMain.health = (((10 + PlayerMain.vitality / 2) * PlayerMain.vitality) / 100 + .07f * PlayerMain.strength) + 1;//increases health based on VIT and STR, Vit provides a much larger bonus-- multiplicative
            PlayerMain.debuffReduction = Mathf.Sqrt(50 * PlayerMain.vitality) / 100;//reduce mortal wounds duration
            PlayerHealthHolder.health = PlayerMain.health;
            PlayerHealthHolder.HealthUp();
            PlayerHealthHolder.Heal(.25f);//added bonus to VIT is a heal for 25% max health
            PlayerMain.debuffBonusDuration = (.05f * PlayerMain.agility) + (.08f * PlayerMain.vitality);//increase duration of debuffs by .08s per VIT-- additive
            PlayerMain.upgradeUIScript.IncreaseStatCounter(3, PlayerMain.vitality);
        }
        PlayerMain.StartCoroutine(PlayerMain.ShownSections(1));
        yield return new WaitForSeconds(.2f);
    }
    //list of methods for upgrading player Powers
    public IEnumerator UnlockPrimaryEffect()//used for primary powers
    {
        GameObject Player = GameObject.FindWithTag("Player");//find player
        PlayerMain = Player.GetComponent<PlayerControls>();
        bool addPower = true;//create bool to check if its a new power or old
        foreach(string power in PlayerMain.activePrimaryEffects)//check list of powers in player relative to power type
        {
            if(power.Contains(tempString))//if the power in player already exists then change the prefix of the new power and set the bool false
            {
                tempString = tempString.Replace("OnHit", "Permanant");
                tempString = tempString.Replace("OnStart", "Permanant");
                addPower = false;
                PlayerMain.UnlockPowerBooster(tempString);//run the method to add the power upgrade to permanent boosters
            }
        }
        if(addPower)//if the player does not own the power then add it to the list of powers
        {
            PlayerMain.activePrimaryEffects.Add(tempString);
        }
        PlayerMain.StartCoroutine(PlayerMain.ShownSections(2));//show power UI section
        AddToUICounter(tempString);//send string name to the UI counter method, this will manage the power icon and tracker visual on the UI
        yield return new WaitForSeconds(.2f);
    }
    public IEnumerator UnlockMeleeEffect()
    {
        GameObject Player = GameObject.FindWithTag("Player");
        PlayerMain = Player.GetComponent<PlayerControls>();
        bool addPower = true;//create bool to check if its a new power or old
        if (tempString.Contains("Permanant"))//if the string name has permanant in it then run the power booster
        {
            addPower = false;
            PlayerMain.UnlockPowerBooster(tempString);//run the method to add the power upgrade to permanent boosters
        }
        foreach (string power in PlayerMain.activeMeleeEffects)//check list of powers in player relative to power type
        {
            if (power.Contains(tempString))//if the power in player already exists then change the prefix of the new power and set the bool false
            {
                tempString = tempString.Replace("OnHit", "Permanant");
                tempString = tempString.Replace("OnStart", "Permanant");
                addPower = false;
                PlayerMain.UnlockPowerBooster(tempString);//run the method to add the power upgrade to permanent boosters
            }
        }
        if (addPower)//if the player does not own the power then add it to the list of powers
        {
            PlayerMain.activeMeleeEffects.Add(tempString);
        }
        PlayerMain.StartCoroutine(PlayerMain.ShownSections(2));
        AddToUICounter(tempString);
        yield return new WaitForSeconds(.2f);
    }
    public IEnumerator UnlockAbilityEffect()
    {
        GameObject Player = GameObject.FindWithTag("Player");
        PlayerMain = Player.GetComponent<PlayerControls>();
        bool addPower = true;//create bool to check if its a new power or old
        if (tempString.Contains("Permanant"))//if the string name has permanant in it then run the power booster
        {
            addPower = false;
            PlayerMain.UnlockPowerBooster(tempString);//run the method to add the power upgrade to permanent boosters
        }
        foreach (string power in PlayerMain.activeAbilityEffects)//check list of powers in player relative to power type
        {
            if (power.Contains(tempString))//if the power in player already exists then change the prefix of the new power and set the bool false
            {
                tempString = tempString.Replace("OnHit", "Permanant");
                tempString = tempString.Replace("OnStart", "Permanant");
                addPower = false;
                PlayerMain.UnlockPowerBooster(tempString);//run the method to add the power upgrade to permanent boosters
            }
        }
        if (addPower)//if the player does not own the power then add it to the list of powers
        {
            PlayerMain.activeAbilityEffects.Add(tempString);
        }
        PlayerMain.StartCoroutine(PlayerMain.ShownSections(2));
        AddToUICounter(tempString);
        yield return new WaitForSeconds(.2f);
    }
    //method for power stacking-- takes in the power string name 
    public IEnumerator PermanantPowerBooster(string powerTargetName)
    {
        //with the string name it checks the various statments and when one is found true it executes the stacking effect
        GameObject Player = GameObject.FindWithTag("Player");
        PlayerMain = Player.GetComponent<PlayerControls>();
        if (powerTargetName.Contains("ExplosiveRounds"))
        {
            //stack in increase bomb damage and size
            PlayerMain.bombTotal += 1;
            PlayerMain.bombSize = Mathf.Sqrt(bombTotal);
        }
        if (powerTargetName.Contains("SplitRounds"))
        {
            PlayerMain.splitRoundTotal += 1;
        }
        if (powerTargetName.Contains("Ignite"))
        {
            PlayerMain.incendiaryMagicTotal += 1;
        }
        if (powerTargetName.Contains("Bleed"))
        {
            PlayerMain.bleedClawTotal += 1;
        }
        if (powerTargetName.Contains("Heal"))
        {
            PlayerMain.abilityHealTotal += 1;
        }
        if (powerTargetName.Contains("MeleeBooster"))
        {
            //stacking will increase booster value, melee size, melee base damage scalar and the leech text on the UI
            PlayerMain.meleeBoosterTotal += 1;
            Vector3 sizeBooster = new Vector3(.2f, 0, .2f);
            PlayerMain.meleeMainOBJ.transform.localScale += sizeBooster;
            PlayerMain.meleeBoosterDMGScale += .14f;
            PlayerMain.leechText.text = "" + Mathf.RoundToInt(((1.15f + PlayerMain.meleeBoosterDMGScale) * (PlayerMain.baseDamage * PlayerMain.baseDMG * PlayerMain.meleeDamage) * (.1f + PlayerMain.meleeVamp) / 1.33f + 3));
        }
        if (powerTargetName.Contains("Oil"))
        {
            PlayerMain.oilClawTotal += 1;
        }
        if (powerTargetName.Contains("Clarity"))
        {
            //stacking will increase the ability charge limit and set each ability into cooldown to increase it to the new limit
            PlayerMain.clarityTotal += 1;
            PlayerMain.abilityNumIsCooling[0] = true;
            PlayerMain.abilityNumIsCooling[1] = true;
            PlayerMain.abilityNumIsCooling[2] = true;
            StartCoroutine(PlayerMain.AbilityCooling(0, 3f / PlayerMain.cooldownReduction, PlayerMain.abilityCooldownIMGs[0]));
            StartCoroutine(PlayerMain.AbilityCooling(1, 3f / PlayerMain.cooldownReduction, PlayerMain.abilityCooldownIMGs[1]));
            StartCoroutine(PlayerMain.AbilityCooling(2, 3f / PlayerMain.cooldownReduction, PlayerMain.abilityCooldownIMGs[2]));
        }
        yield return new WaitForSeconds(.2f);
    }
    //list of methods for unlocking new abilites
    public IEnumerator UnlockPlayerAbility(string upgradeTitle, string upgradeEffect, float cooldown, Sprite abilityIMG)
    {//when an ability is picked up this will send the pickup info to the ability swap manager
        GameObject Player = GameObject.FindWithTag("Player");
        abilityMenuOBJ = Player.GetComponentInChildren<PlayerAbilitySwapMenu>();
        abilityMenuOBJ.PauseGame(upgradeTitle, upgradeEffect, cooldown, abilityIMG);
        yield return new WaitForSeconds(.2f);
    }
    public void AddToUICounter(string powerName)
    {//this will update the UI trackers with the proper data provided, first it removes spaces from strings and checks what the string is
        //once found it will tell the designate counter what the array position is for the power 
        /*
         * UI counter order list
         * 0: split flames
         * 1: fire bombs
         * 2: fire magic
         * 3: bleed claws
         * 4: large claw
         * 5: oil claw
         * 6: healing flames
         * 7: tome of clarity
        */
        powerName = powerName.Replace(" ", "");//remove all spaces from the string
        if (powerName.Contains("SplitRounds"))
        {
            DesignateUICounter(PlayerMain.positionInUI[0], 0); //used for the power upgrade system to place it in UI properly
        }
        if (powerName.Contains("ExplosiveRounds"))
        {
            DesignateUICounter(PlayerMain.positionInUI[1], 1); //used for the power upgrade system to place it in UI properly
        }
        if (powerName.Contains("DebuffIgnite"))
        {
            DesignateUICounter(PlayerMain.positionInUI[2], 2); //used for the power upgrade system to place it in UI properly
        }
        if (powerName.Contains("DebuffBleed"))
        {
            DesignateUICounter(PlayerMain.positionInUI[3], 3); //used for the power upgrade system to place it in UI properly
        }
        if (powerName.Contains("MeleeBooster"))
        {
            DesignateUICounter(PlayerMain.positionInUI[4], 4); //used for the power upgrade system to place it in UI properly
        }
        if (powerName.Contains("DebuffOil"))
        {
            DesignateUICounter(PlayerMain.positionInUI[5], 5); //used for the power upgrade system to place it in UI properly
        }
        if (powerName.Contains("Heal"))
        {
            DesignateUICounter(PlayerMain.positionInUI[6], 6); //used for the power upgrade system to place it in UI properly
        }
        if (powerName.Contains("Clarity"))
        {
            DesignateUICounter(PlayerMain.positionInUI[7], 7); //used for the power upgrade system to place it in UI properly
        }
    }
    public void DesignateUICounter(int UIPositonVal, int positionValue)
    {//based on array position of powers this will add the power icon to the next open slot on the UI and update the num tracker with a +1
        if (PlayerMain.designatedIntValue[positionValue] == 0)//designated int value is used to find if the icon is in the UI yet, if its 0 then the power doesnt exist in the UI and is added to the next open slot
        {
            PlayerMain.targetPoints.Add(positionValue);//add a 0 value object to the list, this is used for tracking
            PlayerMain.designatedIntValue[positionValue] += 1;//set the icon arrays designated value to a 1 meaning it has a spot in the UI
            PlayerMain.upgradeUIScript.ChangeSprite(positionValue);//send the change in data to the upgrade UI script
            PlayerMain.positionInUI[positionValue] = PlayerMain.targetPoints.Count - 1;//set its location value in the array to be the list count - 1
        }
        else//runs when the designated value is above 0, meaning the UI array has a spot in the UI
        {
            //update the power UI with a +1 value based on the location of the power icon
            PlayerMain.designatedIntValue[positionValue] += 1;
            PlayerMain.upgradeUIScript.IncreaseCounter(UIPositonVal, PlayerMain.designatedIntValue[positionValue]);
        }
    }
}
