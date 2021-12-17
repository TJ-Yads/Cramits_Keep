using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControls : PlayerUpgrades
{
    //raycast values
    public RaycastHit HitPoint;
    public RaycastHit target;
    public GameObject mousePoint;
    private Vector3 mousePosition;

    //movement data values
    public bool canInteract = true;//bool that allows the player to do interactions in world such as move and attacks, only false when paused
    public float speed = 9.5f;
    public float newSpeed;
    public float moveHorizontal, moveVertical;
    public bool movingVertical, movingHorizontal;
    public bool isMoving;
    public int inputCounter;
    private Rigidbody rb;
    public Vector3 moveDirection;

    public int currentCurrency;//tracker for gems

    //combat data
    public Transform BulletSpawn;
    public Transform RayEndPoint, playerBack;//values used for dodge raycast
    public float FireRatePrim, fireRateSecond;
    public float castTime, moveSpeedReduction;
    private bool canRotate = true;
    public bool firing, meleeing, usingAbility;
    public GameObject meleeCenter;
    //variables for primary
    public float fireballMax;
    public float fireballCooldown;
    public float fireballCurrent;
    public bool fireCooling;
    public Text fireballCurrentCounter;
    public Image primaryCooldownVisual;
    public GameObject[] handGlow;//0 is right hand, 1 is left hand

    public float baseDMG;//base damage will be multiplied by a value on each instance of damage to increase or decrease attack power
    private float nextFire, nextFireSec, nextFireAbil;
    public Transform PlayerVis;
    public GameObject Bullet,melee, meleeMainOBJ;
    public Text leechText;

    //variables for tracking player abilities
    public string[] abilityName;//names of each ability the player can use, these can be changed by upgrades for diffrent abilites
    public int[] abilityNumCount;
    public bool[] abilityNumIsCooling;
    public Text[] abilityCounterText;

    //a bool check on if the ability is ready to use again, default to true for all
    public bool[] CanAbility;
    //data for UI
    public Image[] AbilityIMGs, abilityCooldownIMGs;
    public Text[] abilityTitleText, abilityEffectText, cooldownTextOBJs;
    public Text playerCurrencyCounter;
    public GameObject chargedAbilityOBJ;
    public Transform[] UIPoints;
    [Tooltip("sections work in order of currency, stats and powers and only use the right side of UI.")]
    public GameObject[] sectionsArray;//sections are shown when the player pauses and hidden during gameplay-they appear for a moment when the player grabs a respective item
    public GameObject leftSideUI;

    //variables for audio
    public AudioSource mainSource, walkingSource;
    public AudioClip[] castingAudio;

    public UpgradeUIList upgradeUIScript;
    public PlayerAbilityManager abilityManagerScript;
    public ItemManager itemManagerScript;
    public PlayerAnimationController playerAnimOBJ;

    // Start is called before the first frame update
    void Start()
    {
        //set basic values
        newSpeed = speed + moveSpeed;
        fireballCurrent = fireballMax + fireballCapacity;
        fireballCurrentCounter.text = "" + fireballCurrent;
        rb = GetComponent<Rigidbody>();
        mainSource = GetComponent<AudioSource>();
        //find ability manager
        GameObject abilityManagerOBJ = GameObject.Find("PlayerAbilityManager");
        abilityManagerScript = abilityManagerOBJ.GetComponent<PlayerAbilityManager>();

        //find item manager and upgrade uiList
        GameObject itemManagerOBJ = GameObject.Find("ItemManager");
        if(itemManagerOBJ != null)
        {
            itemManagerScript = itemManagerOBJ.GetComponent<ItemManager>();
        }
        upgradeUIScript = gameObject.GetComponentInChildren<UpgradeUIList>();
        playerAnimOBJ = gameObject.GetComponentInChildren<PlayerAnimationController>();
        StartCoroutine(GetComponentInChildren<PlayerHealth>().MortalWounds());//start permanent coroutine in player health that allows the player to lose healing by green elementals
        //set leech text for melee
        leechText.text = "" + Mathf.RoundToInt((1.15f + meleeBoosterDMGScale) * (baseDamage * baseDMG * meleeDamage) * (.1f + meleeVamp) / 1.33f + 3);
    }
    private void FixedUpdate()//hold code to allow for player movement and animation
    {
        if(canInteract)
        {
            //movement controls
            moveHorizontal = Input.GetAxisRaw("Horizontal");
            moveVertical = Input.GetAxisRaw("Vertical");
            if (moveVertical != 0 || moveHorizontal != 0)//check if the player is moving for a bool connected to the animation
            {
                isMoving = true;
                walkingSource.enabled = true;
            }
            else
            {
                isMoving = false;
                walkingSource.enabled = false;
            }
            //rotation values taken in are used to determine if the animation data has to be flipped negative, allows the animation to play properly when the player is rotated
            if (transform.eulerAngles.y > 58 && transform.eulerAngles.y < 122 || transform.eulerAngles.y > 238 && transform.eulerAngles.y < 302)//check rotation values
            {
                moveDirection = new Vector3(-moveHorizontal, 0, -moveVertical);//invert numbers if rotated to a certain angle
            }
            else
            {
                moveDirection = new Vector3(moveHorizontal, 0, moveVertical);
            }
            if (moveDirection.magnitude > 1f)//check the move direction values and normalize them if they reach above 1
            {
                moveDirection = moveDirection.normalized;
            }
            moveDirection = transform.TransformDirection(moveDirection);//set the value from local to world space

            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;//create movement value and normalize it
            rb.velocity = (movement * newSpeed);//move player in direction
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (canInteract)
        {
            //find teleport point used for the dodge ability
            Vector3 teleportPoint = new Vector3(moveHorizontal + .01f, 0.0f, moveVertical + .01f);
            playerBack.rotation = Quaternion.LookRotation(teleportPoint);

            //raycast targeter
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out target);
            //find and set mouse position, use this to move a target object around- this object allows the camera to move based on mouse position
            mousePosition = (target.point);
            mousePosition = new Vector3(mousePosition.x, 0, mousePosition.z);
            //find and track distance values of the mouses X and Z relative to player
            float disx = mousePosition.x - transform.position.x;
            float disz = mousePosition.z - transform.position.z;
            disz = Mathf.Abs(disz);
            disx = Mathf.Abs(disx);
            if(disx > 12f || disz > 7f)//minimum distance needed for the mouse object to move away from the player, allows the camera to move away only when the mouse is far enough
            {
                mousePoint.transform.position = new Vector3(mousePosition.x, 0, mousePosition.z);
            }
            else
            {
                mousePoint.transform.position = transform.position;//reset the mouse object to player position
            }

            //look target will equal the postion of the raycast x and z values and the players y value, this allows bullets to fire towards a target without any bullet drop
            Vector3 LookTarget = new Vector3(target.point.x, transform.position.y, target.point.z);
            if(canRotate)
            {
                gameObject.transform.LookAt(LookTarget);
            }

            //primary-ranged
            if (Input.GetButton("Primary") && Time.time > nextFire && !meleeing && !usingAbility && !firing)
            {
                firing = true;
                StartCoroutine(Fire());
            }
            if (Input.GetButtonUp("Primary"))
            {
                firing = false;
            }
            //secondary-melee
            if (Input.GetButton("Secondary") && Time.time > nextFireSec && !firing && !usingAbility)
            {
                meleeing = true;
                StartCoroutine(Melee());
            }
            if (Input.GetButtonUp("Secondary"))
            {
                meleeing = false;
            }
            //ability 1
            if (Input.GetButton("Ability1") && CanAbility[0] && Time.time > nextFireAbil && !usingAbility)
            {
                usingAbility = true;
                Ability(0);
            }
            //ability 2
            if (Input.GetButton("Ability2") && CanAbility[1] && Time.time > nextFireAbil && !usingAbility)
            {
                usingAbility = true;
                Ability(1);
            }
            //ability 3
            if (Input.GetButton("Ability3") && CanAbility[2] && Time.time > nextFireAbil && !usingAbility)
            {
                usingAbility = true;
                Ability(2);
            }
            if(Input.GetButtonUp("Ability1") || Input.GetButtonUp("Ability2") || Input.GetButtonUp("Ability3"))
            {
                usingAbility = false;
            }
        }
        else
        {
            //when not able to interact, lock values and set then to 0
            Vector3 movement = new Vector3(0.0f, 0.0f, 0.0f);
            rb.velocity = movement;
            moveDirection = new Vector3(0, 0, 0);
            isMoving = false;
            firing = false;
            walkingSource.enabled = false;
        }
    }

    //after an ability is used it enters cooldown using this coroutine, once the cooldown ends the player can use the ability again
    //coroutine runs until the ability hits its max charge amount
    //loops allow the icon to charge up over time and create a pulse effect when charge
    public IEnumerator AbilityCooling(int AbilityNum, float Cooldown, Image AbilIcon)
    {
        float CurrentTime;
        while(abilityNumCount[AbilityNum] < clarityTotal)//run until the ability counter is at the clarity total
        {
            //reset fill values
            CurrentTime = 0;
            AbilIcon.fillAmount = 0;
            while (CurrentTime < Cooldown)//loop the cooldown using time and end of frame waiting
            {
                yield return new WaitForEndOfFrame();
                AbilIcon.fillAmount = CurrentTime / Cooldown;
                CurrentTime += Time.deltaTime;
            }
            //update the icon and text and create a charged pulse object
            abilityNumCount[AbilityNum]++;
            abilityCounterText[AbilityNum].text = "" + abilityNumCount[AbilityNum];
            Instantiate(chargedAbilityOBJ, UIPoints[AbilityNum]);
            CanAbility[AbilityNum] = true;
            if(abilityNumCount[AbilityNum] > clarityTotal)
            {
                abilityNumCount[AbilityNum] = clarityTotal;
                abilityCounterText[AbilityNum].text = "" + abilityNumCount[AbilityNum];
            }
            yield return new WaitForEndOfFrame();
        }
        abilityNumIsCooling[AbilityNum] = false;
    }
    //loop used for primary fire, runs when the trigger is held down and checks for fireball counter
    //loop plays an anim and creates a delay for next fire along with modifying speed
    public IEnumerator Fire()
    {
        //reset base values
        bool failFireAudio = false;
        int counter = 0;
        while (firing)
        {
            yield return new WaitForSeconds(.01f);//delay to allow the while loop to run without error
            if(fireballCurrent > 0)
            {

                //animation activators
                playerAnimOBJ.animatorOBJ.SetTrigger("usingPrimary");
                playerAnimOBJ.animatorOBJ.SetFloat("attackAnimSpeed", attackSpeed);
                handGlow[0].SetActive(true);

                float FireDelay = 60f / (FireRatePrim * attackSpeed);//fire delay used for looping attack
                nextFire = Time.time + FireDelay * 1.75f;//next fire delay for the button down function to prevent spam attacks
                newSpeed = (speed + moveSpeed) * moveSpeedReduction;//reduce player move speed during the attack
                yield return new WaitForSeconds(castTime / attackSpeed);//wait for the cast time before attacking
                //cause bullet spawn to look at the mouse
                Vector3 bulletLookTarget = new Vector3(target.point.x, transform.position.y + 1.5f, target.point.z);
                BulletSpawn.LookAt(bulletLookTarget);
                //spawn the bullet and add it to a index for removal
                GameObject BulletOBJs = Instantiate(Bullet, BulletSpawn.position, BulletSpawn.rotation);
                Destroy(BulletOBJs, 10f);

                handGlow[0].SetActive(false);
                //reduce the fireball counter and update text
                fireballCurrent -= 1;
                fireballCurrentCounter.text = "" + fireballCurrent;
                if (fireCooling == false)//start the cooling coroutine if it isnt started
                {
                    StartCoroutine(FireballCooldown());
                }
                if (firing)
                {
                    yield return new WaitForSeconds(FireDelay);
                }
            }
            else
            {
                if(!failFireAudio)
                {//play special audio if the player tries to fire but is out of ammo
                    failFireAudio = true;
                    mainSource.clip = castingAudio[2];
                    mainSource.pitch = 1.4f;
                    mainSource.volume = .65f;
                    mainSource.Play();
                }
                counter++;
                if (counter > 45)
                {
                    counter = 0;
                    failFireAudio = false;
                }
            }
        }
        ReturnSpeed();//return original speed if the player stopped firing
    }

    //cooldown for fireballs, identical to ability coroutine in terms of function
    public IEnumerator FireballCooldown()
    {
        fireCooling = true;
        float CurrentTime;
        while (fireballCurrent < fireballMax + fireballCapacity)
        {
            CurrentTime = 0;
            primaryCooldownVisual.fillAmount = 0;
            while (CurrentTime < fireballCooldown / cooldownReduction)
            {
                yield return new WaitForEndOfFrame();
                primaryCooldownVisual.fillAmount = CurrentTime / (fireballCooldown / cooldownReduction);
                CurrentTime += Time.deltaTime;
            }
            fireballCurrent += 1;
            fireballCurrentCounter.text = "" + fireballCurrent;
            yield return new WaitForEndOfFrame();
        }
        fireCooling = false;
    }
    public IEnumerator Melee()//set melee attack active, this melee rotates around the player about 90 degrees from his right- counterclockwise
    {
        //prep and play melee sound
        mainSource.clip = castingAudio[1];
        mainSource.pitch = .4f;
        mainSource.volume = .3f;
        mainSource.Play();

        meleeCenter.transform.localRotation = Quaternion.Euler(0, 65, 0);//reset melee positon 
        //play animation
        playerAnimOBJ.animatorOBJ.SetFloat("attackAnimSpeed", attackSpeed);
        playerAnimOBJ.animatorOBJ.SetTrigger("UsingMelee");
        handGlow[0].SetActive(true);

        yield return new WaitForSeconds(castTime * .6f / attackSpeed);//wait for the cast time before attacking
        canRotate = false;//lock rotation during the attack
        float FireDelay = 60f / (fireRateSecond * attackSpeed);
        nextFireSec = Time.time + FireDelay;
        melee.SetActive(true);//set attack active allowing damage and object rotation
        handGlow[0].SetActive(false);
        yield return new WaitForSeconds(.3f);
        canRotate = true;
        melee.SetActive(false);
        meleeing = false;
    }
    //using the ability buttons will run this if the player has ability charges
    //calls the ability manager script to run a certain ability based on the ability number and ability string
    public void Ability(int abilNum)
    {
        Vector3 bulletLookTarget = new Vector3(target.point.x, transform.position.y + 1.5f, target.point.z);
        BulletSpawn.LookAt(bulletLookTarget);
        abilityManagerScript.ActivateAbility(abilityName[abilNum], abilNum);
        foreach (string power in activeAbilityEffects)//for any power the player owns that is a ability power, run it on ability usage
        {
            abilityManagerScript.ActivateAbilityPower(power);
        }
        nextFireAbil = Time.time + .6f / attackSpeed;//short delay between abilities
    }
    public IEnumerator ShownSections(int sectionTargetVal)
    {//activates on various actions such as gain gems or stat items, shows that specific UI section for a few seconds before hiding again, pausing the game shows all of them until the player exits pause
        sectionsArray[sectionTargetVal].SetActive(true);
        yield return new WaitForSeconds(2.3f);
        sectionsArray[sectionTargetVal].SetActive(false);
    }
    public void ReturnSpeed()//used when speed values are reduced for any reason, once the reduced speed effect is done this will bring back the base value including boosts from AGL
    {
        newSpeed = speed + moveSpeed;
    }
}
