using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

namespace UI
{
    public class LeaderboardElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private MMF_Player assignTextFeedback;

        public void AssignText(string name, int score)
        {
            nameText.text = name;
            scoreText.text = score.ToString();
            assignTextFeedback?.PlayFeedbacks();
        }
    }
}
