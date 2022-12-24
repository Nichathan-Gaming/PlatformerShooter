using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLogic : MonoBehaviour
{
    Rigidbody2D _rb2d;

    [SerializeField] float _bulletForce;

    [Header("How long does the shot last")]
    [SerializeField] float secondsForShot = 0.5f;

    [SerializeField] float damage = 10;

    /// <summary>
    /// Can be positive or negative
    /// </summary>
    [SerializeField] float critMultipler;

    [SerializeField] MonsterControllerScript controllingMonster;

    // Start is called before the first frame update
    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        gameObject.SetActive(false);
    }

    public float GetDamage()
    {
        if(controllingMonster)
        {
            return controllingMonster.GetDamage();
        }
        else
        {
            return Random.Range(damage - critMultipler, damage + critMultipler);
        }
    }

    public void Shoot(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        Vector3 rotation = transform.position - target;
        _rb2d.velocity = new Vector2(direction.x, direction.y).normalized * _bulletForce;
        float rotateZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotateZ);

        StartCoroutine(TimeShot());
    }

    //turn off the shot
    IEnumerator TimeShot()
    {
        yield return new WaitForSeconds(secondsForShot);

        _rb2d.velocity = Vector2.zero;
        gameObject.SetActive(false);
    }
}
