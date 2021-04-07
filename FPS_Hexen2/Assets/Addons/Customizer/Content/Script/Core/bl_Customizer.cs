using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace MFPS.Addon.Customizer
{
    [RequireComponent(typeof(AudioSource))]
    public class bl_Customizer : MonoBehaviour
    {
        private bool ShowHud = true;
        public int WeaponID = 0;
        public CustomizerAttachments Attachments;
        public CustomizerCamoRender CamoRender;
        public bl_AttachmentsPositions Positions;
        public AnimationCurve SeparateCurve = new AnimationCurve(new Keyframe { time = 0, value = 0 }, new Keyframe { time = 1, value = 1 });
        public AnimationCurve ChangeMovementPath = new AnimationCurve(new Keyframe { time = 0, value = 0 }, new Keyframe { time = 0.5f, value = 1 }, new Keyframe { time = 1, value = 0 });

        //Private Vars
        public int[] AttachmentsIDs = new int[] { 0, 0, 0, 0, 0 };
        private string Level;
        private string CurrentMenu;
        private float rootx;
        private float rooty;
        private bool AutoRot;
        public bool Customize { get; set; }
        private bl_AttachmentsButtons Button;
        private bl_CustomizerManager Manager;
        [HideInInspector] public string WeaponName = "";
        private Camera m_Camera;
        private CustomizerInfo WeaponInfo;
        private bool firstApply = false;
        private List<int> availableCamos = new List<int>();

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            Button = GetComponentInChildren<bl_AttachmentsButtons>();
            Manager = FindObjectOfType<bl_CustomizerManager>();
            Positions.Init();
            Button.Init(this);
            Button.Active(Customize);
            WeaponInfo = bl_CustomizerData.Instance.GetWeapon(WeaponName);
            rootx = -transform.eulerAngles.y;
            rooty = transform.eulerAngles.x;
            m_Camera = Camera.main;
        }

        /// <summary>
        /// OnEnable use this object to see if it was active
        /// verify whether the manager has the information and if not,
        /// activate the first object in the list
        /// </summary>
        void OnEnable()
        {
            if (!firstApply)
            {
                AttachmentsIDs = bl_CustomizerData.Instance.LoadAttachmentsForWeapon(WeaponName);
                CamoRender.ApplyCamo(WeaponName, AttachmentsIDs[4]);
                firstApply = true;
            }
            if (WeaponInfo != null)
            {
#if SHOP && ULSP
            for (int i = 0; i < WeaponInfo.Camos.Count; i++)
            {
                CamoInfo info = WeaponInfo.Camos[i];
                GlobalCamo gc = bl_CustomizerData.Instance.GlobalCamos[info.GlobalID];
                if (gc.Price > 0 && bl_DataBase.Instance != null)
                {
                    if (bl_DataBase.Instance.LocalUser.ShopData.isItemPurchase(ShopItemType.WeaponCamo, info.GlobalID))
                    {
                        availableCamos.Add(i);
                    }
                }
                else
                {
                    availableCamos.Add(i);
                }
            }
#else
                for (int i = 0; i < WeaponInfo.Camos.Count; i++)
                {
                    availableCamos.Add(i);
                }
#endif

            }
        }


        /// <summary>
        /// Update call one per frame
        /// </summary>
        void Update()
        {
            PositionControl();
            RendererModule();
            Rotate();
            DetectButton();
        }

        /// <summary>
        /// 
        /// </summary>
        void Rotate()
        {
            if (!AutoRot)
            {
                if (Manager.UIInput.isDown)
                {
                    float m = bl_UtilityHelper.isMobile ? Manager.RotateSpeed * 5 : Manager.RotateSpeed;
                    rooty += Manager.UIInput.Direction.y * m;
                    rootx += Manager.UIInput.Direction.x * m;
                    rooty = Mathf.Clamp(rooty, -40, 40);
                }
            }
            else if (AutoRot && !Customize)
            {
                rootx -= Time.deltaTime * Manager.AutoRotSpeed;
                if (rooty > 0 || rooty < 0)
                {
                    rooty = Mathf.Lerp(rooty, 0, 0.1f);
                }
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(rooty, -rootx, 0)), Time.deltaTime * 8);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeAttachment(bl_AttachType typ, int ID)
        {
            AttachmentsIDs[(int)typ] = ID;
            StopCoroutine("ChangeMovement");
            StartCoroutine("ChangeMovement", typ);
        }

        public void ChangeCamo(int ID)
        {
            AttachmentsIDs[(int)bl_AttachType.Camo] = ID;
            StopCoroutine("CamoChangeEffect");
            StartCoroutine("CamoChangeEffect", ID);
        }

        IEnumerator CamoChangeEffect(int ID)
        {
            float d = 0;
            Vector2 v = Vector2.zero;
            bool send = false;
            Material oldm = CamoRender.Render.material;
            while (d < 1)
            {
                d += Time.deltaTime * 8;
                v.x = Random.Range(-0.012f, 0.012f);
                v.y = Random.Range(-0.033f, 0.033f);
                if (d > 0.5f)
                {
                    if (!send) { CamoRender.ApplyCamo(WeaponName, ID); send = true; }
                    CamoRender.Render.material.SetTextureOffset("_MainTex", v);
                }
                else
                {
                    oldm.SetTextureOffset("_MainTex", v);
                }

                yield return new WaitForSeconds(0.0112f);
            }
            oldm.SetTextureOffset("_MainTex", Vector2.zero);
            CamoRender.Render.material.SetTextureOffset("_MainTex", Vector2.zero);

            foreach(Renderer r in CamoRender.AdditionalRenders)
			{
                r.material.SetTextureOffset("_MainTex", Vector2.zero);
            }
        }

        IEnumerator ChangeMovement(bl_AttachType _type)
        {
            float d = 0;
            Transform tran = Positions.BarrelRoot.transform;
            if (_type == bl_AttachType.Foregrips) { tran = Positions.FeederRoot.transform; }
            else if (_type == bl_AttachType.Magazine) { tran = Positions.CylinderRoot.transform; }
            else if (_type == bl_AttachType.Sights) { tran = Positions.OpticsRoot.transform; }
            Vector3 v = tran.localPosition;
            while (d < 1)
            {
                d += Time.deltaTime / 0.2f;
                float t = ChangeMovementPath.Evaluate(d);
                tran.localPosition = Vector3.Lerp(v, v + new Vector3(0, t, 0), d);
                tran.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, d);
                yield return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Separate()
        {
            Customize = !Customize;
            if (Customize && AutoRot) { AutoRotate(true); }
            StopAllCoroutines();
            StartCoroutine(ChangeButtonPosition(Customize));
            return Customize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool AutoRotate(bool force = false)
        {
            AutoRot = !AutoRot;
            if (!force)
            {
                if (Customize) { Separate(); }
            }
            return AutoRot;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Randomize()
        {
            AttachmentsIDs[0] = Random.Range(0, Attachments.Suppressers.Count);
            AttachmentsIDs[1] = Random.Range(0, Attachments.Sights.Count);
            AttachmentsIDs[2] = Random.Range(0, Attachments.Foregrips.Count);
            AttachmentsIDs[3] = Random.Range(0, Attachments.Magazines.Count);
            int pci = AttachmentsIDs[4];
            AttachmentsIDs[4] = availableCamos[Random.Range(0, availableCamos.Count)];
            if (AttachmentsIDs[4] != pci)
            {
                StopCoroutine(nameof(CamoChangeEffect));
                StartCoroutine(nameof(CamoChangeEffect), AttachmentsIDs[4]);
            }
        }

        /// <summary>
        /// Draw Info for Attachments
        /// </summary>
        void DrawInfo()
        {
            Rect rect = new Rect(0f, 0f, Screen.width, Screen.height);
            if (Button.Barrel != null)
            {
                Vector3 pointBarrel = m_Camera.WorldToScreenPoint(Button.Barrel.transform.position);
                foreach (CustomizerModelInfo IDBarrel in Attachments.Suppressers)
                {
                    if (IDBarrel.ID == AttachmentsIDs[0])
                    {
                        //position is in screen?
                        if (rect.Contains(pointBarrel))
                        {
                            GUI.Label(new Rect(pointBarrel.x - IDBarrel.Info.Description.Length, (Screen.height - pointBarrel.y) + 15, 250, 75), "<size=10>" + IDBarrel.Info.Description + "</size>");
                        }
                    }
                }
            }
            if (Button.Optics != null)
            {
                Vector3 pointOptics = m_Camera.WorldToScreenPoint(Button.Optics.transform.position);
                foreach (CustomizerModelInfo IDOptics in Attachments.Sights)
                {
                    if (IDOptics.ID == AttachmentsIDs[1])
                    {
                        //position is in screen?
                        if (rect.Contains(pointOptics))
                        {
                            GUI.Label(new Rect(pointOptics.x - IDOptics.Info.Description.Length, (Screen.height - pointOptics.y) + 15, 250, 75), "<size=10>" + IDOptics.Info.Description + "</size>");
                        }
                    }
                }
            }
            if (Button.Feeder != null)
            {
                Vector3 pointFeeder = m_Camera.WorldToScreenPoint(Button.Feeder.transform.position);
                foreach (CustomizerModelInfo IDFeeder in Attachments.Foregrips)
                {
                    if (IDFeeder.ID == AttachmentsIDs[2])
                    {
                        //position is in screen?
                        if (rect.Contains(pointFeeder))
                        {
                            GUI.Label(new Rect(pointFeeder.x - IDFeeder.Info.Description.Length, (Screen.height - pointFeeder.y) + 15, 250, 75), "<size=10>" + IDFeeder.Info.Description + "</size>");
                        }
                    }
                }
            }
            if (Button.Cylinder != null)
            {
                Vector3 pointCylinder = m_Camera.WorldToScreenPoint(Button.Cylinder.transform.position);
                foreach (CustomizerModelInfo IDCylinder in Attachments.Magazines)
                {
                    if (IDCylinder.ID == AttachmentsIDs[3])
                    {
                        //position is in screen?
                        if (rect.Contains(pointCylinder))
                        {
                            GUI.Label(new Rect(pointCylinder.x - IDCylinder.Info.Description.Length, (Screen.height - pointCylinder.y) + 15, 250, 75), "<size=10>" + IDCylinder.Info.Description + "</size>");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void RendererModule()
        {
            foreach (CustomizerModelInfo IDBarrel in Attachments.Suppressers)
            {
                if (IDBarrel.Model == null) continue;
                if (IDBarrel.ID == AttachmentsIDs[0] && IDBarrel.Model != null)
                {
                    IDBarrel.Model.SetActive(true);
                }
                else
                {
                    IDBarrel.Model.SetActive(false);
                }
            }

            foreach (CustomizerModelInfo IDOptics in Attachments.Sights)
            {
                if (IDOptics.Model == null) continue;
                if (IDOptics.ID == AttachmentsIDs[1])
                {
                    IDOptics.Model.SetActive(true);
                }
                else
                {
                    IDOptics.Model.SetActive(false);
                }
            }

            foreach (CustomizerModelInfo IDFeeder in Attachments.Foregrips)
            {
                if (IDFeeder.Model == null) continue;
                if (IDFeeder.ID == AttachmentsIDs[2])
                {
                    IDFeeder.Model.SetActive(true);
                }
                else
                {
                    IDFeeder.Model.SetActive(false);
                }
            }

            foreach (CustomizerModelInfo IDCylinder in Attachments.Magazines)
            {
                if (IDCylinder.Model == null) continue;
                if (IDCylinder.ID == AttachmentsIDs[3])
                {
                    IDCylinder.Model.SetActive(true);
                }
                else
                {
                    IDCylinder.Model.SetActive(false);
                }
            }

        }

        void OnGUI() { if (ShowHud && Customize) { DrawInfo(); } }

        /// <summary>
        /// function for save in a PlayerPrefs the code with info
        /// </summary>
        public void Save()
        {
            bl_CustomizerData.Instance.SaveAttachmentsForWeapon(WeaponName, AttachmentsIDs);
            Debug.Log("successfully saved");
        }

        /// <summary>
        /// Show buttons when necessary
        /// </summary>
        void PositionControl()
        {
            Button.LookAt(m_Camera.transform);
            ShowTextButton();

        }

        IEnumerator ChangeButtonPosition(bool open)
        {
            float d = 0;
            Button.Active(open);
            Vector3[] v = new Vector3[4];
            if (Positions.BarrelRoot != null)
                v[0] = (open) ? Positions.BarrelRoot.transform.position : Positions.BarrelRoot.transform.localPosition;
            if (Positions.OpticsRoot != null)
                v[1] = (open) ? Positions.OpticsRoot.transform.position : Positions.OpticsRoot.transform.localPosition;
            if (Positions.FeederRoot != null)
                v[2] = (open) ? Positions.FeederRoot.transform.position : Positions.FeederRoot.transform.localPosition;
            if (Positions.CylinderRoot != null)
                v[3] = (open) ? Positions.CylinderRoot.transform.position : Positions.CylinderRoot.transform.localPosition;
            while (d < 1)
            {
                if (open)
                {
                    d += Time.deltaTime / Manager.DetachDuration;
                    float t = SeparateCurve.Evaluate(d);
                    t = Out(t);
                    if (Positions.BarrelPosition != null && Positions.BarrelPosition != null)
                        Positions.BarrelRoot.transform.position = Vector3.Lerp(v[0], Positions.BarrelPosition.position, t);
                    if (Positions.OpticsRoot != null && Positions.OpticPosition != null)
                        Positions.OpticsRoot.transform.position = Vector3.Lerp(v[1], Positions.OpticPosition.position, t);
                    if (Positions.FeederRoot != null && Positions.FeederPosition != null)
                        Positions.FeederRoot.transform.position = Vector3.Lerp(v[2], Positions.FeederPosition.position, t);
                    if (Positions.CylinderRoot != null && Positions.CylinderPosition != null)
                        Positions.CylinderRoot.transform.position = Vector3.Lerp(v[3], Positions.CylinderPosition.position, t);
                }
                else
                {
                    d += Time.deltaTime / (Manager.DetachDuration / 3f);
                    float t = SeparateCurve.Evaluate(d);
                    t = -t * (t - 2);
                    if (Positions.BarrelRoot != null)
                        Positions.BarrelRoot.transform.localPosition = Vector3.Lerp(v[0], Positions.defaultPositions[0], t);
                    if (Positions.OpticsRoot != null)
                        Positions.OpticsRoot.transform.localPosition = Vector3.Lerp(v[1], Positions.defaultPositions[1], t);
                    if (Positions.FeederRoot != null)
                        Positions.FeederRoot.transform.localPosition = Vector3.Lerp(v[2], Positions.defaultPositions[2], t);
                    if (Positions.CylinderRoot != null)
                        Positions.CylinderRoot.transform.localPosition = Vector3.Lerp(v[3], Positions.defaultPositions[3], t);
                }
                yield return null;
            }
        }

        public static float Out(float t)
        {
            const float HALF_PI = Mathf.PI / 2;
            const float A = -13 * HALF_PI;

            return Mathf.Sin(A * (t + 1)) * Mathf.Pow(2, -10 * t) + 1;
        }

        /// <summary>
        /// 
        /// </summary>
        void ShowTextButton()
        {
            if (Customize)
            {
                Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000))
                {
                    bl_AttachButton atb = hit.transform.GetComponent<bl_AttachButton>();
                    if (atb != null && atb != Button.ActiveButton)
                    {
                        Button.SetActiveButton(atb);
                    }
                }
                else
                {
                    Button.SetActiveButton(null);
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        void DetectButton()
        {
            if (Customize && Button.ActiveButton != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (Button.ActiveButton.m_Type == bl_AttachType.Suppressers)
                    {
                        Manager.ChangeAttachWindow(Attachments.Suppressers, bl_AttachType.Suppressers);
                    }
                    else
                    if (Button.ActiveButton.m_Type == bl_AttachType.Sights)
                    {
                        Manager.ChangeAttachWindow(Attachments.Sights, bl_AttachType.Sights);
                    }
                    else
                    if (Button.ActiveButton.m_Type == bl_AttachType.Foregrips)
                    {
                        Manager.ChangeAttachWindow(Attachments.Foregrips, bl_AttachType.Foregrips);
                    }
                    else
                    if (Button.ActiveButton.m_Type == bl_AttachType.Magazine)
                    {
                        Manager.ChangeAttachWindow(Attachments.Magazines, bl_AttachType.Magazine);
                    }
                    else if (Button.ActiveButton.m_Type == bl_AttachType.Camo)
                    {
                        Manager.ShowCamos(WeaponName);
                    }
                }
            }
        }

        public bl_GunInfo GetWeaponInfo() { return bl_GameData.Instance.GetWeapon(bl_CustomizerData.Instance.GetWeapon(WeaponName).GunID); }
        public int GunID() { return bl_CustomizerData.Instance.GetWeapon(WeaponName).GunID; }

#if UNITY_EDITOR

        public void BuildAttachments()
        {
            Attachments.Suppressers.Clear();
            Attachments.Sights.Clear();
            Attachments.Foregrips.Clear();
            Attachments.Magazines.Clear();
            CustomizerInfo info = bl_CustomizerData.Instance.GetWeapon(WeaponName);
            for (int i = 0; i < info.Attachments.Suppressers.Count; i++)
            {
                Attachments.Suppressers.Add(new CustomizerModelInfo());
                Attachments.Suppressers[i].SetInfo(info.Attachments.Suppressers[i]);
            }
            for (int i = 0; i < info.Attachments.Sights.Count; i++)
            {
                Attachments.Sights.Add(new CustomizerModelInfo());
                Attachments.Sights[i].SetInfo(info.Attachments.Sights[i]);
            }
            for (int i = 0; i < info.Attachments.Foregrips.Count; i++)
            {
                Attachments.Foregrips.Add(new CustomizerModelInfo());
                Attachments.Foregrips[i].SetInfo(info.Attachments.Foregrips[i]);
            }
            for (int i = 0; i < info.Attachments.Magazines.Count; i++)
            {
                Attachments.Magazines.Add(new CustomizerModelInfo());
                Attachments.Magazines[i].SetInfo(info.Attachments.Magazines[i]);
            }
        }

        public void RefreshAttachments()
        {
            CustomizerInfo info = bl_CustomizerData.Instance.GetWeapon(WeaponName);
            for (int i = 0; i < info.Attachments.Suppressers.Count; i++)
            {
                if (Attachments.Suppressers.Count - 1 < i)
                {
                    Attachments.Suppressers.Add(new CustomizerModelInfo());
                }
                Attachments.Suppressers[i].SetInfo(info.Attachments.Suppressers[i]);
            }
            for (int i = 0; i < info.Attachments.Sights.Count; i++)
            {
                if (Attachments.Sights.Count - 1 < i)
                {
                    Attachments.Sights.Add(new CustomizerModelInfo());
                }
                Attachments.Sights[i].SetInfo(info.Attachments.Sights[i]);
            }
            for (int i = 0; i < info.Attachments.Foregrips.Count; i++)
            {
                if (Attachments.Foregrips.Count - 1 < i)
                {
                    Attachments.Foregrips.Add(new CustomizerModelInfo());
                }
                Attachments.Foregrips[i].SetInfo(info.Attachments.Foregrips[i]);
            }
            for (int i = 0; i < info.Attachments.Magazines.Count; i++)
            {
                if (Attachments.Magazines.Count - 1 < i)
                {
                    Attachments.Magazines.Add(new CustomizerModelInfo());
                }
                Attachments.Magazines[i].SetInfo(info.Attachments.Magazines[i]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.color = new Color32(0, 221, 221, 255);
            if (Positions.BarrelRoot != null)
            {
                Gizmos.DrawLine(Positions.BarrelRoot.transform.position, Positions.BarrelPosition.position);
                Gizmos.DrawWireSphere(Positions.BarrelPosition.position, 0.07f);
            }

            if (Positions.OpticsRoot != null)
            {
                Gizmos.DrawLine(Positions.OpticsRoot.transform.position, Positions.OpticPosition.position);
                Gizmos.DrawWireSphere(Positions.OpticPosition.position, 0.07f);
            }

            if (Positions.FeederRoot != null)
            {
                Gizmos.DrawLine(Positions.FeederRoot.transform.position, Positions.FeederPosition.position);
                Gizmos.DrawWireSphere(Positions.FeederPosition.position, 0.07f);
            }

            if (Positions.CylinderRoot != null)
            {
                Gizmos.DrawLine(Positions.CylinderRoot.transform.position, Positions.CylinderPosition.position);
                Gizmos.DrawWireSphere(Positions.CylinderPosition.position, 0.07f);
            }

            Gizmos.color = new Color32(77, 255, 186, 100);

            if (Positions.BarrelPosition != null)
            {
                Gizmos.DrawSphere(Positions.BarrelPosition.position, 0.07f);
            }
            if (Positions.OpticPosition != null)
            {
                Gizmos.DrawSphere(Positions.OpticPosition.position, 0.07f);
            }
            if (Positions.FeederPosition != null)
            {
                Gizmos.DrawSphere(Positions.FeederPosition.position, 0.07f);
            }
            if (Positions.CylinderPosition != null)
            {
                Gizmos.DrawSphere(Positions.CylinderPosition.position, 0.07f);
            }
        }
#endif
    }
}