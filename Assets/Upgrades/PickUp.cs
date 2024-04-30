using System;
using UnityEngine;
    public interface IPickup
    {
        public event Action OnPickup;
        public void PickUp(GameObject obj);
    }

