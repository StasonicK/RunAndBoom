using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddForce(Vector3.forward * 200 * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    private void OnDisable()
    {
        rb.velocity = Vector3.zero;
    }
}
