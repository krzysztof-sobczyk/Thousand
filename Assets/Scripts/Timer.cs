using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Text timerText;
    [SerializeField] private GameObject Background;
    public float delta_time;
    public int current_time;
    public bool stop_clock = false;
    Timer instance = null;
    void Awake()
    {
        if (instance == null && FindObjectsOfType<Timer>().Count() == 1)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
        }
        if (instance == null) instance = FindObjectOfType<Timer>();
        instance.gameObject.GetComponent<Canvas>().worldCamera = Camera.main;
    }
    void Start()
    {
        delta_time = 0;
        stop_clock = false;
    }
    void Update()
    {
        if (!stop_clock)
        {
            delta_time += Time.deltaTime;
            TimeSpan span = TimeSpan.FromSeconds(delta_time);

            string hour = LeadingZero(span.Hours);
            string minute = LeadingZero(span.Minutes);
            string seconds = LeadingZero(span.Seconds);
            current_time = span.Seconds + span.Minutes * 60 + span.Hours * 3600;

            timerText.text = hour + ":" + minute + ":" + seconds;

            float textWidth = timerText.preferredWidth * 1.3f;
            Background.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
        }
    }

    private string LeadingZero(int n)
    {
        return n.ToString().PadLeft(2, '0');
    }
    public void ResetClock()
    {
        current_time = 0;
        delta_time = 0;
    }
}