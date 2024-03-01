using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

//This script manages the player base, and it's turrets and their behaviour.

public class PlayerManager : MonoBehaviour
{
    public GameObject scriptManagerObject;

    public GameObject blueLevelOne;
    public GameObject blueLevelTwo;
    public GameObject redLevelOne;
    public GameObject redLevelTwo;
    public GameObject redLevelThree;

    private GameObject[] blueDefences;
    private GameObject[] redDefences;

    private EnemyManager enemyManager;
    private WorldGeneration worldGeneration;
    private Building building;

    private List<EnemyManager.Enemy> enemyList;
    private GameObject[,] gameObjects;
    private GameObject temporaryTarget; //use the gameobject the cursor is hovering on later
    private GameObject lastHitObject; 
    private List<Defence> defenceList = new List<Defence>();

    //i need some way to identify which tower is being placed, i could do it with numbers but this is more clear

    enum Types
    {
        BLUE,
        RED
    }
    enum DefenceLevels
    {
        BLUE_ONE,
        BLUE_TWO,
        RED_ONE,
        RED_TWO,
        RED_THREE,
    }

    class Defence
    {
        private int currentLevel = 0;
        private int damageAmount = 20;
        private List<EnemyManager.Enemy> enemyList;
        public int health = 100;
        public float lastShotTime = Time.time;
        public GameObject gameObject;

        public Defence(GameObject type, GameObject spawnObject, List<EnemyManager.Enemy> enemList)
        {
            gameObject = type;
            gameObject.transform.position = new Vector3(spawnObject.transform.position.x + 0.5f,
                                                        spawnObject.transform.position.y + 1.0f,
                                                        spawnObject.transform.position.z + 0.5f);
            enemyList = enemList;
        }
        public void Upgrade()
        {
        
        }

        //Function that returns nearest enemy, return type EnemyManager.Enemy, inputs enemyList 
        public EnemyManager.Enemy ReturnNearestEnemy()
        {
            EnemyManager.Enemy closestEnemy = new EnemyManager.Enemy(); //Temporary blank
            float distance = Mathf.Infinity;

            foreach (EnemyManager.Enemy enemy in enemyList)
            {
                float newDistance = Vector3.Distance(gameObject.transform.position, enemy.obj.transform.position);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    closestEnemy = enemy;

                }
            }
            return closestEnemy;
        }

        public void TrackEnemy()
        {
            EnemyManager.Enemy target = ReturnNearestEnemy();
            Vector3 pos = target.obj.transform.position;
            gameObject.transform.LookAt(new Vector3(pos.x, gameObject.transform.position.y, pos.z));
        }

        public void ShootEnemy()
        {
            //Find nearest enemy, shoot, and set last shot time to now
            //Implement a visibilty check
            if (Time.time - lastShotTime > 2)
            {
                EnemyManager.Enemy target = ReturnNearestEnemy();
                target.health -= damageAmount;
                lastShotTime = Time.time;
                Debug.Log("shot enemy");
            } 
        }
    }


    void SpawnDefence(GameObject targetObject, DefenceLevels defenceType, int amount) //should I just make this a part of the defence class?
    {
        lastHitObject = building.lastHitObject; //get the clicked on block
        for (int i = 0; i < amount; i++)
        {
            GameObject chosenType = gameObject;

            switch (defenceType)
            {
                case DefenceLevels.BLUE_ONE:
                    chosenType = blueLevelOne;
                    break;
                case DefenceLevels.BLUE_TWO:
                    chosenType = blueLevelTwo;
                    break;
            }

            Defence newDefence = new Defence(chosenType, targetObject, enemyList);
            newDefence.gameObject = Instantiate(chosenType, newDefence.gameObject.transform.position, Quaternion.identity);
            newDefence.gameObject.transform.localScale = new Vector3(0.40f, 0.40f, 0.40f);
            
            defenceList.Add(newDefence);
        }
    }

    void MainDefenceLoop()
    {
        if (enemyList.Count > 0)
        {
            foreach (Defence defence in defenceList)
            {
                defence.ShootEnemy();
                defence.TrackEnemy();
            }
        }
    }

    private void Start()
    {
        enemyManager = scriptManagerObject.GetComponent<EnemyManager>();
        worldGeneration = scriptManagerObject.GetComponent<WorldGeneration>();

        //get variables from other scripts
        enemyList = enemyManager.enemyList;
        gameObjects = worldGeneration.gameObjects;

        building = scriptManagerObject.GetComponent<Building>();
        lastHitObject = building.lastHitObject;

        temporaryTarget = gameObjects[Random.Range(2, 20), 10]; //temporary target while i implement the object being hovered over
        //SpawnDefence(temporaryTarget, DefenceLevels.BLUE_ONE, 2);
    }

    private void Update()
    {
        MainDefenceLoop();
        

    }
}
