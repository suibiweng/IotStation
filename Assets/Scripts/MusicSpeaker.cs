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
        ison = true;
  
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
