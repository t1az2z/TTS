using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;
    [HideInInspector]
    public AudioSource source;
    [HideInInspector]
    public bool isPlaying;
    [Range(-1f, 1f)]
    public float stereoPan;
    public bool panable;
    public bool isMusic;
}