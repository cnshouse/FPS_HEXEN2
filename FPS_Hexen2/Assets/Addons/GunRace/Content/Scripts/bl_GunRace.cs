using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class bl_GunRace : bl_MonoBehaviour, IGameMode
{
    [Header("Gun List")]
     public List<int> GunOrderList = new List<int>();

    [Header("References")]
    public GameObject Content;
    public AudioClip PointAudio;
    public GameObject ParticleEffects = null;
    public Text ScoreText = null;

    private int CurrentLocalGun = 0;
    private bl_GameManager GameManager;
    private bl_GunManager GunManager;
    private bl_MatchTimeManager TimeManager;
    private List<Player> PlayerList = new List<Player>();

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
            return;

        base.Awake();
        GameManager = bl_GameManager.Instance;
        TimeManager = bl_MatchTimeManager.Instance;
        Initialize();
        ScoreText.text = string.Format(bl_GameTexts.GRPlayerWinning, GetWinnerPlayer.NickName, GetWinnerPlayer.GetKills() + 1, GunOrderList.Count);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        bl_PhotonCallbacks.PlayerPropertiesUpdate += OnPhotonPlayerPropertiesChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        bl_PhotonCallbacks.PlayerPropertiesUpdate -= OnPhotonPlayerPropertiesChanged;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetNextGun()
    {
        CurrentLocalGun = (CurrentLocalGun + 1) % GunOrderList.Count;
        if(CurrentLocalGun == 0)
        {
            //player won!
          //  OnPlayerWon();
        }
        else
        {
            
            Transform player = GameManager.LocalPlayer.transform;
            Vector3 pos = player.position;
            AudioSource.PlayClipAtPoint(PointAudio, pos, 1);
            if(ParticleEffects != null)
            {
                Vector3 inspos = player.TransformPoint(Vector3.forward * 1.5f);
                Instantiate(ParticleEffects, inspos, Quaternion.identity);
            }
            if(GunManager != null)
            {
                int id = GunManager.AllGuns.FindIndex(x => x.GunID == GetCurrentGun);
                GunManager.ChangeToInstant(id);
            }
        }
        return CurrentLocalGun;
    }

    public void SetGunManager(bl_GunManager gm) { GunManager = gm; }

    public int GetCurrentGun
    {
        get
        {
            return GunOrderList[CurrentLocalGun];
        }
    }

    public bl_Gun GetGunInfo(List<bl_Gun> guns)
    {
        return guns.Find(x => x.GunID == GetCurrentGun);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnPlayerWon()
    {
        TimeManager.FinishRound();
        bl_UIReferences.Instance.SetFinalText(GetWinnerPlayer.NickName);
    }

    public void OnPhotonPlayerPropertiesChanged(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
#if GR
           if (GetGameMode != GameMode.GR)
            return;

        if (changedProps.ContainsKey(PropertiesKeys.KillsKey))
        {
            if (GetWinnerPlayer.GetKills() >= GunOrderList.Count)
            {
                OnPlayerWon();
                return;
            }
            ScoreText.text = string.Format(bl_GameTexts.GRPlayerWinning, GetWinnerPlayer.NickName, GetWinnerPlayer.GetKills() + 1, GunOrderList.Count);
        }
#endif
    }

    public Player GetWinnerPlayer
    {
        get
        {
            PlayerList.Clear();
            PlayerList.AddRange(PhotonNetwork.PlayerList);
            PlayerList.Sort(GetSortPlayerByKills);
            return PlayerList[0];
        }
    }

    private static int GetSortPlayerByKills(Player player1, Player player2)
    {
        if (player1.CustomProperties[PropertiesKeys.KillsKey] != null && player2.CustomProperties[PropertiesKeys.KillsKey] != null)
        {
            return player2.GetKills() - player1.GetKills();
        }
        else
        {
            return 0;
        }
    }
    #region GameMode Interface
    public bool isLocalPlayerWinner
    {
        get { return PhotonNetwork.LocalPlayer.ActorNumber == GetWinnerPlayer.ActorNumber; }
    }

    public void Initialize()
    {
        //check if this is the game mode of this room
        if (bl_GameManager.Instance.IsGameMode(GameMode.GR, this))
        {
            bl_GameManager.Instance.SetGameState(MatchState.Starting);
            Content.SetActive(true);
        }
        else
        {
            Content.SetActive(false);
        }
    }

    public void OnFinishTime(bool gameOver)
    {
        OnPlayerWon();
    }

    public void OnLocalPoint(int points, Team teamToAddPoint)
    {
       
    }

    public void OnLocalPlayerKill()
    {      
    }

    public void OnLocalPlayerDeath()
    {
    }

    public void OnOtherPlayerEnter(Player newPlayer)
    {
    }

    public void OnOtherPlayerLeave(Player otherPlayer)
    {
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
    } 
    #endregion
}