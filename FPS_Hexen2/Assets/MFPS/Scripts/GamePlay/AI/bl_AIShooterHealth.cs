using UnityEngine;
using Photon.Pun;
using MFPS.Runtime.UI;
using MFPS.Runtime.AI;

public class bl_AIShooterHealth : bl_PhotonHelper
{

    [Range(10, 500)] public int Health = 100;

    [Header("References")]
    public Texture2D DeathIcon;

    private bl_AIShooter m_AIShooter;
    private bl_AIMananger AIManager;
    private int LastActorEnemy = -1;
    private bl_AIAnimation AIAnim;
    private int m_RepetingDamage = 1;
    private DamageData RepetingDamageInfo;
    public int lastHitBoxHitted { get; set; }
    private bl_AIShooterReferences references;
    private bl_AIShooterAgent shooterAgent;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        references = GetComponent<bl_AIShooterReferences>();
        m_AIShooter = references.aiShooter;
        shooterAgent = GetComponent<bl_AIShooterAgent>();
        AIManager = bl_AIMananger.Instance;
        AIAnim = references.aiAnimation;
    }
    /// <summary>
    /// 
    /// </summary>
    public void DoDamage(int damage, string wn, Vector3 direction, int vi, bool fromBot, Team team, bool ishead, int hitBoxID)
    {
        if (m_AIShooter.isDeath)
            return;

        if (!isOneTeamMode)
        {
            if (team == m_AIShooter.AITeam) return;
        }

        photonView.RPC(nameof(RpcDoDamage), RpcTarget.All, damage, wn, direction, vi, fromBot, ishead, hitBoxID);
    }

    [PunRPC]
    void RpcDoDamage(int damage, string weaponName, Vector3 direction, int viewID, bool fromBot, bool ishead, int hitBoxID)
    {
        if (m_AIShooter.isDeath)
            return;

        Health -= damage;
        if (LastActorEnemy != viewID)
        {
            if (shooterAgent != null)
                shooterAgent.personal = false;
        }
        lastHitBoxHitted = hitBoxID;
        LastActorEnemy = viewID;

        if (PhotonNetwork.IsMasterClient)
        {
            shooterAgent?.OnGetHit(direction);
        }
        if (viewID == bl_GameManager.LocalPlayerViewID)//if was me that make damage
        {
            bl_UCrosshair.Instance.OnHit();
            bl_EventHandler.DispatchLocalPlayerHitEnemy(gameObject.name);
        }

        if (Health > 0)
        {
            Transform t = bl_GameManager.Instance.FindActor(viewID);
            if (t != null && !t.name.Contains("(die)"))
            {
                if (m_AIShooter.Target == null)
                {
                    if (shooterAgent != null)
                        shooterAgent.personal = true;
                    m_AIShooter.Target = t;
                }
                else
                {
                    if (t != m_AIShooter.Target)
                    {
                        float cd = bl_UtilityHelper.Distance(transform.position, m_AIShooter.Target.position);
                        float od = bl_UtilityHelper.Distance(transform.position, t.position);
                        if (od < cd && (cd - od) > 7)
                        {
                            if (shooterAgent != null)
                                shooterAgent.personal = true;
                            m_AIShooter.Target = t;
                        }
                    }
                }
            }
            AIAnim.OnGetHit();
        }
        else
        {
            Die(viewID, fromBot, ishead, weaponName, direction);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Die(int viewID, bool fromBot, bool ishead, string weaponName, Vector3 direction)
    {
        //Debug.Log($"{gameObject.name} die with {weaponName} from viewID {viewID} Bot?= {fromBot}");
        m_AIShooter.isDeath = true;
        m_AIShooter.OnDeath();
        gameObject.name += " (die)";
        m_AIShooter.AimTarget.name += " (die)";
        references.shooterWeapon.OnDeath();
        m_AIShooter.enabled = false;
        references.Agent.isStopped = true;
        GetComponent<bl_NamePlateDrawer>().enabled = false;
        //update the MFPSPlayer data
        MFPSPlayer player = bl_GameManager.Instance.GetMFPSPlayer(m_AIShooter.AIName);
        if(player != null)
        player.isAlive = false;

        bl_AIShooter killerBot = null;
        if (viewID == bl_GameManager.LocalPlayerViewID && !fromBot)//if was me that kill this bot
        {
            Team team = PhotonNetwork.LocalPlayer.GetPlayerTeam();
            //send kill feed message
            int gunID = bl_GameData.Instance.GetWeaponID(weaponName);
            if (weaponName.Contains("cmd:") || gunID == -1)
            {
                weaponName = weaponName.Replace("cmd:", "");
                gunID = -(bl_KillFeed.Instance.GetCustomIconIndex(weaponName) + 1);
            }

            bl_KillFeed.Instance.SendKillMessageEvent(LocalName, m_AIShooter.AIName, gunID, team, ishead);

            //Add a new kill and update information
            PhotonNetwork.LocalPlayer.PostKill(1);//Send a new kill

            int score;
            //If heat shot will give you double experience
            if (ishead)
            {
                bl_GameManager.Instance.Headshots++;
                score = bl_GameData.Instance.ScoreReward.ScorePerKill + bl_GameData.Instance.ScoreReward.ScorePerHeadShot;
            }
            else
            {
                score = bl_GameData.Instance.ScoreReward.ScorePerKill;
            }

            //Send to update score to player
            PhotonNetwork.LocalPlayer.PostScore(score);

            //show an local notification for the kill
            KillInfo localKillInfo = new KillInfo();
            localKillInfo.Killer = PhotonNetwork.LocalPlayer.NickName;
            localKillInfo.Killed = string.IsNullOrEmpty(m_AIShooter.AIName) ? gameObject.name.Replace("(die)", "") : m_AIShooter.AIName;
            localKillInfo.byHeadShot = ishead;
            localKillInfo.KillMethod = weaponName;
            bl_EventHandler.DispatchLocalKillEvent(localKillInfo);

            //update team score
            bl_GameManager.Instance.SetPoint(1, GameMode.TDM, team);
        }
        else if (fromBot)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonView p = PhotonView.Find(viewID);
                bl_AIShooter bot = null;
                string killer = "Unknown";
                if (p != null)
                {
                    bot = p.GetComponent<bl_AIShooter>();//killer bot
                    killer = bot.AIName;
                    if (string.IsNullOrEmpty(killer)) { killer = p.gameObject.name.Replace(" (die)", ""); }
                    //update bot stats
                    AIManager.SetBotKill(killer);
                }

                //send kill feed message
                int gunID = bl_GameData.Instance.GetWeaponID(weaponName);
                bl_KillFeed.Instance.SendKillMessageEvent(killer, m_AIShooter.AIName, gunID, bot.AITeam, ishead);

                if (bot != null)
                {
                   // bot.KillTheTarget();
                    killerBot = bot;
                }
                else
                {
                    Debug.Log("Bot can't be found");
                }
            }
        }//else, (if other player kill this bot) -> do nothing.

        var mplayer = new MFPSPlayer(photonView, false, false);
        bl_EventHandler.DispatchRemotePlayerDeath(mplayer);

        AIManager.SetBotDeath(m_AIShooter.AIName);
        if (PhotonNetwork.IsMasterClient)
        {
            if (!isOneTeamMode)
            {
                if (m_AIShooter.AITeam == PhotonNetwork.LocalPlayer.GetPlayerTeam())
                {
                    GameObject di = bl_ObjectPooling.Instance.Instantiate("deathicon", transform.position, transform.rotation);
                    di.GetComponent<bl_ClampIcon>().SetTempIcon(DeathIcon, 5, 20);
                }
            }
            AIManager.OnBotDeath(m_AIShooter, killerBot);
        }
        var deathData = bl_UtilityHelper.CreatePhotonHashTable();
        deathData.Add("type", AIRemoteCallType.DestroyBot);
        deathData.Add("direction", direction);
        if (weaponName.Contains("Grenade"))
        {
            deathData.Add("explosion", true);
        }
        this.photonView.RPC(bl_AIShooterAgent.RPC_NAME, RpcTarget.AllBuffered, deathData);//callback is in bl_AIShooterAgent.cs
    }

    /// <summary>
    /// 
    /// </summary>
    public void DestroyBot()
    {
        var deathData = bl_UtilityHelper.CreatePhotonHashTable();
        deathData.Add("type", AIRemoteCallType.DestroyBot);
        deathData.Add("instant", true);
        this.photonView.RPC(bl_AIShooterAgent.RPC_NAME, RpcTarget.AllBuffered, deathData);//callback is in bl_AIShooterAgent.cs
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoRepetingDamage(int damage, int each, DamageData info = null)
    {
        m_RepetingDamage = damage;
        RepetingDamageInfo = info;
        InvokeRepeating("MakeDamageRepeting", 0, each);
    }

    /// <summary>
    /// 
    /// </summary>
    void MakeDamageRepeting()
    {
        DamageData info = new DamageData();
        info.Damage = m_RepetingDamage;
        if (RepetingDamageInfo != null)
        {
            info = RepetingDamageInfo;
            info.Damage = m_RepetingDamage;
        }
        else
        {
            info.Direction = Vector3.zero;
            info.Cause = DamageCause.Map;
        }
        DoDamage((int)info.Damage, "[Burn]", info.Direction, bl_GameManager.LocalPlayerViewID, false, PhotonNetwork.LocalPlayer.GetPlayerTeam(), false, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    public void CancelRepetingDamage()
    {
        CancelInvoke("MakeDamageRepeting");
    }

    [PunRPC]
    void RpcSync(int _health)
    {
        Health = _health;
    }
}