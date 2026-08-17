using Cinemachine;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerCombat : MonoBehaviour
{
    [Header("Flashlight")]
    [SerializeField] Light2D flashlight;
    [SerializeField] float normalAngle = 70f;
    [SerializeField] float aimAngle = 30f;
    [SerializeField] float lightLerpSpeed = 8f;

    [Header("Gun")]
    [SerializeField] int ammoInClip = 6;
    [SerializeField] int maxAmmo = 24;
    [SerializeField] float reloadTime = 1.5f;
    [SerializeField] float fireRate = 0.5f;
    [SerializeField] float damage = 1;
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip emptySound;
    [SerializeField] AudioClip reloadSound;
    [SerializeField] AudioSource audioSource;

    [Header("Gun light")]
    [SerializeField] Light2D gunLight;
    [SerializeField] float flashFadeSpeed = 50f;
    [SerializeField] float gunFalloff = 0.75f;
    [SerializeField] float gunIntensity = 1;

    [Header("Shooting Setup")]
    [SerializeField] Transform firePoint;
    [SerializeField] float weaponRange = 25f;
    [SerializeField] LayerMask hitLayers;
    [SerializeField] float recoilForce;

    [Header("Camera Shake")]
    [SerializeField] CinemachineImpulseSource impulseSource;
    public event Action<int, int> OnAmmoChanged;
    private float nextFireTime = 0f;
    private bool isReloading = false;


    private void Start()
    {
        OnAmmoChanged?.Invoke(ammoInClip, maxAmmo);
    }
    public void HandleCombatLogic(bool isAiming, bool shootInput, bool reloadInput)
    {
        if (reloadInput && ammoInClip < 6 && maxAmmo > 0 && !isReloading)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        if (shootInput && isAiming && !isReloading)
        {
            if (flashlight != null && Mathf.Abs(flashlight.pointLightOuterAngle - aimAngle) < 1f)
            {
                if (Time.time >= nextFireTime)
                {
                    if (ammoInClip > 0)
                    {
                        Shoot();
                    }
                    else
                    {
                        nextFireTime = Time.time + 0.2f;

                        if (audioSource != null && emptySound != null)
                        {
                            audioSource.PlayOneShot(emptySound);
                        }
                    }
                }
            }
        }
    }

    private void Shoot()
    {
        ammoInClip--;
        OnAmmoChanged?.Invoke(ammoInClip, maxAmmo);
        nextFireTime = Time.time + fireRate;

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        if (gunLight != null)
        {
            gunLight.enabled = true;
            gunLight.intensity = gunIntensity;
            gunLight.pointLightOuterRadius = 15f;
            gunLight.falloffIntensity = gunFalloff;
        }

        if (impulseSource != null)
        {
            impulseSource.GenerateImpulseWithVelocity(-transform.up * recoilForce);
        }

        Vector2 startPos = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 direction = transform.up;

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, weaponRange, hitLayers);

        if (hit.collider != null)
        {
            Debug.DrawLine(startPos, hit.point, Color.red, 2f);

            Enemy target = hit.collider.GetComponent<Enemy>();
            if (target != null)
            {
                target.TakeDamage((int)damage);
            }
        }
        else
        {
            Debug.DrawLine(startPos, startPos + (direction * weaponRange), Color.gray, 2f);
        }
    }

    public void UpdateVisuals(bool isAiming)
    {
        if (flashlight != null)
        {
            float targetOuterAngle = isAiming ? aimAngle : normalAngle;
            float targetInnerAngle = Mathf.Max(0f, targetOuterAngle);

            flashlight.pointLightInnerAngle = Mathf.Lerp(flashlight.pointLightInnerAngle, targetInnerAngle, Time.deltaTime * lightLerpSpeed);
            flashlight.pointLightOuterAngle = Mathf.Lerp(flashlight.pointLightOuterAngle, targetOuterAngle, Time.deltaTime * lightLerpSpeed);
        }

        if (gunLight != null && gunLight.intensity > 0)
        {
            gunLight.intensity -= flashFadeSpeed * Time.deltaTime;
            if (gunLight.intensity < 0.01f)
            {
                gunLight.intensity = 0f;
                gunLight.enabled = false;
            }
        }
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

        int bulletsNeeded = 6 - ammoInClip;
        int bulletsToLoad = Mathf.Min(bulletsNeeded, maxAmmo);

        ammoInClip += bulletsToLoad;
        maxAmmo -= bulletsToLoad;
        OnAmmoChanged?.Invoke(ammoInClip, maxAmmo);
        isReloading = false;
    }

    public int GetAmmoInClip() => ammoInClip;
    public int GetMaxAmmo() => maxAmmo;
}