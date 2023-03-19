using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public Rigidbody car;
    public float lookAhead;
    public float stayBehind;
    public Vector3 defaultPos;
    public Vector3 defaultLook;
    public float defaultDist;
    public float minSpeed;
    // Start is called before the first frame update
    void Start()
    {
        defaultDist = (transform.position - car.transform.position).magnitude;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        defaultPos = car.transform.position - car.transform.forward * defaultDist + Vector3.up * 2;
        defaultLook = car.transform.position + car.transform.forward * defaultDist;
        var direction = car.velocity.normalized;
        Vector3 lookat = car.transform.position + car.velocity * lookAhead;
        Vector3 pos = car.transform.position - car.velocity * stayBehind + Vector3.up * 2;
        transform.position = Vector3.Lerp(pos, defaultPos, Mathf.Max(minSpeed - car.velocity.magnitude, 0));
        transform.LookAt(Vector3.Lerp(lookat, defaultLook, Mathf.Max(minSpeed - car.velocity.magnitude, 0)));
    }
}
