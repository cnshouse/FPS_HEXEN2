using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.GameModes.Demolition
{
    public class bl_BombDetonator : MonoBehaviour
    {
        [SerializeField, Range(1, 10)] private float PlantTime = 7;

        [Header("REFERENCES")]
        [SerializeField] private AnimationClip PlantingAnimation;

        private bool isInZone = false;
        private Animation HandAnim;
        private bool isPlanted = false;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            HandAnim = GetComponentInChildren<Animation>();
            bl_DemolitionUI.Instance.BarImg.fillAmount = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            if (isInZone && !isPlanted)
            {
                OnZone();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnZone()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartCoroutine("DoPlanting");
            }
            else if (Input.GetKeyUp(KeyCode.F))
            {
                bl_DemolitionUI.Instance.ProgressUI.SetActive(false);
                bl_DemolitionUI.Instance.inZoneText.gameObject.SetActive(true);
                bl_DemolitionUI.Instance.BarImg.fillAmount = 0;
                StopCoroutine("DoPlanting");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator DoPlanting()
        {
            if (HandAnim != null)
            {
                HandAnim.Rewind(PlantingAnimation.name);
                HandAnim.Play(PlantingAnimation.name);
            }
            bl_DemolitionUI.Instance.inZoneText.gameObject.SetActive(false);
            bl_DemolitionUI.Instance.ProgressUI.SetActive(true);
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime / PlantTime;
                bl_DemolitionUI.Instance.BarImg.fillAmount = d;
                yield return null;
            }

            bl_DemolitionUI.Instance.ProgressUI.SetActive(false);
            Plant();
        }

        /// <summary>
        /// 
        /// </summary>
        void Plant()
        {
            isPlanted = true;
            Vector3 position = transform.root.position;
            Quaternion rotation = Quaternion.identity;
            Vector3 f = transform.root.position + (transform.root.forward * 0.25f);
            Ray r = new Ray(f, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, 2))
            {
                position = hit.point;
                rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            bl_DemolitionUI.Instance.ProgressUI.SetActive(false);
            //  bl_Demolition.Instance.PlantBomb(position, rotation);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            bl_EventHandler.onLocalPlayerDeath += LocalPlayerDeath;
            bl_Demolition.EnterInZone += OnEnterInZone;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.onLocalPlayerDeath -= LocalPlayerDeath;
            bl_Demolition.EnterInZone -= OnEnterInZone;
        }

        /// <summary>
        /// 
        /// </summary>
        void LocalPlayerDeath()
        {
            OnEnterInZone(false);
        }

        /// <summary>
        /// 
        /// </summary>
        void OnEnterInZone(bool enter)
        {
            isInZone = enter;
            bl_DemolitionUI.Instance.inZoneText.gameObject.SetActive(enter);
            if (!enter)
            {
                bl_DemolitionUI.Instance.ProgressUI.SetActive(false);
            }
        }
    }
}