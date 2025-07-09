using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IEnemyData
{
    private bool exited = false;
    public bool hasCard = false;
    private GameObject theCard;
    private Thousand thousand;
    public Exchanging exchanging;
    private Table table;
    private int enemyNumber;
    public int maxBid = -1;
    public int[] marriagesSuits;
    public int[] alone10;
    int[] IEnemyData.alone10 => alone10;
    int[] IEnemyData.marriagesSuits => marriagesSuits;
    public int marriagesCount = 0;
    public GameObject[] cardsToGive;
    [SerializeField] GameObject[] BidImages;
    [SerializeField] GameObject[] NamePanel;
    private Coroutine coro1;
    private Coroutine coro2;

    private void Start()
    {
        thousand = FindObjectOfType<Thousand>();
        table = FindObjectOfType<Table>();
        if (thousand.players[1] == gameObject) enemyNumber = 1;
        else enemyNumber = 3;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        exited = false;
        Collider2D col = collision.GetComponent<Collider2D>();
        if (col.CompareTag("Card") && thousand.phase == 3 && !hasCard)
        {
            GameObject card = col.gameObject;
            card.GetComponent<Draggable>().exited = false;
            StartCoroutine(WaitForRelease(card));
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (thousand.phase == 3)
        {
            collision.gameObject.GetComponent<Draggable>().exited = true;
            exited = true;
        }
    }
    private void OnMouseDown()
    {
        if (enemyNumber == 1)
        {
            if (coro1 != null)
            {
                StopCoroutine(coro1);
                NamePanel[0].SetActive(false);
            }
            coro1 = StartCoroutine(ShowName(NamePanel[0]));
        }
        if (enemyNumber == 3)
        {
            if (coro2 != null)
            {
                StopCoroutine(coro2);
                NamePanel[1].SetActive(false);
            }
            coro2 = StartCoroutine(ShowName(NamePanel[1]));
        }
    }
    private IEnumerator ShowName(GameObject panel)
    {
        panel.SetActive(true);
        GameObject panelT = panel.transform.GetChild(1).gameObject;
        GameObject panelB = panel.transform.GetChild(0).gameObject;
        float elapsedTime = 0f;
        float durationTime = 0.65f;
        Color col1 = panelT.GetComponent<Text>().color;
        Color col2 = panelB.GetComponent<Image>().color;
        panelT.GetComponent<Text>().color = new Color(col1.r, col1.g, col1.b, 1);
        panelB.GetComponent<Image>().color = new Color(col2.r, col2.g, col2.b, 0.8f);
        yield return new WaitForSeconds(0.55f);
        while (elapsedTime < durationTime)
        {
            float t = elapsedTime / durationTime;

            panelT.GetComponent<Text>().color = new Color(col1.r, col1.g, col1.b, Mathf.Lerp(1, 0, t));
            panelB.GetComponent<Image>().color = new Color(col2.r, col2.g, col2.b, Mathf.Lerp(0.8f, 0, t));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        panelT.GetComponent<Text>().color = new Color(col1.r, col1.g, col1.b, 0);
        panelB.GetComponent<Image>().color = new Color(col2.r, col2.g, col2.b, 0);
        panel.SetActive(false);
        if (enemyNumber == 1) coro1 = null;
        else coro2 = null;
    }
    private IEnumerator WaitForRelease(GameObject card)
    {
        while (true)
        {
            if (card.GetComponent<Draggable>().released && !exited) break;
            if (exited || thousand.phase != 3) yield break;
            yield return null;
        }
        if (enemyNumber == 1) card.GetComponent<Draggable>().placed1 = true;
        else card.GetComponent<Draggable>().placed2 = true;
        hasCard = true;
        theCard = card;
    }
    public void TakeTheCard(bool fromOtherEnemy = false)
    {
        theCard.transform.SetParent(thousand.players[enemyNumber].transform);
        theCard.GetComponent<Draggable>().enableMovement = false;
        if (thousand.playerCards.Contains(theCard)) thousand.playerCards.Remove(theCard);
        if (!fromOtherEnemy)
            StartCoroutine(thousand.RotateEnemyCards(theCard, enemyNumber));
        else
            thousand.MoveEnemyCards(enemyNumber);
    }
    public IEnumerator PlayCard(GameObject cardToPlay)
    {
        yield return null;
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.25f, 0.5f));
        //flip the card 
        cardToPlay.transform.localPosition = new Vector3(cardToPlay.transform.localPosition.x, cardToPlay.transform.localPosition.y, -10);
        yield return StartCoroutine(thousand.RotateCards(new List<GameObject> { cardToPlay }, 0.5f, true, 90));
        //place the card on the table
        table.CardPlacedByEnemy(cardToPlay, enemyNumber);
        // calculate the angle to move
        Vector3 direction = Vector3.zero - cardToPlay.transform.localPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
        //print("angle before " + angle);
        if (angle > -180.0f && angle < 0) angle -= 180.0f;
        //print("angle " + angle);

        yield return StartCoroutine(thousand.MoveCards(new List<GameObject> { cardToPlay }, new List<Vector3> { new Vector3(cardToPlay.transform.localPosition.x, cardToPlay.transform.localPosition.y, cardToPlay.transform.localPosition.z) }, 0.15f, new List<float> { angle }));
        yield return StartCoroutine(thousand.MoveCards(new List<GameObject> { cardToPlay }, new List<Vector3> { new Vector3(0, 0, -1 * table.cardsOnStack.Count) }, 0.6f, new List<float> { angle }));
        
        table.CardPlacedByEnemy2(cardToPlay);
        if (!table.collectingCards && !table.gameIsEnding)
        {
            table.turn++;
            if (table.turn == 2) table.turn = 3;
            if (table.turn == 4) table.turn = 0;
            if (table.cardsOnStack.Count != 3) StartCoroutine(table.TurnPointerMove(table.turn));
            table.HandleEnemies();
        }
    }
    public IEnumerator Bid(int valueToBid)
    {
        yield return null;
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 0.75f));
        yield return new WaitForSeconds(1.0f);
        Bidding bidding = FindObjectOfType<Bidding>();
        if (valueToBid > 0)
        {
            bidding.EnemyBid(enemyNumber, valueToBid);
        }
        else
        {
            bidding.OnEnemyPass(enemyNumber);
            bidding.pass[enemyNumber] = true;
            bidding.NextTurn();
        }
    }
    public IEnumerator Exchanging()
    {
        thousand.MoveEnemyCards(enemyNumber);
        yield return new WaitForSeconds(1f);
        // give cards
        int otherEnemy;
        if (enemyNumber == 1) otherEnemy = 3;
        else otherEnemy = 1;
        Enemy enemy = thousand.players[otherEnemy].GetComponent<Enemy>();
        int newBid = table.SetNewMaxBid(enemyNumber);
        // if there is no chance of declaring two marriages then break marriage
        if (enemyNumber == 1)
        {
            enemy.theCard = cardsToGive[0];
            enemy.TakeTheCard(true);
            thousand.MoveEnemyCards(enemyNumber);
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(table.TakeTheCardForThePlayer(cardsToGive[1]));
        }
        else
        {
            StartCoroutine(table.TakeTheCardForThePlayer(cardsToGive[0]));
            thousand.MoveEnemyCards(enemyNumber);
            yield return new WaitForSeconds(0.5f);
            enemy.theCard = cardsToGive[1];
            enemy.TakeTheCard(true);
        }
        thousand.MoveEnemyCards(enemyNumber);

        if (newBid < thousand.bid) newBid = thousand.bid;
        if (newBid != thousand.bid) StartCoroutine(BidImageFade(enemyNumber, newBid));
        yield return new WaitForSeconds(2f);
        StartActualGame(newBid);
    }
    private IEnumerator BidImageFade(int player, int bid)
    {
        if (player == 3) player = 2;
        BidImages[player].SetActive(true);
        Text textCol = BidImages[player].GetComponent<Text>();
        textCol.text = bid.ToString();

        Color startCol = new Color(0.5568628f, 0.8156863f, 0.4156863f, 1);
        Color endCol = new Color(0.5568628f, 0.8156863f, 0.4156863f, 0);
        textCol.color = startCol;
        float elapsedTime = 0f;
        float durationTime = 1.25f;
        yield return new WaitForSeconds(1f);
        while (elapsedTime < durationTime)
        {
            textCol.color = Color.Lerp(startCol, endCol, elapsedTime / durationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        textCol.color = endCol;
        textCol.color = startCol;
        BidImages[player].SetActive(false);
    }
    private void StartActualGame(int newBid)
    {
        thousand.phase = 4;
        thousand.bid = newBid;
        table.TurnPointer.SetActive(true);
        table.TurnPointer.GetComponent<SpriteRenderer>().color = new Color(0.9137255f, 0.7674364f, 0.2196078f);
        table.turn = enemyNumber;
        StartCoroutine(table.TurnPointerMove(table.turn));
        table.SetBidText();
        table.HandleEnemies();
    }
}
