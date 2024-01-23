using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FountainBlock : MonoBehaviour, IPooledObject
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void LaunchBlock()
    {
        rb.AddForce(transform.up * 650 * Time.fixedDeltaTime * 80);
    }
}
