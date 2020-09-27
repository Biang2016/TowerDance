using BiangStudio.GamePlay.UI;
using UnityEngine;
using UnityEngine.UI;

public class ScorePanel : BaseUIPanel
{
    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Fixed,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);

        HighestScoreText.text = "";
        QualityText.text = "";
        ComboText.text = "";
        ScoreText.text = "";
    }

    public Text HighestScoreText;

    public Text QualityText;
    public Text ComboText;
    public Text ScoreText;

    public Animator QualityAnim;
    public Animator ComboAnim;
    public Animator ScoreAnim;

    public void SetScore(int score)
    {
        if (gameEnd) return;
        if (score == 0)
        {
            ScoreText.text = "";
        }
        else
        {
            ScoreText.text = score.ToString("N0");
        }

        ScoreAnim.SetTrigger("Jump");
    }

    public void SetCombo(int comboNumber)
    {
        if (gameEnd) return;
        if (comboNumber == 0)
        {
            ComboText.text = "";
        }
        else
        {
            ComboText.text = $"x{comboNumber}";
        }

        ComboAnim.SetTrigger("Jump");
    }

    public void SetQuality(HitQuality hitQuality)
    {
        QualityText.text = $"{hitQuality}!";
        QualityAnim.SetTrigger("Jump_" + hitQuality);
    }

    private bool gameEnd = false;

    public void ShowHighestScore()
    {
        gameEnd = true;
        int record = 0;
        if (LevelManager.Instance.IsHard)
        {
            record = PlayerPrefs.GetInt("HighestScore_Hard");
            if (LevelManager.Instance.Score > record)
            {
                record = LevelManager.Instance.Score;
                PlayerPrefs.SetInt("HighestScore_Hard", record);
            }
        }
        else
        {
            record = PlayerPrefs.GetInt("HighestScore_Easy");
            if (LevelManager.Instance.Score > record)
            {
                record = LevelManager.Instance.Score;
                PlayerPrefs.SetInt("HighestScore_Easy", record);
            }
        }

        HighestScoreText.text = $"{(LevelManager.Instance.IsHard ? "Hard" : "Easy")}\nHighest Score: {record}\nYour Score: {LevelManager.Instance.Score}";
        QualityText.text = "";
        ComboText.text = "";
        ScoreText.text = "";
    }
}