using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace MFPS.Addon.Customizer
{
    [CustomEditor(typeof(bl_CustomizerWeapon))]
    public class bl_CustomizerWeaponEditor : Editor
    {

        bl_CustomizerWeapon script;
        SerializedProperty attac;
        private string weaponName = "";
        SerializedProperty camor;

        //IMPORT
        public bl_Customizer _CustomizerWeapon;
        public Transform _CustomizerMesh;
        public Transform _TargetMesh;
        private bool openImportMenu = false;
        Dictionary<string, bl_Customizer> allCustomizers = new Dictionary<string, bl_Customizer>();
        Dictionary<string, Transform> allCustomizersMeshs = new Dictionary<string, Transform>();
        int customizerSelected = 0;
        bl_CustomizerManager customizerManager;
        //

        private void OnEnable()
        {
            script = (bl_CustomizerWeapon)target;
            attac = serializedObject.FindProperty("Attachments");
            camor = serializedObject.FindProperty("CamoRender");
            if (string.IsNullOrEmpty(script.WeaponName))
            {
                script.WeaponName = bl_CustomizerData.Instance.Weapons[script.WeaponID].WeaponName;
            }
            weaponName = script.WeaponName;
            script.isFPWeapon = script.GetComponent<bl_Gun>() != null;
            customizerManager = FindObjectOfType<bl_CustomizerManager>();
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal("box");
            script.WeaponID = EditorGUILayout.Popup("Customizer ID", script.WeaponID, bl_CustomizerData.Instance.GetWeaponStringArray(), EditorStyles.toolbarDropDown);
            GUILayout.Space(5);
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                script.RefreshAttachments();
            }
            GUILayout.EndHorizontal();
            script.isFPWeapon = EditorGUILayout.ToggleLeft("is First Person Weapon", script.isFPWeapon, EditorStyles.toolbarButton);
            script.ApplyOnStart = EditorGUILayout.ToggleLeft("Apply On Start", script.ApplyOnStart, EditorStyles.toolbarButton);
            if (GUI.changed)
            {
                script.WeaponName = bl_CustomizerData.Instance.Weapons[script.WeaponID].WeaponName;
                if (script.WeaponName != weaponName)
                {
                    script.BuildAttachments();
                    weaponName = script.WeaponName;
                }
            }

            serializedObject.Update();
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(camor, true);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(attac, true);
            GUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal("box");
            GUI.enabled = customizerManager != null && script.isFPWeapon;
            if (GUILayout.Button("Import", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                openImportMenu = !openImportMenu;
                if (openImportMenu)
                {
                    //ActiveEditorTracker.sharedTracker.isLocked = openImportMenu;

                    allCustomizers.Clear();
                    bl_CustomizerManager cm = FindObjectOfType<bl_CustomizerManager>();
                    if (cm != null)
                    {
                        bl_Customizer[] all = cm.transform.GetComponentsInChildren<bl_Customizer>(true);
                        for (int i = 0; i < all.Length; i++)
                        {
                            allCustomizers.Add(all[i].gameObject.name, all[i]);
                            if (all[i].WeaponID == script.WeaponID)
                            {
                                _CustomizerWeapon = all[i];
                                customizerSelected = i;
                            }
                        }
                        if (script.CamoRender.Render == null)
                        {
                            SetupPlayerWeapons();
                        }
                    }
                }
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Update Attachments", EditorStyles.toolbarButton, GUILayout.Width(200)))
            {
                UpdateAttachemtns();
            }
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            if (openImportMenu)
            {
                GUILayout.BeginVertical("box");
                if (script.CamoRender.Render == null)
                {
                    if (allCustomizers.Count > 0)
                    {
                        if (GUILayout.Button("Setup FP and TP Weapons"))
                        {
                            SetupPlayerWeapons();
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Can't find any customizer weapon in the scene, be sure you're in the Customizer scene", MessageType.Warning);
                    }
                }
                else
                {

                    if (allCustomizers.Count > 0)
                    {
                        customizerSelected = EditorGUILayout.Popup("Customizer Weapon", customizerSelected, allCustomizers.Keys.ToArray());
                        _CustomizerWeapon = allCustomizers.Values.ElementAt(customizerSelected);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Can't find any customizer weapon in the scene, be sure you're in the Customizer scene", MessageType.Warning);
                    }
                    if (_CustomizerWeapon != null && _CustomizerWeapon.gameObject.name != "***TEMPLATE***")
                    {
                        if (allCustomizersMeshs.Count <= 0)
                        {
                            MeshRenderer[] all = _CustomizerWeapon.Positions.ModelParent.GetComponentsInChildren<MeshRenderer>(true);
                            for (int i = 0; i < all.Length; i++)
                            {
                                allCustomizersMeshs.Add(all[i].name, all[i].transform);
                            }
                        }
                        else
                        {
                            _CustomizerMesh = _CustomizerWeapon.CamoRender.Render.transform;
                            _CustomizerMesh = EditorGUILayout.ObjectField("Customizer Mesh", _CustomizerMesh, typeof(Transform), true) as Transform;
                            if (_CustomizerMesh != null)
                            {
                                if (_TargetMesh == null)
                                {
                                    if (script.CamoRender.Render != null) { _TargetMesh = script.CamoRender.Render.transform; }
                                    else
                                    {
                                        if (GUILayout.Button("Search Mesh"))
                                        {
                                            MeshFilter[] all = script.transform.GetComponentsInChildren<MeshFilter>(true);
                                            Mesh m = _CustomizerMesh.GetComponent<MeshFilter>().sharedMesh;
                                            for (int i = 0; i < all.Length; i++)
                                            {
                                                if (all[i].sharedMesh == m)
                                                {
                                                    _TargetMesh = all[i].transform;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    _TargetMesh = EditorGUILayout.ObjectField("Target Weapon Mesh", _TargetMesh, typeof(Transform), true) as Transform;
                                }
                            }
                        }
                    }

                    GUI.enabled = (_CustomizerMesh != null && _CustomizerWeapon != null && _TargetMesh != null);
                    if (GUILayout.Button("Transfer", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    {
                        Transfer();
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();
            }

            EditorGUI.EndChangeCheck();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        void UpdateAttachemtns()
        {
            bl_Customizer[] all = customizerManager.transform.GetComponentsInChildren<bl_Customizer>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].WeaponID == script.WeaponID)
                {
                    _CustomizerWeapon = all[i];
                    customizerSelected = i;
                }
            }
            int gunID = script.GetComponent<bl_Gun>().GunID;
            bl_NetworkGun ngun = script.transform.root.GetComponentInChildren<bl_PlayerNetwork>().NetworkGuns.Find(x => x.LocalGun.GunID == gunID);
            bl_CustomizerWeapon networkScript = ngun.GetComponent<bl_CustomizerWeapon>();

            CompareLists(_CustomizerWeapon.Attachments.Foregrips, ref script.Attachments.Foregrips, ref networkScript.Attachments.Foregrips);
            CompareLists(_CustomizerWeapon.Attachments.Magazines, ref script.Attachments.Magazines, ref networkScript.Attachments.Magazines);
            CompareLists(_CustomizerWeapon.Attachments.Sights, ref script.Attachments.Sights, ref networkScript.Attachments.Sights);
            CompareLists(_CustomizerWeapon.Attachments.Suppressers, ref script.Attachments.Suppressers, ref networkScript.Attachments.Suppressers);
            EditorUtility.SetDirty(networkScript);
            EditorUtility.SetDirty(target);
        }

        void CompareLists(List<CustomizerModelInfo> customizerList, ref List<CustomizerModelInfo> localList, ref List<CustomizerModelInfo> netList)
        {
            Transform localParent = null;
            Transform networkParent = null;

            for (int i = 0; i < customizerList.Count; i++)
            {
                if (i > localList.Count - 1) { localList.Add(new CustomizerModelInfo()); }
                if (i > netList.Count - 1) { netList.Add(new CustomizerModelInfo()); }

                localList[i].Name = customizerList[i].Name;
                localList[i].ID = customizerList[i].ID;
                netList[i].Name = customizerList[i].Name;
                netList[i].ID = customizerList[i].ID;

                if (localList[i].Model != null && localList[i].Model.name == customizerList[i].Model.name)
                {
                    localList[i].Model.transform.localPosition = customizerList[i].Model.transform.localPosition;
                    localList[i].Model.transform.localEulerAngles = customizerList[i].Model.transform.localEulerAngles;
                    localList[i].Model.transform.localScale = customizerList[i].Model.transform.localScale;
                    if (localParent == null) { localParent = localList[i].Model.transform.parent; }
                }
                else if (localList[i].Model == null && customizerList[i].Model != null)
                {
                    string parentName = customizerList[i].Model.transform.parent.name;
                    GameObject clone = Instantiate(customizerList[i].Model) as GameObject;
                    clone.name = customizerList[i].Model.name;

                    clone.transform.parent = localParent.gameObject.FindInChildren(parentName).transform;
                    clone.transform.localPosition = customizerList[i].Model.transform.localPosition;
                    clone.transform.localEulerAngles = customizerList[i].Model.transform.localEulerAngles;
                    clone.transform.localScale = customizerList[i].Model.transform.localScale;
                    localList[i].Model = clone;
                }
                //TP
                if (netList[i].Model != null && netList[i].Model.name == customizerList[i].Model.name)
                {
                    netList[i].Model.transform.localPosition = customizerList[i].Model.transform.localPosition;
                    netList[i].Model.transform.localEulerAngles = customizerList[i].Model.transform.localEulerAngles;
                    netList[i].Model.transform.localScale = customizerList[i].Model.transform.localScale;
                    if (networkParent == null) { networkParent = netList[i].Model.transform.parent; }
                }
                else if (netList[i].Model == null && customizerList[i].Model != null)
                {
                    string parentName = customizerList[i].Model.transform.parent.name;
                    GameObject clone = Instantiate(customizerList[i].Model) as GameObject;
                    clone.name = customizerList[i].Model.name;

                    clone.transform.parent = networkParent.gameObject.FindInChildren(parentName).transform;
                    clone.transform.localPosition = customizerList[i].Model.transform.localPosition;
                    clone.transform.localEulerAngles = customizerList[i].Model.transform.localEulerAngles;
                    clone.transform.localScale = customizerList[i].Model.transform.localScale;
                    netList[i].Model = clone;
                }
            }
        }

        void Transfer()
        {
            Transform oldRig = _CustomizerWeapon.Positions.BarrelRoot.transform.parent;
            GameObject clone = Instantiate(oldRig.gameObject) as GameObject;
            clone.name = "Attachments [FP]";
            Dictionary<string, string> paths = new Dictionary<string, string>();
            _CustomizerWeapon.Attachments.Sights.ForEach(x => { if (x.Model != null) { paths.Add(x.Name, AnimationUtility.CalculateTransformPath(x.Model.transform, oldRig)); } });
            _CustomizerWeapon.Attachments.Suppressers.ForEach(x => { if (x.Model != null) { paths.Add(x.Name, AnimationUtility.CalculateTransformPath(x.Model.transform, oldRig)); } });
            _CustomizerWeapon.Attachments.Magazines.ForEach(x => { if (x.Model != null) { paths.Add(x.Name, AnimationUtility.CalculateTransformPath(x.Model.transform, oldRig)); } });
            _CustomizerWeapon.Attachments.Foregrips.ForEach(x => { if (x.Model != null) { paths.Add(x.Name, AnimationUtility.CalculateTransformPath(x.Model.transform, oldRig)); } });

            clone.transform.parent = oldRig.parent;
            clone.transform.localPosition = oldRig.localPosition;
            clone.transform.localEulerAngles = oldRig.localEulerAngles;
            clone.transform.localScale = oldRig.localScale;

            clone.transform.parent = _CustomizerMesh;
            Vector3[] data = new Vector3[3];
            data[0] = clone.transform.localPosition;
            data[1] = clone.transform.localEulerAngles;
            data[2] = clone.transform.localScale;

            clone.transform.parent = _TargetMesh;
            clone.transform.localPosition = data[0];
            clone.transform.localEulerAngles = data[1];
            clone.transform.localScale = data[2];

            script.Attachments.Sights.ForEach(x => { if (paths.ContainsKey(x.Name)) { x.Model = FindChild(clone, paths[x.Name]); } });
            script.Attachments.Suppressers.ForEach(x => { if (paths.ContainsKey(x.Name)) { x.Model = FindChild(clone, paths[x.Name]); } });
            script.Attachments.Foregrips.ForEach(x => { if (paths.ContainsKey(x.Name)) { x.Model = FindChild(clone, paths[x.Name]); } });
            script.Attachments.Magazines.ForEach(x => { if (paths.ContainsKey(x.Name)) { x.Model = FindChild(clone, paths[x.Name]); } });

            if (script.isFPWeapon)
            {
                //clone.layer = LayerMask.NameToLayer("Weapon");
                foreach (Transform t in clone.GetComponentsInChildren<Transform>(true))
                {
                    t.gameObject.layer = LayerMask.NameToLayer("Weapons");
                }
            }
            EditorUtility.SetDirty(target);

            //TP
            int gunID = script.GetComponent<bl_Gun>().GunID;
            bl_NetworkGun ngun = script.transform.root.GetComponentInChildren<bl_PlayerNetwork>().NetworkGuns.Find(x => {
                if (x.LocalGun == null) return false;
                return x.LocalGun.GunID == gunID;
            });

            if (ngun != null)
            {
                bl_CustomizerWeapon networkCustomizer = ngun.GetComponent<bl_CustomizerWeapon>();
                if (networkCustomizer == null) { networkCustomizer = ngun.gameObject.AddComponent<bl_CustomizerWeapon>(); }
                networkCustomizer.isFPWeapon = false;
                networkCustomizer.WeaponID = script.WeaponID;
                networkCustomizer.WeaponName = script.WeaponName;
                networkCustomizer.BuildAttachments();
                if (networkCustomizer.CamoRender == null) { networkCustomizer.CamoRender = new CustomizerCamoRender(); }
                networkCustomizer.CamoRender.MaterialID = script.CamoRender.MaterialID;

                //Transform networkTarget = ngun
                MeshFilter[] meshes = ngun.GetComponentsInChildren<MeshFilter>();
                Mesh mesh = script.CamoRender.Render.GetComponent<MeshFilter>().sharedMesh;
                Transform networkTarget = null;
                for (int i = 0; i < meshes.Length; i++)
                {
                    if (meshes[i].sharedMesh == mesh)
                    {
                        networkCustomizer.CamoRender.Render = meshes[i].GetComponent<MeshRenderer>();
                        networkTarget = networkCustomizer.CamoRender.Render.transform;
                    }
                }

                clone = Instantiate(oldRig.gameObject) as GameObject;
                clone.name = "Attachments [TP]";

                clone.transform.parent = networkTarget;
                clone.transform.localPosition = data[0];
                clone.transform.localEulerAngles = data[1];
                clone.transform.localScale = data[2];

                networkCustomizer.Attachments.Sights.ForEach(x => { if (paths.ContainsKey(x.Name)) { x.Model = FindChild(clone, paths[x.Name]); } });
                networkCustomizer.Attachments.Suppressers.ForEach(x => { if (paths.ContainsKey(x.Name)) { x.Model = FindChild(clone, paths[x.Name]); } });
                networkCustomizer.Attachments.Foregrips.ForEach(x => { if (paths.ContainsKey(x.Name)) { x.Model = FindChild(clone, paths[x.Name]); } });
                networkCustomizer.Attachments.Magazines.ForEach(x => { if (paths.ContainsKey(x.Name)) { x.Model = FindChild(clone, paths[x.Name]); } });
                EditorUtility.SetDirty(networkCustomizer);

                Debug.Log("All done!");
            }
            else
            {
                Debug.Log("Could not find the network gun with GunID: " + gunID);
            }
            ActiveEditorTracker.sharedTracker.isLocked = false;
        }

        GameObject FindChild(GameObject go, string cName)
        {
            var t = go.transform.Find(cName);
            if (t != null) return t.gameObject;
            return null;
        }

        void SetupPlayerWeapons()
        {
            bl_Gun gun = script.GetComponent<bl_Gun>();
            if (gun != null)
            {
                int gunID = gun.GunID;
                script.WeaponID = bl_CustomizerData.Instance.GetCustomizerID(gunID);
                script.WeaponName = bl_CustomizerData.Instance.Weapons[script.WeaponID].WeaponName;
                bl_CustomizerManager cm = FindObjectOfType<bl_CustomizerManager>();
                if (cm != null)
                {
                    bl_Customizer[] all = cm.transform.GetComponentsInChildren<bl_Customizer>(true);
                    for (int i = 0; i < all.Length; i++)
                    {
                        if (all[i].WeaponID == script.WeaponID)
                        {
                            _CustomizerWeapon = all[i];
                            customizerSelected = i;
                        }
                    }
                    if (_CustomizerWeapon != null)
                    {
                        script.BuildAttachments();
                        script.CamoRender.MaterialID = _CustomizerWeapon.CamoRender.MaterialID;
                        Mesh mesh = _CustomizerWeapon.CamoRender.Render.GetComponent<MeshFilter>().sharedMesh;
                        MeshFilter[] allm = script.transform.GetComponentsInChildren<MeshFilter>(true);
                        for (int i = 0; i < allm.Length; i++)
                        {
                            if (allm[i].sharedMesh == mesh)
                            {
                                script.CamoRender.Render = allm[i].GetComponent<MeshRenderer>();
                                _TargetMesh = script.CamoRender.Render.transform;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Can't find the Customizer weapon with GunID: " + gunID);
                    }
                }
            }
            else
            {
                Debug.LogWarning("You only can setup customizer weapon from the FPWeapon");
            }
        }
    }
}