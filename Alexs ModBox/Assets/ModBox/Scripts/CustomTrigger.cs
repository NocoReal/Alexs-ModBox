using UnityEngine;
using UnityEngine.Events;

public class CustomTrigger : MonoBehaviour
{
    public UnityEvent<Collider> TriggerEnter;
    public UnityEvent<Collider> TriggerExit;
    public UnityEvent<Collider> TriggerStay;

    void OnTriggerEnter(Collider other)
    {
        TriggerEnter.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        TriggerExit.Invoke(other);
    }
    private void OnTriggerStay(Collider other)
    {
        TriggerStay.Invoke(other);
    }
}

