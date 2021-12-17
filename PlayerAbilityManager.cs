using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//playerAbilityManager holds the data and activation of all player abilities, the script auto links to the player scripts
//when the player uses an ability it will start the ActivateAbility method and take in the string name of that ability along with the ability number (0,1,2)
//from there the string will invoke the method with the same string name and cause a cooldown for the ability number

//this script also holds all player ability prefabs and cooldowns and other player variables
public class PlayerAbilityManager : PlayerControlsLink
{
    //change all abilities to use a coroutine to allow for use of animations on abilities


    //collection of ability prefabs the player can use
    public GameObject clusterBomb, dodgeEffect, flameBreath, fireBlast, oilSlick, combustion, whirlwind, crushFlame;
    //collection of ability cooldowns
    public float clusterCooldown, dodgeCooldown, breathCooldown, fireBlastCooldown, oilSlickCooldown, combustionCooldown, whirlwindCooldown, crushFlameCooldown;
    //collection of ability descriptions
    public string clusterTitle, dodgeTitle, breathTitle, fireBlastTitle, oilSlickTitle, combustionTitle, whirlwindTitle, crushFlameTitle;
    public string clusterEffect, dodgeEffectText, breathEffect, fireBlastEffect, oilSlickEffect, combustionEffect, whirlwindEffect, crushFlameEffect;
    //collection of ability cast times
    public float clusterCast, dodgeCast, breathCast, fireBlastCast, oilSlickCast, combustionCast, whirlwindCast, crushFlameCast;
    //collection of UI images
    public Sprite clusterIMG, dodgeIMG, breathIMG, fireBlastIMG, oilSlickIMG, combustionIMG, whirlwindIMG, crushFlameIMG;
    //collection of ability cast sounds
    public AudioClip clusterClip, dodgeClip, flameBreathClip, fireBlastClip, oilSlickClip, combustionClip, whirlwindClip, crushFlameClip;

    public bool isDescription;

    private int tempAbilNum;
    //any refrence to player abilites (0,1,2) are used below and they all start with the ActivateAbility method
    public void ActivateAbility(string abilName, int abilNum)//use an ability based on the string passed in and track the ability number for cooldowns and other effects
    {
        //Invoke(abilName, 0f);
        tempAbilNum = abilNum;
        StartCoroutine(abilName);
    }
    public IEnumerator ClusterBomb()//create a projectile the explodes multiple times
    {
        //prep and play audio
        PlayerMain.mainSource.clip = clusterClip;
        PlayerMain.mainSource.pitch = 1f;
        PlayerMain.mainSource.PlayScheduled(AudioSettings.dspTime + .3f);

        //update UI with ability num and start the cooldown
        PlayerMain.abilityNumCount[tempAbilNum]--;
        PlayerMain.abilityCounterText[tempAbilNum].text = "" + PlayerMain.abilityNumCount[tempAbilNum];
        if (PlayerMain.abilityNumCount[tempAbilNum] <= 0)
        {
            PlayerMain.CanAbility[tempAbilNum] = false;
        }
        if(!PlayerMain.abilityNumIsCooling[tempAbilNum])
        {
            PlayerMain.abilityNumIsCooling[tempAbilNum] = true;
            StartCoroutine(PlayerMain.AbilityCooling(tempAbilNum, clusterCooldown / PlayerMain.cooldownReduction, PlayerMain.abilityCooldownIMGs[tempAbilNum]));
        }

        //start the animation and speed modifiers
        PlayerMain.playerAnimOBJ.animatorOBJ.SetTrigger("usingPrimary");
        PlayerMain.playerAnimOBJ.animatorOBJ.SetFloat("attackAnimSpeed", PlayerMain.attackSpeed / (clusterCast * 1.6f));
        PlayerMain.newSpeed = (PlayerMain.speed + PlayerMain.moveSpeed) * PlayerMain.moveSpeedReduction;
        PlayerMain.handGlow[0].SetActive(true);

        yield return new WaitForSeconds(clusterCast / PlayerMain.attackSpeed);

        //create projectile and reset certain values
        Instantiate(clusterBomb, PlayerMain.BulletSpawn.position, PlayerMain.BulletSpawn.rotation);
        PlayerMain.ReturnSpeed();
        PlayerMain.handGlow[0].SetActive(false);
    }
    public IEnumerator Dodge()//dodge will move the player in the direction of movement based on data in the playerControls for the teleportPoint
    {
        //prep and play audio
        PlayerMain.mainSource.clip = dodgeClip;
        PlayerMain.mainSource.pitch = 1f;
        PlayerMain.mainSource.volume = .6f;
        PlayerMain.mainSource.PlayScheduled(AudioSettings.dspTime + .1f);

        //update UI with ability num and start the cooldown
        PlayerMain.abilityNumCount[tempAbilNum]--;
        PlayerMain.abilityCounterText[tempAbilNum].text = "" + PlayerMain.abilityNumCount[tempAbilNum];
        if (PlayerMain.abilityNumCount[tempAbilNum] <= 0)
        {
            PlayerMain.CanAbility[tempAbilNum] = false;
        }
        if (!PlayerMain.abilityNumIsCooling[tempAbilNum])
        {
            PlayerMain.abilityNumIsCooling[tempAbilNum] = true;
            StartCoroutine(PlayerMain.AbilityCooling(tempAbilNum, dodgeCooldown / PlayerMain.cooldownReduction, PlayerMain.abilityCooldownIMGs[tempAbilNum]));
        }

        //start the animation and speed modifiers
        PlayerMain.playerAnimOBJ.animatorOBJ.SetTrigger("usingSupport");
        PlayerMain.playerAnimOBJ.animatorOBJ.SetFloat("attackAnimSpeed", PlayerMain.attackSpeed / (dodgeCast * 1.25f));
        PlayerMain.handGlow[0].SetActive(true);
        PlayerMain.handGlow[1].SetActive(true);

        //create the dodge visual object at start location
        GameObject DodgeEffectOBJ1 = Instantiate(dodgeEffect, PlayerMain.gameObject.transform.position, PlayerMain.gameObject.transform.rotation);
        Destroy(DodgeEffectOBJ1, .4f);

        yield return new WaitForSeconds(dodgeCast / PlayerMain.attackSpeed);
        //find and fire raycast from the players movement direction, raycast travels up to 7 spaces
        Vector3 backTarget = new Vector3(PlayerMain.RayEndPoint.transform.position.x - PlayerMain.transform.position.x, 1.75f, PlayerMain.RayEndPoint.transform.position.z - PlayerMain.transform.position.z);
        Physics.Raycast(PlayerMain.gameObject.transform.position, backTarget, out PlayerMain.HitPoint, 7);
        if (PlayerMain.HitPoint.transform == null)
        {//if the ray doesnt hit anything then teleport to ray position
            PlayerMain.gameObject.transform.localPosition = PlayerMain.RayEndPoint.position;
        }
        else
        {//if the ray hits an object then teleport to the rays collision point
            PlayerMain.gameObject.transform.localPosition = PlayerMain.HitPoint.point;
        }
        //teleport to the position found and set the player Y position back to 0
        Vector3 correctionPoint = new Vector3(PlayerMain.transform.localPosition.x, 0, PlayerMain.transform.localPosition.z);
        PlayerMain.transform.localPosition = correctionPoint;
        //create the dodge visual object at end location
        GameObject DodgeEffectOBJ2 = Instantiate(dodgeEffect, PlayerMain.gameObject.transform.position, PlayerMain.gameObject.transform.rotation);
        Destroy(DodgeEffectOBJ2, .4f);

        PlayerMain.handGlow[0].SetActive(false);
        PlayerMain.handGlow[1].SetActive(false);
    }
    public IEnumerator FlameBreath()//create a carpet of flame
    {
        //prep and play audio
        PlayerMain.mainSource.clip = flameBreathClip;
        PlayerMain.mainSource.pitch = .4f;
        PlayerMain.mainSource.volume = .3f;
        PlayerMain.mainSource.Play();

        //update UI with ability num and start the cooldown
        PlayerMain.abilityNumCount[tempAbilNum]--;
        PlayerMain.abilityCounterText[tempAbilNum].text = "" + PlayerMain.abilityNumCount[tempAbilNum];
        if (PlayerMain.abilityNumCount[tempAbilNum] <= 0)
        {
            PlayerMain.CanAbility[tempAbilNum] = false;
        }
        if (!PlayerMain.abilityNumIsCooling[tempAbilNum])
        {
            PlayerMain.abilityNumIsCooling[tempAbilNum] = true;
            StartCoroutine(PlayerMain.AbilityCooling(tempAbilNum, breathCooldown / PlayerMain.cooldownReduction, PlayerMain.abilityCooldownIMGs[tempAbilNum]));
        }

        //start the animation and speed modifiers
        PlayerMain.playerAnimOBJ.animatorOBJ.SetTrigger("usingSupport");
        PlayerMain.playerAnimOBJ.animatorOBJ.SetFloat("attackAnimSpeed", PlayerMain.attackSpeed / (breathCast * 1.25f));
        PlayerMain.newSpeed = (PlayerMain.speed + PlayerMain.moveSpeed) * (PlayerMain.moveSpeedReduction - .1f);
        PlayerMain.handGlow[0].SetActive(true);
        PlayerMain.handGlow[1].SetActive(true);

        yield return new WaitForSeconds(breathCast / PlayerMain.attackSpeed);

        //create projectile and reset certain values
        Instantiate(flameBreath, PlayerMain.gameObject.transform.position, PlayerMain.BulletSpawn.rotation);
        PlayerMain.ReturnSpeed();
        PlayerMain.handGlow[0].SetActive(false);
        PlayerMain.handGlow[1].SetActive(false);
    }
    public IEnumerator FireBlast()//create a expanding blast of fire on the player
    {
        //prep and play audio
        PlayerMain.mainSource.clip = fireBlastClip;
        PlayerMain.mainSource.pitch = .7f;
        PlayerMain.mainSource.volume = .3f;
        PlayerMain.mainSource.Play();

        //update UI with ability num and start the cooldown
        PlayerMain.abilityNumCount[tempAbilNum]--;
        PlayerMain.abilityCounterText[tempAbilNum].text = "" + PlayerMain.abilityNumCount[tempAbilNum];
        if (PlayerMain.abilityNumCount[tempAbilNum] <= 0)
        {
            PlayerMain.CanAbility[tempAbilNum] = false;
        }
        if (!PlayerMain.abilityNumIsCooling[tempAbilNum])
        {
            PlayerMain.abilityNumIsCooling[tempAbilNum] = true;
            StartCoroutine(PlayerMain.AbilityCooling(tempAbilNum, fireBlastCooldown / PlayerMain.cooldownReduction, PlayerMain.abilityCooldownIMGs[tempAbilNum]));
        }

        //start the animation and speed modifiers
        PlayerMain.playerAnimOBJ.animatorOBJ.SetTrigger("usingBlast");
        PlayerMain.playerAnimOBJ.animatorOBJ.SetFloat("attackAnimSpeed", PlayerMain.attackSpeed / (fireBlastCast * 1.25f));
        PlayerMain.newSpeed = (PlayerMain.speed + PlayerMain.moveSpeed) * (PlayerMain.moveSpeedReduction - .1f);
        PlayerMain.handGlow[0].SetActive(true);
        PlayerMain.handGlow[1].SetActive(true);

        yield return new WaitForSeconds(fireBlastCast / 2 / PlayerMain.attackSpeed);

        //create projectile and reset certain values
        Instantiate(fireBlast, PlayerMain.gameObject.transform.position, PlayerMain.BulletSpawn.rotation);
        PlayerMain.ReturnSpeed();
        PlayerMain.handGlow[0].SetActive(false);
        PlayerMain.handGlow[1].SetActive(false);
    }
    public IEnumerator OilSlick()//create a oil canister the expands slowly
    {
        //prep and play audio
        PlayerMain.mainSource.clip = oilSlickClip;
        PlayerMain.mainSource.pitch = .35f;
        PlayerMain.mainSource.volume = .3f;
        PlayerMain.mainSource.PlayScheduled(AudioSettings.dspTime + .2f);

        //update UI with ability num and start the cooldown
        PlayerMain.abilityNumCount[tempAbilNum]--;
        PlayerMain.abilityCounterText[tempAbilNum].text = "" + PlayerMain.abilityNumCount[tempAbilNum];
        if (PlayerMain.abilityNumCount[tempAbilNum] <= 0)
        {
            PlayerMain.CanAbility[tempAbilNum] = false;
        }
        if (!PlayerMain.abilityNumIsCooling[tempAbilNum])
        {
            PlayerMain.abilityNumIsCooling[tempAbilNum] = true;
            StartCoroutine(PlayerMain.AbilityCooling(tempAbilNum, oilSlickCooldown / PlayerMain.cooldownReduction, PlayerMain.abilityCooldownIMGs[tempAbilNum]));
        }

        //start the animation and speed modifiers
        PlayerMain.playerAnimOBJ.animatorOBJ.SetTrigger("usingSupport");
        PlayerMain.playerAnimOBJ.animatorOBJ.SetFloat("attackAnimSpeed", PlayerMain.attackSpeed / (oilSlickCast * 1.25f));
        PlayerMain.newSpeed = (PlayerMain.speed + PlayerMain.moveSpeed) * PlayerMain.moveSpeedReduction;
        PlayerMain.handGlow[0].SetActive(true);

        yield return new WaitForSeconds(oilSlickCast / PlayerMain.attackSpeed);

        //create projectile and reset certain values
        Instantiate(oilSlick, PlayerMain.gameObject.transform.position, PlayerMain.BulletSpawn.rotation);
        PlayerMain.ReturnSpeed();
        PlayerMain.handGlow[0].SetActive(false);
    }
    public IEnumerator Combustion()//combust the nearest target within range
    {
        GameObject[] enemies;//create enemy array
        EnemyAI nearestTarget = null;//make variable of nearest target which is the enemy AI script
        enemies = GameObject.FindGameObjectsWithTag("Enemy");//populate array with every enemy 
        if (enemies.Length > 0)//run if there is enemies
        {
            GameObject currentTarget = null;//set variable of current target object
            float lowestDistance = 999;//set variable of lowest distance
            foreach (GameObject enemy in enemies)//check each enemy in the list
            {
                EnemyAI enemyAIOBJ = enemy.GetComponent<EnemyAI>();//track its AI script
                EnemyHealth enemyHealthOBJ = enemy.GetComponent<EnemyHealth>();//track its health script
                float distanceVal = Vector3.Distance(enemy.transform.position, PlayerMain.transform.position);//check its distance relative to player
                if(distanceVal <= 28 && distanceVal < lowestDistance && !enemyHealthOBJ.died)//if the enemy is within range, closer then the current lowestDistance and alive
                {
                    //set variables to the current enemy
                    currentTarget = enemy;
                    lowestDistance = distanceVal;
                    nearestTarget = enemyAIOBJ;
                }
            }
            if(currentTarget != null)//if there is a target then run the ability activation
            {

                //update UI with ability num and start the cooldown
                PlayerMain.abilityNumCount[tempAbilNum]--;
                PlayerMain.abilityCounterText[tempAbilNum].text = "" + PlayerMain.abilityNumCount[tempAbilNum];
                if (PlayerMain.abilityNumCount[tempAbilNum] <= 0)
                {
                    PlayerMain.CanAbility[tempAbilNum] = false;
                }
                if (!PlayerMain.abilityNumIsCooling[tempAbilNum])
                {
                    PlayerMain.abilityNumIsCooling[tempAbilNum] = true;
                    StartCoroutine(PlayerMain.AbilityCooling(tempAbilNum, combustionCooldown / PlayerMain.cooldownReduction, PlayerMain.abilityCooldownIMGs[tempAbilNum]));
                }

                //prep and play audio
                PlayerMain.mainSource.clip = combustionClip;
                PlayerMain.mainSource.pitch = 1f;
                PlayerMain.mainSource.volume = .3f;
                PlayerMain.mainSource.Play();

                //start the animation and speed modifiers
                PlayerMain.playerAnimOBJ.animatorOBJ.SetTrigger("using1HSupport");
                PlayerMain.playerAnimOBJ.animatorOBJ.SetFloat("attackAnimSpeed", PlayerMain.attackSpeed / (combustionCast * 1.25f));
                PlayerMain.newSpeed = (PlayerMain.speed + PlayerMain.moveSpeed) * PlayerMain.moveSpeedReduction;
                PlayerMain.handGlow[0].SetActive(true);


                yield return new WaitForSeconds(combustionCast / PlayerMain.attackSpeed);

                //create projectile and reset certain values
                nearestTarget.ActivateDebuff(1, 1, "Combustion", currentTarget);
                PlayerMain.ReturnSpeed();
                PlayerMain.handGlow[0].SetActive(false);
            }
        }
        if(enemies.Length <= 0 || nearestTarget == null)//if there is no enemies or it fails to find a nearby target
        {//run the cooldown but do not create any projectile, cooldown is half of the typical cooldown

            //update UI with ability num and start the cooldown
            PlayerMain.abilityNumCount[tempAbilNum]--;
            PlayerMain.abilityCounterText[tempAbilNum].text = "" + PlayerMain.abilityNumCount[tempAbilNum];
            if (PlayerMain.abilityNumCount[tempAbilNum] <= 0)
            {
                PlayerMain.CanAbility[tempAbilNum] = false;
            }
            if (!PlayerMain.abilityNumIsCooling[tempAbilNum])
            {
                PlayerMain.abilityNumIsCooling[tempAbilNum] = true;
                StartCoroutine(PlayerMain.AbilityCooling(tempAbilNum, combustionCooldown / 2 / PlayerMain.cooldownReduction, PlayerMain.abilityCooldownIMGs[tempAbilNum]));
            }
        }
    }
    public IEnumerator WhirlWind()//create a whirlwind that follows the player and deals damage to targets
    {
        //audio is handled by the game object

        //update UI with ability num and start the cooldown
        PlayerMain.abilityNumCount[tempAbilNum]--;
        PlayerMain.abilityCounterText[tempAbilNum].text = "" + PlayerMain.abilityNumCount[tempAbilNum];
        if (PlayerMain.abilityNumCount[tempAbilNum] <= 0)
        {
            PlayerMain.CanAbility[tempAbilNum] = false;
        }
        if (!PlayerMain.abilityNumIsCooling[tempAbilNum])
        {
            PlayerMain.abilityNumIsCooling[tempAbilNum] = true;
            StartCoroutine(PlayerMain.AbilityCooling(tempAbilNum, whirlwindCooldown / PlayerMain.cooldownReduction, PlayerMain.abilityCooldownIMGs[tempAbilNum]));
        }

        //start the animation and speed modifiers
        PlayerMain.playerAnimOBJ.animatorOBJ.SetTrigger("usingBlast");
        PlayerMain.playerAnimOBJ.animatorOBJ.SetFloat("attackAnimSpeed", PlayerMain.attackSpeed / (fireBlastCast * 1.25f));
        PlayerMain.newSpeed = (PlayerMain.speed + PlayerMain.moveSpeed) * (PlayerMain.moveSpeedReduction - .1f);
        PlayerMain.handGlow[0].SetActive(true);
        PlayerMain.handGlow[1].SetActive(true);


        yield return new WaitForSeconds(whirlwindCast / 2 / PlayerMain.attackSpeed);

        //create projectile and reset certain values
        Instantiate(whirlwind, PlayerMain.gameObject.transform.position, PlayerMain.BulletSpawn.rotation);
        PlayerMain.ReturnSpeed();
        StartCoroutine(DamageReduction());//special effect of the ability, reduces player damage taken
        PlayerMain.handGlow[0].SetActive(false);
        PlayerMain.handGlow[1].SetActive(false);
    }
    public IEnumerator CrushingFlame()
    {
        //prep and play audio
        PlayerMain.mainSource.clip = crushFlameClip;
        PlayerMain.mainSource.pitch = .7f;
        PlayerMain.mainSource.volume = .3f;
        PlayerMain.mainSource.Play();

        //update UI with ability num and start the cooldown
        PlayerMain.abilityNumCount[tempAbilNum]--;
        PlayerMain.abilityCounterText[tempAbilNum].text = "" + PlayerMain.abilityNumCount[tempAbilNum];
        if (PlayerMain.abilityNumCount[tempAbilNum] <= 0)
        {
            PlayerMain.CanAbility[tempAbilNum] = false;
        }
        if (!PlayerMain.abilityNumIsCooling[tempAbilNum])
        {
            PlayerMain.abilityNumIsCooling[tempAbilNum] = true;
            StartCoroutine(PlayerMain.AbilityCooling(tempAbilNum, crushFlameCooldown / PlayerMain.cooldownReduction, PlayerMain.abilityCooldownIMGs[tempAbilNum]));
        }

        //start the animation and speed modifiers
        PlayerMain.playerAnimOBJ.animatorOBJ.SetTrigger("usingBlast");
        PlayerMain.playerAnimOBJ.animatorOBJ.SetFloat("attackAnimSpeed", PlayerMain.attackSpeed / (crushFlameCast * 1.25f));
        PlayerMain.newSpeed = (PlayerMain.speed + PlayerMain.moveSpeed) * (PlayerMain.moveSpeedReduction - .1f);
        PlayerMain.handGlow[0].SetActive(true);
        PlayerMain.handGlow[1].SetActive(true);

        yield return new WaitForSeconds(whirlwindCast / 2 / PlayerMain.attackSpeed);

        //create projectile and reset certain values
        Instantiate(crushFlame, PlayerMain.BulletSpawn.position, PlayerMain.BulletSpawn.rotation);
        PlayerMain.ReturnSpeed();
        StartCoroutine(DamageReduction());
        PlayerMain.handGlow[0].SetActive(false);
        PlayerMain.handGlow[1].SetActive(false);
    }
    public void ActivateAbilityPower(string powerName)
    {
        Invoke(powerName, 0.0f);//run any method using the string power name
    }
    public void OnStartHeal()//healing method from the previous method, heal the player for a percent of max health
    {
        if(PlayerHealthHolder.canHeal)
        {
            PlayerHealthHolder.Heal(.012f * PlayerMain.abilityHealTotal + .02f);//heal for 2% + 1.2% per upgrade of healing flame
        }
    }
    public IEnumerator DamageReduction()//coroutine for reducing damage taken for a short time
    {
        PlayerHealthHolder.damageReduction = .75f;//damage reduction will reduce all damage by 25% for 3.5s
        yield return new WaitForSeconds(3.5f);
        PlayerHealthHolder.damageReduction = 1f;
    }
}
