using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunScript : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    private void FixedUpdate()
    {
        transform.Rotate(rotationSpeed, 0, 0);
    }
}
