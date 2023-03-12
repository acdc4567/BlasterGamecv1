using UnityEngine;
using System.Collections;

public class ItemsBase : MonoBehaviour {

    public ItemType itemType;

    public enum ItemType
    {
        weapon,
        health,
        ammo,
        etc
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.GetComponent<ItemPickupBehaviour>())
        {
            other.transform.GetComponent<ItemPickupBehaviour>().itemToPickup = this;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform.GetComponent<ItemPickupBehaviour>())
        {
            other.transform.GetComponent<ItemPickupBehaviour>().itemToPickup = null;
        }
    }
}
