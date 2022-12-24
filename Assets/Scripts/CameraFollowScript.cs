using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows the camera to follow the player based on a set offset
/// </summary>
public class CameraFollowScript : MonoBehaviour
{
    public static CameraFollowScript instance;

    [Header("The transform to track")]
    /// <summary>
    /// The transform to track
    /// </summary>
    [SerializeField] Transform trackTransform;

    [SerializeField] Transform reticle;

    [SerializeField] float offset = 0.25f;
    [SerializeField] Vector2 recaptureScreen = new Vector2(8, 3);
    [SerializeField] Vector2 maxDistance = new Vector2(3, 2.5f);

    [Header("Follow Speed")]
    /// <summary>
    /// How fast does the camera move, multiplied with distance
    /// </summary>
    [SerializeField] float cameraSpeed = 3;

    Rigidbody2D _rb;

    public Camera _cam;

    [SerializeField] SpriteRenderer keepOnCamera;

    [SerializeField] float minCamSize = 3;
    [SerializeField] float maxCamSize = 6;

    [SerializeField] float cameraChangeRate = 0.05f;

    [SerializeField] float antiJitterValue = 0.3f;

    [SerializeField] float lerpVal = 1.5f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                Destroy(this);
            }
        }
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        #region Keep player on screen
        //distance x should be 0
        //distance y should be 1.6 
        //distance z should be -10

        Vector3 mouseOffset = reticle.position - trackTransform.position;

        Vector3 dist = transform.position - trackTransform.position;

        Vector2 newVelocity = new Vector2();
        
        //GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(CameraFollowScript.instance._cam), _sr.bounds)
        if (dist.x != 0)
        {
            if((dist.x > maxDistance.x || dist.x < -maxDistance.x) && (mouseOffset.x > recaptureScreen.x || mouseOffset.x < -recaptureScreen.x))
            {
                newVelocity.x = 0;
            }
            else
            {
                newVelocity.x = (-Mathf.Lerp(dist.x, -mouseOffset.x, offset)) * cameraSpeed;
            }
        }

        if (dist.y != 0)
        {
            newVelocity.y = 0;
            if ((dist.y > maxDistance.y || dist.y < -maxDistance.y) && (mouseOffset.y > recaptureScreen.y || mouseOffset.y < -recaptureScreen.y))
            {
                newVelocity.y = 0;
            }
            else
            {
                newVelocity.y = (-Mathf.Lerp(dist.y, -mouseOffset.y, offset)) * cameraSpeed;
            }
        }

        _rb.velocity = newVelocity;

        #region keep player on the screen for radical movements
        //player is off of the screen
        if(!GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(_cam), keepOnCamera.bounds))
        {
            transform.position = PlayerController.instance.transform.position - new Vector3(0, 0, 10);
            _rb.velocity = Vector2.zero;
        }
        #endregion
        #endregion

        #region increase screen size by speed
        //if player velocity = 0, screen size =3
        //float newSize = Mathf.Round(Mathf.Clamp(Mathf.Lerp(0, Mathf.Abs(PlayerController.instance._rigidbody2D.velocity.x), lerpVal), minCamSize, maxCamSize) * 10) / 10;
        float newSize = Mathf.Round(Mathf.Clamp(Mathf.Abs(PlayerController.instance._rigidbody2D.velocity.x), minCamSize, maxCamSize) * 10) / 10;
        float oldSize = _cam.orthographicSize;

        if((oldSize+antiJitterValue) < newSize)
        {
            _cam.orthographicSize += cameraChangeRate;
        } 
        else if((oldSize - antiJitterValue) > newSize)
        {
            _cam.orthographicSize -= cameraChangeRate;
        }
        #endregion
    }
}
