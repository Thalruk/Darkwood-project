using NavMeshPlus.Components;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] NavMeshSurface navMeshSurface;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        UpdateNavMeshDelayed();
    }

    public void UpdateNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
    }

    public void UpdateNavMeshDelayed()
    {
        Invoke(nameof(UpdateNavMesh), 0.1f);
    }
}
