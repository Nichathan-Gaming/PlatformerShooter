using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotates the Player Gun Arm based on the mouse position
/// </summary>
public class AimController : MonoBehaviour
{
    /// <summary>
    /// The camera following the player
    /// </summary>
    Camera _followCam;

    /// <summary>
    /// The transform of the player
    /// </summary>
    [SerializeField] Transform player;

    /// <summary>
    /// Required to avoid constant flipping
    /// </summary>
    [SerializeField] float timeBetweenRotations;
    bool canFlip = true;

    // Start is called before the first frame update
    void Start()
    {
        _followCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        #region move arm
        Vector3 _mousePos = _followCam.ScreenToWorldPoint(Input.mousePosition);

        Vector3 rotation = _mousePos - transform.position;
        float rotateZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

        float z = transform.rotation.eulerAngles.z;

        transform.rotation = Quaternion.Euler(0, 0, rotateZ);
        #endregion

        #region flip character
        if (!canFlip) return;

        //facing right
        if (player.rotation.y == 0)
        {
            if(z > 90 && z < 270)
            {
                StartCoroutine(AllowFlipTimer());
                transform.localScale = new Vector3(1, -1, 1);
                if (player.rotation.y == 0) player.Rotate(new Vector3(0, 180));
            }
        }
        //facing left
        else
        {
            if(z < 90 || z > 270)
            {
                StartCoroutine(AllowFlipTimer());
                transform.localScale = new Vector3(1, 1, 1);
                player.rotation = Quaternion.identity;
            }
        }
        #endregion
    }

    IEnumerator AllowFlipTimer()
    {
        canFlip = false;
        yield return new WaitForSeconds(timeBetweenRotations);
        canFlip = true;
    }
}
