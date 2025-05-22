using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] Sprite[] pauseImages;
    [SerializeField] Sprite[] CardFaces;
    [SerializeField] Sprite CardBack;
    [SerializeField] GameObject PauseButton;
    [SerializeField] GameObject Card;
    private bool isPaused = false;
    private List<int> faces = new() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };
    private bool oneTime = true;
    public void Pause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        PauseButton.GetComponent<Image>().sprite = pauseImages[isPaused ? 1 : 0];
        gameObject.SetActive(!gameObject.activeSelf);
        if (isPaused)
        {
            faces = new() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };
            oneTime = true;
            Card.GetComponent<Image>().sprite = CardBack;
            Card.transform.eulerAngles = Vector3.zero;

            StartCoroutine(RotateCard(Card));
        }
    }

    private IEnumerator RotateCard(GameObject card)
    {
        while (isPaused)
        {
            yield return new WaitForSecondsRealtime(0.15f);
            float elapsedTime = 0f;
            float durationTime = 1.25f / 2;
            while (elapsedTime < durationTime)
            {
                float t = elapsedTime / durationTime;

                card.transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(0, 90, t), 0);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            card.transform.eulerAngles = new Vector3(0, 90, 0);
            if (!isPaused) yield break;
            elapsedTime = 0f;
            int randInd = Random.Range(0, faces.Count);
            int rand = faces[randInd];
            card.GetComponent<Image>().sprite = CardFaces[rand];
            if (oneTime) faces.RemoveAt(randInd);
            else faces = new() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };
            oneTime = !oneTime;
            while (elapsedTime < durationTime)
            {
                float t = elapsedTime / durationTime;

                card.transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(90, 0, t), 0);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            card.transform.eulerAngles = Vector3.zero;
            yield return new WaitForSecondsRealtime(0.15f);
            if (!isPaused) yield break;
            elapsedTime = 0f;
            while (elapsedTime < durationTime)
            {
                float t = elapsedTime / durationTime;

                card.transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(0, 90, t), 0);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            card.transform.eulerAngles = new Vector3(0, 90, 0);
            if (!isPaused) yield break;
            elapsedTime = 0f;
            card.GetComponent<Image>().sprite = CardBack;
            while (elapsedTime < durationTime)
            {
                float t = elapsedTime / durationTime;

                card.transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(90, 0, t), 0);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            card.transform.eulerAngles = Vector3.zero;
        }
    }
}
