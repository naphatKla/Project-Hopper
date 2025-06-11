using System.Collections.Generic;
using Characters.Controllers;
using Dan.Main;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LeaderboardDisplay : MonoBehaviour
    {
        [SerializeField] private LeaderboardElement elementPrefab;
        [SerializeField] private Transform leaderBoardParent;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Button playButton;
        
        [SerializeField] private int maxDisplayAmount = 10;
        private readonly List<LeaderboardElement> elementList = new List<LeaderboardElement>();
        private bool _isInitialized;

        private void Start()
        {
            playButton.gameObject.SetActive(false);
            nameInput.gameObject.SetActive(false);
            
            // load data from leader board
            Leaderboards.ProjectHopper.GetPersonalEntry(entry =>
            {
                PlayerController.LoadData(entry.Username, entry.Score);
                _isInitialized = true;
                
                nameInput.text = PlayerController.PlayerName;
                playButton.onClick.AddListener(() => PlayerController.LoadData(nameInput.text, entry.Score));
                nameInput.gameObject.SetActive(true);
                playButton.gameObject.SetActive(true);
            });

            // time out
            DOVirtual.DelayedCall(5f, () =>
            {
                nameInput.gameObject.SetActive(true);
                playButton.gameObject.SetActive(true);
            });
            
            elementPrefab.gameObject?.SetActive(false);
            Leaderboards.ProjectHopper.GetEntries(entries =>
            {
                int elementAmount = Mathf.Min(entries.Length, maxDisplayAmount);
                
                for (int i = 0; i < elementAmount; i++)
                {
                    elementList.Add(Instantiate(elementPrefab, leaderBoardParent));
                    elementList[i].gameObject.SetActive(true);

                    string elementName = $"{entries[i].Rank}. {entries[i].Username}";
                    int elementScore = entries[i].Score;
                    elementList[i].AssignText(elementName, elementScore);
                }
            });
            
        }
    }
}
