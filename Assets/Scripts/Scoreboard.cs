using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    Scoreboard instance;
    [SerializeField] private GameObject scorePrefab;
    [SerializeField] private GameObject[] Players;
    [SerializeField] private GameObject newCanvas;
    [SerializeField] private GameObject[] Content;
    [SerializeField] private GameObject[] ScrollView;
    [SerializeField] private GameObject ArrowPrefab;
    [SerializeField] private Sprite[] ArrowImages;
    public GameObject CloseButton;
    public GameObject ExitButton;
    void Awake()
    {
        if (instance == null && FindObjectsOfType<Scoreboard>().Count() == 1)
        {
            instance = this;
            FindObjectOfType<Table>().SetScoreboard(instance);
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
            FindObjectOfType<Table>().SetScoreboard(FindObjectOfType<Scoreboard>().instance);
        }
        if (instance == null) instance = FindObjectOfType<Scoreboard>();
        instance.gameObject.transform.GetChild(0).GetComponent<Canvas>().worldCamera = Camera.main;
        Players[0].GetComponent<Text>().text = FindObjectOfType<Settings>().yourName;
        instance.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        CloseButton.GetComponent<Button>().interactable = false;
        StartCoroutine(WaitAndCheck());
    }
    private IEnumerator WaitAndCheck()
    {
        yield return null;
        if (!ScrollView[0].transform.GetChild(1).gameObject.activeSelf)
        {
            ScrollView[0].GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Clamped;
        }
        else
        {
            ScrollView[0].GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Elastic;
        }
    }
    void Update()
    {
        ScrollView[1].GetComponent<ScrollRect>().verticalNormalizedPosition = ScrollView[0].GetComponent<ScrollRect>().verticalNormalizedPosition;
        ScrollView[2].GetComponent<ScrollRect>().verticalNormalizedPosition = ScrollView[0].GetComponent<ScrollRect>().verticalNormalizedPosition;
    }
    public IEnumerator SetScores(int declarer, bool gotEnoughPoints = false)
    {
        // firstly set scoreboard active
        for (int i = 0; i < 3; i++)
        {
            int n = 2;
            if (i == 0) n = 1;
            else if (i == 1) n = 0;
            int k = i;
            if (k == 2) k = 3;
            yield return new WaitForSeconds(0.5f);
            GameObject newScore = Instantiate(scorePrefab, Content[n].transform);
            newScore.transform.localScale = Vector3.one;
            newScore.GetComponent<Text>().text = FindObjectOfType<Settings>().gamePoints[k].ToString();

            if (k == declarer && gotEnoughPoints)
            {
                GameObject arrow = Instantiate(ArrowPrefab, newScore.transform);
                if (FindObjectOfType<Settings>().gamePoints[k] < 1000)
                    arrow.GetComponent<Image>().sprite = ArrowImages[0];
                else arrow.GetComponent<Image>().sprite= ArrowImages[2];
                arrow.transform.localPosition = new Vector3(100f, 0, 0);
            }
            else if (k == declarer && !gotEnoughPoints)
            {
                GameObject arrow = Instantiate(ArrowPrefab, newScore.transform);
                arrow.GetComponent<Image>().sprite = ArrowImages[1];
                arrow.transform.localPosition = new Vector3(100f, 0, 0);
            }
            Canvas.ForceUpdateCanvases();
            ScrollView[0].GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        }
        CloseButton.GetComponent<Button>().interactable = true;
    }
    public void CloseScoreboard()
    {
        FindObjectOfType<Settings>().ResetScene();
    }
    public void ExitToTheStart()
    {
        Settings settings = FindObjectOfType<Settings>();
        FindObjectOfType<Timer>().ResetClock();
        FindObjectOfType<Timer>().gameObject.SetActive(false);
        for (int i = 0; i < settings.gamePoints.Count(); i++) settings.gamePoints[i] = 0;
        ExitButton.SetActive(false);
        CloseButton.SetActive(true);
        Destroy(gameObject);
        SceneManager.LoadScene("StartScene");
    }
}
