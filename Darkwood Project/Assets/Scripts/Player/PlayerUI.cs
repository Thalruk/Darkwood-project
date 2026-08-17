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

    private void OnEnable()
    {
        if (health != null) health.OnHealthChanged += UpdateHealthUI;
        if (combat != null) combat.OnAmmoChanged += UpdateAmmoUI;
    }

    private void OnDisable()
    {
        if (health != null) health.OnHealthChanged -= UpdateHealthUI;
        if (combat != null) combat.OnAmmoChanged -= UpdateAmmoUI;
    }

    private void UpdateHealthUI(int currentHp, int maxHp)
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHp / maxHp;
        }
    }

    private void UpdateAmmoUI(int currentAmmo, int maxAmmoAmount)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {maxAmmoAmount}";
        }
    }
}