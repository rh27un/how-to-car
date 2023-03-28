using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [SerializeField]
    protected Vector3 camOffset;
    [SerializeField, Range(0f, 1f)]
    protected float stiffness;

    protected Transform car;
    // Start is called before the first frame update
    void Start()
    {
        car = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, car.position + camOffset, stiffness);
    }
}
