using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSpeaker : MonoBehaviour
{

    public AudioClip [] audioClips;

    public AudioSource audioSource;
  

    public int currentTrack;
    public bool ison;

     public void On()
    {
                if (audioSource == null)
                    {
            Debug.LogWarning("[MusicSpeaker] Cannot turn on: AudioSource is not assigned.");
                        return;
                    }
        
                 ison = true;
        
                if (audioSource.clip == null && audioClips != null && audioClips.Length > 0)
                    {
            currentTrack = Mathf.Clamp(currentTrack, 0, audioClips.Length - 1);
            audioSource.clip = audioClips[currentTrack];
                    }
        
                if (audioSource.clip == null)
                    {
            Debug.LogWarning("[MusicSpeaker] Cannot play: no AudioClip assigned on AudioSource or audioClips list.");
                        return;
                    }
        
                 audioSource.Play();
    }
    public void Off()
    {
        ison = false;
 
        audioSource.Stop();
    }

    public void Next()
    {
        if(!ison) return;

        if (audioClips.Length > 0)
        {
            currentTrack = (currentTrack + 1) % audioClips.Length;
            audioSource.clip = audioClips[currentTrack];
            audioSource.Play();
        }
        else return;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
