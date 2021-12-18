using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//this script is used to allow rooms to spawn enemies, create upgrades and allow progression to the next room
//it also tracks enemies in the room and can remove them from a list when killed
//the script makes use of enemySpawnSystem to assist with the spawning and managment of enemies
//an added part of the script manages the spawning of the boss and some boss interactions

public class RoomMechanic : EnemySpawnSystem
{
    //derived class EnemySpawnSystem holds values such as enemies and spawn chance, room cost and maximums and the function to proccess the cost of enemies

    //room logic
    public bool finishedRoom = false;
    public GameObject roomDoneUI;
    public bool isBossRoom;
    private bool playerEntered;
    private GameObject player;
    private AudioSource unlockDoorSource;

    //Items/pointers
    public GameObject upgradeOBJ;
    public Transform UpgradeSpawn;
    public string upgradeType;
    public GameObject upgradeSceneItem;
    public GameObject lootPointer;
    public GameObject lootPointerInScene;
    public GameObject enemyPointer;

    //Transforms
    public GameObject entrance;
    public List<GameObject> exits = new List<GameObject>();
    public List<GameObject> doors = new List<GameObject>();

    //Enemy Logic
    public List<GameObject> EnemyList = new List<GameObject>();
    public List<Transform> spawnPoints = new List<Transform>();
    private bool EnemiesSpawned;

    //boss data
    public GameObject bossEnemy;
    public Transform roomCenterPoint;
    public GameObject blockWallOBJ;
    public GameObject bossDropBlocks;
    public AudioClip[] bossMusic;
    
    private void OnTriggerEnter(Collider other)//enter method is used for refrence data and most run on startup or upon entering the room
    {

        if (other.CompareTag("Enemy"))//if the enemy enters then add it to the list for tracking, use health as part of tracking method
        {
            GameObject EnemyHit = other.gameObject;
            EnemyHealth HitEffect = other.GetComponent<EnemyHealth>();
            HitEffect.RoomAttached = GetComponent<RoomMechanic>();
            EnemyList.Add(EnemyHit);
        }

        if(other.CompareTag("SpawnPoint"))//for each spawn point/gizmo in the room add it to the list of potential spawn areas
        {
            spawnPoints.Add(other.gameObject.transform);
        }

        if (other.CompareTag("Entrance"))//set entrance object for later, used to place the player when they enter
        {
            entrance = other.gameObject; 
        }

         if (other.CompareTag("Exit"))//add exits to the list for leaving a room and setting the upgrade options
        {
            GameObject exit = other.gameObject;
            exits.Add(exit);
        }

         if (other.CompareTag("Door"))//add doors to the list for the FX of clearing a room
        {
            GameObject door = other.gameObject;
            doors.Add(door);
        }

        if (other.CompareTag("Player") && !playerEntered)//run once upon the player entering a room
        {
            maxEnemiesInRoom = 15 + gameStateScript.roomsCleared * 3;//determine the max amount of enemies allowed at once
            if (maxEnemiesInRoom > 45)
            {
                maxEnemiesInRoom = 45;//limit this to a hard cap of 45
            }

            playerEntered = true;//lockout to prevent rerunning the script

            player = other.gameObject;

            if (EnemiesSpawned == false && isBossRoom == false)//spawns enemies once
            {
                StartCoroutine(SpawnEnemies());//start the spawning of enemies
            }

            if(isBossRoom)//spawns the boss once
            {
                StartCoroutine(SpawnBoss());//spawn the boss instead
                blockWallOBJ.SetActive(true);
            }

            StartCoroutine(SetExitUpgrades());//runs for each door to set what upgrades the offer
        }
    }

    private void OnTriggerExit(Collider other)//used only when the player exits a room to hide some UI and destroy a loot pointer
    {
        if (other.CompareTag("Player"))//upon leaving remove the UI and lootpointer OBJ
        {
            roomDoneUI.SetActive(false);
            if(lootPointerInScene != null)
            {
                Destroy(lootPointerInScene);
            }
        }
    }

    //spawn coroutine will keep spawning enemies until the rooms max for total power or total cluster power has reached its limit
    //this script uses the derived EnemySpawnSystem script to function
    IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(1f);
        int testNum;//declare testnum variable
        int failedCounter = 0;//safety counter that allows the spawn limits of a cluster to be changed if the counter reaces a limit

        for (int i = 0; i <= spawnDataArray.Length - 1; i++)//find the cluster size of each enemy in the list, this cluster limits how many of each type can spawn
        {
            DetermineCluster(i);
        }

        for (int i = 0; i < maxRoomCost;)//runs until I is more then maxRoomCost
        {
            testNum = Random.Range(1, spawnChanceTotal + 1);//roll the number based on spawnChanceTotal
            for (int w = 0; w < spawnDataArray.Length; w++)//search the array for enemies and spawn chances
            {
                if (testNum >= spawnDataArray[w].lowSpawnChance && testNum <= spawnDataArray[w].highSpawnChance)//spawn the enemy that has the low and high range within the randomroll
                {
                    if(groupingCount[w] > 0 || failedCounter < 5)//allow spawning if the group/cluster count is above 0 or if the loop failed 5 times in a row
                    {
                        int range = Random.Range(0, spawnPoints.Count);//find the random location to spawn
                        Vector3 spawnRange = new Vector3(Random.Range(spawnPoints[range].transform.localScale.x / 2 * -1, spawnPoints[range].transform.localScale.x / 2), 0, Random.Range(spawnPoints[range].transform.localScale.z / 2 * -1, spawnPoints[range].transform.localScale.z / 2));//find a spot within the random location to spawn, area is decided by gizmo size
                        Instantiate(spawnDataArray[w].enemyOBJ, spawnPoints[range].transform.position + spawnRange, spawnDataArray[w].enemyOBJ.transform.rotation);//spawn the enemy
                        currentEnemiesInRoom++;//update the room tracker for enemies alive
                        groupingCount[w] -= 1;//update cluster tracker for that enemy
                        i += enemySpawnCosts[w];//update i with the cost of the enemy. allows loop to end if i is at the limit
                        TotalEnemyCount -= 1;//update enemy total potential counter
                        failedCounter = 0;//reset fail counter
                    }
                    else
                    {
                        failedCounter++;//if the enemy did not spawn increase fail counter
                    }
                }
                if (TotalEnemyCount <= 0)
                {
                    maxRoomCost = 0;
                }
                yield return new WaitUntil(() => currentEnemiesInRoom < maxEnemiesInRoom);
            }
            yield return new WaitForEndOfFrame();
        }
        EnemiesSpawned = true;
    }

    IEnumerator SpawnBoss()//if the room allows for a boss then spawn a boss at the center
    {
        //change the games music to the boss theme
        GameObject gameControllerOBJ = GameObject.FindWithTag("MainSceneController");
        AudioSource musicSource = gameControllerOBJ.GetComponentInChildren<AudioSource>();
        musicSource.clip = bossMusic[0];
        musicSource.Play();
        musicSource.volume = .25f;

        Instantiate(bossEnemy, spawnPoints[0].position, spawnPoints[0].transform.rotation);
        yield return new WaitForSeconds(2f);
        EnemiesSpawned = true;
    }

    public void DeleteEnemy(GameObject Enemy)//removes enemies from the list when killed if they all die and spawning is done the player gains an item
    {
        //update the various trackers on enemies
        currentEnemiesInRoom--;
        EnemyList.Remove(Enemy);
        gameStateScript.killCounter += 1;

        if(EnemyList.Count == 3 && EnemiesSpawned && !isBossRoom)//if there is only 3 enemies left and the room is done spawning then create a enemy tracker 
        {
            Instantiate(enemyPointer, player.transform.position, player.transform.rotation);
        }

        if (EnemyList.Count <= 0 && EnemiesSpawned)//run if the last enemy was killed
        {
            //run unlocking audio
            unlockDoorSource = GetComponent<AudioSource>();
            unlockDoorSource.volume = .9f;
            unlockDoorSource.Play();

            foreach (GameObject exit in exits)//show upgrades on each door
            {
                ExitDoor doorScript = exit.GetComponent<ExitDoor>();
                doorScript.RevealUpgrades();
            }

            upgradeSceneItem = Instantiate(upgradeOBJ, UpgradeSpawn.position, UpgradeSpawn.rotation);//create the upgrade for this room

            //set various activations allowing the room to be exited
            finishedRoom = true;
            UnlockDoor();
            roomDoneUI.SetActive(true);

            if(isBossRoom)//if the room was a boss room then create an additional Power based upgrade
            {
                //change the music to an alternate version of the game music
                GameObject gameControllerOBJ = GameObject.FindWithTag("MainSceneController");
                AudioSource musicSource = gameControllerOBJ.GetComponentInChildren<AudioSource>();
                musicSource.clip = bossMusic[1];
                musicSource.Play();
                musicSource.volume = .35f;

                //spawn a random bonus item from the power loot pool
                GameObject tempManagerOBJ = GameObject.FindWithTag("GameController");
                ItemManager tempManager = tempManagerOBJ.GetComponentInChildren<ItemManager>();
                upgradeOBJ = tempManager.availablePowerUpgrades[Random.Range(0, tempManager.availablePowerUpgrades.Count)];
                Vector3 moveAway = new Vector3(5, 0, 0);
                Instantiate(upgradeOBJ, UpgradeSpawn.position + moveAway, UpgradeSpawn.rotation);
            }
            
            lootPointerInScene =  Instantiate(lootPointer, player.transform.position, player.transform.rotation);//spawn the loot pointer to show where the upgrade is
        }
    }

    public IEnumerator SetExitUpgrades()//runs on each door and uses the ExitDoor script to allow them to determine what upgrade they offer
    {
        yield return new WaitForSeconds(1f);
        //foreach loop will cause the exits to roll for an upgrade when the player enters the room, takes a string for upgrade type- none means a stat upgrade
        foreach (GameObject exit in exits)
        {
            ExitDoor doorScript = exit.GetComponent<ExitDoor>();
            doorScript.RollUpgrades(upgradeType);
        }
    }

    private void UnlockDoor()//runs for each door and changes the FX the show to change from the locked state to unlocked
    {
        foreach(GameObject door in doors)//sets the FX of doors to the unlocked state
        {
            LockDoor doorScript = door.GetComponent<LockDoor>();
            doorScript.unLockedDoor.SetActive(true);
            doorScript.lockedDoor.SetActive(false);
        }
    }
}
