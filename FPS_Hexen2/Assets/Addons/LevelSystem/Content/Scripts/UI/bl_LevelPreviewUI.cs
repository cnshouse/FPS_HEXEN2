using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_LevelPreviewUI : MonoBehaviour
{
    [SerializeField]private Image Icon;
    [SerializeField] private Text NameText;
    [SerializeField] private Text XpText;
    [SerializeField] private Image ProgressImg;
    public GameObject CurrentMarkUI;

    public void Set(LevelInfo info, int score, bl_LevelPreview manager)
    {
        Icon.sprite = info.Icon;
        NameText.text = info.Name.ToUpper();
        XpText.text = info.ScoreNeeded.ToString() + "XP";
        gameObject.name = info.Name;
        if (score <= -1) { ProgressImg.fillAmount = 0; return; }
        if (info.ScoreNeeded > 0)
        {
            if (info.ScoreNeeded > score)
            {
                ProgressImg.fillAmount = Mathf.Clamp01((float)score / (float)info.ScoreNeeded);
                if (!manager.LevelToReachDetected)
                {
                    manager.selectedLevel = GetComponent<RectTransform>();
                }
                manager.LevelToReachDetected = true;
            }
        }
        else { ProgressImg.fillAmount = 1; }
    }
}