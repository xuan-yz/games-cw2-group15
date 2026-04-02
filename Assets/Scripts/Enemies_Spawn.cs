using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class Enemies_Spawn : MonoBehaviour
{
    public int maxEnemies = 5; 
    public int minEnemies = 1;
    public List<GameObject> NormalRooms = new List<GameObject>();
    public GameObject SpawnPoint;
    public GameObject BossRoom;
    public GameObject PlayerPrefab;
    public GameObject BossPrefab;
    public List<GameObject> EnemyPrefabs = new List<GameObject>();
    public bool Spawn_Enemies = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Spawn_Enemies = false;
    }
    void Update()
    {
        if (Spawn_Enemies)
        {
            StartCoroutine(StartSpawn());
            Spawn_Enemies = false;
        }
    }
    
    System.Collections.IEnumerator StartSpawn()
    {
        CleanAllEnemies();
        yield return new WaitForSeconds(1f);

        PlayerPrefab = Resources.Load<GameObject>("Player");
        BossPrefab = Resources.Load<GameObject>("Boss");
        NormalRooms = new List<GameObject>(GameObject.FindGameObjectsWithTag("Room"));
        foreach (GameObject room in NormalRooms)
        {
            Bounds roomBound = GetRoomBounds(room);
            int NumberOfEnemies = Random.Range(minEnemies, maxEnemies + 1);

            for (int i = 0; i < NumberOfEnemies; i++)
            {
                float RandomX = Random.Range(roomBound.min.x+2f, roomBound.max.x-2f);
                float RandomZ = Random.Range(roomBound.min.z+2f, roomBound.max.z-2f);
                Vector3 Position = new Vector3(RandomX, 1f, RandomZ);
                SpawnEnemy(Position);
            }
        }

        SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        Bounds spawnBound = GetRoomBounds(SpawnPoint);
        SpawnPlayer(spawnBound.center); 

        BossRoom = GameObject.FindGameObjectWithTag("BossRoom");
        Bounds bossBound = GetRoomBounds(BossRoom);
        SpawnBoss(bossBound.center);
    }

    void CleanAllEnemies()
    {
        NormalRooms.Clear();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
    }

    Bounds GetRoomBounds(GameObject room)
    {
        Renderer[] renderers = room.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(room.transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }
    Bounds GetIntersectionBounds(Bounds a, Bounds b)
    {   
        float minX = Mathf.Max(a.min.x, b.min.x);
        float maxX = Mathf.Min(a.max.x, b.max.x);
        float minY = Mathf.Max(a.min.y, b.min.y);
        float maxY = Mathf.Min(a.max.y, b.max.y);
        float minZ = Mathf.Max(a.min.z, b.min.z);
        float maxZ = Mathf.Min(a.max.z, b.max.z);

        if (minX > maxX || minY > maxY || minZ > maxZ)
            return new Bounds();

        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, (minZ + maxZ) / 2f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
        
        return new Bounds(center, size);
    }
    void SpawnEnemy(Vector3 Position)
    {
        string path = "Assets\\EnemyAIs";
        string fullPath = Path.Combine(Application.dataPath, "EnemyAIs");
        string[] blenderFiles = Directory.GetFiles(fullPath, "*.prefab", SearchOption.TopDirectoryOnly);
        if (blenderFiles.Length == 0)
        {
            Debug.LogError("No prefab files found in " + path);
            return;
        }
        else
        {
            string randomPrefabPath = blenderFiles[Random.Range(0, blenderFiles.Length)];
            string assetPath = "Assets/EnemyAIs/" + Path.GetFileName(randomPrefabPath);
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (enemyPrefab != null)
            {
                GameObject enemy = Instantiate(enemyPrefab, Position, Quaternion.identity);
                enemy.tag = "Enemy";
                NavMeshSurface surface = FindObjectOfType<NavMeshSurface>();
            }
            else
            {
                Debug.LogWarning("Failed to load prefab at " + assetPath);
            }
            EnemyPrefabs.Add(enemyPrefab);
        }
    }
    void SpawnPlayer(Vector3 Position)
    {

    }
    void SpawnBoss(Vector3 Position)
    {

    }
}
