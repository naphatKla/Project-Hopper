using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dan.Models;
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
        private readonly List<LeaderboardElement> _elementList = new List<LeaderboardElement>();

        private async void Start()
        {
            playButton.gameObject.SetActive(false);
            nameInput.gameObject.SetActive(false);
            
            Entry ownerEntry = await LeaderboardManager.LoadOwnerDataFromServer();
            nameInput.text = ownerEntry.Username;
            nameInput.gameObject.SetActive(true);
            playButton.gameObject.SetActive(true);
            playButton.onClick.AddListener(() => LeaderboardManager.SetLocalData(nameInput.text, ownerEntry.Score));

            Entry[] entries = await LeaderboardManager.LoadAllDataFromServer();
            int elementAmount = Mathf.Min(entries.Length, maxDisplayAmount);
                
            for (int i = 0; i < elementAmount; i++)
            {
                _elementList.Add(Instantiate(elementPrefab, leaderBoardParent));
                _elementList[i].gameObject.SetActive(true);

                string elementName = $"{entries[i].Rank}. {entries[i].Username}";
                int elementScore = entries[i].Score;
                _elementList[i].AssignText(elementName, elementScore);
            }
            
            Debug.Log("Done");
        }
    }
}
