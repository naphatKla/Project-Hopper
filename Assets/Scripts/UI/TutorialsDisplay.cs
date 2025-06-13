using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TutorialsDisplay : MonoBehaviour
    {
        [SerializeField] private List<Image> tutorialsElements;
        private static bool hasPlayOne;
        
        private void Awake()
        {
            if (hasPlayOne) return;
            ToggleUI(true);
        }

        private void Update()
        {
            if (hasPlayOne) return;
            if (!Input.anyKeyDown) return;
            ToggleUI(false);
        }

        private void ToggleUI(bool enable)
        {
            foreach (var element in tutorialsElements)
                element.gameObject.SetActive(enable);
            
            if (!enable)
            {
                hasPlayOne = true;
                Time.timeScale = 1;
                return;
            }

            Time.timeScale = 0;
        }
    }
}
