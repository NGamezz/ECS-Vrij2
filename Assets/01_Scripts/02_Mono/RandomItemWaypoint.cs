using UnityEngine;

public class RandomItemWaypoint : MonoBehaviour
{
    [SerializeField] private RandomItemType randomItemType;

    void Start()
    {
        var prefab = RandomItemManager.Instance.RequestRandomObject(randomItemType);
        var gameObject = Instantiate(prefab, transform);
        gameObject.SetActive(true);
        gameObject.transform.position = transform.position;
    }
}