using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
public class TV : MonoBehaviour
{

    public VideoPlayer videoPlayer;
    public CanvasGroup canvasGroup;

    public VideoClip [] clips;
    public int cureentchanel; 

    public bool ison;



    public void On()
    {
        ison=true;

        canvasGroup.alpha=1;

        videoPlayer.Play();
        


    }


    public void Off()
    {

        ison=false;


        canvasGroup.alpha=0;

        videoPlayer.Stop();
        


    }


    public void Next()
    {
        if(!ison) return;

        if (clips.Length > 0)
        {
            cureentchanel = (cureentchanel + 1) % clips.Length;
            videoPlayer.clip = clips[cureentchanel];
            videoPlayer.Play();
        }
        else return;
    }


    public void Previous()
    {
        if(!ison) return;
        
        if (clips.Length > 0)
        {
            cureentchanel = (cureentchanel - 1 + clips.Length) % clips.Length;
            videoPlayer.clip = clips[cureentchanel];
            videoPlayer.Play();
        }
    }


    public void ChangeChanel(int chanel)
    {
        if(!ison) return;
        if (clips.Length > 0)
        {
            if (chanel < 0 || chanel >= clips.Length) return;

            cureentchanel = chanel;

            videoPlayer.clip = clips[cureentchanel];
            videoPlayer.Play();
        }
    }


    public void setVolume(float volume)
    {
        videoPlayer.SetDirectAudioVolume(0, volume);
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
