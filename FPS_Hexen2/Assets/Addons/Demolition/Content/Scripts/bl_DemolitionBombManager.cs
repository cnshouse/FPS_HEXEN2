using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using HashTable = ExitGames.Client.Photon.Hashtable;

namespace MFPS.GameModes.Demolition
{
    public class bl_DemolitionBombManager : bl_PhotonHelper
    {
        [Header("References")]
        public bl_DemolitionBomb Bomb;
        public GameObject ExplosionPrefab;

        public int carrierActorNumber { get; set; }
        public bool isPlating { get; set; }
        public float detonationTime { get; set; }

        public bool isPlantButtonPressed { get; set; }
        public bool isDefuseButtonPressed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            InputControl();
        }

        /// <summary>
        /// 
        /// </summary>
        void InputControl()
        {
            if (bl_UtilityHelper.isMobile) return;

            //if player is inside of a bomb zone and carries the bomb
            if (canLocalPlant && Bomb.isAvailableToPlant)
            {
                //and him press the plant button
                if (Input.GetKeyDown(bl_Demolition.Instance.plantKey))
                {
                    PlantBomb();
                }
                else if (Input.GetKeyUp(bl_Demolition.Instance.plantKey))//if for some reason him is not keep pressing the plant button
                {
                    CancelPlant(false);
                }
            }
            else if (bl_Demolition.Instance.bombInSight && Bomb.isAvailableToDefuse)
            {
                if (Input.GetKeyDown(bl_Demolition.Instance.plantKey))
                {
                    DefuseBomb();
                }
                else if (Input.GetKeyUp(bl_Demolition.Instance.plantKey))//if for some reason him is not keep pressing the plant button
                {
                    CancelDefuse(false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnPlantButton(bool press)
        {
            if (!canLocalPlant || !Bomb.isAvailableToPlant) return;

            if (press) PlantBomb();
            else CancelPlant(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnDefuseButton(bool press)
        {
            if (!bl_Demolition.Instance.bombInSight || !Bomb.isAvailableToDefuse) return;

            if (press) DefuseBomb();
            else CancelDefuse(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopAll()
        {
            StopAllCoroutines();
            CancelInvoke();
            Bomb.bombEffects.SetActive(false);
            CancelDefuse(false);
            CancelPlant(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void PlantBomb()
        {
            StartCoroutine("DoPlant");
            bl_DemolitionUI.Instance.BarImg.fillAmount = 0;
            bl_DemolitionUI.Instance.ProgressUI.SetActive(true);
            bl_DemolitionUI.Instance.ShowPlantGuide(false);
            //here you can replace the BlockAllWeapons() with your custom code in order to show a bomb activation hand animation instead of just hide the weapons.      
            bl_GameManager.Instance.LocalPlayer.GetComponent<bl_PlayerNetwork>().gunManager.BlockAllWeapons();
            bl_MFPS.LocalPlayerReferences.firstPersonController.isControlable = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CancelPlant(bool byDeath)
        {
            StopCoroutine("DoPlant");
            bl_DemolitionUI.Instance.ProgressUI.SetActive(false);
            if (!byDeath)
            {
                if (bl_GameManager.Instance.LocalPlayer != null)
                {
                    bl_GameManager.Instance.LocalPlayer.GetComponent<bl_PlayerNetwork>().gunManager.ReleaseWeapons(true);
                    bl_MFPS.LocalPlayerReferences.firstPersonController.isControlable = true;
                }
                bl_DemolitionUI.Instance.ShowPlantGuide(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void DefuseBomb()
        {
            StartCoroutine(nameof(DoDefuse));
            bl_DemolitionUI.Instance.BarImg.fillAmount = 1;
            bl_DemolitionUI.Instance.ProgressUI.SetActive(true);
            bl_DemolitionUI.Instance.ShowDefuseGuide(false);
            //here you can replace the BlockAllWeapons() with your custom code in order to show a bomb activation hand animation instead of just hide the weapons.
            bl_GameManager.Instance.LocalPlayer.GetComponent<bl_PlayerNetwork>().gunManager.BlockAllWeapons();
            bl_MFPS.LocalPlayerReferences.firstPersonController.isControlable = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CancelDefuse(bool byDeath)
        {
            StopCoroutine(nameof(DoDefuse));
            bl_DemolitionUI.Instance.ProgressUI.SetActive(false);
            bl_DemolitionUI.Instance.ShowDefuseGuide(false);
            if (!byDeath)
            {
                if (bl_GameManager.Instance.LocalPlayer != null)
                {
                    bl_GameManager.Instance.LocalPlayer.GetComponent<bl_PlayerNetwork>().gunManager.ReleaseWeapons(true);
                    bl_MFPS.LocalPlayerReferences.firstPersonController.isControlable = true;
                }
                bl_DemolitionUI.Instance.ShowDefuseGuide(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartDetonationCountDown(bool start)
        {
            if (start) { StartCoroutine("DoDetonationCountDown"); }
            else { StopCoroutine("DoDetonationCountDown"); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator DoPlant()
        {
            float plantTime = 0;
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime / bl_Demolition.Instance.plantDuration;
                //d = normalized plant time (0 - 1)
                //plantTime = complete countdown time (0 - plantDuration)
                plantTime = bl_Demolition.Instance.plantDuration / 1;
                bl_DemolitionUI.Instance.UpdateProgress(plantTime, d);
                yield return null;
            }
            //plant complete
            bl_DemolitionUI.Instance.ProgressUI.SetActive(false);
            bl_DemolitionUI.Instance.ShowPlantGuide(false);
            //send plantation event
            Bomb.OnLocalPlant();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator DoDefuse()
        {
            float plantTime = 0;
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime / bl_Demolition.Instance.defuseDuration;
                //d = normalized plant time (0 - 1)
                //plantTime = complete countdown time (0 - plantDuration)
                plantTime = bl_Demolition.Instance.defuseDuration * 1 - d;
                bl_DemolitionUI.Instance.UpdateProgress(plantTime, 1 - d);
                yield return null;
            }
            //defuse complete
            bl_DemolitionUI.Instance.ProgressUI.SetActive(false);
            bl_DemolitionUI.Instance.ShowPlantGuide(false);
            //send defuse event
            Bomb.OnLocalDefuse();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator DoDetonationCountDown()
        {
            while (true)
            {
                detonationTime = bl_Demolition.Instance.DetonationTime - ((float)PhotonNetwork.Time - Bomb.plantedTime);
                if (detonationTime <= 0)
                {
                    //explosion and round finish with the terrorist team win
                    Bomb.bombEffects.SetActive(false);
                    bl_DemolitionUI.Instance.onPlantUI.SetActive(false);
                    //instance explosions effects
                    StartCoroutine(ExplosionSequence());
                    yield return new WaitForSeconds(1);
                    if (PhotonNetwork.IsMasterClient)
                    {
                        //send the round finish event and let others know who win
                        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.DemolitionEvent, new HashTable()
                {
                {"type", bl_Demolition.DemolitionEventType.RoundFinish },
                {"finalType",1 },
                {"winner",bl_Demolition.Instance.terroristsTeam }
                });
                    }
                    //
                    yield break;
                }
                yield return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator ExplosionSequence()
        {
            //change the 3 for the number of explosion that you want
            for (int i = 0; i < 3; i++)
            {
                //select a random position around the bomb to instance the explosion effect
                Vector3 rp = Bomb.transform.position + (Random.insideUnitSphere * 5);
                rp.y = Bomb.transform.position.y;
                Instantiate(ExplosionPrefab, rp, Quaternion.identity);
                yield return new WaitForSeconds(0.5f);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnSelectPlayerToCarrieBomb(HashTable data)
        {
            Bomb.PickUp(data);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnLocalDeath()
        {
            if (carrierActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                Vector3 playerPos = bl_GameManager.Instance.LocalPlayer.transform.position;
                Vector3 dropPosition = playerPos;
                Quaternion rot = Quaternion.identity;
                RaycastHit hit;
                if (Physics.Raycast(playerPos, Vector3.down * 5, out hit, 5, Bomb.layerMask, QueryTriggerInteraction.Ignore))
                {
                    dropPosition = hit.point;
                    rot = Quaternion.LookRotation(hit.normal, Vector3.up);
                }
                //drop the bomb activator
                var data = bl_UtilityHelper.CreatePhotonHashTable();
                data.Add("type", bl_DemolitionBomb.BombStatus.Droped);
                data.Add("position", dropPosition);
                data.Add("rotation", rot);
                bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.DMBombEvent, data);
                CancelPlant(true);
                CancelDefuse(true);
                Bomb.transform.parent = null;
            }
        }

        public bool isLocalPlayerTheCarrier { get { return carrierActorNumber == PhotonNetwork.LocalPlayer.ActorNumber; } }
        public bool canLocalPlant { get { return bl_Demolition.Instance.isLocalInZone && bl_Demolition.Instance.canPlant; } }
        public float getFinalCountPercentage { get { if (detonationTime > 0) { return detonationTime / bl_Demolition.Instance.DetonationTime; } else { return 0; } } }

        private static bl_DemolitionBombManager _instance;
        public static bl_DemolitionBombManager Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_DemolitionBombManager>(); }
                return _instance;
            }
        }
    }
}