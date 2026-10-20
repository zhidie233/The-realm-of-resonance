using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CameraImpulse : CameraImpulseBase
{
    #region Private members
    private Vector3 OrigiPosition;
    private Dictionary<string, ShakePreset> shakersRunning = new Dictionary<string, ShakePreset>();
    private Transform m_Transform;
    private Vector3 tempVector = Vector3.zero;
    float valX = 0;
    float valY = 0;
    float iAmplitude = 1;
    float iFrequency = 0;
    float maxAmplitude = 0;
    #endregion

    private void Awake()
    {
        m_Transform = transform;
        SetCurrentAsOriginPosition();
    }

    public override void SetCurrentAsOriginPosition()
    {
        if (m_Transform == null) m_Transform = transform;
        OrigiPosition = m_Transform.localEulerAngles;
    }

    private void OnEnable()
    {
        GameEvents.onLocalPlayerShake += OnShake;
    }

    private void OnDisable()
    {
        GameEvents.onLocalPlayerShake -= OnShake;
    }

    public override void SetShake(ShakePreset present, string key)
    {
        AddShake(present, key, 1);
    }

    public override void Stop()
    {
        StopAllCoroutines();
        shakersRunning.Clear();
        m_Transform.localRotation = Quaternion.Euler(OrigiPosition);
    }

    void OnShake(ShakePreset present, string key, float influence)
    {
        AddShake(present, key, influence);
    }

    public void AddShake(ShakePreset present, string key, float influenced = 1)
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

    public override void RemoveShake(string key)
    {
        if (shakersRunning.ContainsKey(key))
        {
            shakersRunning.Remove(key);
        }
    }

    IEnumerator UpdateShake()
    {
        Vector3 pos;
        while (true)
        {
            if (shakersRunning.Count <= 0) { yield break; }
            pos = Vector2.zero;
            ShakePreset p;
            for (int i = 0; i < shakersRunning.Count; i++)
            {
                p = shakersRunning.Values.ElementAt(i);
                if (p.starting)
                {
                    p.currentTime += Time.deltaTime / (p.Duration * p.fadeInTime);
                    if (p.currentTime >= 1) { p.currentTime = 1; p.starting = false; }
                }
                else
                {
                    if (!p.Loop)
                        p.currentTime -= Time.deltaTime / (p.Duration - (p.Duration * p.fadeInTime));
                }
                float amplitude = p.amplitude * p.currentTime;

                if (p.shakeMethod == ShakePreset.ShakeMethod.PerlinNoise)
                    pos += Shake(amplitude, p.frequency, p.octaves, p.persistance, p.lacunarity, p.burstFrequency, p.burstContrast, p.influence);
                else if (p.shakeMethod == ShakePreset.ShakeMethod.RandomSet)
                    pos += ShakeSimple(amplitude);

                if (!p.starting && p.currentTime <= 0)
                {
                    shakersRunning.Remove(shakersRunning.ElementAt(i).Key);
                }
            }
            m_Transform.localRotation = Quaternion.Euler(OrigiPosition + pos);
            yield return null;
        }
    }

    public Vector3 ShakeSimple(float intensity)
    {
        tempVector.x = Random.Range(-intensity, intensity) * 0.5f;
        tempVector.y = Random.Range(-intensity, intensity) * 0.5f;
        tempVector.z = Random.Range(-intensity, intensity) * 0.5f;
        return tempVector;
    }

    public Vector3 Shake(float amplitude, float frequency, int octaves, float persistance, float lacunarity, float burstFrequency, int burstContrast, float influence)
    {
        valX = 0;
        valY = 0;
        amplitude = amplitude * 10;
        iAmplitude = 1;
        iFrequency = frequency;
        maxAmplitude = 0;
        float time = Time.time;
        //爆发频率
        float burstCoord = time / (1 - burstFrequency);

        //在对角线上采样 Perlin 噪声
        float burstMultiplier = Mathf.PerlinNoise(burstCoord, burstCoord);

        //用幂运算给爆发倍率加对比度，让数值更多贴近 0，较少接近 1
        burstMultiplier = Mathf.Pow(burstMultiplier, burstContrast);

        for (int i = 0; i < octaves; i++) // Iterate trough octaves
        {
            float noiseFrequency = time / (1 - iFrequency) / 10;

            float perlinValueX = Mathf.PerlinNoise(noiseFrequency, 0.5f);
            float perlinValueY = Mathf.PerlinNoise(0.5f, noiseFrequency);

            //加一个小值使均值为 0，再 *2 - 1 使数值落在 -1 到 1 之间。
            perlinValueX = (perlinValueX + 0.0352f) * 2 - 1;
            perlinValueY = (perlinValueY + 0.0345f) * 2 - 1;

            valX += perlinValueX * iAmplitude;
            valY += perlinValueY * iAmplitude;

            //记录最大振幅，方便之后归一化
            maxAmplitude += iAmplitude;

            iAmplitude *= persistance;
            iFrequency *= lacunarity;
        }

        valX *= burstMultiplier;
        valY *= burstMultiplier;

        //归一化
        valX /= maxAmplitude;
        valY /= maxAmplitude;

        valX *= (amplitude * influence);
        valY *= (amplitude * influence);
        tempVector.Set(valX, valX, 0);

        return tempVector;
    }
}