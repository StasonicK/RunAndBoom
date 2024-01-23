using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaSpawn : MonoBehaviour
{
    [Header("Spawned Object Holder (Optional)")]
    [SerializeField] private GameObject parentObject;

    [System.Serializable]
    public class AreaProperties
    {
        public Vector2 area;
        public GameObject objectModel;
        public Vector3 objectScale;
        public Vector3 objectRotation;
        public float objectOffset;
    }
    [Header("Area Properties")]
    [SerializeField] public AreaProperties areaProperties;
    [Header("Is The Area Modifiable At Runtime?")]
    [SerializeField] public bool isModify;

    private int calcCount;
    private int rowCount;

    //This list is used to keep track of all the spawned objects' position
    //to disable them when they are behing the deactivation point.
    private Queue<GameObject> objectPool = new Queue<GameObject>();
    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Awake()
    {
        SetArea();
        Initialize();
    }

    private void Start()
    {
        if(!isModify)
        {
            SpawnInArea();
        }
    }

    private void Update()
    {
        if (isModify)
        {
            SpawnInArea();
        }
    }

    //Spawn the objects after calculating the rows and columns.
    private void SpawnInArea()
    {
        int rCount = 0;
        int cCount = 0;
        for (int i = 0; i < calcCount; i++)
        {
            GameObject obj = SpawnfromPool();
            obj.SetActive(true);
            obj.transform.localRotation = Quaternion.Euler(areaProperties.objectRotation);
            if (parentObject)
            {
                obj.transform.localPosition = new Vector3((areaProperties.objectOffset + areaProperties.objectScale.x) * rCount, areaProperties.objectScale.y, (areaProperties.objectOffset + areaProperties.objectScale.z) * cCount);
            }
            else
            {
                obj.transform.position = new Vector3((areaProperties.objectOffset + areaProperties.objectScale.x) * rCount, areaProperties.objectScale.y, (areaProperties.objectOffset + areaProperties.objectScale.z) * cCount);
            }
            rCount++;
            if(rCount == rowCount)
            {
                rCount = 0;
                cCount++;
            }
        }
    }

    //Calculate the number of rows and columns based on the area.
    private void SetArea()
    {
        rowCount = Mathf.FloorToInt(areaProperties.area.x / (areaProperties.objectScale.x + areaProperties.objectOffset));
        int _colCount = Mathf.FloorToInt(areaProperties.area.y / (areaProperties.objectScale.z + areaProperties.objectOffset));
        calcCount = rowCount * _colCount;
    }

    //Create objects before the game starts to make the ready to be reused.
    private void Initialize()
    {
        for (int i = 0; i < calcCount; i++)
        {
            GameObject obj = Instantiate(areaProperties.objectModel);
            obj.SetActive(false);
            if (parentObject)
            {
                obj.transform.parent = parentObject.transform;
            }
            objectPool.Enqueue(obj);
            spawnedObjects.Add(obj);
        }
    }

    //Returns an object from the queue based on the given ID.
    public GameObject SpawnfromPool()
    {
        GameObject objecttoSpawn = objectPool.Dequeue();

        objectPool.Enqueue(objecttoSpawn);
        return objecttoSpawn;
    }
}
