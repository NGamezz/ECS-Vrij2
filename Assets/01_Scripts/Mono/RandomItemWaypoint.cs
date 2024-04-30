using UnityEngine;

public class RandomItemWaypoint : MonoBehaviour
{
    void Start()
    {
        var prefab = RandomItemManager.Instance.RequestRandomObject();
        var gameObject = Instantiate(prefab, transform);
        gameObject.SetActive(true);
        gameObject.transform.position = transform.position;
    }
}