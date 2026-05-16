using UnityEngine;

public class DiscoverableArea : MonoBehaviour
{
    [Header("Area Info")]
    [Tooltip("Name shown on screen when discovered, e.g. 'Old Dharahara'")]
    public string areaName = "Old Dharahara";

    [Header("Manager")]
    [Tooltip("Drag the DiscoveryManager GameObject here.")]
    public AreaDiscoveryManager manager;

    private bool _discovered = false;

    void OnTriggerEnter(Collider other)
    {
        if (_discovered || !other.CompareTag("Player")) return;

        _discovered = true;

        AreaDiscoveryManager mgr = manager != null ? manager : AreaDiscoveryManager.Instance;

        if (mgr != null)
            mgr.RegisterDiscovery(areaName);
        else
            Debug.LogWarning($"[DiscoverableArea] '{areaName}' could not find AreaDiscoveryManager.");
    }

    void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        if (col is SphereCollider sc)
            Gizmos.DrawWireSphere(transform.position, sc.radius);
        else
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}