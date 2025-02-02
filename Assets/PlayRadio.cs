﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class PlayRadio : MonoBehaviour
{
    public VRTK_InteractableObject Radio;
    public VRTK_InteractableObject Sensor;
    public VRTK_InteractableObject HeadSet;
    public VoiceOverFolder voiceOverFolder;
    bool radiospielt = false;
    AudioSource tina;
    public GameObject HeadsetIcon;
    public GameObject SensorIcon;
    float Audiolength;
    public FuseboxDeckel fuseboxdeckel;
    bool ansagegespielt = false;

    
    

    // Start is called before the first frame update
    void Start()
    {
        tina = GetComponent<AudioSource>();
    }

    //-----------------RadioOn-------------------
    private void OnEnable()
    {
        Radio = (Radio == null ? GetComponent<VRTK_InteractableObject>() : Radio);

        if (Radio != null)
        {
            Radio.InteractableObjectUsed += Radio_InteractableObjectUsed; ;
            Radio.InteractableObjectUnused += Radio_InteractableObjectUnused; ;
        }

        Sensor = (Sensor == null ? GetComponent<VRTK_InteractableObject>() : Sensor);

        if (Sensor != null)
        {
            Sensor.InteractableObjectGrabbed += Sensor_InteractableObjectGrabbed; ;
            Sensor.InteractableObjectUngrabbed += Sensor_InteractableObjectUngrabbed; ;
        }

        HeadSet = (HeadSet == null ? GetComponent<VRTK_InteractableObject>() : HeadSet);

        if (HeadSet != null)
        {
            HeadSet.InteractableObjectGrabbed += HeadSet_InteractableObjectGrabbed; ;
            HeadSet.InteractableObjectUngrabbed += HeadSet_InteractableObjectUngrabbed; ;
        }
    }

    private void OnDisable()
    {
        if (Radio != null)
        {
            Radio.InteractableObjectUsed -= Radio_InteractableObjectUsed; ;
            Radio.InteractableObjectUnused -= Radio_InteractableObjectUnused; ;
        }

        if (Sensor != null)
        {
            Sensor.InteractableObjectGrabbed -= Sensor_InteractableObjectGrabbed; ;
            Sensor.InteractableObjectUngrabbed -= Sensor_InteractableObjectUngrabbed; ;
        }

        if (HeadSet != null)
        {
            HeadSet.InteractableObjectGrabbed -= HeadSet_InteractableObjectGrabbed; ;
            HeadSet.InteractableObjectUngrabbed -= HeadSet_InteractableObjectUngrabbed; ;
        }
    }

    private void Radio_InteractableObjectUnused(object sender, InteractableObjectEventArgs e)
    {


    }

    private void Radio_InteractableObjectUsed(object sender, InteractableObjectEventArgs e)
    {
        if (!ansagegespielt)
        {
            Debug.Log("Playing Tina");

            tina.Play();
            AudioWinner();
            fuseboxdeckel.IconRadio.SetActive(false);
            ansagegespielt = true;
        }
    }

    //--------------------SensorGrabbed---------

    private void Sensor_InteractableObjectUngrabbed(object sender, InteractableObjectEventArgs e)
    {
    }

    private void Sensor_InteractableObjectGrabbed(object sender, InteractableObjectEventArgs e)
    {
        Debug.Log("SensorGrabbed");
        SensorIcon.SetActive(false);
        IconHeadset();
    }

    private void HeadSet_InteractableObjectUngrabbed(object sender, InteractableObjectEventArgs e)
    {
    }

    private void HeadSet_InteractableObjectGrabbed(object sender, InteractableObjectEventArgs e)
    {
        HeadsetIcon.SetActive(false);
    }



    private void AudioWinner()
    {
        voiceOverFolder.PlayAudioClipDelayed("holetitel_trainieren_lidar", 21f);
        Audiolength = voiceOverFolder.currentClip.length;
        Invoke("IconSensor", 30f);
    }

    private void IconHeadset()
    {
        HeadsetIcon.SetActive(true);
    }
    private void IconSensor()
    {
        //voiceOverFolder.PlayAudioClipDelayed("dann_teste_ich_meinen_lidar_sensor", 6f);
        SensorIcon.SetActive(true);
    }
}
