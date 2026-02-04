using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coins : MonoBehaviour
{
    public float rotationSpeed = 100f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
    }
}
