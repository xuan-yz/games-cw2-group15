using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Room_spawn roomSpawn;
    public Enemies_Spawn enemySpawn;
    public bool start = false;
    void Start()
    {
        start = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            roomSpawn.Start_spawn=true;
            enemySpawn.Spawn_Enemies = true;
            start = false;
        }
    }

}
