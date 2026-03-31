using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class Room_spawn : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private string folderPath =  "Scenes";
        public int maxX =250;
        public int maxZ =250;
        public int minX = -250;
        public int minZ = -250;
        public int attempts_spawn = 50;
        public float least_distance=70f;
        public float max_distance=150f;
        public bool Start_spawn = false;
        public float Road_length = 10f;
        private float Road_width = 0.1f;
        public float wall_height = 10f;
        public List<GameObject> spawned_prefabs = new List<GameObject>();
        public List<Bounds> road_bounds = new List<Bounds>();
        public List<Bounds> room_bounds = new List<Bounds>();
        public List<GameObject> turning_points = new List<GameObject>();
    void Start()
    {
        Start_spawn = false;
    }
// Update is called once per frame
    void Update()
    {
        if (Start_spawn){
            StartCoroutine(SpawnRoutine());
            Start_spawn = false;
        }
    }
    System.Collections.IEnumerator SpawnRoutine()
    {
        Clean_all();
        yield return null;
        Generate_spawn();
        Generate_room();
        Generate_boss_room();
        Generate_road();

        GenerateRoom_wall(road_bounds, room_bounds);
        List<GameObject> turning_points = GameObject.FindGameObjectsWithTag("Turrning").ToList();
        foreach (GameObject turning in turning_points){
            Bounds turning_bound = turning.GetComponent<Renderer>().bounds;
            int count = 0;
            foreach(Bounds roomBound in room_bounds)
            {
                if (IsCovered(turning_bound, roomBound))
                {
                    count++;
                    break;
                }
            }
            if(count==0)
                Generatewall_turning(turning.transform.position, road_bounds);
        }
        CreateCorridorWall();

        place_navigation();
    }
    
    void Clean_all()
    {
        spawned_prefabs.Clear();
        road_bounds.Clear();
        room_bounds.Clear();
        turning_points.Clear();
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if(obj.name.EndsWith("(Clone)")||obj.name.EndsWith("(Road)")||obj.name.EndsWith("(Wall)")||obj.name.EndsWith("(teleport)"))
            {
                Destroy(obj);
            }
        }
    }
    struct Edge
    {
        public int from, to;
        public float distance;
        public Edge(int from, int to, float dist)
        {
            this.from = from;
            this.to = to;
            this.distance = dist;
        }
    }
    void Generate_road()
    {
        if (spawned_prefabs.Count < 2) return;

        List<Vector3> positions = new List<Vector3>();
        foreach (var prefab in spawned_prefabs)
        {
            positions.Add(prefab.transform.position);
        }
        List<Edge> edges = new List<Edge>();
        for(int i=0; i<positions.Count;i++)
        {
            for(int j=i+1; j < positions.Count; j++)
            {
                Vector3 start = positions[i];
                Vector3 end = positions[j];
                float distance = Mathf.Abs(start.x - end.x) + Mathf.Abs(start.z - end.z);
                edges.Add(new Edge(i, j, distance));
            }
        }

        edges.Sort((a, b) => a.distance.CompareTo(b.distance));

        int[] parent = new int[positions.Count];
        for (int i = 0; i < parent.Length; i++)
        {
            parent[i] = i;
        }

        List<Edge> mst = new List<Edge>();
        foreach (Edge edge in edges)
        {
            int rootS = FindRoot(parent, edge.from);
            int rootE = FindRoot(parent, edge.to);
            if (rootS != rootE)
            {
                mst.Add(edge);
                parent[rootS] = rootE;
            }
        }

        HashSet<(int, int)> createdSegments = new HashSet<(int, int)>();
        foreach(Edge edge in mst)
        {
            Vector3 start = positions[edge.from];
            Vector3 end = positions[edge.to];

            Vector3 corner = new Vector3(start.x, 0, end.z);
            GameObject turning= GameObject.CreatePrimitive(PrimitiveType.Cube);
            turning.name = "turning(Road)";
            turning.transform.position = corner;
            turning.transform.localScale = new Vector3(Road_length, Road_width, Road_length);
            turning.tag = "Turrning";
            turning_points.Add(turning);
            GameObject road1 = CreateRoad(start, corner, createdSegments);
            GameObject road2 = CreateRoad(end, corner , createdSegments);

        
        }
    }

    GameObject CreateRoad(Vector3 start, Vector3 end, HashSet<(int, int)> createdSegments)
    {
        bool isXSegment = Mathf.Approximately(start.z, end.z);
        bool isZSegment = Mathf.Approximately(start.x, end.x);
        Vector3 newStart = start;
        float length = Vector3.Distance(newStart, end);
        
        Vector3 direction = (end - newStart).normalized;

        int hashKey = GetSegmentHash(newStart, end);
        if (createdSegments.Contains((hashKey, hashKey))) return null; 
        GameObject roadPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roadPiece.name = "Corridor(Road)";
        roadPiece.transform.position = newStart + direction * (length / 2f);
        if (isXSegment)
        {
            roadPiece.transform.localScale = new Vector3(length- Road_length, Road_width, Road_length);
            road_bounds.Add(roadPiece.GetComponent<Renderer>().bounds);
        }
        else
        {
            roadPiece.transform.localScale = new Vector3(Road_length, Road_width, length- Road_length);
            road_bounds.Add(roadPiece.GetComponent<Renderer>().bounds);
        }


        createdSegments.Add((hashKey, hashKey));
        return roadPiece;

    }
    bool IsCovered(Bounds road, Bounds room)
    {
        if (room.Contains(road.min) && room.Contains(road.max))
        {
            return true;
        }
        return false;
    }    
    void Generatewall_turning(Vector3 corner, List<Bounds> roadBounds)
    {
        List<Vector3> walls = new List<Vector3>
        {
            new Vector3(corner.x - Road_length / 2f-0.1f, 0, corner.z),
            new Vector3(corner.x + Road_length / 2f+0.1f, 0, corner.z),
            new Vector3(corner.x, 0, corner.z - Road_length / 2f-0.1f),
            new Vector3(corner.x, 0, corner.z + Road_length / 2f+0.1f)
        };
        
        foreach ( Vector3 wall in walls)
        {
            int count = 0;
            foreach (Bounds roadBound in roadBounds)
            {
                if(roadBound.Contains(wall))
                {
                    count++;
                    break;
                }
            }
            if (count == 0)
            {
                GameObject Turning_wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Turning_wall.name = "turning(Wall)";
                Turning_wall.transform.position = new Vector3(wall.x, wall_height/2f,wall.z);
            
                if (Mathf.Approximately(wall.z, corner.z))
                {

                    Turning_wall.transform.localScale = new Vector3(0.3f, wall_height, Road_length);
                }
                else
                {
                    Turning_wall.transform.localScale = new Vector3(Road_length, wall_height, 0.3f);
                }
            }
            else
            {
                int a=0;
                foreach(Bounds roomBound in room_bounds)
                {
                    if (roomBound.Contains(wall))
                    {
                        a++;
                        break;
                    }
                }
                if (a == 0)
                {
                    
                   if (Mathf.Approximately(wall.z, corner.z))
                    {
                        if (Mathf.Approximately(wall.x, corner.x + Road_length / 2f+0.1f))
                        {
                            List<float> distances = new List<float>();
                            Ray ray = new Ray(new Vector3(wall.x + 0.5f, 0, wall.z), Vector3.right);
                            foreach(Bounds roomBound in room_bounds)
                            {
                                if (roomBound.IntersectRay(ray, out float distance))
                                {
                                    distances.Add(distance);
                                }
                            }
                            foreach (GameObject turning in turning_points)
                            {
                                Bounds turning_bound = turning.GetComponent<Renderer>().bounds;
                                if (turning_bound.IntersectRay(ray, out float distance))
                                {
                                    distances.Add(distance);
                                }
                            }
                            if (distances.Count > 0)
                            {
                                float distance = distances.Min();
                                GameObject Corridor_lwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                GameObject Corridor_rwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                Corridor_lwall.name = "Corridor_lwall(Wall)";
                                Corridor_rwall.name = "Corridor_rwall(Wall)";
                                Corridor_lwall.transform.localScale = new Vector3(distance, wall_height, 0.1f);
                                Corridor_rwall.transform.localScale = new Vector3(distance, wall_height, 0.1f);
                                Corridor_lwall.transform.position = new Vector3(wall.x-0.1f + distance / 2f, wall_height / 2f, wall.z+Road_length/2f);
                                Corridor_rwall.transform.position = new Vector3(wall.x-0.1f + distance / 2f, wall_height / 2f, wall.z-Road_length/2f);
                            }
                            
                        }
                        else
                        {
                            List<float> distances = new List<float>();
                            Ray ray = new Ray(new Vector3(wall.x - 0.5f, 0, wall.z), Vector3.left);
                            foreach(Bounds roomBound in room_bounds)
                            {
                                if (roomBound.IntersectRay(ray, out float distance))
                                {
                                    distances.Add(distance);
                                }
                            }
                            foreach (GameObject turning in turning_points)
                            {
                                Bounds turning_bound = turning.GetComponent<Renderer>().bounds;
                                if (turning_bound.IntersectRay(ray, out float distance))
                                {
                                    distances.Add(distance);
                                }
                            }
                            if (distances.Count > 0)
                            {
                                float distance = distances.Min();
                                GameObject Corridor_lwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                GameObject Corridor_rwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                Corridor_lwall.name = "Corridor_lwall(Wall)";
                                Corridor_rwall.name = "Corridor_rwall(Wall)";
                                Corridor_lwall.transform.localScale = new Vector3(distance, wall_height, 0.1f);
                                Corridor_rwall.transform.localScale = new Vector3(distance, wall_height, 0.1f);
                                Corridor_lwall.transform.position = new Vector3(wall.x+0.1f - distance / 2f, wall_height / 2f, wall.z+Road_length/2f);
                                Corridor_rwall.transform.position = new Vector3(wall.x+0.1f - distance / 2f, wall_height / 2f, wall.z-Road_length/2f);
                            }
                        }
                    }
                    else
                    {
                        if (Mathf.Approximately(wall.z, corner.z + Road_length / 2f+0.1f))
                        {
                            List<float> distances = new List<float>();
                            Ray ray = new Ray(new Vector3(wall.x, 0, wall.z + 0.5f), Vector3.forward);
                            foreach(Bounds roomBound in room_bounds)
                            {
                                if (roomBound.IntersectRay(ray, out float distance))
                                {
                                    distances.Add(distance);
                                }
                            }
                                foreach (GameObject turning in turning_points)
                                {
                                    Bounds turning_bound = turning.GetComponent<Renderer>().bounds;
                                    if (turning_bound.IntersectRay(ray, out float distance))
                                    {
                                        distances.Add(distance);
                                    }
                                }
                            if (distances.Count > 0){
                                float distance = distances.Min();
                                GameObject Corridor_lwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                GameObject Corridor_rwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                Corridor_lwall.name = "Corridor_lwall(Wall)";
                                Corridor_rwall.name = "Corridor_rwall(Wall)";
                                Corridor_lwall.transform.localScale = new Vector3(0.1f, wall_height, distance);
                                Corridor_rwall.transform.localScale = new Vector3(0.1f, wall_height, distance);
                                Corridor_lwall.transform.position = new Vector3(wall.x+Road_length/2f, wall_height / 2f, wall.z+0.1f + distance / 2f);
                                Corridor_rwall.transform.position = new Vector3(wall.x-Road_length/2f, wall_height / 2f, wall.z+0.1f + distance / 2f);
                            }
                                
                        }
                        else
                        {
                            Ray ray = new Ray(new Vector3(wall.x, 0, wall.z - 0.5f), Vector3.back);
                            List<float> distances = new List<float>();

                            foreach(Bounds roomBound in room_bounds)
                            {
                                
                                if (roomBound.IntersectRay(ray, out float distance))
                                {
                                    distances.Add(distance);
                                }
                            }
                            foreach (GameObject turning in turning_points)
                            {
                                Bounds turning_bound = turning.GetComponent<Renderer>().bounds;
                                if (turning_bound.IntersectRay(ray, out float distance))
                                {
                                    distances.Add(distance);
                                }
                            }
                            if (distances.Count > 0)
                            {
                                float distance = distances.Min();
                                GameObject Corridor_lwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                GameObject Corridor_rwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                Corridor_lwall.name = "Corridor_lwall(Wall)";
                                Corridor_rwall.name = "Corridor_rwall(Wall)";
                                Corridor_lwall.transform.localScale = new Vector3(0.1f, wall_height, distance);
                                Corridor_rwall.transform.localScale = new Vector3(0.1f, wall_height, distance);
                                Corridor_lwall.transform.position = new Vector3(wall.x+Road_length/2f, wall_height / 2f, wall.z+0.1f - distance / 2f);
                                Corridor_rwall.transform.position = new Vector3(wall.x-Road_length/2f, wall_height / 2f, wall.z+0.1f - distance / 2f);
                            }
                        }
                    }
                }
            }

        }
    }
    void CreateCorridorWall()
    {
        foreach (Bounds roadBound in road_bounds)
        {
           for(int i = 0; i < room_bounds.Count;i++)
            {
                for(int j=i+1; j < room_bounds.Count; j++)
                {
                    int count = 0;
                    foreach (GameObject turning in turning_points)
                    {
                        int count_cover = 0;
                        foreach (Bounds roomBound in room_bounds)
                        {
                            if(IsCovered(turning.GetComponent<Renderer>().bounds, roomBound))
                            {
                                count_cover++;
                                break;
                            }
                        }
                        if(count_cover > 0) continue;
                        Bounds turning_bound = turning.GetComponent<Renderer>().bounds;
                        if (roadBound.Contains(turning_bound.center))
                        {
                            
                            count++;
                            break;
                        }
                    }
                    if (count > 0) continue;
                    if (Mathf.Approximately(roadBound.max.x-roadBound.min.x, Road_length))
                    {
                        Vector3 up_point = new Vector3(roadBound.center.x, 0, roadBound.max.z);
                        Vector3 down_point = new Vector3(roadBound.center.x, 0, roadBound.min.z);
                        if((room_bounds[i].Contains(up_point) && room_bounds[j].Contains(down_point))||(room_bounds[i].Contains(down_point) && room_bounds[j].Contains(up_point)))
                        {
                            float maxZ = Mathf.Max(room_bounds[i].min.z, room_bounds[j].min.z);
                            float minZ = Mathf.Min(room_bounds[i].max.z, room_bounds[j].max.z);
                            GameObject Corridor_lwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            GameObject Corridor_rwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Corridor_lwall.name = "NormalCorridor_lwall(Wall)";
                            Corridor_rwall.name = "NormalCorridor_rwall(Wall)";
                            Corridor_lwall.transform.localScale = new Vector3(0.1f, wall_height, maxZ - minZ);
                            Corridor_rwall.transform.localScale = new Vector3(0.1f, wall_height, maxZ - minZ);
                            Corridor_lwall.transform.position = new Vector3(roadBound.center.x - Road_length / 2f, wall_height / 2f, (maxZ + minZ) / 2f);
                            Corridor_rwall.transform.position = new Vector3(roadBound.center.x + Road_length / 2f, wall_height / 2f, (maxZ + minZ) / 2f);
                        }
                    }
                    else
                    {
                        Vector3 left_point = new Vector3(roadBound.min.x, 0, roadBound.center.z);
                        Vector3 right_point = new Vector3(roadBound.max.x, 0, roadBound.center.z);
                        if((room_bounds[i].Contains(left_point) && room_bounds[j].Contains(right_point))||(room_bounds[i].Contains(right_point) && room_bounds[j].Contains(left_point)))
                        {
                            float maxX = Mathf.Max(room_bounds[i].min.x, room_bounds[j].min.x);
                            float minX = Mathf.Min(room_bounds[i].max.x, room_bounds[j].max.x);
                            GameObject Corridor_lwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            GameObject Corridor_rwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Corridor_lwall.name = "NormalCorridor_lwall(Wall)";
                            Corridor_rwall.name = "NormalCorridor_rwall(Wall)";
                            Corridor_lwall.transform.localScale = new Vector3(maxX - minX, wall_height, 0.1f);
                            Corridor_rwall.transform.localScale = new Vector3(maxX - minX, wall_height, 0.1f);
                            Corridor_lwall.transform.position = new Vector3((maxX + minX) / 2f, wall_height / 2f, roadBound.center.z - Road_length / 2f);
                            Corridor_rwall.transform.position = new Vector3((maxX + minX) / 2f, wall_height / 2f, roadBound.center.z + Road_length / 2f);
                        }

                    }
                }
            }
        }
    }

    void GenerateRoom_wall(List<Bounds> roadBounds, List<Bounds> roomBounds)
    {   
        List<Bounds> Walls = new List<Bounds>();
        // List<Direction_bound> coveredBounds = new List<Direction_bound>();
        foreach(Bounds roadBound in roadBounds)
        {
            foreach(Bounds roomBound in roomBounds)
            {
                // if (!IsCovered(roadBound, roomBound)&&roadBound.Intersects(roomBound))
                if (roadBound.Intersects(roomBound))
                {
                    Bounds newbound = GetIntersectionBounds(roadBound, roomBound);
                    GameObject wall_up = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    GameObject wall_down = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // GameObject wall_support = GameObject.CreatePrimitive(PrimitiveType.Cube);

                    wall_up.name = "room_upwall(Wall)";
                    wall_down.name = "room_downwall(Wall)";
                    // wall_support.name = "room_wall_support(teleport)";
                    // wall_support.tag = "teleport";
                    // wall_support.GetComponent<Renderer>().material.color = Color.red;
                    float middle_up = 0f;
                    float middle_down = 0f;
                    if (Mathf.Approximately(newbound.min.x, roomBound.min.x) || Mathf.Approximately(newbound.max.x, roomBound.max.x))
                    {
                        middle_up = (newbound.max.z + roomBound.max.z) / 2f; 
                        middle_down = (newbound.min.z + roomBound.min.z) / 2f;

                        float leghth_up = Mathf.Abs(roomBound.max.z - newbound.max.z);
                        float length_down = Mathf.Abs(newbound.min.z - roomBound.min.z);

                        if(Mathf.Approximately(newbound.min.x, roomBound.min.x))
                        {
                            wall_up.transform.position = new Vector3(newbound.min.x, wall_height / 2f, middle_up);
                            wall_down.transform.position = new Vector3(newbound.min.x, wall_height / 2f, middle_down);
                            // wall_support.transform.position = new Vector3(newbound.min.x, wall_height / 2f, newbound.center.z);
                            // wall_support.transform.localScale = new Vector3(0.1f, wall_height, Mathf.Abs(roadBound.max.z - roadBound.min.z));
                        }
                        else if (Mathf.Approximately(newbound.max.x, roomBound.max.x))
                        {
                            wall_up.transform.position = new Vector3(newbound.max.x, wall_height / 2f, middle_up);
                            wall_down.transform.position = new Vector3(newbound.max.x, wall_height / 2f, middle_down);
                            // wall_support.transform.position = new Vector3(newbound.max.x, wall_height / 2f, newbound.center.z);
                            // wall_support.transform.localScale = new Vector3(0.1f, wall_height, Mathf.Abs(roadBound.max.z - roadBound.min.z));

                        }
                        wall_up.transform.localScale = new Vector3(0.1f, wall_height, leghth_up);
                        wall_down.transform.localScale = new Vector3(0.1f, wall_height, length_down);
                        
                    }
                    else if (Mathf.Approximately(newbound.min.z, roomBound.min.z) || Mathf.Approximately(newbound.max.z, roomBound.max.z))
                    {
                        middle_up = (roomBound.max.x + newbound.max.x) / 2f; 
                        middle_down = (roomBound.min.x + newbound.min.x) / 2f;

                        float leghth_up = Mathf.Abs(roomBound.max.x - newbound.max.x);
                        float length_down = Mathf.Abs(newbound.min.x - roomBound.min.x);

                        if(Mathf.Approximately(newbound.min.z, roomBound.min.z))
                        {
                            wall_up.transform.position = new Vector3(middle_up, wall_height / 2f, newbound.min.z);
                            wall_down.transform.position = new Vector3(middle_down, wall_height / 2f, newbound.min.z);
                            // wall_support.transform.position = new Vector3(newbound.center.x, wall_height / 2f, newbound.min.z);
                            // wall_support.transform.localScale = new Vector3(Mathf.Abs(roadBound.max.x - roadBound.min.x), wall_height, 0.1f);
                        }
                        else if (Mathf.Approximately(newbound.max.z, roomBound.max.z))
                        {
                            wall_up.transform.position = new Vector3(middle_up, wall_height / 2f, newbound.max.z);
                            wall_down.transform.position = new Vector3(middle_down, wall_height / 2f, newbound.max.z);
                            // wall_support.transform.position = new Vector3(newbound.center.x, wall_height / 2f, newbound.max.z);
                            // wall_support.transform.localScale = new Vector3(Mathf.Abs(roadBound.max.x - roadBound.min.x), wall_height, 0.1f);

                        }
                        wall_up.transform.localScale = new Vector3(leghth_up, wall_height, 0.1f);
                        wall_down.transform.localScale = new Vector3(length_down, wall_height, 0.1f);
                    }
                    Walls.Add(wall_up.GetComponent<Renderer>().bounds);
                    Walls.Add(wall_down.GetComponent<Renderer>().bounds);
                }
            }
        }
        
        foreach(Bounds room in roomBounds)
        {
            List<Vector3> sides = new List<Vector3>
            {
                new Vector3(room.min.x, wall_height/2f, room.center.z),
                new Vector3(room.max.x, wall_height/2f, room.center.z),
                new Vector3(room.center.x, wall_height/2f, room.min.z),
                new Vector3(room.center.x, wall_height/2f, room.max.z)
            };
            foreach(Vector3 side in sides)
            {
                Vector3 side2 = Vector3.zero;
                if (Mathf.Approximately(side.z, room.center.z))
                {
                    side2 = new Vector3(side.x,side.y,side.z+Road_length);
                }
                else
                {
                    side2 = new Vector3(side.x+Road_length,side.y,side.z);
                }
                int count = 0;
                foreach(Bounds wall in Walls)                {
                    if (wall.Contains(side) || wall.Contains(side2))
                    {
                        count++;
                        break;
                    }
                }
                if (count == 0)
                {
                    GameObject room_wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    room_wall.name = "room_wall(Wall)";
                    room_wall.transform.position = side;
                    float length = 0f;
                    if (Mathf.Approximately(side.z, room.center.z))
                    {
                        length = Mathf.Abs(room.max.x - room.min.x);
                        room_wall.transform.localScale = new Vector3(0.1f, wall_height, length);
                    }
                    else
                    {
                        length = Mathf.Abs(room.max.z - room.min.z);
                        room_wall.transform.localScale = new Vector3(length, wall_height, 0.1f);
                    }
                }
            }
        }
    }

       int GetSegmentHash(Vector3 a, Vector3 b)
    {
        int ax = Mathf.RoundToInt(a.x);
        int az = Mathf.RoundToInt(a.z);
        int bx = Mathf.RoundToInt(b.x);
        int bz = Mathf.RoundToInt(b.z);
        if (ax < bx || (ax == bx && az < bz))
        {
            return (ax * 1000 + az) * 10000 + (bx * 1000 + bz);
        }
        else
        {
            return (bx * 1000 + bz) * 10000 + (ax * 1000 + az);
        }
    }
    int FindRoot(int[] parent, int index)
    {
        if (parent[index] != index)
        {
            parent[index] = FindRoot(parent, parent[index]);
        }
        return parent[index];

    }
    void Generate_spawn()
    {
        
        string assetPath = $"Assets\\Scenes\\special\\spawn.blend";
        GameObject spawnpoint = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (spawnpoint == null)
        {
            Debug.LogError($"Cannot load spawn point prefab: {assetPath}");
            return;
        }

        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);
        int random_rotate = Random.Range(0, 4); 
        Vector3 rotation = new Vector3(0, random_rotate*90, 0);
        Quaternion rotation_quaternion = Quaternion.Euler(rotation);
        Vector3 position = new Vector3(x, 0, z);
        GameObject instance = Instantiate(spawnpoint, position, rotation_quaternion);
        instance.tag = "SpawnPoint";
        spawned_prefabs.Add(instance);
        room_bounds.Add(GetRoomBounds(instance));
    }
    void Generate_boss_room()
    {
            string assetPath = $"Assets\\Scenes\\special\\boss_room.blend";
            GameObject boss_room = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (boss_room == null)
            {
                Debug.LogError($"Cannot load boss room prefab: {assetPath}");
                return;
            }
            for(int i=0; i<attempts_spawn*2; i++)
            {
                float x = Random.Range(minX, maxX);
                float z = Random.Range(minZ, maxZ);
                Vector3 position = new Vector3(x, 0, z);
                Vector3 halfExtents = new Vector3(1.5f*least_distance, 0.1f, 1.5f*least_distance);
                Vector3 halfExtents_max = new Vector3(1.2f*max_distance, 0.1f, 1.2f*max_distance);
                if (!Physics.CheckBox(position, halfExtents)&&Physics.CheckBox(position, halfExtents_max))
                {
                    int random_rotate = Random.Range(0, 4); 
                    Vector3 rotation = new Vector3(0, random_rotate*90, 0);
                    Quaternion rotation_quaternion = Quaternion.Euler(rotation);
                    GameObject instance = Instantiate(boss_room, position, rotation_quaternion);
                    instance.tag = "BossRoom";
                    spawned_prefabs.Add(instance);
                    room_bounds.Add(GetRoomBounds(instance));
                    break;
                }
            }
    }
    void Generate_room()
    {
        
        // go through the file
        string fullPath = Path.Combine(Application.dataPath, folderPath);
        if (!Directory.Exists(fullPath))
        {
            Debug.LogError($"not exist: {fullPath}");
            return;
        }
        string[] blenderFiles = Directory.GetFiles(fullPath, "*.blend", SearchOption.TopDirectoryOnly);
        if (blenderFiles.Length == 0)
        {
            Debug.LogError($"No .blender files found in {fullPath}");
            return;
        }

        // store in a list
        List<GameObject> prefabs = new List<GameObject>();
        foreach (string file in blenderFiles)
        {
            string assetPath = file.Replace(Application.dataPath, "Assets");
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (obj != null)
            {
                prefabs.Add(obj);
            }
            else
            {
                Debug.LogWarning($"Cannot load asset: {assetPath}");
            }
        }

        if (prefabs.Count == 0)
        {
            Debug.LogError("No available prefabs");
            return;
        }

        // spawn the prefabs
        foreach (GameObject prefab in prefabs)
        {
            bool spawned = false;
            for (int i = 0; i < attempts_spawn; i++)
            {
                float x = Random.Range(minX, maxX);
                float z = Random.Range(minZ, maxZ);
                int random_rotate = Random.Range(0, 4); 
                Vector3 rotation = new Vector3(0, random_rotate*90, 0);
                Quaternion rotation_quaternion = Quaternion.Euler(rotation);
                Vector3 position = new Vector3(x, 0, z);
                Vector3 halfExtents = new Vector3(least_distance, 0.1f, least_distance);
                Vector3 halfExtents_max = new Vector3(max_distance, 0.1f, max_distance);
                if (!Physics.CheckBox(position, halfExtents)&&Physics.CheckBox(position, halfExtents_max))
                {
                    GameObject instance = Instantiate(prefab, position, rotation_quaternion);
                    instance.tag = "Room";
                    // CreateRoom_Wall(instance);
                    spawned_prefabs.Add(instance);
                    room_bounds.Add(GetRoomBounds(instance));
                    spawned = true;
                    break;
                }
            }
            if (!spawned)
            {
                Debug.LogWarning($"Failed to spawn {prefab.name} after {attempts_spawn} attempts");
            }
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
    void place_navigation()
    {
        foreach (GameObject prefab in spawned_prefabs)
        {
            foreach (Transform child in prefab.transform)
            {
                if(child.name == "plain")
                {
                    NavMeshSurface surface = child.gameObject.AddComponent<NavMeshSurface>();
                    surface.BuildNavMesh();
                }
                else
                {
                    NavMeshObstacle obstacle = child.gameObject.AddComponent<NavMeshObstacle>();
                    obstacle.carving = true;
                    obstacle.shape = NavMeshObstacleShape.Box;
                }
            }
        }
    }
}

