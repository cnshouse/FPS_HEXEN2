using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace MFPS.GameModes.Demolition
{
    public class bl_DemolitionBombEffects : MonoBehaviour
    {
        [Header("Settings")]
        public float sequenceRate = 2;
        public float maxLightIntensity = 4;
        [Header("References")]
        public Light redLight;
        public AudioClip countSound;
        private AudioSource aSource;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            aSource = GetComponent<AudioSource>();
            aSource.clip = countSound;
            StartCoroutine(Do());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            aSource.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator Do()
        {
            float d = 0;
            float rate = sequenceRate;
            while (true)
            {
                while (d < 1)
                {
                    d += Time.deltaTime / rate;
                    redLight.intensity = Mathf.PingPong(d * (maxLightIntensity * 2), maxLightIntensity);
                    if (bl_DemolitionBombManager.Instance.detonationTime <= (bl_Demolition.Instance.DetonationTime * 0.5f))
                    {
                        rate = Mathf.Lerp(sequenceRate, 0.1f, 1 - bl_DemolitionBombManager.Instance.getFinalCountPercentage);
                    }
                    yield return null;
                }
                d = 0;
                aSource.Play();
                yield return null;
            }
        }
    }
}