using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomFromList : MonoBehaviour
{
    public GameObject[] list;

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject obj in list)
        {
            obj.SetActive(false);
        }
        int rand = Random.Range(0, list.Length);
        list[rand].SetActive(true);
    }
}
