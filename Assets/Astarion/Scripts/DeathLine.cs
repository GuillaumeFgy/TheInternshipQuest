using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DeathLineReload2D : MonoBehaviour
{
    [SerializeField] private Transform playerRoot; // e.g., your "motorbike" object
    [SerializeField] private Menu menu;
    [SerializeField] private string type;
    bool reloading;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (reloading) return;
        if (!BelongsToPlayer(other)) return;

        reloading = true;
        menu.ShowMenu(type);
    }

    bool BelongsToPlayer(Collider2D col)
    {
        if (!playerRoot) return false;
        if (col.transform == playerRoot) return true;
        if (col.attachedRigidbody && col.attachedRigidbody.transform.IsChildOf(playerRoot)) return true;
        return col.transform.IsChildOf(playerRoot);
    }
}
