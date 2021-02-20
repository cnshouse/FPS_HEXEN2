using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MFPS.Core.Motion;

public class bl_CameraShaker : MonoBehaviour
{
    private Vector3 OrigiPosition;

    private Dictionary<string, ShakerPresent> shakersRunning = new Dictionary<string, ShakerPresent>();
    private Transform m_Transform;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        m_Transform = transform;
        GetDefaultPosition();
    }

    public void GetDefaultPosition()
    {
        if (m_Transform == null) m_Transform = transform;
        OrigiPosition = m_Transform.localEulerAngles;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onLocalPlayerShake += OnShake;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onLocalPlayerShake -= OnShake;
    }

    void OnShake(ShakerPresent present, string key, float influence)
    {
        AddShake(present, key, influence);
    }

    public void AddShake(ShakerPresent present, string key, float influenced = 1)
    {
        if (present == null) return;
        bool first = shakersRunning.Count <= 0;
        if (shakersRunning.ContainsKey(key))
        {
            shakersRunning.Remove(key);
        }
        present.currentTime = 1;
        present.starting = false;
        present.influence = influenced;
        shakersRunning.Add(key, present);

        if (first)
        {
            StartCoroutine(UpdateShake());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void RemoveShake(string key)
    {
        if (shakersRunning.ContainsKey(key))
        {
            shakersRunning.Remove(key);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdateShake()
    {
        Vector3 pos = Vector3.zero;
        while (true)
        {
            if(shakersRunning.Count <= 0) { yield break; }
            pos = Vector2.zero;
            for (int i = 0; i < shakersRunning.Count; i++)
            {
                ShakerPresent p = shakersRunning.Values.ElementAt(i);
                if (p.starting)
                {
                    p.currentTime += Time.deltaTime / (p.Duration * p.fadeInTime);
                    if(p.currentTime >= 1) { p.currentTime = 1; p.starting = false; }
                }
                else
                {
                    if(!p.Loop)
                    p.currentTime -= Time.deltaTime / (p.Duration - (p.Duration * p.fadeInTime));
                }             
                float amplitude = p.amplitude * p.currentTime;
                pos += Shake(amplitude, p.frequency, p.octaves, p.persistance, p.lacunarity, p.burstFrequency, p.burstContrast, p.influence);
                if (!p.starting && p.currentTime <= 0)
                {
                    shakersRunning.Remove(shakersRunning.ElementAt(i).Key);
                }
            }
            m_Transform.localRotation = Quaternion.Euler( OrigiPosition + pos);
            yield return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static Vector3 Shake(float amplitude, float frequency, int octaves, float persistance, float lacunarity, float burstFrequency, int burstContrast, float influence)
    {
        float valX = 0;
        float valY = 0;
        amplitude = amplitude * 8;
        float iAmplitude = 1;
        float iFrequency = frequency;
        float maxAmplitude = 0;
        float time = Time.time;
        // Burst frequency
        float burstCoord = time / (1 - burstFrequency);

        // Sample diagonally trough perlin noise
        float burstMultiplier = Mathf.PerlinNoise(burstCoord, burstCoord);

        //Apply contrast to the burst multiplier using power, it will make values stay close to zero and less often peak closer to 1
        burstMultiplier = Mathf.Pow(burstMultiplier, burstContrast);

        for (int i = 0; i < octaves; i++) // Iterate trough octaves
        {
            float noiseFrequency = time / (1 - iFrequency) / 10;

            float perlinValueX = Mathf.PerlinNoise(noiseFrequency, 0.5f);
            float perlinValueY = Mathf.PerlinNoise(0.5f, noiseFrequency);

            // Adding small value To keep the average at 0 and   *2 - 1 to keep values between -1 and 1.
            perlinValueX = (perlinValueX + 0.0352f) * 2 - 1;
            perlinValueY = (perlinValueY + 0.0345f) * 2 - 1;

            valX += perlinValueX * iAmplitude;
            valY += perlinValueY * iAmplitude;

            // Keeping track of maximum amplitude for normalizing later
            maxAmplitude += iAmplitude;

            iAmplitude *= persistance;
            iFrequency *= lacunarity;
        }

        valX *= burstMultiplier;
        valY *= burstMultiplier;

        // normalize
        valX /= maxAmplitude;
        valY /= maxAmplitude;

        valX *= (amplitude * influence);
        valY *= (amplitude * influence);

        return new Vector3(valX, valY, 0);
    }
}