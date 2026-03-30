using Cinemachine;
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

    public void HandleCombatLogic(bool isAiming)
    {
        if (Input.GetButtonDown("Fire1") && isAiming)
        {
            if (flashlight != null && Mathf.Abs(flashlight.pointLightOuterAngle - aimAngle) < 1f)
            {
                Shoot();
            }
        }
    }

    private void Shoot()
    {
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
            Debug.Log($"Hit: {hit.collider.name}");

            Debug.DrawLine(startPos, hit.point, Color.red, 2f);
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
}