using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicHandler : MonoBehaviour
{
    public static MusicHandler Instance { get; private set; }
    [SerializeField] private List<AudioClip> AudiosList;
    public AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps it across scenes
        }
        else
        {
            Destroy(gameObject); // Prevents duplicates
            return;
        }
    }

    private void Start()
    {
        if (musicSource == null)
        {
            Debug.LogError("AudioSource is missing on MusicHandler.");
            return;
        }

        musicSource.volume = 0.2f;
        int startSong = Random.Range(0, AudiosList.Count);
        PlayAudio(startSong);
    }

    private void PlayAudio(int songIndex)
    {
        if (AudiosList.Count == 0) return;

        musicSource.clip = AudiosList[songIndex];
        musicSource.Play();
        StartCoroutine(WaitForSongToEnd(songIndex));
    }

    private IEnumerator WaitForSongToEnd(int songIndex)
    {
        yield return new WaitForSecondsRealtime(4);
        ChangeAudio(songIndex);
    }

    private void ChangeAudio(int songIndex)
    {
        if (AudiosList.Count == 0) return;

        songIndex = (songIndex + 1) % AudiosList.Count;
        PlayAudio(songIndex);
    }
}
