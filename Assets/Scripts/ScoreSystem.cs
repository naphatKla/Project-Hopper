using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [SerializeField] private int score;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private MMF_Player scoreChangeFeedback;
    
    public void AddScore()
    {
        score++;
        scoreChangeFeedback?.PlayFeedbacks();
        if (!scoreText) return;
        scoreText.text = $"Score : {score}";
    }
}
