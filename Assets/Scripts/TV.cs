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

    [Header("TV Indicators")]
    [SerializeField] private TMP_Text channelIndicator;
    [SerializeField] private TMP_Text volumeIndicator;
    [SerializeField] private float indicatorDuration = 2f;

    private Coroutine channelIndicatorRoutine;
    private Coroutine volumeIndicatorRoutine;

    void Awake()
    {
        // Create the indicators automatically so the existing scene works without
        // requiring extra objects to be assigned in the Inspector.
        if (canvasGroup != null)
        {
            if (channelIndicator == null)
                channelIndicator = CreateIndicator("Channel Indicator", new Vector2(-30f, -30f), TextAlignmentOptions.TopRight);

            if (volumeIndicator == null)
                volumeIndicator = CreateIndicator("Volume Indicator", new Vector2(30f, -30f), TextAlignmentOptions.TopLeft);
        }

        HideIndicator(channelIndicator);
        HideIndicator(volumeIndicator);
    }



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
            ShowChannelIndicator();
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
            ShowChannelIndicator();
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
            ShowChannelIndicator();
        }
    }


    public void setVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        videoPlayer.SetDirectAudioVolume(0, volume);
        ShowVolumeIndicator(volume);
    }

    private TMP_Text CreateIndicator(string objectName, Vector2 offset, TextAlignmentOptions alignment)
    {
        GameObject indicatorObject = new GameObject(objectName, typeof(RectTransform));
        indicatorObject.transform.SetParent(canvasGroup.transform, false);

        RectTransform rectTransform = indicatorObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = offset;
        rectTransform.sizeDelta = new Vector2(-60f, 80f);

        TextMeshProUGUI indicator = indicatorObject.AddComponent<TextMeshProUGUI>();
        indicator.fontSize = 36f;
        indicator.fontStyle = FontStyles.Bold;
        indicator.color = Color.white;
        indicator.alignment = alignment;
        indicator.raycastTarget = false;
        indicator.outlineWidth = 0.2f;
        indicator.outlineColor = Color.black;
        return indicator;
    }

    private void ShowChannelIndicator()
    {
        ShowIndicator(channelIndicator, $"CH {cureentchanel + 1}", ref channelIndicatorRoutine);
    }

    private void ShowVolumeIndicator(float volume)
    {
        int percentage = Mathf.RoundToInt(volume * 100f);
        ShowIndicator(volumeIndicator, $"VOL {percentage}%", ref volumeIndicatorRoutine);
    }

    private void ShowIndicator(TMP_Text indicator, string message, ref Coroutine routine)
    {
        if (indicator == null)
            return;

        indicator.text = message;
        indicator.gameObject.SetActive(true);

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(HideIndicatorAfterDelay(indicator));
    }

    private IEnumerator HideIndicatorAfterDelay(TMP_Text indicator)
    {
        yield return new WaitForSeconds(indicatorDuration);
        HideIndicator(indicator);
    }

    private void HideIndicator(TMP_Text indicator)
    {
        if (indicator != null)
            indicator.gameObject.SetActive(false);
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
