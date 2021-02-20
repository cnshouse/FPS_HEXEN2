using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_PlayerScoreboardTable : MonoBehaviour
{
    public Team team = Team.All;
    public RectTransform panel;
    public GameObject joinButton;
    public Graphic[] teamColorGraphics;
    public Text[] teamNameTexts;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        Color c = team.GetTeamColor();
        foreach (var item in teamColorGraphics)
        {
            if (item != null)
                item.color = c;
        }
        string tn = team.GetTeamName().ToUpper();
        foreach (var item in teamNameTexts)
        {
            if (item != null)
                item.name = tn;
        }
    }

    public bl_PlayerScoreboardUI Instance(Player player, GameObject uiPrefab)
    {
        GameObject instance = Instantiate(uiPrefab) as GameObject;
        instance.transform.SetParent(panel, false);
        bl_PlayerScoreboardUI script = instance.GetComponent<bl_PlayerScoreboardUI>();
        script.Init(player);
        return script;
    }

    public bl_PlayerScoreboardUI InstanceBot(bl_AIMananger.BotsStats player, GameObject uiPrefab)
    {
        GameObject instance = Instantiate(uiPrefab) as GameObject;
        instance.transform.SetParent(panel, false);
        bl_PlayerScoreboardUI script = instance.GetComponent<bl_PlayerScoreboardUI>();
        script.Init(null, player);
        return script;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetActiveJoinButton(bool active)
    {
        joinButton.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetJoinButton()
    {
        joinButton.GetComponent<Button>().interactable = true;
    }
}