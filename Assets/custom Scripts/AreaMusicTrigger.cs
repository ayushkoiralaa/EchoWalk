using UnityEngine;

public class AreaMusicTrigger : MonoBehaviour
{
    [Tooltip("Must exactly match the Area Name in MusicManager's Area Tracks list.")]
    public string areaName;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayAreaMusic(areaName);
    }
}
