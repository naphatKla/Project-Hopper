using System.Collections.Generic;
using Characters.Controllers;
using Dan.Main;
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
        private List<LeaderboardElement> elementList = new List<LeaderboardElement>();
        private bool _isInitialized;

        private async void Start()
        {
            playButton.gameObject.SetActive(false);
            nameInput.gameObject.SetActive(false);
            
            // load data from leader board
            Leaderboards.ProjectHopper.GetPersonalEntry(entry =>
            {
                PlayerController.SetName(entry.Username);
                PlayerController.SetHighestScore(entry.Score);
                _isInitialized = true;
                
                nameInput.text = PlayerController.PlayerName;
                playButton.onClick.AddListener(() => PlayerController.SetName(nameInput.text));
                
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
