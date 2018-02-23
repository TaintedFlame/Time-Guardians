using UnityEngine;
using System.Collections;

[System.Serializable]
public class SoundType
{
    public string soundType;
    public int priority;

    public int currentIndex;

    public AudioSource[] audioSources;
}