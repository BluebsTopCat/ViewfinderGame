using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class Enviornment : MonoBehaviour
{
    [FormerlySerializedAs("MinutesPerDay")] [Header("Testing Settings")]
    public float minutesPerDay = 10;
    [FormerlySerializedAs("UpdatesPerMinute")] public int updatesPerDay = 10;
    [FormerlySerializedAs("GlobalTime")] public float globalTime;
    [FormerlySerializedAs("CloudColorStr")] [Range(0,1)]
    public float cloudColorStr = 0.446f;

    [FormerlySerializedAs("CloudSpeedMult")] public float cloudSpeedMult = .05f;
    public Texture2D grungeglobal;
    public Vector3 windSpeed;
    Vector2 _cloudAcc;
    Vector2 _cloudPos;
    Vector2 _cloudVel;

    [FormerlySerializedAs("Sun")] [Header("Properties")]
    public Light sun;
    [FormerlySerializedAs("Cloud_Color")] public Color cloudColor;
    [FormerlySerializedAs("LowGrad")] public Gradient lowGrad;
    [FormerlySerializedAs("HighGrad")] public Gradient highGrad;
    public AnimationCurve sunbrightness = new AnimationCurve();
    
    // Start is called before the first frame update

    private void OnValidate()
    {
    
        Shader.SetGlobalTexture("GrungeGlobal", grungeglobal);
        UpdateShaders();
    }

    private void Update()
    {
        UpdateSkyBox();
    }

    void UpdateShaders()
    {
        float standardTime = (globalTime % 24);
        
        Color skyTop = highGrad.Evaluate(standardTime/ 24);
        
        Color skyBot = lowGrad.Evaluate(standardTime/ 24);
        
        Shader.SetGlobalColor("Sky_Top", skyTop);
        Shader.SetGlobalColor("Sky_Bot", lowGrad.Evaluate(standardTime/24));
        Shader.SetGlobalFloat("Time", standardTime);
        Shader.SetGlobalFloat("CloudCover", RandBetween(0, 1, globalTime, .1f));
        Shader.SetGlobalFloat("CloudColorStr", cloudColorStr);
        Shader.SetGlobalColor("CloudColor", cloudColor);
        Shader.SetGlobalVector("CloudPos", _cloudPos);
        Shader.SetGlobalVector("WindDir", windSpeed);
        RenderSettings.fogColor = Color.Lerp(skyTop, skyBot, .5f);
        sun.color = skyTop;
        float sunRot = -90 + 360 * (standardTime / 24);
        sun.transform.eulerAngles = new Vector3(sunRot, 0f, 0f);
        sun.intensity = sunbrightness.Evaluate(standardTime / 24);
    }

    void UpdateSkyBox()
    {
        globalTime +=  (Time.deltaTime * 24) / (60 * minutesPerDay);
        _cloudAcc.x = RandBetween(-1, 1, globalTime, .1f);
        _cloudAcc.y = RandBetween(-1, 1, globalTime + 50, .1f);
        _cloudVel += new Vector2(_cloudAcc.x * Time.deltaTime, _cloudAcc.y * Time.deltaTime);
        _cloudPos += _cloudVel * (Time.deltaTime * cloudSpeedMult);
        UpdateShaders();
    }

    private float RandBetween(float min, float max, float time, float speed)
    {
        float half = (max - min) / 2;
        float perlintime = (Mathf.Sin(2 * (time * speed)) + Mathf.Sin(Mathf.PI * (time * speed)))/2;
        return min + half + perlintime * half;
    }

    private float IntegralCalc(float min, float max, float time, float speed)
    {
        float half = (max - min) / 2;
        float pi = MathF.PI;
        float perlinintegral = (-Mathf.Cos(pi * speed * time)/pi - Mathf.Cos(2 * time * speed)/2) /2;
        return min + half + perlinintegral * half;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(Vector3.zero,  windSpeed);
    }
}
