using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScript : MonoBehaviour
{
    [SerializeField] GameObject InputText;
    [SerializeField] GameObject NamingPanel;
    [SerializeField] GameObject StatsPanel;
    [SerializeField] GameObject InfoPanel;
    [SerializeField] GameObject ActualName;
    [SerializeField] Sprite[] CardFaces;
    [SerializeField] GameObject Tip;
    private int steps = 0;
    private Coroutine timer;
    private List<int> faces = new() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };
    private void Start()
    {
        timer = StartCoroutine(ShowTipTimer());
        faces = new() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };
    }
    public void SubmitNick()
    {
        FindObjectOfType<Settings>().yourName = InputText.GetComponent<Text>().text;
        FindObjectOfType<Settings>().Save();
        ActualName.GetComponent<Text>().text = "Actual name: \n" + FindObjectOfType<Settings>().yourName;
        InputText.transform.parent.GetComponent<InputField>().text = "";
    }
    public void ClosePanel(GameObject obj)
    {
        obj.SetActive(false);
        timer = StartCoroutine(ShowTipTimer());
    }
    public void OpenStatsPanel()
    {
        Tip.SetActive(false);
        if (timer != null) StopCoroutine(timer);
        StatsPanel.SetActive(true);
    }
    public void OpenNamingPanel()
    {
        Tip.SetActive(false);
        if (timer != null) StopCoroutine(timer);
        NamingPanel.SetActive(true);
        ActualName.GetComponent<Text>().text = "Actual name: \n" + FindObjectOfType<Settings>().yourName;
    }
    public void OpenInfoPanel()
    {
        Tip.SetActive(false);
        if (timer != null) StopCoroutine(timer);
        InfoPanel.SetActive(true);
    }
    public void StartTheGameStep(Button button)
    {
        steps++;
        Tip.SetActive(false);
        if (timer != null) StopCoroutine(timer);
        timer = StartCoroutine(ShowTipTimer());
        button.interactable = false;
        StartCoroutine(RotateCard(button.gameObject));
        CheckGameStart();
    }
    private void CheckGameStart()
    {
        if (steps == 3) StartCoroutine(WaitForStart());
    }
    private IEnumerator WaitForStart()
    {
        yield return new WaitForSeconds(1f);
        steps = 0;
        SceneManager.LoadScene("GameScene");
    }
    private IEnumerator ShowTipTimer()
    {
        yield return new WaitForSeconds(5.0f);
        Tip.SetActive(true);
        StartCoroutine(TipBehavior());
    }
    private IEnumerator TipBehavior()
    {
        while (Tip.activeSelf)
        {
            Color actualCol = Tip.GetComponent<Text>().color;
            Tip.GetComponent<Text>().color = new Color(actualCol.r, actualCol.g, actualCol.b, 0);
            float elapsedTime = 0f;
            float durationTime = 1.5f;
            while (elapsedTime < durationTime)
            {
                float t = elapsedTime / durationTime;
                Tip.GetComponent<Text>().color = new Color(actualCol.r, actualCol.g, actualCol.b, Mathf.Lerp(0, 1, t));
                elapsedTime += Time.deltaTime;
                yield return null;
                if (!Tip.activeSelf) yield break;
            }
            elapsedTime = 0f;
            durationTime = 2.0f;
            Tip.GetComponent<Text>().color = new Color(actualCol.r, actualCol.g, actualCol.b, 1);
            while (elapsedTime < durationTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
                if (!Tip.activeSelf) yield break;
            }
            elapsedTime = 0f;
            durationTime = 1.5f;
            while (elapsedTime < durationTime)
            {
                float t = elapsedTime / durationTime;
                Tip.GetComponent<Text>().color = new Color(actualCol.r, actualCol.g, actualCol.b, Mathf.Lerp(1, 0, t));
                elapsedTime += Time.deltaTime;
                yield return null;
                if (!Tip.activeSelf) yield break;
            }
            elapsedTime = 0f;
            durationTime = 2.0f;
            Tip.GetComponent<Text>().color = new Color(actualCol.r, actualCol.g, actualCol.b, 0);
            while (elapsedTime < durationTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
                if (!Tip.activeSelf) yield break;
            }
        } 
    }
    private IEnumerator RotateCard(GameObject card)
    {
        float elapsedTime = 0f;
        float durationTime = 0.75f / 2;
        while (elapsedTime < durationTime)
        {
            float t = elapsedTime / durationTime;

            card.transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(0, 90, t), 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0f;
        int randInd = Random.Range(0, faces.Count());
        int rand = faces[randInd];
        card.GetComponent<Image>().sprite = CardFaces[rand];
        faces.RemoveAt(randInd);
        while (elapsedTime < durationTime)
        {
            float t = elapsedTime / durationTime;

            card.transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(90, 0, t), 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

}
