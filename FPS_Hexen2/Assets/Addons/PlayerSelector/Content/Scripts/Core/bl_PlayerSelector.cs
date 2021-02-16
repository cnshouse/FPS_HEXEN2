using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using MFPS.Addon.PlayerSelector;
using Photon.Pun;

public class bl_PlayerSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField]private GameObject ContentUI;
    [SerializeField]private GameObject PlayerOptionUI;
    [SerializeField]private Transform ListPanel;
    public RectTransform CenterReference;

#if PSELECTOR
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
        SpawnSelectedPlayer(info, SelectTeam);
#endif
        RoomMenu.isPlaying = true;
        isSelected = true;
    }

    public bool TrySpawnSelectedPlayer(Team playerTeam)
    {
        if (bl_PlayerSelectorData.Instance.PlayerSelectorMode == bl_PlayerSelectorData.PSType.InMatch)
        {
            if (IsSelected && !isChangeOfTeam)
            {
                SpawnSelected(playerTeam);
                return true;
            }
            else
            {
                OpenSelection(playerTeam);
                return false;
            }
        }
        else
        {
            if (!PhotonNetwork.OfflineMode)
               SpawnSelectedPlayer(bl_PlayerSelectorData.Instance.GetSelectedPlayerFromTeam(playerTeam), playerTeam);
            else
            {
                bl_GameManager.Instance.SpawnPlayerModel(playerTeam);
            }
        }
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="team"></param>
    public void SpawnSelected(Team team)
    {
        ContentUI.SetActive(false);
#if PSELECTOR
        if (bl_PlayerSelectorData.Instance.PlayerSelectorMode == bl_PlayerSelectorData.PSType.InMatch)
        {
            SpawnSelectedPlayer(Info, SelectTeam);
        }
        else
        {
            bl_PlayerSelectorInfo playerInfo = bl_PlayerSelectorData.Instance.GetSelectedPlayerFromTeam(team);
            SpawnSelectedPlayer(playerInfo, team);
        }
#endif
    }

    public static void SpawnPreSelectedPlayer(Team playerTeam) => SpawnSelectedPlayer(bl_PlayerSelectorData.Instance.GetSelectedPlayerFromTeam(playerTeam), playerTeam);

    /// <summary>
    /// 
    /// </summary>
    public static void SpawnSelectedPlayer(bl_PlayerSelectorInfo info, Team playerTeam)
    {
        if (PhotonNetwork.OfflineMode)
        {
            bl_GameManager.Instance.SpawnPlayerModel(playerTeam);
            return;
        }
        Vector3 pos;
        Quaternion rot;
        bl_SpawnPointManager.Instance.GetPlayerSpawnPosition(playerTeam, out pos, out rot);

        bl_GameManager.Instance.InstancePlayer(info.Prefab, pos, rot, playerTeam);

        bl_GameManager.Instance.AfterSpawnSetup();
        if (!bl_GameManager.Instance.FirstSpawnDone && bl_MatchInformationDisplay.Instance != null) { bl_MatchInformationDisplay.Instance.DisplayInfo(); }
        bl_GameManager.Instance.FirstSpawnDone = true;
        bl_UCrosshair.Instance.Show(true);
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
        bl_UtilityHelper.LockCursor(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
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
    public static bool InMatch => bl_PlayerSelectorData.Instance.PlayerSelectorMode == bl_PlayerSelectorData.PSType.InMatch;
    public static bl_PlayerSelectorData Data => bl_PlayerSelectorData.Instance;

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