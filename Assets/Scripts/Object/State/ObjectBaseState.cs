using ObjectItem;
using UnityEngine;

namespace ObjectItem
{
    public abstract class ObjectBaseState : MonoBehaviour
    {
        public abstract string StateID { get; }
        public abstract void OnSpawned(ObjectManager manager);
        public abstract void OnDespawned(ObjectManager manager);
        public abstract void UpdateState(ObjectManager manager);
        public abstract void OnTriggerEnterObject(Collider2D other, ObjectManager manager);
    }
}