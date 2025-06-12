using Characters.Controllers;
using Characters.HealthSystems;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ObjectItem
{
    public class ObjectTreasure : ObjectBaseState
    {
        public override string StateID { get; }
        [SerializeField] private float minDropAmount;
        [SerializeField] private float maxDropAmount;
        private HealthSystem _healthSystem;

        public override void OnSpawned(ObjectManager manager)
        {
            _healthSystem = manager.GetComponent<HealthSystem>();
            _healthSystem.OnDead.AddListener(() => DropCoin(manager));
        }

        public override void OnDespawned(ObjectManager manager)
        {
            _healthSystem.OnDead.RemoveListener(() => DropCoin(manager));
        }

        public override void UpdateState(ObjectManager manager) { }

        public override void OnTriggerEnterObject(Collider2D other, ObjectManager manager) { }

        private void DropCoin(ObjectManager manager)
        {
            Vector3 originPos = manager.transform.position;

            //Potion drop if health < 33%
            if (PlayerController.Instance.HealthSystem.CurrentHp == 1)
                SpawnerController.Instance.Spawn("Potion", manager.transform.position);
            //Coin drop if health > 33%
            else
            {
                var dropCount = Random.Range(Mathf.CeilToInt(minDropAmount), Mathf.FloorToInt(maxDropAmount) + 1);
                for (var i = 0; i < dropCount; i++)
                {
                    var coin = SpawnerController.Instance.Spawn("Coin", manager.transform.position);

                    if (coin == null) continue;
                    var offsetX = Random.Range(-1.5f, 1.5f);

                    var targetPos = originPos + new Vector3(offsetX, 0, 0);
                    coin.transform.DOJump(targetPos, 1f, 1, 1f);
                }
            }
        }
    }
}
