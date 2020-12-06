using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using MFPS.PlayerSelector;

public class bl_PlayerSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField]private GameObject ContentUI;
    [SerializeField]private GameObject PlayerOptionUI;
    [SerializeField]private Transform ListPanel;
    public RectTransform CenterReference;

#if PSELECTOR
    private bl_GameManager GameManager;
    private Team SelectTeam = Team.None;
#endif
    private bl_RoomMenu RoomMenu;
    private bl_PlayerSelectorInfo Info;
    private bool isSelected = false;
    public bool isChangeOfTeam { get; set; }
    private List<GameObject> cacheList = new List<GameObject>();

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        #if PSELECTOR
        GameManager = FindObjectOfType<bl_GameManager>();
        #endif
        RoomMenu = FindObjectOfType<bl_RoomMenu>();
    }


    /// <summary>
    /// 
    /// </summary>
    public void SelectPlayer(bl_PlayerSelectorInfo info)
    {
        Info = info;
        ContentUI.SetActive(false);
#if PSELECTOR
        GameManager.SpawnSelectedPlayer(info, SelectTeam);
#endif
        RoomMenu.isPlaying = true;
        isSelected = true;
    }

    public void SpawnSelected(Team team)
    {
        ContentUI.SetActive(false);
#if PSELECTOR
        if (bl_PlayerSelectorData.Instance.PlayerSelectorMode == bl_PlayerSelectorData.PSType.InMatch)
        {
            GameManager.SpawnSelectedPlayer(Info, SelectTeam);
        }
        else
        {
            bl_PlayerSelectorInfo playerInfo = bl_PlayerSelectorData.Instance.GetSelectedPlayerFromTeam(team);
            GameManager.SpawnSelectedPlayer(playerInfo, team);
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void OpenSelection(Team team)
    {
#if PSELECTOR
        SelectTeam = team;
#endif
        ListPanel.GetComponent<HorizontalLayoutGroup>().enabled = true;
        ListPanel.GetComponent<ContentSizeFitter>().enabled = true;
        if (team == Team.Team1)
        {
            for (int i = 0; i < bl_PlayerSelectorData.Instance.Team1Players.Count; i++)
            {
                GameObject g = Instantiate(PlayerOptionUI);
                g.GetComponent<bl_PlayerSelectorUI>().Set(bl_PlayerSelectorData.Instance.GetPlayer(Team.Team1, i), this);
                g.transform.SetParent(ListPanel, false);
                cacheList.Add(g);
            }
        }
        else if (team == Team.Team2)
        {
            for (int i = 0; i < bl_PlayerSelectorData.Instance.Team2Players.Count; i++)
            {
                GameObject g = Instantiate(PlayerOptionUI);
                g.GetComponent<bl_PlayerSelectorUI>().Set(bl_PlayerSelectorData.Instance.GetPlayer(Team.Team2, i), this);
                g.transform.SetParent(ListPanel, false);
                cacheList.Add(g);
            }
        }
        else
        {
            for (int i = 0; i < bl_PlayerSelectorData.Instance.FFAPlayers.Count; i++)
            {
                GameObject g = Instantiate(PlayerOptionUI);
                g.GetComponent<bl_PlayerSelectorUI>().Set(bl_PlayerSelectorData.Instance.GetPlayer(Team.All, i), this);
                g.transform.SetParent(ListPanel, false);
                cacheList.Add(g);
            }
        }
        isChangeOfTeam = false;
        ContentUI.SetActive(true);
    }

    public void DeleteAllBut(GameObject obj)
    {
        for (int i = 0; i < cacheList.Count; i++)
        {
            if(cacheList[i] != obj)
            {
                Destroy(cacheList[i]);
            }
        }
        cacheList.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public GameObject GetPlayerModel()
    {
        if (Info == null)
        {
            return Info.Prefab;
        }else
        {
            Debug.Log("Any team selected yet");
            return null;
        }
    }

    public bool IsSelected { get { return isSelected; } }

    private static bl_PlayerSelector _ps;
    public static bl_PlayerSelector Instance
    {
        get
        {
            if(_ps == null) { _ps = FindObjectOfType<bl_PlayerSelector>(); }
            return _ps;
        }
    }
}