using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public interface IInteractable
{
    void Activate();
}

public class PlayerController : MonoBehaviour
{
    [Header("MechStats and Components")]
    public MechStatsSO mechStats;
    public GameObject playerMech;
    public FetchMechComponentsScript compFetch;
    public Transform torsoTrackingSphere;

    private Vector3 currentVelocity;

    [Header("Ground Settings")]
    public LayerMask groundMask;
    public float groundRayDistance = 3f;
    public Transform groundRayOriginPoint;

    [Header("Weapons")]
    public bool armLock = false;
    public bool elevatedAim = false;
    private enum WeaponElevation
    {
        groundLevel,
        eyeLevel,
        elevatedLevel
    }
    private WeaponElevation weaponElevation;

    public WeaponHandler weaponRHandler;
    public WeaponHandler weaponLHandler;

    [Header("Ability")]
    public float abilityCooldown = 30f;
    private float abilityCooldownCounter;
    public float abilityDuration = 12f;
    public bool abilityActive = false;

    private bool isRunning = false;
    private bool isAccelerated = false;

    private Rigidbody rigidBody;
    private Camera playerCamera;

    public IInteractable currentInteractable;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        playerCamera = Camera.main;

        if (mechStats == null)
        {
            int selectedMech = SelectionManager.Instance.selectedMech;
            mechStats = SelectionManager.Instance.mechSelections[selectedMech];
            if (mechStats != null)
            {
                DestructibleScript destructible = GetComponent<DestructibleScript>();
                destructible.SetStats(mechStats.maxHealth, mechStats.armor, false, false);
            }
        }
        if (playerMech == null)
        {
            Instantiate(mechStats.model, this.transform);
        }
        if (compFetch == null)
        {
            compFetch = GetComponentInChildren<FetchMechComponentsScript>();
        }
        if (compFetch.multiAC != null)
        {
            WeightedTransformArray sources = compFetch.multiAC.data.sourceObjects;

            sources.Add(new WeightedTransform(torsoTrackingSphere, 1f));

            compFetch.multiAC.data.sourceObjects = sources;

            compFetch.rig.Clear();
            compFetch.rig.Build();
        }

    }

    void Update()
    {
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
            currentInteractable.Activate();

        if (compFetch.animator.GetBool("IsStomping"))
            return;

        if (Input.GetKeyUp(KeyCode.Mouse2))
        {
            weaponElevation = (WeaponElevation)(((int)weaponElevation + 1) % 3);
        }

        if (Input.GetKeyDown(KeyCode.F))
            StartCoroutine(StompRoutine());

        HandleLegRotation();
        RotateTowardsMouse();
        HandleShooting();
        HandleRun();
    }

    void FixedUpdate()
    {
        if (compFetch.animator.GetBool("IsStomping"))
        {
            rigidBody.linearVelocity = Vector3.zero;
            return;
        }

        HandleMovement();
    }

    private IEnumerator StompRoutine()
    {
        rigidBody.linearVelocity = Vector3.zero;
        currentVelocity = Vector3.zero;
        compFetch.animator.SetBool("IsStomping", true);

        yield return new WaitForSeconds(1f);

        compFetch.animator.SetBool("IsStomping", false);
    }

    private void HandleShooting()
    {
        // RIGHT MOUSE – Weapon 1
        if (weaponRHandler != null)
        {
            var stats = weaponRHandler.weaponStats;

            if (stats.chargedWeapon)
            {
                if (Input.GetKey(KeyCode.Mouse1))
                {
                    weaponRHandler.TryFire();
                }

                if (Input.GetKeyUp(KeyCode.Mouse1))
                { 
                    weaponRHandler.ReleaseChargeShot();
                }
            }
            else if (Input.GetKey(KeyCode.Mouse1))
            {
                Debug.Log("attempting shot weapon1");
                weaponRHandler.TryFire();
            }

            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                weaponRHandler.StopFire();
            }
        }

        // LEFT MOUSE – Weapon 2
        if (weaponLHandler != null)
        {
            var stats = weaponLHandler.weaponStats;

            if (stats.chargedWeapon)
            {
                if (Input.GetKey(KeyCode.Mouse0))
                    weaponLHandler.TryFire();

                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    weaponLHandler.ReleaseChargeShot();
                }
            }
            else if (Input.GetKey(KeyCode.Mouse0))
            {
                Debug.Log("attempting shot weapon2");
                weaponLHandler.TryFire();
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                weaponLHandler.StopFire();
            }
        }
    }

    private void HandleRun()
    {
        isRunning = Input.GetKey(KeyCode.LeftShift);
        compFetch.animator.SetBool("IsRunning", isRunning);
    }

    /*public void Ability()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && Time.time >= abilityCooldownCounter && !abilityActive)
        {
            abilityActive = true;
            abilityCooldownCounter = Time.time + abilityCooldown;
            StartCoroutine(DeactivateAbility());
        }
    }

    private IEnumerator DeactivateAbility()
    {
        yield return new WaitForSeconds(abilityDuration);
        abilityActive = false;
    }*/

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 targetDirection = new Vector3(horizontal, 0, vertical).normalized;

        float targetSpeed = isRunning ? mechStats.runningSpeed : mechStats.walkingSpeed;

        if (targetDirection.magnitude > 0.1f)
        {
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                targetDirection * targetSpeed,
                mechStats.acceleartion * Time.fixedDeltaTime
            );
            compFetch.animator.SetBool("IsWalking", true);
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                Vector3.zero,
                mechStats.decelaration * Time.fixedDeltaTime
            );
            compFetch.animator.SetBool("IsWalking", false);
        }

        rigidBody.linearVelocity = new Vector3(currentVelocity.x, rigidBody.linearVelocity.y, currentVelocity.z);

        float currentSpeed = new Vector3(rigidBody.linearVelocity.x, 0, rigidBody.linearVelocity.z).magnitude;
        compFetch.animator.SetFloat("MoveSpeed", currentSpeed);

        isAccelerated = currentSpeed > mechStats.walkingSpeed;
    }

    private void HandleLegRotation()
    {
        Vector3 velocityDir = new Vector3(rigidBody.linearVelocity.x, 0, rigidBody.linearVelocity.z);
        if (velocityDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocityDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, mechStats.legRotationSpeed * Time.deltaTime);
        }
    }
    private void RotateTowardsMouse()
    {
        Ray cameraRay = playerCamera.ScreenPointToRay(Input.mousePosition);

        Plane aimPlane;

        switch (weaponElevation)
        {
            case WeaponElevation.groundLevel:
                aimPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
                break;

            case WeaponElevation.eyeLevel:
                aimPlane = new Plane(Vector3.up, new Vector3(0, compFetch.aimPivot.transform.position.y, 0));
                break;

            case WeaponElevation.elevatedLevel:
                aimPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y + 20, 0));
                break;

            default:
                aimPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
                break;
        }

        if (aimPlane.Raycast(cameraRay, out float rayLength))
        {
            Vector3 pointToLook = cameraRay.GetPoint(rayLength);

            bool lockY = (weaponElevation == WeaponElevation.eyeLevel);

            Vector3 aimTarget = lockY
                ? new Vector3(pointToLook.x, compFetch.aimPivot.transform.position.y, pointToLook.z)
                : pointToLook;

            float maxAngle = mechStats.weaponMaxRot;
            Vector3 forward = compFetch.aimPivot.transform.forward;

            // Weapon 1
            Vector3 aimDir1 = (aimTarget - compFetch.weaponR.transform.position).normalized;
            float angle1 = Vector3.Angle(forward, aimDir1);
            if (angle1 > maxAngle)
                aimDir1 = Vector3.RotateTowards(forward, aimDir1, Mathf.Deg2Rad * maxAngle, 0f);

            // Weapon 2
            Vector3 aimDir2 = (aimTarget - compFetch.weaponL.transform.position).normalized;
            float angle2 = Vector3.Angle(forward, aimDir2);
            if (angle2 > maxAngle)
                aimDir2 = Vector3.RotateTowards(forward, aimDir2, Mathf.Deg2Rad * maxAngle, 0f);

            compFetch.weaponR.transform.rotation = Quaternion.Slerp(
                compFetch.weaponR.transform.rotation,
                Quaternion.LookRotation(aimDir1),
                mechStats.weaponRotSpeed * Time.deltaTime
            );

            compFetch.weaponL.transform.rotation = Quaternion.Slerp(
                compFetch.weaponL.transform.rotation,
                Quaternion.LookRotation(aimDir2),
                mechStats.weaponRotSpeed * Time.deltaTime
            );
        }
    }
    public void AssignWeapon(WeaponHandler newWeapon)
    {
        if (weaponRHandler == null)
        {
            weaponRHandler = newWeapon;
        }
        else if (weaponLHandler == null)
        {
            weaponLHandler = newWeapon;
        }
    }
}
