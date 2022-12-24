using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleFollowScript : MonoBehaviour
{
    SpriteRenderer _sr;

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();     
    }

    void Update()
    {
        transform.position = CameraFollowScript.instance._cam.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);

        //if(!GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(CameraFollowScript.instance._cam), _sr.bounds))
        //{
        //    print("off screen");
        //}
    }
}
