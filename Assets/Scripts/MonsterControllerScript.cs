using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterControllerScript : MonoBehaviour
{
    #region monster types
    [Header("Monster type")]
    /// <summary>
    /// true: ground, false: flying
    /// </summary>
    [SerializeField] bool groundOrFlying;
    /// <summary>
    /// true: melee, false: ranged
    /// </summary>
    [SerializeField] bool meleeOrRanged;
    #endregion

    [Header("The health of the monster")]
    public float maxHealth = 20;
    public float currentHealth = 20;

    #region On Collide
    [Header("On Collide")]
    /// <summary>
    /// When the player collides with this object, how hard are they pushed?
    /// </summary>
    [SerializeField] float basePushForce = 5;

    /// <summary>
    /// Wait this time on collision
    /// </summary>
    [SerializeField] float timeToWaitOnPush = 1;
    #endregion

    /// <summary>
    /// False on collision
    /// </summary>
    bool canMove = true;

    #region movement
    [Header("Movement")]
    /// <summary>
    /// The spawner that this stays near
    /// </summary>
    [SerializeField] MonsterSpawnerScript spawnerParent;
    /// <summary>
    /// when moved this far from the spawner, do not allow to move anymore
    /// </summary>
    [SerializeField] float maxDistanceFromSpawner = 5;
    /// <summary>
    /// continue moving for this length of time
    /// </summary>
    [SerializeField] float timeToWalk = 1;

    /// <summary>
    /// Controls the rigidbody
    /// </summary>
    Rigidbody2D _rb;
    #endregion

    #region Attack
    [SerializeField] int attackDamage;
    [SerializeField] int critMultiplier;

    bool isAttacking = false,
        canAttack = false;

    [SerializeField] float preAttackCooldown;
    [SerializeField] float postAttackCooldown;
    [SerializeField] float attackDuration;

    [SerializeField] float meleeAttackRange;
    [SerializeField] float rangedAttackRange;

    [SerializeField] float meleeAttackBaseSpeed;
    [SerializeField] float meleeAttackMaxSpeed;
    [SerializeField] float rangedAttackSpeed;

    /// <summary>
    /// Alerts the player that this monster is attacking
    /// </summary>
    [SerializeField] GameObject attackSignifier;

    [SerializeField] BulletLogic[] monsterBullets;
    [SerializeField] Transform bulletSpawnLocation;
    #endregion

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        StartCoroutine(MoveMonster());

        StartCoroutine(WaitToAttackOnSpawn());
    }

    private void Update()
    {
        if (canAttack)
        {
            if (meleeOrRanged)
            {
                CheckMeleeAttack();
            }
            else
            {
                CheckRangedAttack();
            }
        }

        if (isAttacking)
        {
            float dist = transform.position.x - PlayerController.instance.transform.position.x;

            //x < 0 when player on right
            if (dist > 0)
            {
                _rb.velocity = new Vector2(Mathf.Clamp(_rb.velocity.x - meleeAttackBaseSpeed, -meleeAttackMaxSpeed, -meleeAttackBaseSpeed), 0);
            }
            else
            {
                _rb.velocity = new Vector2(Mathf.Clamp(_rb.velocity.x + meleeAttackBaseSpeed, meleeAttackMaxSpeed, meleeAttackBaseSpeed), 0);
            }
        }
    }

    void CheckMeleeAttack()
    {
        float distanceFromPlayer = Vector3.Distance(PlayerController.instance.transform.position, transform.position);

        if (distanceFromPlayer < meleeAttackRange)
        {
            attackSignifier.SetActive(true);

            canMove = false;
            canAttack = false;

            isAttacking = true;

            StartCoroutine(IsAttackingTimer());
        }
    }

    void CheckRangedAttack()
    {
        float distanceFromPlayer = Vector3.Distance(PlayerController.instance.transform.position, transform.position);

        if (distanceFromPlayer < rangedAttackRange)
        {
            StartCoroutine(PauseToAttack());
        }
    }

    IEnumerator PauseToAttack()
    {
        attackSignifier.SetActive(true);
        canMove = false;
        canAttack = false;

        //move up to get better shot
        _rb.velocity = new Vector2(0, 1);

        yield return new WaitForSeconds(preAttackCooldown);

        CreateBullet();

        yield return new WaitForEndOfFrame();

        attackSignifier.SetActive(false);
        //needed to avoid comprimizing the bullets trajectory
        canMove = true;
    }

    /// <summary>
    /// shoots the first inactive bullet
    /// </summary>
    private void CreateBullet()
    {
        foreach (BulletLogic bullet in monsterBullets)
        {
            if (!bullet.gameObject.activeInHierarchy)
            {
                bullet.gameObject.SetActive(true);
                bullet.transform.position = bulletSpawnLocation.position;
                bullet.transform.rotation = Quaternion.identity;
                bullet.Shoot(PlayerController.instance.transform.position);

                break;
            }
        }

        StartCoroutine(WaitToFire());
    }

    /// <summary>
    /// Waits _cooldownTimer seconds before allowing the player to fire again
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitToFire()
    {
        yield return new WaitForSeconds(rangedAttackSpeed);

        canAttack = true;
    }

    void AttackOver(bool hadCollision)
    {
        StopCoroutine(IsAttackingTimer());

        attackSignifier.SetActive(false);

        isAttacking = false;
        if (!hadCollision) canMove = true;

        StartCoroutine(WaitForNextAttack());
    }

    IEnumerator WaitForNextAttack()
    {
        yield return new WaitForSeconds(postAttackCooldown);

        canAttack = true;
    }

    IEnumerator IsAttackingTimer()
    {
        yield return new WaitForSeconds(attackDuration);

        AttackOver(false);
    }

    IEnumerator WaitToAttackOnSpawn()
    {
        yield return new WaitForSeconds(preAttackCooldown);

        canAttack = true;
    }

    /// <summary>
    /// Creates a random number in the range of attackDamage -+ critMultiplier
    /// </summary>
    /// <returns>the damage dealt by this monster</returns>
    public float GetDamage()
    {
        return Random.Range(attackDamage - critMultiplier, attackDamage + critMultiplier);
    }

    /// <summary>
    /// revives the monster
    /// </summary>
    public void Revive()
    {
        StopAllCoroutines();
        currentHealth = maxHealth;
        transform.position = spawnerParent.transform.position;

        //forces the monster to the ground or into the air
        _rb.velocity = new Vector2(0, groundOrFlying?30 :-30);
        attackSignifier.SetActive(false);
        canMove = false;

        gameObject.SetActive(true);
        StartCoroutine(WaitToAttackOnSpawn());
        StartCoroutine(MoveMonster());

        StartCoroutine(pauseWalk());
    }

    /// <summary>
    /// Moves the monster based on their movement type
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveMonster()
    {
        if (canMove)
        {
            if (groundOrFlying)
            {
                MoveGround();
            }
            else
            {
                MoveFlying();
            }
        }

        yield return new WaitForSeconds(timeToWalk);

        StartCoroutine(MoveMonster());

        void MoveFlying()
        {
            Vector3 dist = transform.position - spawnerParent.transform.position;

            float minX = dist.x < -maxDistanceFromSpawner ? 0 : -maxDistanceFromSpawner,
                maxX = dist.x > maxDistanceFromSpawner ? 0 : maxDistanceFromSpawner,
                minY = dist.y < -maxDistanceFromSpawner ? 0 : -maxDistanceFromSpawner,
                maxY = dist.y > maxDistanceFromSpawner ? 0 : maxDistanceFromSpawner;

            _rb.velocity = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
                );
        }

        // The same as move flying just without Y
        void MoveGround()
        {
            Vector3 dist = transform.position - spawnerParent.transform.position;

            float minX = dist.x < -maxDistanceFromSpawner ? 0 : -maxDistanceFromSpawner,
                maxX = dist.x > maxDistanceFromSpawner ? 0 : maxDistanceFromSpawner;

            _rb.velocity = new Vector2(Random.Range(minX, maxX),0);
        }
    }

    /// <summary>
    /// When the player contacts this object, determine who's velocity was greater and reverse it
    /// </summary>
    /// <param name="collision">The object that collided with this</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string colName = collision.collider.name;
        if (colName.Equals(StaticStrings.playerName))
        {
            CollisionWithPlayer(collision);
        }
        else if (colName.Equals(StaticStrings.bulletName))
        {
            CollisionWithBullet(collision);
        }
    }

    /// <summary>
    /// Called if the monster wandered into a bullet
    /// </summary>
    /// <param name="collision"></param>
    void CollisionWithBullet(Collision2D collision)
    {
        //get the logic of the bullet
        BulletLogic bulletLogic = collision.gameObject.GetComponent<BulletLogic>();

        currentHealth -= bulletLogic.GetDamage();

        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
            spawnerParent.MonsterDeath(this);
        }

        //turn off the bullet so it can be reused
        collision.gameObject.SetActive(false);
    }

    /// <summary>
    /// A wild player has appeared, push him back
    /// </summary>
    /// <param name="collision"></param>
    void CollisionWithPlayer(Collision2D collision)
    {
        PlayerController.instance.PauseWalk(timeToWaitOnPush);

        if (isAttacking)
        {
            PlayerController.instance.TakeDamage(GetDamage());
            AttackOver(true);
            PushBack();
        }
        else
        {
            PushBack();
        }

        void PushBack()
        {
            //where is the player
            float dist = transform.position.x - collision.transform.position.x;

            //x < 0 when player on right
            if (dist > 0)
            {
                _rb.velocity = new Vector2(basePushForce, 0);
                collision.rigidbody.velocity = new Vector2(-basePushForce, 0);
            }
            else
            {
                _rb.velocity = new Vector2(-basePushForce, 0);
                collision.rigidbody.velocity = new Vector2(basePushForce, 0);
            }

            StartCoroutine(pauseWalk());
        }
    }

    IEnumerator pauseWalk()
    {
        canMove = false;

        yield return new WaitForSeconds(timeToWaitOnPush);

        canMove = true;
    }
}
