using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//primary AI controls for the boss fight
//this script holds the variables of each attack along with audio, rotation, targets and move speeds
//the boss cycles through melee and ranged modes for combat along with a special attack to lock down areas within the arena
//each boss kill will add new attacks to the boss while also changing his size and name
public class AWBossAI : MonoBehaviour
{
    //arcane dart data
    public GameObject arcaneDartOBJ;
    public float ADFireRate, ADCastTime;
    public bool useDart;

    //arcane hailstorm data
    public GameObject hailstormOBJ;
    public float hailstormDuration, hailstormCastTime;
    private bool isHailStorm;

    //elemental stomp data
    public GameObject elementalStompOBJ;
    public float stompCastTime;

    //elemental thrust data
    public GameObject elementalThrustOBJ;
    public float thrustCastTime;
    public float thrustSpread;

    //arcane enclosure data
    public GameObject[] AEOBJs;
    public int cyclesUntilEnclosure;
    public float AECastTime;
    private int cycleCounter, AEsAllowed = 2;
    public List<GameObject> AEOBJList = new List<GameObject>();
    private bool enclosureActive;
    private bool permanantAEActive;
    public GameObject AEPermanant;
    private int AERollModifier;
    public AudioClip AECastSound;

    //arcane flame data
    public GameObject arcaneFlameOBJ;
    public int arcaneFlameSpawnCount;
    public int killsTillFlameUsage;
    public float arcaneFlameCasttime;
    public float arcaneFlameCooldown;

    //particle orb data
    public GameObject particleOrbOBJ;
    public float pOrbCooldown;
    public float pOrbCasttime;
    public int killsTillOrbUsage;
    private bool canOrb = true;

    //spiked snare data
    public GameObject spikeSnareOBJ;
    public float sSnareCooldown;
    public float sSnareCasttime;
    public int killsTillSnareUsage;
    private bool canSnare = true;

    //add summon data
    public GameObject[] addCollection;
    public float addCooldown;
    public float addCasttime;
    public int addCount;
    public int killsTillAddSpawning;

    //basic combat data
    public float distanceForMelee;//distance for the boss to activate melee attacks
    public Transform bulletSpawn;
    public float distanceFromPlayer;
    private int abilityLowRoll = 0, abilityHighRoll = 2;//roll values that can encourage the boss to use a diffrent attack
    public Transform centerRoomPoint;
    private bool meleeMode = true, rangedMode;
    public float rotationSpeed;//base rotation speed
    private float modifiedRotationSpeed;//rotation speed used for all rotations, modified based on attack used and resets back to base value
    private bool usingAbility;

    //rage variables
    public float arcaneRage;
    public Text rageCounter;
    public Image rageBar;
    public GameObject enragedFog;
    private GameObject fallBlockOBJ;
    public GameObject pillarBurstFX;
    private bool droppedBlocks;

    public float scalingDMG;//scale factor to increase all damage by a multiplied value
    public GameObject leftGlow, rightGlow;//visual effect of casting on his hands similar to the players hand glows
    public Text bossTitle;
    public AudioSource bossVoiceSource;
    public AudioClip[] bossVoices;

    //refrence data
    public RoomMechanic roomMechanicOBJ;
    public BossMoverAI enemAI;
    private EnemyHealth bossHealthScript;
    public GameObject player;
    private Transform targetRotater;
    private GameStateController gameStateScript;
    public AudioSource castSource, enclosureSource, collapseSource;

    public void Start()
    {
        //Reference various scripts
        enemAI = GetComponent<BossMoverAI>();

        bossHealthScript = GetComponent<EnemyHealth>();

        GameObject gameControllerOBJ = GameObject.FindGameObjectWithTag("GameController");

        //use level manager as reference and find the center room point of boss room along with the fall blocks object
        LevelManager levelManagerScript = gameControllerOBJ.GetComponentInChildren<LevelManager>();
        GameObject roomOBJ = levelManagerScript.ActiveRoomPrefabs[levelManagerScript.ActiveRoomPrefabs.Count - 1];
        roomMechanicOBJ = roomOBJ.GetComponent<RoomMechanic>();
        centerRoomPoint = roomMechanicOBJ.roomCenterPoint;
        fallBlockOBJ = roomMechanicOBJ.bossDropBlocks;

        //find main scene to get boss kill counter and scaling counter
        GameObject mainSceneManager = GameObject.FindGameObjectWithTag("MainSceneController");
        gameStateScript = mainSceneManager.GetComponentInChildren<GameStateController>();

        scalingDMG += gameStateScript.enemyDamageBonus - 1;//update damage modifier based on game state damage bonus, this will increase all damage by the multiplier
        
        //based on boss kill count change the bosses name and size
        if(gameStateScript.bossKillCounter == 1)
        {
            bossTitle.text = ("Archmage Cramit, Scourge of Crescent Peaks") + ("  +");
            transform.localScale = new Vector3(transform.localScale.x * 1.15f, transform.localScale.y * 1.15f, transform.localScale.z * 1.15f);
        }
        if (gameStateScript.bossKillCounter == 2)
        {
            bossTitle.text = ("Archmage Cramit, Keeper of the Shifting Keep") + ("  + +");
            transform.localScale = new Vector3(transform.localScale.x * 1.3f, transform.localScale.y * 1.3f, transform.localScale.z * 1.3f);
        }
        if (gameStateScript.bossKillCounter == 3)
        {
            bossTitle.text = ("Archmage Cramit, The Necromatic Dragonoid") + ("  + + +");
            transform.localScale = new Vector3(transform.localScale.x * 1.4f, transform.localScale.y * 1.4f, transform.localScale.z * 1.4f);
        }
        if (gameStateScript.bossKillCounter >= 4)
        {
            bossTitle.text = ("Archmage Cramit, The Undying") + ("  + + + +");
            transform.localScale = new Vector3(transform.localScale.x * 1.5f, transform.localScale.y * 1.5f, transform.localScale.z * 1.5f);
            bossHealthScript.Health += 100 * gameStateScript.bossKillCounter;
        }

        //find player and set rotater target to player
        player = GameObject.FindGameObjectWithTag("Player");
        targetRotater = player.transform;
        modifiedRotationSpeed = rotationSpeed;

        //if boss kill count is high enough then enable the arcane flame and summon add abilities
        if(killsTillFlameUsage <= gameStateScript.bossKillCounter)
        {
            arcaneFlameSpawnCount += Mathf.RoundToInt(gameStateScript.bossKillCounter - killsTillFlameUsage) * 2;
            StartCoroutine(ArcaneFlame());
        }
        if (killsTillAddSpawning <= gameStateScript.bossKillCounter)
        {
            addCount += Mathf.RoundToInt(gameStateScript.bossKillCounter - killsTillAddSpawning) * 2;
            StartCoroutine(SummonAdds());
        }

        //start inital rage and combat loops
        StartCoroutine(CombatLoop());
        StartCoroutine(GainRage());
    }
    public void DistanceCheck()//check distance from boss to player and use this distance to determine what combat mode to use
    {
        Vector3 targetDistance;
        targetDistance = transform.position - player.transform.position;
        distanceFromPlayer = targetDistance.sqrMagnitude;
        if(distanceFromPlayer > distanceForMelee * 6)//if the player is way out of range then fire a large amout of dart attacks
        {
            for(int i = 0; i < 5;i++)
            {
                Vector3 spawn = new Vector3(bulletSpawn.position.x, bulletSpawn.position.y + 3, bulletSpawn.position.z);
                GameObject dartOBj = Instantiate(arcaneDartOBJ, spawn, bulletSpawn.rotation, gameObject.transform);
                Destroy(dartOBj, 10f);
            }
        }
    }
    private void Update()
    {
        //constantly update rotation to look towards the player
        Vector3 targetDirection = targetRotater.position - transform.position;
        targetDirection = new Vector3(targetDirection.x, 0, targetDirection.z);
        float singleStep = modifiedRotationSpeed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }
    public IEnumerator GainRage()//gain rage per second which will increase attack speed
    {
        float imgCounter = 0;//visual bar tracker of rage
        while(arcaneRage < 100)//loop runs when rage is less then 100
        {
            arcaneRage += 1f;
            imgCounter += .029f;//tracker goes up a little less then 1/3 per rage gain
            rageBar.fillAmount = imgCounter;//update tracker

            if (arcaneRage > 66)//once rage is at 2/3 then the text changes
            {
                rageCounter.text = "x2";
            }
            else if(arcaneRage > 33)//once rage is at 1/3 then the text changes
            {
                rageCounter.text = "x1";
            }

            if(arcaneRage > 33 && !droppedBlocks)//if rage has reached 1/3 and has not dropped blocks then run this
            {
                collapseSource.PlayScheduled(AudioSettings.dspTime + 1.2f);//play bursting sound in 1.2s

                GameObject burstOBJ = Instantiate(pillarBurstFX, centerRoomPoint.position, pillarBurstFX.transform.rotation);//create the burst FX, a grey ring expands outward
                Destroy(burstOBJ, 2f);

                imgCounter = 0;//reset visual bar counter since it has reached the 1/3 mark of max rage

                //drop block objects to shatter the pillars
                fallBlockOBJ.SetActive(true);
                droppedBlocks = true;
                Destroy(fallBlockOBJ, 3f);
            }

            yield return new WaitForSeconds(.75f);//rage count can loop every .75s

            if (arcaneRage > 66 && !permanantAEActive && !enclosureActive)//if rage is 2/3 and the permanent enclosure hasnt spawned while the boss also is not using that attack then run this
            {
                imgCounter = 0;//reset the visual tracker once more since it is 2/3 at max

                //keep track that the AE attack cannot use the outer area and can only place 1 at a time
                permanantAEActive = true;
                AEsAllowed -= 1;
                AERollModifier -= 1;
                Instantiate(AEPermanant, centerRoomPoint.position, centerRoomPoint.rotation);
            }
        }
        Instantiate(enragedFog, centerRoomPoint.position, centerRoomPoint.rotation);//if rage  is maxed then update the text and create a light purple fog on the ground
        rageCounter.text = "x3";
    }

    public void MeleeMode()//this mode allows for melee based attacks and locks ranged attacks
    {
        //while in this mode the boss has a closer end distance and increased speed and rotation speed
        if(rangedMode)
        {
            modifiedRotationSpeed = rotationSpeed * 1.33f;
            enemAI.aiPath.maxSpeed = enemAI.enemySpeed * 1.45f;
            meleeMode = true;
            rangedMode = false;
            enemAI.aiPath.endReachedDistance = 4;
            StartCoroutine(MeleeLoop());
        }
    }

    public void RangedMode()//this mode allows for ranged based attacks and locks melee attacks
    {
        //while in this mode the speed and rotation speed are default and the end distance is higher
        if(meleeMode)
        {
            modifiedRotationSpeed = rotationSpeed;
            enemAI.aiPath.maxSpeed = enemAI.enemySpeed;
            rangedMode = true;
            meleeMode = false;
            enemAI.aiPath.endReachedDistance = 20;
            StartCoroutine(RangedLoop());
        }
    }

    public IEnumerator CombatLoop()//the primary combat loop, this allows swaping to diffrent modes and activation of the AE attack
    {
        yield return new WaitForSeconds(2.3f);//delay before boss begins combat

        //enable darts which is active from start to finish
        useDart = true;
        StartCoroutine(ArcaneDarts());

        //default to ranged mode
        RangedMode();

        while (true)//constant loop that plays during the boss- manages what attack mode the boss is in and when to use enclosure
        {
            yield return new WaitForSeconds(1.5f);//wait timer before the boss tries to change combat modes or use the AE attack

            yield return new WaitWhile(() => usingAbility);//extra wait timer that prevents the loop from continuing if an ability is in use

            DistanceCheck();//run a check for distance- determines the bosses next attack

            if(distanceFromPlayer <= distanceForMelee * 2f)//allow for melee attacks if the player is within a certain distance from the boss
            {
                MeleeMode();
            }
            else//return back to ranged attacks if the player is to far
            {
                RangedMode();
            }

            yield return new WaitForSeconds(1.5f);//add wait timer after swapping combat modes 

            cycleCounter += 1;//add to the cycle counter
            if(cycleCounter >= cyclesUntilEnclosure && !usingAbility)//if the cycle counter is at its limit and the boss is not actively using an ability then run this
            {
                //locks out combat modes and starts the AE attack coroutine
                rangedMode = false;
                meleeMode = false;
                enclosureActive = true;
                StartCoroutine(ArcaneEnclosure(AEsAllowed));

                yield return new WaitWhile(() => enclosureActive);//while being used it pauses this coroutine

                cycleCounter = 0;//reset the counter 
            }
        }
    }

    public IEnumerator MeleeLoop()//main melee loop that will activate various melee attacks
    {
        enemAI.aiDestination.flexTransform = enemAI.aiDestination.playerTarget;//set destination to player

        while (meleeMode)//keep running the loop while melee mode is still true
        {
            if (killsTillSnareUsage <= gameStateScript.bossKillCounter && canSnare)//if the snare attack is available due to boss kill counter and its ready to use then use it instantly
            {
                canSnare = false;
                StartCoroutine(SpikedSnare());
            }

            yield return new WaitForSeconds(2f);//wait for a moment before trying to use a melee attack

            DistanceCheck();//run a check for distance- determines the bosses next attack

            if (distanceFromPlayer <= distanceForMelee && !enclosureActive)//if in melee range and not using the AE attack then use a melee attack
            {
                int rollval = Random.Range(abilityLowRoll, abilityHighRoll);//roll for what melee action to do

                if (rollval <= 0)
                {
                    StartCoroutine(ElementalStomp());
                }
                else
                {
                    StartCoroutine(ElementalThrust());
                }
            }
        }
    }

    public IEnumerator RangedLoop()//main ranged that will activate various ranged attacks
    {
        enemAI.aiDestination.flexTransform = enemAI.aiDestination.playerTarget;//set destination to player

        while (rangedMode)//keep running the loop while ranged mode is still true
        {
            if (killsTillOrbUsage <= gameStateScript.bossKillCounter && canOrb)//if the orb attack is available due to boss kill counter and its ready to use then use it instantly
            {
                canOrb = false;
                StartCoroutine(ParticleOrb());
            }

            yield return new WaitForSeconds(2.5f);//wait for a moment before trying to use a ranged attack

            int rollval = Random.Range(abilityLowRoll, abilityHighRoll);//roll for what melee action to do
            if (rollval <= 0 && !enclosureActive)//roll allows boss to use hailstorm
            {
                isHailStorm = true;
                StartCoroutine(ArcaneHailStorm());

                yield return new WaitWhile(() => isHailStorm);//prevents the loop from running until this attack is done
            }
            else//roll allows boss to use dart at a faster rate
            {
                ADFireRate = .15f;
                enemAI.anim.SetTrigger("Use1HSupport");

                yield return new WaitForSeconds(2f);//attack is 2s long but doesnt lock out other attacks

                //modify roll to cause hail to be more likely
                abilityLowRoll -= 1;
                abilityHighRoll = 2;
                ADFireRate = 1f;
            }
        }
    }

    //coroutines of combat abilities- darts can play while others are active and does not use any animations
    //abilities follow an operation order set usingAbility true, change rotation speed, change move speed, set anim
    //after ability is done the values are set to default in reverse order and ability rolls are set for next cycle
    public IEnumerator ArcaneDarts()//basic dart attack loop this is active at all times
    {
        while(true)//loop stays on while the boss is alive
        {
            yield return new WaitForSeconds(.01f);//tiny delay of the bosses dart attack

            while (useDart)//check bool to allow the dart to fire
            {
                yield return new WaitForSeconds(ADCastTime * ADFireRate / (arcaneRage / 100 + 1));//wait for this timer before running again, delay for darts can change based on rage and modified fire rate from ranged mode

                Vector3 spawn = new Vector3(bulletSpawn.position.x, bulletSpawn.position.y + 3, bulletSpawn.position.z);//set the spawn area of the dart at the boss position with a +3 to the Y

                GameObject dartOBj  = Instantiate(arcaneDartOBJ, spawn, bulletSpawn.rotation, gameObject.transform);//create the dart
                Destroy(dartOBj, 10f);

                if(arcaneRage >= 100)//if the rage counter has reached its max then spawn an extra dart each time a regular dart spawns
                {
                    GameObject dartOBj2 = Instantiate(arcaneDartOBJ, spawn, bulletSpawn.rotation, gameObject.transform);
                    Destroy(dartOBj2, 10f);
                }
            }
        }
    }

    public IEnumerator ArcaneHailStorm()//hail ability, will create a cone of damage and reduce rotation speed while active, also locks movement
    {
        if(!usingAbility)
        {
            //set visual effects and anims along with changing rotation and move speed
            leftGlow.SetActive(true);
            usingAbility = true;
            modifiedRotationSpeed = .8f;
            enemAI.aiPath.maxSpeed = 0;
            enemAI.anim.SetBool("UsingBlock", true);

            yield return new WaitForSeconds(hailstormCastTime / (arcaneRage / 100 + 1));//runs for animiation duration/cast time

            GameObject hailOBJ = Instantiate(hailstormOBJ, bulletSpawn.position, bulletSpawn.rotation, gameObject.transform);//create the hail object

            yield return new WaitForSeconds(hailstormDuration);//wait for its duration before ending the attack

            Destroy(hailOBJ);

            //stop anims and visual while also bring the speed and rotation to normal values
            enemAI.anim.SetBool("UsingBlock", false);
            enemAI.aiPath.maxSpeed = enemAI.enemySpeed;
            modifiedRotationSpeed = rotationSpeed;
            leftGlow.SetActive(false);

            //re-enable other attacks 
            usingAbility = false;
            isHailStorm = false;

            //modify roll to cause thrust to be more likely
            abilityHighRoll += 1;
            abilityLowRoll = 0;
        }
    }

    public IEnumerator ElementalStomp()//stomp stops movement and creates a large ring around the boss dealing heavy damage, disables rotation and movement while casting
    {
        if(!usingAbility)
        {
            //set visual effects and anims along with changing rotation and move speed
            leftGlow.SetActive(true);
            rightGlow.SetActive(true);
            modifiedRotationSpeed = 0f;
            usingAbility = true;
            enemAI.aiPath.maxSpeed = 0;
            enemAI.anim.SetTrigger("UseAOEBlast");

            yield return new WaitForSeconds(stompCastTime / (arcaneRage / 100 + 1));//runs for animiation duration/cast time

            transform.LookAt(player.transform.position);//cause boss to look at player the moment of creating the attack

            Instantiate(elementalStompOBJ, transform.position, transform.rotation);//create the stomp object

            //modify roll to cause thrust to be more likely
            abilityHighRoll += 1;
            abilityLowRoll = 0;

            //stop anims and visual while also bring the speed and rotation to normal values
            enemAI.aiPath.maxSpeed = enemAI.enemySpeed * 1.45f;
            usingAbility = false;
            modifiedRotationSpeed = rotationSpeed * 1.33f;
            leftGlow.SetActive(false);
            rightGlow.SetActive(false);
        }
    }

    public IEnumerator ElementalThrust()//thrust creates spears that lunge forward a short distance, reduces speed while casting
    {
        if(!usingAbility)
        {
            //set visual effects and anims along with changing rotation and move speed
            rightGlow.SetActive(true);
            usingAbility = true;
            enemAI.aiPath.maxSpeed = enemAI.enemySpeed * .3f * 1.45f;
            enemAI.anim.SetTrigger("Use1HFire");

            //create a angle value that allows the spears to spawn at various angles around the boss
            Vector3 thrustSpreadAngle = new Vector3(0, thrustSpread, 0);

            yield return new WaitForSeconds(thrustCastTime / (arcaneRage / 100 + 1));//runs for animiation duration/cast time

            Instantiate(elementalThrustOBJ, bulletSpawn.position, bulletSpawn.rotation);//make spear 1

            yield return new WaitForSeconds(.1f);//wait

            Instantiate(elementalThrustOBJ, bulletSpawn.position, bulletSpawn.rotation * Quaternion.Euler(thrustSpreadAngle));//make spear 2 at positive angle spread

            yield return new WaitForSeconds(.1f);//wait

            Instantiate(elementalThrustOBJ, bulletSpawn.position, bulletSpawn.rotation * Quaternion.Euler(-thrustSpreadAngle));//make spear 3 at negative angle spread

            //modify roll to cause stomp to be more likely
            abilityLowRoll -= 1;
            abilityHighRoll = 2;

            //stop anims and visual while also bring the speed and rotation to normal values
            enemAI.aiPath.maxSpeed = enemAI.enemySpeed * 1.45f;
            usingAbility = false;
            rightGlow.SetActive(false);
        }
    }

    public IEnumerator ArcaneEnclosure(int AEsAllowed)//attack that will create large areas around the arena and lock them down for a time, boss will not do other attacks and will sit in the rooms center point during this
    {
        //start hand glows
        leftGlow.SetActive(true);
        rightGlow.SetActive(true);

        //set target to room center and increase speed and rotation
        enemAI.aiPath.maxSpeed = enemAI.enemySpeed * 3;
        enemAI.aiPath.endReachedDistance = .5f;
        enemAI.aiDestination.flexTransform = centerRoomPoint;
        targetRotater = centerRoomPoint;

        yield return new WaitForSeconds(.33f);//small delay to ensure it runs properly

        yield return new WaitUntil(() => enemAI.aiPath.destinationReached == true);//wait until the boss reaches the center

        //being AE attack audio
        enclosureSource.clip = AECastSound;
        enclosureSource.loop = true;
        enclosureSource.Play();

        //lock rotation and set rotation to face south
        modifiedRotationSpeed = 0;
        gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);

        //start attack anim
        enemAI.anim.SetTrigger("StartSupport");

        yield return new WaitForSeconds(.33f);//delat timer to make sure it runs properly

        //start next attack anim
        enemAI.anim.SetBool("UsingHold", true);

        //play a laughing sound at attack start
        bossVoiceSource.clip = bossVoices[0];
        bossVoiceSource.Play();

        foreach (GameObject AEOBJ in AEOBJs)//check the list of AE objects and allow them to be used in the attack
        {
            AEOBJList.Add(AEOBJ);
        }

        while (AEsAllowed > 0)//while the allowed amount of attacks is above 0 run this 
        {
            yield return new WaitForSeconds(AECastTime);//runs for animiation duration/cast time

            int rollval;
            rollval = Random.Range(0, AEOBJList.Count + AERollModifier);//roll to find which attack is placed down, roll is changed if the outer area is already marked due to rage

            GameObject AOEOBJ = Instantiate(AEOBJList[rollval], centerRoomPoint.position, centerRoomPoint.rotation);//create the AE object based on roll value

            //update placement trackers and prevent this AE object from being placed again during this loop
            AEOBJList.RemoveAt(rollval);
            AEsAllowed -= 1;
            Destroy(AOEOBJ, 11f);
        }

        AEOBJList.Clear();//clear the list of AE objects

        yield return new WaitForSeconds(10f);//wait a while before starting regular combat

        enclosureSource.Pause();//pause the AE audio

        //reset anims, visual effects along with target, rotation and speed values
        //default the boss to ranged attack mode and enable other loops again
        enemAI.anim.SetBool("UsingHold", false);
        targetRotater = player.transform;
        enclosureActive = false;
        enemAI.aiDestination.flexTransform = enemAI.aiDestination.playerTarget;
        meleeMode = true;
        RangedMode();
        modifiedRotationSpeed = rotationSpeed;
        enemAI.aiPath.maxSpeed = enemAI.enemySpeed;
        leftGlow.SetActive(false);
        rightGlow.SetActive(false);
    }

    public IEnumerator ArcaneFlame()//flame attack will place circles of fire at random locations of the boss arena
    {
        while(true)//loop stays active when first called and will only place flames once the wait timer is done
        {
            rightGlow.SetActive(true);

            for (int i = 0; i != arcaneFlameSpawnCount; i++)//run for the total amount of flames the boss can place down
            {
                yield return new WaitForSeconds(arcaneFlameCasttime);//small delay between placement of each flame

                Instantiate(arcaneFlameOBJ, centerRoomPoint.position + new Vector3(Random.Range(-30,30), 0, Random.Range(-35, 35)), arcaneFlameOBJ.transform.rotation);//create fire object at a random arena location based off room center point
            }

            yield return new WaitForSeconds(arcaneFlameCooldown / (arcaneRage / 100 + 1));//wait until the attack can be enabled once more, modified by rage
        }
    }

    public IEnumerator ParticleOrb()//create a large dart attack, travels slower and deals higher damage
    {
        if (!usingAbility)
        {
            //enable visuals and anims along with changing speed values
            leftGlow.SetActive(true);
            usingAbility = true;
            enemAI.aiPath.maxSpeed = enemAI.enemySpeed / 2;
            enemAI.anim.SetBool("UsingBlock", true);

            Vector3 spawn = new Vector3(bulletSpawn.position.x, bulletSpawn.position.y + 3, bulletSpawn.position.z);//find the spawn location of the orb which is the same as the darts

            yield return new WaitForSeconds(pOrbCasttime / (arcaneRage / 100 + 1));//runs for animiation duration/cast time

            Instantiate(particleOrbOBJ, spawn, bulletSpawn.rotation, gameObject.transform);//create the orb

            //reset visual and anim values, set speed back to base value
            enemAI.anim.SetBool("UsingBlock", false);
            enemAI.aiPath.maxSpeed = enemAI.enemySpeed;
            usingAbility = false;
            leftGlow.SetActive(false);

            yield return new WaitForSeconds(pOrbCooldown / (arcaneRage / 100 + 1));//wait until orb can be used again

            canOrb = true;//allow orb to be cast
        }
    }

    public IEnumerator SpikedSnare()//create a + shaped area on the boss based on boss rotation, area deals damage after a short delay
    {
        if (!usingAbility)
        {
            //enable visuals and anims along with changing speed values
            rightGlow.SetActive(true);
            usingAbility = true;
            modifiedRotationSpeed = 0f;
            enemAI.aiPath.maxSpeed = 0;
            enemAI.anim.SetTrigger("Use1HFire");

            yield return new WaitForSeconds(sSnareCasttime / (arcaneRage / 100 + 1));//runs for animiation duration/cast time

            Instantiate(spikeSnareOBJ, transform.position, bulletSpawn.rotation);//create the snare

            //reset visual and anim values, set speed back to base value
            enemAI.aiPath.maxSpeed = enemAI.enemySpeed;
            usingAbility = false;
            rightGlow.SetActive(false);
            modifiedRotationSpeed = rotationSpeed * 1.33f;

            yield return new WaitForSeconds(sSnareCooldown / (arcaneRage / 100 + 1));//wait until snare can be used again

            canSnare = true;//allow snare to be cast
        }
    }

    public IEnumerator SummonAdds()//spawns elite enemies randomly at random locations of the arena
    {
        while (true)//loop stays active while the boss is alive
        {
            rightGlow.SetActive(true);
            leftGlow.SetActive(true);

            for (int i = 0; i != addCount; i++)//run for an equal amount of potential add spawns
            {
                yield return new WaitForSeconds(addCasttime);//wait a short time between each add

                //spawn a random enemy from the bosses array of enemy options, spawn that enemy at a random spot within the boss arena, rotation of that enemy is the same as flame for simplicity
                Instantiate(addCollection[Random.Range(0,addCollection.Length)], centerRoomPoint.position + new Vector3(Random.Range(-28, 28), 0, Random.Range(-30, 30)), arcaneFlameOBJ.transform.rotation);
            }

            yield return new WaitForSeconds(addCooldown / (arcaneRage / 100 + 1));//wait until the attack can be used again
        }
    }
}
