using UnityEngine;

namespace Platform
{
    [CreateAssetMenu(fileName = "TNTState", menuName = "PlatformStates/TNT")]
    public class PlatformTNTStateSO : PlatformBaseStateSO
    {
        [SerializeField] private float explosionOffset;
        
        public override string StateID => "TNT";
        public override void UpdateState(PlatformManager manager) { }

        public override async void OnStepped(PlatformManager manager, GameObject player)
        {
            await manager.BlinkColor(Color.white, Color.red, 0.66f, 3);
            
            //Explosion
            Collider2D[] hits = Physics2D.OverlapCircleAll(manager.transform.position + Vector3.down * explosionOffset, 1f);
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Platform"))
                {
                    hit.gameObject.SetActive(false);
                }
            }
            
            manager.PlayAndDestroyParticleAsync(manager.feedback, manager.transform.position + Vector3.down * 0.5f);
            manager.gameObject.SetActive(false);
        }
        
        public override void OnSpawned(PlatformManager manager)
        {
            manager.ResetPlatform();
        }

        public override void OnDespawned(PlatformManager manager)
        {
            manager.ResetPlatform();
        }
    }
}
