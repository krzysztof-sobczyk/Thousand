using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Exchanging : MonoBehaviour
{
    [SerializeField] private Text currentBidText;
    [SerializeField] private GameObject InfoHUD;
    [SerializeField] private GameObject PlayerBidHUD;
    [SerializeField] private GameObject BidInput;
    [SerializeField] private GameObject AcceptButton;
    [SerializeField] private GameObject AbandonButton;
    [SerializeField] private GameObject GiveCardsButton;
    private int currentBid;
    private int startBid;
    private bool accepted = false;
    private bool cardsGiven = false;
    private Thousand thousand;
    Coroutine coro;

    private void Start()
    {
        thousand = FindObjectOfType<Thousand>();
        startBid = currentBid = thousand.bid;
    }
    private void Update()
    {
        currentBidText.text = currentBid.ToString();
    }
    private IEnumerator ColorChange(Text textCol, Color endCol)
    {
        textCol.color = Color.red;
        float elapsedTime = 0f;
        float durationTime = 1.5f;
        while (elapsedTime < durationTime)
        {
            textCol.color = Color.Lerp(Color.red, endCol, elapsedTime / durationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        textCol.color = endCol;
    }
    private void ClearBid()
    {
        var input = BidInput.GetComponent<InputField>();
        input.text = "";
    }
    private void StartActualGame()
    {
        AbandonButton.SetActive(false);
        thousand.phase = 4;
        thousand.bid = currentBid;
        Table table = FindObjectOfType<Table>();
        table.TurnPointer.SetActive(true);
        table.TurnPointer.GetComponent<SpriteRenderer>().color = new Color(0.9137255f, 0.7674364f, 0.2196078f);
        table.turn = 0;
        table.SetBidText();
        gameObject.SetActive(false);
    }

    // public functions
    public void SubmitBid()
    {
        var input = BidInput.GetComponent<InputField>();
        int bid = int.Parse(input.text);
        bool valChanged = false;
        if (bid % 10 != 0)
        {
            input.text = ((bid + 5) / 10 * 10).ToString();
            valChanged = true;
            StartCoroutine(ColorChange(BidInput.transform.GetChild(2).gameObject.GetComponent<Text>(), Color.black));
        }
        if (bid > 350)
        {
            input.text = "350";
            if (!valChanged)
                StartCoroutine(ColorChange(BidInput.transform.GetChild(2).gameObject.GetComponent<Text>(), Color.black));
        }
        else if (bid < startBid)
        {
            input.text = (startBid).ToString();
            if (!valChanged)
                StartCoroutine(ColorChange(BidInput.transform.GetChild(2).gameObject.GetComponent<Text>(), Color.black));
        }
        else if (!valChanged)
        {
            currentBid = bid;
            ClearBid();
        }
    }
    public void AcceptBid()
    {
        var input = BidInput.GetComponent<InputField>();
        if (input.text != "")
        {
            int bid = int.Parse(input.text);
            if (bid % 10 != 0)
            {
                input.text = ((bid + 5) / 10 * 10).ToString();
                bid = int.Parse(input.text);
            }
            if (bid <= 350 && bid >= startBid)
            {
                currentBid = bid;
            }
        }
        startBid = currentBid;
        PlayerBidHUD.SetActive(false);
        InfoHUD.transform.GetChild(2).gameObject.SetActive(false);
        accepted = true;
        if (cardsGiven)
        {
            StartActualGame();
        }
    }
    public void AbandonGame()
    {
        AbandonButton.GetComponent<Button>().interactable = false;
        int[] points = new int[4] { -startBid, 60, 0, 60 };
        FindObjectOfType<Settings>().SetPoints(points, 0);
        FindObjectOfType<Table>().scoreboard.gameObject.SetActive(true);
        StartCoroutine(FindObjectOfType<Table>().scoreboard.SetScores(FindObjectOfType<Table>().declarerNumber));
        AbandonButton.GetComponent<Button>().interactable = false;
        // start new game
    }
    public void GiveCards() // player gives cards to the enemies
    {
        Enemy enemy1 = thousand.players[1].GetComponent<Enemy>();
        Enemy enemy2 = thousand.players[3].GetComponent<Enemy>();
        if (enemy1.hasCard && enemy2.hasCard)
        {
            cardsGiven = true;
            enemy1.TakeTheCard();
            enemy2.TakeTheCard();
            GiveCardsButton.SetActive(false);
            InfoHUD.transform.GetChild(1).gameObject.SetActive(false);
            thousand.MoveCardsAfterDragging();
            if (accepted)
            {
                StartActualGame();
            }
        }
        else
        {
            if (coro != null)
            {
                StopCoroutine(coro);
            }
            coro = StartCoroutine(ColorChange(InfoHUD.transform.GetChild(1).gameObject.GetComponent<Text>(), Color.white));
        }
    }
}
