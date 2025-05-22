using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsPanel : MonoBehaviour
{
    [SerializeField] GameObject[] Stats;
    private Settings settings;
    private void OnEnable()
    {
        settings = FindObjectOfType<Settings>();
        //print("best time: " + bestTime);
        //print("gamesPlayed: " + gamesPlayed + " games won: " + gamesWon);
        //print("winratio: " + Mathf.RoundToInt((gamesWon * 100f / gamesPlayed) * 100f) / 100f + "%");
        int totalTime = settings.bestTime;
        int hours = totalTime / 3600;
        int minutes = (totalTime % 3600) / 60;
        int seconds = totalTime % 60;
        Stats[0].GetComponent<Text>().text = "Best time:\n" + LeadingZero(hours) + ":" + LeadingZero(minutes) + ":" + LeadingZero(seconds);
        // average game time
        if (settings.gamesPlayed != 0)
            totalTime = Mathf.RoundToInt(settings.timeSum / settings.gamesPlayed);
        else totalTime = 0;
        hours = totalTime / 3600;
        minutes = (totalTime % 3600) / 60;
        seconds = totalTime % 60;
        Stats[1].GetComponent<Text>().text = "Average game time:\n" + LeadingZero(hours) + ":" + LeadingZero(minutes) + ":" + LeadingZero(seconds);
        Stats[2].GetComponent<Text>().text = "Max won points: " + settings.maxWonPoints;
        Stats[3].GetComponent<Text>().text = "Games played: " + settings.gamesPlayed;
        Stats[4].GetComponent<Text>().text = "Games won: " + settings.gamesWon;
        if (settings.gamesPlayed != 0)
            Stats[5].GetComponent<Text>().text = "Win rate: " + (Mathf.RoundToInt((settings.gamesWon * 100f / settings.gamesPlayed) * 100f) / 100f).ToString() + "%";
        else Stats[5].GetComponent<Text>().text = "Win rate: 0%";
    }
    private string LeadingZero(int n)
    {
        return n.ToString().PadLeft(2, '0');
    }
}
