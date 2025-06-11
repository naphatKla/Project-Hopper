using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [SerializeField] private int score;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private MMF_Player scoreChangeFeedback;
    public int Score => score;

    public void AddScore()
    {
        IncreaseScore(1);
        scoreChangeFeedback?.PlayFeedbacks();
    }

    public void IncreaseScore(int num)
    {
        score += num;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (!scoreText) return;
        scoreText.text = $"Score : {score}";
    }
}
