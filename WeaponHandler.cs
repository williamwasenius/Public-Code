using AudioSystem;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponHandler : MonoBehaviour
{
    [Header("Weapon Variants")]
    public WeaponStatsSO[] weaponVariants;
    public bool altWeapon = false;

    [Header("Weapon Information")]
    public WeaponStatsSO weaponStats;
    public Transform firePoint;
    public Transform barrel;
    private VisualEffect firePointVFX;

    private bool isFiring;
    private bool isDisabled;

    private float cooldownTimer = 0f;

    [Header("Charge Weapon Specific")]
    public VisualEffect chargeProgressVFX;
    private float chargeProgress = 0f;
    private bool isCharging = false;
    private bool isCharged = false;
    private AudioSource chargeAudioSource;

    [Header("Weapon Heat Specific")]
    public float currentHeat;

    [Header("Weapon Ramp Specific")]
    private float currentRoF;

    [Header("Shooter")]
    public PlayerController player;
    public GameObject shooter;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>(); 
        if (player != null) 
        {
            Debug.Log("player found");
            player.AssignWeapon(this); 
        }
    }

    private void Start()
    {
        if (altWeapon && weaponVariants.Length > 1 && weaponVariants != null)
            {
                weaponStats = weaponVariants[1];
            }
        else if (weaponVariants.Length == 1)
            { 
            weaponStats = weaponVariants[0]; 
        }
        else
        {

        }

        shooter = transform.root.gameObject; 
        firePointVFX = firePoint.GetComponent<VisualEffect>();
        currentRoF = weaponStats.startFireRate;
    }

    private void Update()
    {

     if (isFiring && weaponStats.rotatingBarrel)
        {
            rotateWeapons();
        }

    }

    private void FixedUpdate()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (!isFiring)
        {
            DecreaseHeat();
            currentRoF = Mathf.MoveTowards(currentRoF, weaponStats.startFireRate, weaponStats.rampSpeed * Time.deltaTime);
        }
        else 
        {
            if (weaponStats.rampingFirerate)
            {
                RampUp();
            }
        }
    }

    public void TryFire()
    {
        if (!isDisabled)
        {        
            if (weaponStats.chargedWeapon)
            {
                ChargeWeapon();
                return;
            }

            if (cooldownTimer <= 0)
            {
                Fire();
            }
        }
    }
    public void StopFire()
    {
        isFiring = false;
    }

    private void ChargeWeapon()
    {
        if (!isCharging)
        {
            isCharging = true;
            chargeProgress = 0f;

            if (weaponStats.chargeSound != null)
            {
                chargeAudioSource = AudioManager.Instance.Play(weaponStats.chargeSound.id, transform.position);
            }
            if (chargeProgressVFX != null)
                chargeProgressVFX.Play();

        }

        chargeProgress += Time.deltaTime;

        if (chargeProgress >= weaponStats.chargeTime)
        {
            isCharged = true;
        }
    }

    public void ReleaseChargeShot()
    {
        if (!weaponStats.chargedWeapon)
            return;

        if (isCharged)
        {
            Fire();
            ChargeShootConclusion();
        }
        else
        {
            ChargeShootConclusion();

        }
    }

    public void ChargeShootConclusion()
    {
        if (chargeAudioSource != null)
        {
            chargeAudioSource.Stop();
            Destroy(chargeAudioSource.gameObject);
            chargeAudioSource = null;
        }

        if (chargeProgressVFX != null)
            chargeProgressVFX.Stop();

        isCharging = false;
        isCharged = false;
        chargeProgress = 0f;

        cooldownTimer = weaponStats.fireRate;
    }

    public void rotateWeapons()
    {
        barrel.Rotate(Vector3.forward * weaponStats.rotationSpeed * Time.deltaTime);
    }

    private void Fire()
    {
        isFiring = true;

        ProjectileStatsSO projectileStats = weaponStats.projectileStats;
        GameObject projectile;

        if (ProjectilePoolerScript.Instance != null)
        {
            projectile = ProjectilePoolerScript.Instance.Spawn(projectileStats.projectilePrefab, firePoint.position, firePoint.rotation);
        }
        else
        {
            projectile = Instantiate(projectileStats.projectilePrefab, firePoint.position, firePoint.rotation);
        }

        ProjectileLogic projectileLogic = projectile.GetComponent<ProjectileLogic>();
        projectileLogic.shooter = shooter;
        projectileLogic.projectileStats = projectileStats;

        if (firePointVFX != null)
        {
            firePointVFX.Play();
        }

        if (weaponStats.shootSound != null)
        {
            AudioManager.Instance.Play(weaponStats.shootSound.id, transform.position);
        }

        cooldownTimer = weaponStats.rampingFirerate ? currentRoF : weaponStats.fireRate;

        if (weaponStats.buildsHeat)
        {
            IncreaseHeat();
        }
        
    }

    public float GetCooldownProgress()
    {
        if (weaponStats.chargedWeapon)
        {
            return Mathf.Clamp01(chargeProgress / weaponStats.chargeTime);
        }
        else
        {
            return 1f - Mathf.Clamp01(cooldownTimer / weaponStats.fireRate);
        }
    }

    public void RampUp()
    {
        currentRoF = Mathf.MoveTowards(currentRoF, weaponStats.fireRate, weaponStats.rampSpeed * Time.deltaTime);
    }

    public void IncreaseHeat()
    {
        currentHeat += weaponStats.heatPerShot;
        if (currentHeat >= weaponStats.maxHeat)
        {
            isDisabled = true;
        }
    }

    public void DecreaseHeat()
    {
        float coolRate = weaponStats.maxHeat / weaponStats.maxCooldownDuration;

        if (currentHeat > 0)
        {
            currentHeat -= coolRate * Time.deltaTime;
        }
        if (currentHeat <= 0)
        {
            currentHeat = 0;
            isDisabled = false;
        }
    }

}
