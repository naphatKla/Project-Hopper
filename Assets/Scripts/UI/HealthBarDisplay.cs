using Characters.HealthSystems;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI
{
    public class HealthBarDisplay : MonoBehaviour
    {
        [Title("References")] 
        [SerializeField] private HealthSystem ownerHealthSystem;

        [Title("UI")] 
        [SerializeField] private MMProgressBar healthBar;
        
        void Start()
        {
            ownerHealthSystem.OnHealthChange += UpdateUI;
        }

        private void OnDestroy()
        {
            if (!ownerHealthSystem) return;
            ownerHealthSystem.OnHealthChange -= UpdateUI;
        }

        private void UpdateUI()
        {
            float progress = Mathf.Clamp01(ownerHealthSystem.CurrentHp / ownerHealthSystem.MaxHp);
            healthBar.UpdateBar01(progress);
        }
    }
}
