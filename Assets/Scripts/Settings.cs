using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public int bestTime;
    public int timeSum;
    public int gamesPlayed;
    public int gamesWon;
    public int[] gamePoints = new int[4] { 0, 0, 0, 0 };
    public int whoMust = 0;
    public int maxWonPoints;
    public string yourName;
    Settings instance = null;
    void Awake()
    {
        if (instance == null && FindObjectsOfType<Settings>().Count() == 1)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    void Start()
    {
        bestTime = PlayerPrefs.GetInt("bestTime", 0);
        timeSum = PlayerPrefs.GetInt("timeSum", 0);
        gamesPlayed = PlayerPrefs.GetInt("gamesPlayed", 0);
        gamesWon = PlayerPrefs.GetInt("gamesWon", 0);
        yourName = PlayerPrefs.GetString("yourName", "Player1");
        maxWonPoints = PlayerPrefs.GetInt("maxWonPoints", 0);
        whoMust = Random.Range(0, 3);
        if (whoMust == 2) whoMust = 3;
    }

    public void Save()
    {
        PlayerPrefs.SetInt("bestTime", bestTime);
        PlayerPrefs.SetInt("timeSum", timeSum);
        PlayerPrefs.SetInt("gamesPlayed", gamesPlayed);
        PlayerPrefs.SetInt("gamesWon", gamesWon);
        PlayerPrefs.SetString("yourName", yourName);
        PlayerPrefs.SetInt("maxWonPoints", maxWonPoints);
        PlayerPrefs.Save();
        //print("best time: " + bestTime);
        //print("gamesPlayed: " + gamesPlayed + " games won: " + gamesWon);
        //print("winratio: " + Mathf.RoundToInt((gamesWon * 100f / gamesPlayed) * 100f) / 100f + "%");
    }
    public void SetPoints(int[] points, int declarer)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i == 2) continue;
            if (points[i] == 0) continue;
            else if (points[i] > 0)
            {
                if (gamePoints[i] < 800)
                    gamePoints[i] += ((points[i] + 5) / 10 * 10);
                else
                {
                    if (declarer == i) gamePoints[i] += ((points[i] + 5) / 10 * 10);
                }
            }  
            else
                gamePoints[i] += ((points[i] - 5) / 10 * 10);
        }
        CheckGameEnd();
    }
    private void CheckGameEnd()
    {
        for(int i = 0; i < 4; i++)
        {
            if (gamePoints[i] >= 1000)
            {
                gamePoints[i] = 1000;
                FindObjectOfType<Table>().scoreboard.CloseButton.SetActive(false);
                // set active another button that takes you to the start scene
                FindObjectOfType<Table>().scoreboard.ExitButton.SetActive(true);
                if (i == 0) gamesWon++;
                gamesPlayed++;
                FindObjectOfType<Timer>().stop_clock = true;
                timeSum += FindObjectOfType<Timer>().current_time;
                if (FindObjectOfType<Timer>().current_time < bestTime || bestTime == 0) 
                    bestTime = FindObjectOfType<Timer>().current_time;
                Save();
                // show scoreboard
            }
        }
    }
    public void ResetScene()
    {
        Scoreboard scoreboard = FindObjectOfType<Scoreboard>();
        SceneManager.LoadScene("GameScene");
    }
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
