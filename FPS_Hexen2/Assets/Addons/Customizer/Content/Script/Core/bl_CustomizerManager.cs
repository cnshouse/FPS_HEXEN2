using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class bl_CustomizerManager : MonoBehaviour
{

    [Header("Settings")]
    public string ReturnScene;
    /// <summary>
    /// List all objects that can customize
    /// </summary>
    public List<bl_Customizer> AllCustom = new List<bl_Customizer>();
    public int defaultFirstWeapon = 0;
    [Range(0.01f, 2)]
    public float DetachDuration = 0.2f;
    [Range(1, 10)]
    public float RotateSpeed;
    [Range(-100, 100)]
    public float AutoRotSpeed = -60f;

    [Header("UI References")]
    [SerializeField]
    private Text SeparateText;
    [SerializeField]
    private Text AutoRotateText;
    [SerializeField]
    private Image LoadingRect;
    [Header("References")]
    public Transform m_Manager;
    [SerializeField]
    private Animator WarningAnim;
    [SerializeField]
    private Animator RootAnim;
    [SerializeField]
    private GameObject AttachButtonPrefab;
    [SerializeField]
    private GameObject CustomizerButtonPrefab;
    [SerializeField]
    private Transform CustomizerPanel;
    [SerializeField] private Transform CamoPanel;
    [SerializeField] private GameObject CamoUIPrefab;
    [SerializeField]
    private Transform AttachPanel;
    [SerializeField]
    private AudioClip AttachSound;
    [SerializeField] private AudioClip OnChangeAttachmentSound;
    [SerializeField] private AudioClip OnChangeCamoSound;

    [HideInInspector]
    public bl_Customizer CurrentCustomizer;
    private bool isSave = false;
    [HideInInspector]
    public bool Have_Name = false;
    private bool isLoading = false;
    private List<GameObject> cacheButtons = new List<GameObject>();
    private bl_Customizer currentCustomizer;
    private int savedWeaponID = 0;


    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        savedWeaponID = PlayerPrefs.GetInt(bl_CustomizerData.CURRENT_CUSTOMIZER, defaultFirstWeapon);
        showCustomizerWeapon(AllCustom.Find(x => x.WeaponID == savedWeaponID));

        InstanceCustomizers();
    }

    /// <summary>
    /// 
    /// </summary>
    void InstanceCustomizers()
    {
        for (int i = 0; i < AllCustom.Count; i++)
        {
            if (AllCustom[i] == null) continue;
            GameObject g = Instantiate(CustomizerButtonPrefab) as GameObject;
            g.transform.SetParent(CustomizerPanel, false);
            g.GetComponent<bl_CustomizerInfoButton>().Init(AllCustom[i]);
        }
    }


    /// <summary>
    /// activate the object in the list with the name of the information
    /// </summary>
    /// <param name="t_active"></param>
    public void showCustomizerWeapon(bl_Customizer weaponToShow)
    {
        currentCustomizer = weaponToShow;
        RootAnim.Play("Change", 0, 0);
        isSave = false;
        float t = RootAnim.GetCurrentAnimatorStateInfo(0).length / 2;
        Invoke("ActiveInvoke", t);
    }

    /// <summary>
    /// 
    /// </summary>
    void ActiveInvoke()
    {
        foreach (bl_Customizer c in AllCustom)
        {
            c.gameObject.SetActive(false);
            if (c.WeaponID == currentCustomizer.WeaponID)
            {
                c.gameObject.SetActive(true);
                CurrentCustomizer = c;
                SeparateText.text = (CurrentCustomizer.Customize) ? "UNITE" : "SEPARATE";
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (isLoading)
        {
            LoadingRect.CrossFadeAlpha(1, 0.5f, true);
            LoadingRect.rectTransform.Rotate(-Vector3.forward * 20);
        }
        else { LoadingRect.CrossFadeAlpha(0, 0.2f, true); }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    public void ChangeAttachWindow(List<CustomizerModelInfo> list, bl_AttachType type)
    {
        ClearUIList();
        for (int i = 0; i < list.Count; i++)
        {
            GameObject go = Instantiate(AttachButtonPrefab) as GameObject;
            go.GetComponent<bl_AttachmentInfoButton>().Init(list[i], type, 0.1f * i, CurrentCustomizer.AttachmentsIDs[(int)type] == list[i].ID);
            go.transform.SetParent(AttachPanel, false);
            cacheButtons.Add(go);
        }
    }

    public void ShowCamos(string weapon)
    {
        ClearUIList();
        CustomizerInfo info = bl_CustomizerData.Instance.GetWeapon(weapon);
        for (int i = 0; i < info.Camos.Count; i++)
        {
            GameObject go = Instantiate(CamoUIPrefab) as GameObject;
            go.GetComponent<bl_AttachmentInfoButton>().InitCamo(info.Camos[i], 0.1f * i, CurrentCustomizer.AttachmentsIDs[(int)bl_AttachType.Camo] == info.Camos[i].ID);
            go.transform.SetParent(CamoPanel, false);
            cacheButtons.Add(go);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void ClearUIList()
    {
        foreach (GameObject g in cacheButtons) { Destroy(g); }
        cacheButtons.Clear();

    }
    /// <summary>
    /// 
    /// </summary>
    public void Separate()
    {
        if (CurrentCustomizer == null)
            return;

        isSave = false;
        bool b = CurrentCustomizer.Separate();
        if (!b) { ClearUIList(); }
        SeparateText.text = (b) ? "UNITE" : "SEPARATE";
        GetComponent<AudioSource>().PlayOneShot(AttachSound);
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoRotate()
    {
        if (CurrentCustomizer == null)
            return;

        bool b = CurrentCustomizer.AutoRotate();
        AutoRotateText.text = (b) ? "AUTOROTATE [ON]" : "AUTOROTATE [OFF]";
    }

    /// <summary>
    /// 
    /// </summary>
    public void Randomize()
    {
        if (CurrentCustomizer == null)
            return;

        isSave = false;
        CurrentCustomizer.Randomize();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Save()
    {
        if (CurrentCustomizer == null)
            return;

        isLoading = true;
        CurrentCustomizer.Save();
        isSave = true;
        Invoke("HideLoading", 2f);
    }

    void HideLoading() { isLoading = false; }

    /// <summary>
    /// 
    /// </summary>
    public void Exit()
    {
        if (isSave)
        {
            SceneManager.LoadScene(ReturnScene);
        }
        else
        {
            WarningAnim.gameObject.SetActive(true);
            WarningAnim.SetBool("show", true);
        }
    }

    public void ForceExit() { SceneManager.LoadScene(ReturnScene); }

    /// <summary>
    /// 
    /// </summary>
    public void HideWarning()
    {
        WarningAnim.SetBool("show", false);
    }

    /// <summary>
    /// if not have any information, activate the first item in the list
    /// </summary>
    void Enabled_Firts()
    {
        if (m_Manager.childCount > 0)
        {
            for (int i = 0; i < m_Manager.childCount; i++)
            {
                m_Manager.GetChild(i).gameObject.SetActive(false);
            }

            m_Manager.GetChild(0).gameObject.SetActive(true);
            CurrentCustomizer = m_Manager.GetChild(0).gameObject.GetComponent<bl_Customizer>();
        }
        else
        {
            Debug.LogError("has no child in this Manager: " + m_Manager.name);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typ"></param>
    /// <param name="ID"></param>
    public void OnSelectAttachment(bl_AttachType typ, int ID)
    {
        if (CurrentCustomizer == null)
            return;

        CurrentCustomizer.ChangeAttachment(typ, ID);
        GetComponent<AudioSource>().PlayOneShot(OnChangeAttachmentSound);
    }

    public void OnSelectCamo(int ID)
    {
        if (CurrentCustomizer == null)
            return;

        CurrentCustomizer.ChangeCamo(ID);
        GetComponent<AudioSource>().PlayOneShot(OnChangeCamoSound);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    private bool ActualSelect(bl_Customizer c)
    {
        bool b = false;

        foreach (bl_Customizer custom in AllCustom)
        {
            if (c == custom)
            {
                if (custom.gameObject.activeSelf)
                {
                    b = true;
                }
            }
        }
        return b;
    }
}
