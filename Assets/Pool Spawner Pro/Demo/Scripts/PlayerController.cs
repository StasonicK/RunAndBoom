using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool isSideMovement;

    void Update()
    {
        if (!isSideMovement)
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(new Vector3(0, 0, 30 * Time.deltaTime));
            }
        }
        else
        {
            if(Input.GetKey(KeyCode.A))
            {
                transform.Translate(new Vector3(-30 * Time.deltaTime, 0, 0));
            }
            else if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(new Vector3(30 * Time.deltaTime, 0, 0));
            }
        }
    }
}
