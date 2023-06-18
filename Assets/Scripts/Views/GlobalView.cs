using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class GlobalView : MonoBehaviour
    {
        public Text TextScore;

        public void UpdateScore(int score, int scoreForNextLevel)
        {
            TextScore.text = "SCORE: " + score + " / " + scoreForNextLevel + " (for next level)";
        }
    }
}