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
    protected Rigidbody carBody;
    new protected Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        car = GameObject.FindGameObjectWithTag("Player").transform;
        carBody = car.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 trajectory = carBody.velocity;
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, 5 + (trajectory.magnitude * 0.5f), stiffness);
        transform.position = Vector3.Lerp(transform.position, car.position + trajectory + camOffset, stiffness);
    }
}
