using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class bl_PlayerScoreboardUI : MonoBehaviour
{
    [SerializeField] private Text NameText = null;
    [SerializeField] private Text KillsText = null;
    [SerializeField] private Text DeathsText = null;
    [SerializeField] private Text ScoreText = null;
    [SerializeField] private GameObject KickButton = null;
    [SerializeField] private Image LevelIcon = null;
    public Text levelNumberText;

    private bool isInitializated = false;
    private Image BackgroundImage;
    private Team InitTeam = Team.None;
    public bl_AIMananger.BotsStats Bot { get; set; }
    public bool isBotBinding { get; set; } = false;
    public Player cachePlayer { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public void Init(Player player, bl_AIMananger.BotsStats bot = null)
    {
        Bot = bot;
        BackgroundImage = GetComponent<Image>();
        isBotBinding = bot != null;

        if (Bot != null)
        {
            UpdateBot();
            return;
        }

        cachePlayer = player;
        gameObject.name = player.NickName + player.ActorNumber;
        if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            Color c = BackgroundImage.color;
            c.a = 0.35f;
            BackgroundImage.color = c;
        }
        InitTeam = player.GetPlayerTeam();
        NameText.text = player.NickNameAndRole();
        if (!player.CustomProperties.ContainsKey(PropertiesKeys.KillsKey)) return;

        KillsText.text = player.CustomProperties[PropertiesKeys.KillsKey].ToString();
        DeathsText.text = player.CustomProperties[PropertiesKeys.DeathsKey].ToString();
        ScoreText.text = player.CustomProperties[PropertiesKeys.ScoreKey].ToString();
        if (bl_GameData.Instance.MasterCanKickPlayers && player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            KickButton.SetActive(PhotonNetwork.IsMasterClient);
        }
        else { KickButton.SetActive(false); }
#if LM
         LevelIcon.gameObject.SetActive(true);
         var li = bl_LevelManager.Instance.GetPlayerLevelInfo(cachePlayer);
         LevelIcon.sprite = li.Icon;
        if (levelNumberText != null) levelNumberText.text = li.LevelID.ToString();
#else
        LevelIcon.gameObject.SetActive(false);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public bool Refresh()
    {
        if (Bot != null || isBotBinding) {  return UpdateBot(); }

        if (cachePlayer == null || cachePlayer.GetPlayerTeam() != InitTeam)
        {
            if (!bl_PlayerScoreboard.Instance.RemoveUIBinding(this))
            {
                Destroy();
            }
            return false;
        }
        if (!cachePlayer.CustomProperties.ContainsKey(PropertiesKeys.KillsKey)) return true;

        NameText.text = cachePlayer.NickNameAndRole();
        KillsText.text = cachePlayer.CustomProperties[PropertiesKeys.KillsKey].ToString();
        DeathsText.text = cachePlayer.CustomProperties[PropertiesKeys.DeathsKey].ToString();
        ScoreText.text = cachePlayer.CustomProperties[PropertiesKeys.ScoreKey].ToString();

        if (bl_GameData.Instance.MasterCanKickPlayers && cachePlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            KickButton.SetActive(PhotonNetwork.IsMasterClient);
        }
        else { KickButton.SetActive(false); }
#if LM
        var li = bl_LevelManager.Instance.GetPlayerLevelInfo(cachePlayer);
        LevelIcon.sprite = li.Icon;
        if (levelNumberText != null) levelNumberText.text = li.LevelID.ToString();
#endif
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool UpdateBot()
    {
        if (Bot == null || string.IsNullOrEmpty(Bot.Name) || !bl_AIMananger.Instance.BotsStatistics.Exists(x => x.Name == Bot.Name))
        {
            if (!bl_PlayerScoreboard.Instance.RemoveUIBinding(this))
            {
                Destroy();
                return false;
            }
        }

        gameObject.name = Bot.Name;
        NameText.text = Bot.Name;
        KillsText.text = Bot.Kills.ToString();
        DeathsText.text = Bot.Deaths.ToString();
        ScoreText.text = Bot.Score.ToString();
        InitTeam = Bot.Team;
        KickButton.SetActive(false);
        LevelIcon.gameObject.SetActive(false);
        return true;
    }

    public void Kick()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            bl_PhotonNetwork.Instance.KickPlayer(cachePlayer);
        }
    }

    public void OnClick()
    {
        if (cachePlayer == null)
            return;
        if (cachePlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber && Bot == null)
        {
            bl_UIReferences.Instance.OpenScoreboardPopUp(true, cachePlayer);
        }
    }

    void OnEnable()
    {
        if (cachePlayer == null && !isBotBinding && isInitializated)
        {
            Destroy(gameObject);
            isInitializated = true;
        }
        else if (isBotBinding && Bot == null && isInitializated)
        {
            Destroy(gameObject);
            isInitializated = true;
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public int GetScore()
    {
        if (Bot == null) { return cachePlayer.GetPlayerScore(); }
        else { return Bot.Score; }
    }

    public Team GetTeam()
    {
        return InitTeam;
    }
}