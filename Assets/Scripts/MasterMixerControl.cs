using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MasterMixerControl : MonoBehaviour
{
    public AudioMixer MasterMixer;

    public void SetMasterVolume(float volume)
    {
        MasterMixer.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        MasterMixer.SetFloat("MusicVolume", volume);
    }

    public void SetEffectsVolume(float volume)
    {

        MasterMixer.SetFloat("EffectsVolume", volume);
    }

    public void SetPhasedMode()
    {
        MasterMixer.SetFloat("ChorusDryMix", 0);
    }

    public void SetNormalMode()
    {
        MasterMixer.SetFloat("ChorusDryMix", 1);
    }
}
