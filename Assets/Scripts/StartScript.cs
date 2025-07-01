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
            yield return TipColorChange(actualCol, 0, 1);
            if (!Tip.activeSelf) yield break;

            yield return TipColorChange(actualCol, 1, 0);
            if (!Tip.activeSelf) yield break;
        } 
    }
    private IEnumerator TipColorChange(Color actualCol, int from = 0, int to = 0)
    {
        float elapsedTime = 0f;
        float durationTime = 1.5f;
        while (elapsedTime < durationTime)
        {
            float t = elapsedTime / durationTime;
            Tip.GetComponent<Text>().color = new Color(actualCol.r, actualCol.g, actualCol.b, Mathf.Lerp(from, to, t));
            elapsedTime += Time.deltaTime;
            yield return null;
            if (!Tip.activeSelf) yield break;
        }
        Tip.GetComponent<Text>().color = new Color(actualCol.r, actualCol.g, actualCol.b, to);
        yield return new WaitForSeconds(2.0f);
    }
    private IEnumerator RotateCard(GameObject card)
    {
        yield return PerformSigleRotation(card, 0, 90);

        int randInd = Random.Range(0, faces.Count());
        int rand = faces[randInd];
        card.GetComponent<Image>().sprite = CardFaces[rand];
        faces.RemoveAt(randInd);

        yield return PerformSigleRotation(card, 90, 0);
    }
    private IEnumerator PerformSigleRotation(GameObject card, int fromAngle = 0, int toAngle = 0)
    {
        float elapsedTime = 0f;
        float durationTime = 0.75f / 2;

        while (elapsedTime < durationTime)
        {
            float t = elapsedTime / durationTime;

            card.transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(fromAngle, toAngle, t), 0);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        card.transform.eulerAngles = new Vector3(0, toAngle, 0);
    }
}
