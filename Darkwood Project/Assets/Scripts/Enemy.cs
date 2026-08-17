using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public int health = 3;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
    private float nextAttackTime = 0f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("Kloc dotyka obiektu: " + collision.gameObject.name); // DODAJ TYLKO TÊ LINIJKÊ

        if (Time.time >= nextAttackTime)
        {
            if (collision.gameObject.TryGetComponent(out PlayerHealth playerHP))
            {
                playerHP.TakeDamage(25);
                nextAttackTime = Time.time + 1f;
            }
        }
    }
}