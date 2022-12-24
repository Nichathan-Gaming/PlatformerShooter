using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// A basic controller for the player, allows the player to move and shoot bullets
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    /// <summary>
    /// the animator attached to this
    /// </summary>
    Animator _anim;

    /// <summary>
    /// The rigidbody2D attached to this
    /// </summary>
    public Rigidbody2D _rigidbody2D;

    bool isTouchingGround;

    #region Shooting based variables
    [Header("Shooting based variables")]
    [SerializeField] bool _canFire = true;

    /// <summary>
    /// The bullet prefab
    /// </summary>
    [SerializeField] BulletLogic[] _bullets;

    /// <summary>
    /// The Instantiation location for the bullet
    /// </summary>
    [SerializeField] Transform _bulletTransform;

    /// <summary>
    /// The time before the player can fire again
    /// </summary>
    [SerializeField] float _cooldownTimer = 0.35f;

    bool isTriggerReleased = true;
    #endregion

    #region movement area
    public bool canMove = true;

    #region Horizontal Movement
    [Header("Horizontal Movement")]

    [SerializeField] float speedReduceOnWalkBackwards;

    /// <summary>
    /// -1: Left
    /// 0: No movement
    /// 1: Right
    /// </summary>
    int previousInputDirection;

    [SerializeField] float secondsForTap;

    #region Walk
    [Header("Walk")]
    /// <summary>
    /// What speed do we add to velocity, 
    /// should be the minimum that still allows movement based on mass
    /// </summary>
    [SerializeField] float baseWalkSpeed = 0.05f;

    /// <summary>
    /// The max speed that velocity can be set to
    /// </summary>
    [SerializeField] float maxWalkSpeed=5;
    #endregion

    #region HoverPack
    [Header("HoverPack area")]
    bool hasHoverPack,
        releasedKey;

    [SerializeField] GameObject hoverPackDisplay;

    bool isDashing,
        dashRight,
        dashLeft,
        dashReleased;

    [SerializeField] float dashDoubleClickTimer = 0.1f;

    [SerializeField] float dashSpeed;
    [SerializeField] float baseDashSpeed;
    [SerializeField] float currentDashDuration;
    [SerializeField] float maxDashDuration;

    [SerializeField] Image hoverFuelGaugeFill;
    [SerializeField] float maxHoverPower = 50;
    [SerializeField] float currentHoverPower = 50;
    [SerializeField] float hoverPowerLoss = 50;
    [SerializeField] float hoverPowerGain = 50;

    [SerializeField] float baseHoverSpeed;
    [SerializeField] float maxHoverSpeed;

    [SerializeField] float hoverHeight;
    [SerializeField] float hoverHeightIncrease = 1;
    [SerializeField] float hoverPower = 0.1962f;
    [SerializeField] float risingHoverPower=1f;
    #endregion

    #region Crouch
    [Header("Crouch area")]
    [SerializeField] float crouchWalkingSpeed;
    #endregion
    #endregion

    #region Vertical movement
    [Header("Vertical Movement")]
    /// <summary>
    /// Only true if the player is touching the ground and Jump is not pressed
    /// </summary>
    bool jumpReleased =true;

    /// <summary>
    /// The amount to increase vertical velocity on successful jump
    /// </summary>
    [SerializeField] float jumpSpeed;
    #endregion
    #endregion

    #region climbing area
    bool nearClimbingArea,
        isClimbing=false;

    [SerializeField] float gravityScale = 1;

    [SerializeField] float climbingSpeed = 5;
    #endregion

    #region Battle
    [SerializeField] float health = 100;
    #endregion

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

    /// <summary>
    /// Sets basic components
    /// </summary>
    private void Start()
    {
        Cursor.visible = false;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
    }

    /// <summary>
    /// Used to track player input
    /// </summary>
    private void Update()
    {
        //see if the player is trying to activate the hover pack
        CheckHoverPack();

        //see if the player should move
        CheckMovement();

        //see if the player should jump
        CheckJump();

        //see if the player can fire
        CheckFire();

        //ensure max speed is not exceeded
        if (isDashing)
        {
            if (_rigidbody2D.velocity.x > baseDashSpeed)
            {
                _rigidbody2D.velocity = new Vector2(baseDashSpeed, _rigidbody2D.velocity.y);
            }
            else if (_rigidbody2D.velocity.x < -baseDashSpeed)
            {
                _rigidbody2D.velocity = new Vector2(-baseDashSpeed, _rigidbody2D.velocity.y);
            }
        }
        else if (hasHoverPack)
        {
            if(_rigidbody2D.velocity.x > maxHoverSpeed)
            {
                _rigidbody2D.velocity = new Vector2(maxHoverSpeed, _rigidbody2D.velocity.y);
            }
            else if(_rigidbody2D.velocity.x < -maxHoverSpeed)
            {
                _rigidbody2D.velocity = new Vector2(-maxHoverSpeed, _rigidbody2D.velocity.y);
            }
        }
        else
        {
            if (_rigidbody2D.velocity.x > maxWalkSpeed)
            {
                _rigidbody2D.velocity = new Vector2(maxWalkSpeed, _rigidbody2D.velocity.y);
            }
            else if (_rigidbody2D.velocity.x < -maxWalkSpeed)
            {
                _rigidbody2D.velocity = new Vector2(-maxWalkSpeed, _rigidbody2D.velocity.y);
            }
        }
    }

    private void CheckHoverPack()
    {
        if (Input.GetAxis("HoverPack") > 0 && releasedKey)
        {
            hasHoverPack = !hasHoverPack;
            hoverPackDisplay.SetActive(hasHoverPack);
            releasedKey = false;

            if (hasHoverPack)
            {
                hoverHeight = transform.localPosition.y + hoverHeightIncrease;
            }
        }

        if (Input.GetAxis("HoverPack") ==0)
        {
            releasedKey = true;
        }

        if (currentHoverPower <= 0)
        {
            hasHoverPack = false;
            hoverPackDisplay.SetActive(false);
            isDashing = false;
        }
        else if (hasHoverPack)
        {
            currentHoverPower -= hoverPowerLoss;

            hoverFuelGaugeFill.fillAmount = currentHoverPower/ maxHoverPower;

            #region old code for future use
            //if (transform.localPosition.y < hoverHeight)
            //{
            //    _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, risingHoverPower);
            //}
            #endregion

            if (transform.localPosition.y != hoverHeight)
            {
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, hoverPower + (hoverPower * (hoverHeight - transform.localPosition.y)));
            }
            else
            {
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, hoverPower);
            }
        }
        
        if(currentHoverPower < maxHoverPower)
        {
            currentHoverPower += hoverPowerGain;

            if(currentHoverPower > maxHoverPower)
            {
                currentHoverPower = maxHoverPower;
            }

            hoverFuelGaugeFill.fillAmount = currentHoverPower / maxHoverPower;
        }
    }

    #region battle area
    /// <summary>
    /// Sets Game over
    /// </summary>
    void GameOver()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets damage to the player
    /// </summary>
    /// <param name="damage">The amount of damage</param>
    public void TakeDamage(float damage)
    {
        health -= damage;

        if(health <= 0)
        {
            GameOver();
        }
    }

    /// <summary>
    /// Check if the player can fire and if they are pressing Fire1
    /// </summary>
    void CheckFire()
    {
        if (Input.GetAxis("Fire1") > 0 && _canFire && isTriggerReleased)
        {
            isTriggerReleased = false;
            _canFire = false;
            CreateBullet();
        }
        else if(Input.GetAxis("Fire1") <= 0)
        {
            isTriggerReleased = true;
        }
    }

    /// <summary>
    /// shoots the first inactive bullet
    /// </summary>
    private void CreateBullet()
    {
        foreach(BulletLogic bullet in _bullets)
        {
            if (!bullet.gameObject.activeInHierarchy)
            {
                bullet.gameObject.SetActive(true);
                bullet.transform.position = _bulletTransform.position;
                bullet.transform.rotation = Quaternion.identity;
                bullet.Shoot(CameraFollowScript.instance._cam.ScreenToWorldPoint(Input.mousePosition));

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
        yield return new WaitForSeconds(_cooldownTimer);

        _canFire = true;
    }
    #endregion

    /// <summary>
    /// Should the player jump?
    /// </summary>
    private void CheckJump()
    {
        //if the player is not touching the ground, then return
        if (!isTouchingGround) return;

        //is jumping
        if (Input.GetAxis("Jump")>0 && jumpReleased)
        {
            jumpReleased = false;

            //is moving more than half max
            if (_rigidbody2D.velocity.x > maxWalkSpeed / 3 || _rigidbody2D.velocity.x < -maxWalkSpeed / 3)
            {
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, jumpSpeed);
            }
            else
            {
                _rigidbody2D.velocity = new Vector2(0, jumpSpeed);
            }
        }
        //wait for jump to be completely released while on ground
        else if (Input.GetAxis("Jump") == 0)
        {
            jumpReleased = true;
        }
    }

    /// <summary>
    /// Do not allow walk input for seconds
    ///     controlled by MonsterCollerScript
    /// </summary>
    /// <param name="timeToWaitOnPush">The time to wait</param>
    public void PauseWalk(float timeToWaitOnPush)
    {
        StartCoroutine(PauseWalk());

        IEnumerator PauseWalk()
        {
            canMove = false;

            yield return new WaitForSeconds(timeToWaitOnPush);

            canMove = true;
        }
    }

    /// <summary>
    /// Handles the player movement
    /// </summary>
    void CheckMovement()
    {
        //Get horizontal input
        float horizontalInput = Input.GetAxis("Horizontal");

        float verticalInput = Input.GetAxis("Vertical");

        _anim.SetBool("isWalking", _rigidbody2D.velocity.x > 0 || _rigidbody2D.velocity.y > 0);

        if (isDashing)
        {
            _rigidbody2D.velocity = new Vector2(dashSpeed, 0);

            currentDashDuration -= Time.deltaTime;

            if(currentDashDuration <= 0)
            {
                isDashing = false;
                _rigidbody2D.velocity = Vector2.zero;
            }

            _anim.SetBool("isWalking", false);
        }
        else if (hasHoverPack)
        {
            //detect if the player releases the horizontal key then presses it again
            if (horizontalInput == 0)
            {
                dashReleased = true;
            }
            else
            {
                if (dashReleased)
                {
                    dashReleased = false;

                    if (horizontalInput < 0 && transform.rotation.eulerAngles.y == 180)
                    {
                        //left
                        dashLeft = !dashLeft;

                        if (!dashLeft)
                        {
                            dashSpeed = -baseDashSpeed;
                            _rigidbody2D.velocity = new Vector2(dashSpeed, 0);
                            isDashing = true;
                            currentDashDuration = maxDashDuration;
                        }
                        else
                        {
                            StartCoroutine(TimeLeft());

                            IEnumerator TimeLeft()
                            {
                                yield return new WaitForSeconds(dashDoubleClickTimer);

                                dashLeft = false;
                            }
                        }
                    }
                    else if (horizontalInput > 0 && transform.rotation.eulerAngles.y == 0)
                    {
                        //right
                        dashRight = !dashRight;

                        if (!dashRight)
                        {
                            dashSpeed = baseDashSpeed;
                            _rigidbody2D.velocity = new Vector2(dashSpeed, 0);
                            isDashing = true;
                            currentDashDuration = maxDashDuration;
                        }
                        else
                        {
                            StartCoroutine(TimeRight());

                            IEnumerator TimeRight()
                            {
                                yield return new WaitForSeconds(dashDoubleClickTimer);

                                dashRight = false;
                            }
                        }
                    }
                }
            }

            float yVelo = _rigidbody2D.velocity.y;

            float speed = Mathf.Clamp(_rigidbody2D.velocity.x + (baseHoverSpeed * horizontalInput), -maxHoverSpeed, maxHoverSpeed);

            //player is moving backwards - slow down
            if ((horizontalInput > 0 && transform.rotation.eulerAngles.y == 180) || (horizontalInput < 0 && transform.rotation.eulerAngles.y == 0))
            {
                speed /= speedReduceOnWalkBackwards;
            }

            _rigidbody2D.velocity = new Vector2(speed, yVelo);

            _anim.SetBool("isWalking", false);
        }
        else if (isClimbing)
        {
            _rigidbody2D.velocity = new Vector2(5 * horizontalInput, verticalInput * climbingSpeed);
        }
        else if (verticalInput > 0 && nearClimbingArea && !isClimbing)
        {
            isClimbing = true;

            _rigidbody2D.velocity = new Vector2(0, verticalInput * climbingSpeed);
        }
        else if (isTouchingGround && canMove)
        {
            //the players final yVelocity
            float yVelo = _rigidbody2D.velocity.y;

            //adjust the players velocity : The players final horizontal velocity
            float speed = Mathf.Clamp(_rigidbody2D.velocity.x + (baseWalkSpeed * horizontalInput), -maxWalkSpeed, maxWalkSpeed);

            //player is moving backwards - slow down
            if ((horizontalInput > 0 && transform.rotation.eulerAngles.y == 180) || (horizontalInput < 0 && transform.rotation.eulerAngles.y == 0))
            {
                speed /= speedReduceOnWalkBackwards;
            }

            if (verticalInput < 0)
            {
                speed = crouchWalkingSpeed * horizontalInput;
            }

            //set the new velocity of the player
            _rigidbody2D.velocity = new Vector2(speed, yVelo);

            //Save the horizontal input
            previousInputDirection = (int)horizontalInput;
        }
    }

    #region handle collisions
    /// <summary>
    /// See what enters the collision
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string colName = collision.collider.name;
        if (colName.Equals(StaticStrings.groundName))
        {
            isTouchingGround = true;
            isClimbing = false;
        }
        else if (colName.Equals(StaticStrings.monsterBulletName))
        {
            CollisionWithBullet(collision);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals(StaticStrings.climbingAreaName))
        {
            nearClimbingArea = true;
        }
    }

    /// <summary>
    /// Called if the player wandered into a bullet
    /// </summary>
    /// <param name="collision"></param>
    void CollisionWithBullet(Collision2D collision)
    {
        //get the logic of the bullet
        BulletLogic bulletLogic = collision.gameObject.GetComponent<BulletLogic>();

        TakeDamage(bulletLogic.GetDamage());

        //turn off the bullet so it can be reused
        collision.gameObject.SetActive(false);
    }


    /// <summary>
    /// See what exits the collision
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.name.Equals(StaticStrings.groundName))
        {
            isTouchingGround = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals(StaticStrings.climbingAreaName))
        {
            nearClimbingArea = false;
            isClimbing = false;
        }
    }
    #endregion
}

/*OLD Code
    #region attached objects variables
    /// <summary>
    /// The move input received from an input device such as keyboard or controller
    /// </summary>
    Vector2 _moveInput;

    /// <summary>
    /// the rigidbody attached to this
    /// </summary>
    Rigidbody2D _rb2d;

    /// <summary>
    /// the rigidbody attached to this
    /// </summary>
    Animator _anim;

    /// <summary>
    /// the collider attached to this
    /// </summary>
    BoxCollider2D _collider;
    #endregion

    [Header("Movement based Variables")]
    #region Movement based Variables
    /// <summary>
    /// the force applied to the y of this RB when Jump is pressed
    /// </summary>
    [SerializeField] float _jumpForce = 20.0f;

    /// <summary>
    /// the force applied to the y of this RB while climbing
    /// </summary>
    [SerializeField] float _climbSpeed = 3.0f;

    /// <summary>
    /// the force applied to the X of this RB while moving left or right
    /// </summary>
    [SerializeField] float _moveSpeed = 2.5f;

    /// <summary>
    /// Unused
    /// </summary>
    //[SerializeField] float _runSpeed = 5f;
    #endregion

    [Header("Shooting based variables")]
    #region Shooting based variables
    /// <summary>
    /// The bullet prefab
    /// </summary>
    [SerializeField] GameObject _bullet;

    /// <summary>
    /// The Instantiation location for the bullet
    /// </summary>
    [SerializeField] Transform _bulletTransform;

    /// <summary>
    /// The object with the gun arm sprite
    /// </summary>
    [SerializeField] Transform _gunArm;

    /// <summary>
    /// The time before the player can fire again
    /// </summary>
    [SerializeField] float _cooldownTimer = 0.35f;

    /// <summary>
    /// Can the player fire again?
    /// </summary>
    bool _canFire = true;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // Gets a reference to various components on the player GameObject
        _rb2d = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _collider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Range of -1 to 1 used to scale movement
        _moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        //See if the player should move
        Run();

        //See if the player should jump
        OnJump();

        //See if the player should climb
        ClimbLadder();

        //See if the player is firing their weapon
        OnFire();
    }

    /// <summary>
    /// Add vertical velocity if the player is touching the ground and jumping
    /// </summary>
    void OnJump()
    {
        Debug.LogWarning("This allows continuous jumping while holding the jump key. If this is undesired behavior then check for Input=0 before allowing subsequent jumps");
        if (_collider.IsTouchingLayers(LayerMask.GetMask("Ground")) && Input.GetAxis("Jump") > 0)
        {
            _rb2d.velocity = new Vector2(_rb2d.velocity.x, _jumpForce);
        }
    }

    /// <summary>
    /// Fire a bullet when Fire1 is pressed
    /// </summary>
    void OnFire()
    {
        if (Input.GetAxis("Fire1")>0 && _canFire)
        {
            _canFire = false;
            CreateBullet();
        }
    }

    /// <summary>
    /// Manages the creation and reuse of bullets
    /// </summary>
    void CreateBullet()
    {
        Debug.LogWarning("How many bullets can be on the screen at once?\n" +
            "How far or long can a bullet travel?");

        Instantiate(_bullet, _bulletTransform.position, Quaternion.identity);
        StartCoroutine(WaitToFire());
    }

    /// <summary>
    /// Waits _cooldownTimer seconds before allowing the player to fire again
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitToFire()
    {
        yield return new WaitForSeconds(_cooldownTimer);

        _canFire = true;
    }

    /// <summary>
    /// Handles the actual movement across the screen while on the ground
    /// </summary>
    void Run()
    {
        // Multiplies the input vector by the moveSpeed variable and disable the movement on the y axis
        Vector2 playerVelocity = new Vector2(_moveInput.x * _moveSpeed, _rb2d.velocity.y);
        _rb2d.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(_rb2d.velocity.x) > Mathf.Epsilon;
        // Get the isWalking bool and set it to true if there is movement, false otherwise
        _anim.SetBool("isWalking", playerHasHorizontalSpeed);

        if(playerHasHorizontalSpeed) FlipSprite();
    }

    /// <summary>
    /// Sets the rotation from 0 to -180 if moving and not already facing that direction
    ///     Called in Run
    /// </summary>
    void FlipSprite()
    {
        //is the player moving left but facing right?
        if (_rb2d.velocity.x < 0 && transform.rotation.y == 0)
        {
            transform.Rotate(new Vector3(0, 180));
            _gunArm.localScale = new Vector3(1, -1, 1);
        }
        //is the player moving right but not facing right?
        else if (_rb2d.velocity.x > 0 && transform.rotation.y != 0)
        {
            transform.rotation = Quaternion.identity;
            _gunArm.localScale = new Vector3(1, 1, 1);
        }
    }

    /// <summary>
    /// Should the player climb the ladder or not
    ///     if so, up or down
    /// </summary>
    void ClimbLadder()
    {
        //is the player touching a climbing object?
        if(_collider.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            //the climbing velocity
            Vector2 climbVelocity = new Vector2(_rb2d.velocity.x, _moveInput.y * _climbSpeed);

            //add to the velocity
            _rb2d.velocity = climbVelocity;
        }
    }
 */