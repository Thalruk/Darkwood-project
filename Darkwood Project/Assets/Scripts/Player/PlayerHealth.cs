using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth { get; private set; }
    public event Action<int, int> OnHealthChanged;
    private void Awake()
    {
        currentHealth = maxHealth;
    }
    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}