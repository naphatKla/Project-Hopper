using System.Collections.Generic;
using Characters.HealthSystems;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HealthDisplayUI : MonoBehaviour
    {
        [Title("References")] 
        [SerializeField] private HealthSystem ownerHealthSystem;
        
        [Title("UI")]
        [SerializeField] private Transform healthUIParent;
        [SerializeField] private Image healthUI;

        private List<Image> _healthUIList = new List<Image>();
        
        
        void Start()
        {
            InstantiateUI();
            ownerHealthSystem.OnHealthChange += UpdateUI;
        }

        private void OnDestroy()
        {
            if (!ownerHealthSystem) return;
            ownerHealthSystem.OnHealthChange -= UpdateUI;
        }

        private void InstantiateUI()
        {
            if (_healthUIList.Count >= ownerHealthSystem.MaxHp) return;
            int amountToInit = (int)ownerHealthSystem.MaxHp - _healthUIList.Count;
            
            for (int i = 0; i < amountToInit; i++)
            {
                _healthUIList.Add(Instantiate(healthUI, healthUIParent));
                _healthUIList[i].gameObject.SetActive(true);
            }
        }
        
        private void UpdateUI()
        {
            InstantiateUI();
            
            for (int i = 0; i < _healthUIList.Count; i++)
                _healthUIList[i].gameObject.SetActive(i < ownerHealthSystem.CurrentHp);
        }
    }
}
