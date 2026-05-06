using UnityEngine;

public class Destructible : MonoBehaviour
{
    public int health = 50;

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            gameObject.SetActive(false);

            GameManager.Instance.UpdateNavMeshDelayed();

            Destroy(gameObject);
        }
    }
}