using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float speed = 2f;
    private Rigidbody2D rb;
    private Animator playerAnimator;

    public float currentXRamp;
    public float currentYRamp;
    public float rampCountFactor = 1f;
    public float rampUpTimeHoriz = 1f;
    public float rampUpTimeVert = 1f;
    public bool isMoving;
    public bool useAnimator = false;
    public bool lerpTurnRate;
    public float trackingTurnRate = 2f;
    public float aimAngle;
    private float lastAimAngle;
    public Vector2 direction;

    public Transform gunPoint;
    public bool weaponRelativeToComponent;
    public float bulletSpeed = 12f;
    public float weaponFireRate = 0.07f;
    public float bulletRandomness = 0f;
    public float AimAngle;

    private float inputX;
    private float inputY;
    private bool fire;
    private bool shoulderButtonLeft;
    private bool shoulderButtonRight;

    private Vector2 facingDirection;
    private float coolDown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); 
        if (useAnimator)
            playerAnimator = GetComponent<Animator>();
    }

    void Start ()
    {
        currentXRamp = 0f;
        currentYRamp = 0f;
    }

    Vector3 AngleLerp(Vector3 StartAngle, Vector3 FinishAngle, float t)
    {
        float xLerp = Mathf.LerpAngle(StartAngle.x, FinishAngle.x, t);
        float yLerp = Mathf.LerpAngle(StartAngle.y, FinishAngle.y, t);
        float zLerp = Mathf.LerpAngle(StartAngle.z, FinishAngle.z, t);
        Vector3 Lerped = new Vector3(xLerp, yLerp, zLerp);
        return Lerped;
    }

    void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
        fire = Input.GetButton("Fire");
        shoulderButtonLeft = Input.GetButton("LeftShoulder");
        shoulderButtonRight = Input.GetButton("RightShoulder");

        var posInputX = Mathf.Abs(inputX);
        var posInputY = Mathf.Abs(inputY);

    if (posInputX > 0.2f || posInputY > 0.2f)
        {
            if (posInputX > 0f && currentXRamp < rampUpTimeHoriz)
            {
                currentXRamp += rampCountFactor * Time.deltaTime;
            }

            if (posInputY > 0f && currentYRamp < rampUpTimeVert)
            {
                currentYRamp += rampCountFactor * Time.deltaTime;
            }

            isMoving = true;
            if (useAnimator && playerAnimator != null)
            {
                playerAnimator.SetBool("IsMoving", true);
            }
        }
        else
        {
            if (currentXRamp > 0f)
            {
                currentXRamp -= rampCountFactor * Time.deltaTime;
            }

            if (currentYRamp > 0f)
            {
                currentYRamp -= rampCountFactor * Time.deltaTime;
            }

            isMoving = false;
            if (useAnimator && playerAnimator != null)
            {
                playerAnimator.SetBool("IsMoving", false);
            }
        }

    // Gives the player body a velocity from input
    rb.velocity = new Vector2(inputX * speed, inputY * speed);

    // Checks if player is moving
    if(isMoving)
        direction = rb.velocity.normalized;

        if (lerpTurnRate && isMoving)
        {
            var wantedAimAngle = Mathf.Atan2(direction.y, direction.x);
            aimAngle = wantedAimAngle;
        }
        else
        {
            if (!lerpTurnRate && isMoving)
            {
                aimAngle = Mathf.Atan2(direction.y, direction.x);
                if (aimAngle < 0f)
                {
                    aimAngle = Mathf.PI * 2 + aimAngle;
                }
            }
            
        }

        if (isMoving)
            lastAimAngle = aimAngle;

        var aimAngleDegrees = (aimAngle * Mathf.Rad2Deg);
        if (lerpTurnRate && isMoving)
        {
            var t = Time.deltaTime * trackingTurnRate;
            float rot = Mathf.LerpAngle(transform.eulerAngles.z, aimAngleDegrees, t);
            transform.rotation = Quaternion.Euler(0, 0, rot);
        }
        else
        {
            transform.eulerAngles = new Vector3(0.0f, 0.0f, aimAngleDegrees);
        }

        if (rampUpTimeHoriz < 0f) rampUpTimeHoriz = 0f;
        if (rampUpTimeVert < 0f) rampUpTimeVert = 0f;

        //CalculateAimAndFacingAngles(facingDirection);
        HandleShooting();

    }

    private void HandleShooting()
    {
        coolDown -= Time.deltaTime;

        if(Input.GetButton("Fire") || Input.GetButton("LeftShoulder") || Input.GetButton("RightShoulder"))
        {
            ShootWithCoolDown();
        }
    }

    private void ShootWithCoolDown()
    {
        if (coolDown <= 0f)
        {
            ProcessBullets();
            coolDown = weaponFireRate;
        }
    }

    private void ProcessBullets()
    {
        var bulletSpreadInitial = 0f;
        var bulletSpacingInitial = 0f;
        var bulletSpreadIncrement = 0f;
        var bulletSpacingIncrement = 0f;

        var bullet = GetBulletFromPool();
        var bulletComponent = (Bullet)bullet.GetComponent(typeof(Bullet));


        var offsetX = Mathf.Cos(aimAngle - Mathf.PI / 2) * (bulletSpacingInitial - 0f * bulletSpacingIncrement);
        var offsetY = Mathf.Sin(aimAngle - Mathf.PI / 2) * (bulletSpacingInitial - 0f * bulletSpacingIncrement);

        bulletComponent.directionAngle = aimAngle + bulletSpreadInitial + 0f * bulletSpreadIncrement;

        bulletComponent.speed = bulletSpeed;

        // Setup the point at which bullets need to be placed based on all the parameters
        var initialPosition = gunPoint.position + (gunPoint.transform.forward * (bulletSpacingInitial - 0f * bulletSpacingIncrement));
        var bulletPosition = new Vector3(initialPosition.x + offsetX + Random.Range(0f, 1f) * bulletRandomness - bulletRandomness / 2,
            initialPosition.y + offsetY + Random.Range(0f, 1f) * bulletRandomness - bulletRandomness / 2, 0f);

        bullet.transform.position = bulletPosition;

        bulletComponent.bulletXPosition = bullet.transform.position.x;
        bulletComponent.bulletYPosition = bullet.transform.position.y;

        // Activate the bullet to get it going
        bullet.SetActive(true);
    }

    private GameObject GetBulletFromPool()
    {
        return ObjectPoolManager.instance.GetUsableBeam2Bullet();
    }

    private void CalculateAimAndFacingAngles(Vector2 facingDirection)
    {
        aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
        if (aimAngle < 0f)
        {
            aimAngle = Mathf.PI * 2 + aimAngle;
        }

        // Rotate the GameObject to face the direction of the mouse cursor (the object with the weaponsystem component attached, or, if the weapon configuration specifies relative to the gunpoint, rotate the gunpoint instead.
        if (weaponRelativeToComponent)
        {
            gunPoint.transform.eulerAngles = new Vector3(0.0f, 0.0f, aimAngle * Mathf.Rad2Deg);
        }
        else
        {
            transform.eulerAngles = new Vector3(0.0f, 0.0f, aimAngle * Mathf.Rad2Deg);
        }
    }

}
