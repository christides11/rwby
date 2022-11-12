using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace rwby
{
    public class TeamScoreElement : MonoBehaviour
    {
        public TextMeshProUGUI scoreText;
        public Image fillImage;

        public int currentScore = -1;
        public int maxScore = 10;

        public void UpdateValue(int score, int currentMaxScore)
        {
            currentScore = score;
            maxScore = currentMaxScore;
            scoreText.text = $"{score.ToString()}/{maxScore}";
            fillImage.fillAmount = (float)currentScore / (float)maxScore;
        }
    }
}