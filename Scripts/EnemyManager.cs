using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject scriptManagerObject;
    private WorldGeneration worldGenScript;

    private static int mapSize;
    private GameObject[,] gameObjects;
    public List<Enemy> enemyList = new List<Enemy>();

    public Enemy[] enemyArray = new Enemy[99];

    public class Enemy
    {
        public Vector2Int pos;
        public GameObject obj;
        public float health = 100.0f;

        public void colorOnHealth()
        {
            Color newColor = new Color(0.01f * (100 - health), 0.01f * health, 0, 1);
            obj.GetComponent<Renderer>().material.SetColor("_Color", newColor); //Color it less green, more red the lower the hp
        }

        public void moveEnemy()
        {

        }
    }


    void SpawnEnemies(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Enemy newEnemy = new Enemy();
            newEnemy.pos = new Vector2Int(Random.Range(0, mapSize - 1), Random.Range(0, mapSize - 1));
            GameObject mapCube = gameObjects[newEnemy.pos.x, newEnemy.pos.y];  //maybe i should put this into the class definition

            newEnemy.obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newEnemy.obj.transform.position = new Vector3(mapCube.transform.position.x + 0.5f,
                                                              mapCube.transform.position.y + 1.5f,
                                                              mapCube.transform.position.z + 0.5f); //this places the enemy above a map cube, i need the 0.5f due to the way i calculated the vertices on my cubes which is differnet from unity's
            enemyList.Add(newEnemy);
            
            newEnemy.obj.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            Debug.Log("enemy spawned");
        }
    }

    void CheckEnemyDeath(Enemy enemy)
    {
        if (enemy.health <= 0)
        {
            Destroy(enemy.obj);
            enemyList.Remove(enemy);
        }
    }

    void MainEnemyLoop()
    {
        if (enemyList.Count != 0)
        {
            foreach (Enemy currentEnemy in enemyList.ToList()) //toList seems a bit dangerous but
            {
                //All enemy functions go here, run per frame
                //currentEnemy.colorOnHealth();
                CheckEnemyDeath(currentEnemy);
            }
        }
        else
        {
            Debug.Log("none in list");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //get vars from the WorldGeneration script
        worldGenScript = scriptManagerObject.GetComponent<WorldGeneration>();
        gameObjects = worldGenScript.gameObjects;
        mapSize = gameObjects.GetLength(0); //normal mapSize doesn't work, use getLength for mapSize

        //create test enemy
        SpawnEnemies(5);

        Enemy enemy_test = new Enemy();
    }

    // Update is called once per frame

    void Update()
    {
        MainEnemyLoop();

        if (Input.GetMouseButtonDown(1))
        {
            SpawnEnemies(3);
        }
    }
}
