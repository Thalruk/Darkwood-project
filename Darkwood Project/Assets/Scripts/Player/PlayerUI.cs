using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Player Resources")]
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerHealth health;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Slider healthSlider;

    void Update()
    {
        if (combat != null && ammoText != null)
        {
            ammoText.text = $"{combat.GetAmmoInClip()} / {combat.GetMaxAmmo()}";
        }

        if (health != null && healthSlider != null)
        {
            healthSlider.value = (float)health.currentHealth / health.maxHealth;
        }
    }
}