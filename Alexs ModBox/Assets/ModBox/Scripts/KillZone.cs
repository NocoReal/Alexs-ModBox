using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private Vector3 PlayerStartPos;
    [SerializeField] private GameObject player;
    private PlayerToolSpawn PTS;

    private void Awake()
    {
        PTS = player.GetComponent<PlayerToolSpawn>();
    }

    public void KillObject(Collider col)
    {
        GameObject obj = col.gameObject;
        if (obj.CompareTag("Player"))
        {
            obj.transform.position = PlayerStartPos;
            obj.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }
        else
        {
            PTS.KillZoneDestroy(obj);
            Destroy(obj);
        }
    }
}
