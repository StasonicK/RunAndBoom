using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicSpawn : MonoBehaviour
{
    //Custom class for keeping all variables in one place.
    [System.Serializable]
    public class Pool
    {
        public string id;
        public GameObject poolObject;
        public int count;

        public float zOffset;
        //This is hidden because it should always be 0 on start.
        [HideInInspector] public float lastPos;
        public float spawnChance;
        public Vector3 position;
        public Vector3 randPosFactor;
        public Vector3 rotation;
        public Vector3 randRotFactor;
        public int rows;
        public Vector3 rowOffset;
    }
    //Custom class for keeping all reset properties in one place.
    [System.Serializable]
    public class ResetProcess
    {
        [Tooltip("The z position at which everything gets reset to origin.")]
        public float maxTravelDistance;
        [Tooltip("(Optional) Assign something that hides everything that gets reset. For example, you can use a tunnel model.")]
        public Transform resetTransitionObject;
    }
    [System.Serializable]
    public class Points
    {
        [Header("Spawn Point (Stays in front)")]
        [Tooltip("This GameObject moves with the player. It stays in front the player to help spawn new objects.")]
        public Transform spawnPoint;
        [Tooltip("Assign a positive number to move it in front the player.")]
        public float spawnPointOffset;
        [Header("Deactivation Point (Stays behind)")]
        [Tooltip("This GameObject moves with the player. It stays behind the player to help decide when to recycle (disable) objects.")]
        public Transform deactivationPoint;
        [Tooltip("Assign a positive number to move it behind the player.")]
        public float deactivationPointOffset;
    }
    [System.Serializable]
    public class Main
    {
        [Header("Spawned Object Holder (Optional)")]
        [Tooltip("This helps group all of the generated objects under itself to make the hierarchy a neat place.")]
        public GameObject parentObject;
        [Header("Player")]
        [Tooltip("Spawn objects based on the moving player's forward position.")]
        public Transform player;
    }
    [Tooltip("The general components to help drive the spawning process.")]
    [SerializeField] private Main mainComponents;
    [Tooltip("These points help track the spawning process to spawn new and recycle old objects in the pools.")]
    [SerializeField] private Points points;
    [Tooltip("Properties for positioning everything at origin to prevent floating point issues.")]
    [SerializeField] public ResetProcess resetProperties;
    [Tooltip("Configure Objects For Spawning")]
    [SerializeField] private List<Pool> pools;

    //This list is used to keep track of all the spawned objects' position
    //to disable them when they are behing the deactivation point.
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    //Instantiate objects before the game starts and add them to the dictionary with their IDs.
    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.count; i++)
            {
                GameObject obj = Instantiate(pool.poolObject);
                obj.SetActive(false);
                if (mainComponents.parentObject)
                {
                    obj.transform.parent = mainComponents.parentObject.transform;
                }
                objectPool.Enqueue(obj);
                spawnedObjects.Add(obj);
            }

            poolDictionary.Add(pool.id, objectPool);
        }
    }

    //Returns an object from the queue based on the given ID.
    public GameObject SpawnfromPool(string _id)
    {
        //Notify if id is missing.
        if (!poolDictionary.ContainsKey(_id))
        {
            Debug.Log("ID: " + _id + " does not exist.");
            return null;
        }

        GameObject objecttoSpawn = poolDictionary[_id].Dequeue();

        poolDictionary[_id].Enqueue(objecttoSpawn);
        return objecttoSpawn;
    }

    void Update()
    {
        //Constantly checks if the spawn point is in front of the last spawned objects.
        //If spawn point is in front of the latest spawned objects, new objects are spawned in front.
        ConstantSpawn();

        //Moves spawn point and deactivation point with the player or any other Transform assigned to "target"
        ManagePoints();

        //Checks if the spawned objects are behind deactivation point.
        //If they are behing deactivation point, they are disabled.
        ManageSpawnedObjects();

        //Checks if objects need to be reset back to origin to prevent floating point issues.
        ResetStatus();
    }

    private void ResetStatus()
    {
        if(mainComponents.player.transform.position.z >= resetProperties.maxTravelDistance)
        {
            //Move player to origin.
            Vector3 pos = mainComponents.player.transform.position;
            pos.z = 0;
            mainComponents.player.transform.position = pos;
            //Disable all active objects.
            foreach(GameObject obj in spawnedObjects)
            {
                obj.SetActive(false);
            }
            //Reset lastPos of pooled objects to zero (origin).
            foreach(Pool pool in pools)
            {
                pool.lastPos = 0;
            }
        }
        else
        {
            if (resetProperties.resetTransitionObject)
            {
                if (mainComponents.player.position.z > resetProperties.resetTransitionObject.localScale.z * 4)
                {
                    resetProperties.resetTransitionObject.position = new Vector3(resetProperties.resetTransitionObject.position.x, resetProperties.resetTransitionObject.position.y, resetProperties.maxTravelDistance);
                }
                else
                {
                    resetProperties.resetTransitionObject.position = new Vector3(resetProperties.resetTransitionObject.position.x, resetProperties.resetTransitionObject.position.y, 0);
                }
            }
        }
    }

    private void ConstantSpawn()
    {
        for (int i = 0; i < pools.Count; i++)
        {
            SpawnDynamicObject(pools[i].id, pools[i].lastPos, pools[i].position,
                        pools[i].randPosFactor, pools[i].rotation, pools[i].randRotFactor,
                        pools[i].spawnChance, pools[i].rows, pools[i].rowOffset);
        }
    }

    private void SpawnDynamicObject(string id, float lastZPos, Vector3 position, Vector3 randPosFactor, Vector3 rotation, Vector3 randRotFactor, float spawnChance, int rows, Vector3 rowOffset)
    {
        //Check if spawn point if spawn point is in front of the last spawned objects.
        if (points.spawnPoint.position.z > lastZPos)
        {
            //Update last position.
            for (int j = 0; j < pools.Count; j++)
            {
                if (pools[j].id == id)
                    pools[j].lastPos += pools[j].zOffset;
            }
            //Spawn an object per row.
            for (int i = 0; i < rows; i++)
            {
                //Generate random value and spawn based on it.
                if (Random.value <= spawnChance)
                {
                    GameObject obj = SpawnfromPool(id);

                    obj.SetActive(true);

                    Vector3 randomPos = new Vector3(Random.Range(-randPosFactor.x, randPosFactor.x) + RowPos(i, rowOffset).x,
                                                 Random.Range(-randPosFactor.y, randPosFactor.y) + RowPos(i, rowOffset).y,
                                                 Random.Range(-randPosFactor.z, randPosFactor.z) + RowPos(i, rowOffset).z);

                    Vector3 randomRot = new Vector3(Random.Range(-randRotFactor.x, randRotFactor.x),
                                                 Random.Range(-randRotFactor.y, randRotFactor.y),
                                                 Random.Range(-randRotFactor.z, randRotFactor.z));

                    obj.transform.rotation = Quaternion.Euler(rotation + randomRot);
                    obj.transform.position = new Vector3(position.x, position.y, lastZPos) + randomPos;
                }
            }
        }
    }

    //Returns the position of the new row based on row offset and number of rows if there is more than one row.
    private Vector3 RowPos(int rowNumber, Vector3 offset)
    {
        return new Vector3(offset.x * rowNumber, offset.y * rowNumber, offset.z * rowNumber);
    }

    private void ManageSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            //Check if the object is active. If not, this is ignored.
            if (obj.activeInHierarchy)
            {
                if (obj.transform.position.z < points.deactivationPoint.position.z)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    private void ManagePoints()
    {
        points.spawnPoint.position = new Vector3(0, 0, mainComponents.player.position.z + points.spawnPointOffset);
        points.deactivationPoint.position = new Vector3(0, 0, mainComponents.player.position.z - points.deactivationPointOffset);
    }
}
