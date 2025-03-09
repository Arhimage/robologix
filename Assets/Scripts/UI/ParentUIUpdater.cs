using UnityEngine;
using UnityEngine.Events;

public class ParentUIUpdater : MonoBehaviour
{
    public UnityEvent OnUIUpdated;

    public void UpdateUI()
    {
        OnUIUpdated?.Invoke();
    }
}