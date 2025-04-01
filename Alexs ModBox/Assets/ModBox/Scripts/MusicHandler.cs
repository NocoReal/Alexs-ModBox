using System.Collections.Generic;
using UnityEngine;

public class MusicHandler : MonoBehaviour
{
    [SerializeField] private List<AudioClip> AudiosList;
    public AudioSource musicSource;
    int song;
    private void OnEnable()
    {
        musicSource.volume = 0.2f;
        DontDestroyOnLoad(gameObject);
        int startSong = Random.Range(0, AudiosList.Count);
        song = startSong;
        musicSource.clip = AudiosList[startSong];
        musicSource.Play();
        Invoke(nameof(ChangeAudio), musicSource.clip.length);
    }
    void ChangeAudio()
    {
        musicSource.Stop();
        song++;
        if (song > AudiosList.Count) song = 0;
        musicSource.clip = AudiosList[song];
        musicSource.Play();
        Invoke(nameof(ChangeAudio), musicSource.clip.length);
    }
}
