using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Table : MonoBehaviour
{
    private Thousand thousand;
    public Scoreboard scoreboard;
    public int turn;
    public List<GameObject> cardsOnStack = new();
    private List<GameObject> removedCards = new();
    private float zOffset = -1;
    private bool exited = false;
    private int marriageNumber = -1;
    private List<int> playerNumbers = new();
    public GameObject TurnPointer;
    private Vector3[] turnPointerPositions = {
        new Vector3(0,-157.5f,0),
        new Vector3(-421.25f, 0, 0),
        Vector3.zero,
        new Vector3(421.25f, 0, 0)
    };
    public bool collectingCards = false;
    public bool gameIsEnding = false;
    public bool everyonePassed = false;
    [SerializeField] private GameObject DeclareMarriageText;
    [SerializeField] private GameObject MarriageConfirmationPanel;
    [SerializeField] private GameObject MarriageImage;
    [SerializeField] private List<Sprite> marriageImages;
    private int response = -1;
    public int declarerNumber = -1;
    int[] marriagesDeclared = new int[4] { -1, -1, -1, -1 };
    [SerializeField] private GameObject PointsPanel;
    [SerializeField] private GameObject[] BidTexts;
    [SerializeField] private GameObject SimulateRemovedCards;
    private Coroutine coro;
    private bool wasUp = false;
    private Vector3 startPos;
    readonly Dictionary<int, int> cardPoints = new()
    {
        { 0, 0 },
        { 1, 2 },
        { 2, 3 },
        { 3, 4 },
        { 4, 10 },
        { 5, 11 }
    };
    Dictionary<int, string> suitsNames = new Dictionary<int, string>
    {
        { 0, "H"},
        { 1, "C" },
        { 2, "D" },
        { 3, "S" }
    };
    Dictionary<int, string> valuesNames = new Dictionary<int, string>
    {
        { 0, "9" },
        { 1, "J" },
        { 2, "Q" },
        { 3, "K" },
        { 4, "10" },
        { 5, "A" }
    };
    public void SetScoreboard(Scoreboard newScoreboard)
    {
        scoreboard = newScoreboard;
    }
    private void Start()
    {
        thousand = FindObjectOfType<Thousand>();
        SetPointerPositions();
    }
    private void SetPointerPositions()
    {
        Vector3 player0Position = thousand.players[0].transform.position;
        Vector3 player1Position = thousand.players[1].transform.position;
        Vector3 player3Position = thousand.players[3].transform.position;
        turnPointerPositions[0] = new Vector3(player0Position.x, player0Position.y - 150, player0Position.z);
        turnPointerPositions[1] = new Vector3(player1Position.x - 200, player1Position.y, player1Position.z);
        turnPointerPositions[3] = new Vector3(player3Position.x + 200, player3Position.y, player3Position.z);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        exited = false;
        Collider2D col = collision.GetComponent<Collider2D>();
        GameObject card = collision.gameObject;
        if (col.CompareTag("Card") && thousand.phase == 4 && card.transform.parent.gameObject == thousand.players[turn])
        {
            if (CheckIfCanBePlaced(card))
            {
                card.GetComponent<Draggable>().exited = false;
                StartCoroutine(WaitForRelease(card));
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (thousand.phase == 4 && collision.gameObject.transform.parent.gameObject == thousand.players[0])
        {
            collision.gameObject.GetComponent<Draggable>().exited = true;
            exited = true;
        }
    }
    private void OnMouseDown()
    {
        if (coro == null && cardsOnStack.Count == 2 && !wasUp)
        {
            coro = StartCoroutine(UpCard());
            wasUp = true;
            startPos = cardsOnStack[0].transform.localPosition;
        }
    }
    private void OnMouseUp()
    {
        if (coro != null) StopCoroutine(coro);
        if (cardsOnStack.Count == 2 && wasUp) coro = StartCoroutine(DownCard());
    }
    private IEnumerator UpCard()
    {
        float elapsedTime = 0f;
        float durationTime = 0.15f;
        Vector3 itsPos = cardsOnStack[0].transform.localPosition;
        while (elapsedTime < durationTime)
        {
            float t = elapsedTime / durationTime;

            cardsOnStack[0].transform.localPosition = new Vector3(itsPos.x, Mathf.Lerp(itsPos.y, itsPos.y + 75, t), itsPos.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cardsOnStack[0].transform.localPosition = new Vector3(itsPos.x, itsPos.y + 75, itsPos.z);
    }
    private IEnumerator DownCard()
    {
        float elapsedTime = 0f;
        float durationTime = 0.15f;
        Vector3 itsPos = cardsOnStack[0].transform.localPosition;
        while (elapsedTime < durationTime)
        {
            float t = elapsedTime / durationTime;

            cardsOnStack[0].transform.localPosition = new Vector3(itsPos.x, Mathf.Lerp(itsPos.y, startPos.y, t), itsPos.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cardsOnStack[0].transform.localPosition = new Vector3(itsPos.x, startPos.y, itsPos.z);
        wasUp = false;
        coro = null;
    }
    private bool CheckIfCanBePlaced(GameObject card)
    {
        if (cardsOnStack.Count == 0) return true;
        else
        {
            int suit = card.GetComponent<Selectable2>().suit;
            int prevSuit = cardsOnStack[0].GetComponent<Selectable2>().suit;
            if (suit == prevSuit) return true;

            bool noPrevSuitCards = HasNoCardsOfSuit(prevSuit);
            bool noMarriageCards = HasNoCardsOfSuit(marriageNumber);
            if ((suit == marriageNumber && noPrevSuitCards) || (suit != marriageNumber && noPrevSuitCards && noMarriageCards)) return true;
        }
        return false;
    }
    private bool HasNoCardsOfSuit(int suitToCheck)
    {
        for (int i = 0; i < thousand.players[turn].transform.childCount; i++)
        {
            Selectable2 card = thousand.players[turn].transform.GetChild(i).GetComponent<Selectable2>();
            if (card.suit ==  suitToCheck) return false;
        }
        return true;
    }
    private IEnumerator MoveCardToTheCentre(GameObject card)
    {
        zOffset = -1 * cardsOnStack.Count;
        float elapsedTime = 0f;
        float durationTime = 0.6f;
        Vector3 spos = card.transform.localPosition;
        Vector3 dest = new Vector3(0, 0, 0 + zOffset);
        while (elapsedTime < durationTime)
        {
            float t = elapsedTime / durationTime;
            card.transform.localPosition = Vector3.Lerp(spos, dest, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        card.transform.localPosition = dest;
    }

    private IEnumerator WaitForRelease(GameObject card)
    {
        while (true)
        {
            if (card.GetComponent<Draggable>().released && !exited) break;
            if (exited) yield break;
            yield return null;
        }
        card.GetComponent<Draggable>().onTable = true;
        thousand.canDragCards = false;
        // placed card was from marriage
        if (card.GetComponent<Selectable2>().value is 2 or 3 && CanDeclareMarriage(card, 0))
        {
            DeclareMarriage(card, 0, true);
            while (response == -1) yield return null;
            if (response == -2) response = -1;
        }
        thousand.playerCards.Remove(card);
        card.transform.SetParent(gameObject.transform);
        card.GetComponent<Draggable>().enableMovement = false;
        cardsOnStack.Add(card);
        playerNumbers.Add(turn);
        StartCoroutine(MoveCardToTheCentre(card));
        thousand.MoveCardsAfterDragging();
        //cards collecting
        if (cardsOnStack.Count == 3) StartCoroutine(CollectCards());
        if (!collectingCards)
        {
            //set new turn
            do
            {
                turn = (turn + 1) % 4;
            }
            while (turn == 2);
            StartCoroutine(TurnPointerMove(turn));
            HandleEnemies();
        }
    }
    public IEnumerator TurnPointerMove(int turn)
    {
        float elapsedTime = 0.0f;
        float durationTime = 0.2f;
        Vector3 spos = TurnPointer.transform.localPosition;
        Vector3 dest = turnPointerPositions[turn];
        while (elapsedTime < durationTime)
        {
            float t = elapsedTime / durationTime;
            
            TurnPointer.transform.localPosition = Vector3.Lerp(spos, dest, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        TurnPointer.transform.localPosition = dest;
    }
    private IEnumerator CollectCards()
    {
        collectingCards = true;
        // 1) check who won
        int wonNumber = WhoWon();
        turn = wonNumber;
        if (thousand.playerCards.Count != 0) StartCoroutine(TurnPointerMove(turn));
        yield return new WaitForSeconds(0.15f);
        // 2) turn card's z axis
        yield return StartCoroutine(thousand.MoveCards(cardsOnStack, new List<Vector3> { new(0, 0, -1), new(0, 0, -2), new(0, 0, -3) }, 0.5f));
        // 3) flip cards
        yield return StartCoroutine(thousand.RotateCards(cardsOnStack));
        // 4) move cards to the player who won
        foreach (GameObject card in cardsOnStack)
        {
            removedCards.Add(card);
            card.transform.SetParent(thousand.piles[wonNumber].transform);
        }
        if(wonNumber  == 0)
            yield return StartCoroutine(thousand.MoveCards(cardsOnStack, new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero }));
        else
            yield return StartCoroutine(thousand.MoveCards(cardsOnStack, new List<Vector3> { Vector3.zero, Vector3.zero, Vector3.zero }, 0.6f, new List<float> { 90, 90, 90}));
        cardsOnStack.Clear();
        playerNumbers.Clear();
        if (thousand.playerCards.Count == 0) EndGame();
        thousand.canDragCards = true;
        collectingCards = false;
        HandleEnemies();
    }
    public void CardPlacedByEnemy(GameObject card, int enemy)
    {
        card.transform.SetParent(gameObject.transform);
        thousand.MoveEnemyCards(enemy);
        cardsOnStack.Add(card);
        playerNumbers.Add(turn);
    }
    public void CardPlacedByEnemy2(GameObject card)
    {
        if (cardsOnStack.Count == 3) StartCoroutine(CollectCards());
        StartCoroutine(MoveCardToTheCentre(card));
    }
    private void EndGame()
    {
        gameIsEnding = true;
        TurnPointer.SetActive(false);
        MarriageImage.SetActive(false);
        CalculatePoints(out int[] points, out int pointsGotByDeclarer);
        //show points
        PointsPanel.SetActive(true);
        int n;
        for (int i = 0; i < 4; i++)
        {
            n = i;
            if (i == 2) continue;
            if (i == 3) n = 2;
            Text txt = PointsPanel.transform.GetChild(n).gameObject.GetComponent<Text>();
            if (declarerNumber == i) txt.text = (pointsGotByDeclarer + " / " + thousand.bid).ToString();
            else txt.text = points[i].ToString();
            if (points[i] >= 0) txt.color = new Color(0.5587758f, 0.8167115f, 0.4171613f);
            else txt.color = new Color(0.8274932f, 0.3450525f, 0.3379116f);
            StartCoroutine(FadeOut(PointsPanel.transform.GetChild(n).gameObject, 2f));
        }
        // 2) add points to the score
        FindObjectOfType<Settings>().SetPoints(points, declarerNumber);
        if (points[0] > FindObjectOfType<Settings>().maxWonPoints) FindObjectOfType<Settings>().maxWonPoints = points[0];
        FindObjectOfType<Settings>().Save();
        // 3) start a new game
        StartCoroutine(WaitForScoreboard(points[declarerNumber] > 0));
        marriagesDeclared = new int[4] { -1, -1, -1, -1 };

        void CalculatePoints(out int[] points, out int pointsGotByDeclarer)
        {
            Dictionary<int, int> pointsForMarriages = new()
            {
                { 0, 100 },
                { 1, 60 },
                { 2, 80 },
                { 3, 40 }
            };
            // 1) count points of every player's pile
            points = new int[4] { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                if (i == 2) continue;
                points[i] = CountPointsFromAPile(thousand.piles[i]);
            }
            // add points from marriages
            for (int i = 0; i < 4; i++)
            {
                if (marriagesDeclared[i] == -1) continue;
                points[marriagesDeclared[i]] += pointsForMarriages[i];
            }
            // check if declarer got enough points
            pointsGotByDeclarer = points[declarerNumber];
            if (points[declarerNumber] >= thousand.bid)
            {
                points[declarerNumber] = thousand.bid;
            }
            else
            {
                points[declarerNumber] = -1 * thousand.bid;
            }
        }
    }
    private IEnumerator WaitForScoreboard(bool gotEnoughPoints)
    {
        UpdateSprite[] cards = FindObjectsOfType<UpdateSprite>();
        foreach (UpdateSprite card in cards)
        {
            Destroy(card.gameObject);
        }
        yield return new WaitForSeconds(3.5f);
        scoreboard.gameObject.SetActive(true);
        StartCoroutine(scoreboard.SetScores(declarerNumber, gotEnoughPoints));
    }
    private int CountPointsFromAPile(GameObject pile)
    {
        int points = 0;
        foreach (Transform tran in pile.transform)
        {
            GameObject card = tran.gameObject;
            points += cardPoints[card.GetComponent<Selectable2>().value];
        }
        return points;
    }
    public void SetBidText()
    {
        BidTexts[0].SetActive(true);
        BidTexts[1].SetActive(true);
        BidTexts[0].GetComponent<Text>().text = "Declarer: " + thousand.players[declarerNumber].name;
        BidTexts[1].GetComponent<Text>().text = "Bid: " + thousand.bid.ToString();
    }
    private int WhoWon()
    {
        GameObject winningCard = null;
        foreach (GameObject card in cardsOnStack)
        {
            int suit = card.GetComponent<Selectable2>().suit;
            int value = card.GetComponent<Selectable2>().value;
            if (winningCard == null) winningCard = card;
            else
            {
                int winSuit = winningCard.GetComponent<Selectable2>().suit;
                int winValue = winningCard.GetComponent<Selectable2>().value;
                if ((winSuit == suit && value > winValue) ||
                    (winSuit != suit && winSuit != marriageNumber && suit == marriageNumber)) winningCard = card;
            }
        }
        for (int i = 0; i < 3; i++)
        {
            if (winningCard == cardsOnStack[i]) return playerNumbers[i];
        }
        return 0;
    }
    public void HandleEnemies()
    {
        if (collectingCards || gameIsEnding) return;
        Enemy enemy1 = thousand.players[1].GetComponent<Enemy>();
        Enemy enemy2 = thousand.players[3].GetComponent<Enemy>();
        if (turn == 1 || turn == 3) SetMarriagesSuits(turn);
        if (turn == 1) StartCoroutine(enemy1.PlayCard(ChooseACard(1)));
        if (turn == 3) StartCoroutine(enemy2.PlayCard(ChooseACard(3)));
    }
    public void HandleBids(int biddingTurn)
    {
        if (everyonePassed) return;        
        HandleEnemy(1, biddingTurn);
        HandleEnemy(3, biddingTurn);

        void HandleEnemy(int turn, int biddingTurn)
        {
            Enemy enemy = thousand.players[turn].GetComponent<Enemy>();
            Bidding bidding = FindObjectOfType<Bidding>();

            if (biddingTurn != turn) return;
            if (!bidding.pass[turn])
            {
                if (enemy.maxBid == -1) enemy.maxBid = SetUltimateBid(turn);
                StartCoroutine(enemy.Bid(ValueToBid(turn)));
            }
            else bidding.NextTurn();
        }
    }
    private bool CanDeclareMarriage(GameObject declaringCard, int player)
    {
        if (player == 0 && cardsOnStack.Count != 0) return false;
        int dSuit = declaringCard.GetComponent<Selectable2>().suit;
        int dValue = declaringCard.GetComponent<Selectable2>().value;
        if (dValue != 2 && dValue != 3) return false;
        for (int i = 0; i < thousand.players[player].transform.childCount; i++)
        {
            GameObject card = thousand.players[player].transform.GetChild(i).gameObject;
            int cardValue = card.GetComponent<Selectable2>().value;
            int cardSuit = card.GetComponent<Selectable2>().suit;
            if (cardSuit != dSuit) continue;
            if ((dValue == 2 && cardValue == 3) || (dValue == 3 && cardValue == 2)) return true;
        }
        return false;
    }
    private void DeclareMarriage(GameObject declaringCard, int player, bool canDeclare = true)
    {
        int dSuit = declaringCard.GetComponent<Selectable2>().suit;
        if (canDeclare)
        {
            marriagesDeclared[dSuit] = player;
            if (player == 0)
            {
                MarriageConfirmationPanel.SetActive(true);
                StartCoroutine(WaitForResponse(dSuit));
            }
            else
            {
                if (!MarriageImage.activeSelf) MarriageImage.SetActive(true);
                MarriageImage.GetComponent<Image>().sprite = marriageImages[dSuit];
                DeclareMarriageText.SetActive(true);
                StartCoroutine(FadeOut(DeclareMarriageText));
                marriageNumber = dSuit;
            }
        }
    }
    private IEnumerator FadeOut(GameObject obj, float waitTime = 0.75f)
    {
        Text textCol = obj.GetComponent<Text>();
        Color startCol = new Color(textCol.color.r, textCol.color.g, textCol.color.b, 1);
        Color endCol = new Color(textCol.color.r, textCol.color.g, textCol.color.b, 0);
        textCol.color = startCol;
        float elapsedTime = 0f;
        float durationTime = 1.5f;
        yield return new WaitForSeconds(waitTime);
        while (elapsedTime < durationTime)
        {
            textCol.color = Color.Lerp(startCol, endCol, elapsedTime / durationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        textCol.color = endCol;
        textCol.color = startCol;
        obj.SetActive(false);
    }
    private IEnumerator WaitForResponse(int dSuit)
    {
        while (true)
        {
            if (response == 1) { response = -2; break; }
            if (response == 0)
            {
                response = -2;
                MarriageConfirmationPanel.SetActive(false);
                thousand.canDragCards = true;
                yield break;
            }
            yield return null;
        }
        if (!MarriageImage.activeSelf) MarriageImage.SetActive(true);
        MarriageImage.GetComponent<Image>().sprite = marriageImages[dSuit];
        thousand.canDragCards = true;
        MarriageConfirmationPanel.SetActive(false);
        DeclareMarriageText.SetActive(true);
        StartCoroutine(FadeOut(DeclareMarriageText));
        marriageNumber = dSuit;
    }
    public void SetResponse(int resp)
    {
        response = resp;
    }
    private int SetUltimateBid(int enemy) // the real way the bid is set before getting the musk
    {
        int val = ((((SetMaxBid(enemy) + CheckMaxBid(enemy)) / 2 + 8) / 10) * 10);
        int rand = Random.Range(0, 2);
        if (val < 110 && val > 80 && rand == 1) val = 110;
        if (val > 120 && thousand.players[enemy].GetComponent<Enemy>().marriagesCount == 0) val = 120;
        return val;
    }
    public int SetMaxBid(int enemy) // before getting the musk
    {
        int maxBid;
        List<GameObject> enemyCards = SetEnemyCards(enemy);
        GameObject[,] sortedCards = SetSortedCardsHorizontal(enemyCards);
        List<int> cardValues = new List<int>
        { 0, 0, 0, 0, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 10, 10, 10, 10, 11, 11, 11, 11};
        // count how many cards you can take
        int yourPoints = 0;
        int cardsYouCanTake = 0;
        int pointsFromMarriages = 0;
        int pointsFromHalves = 0;
        int[] howManyCards = new int[4];
        List<GameObject> halves = new();
        int halfPoints = 0;
        int marriages = 0;
        for (int i = 0; i < 4; i++)
        {
            int cardsFromI = SetYourPoits(sortedCards, cardValues, ref yourPoints, howManyCards, ref halfPoints, i);

            cardsYouCanTake += cardsFromI;

            AddPointsFromMarriages(sortedCards, ref pointsFromMarriages, howManyCards, ref marriages, i);
            // halves
            if (sortedCards[i, 2] != null && sortedCards[i, 3] == null) halves.Add(sortedCards[i, 2]);
            else if (sortedCards[i, 2] == null && sortedCards[i, 3] != null) halves.Add(sortedCards[i, 3]);
        }
        thousand.players[enemy].GetComponent<Enemy>().marriagesCount = marriages;
        //print("halves count: " + halves.Count);
        if (halves.Count >= 1) // it might be worth risking
        {
            // add minimal marriage points
            int[] suitPoints = { 100, 60, 80, 40 };
            int minPoints = 120;
            int minSuit = -1;
            foreach (GameObject h in halves)
            {
                int suit = h.GetComponent<Selectable2>().suit;
                int points = suitPoints[suit];

                if (points < minPoints)
                {
                    minPoints = points;
                    minSuit = suit;
                }
            }
            if (halves.Count >= 3 || 
                (halves.Count == 2 && (minPoints >= 60 || howManyCards[minSuit] >= 4)) ||
                (halves.Count == 1 && minPoints >= 80 && howManyCards[minSuit] >= 5))
                pointsFromHalves = minPoints;
        }
        //print("values counted");
        if (halfPoints == 2)
        {
            halfPoints = 0;
            cardsYouCanTake--;
        }
        else if (halfPoints == 3)
        {
            halfPoints = 1;
            cardsYouCanTake--;
        }
        for (int i = 0; i < cardsYouCanTake; i++)
        {
            // one value
            int val = cardValues.Min();
            yourPoints += val;
            cardValues.Remove(val);
            if (halfPoints == 1)
            {
                halfPoints = 0;
                continue;
            }
            // second value
            val = cardValues.Min();
            yourPoints += val;
            cardValues.Remove(val);
        }
        if (yourPoints > 120) yourPoints = 120;
        maxBid = yourPoints + pointsFromMarriages + pointsFromHalves;
        //print(cardsYouCanTake + " speculatet result = " + 15*cardsYouCanTake + " your points: " + yourPoints);
        //print("maxbid: " + maxBid);
        //print("old maxbid: " + (15 * cardsYouCanTake + pointsFromMarriages + pointsFromHalves));

        int gameMode = Random.Range(0, 2); // from 0 to 1
        if (gameMode == 0)
        {
            //print("I'll try my luck");
            return (((maxBid + 5) / 10) * 10);
        }
        else
        {
            return ((maxBid / 10) * 10);
        }
    }
    private List<GameObject> SetEnemyCards(int enemy)
    {
        List<GameObject> enemyCards = new();
        foreach (Transform tran in thousand.players[enemy].transform)
        {
            GameObject card = tran.gameObject;
            enemyCards.Add(card);
        }

        return enemyCards;
    }
    private static GameObject[,] SetSortedCardsHorizontal(List<GameObject> enemyCards)
    {
        GameObject[,] sortedCards = new GameObject[4, 6]
        {
            { null, null, null, null, null, null },
            { null, null, null, null, null, null },
            { null, null, null, null, null, null },
            { null, null, null, null, null, null }
        };
        for (int i = 0; i < 4; i++)
        {
            foreach (GameObject card in enemyCards)
            {
                if (card.GetComponent<Selectable2>().suit == i)
                {
                    sortedCards[i, card.GetComponent<Selectable2>().value] = card;
                }
            }
        }

        return sortedCards;
    }
    private int SetYourPoits(GameObject[,] sortedCards, List<int> cardValues, ref int yourPoints, int[] howManyCards, ref int halfPoints, int i)
    {
        int cardsFromI = 0;
        int[] fixedPoints = { 0, 0, 0, 4, 10, 11 };

        for (int j = 5; j >= 0; j--)
        {
            if (sortedCards[i, j] == null) continue;
            int cardValue = cardPoints[sortedCards[i, j].GetComponent<Selectable2>().value];
            cardValues.Remove(cardValue);
            howManyCards[i]++;
            if ((j == 5) || (j == 4 && cardsFromI == 1) || (j == 3 && cardsFromI == 2)) yourPoints += fixedPoints[j];
            else if (cardsFromI > 2) yourPoints += cardValue;
            else continue;

            cardsFromI++;
        }

        if (cardsFromI == 1 && howManyCards[i] == 5) // unprotected 10
        {
            cardsFromI += 4; // 4 - 0.5
            halfPoints++;
            yourPoints += 19;
            cardValues.Remove(10);
        }
        else if (cardsFromI == 1 && howManyCards[i] == 4) // there is high? chance you will get this card
        {
            cardsFromI += 2; // 3 - 1
            yourPoints += 19;
            cardValues.Remove(10);
            if (sortedCards[i, 0] == null) cardValues.Remove(0);
            else if (sortedCards[i, 1] == null) cardValues.Remove(2);
            else if (sortedCards[i, 2] == null) cardValues.Remove(3);
            else if (sortedCards[i, 3] == null) cardValues.Remove(4);
        }

        return cardsFromI;
    }
    private static void AddPointsFromMarriages(GameObject[,] sortedCards, ref int pointsFromMarriages, int[] howManyCards, ref int marriages, int i)
    {
        Dictionary<int, int> marriagesPoints = new()
        {
            { 0, 140 }, // 100 + 40
            { 1, 80 },  // 60 + 20
            { 2, 110 }, // 80 + 30
            { 3, 50 }   // 40 + 10
        };

        if (sortedCards[i, 2] == null || sortedCards[i, 3] == null) return;

        marriages++;
        //  one marriage                or more but with at least 3 cards from that suit
        if (pointsFromMarriages == 0 || howManyCards[i] >= 3)
        {
            pointsFromMarriages += marriagesPoints[i];
        }
        else if (pointsFromMarriages == 80 && i == 2) // when taken 80 instead of 110
            pointsFromMarriages = 110;
    }
    public int CheckMaxBid(int enemy) //second algorithm
    {
        int maxBid;
        List<GameObject> enemyCards = SetEnemyCards(enemy);
        GameObject[,] sortedCards = SetSortedCardsHorizontal(enemyCards);
        List<int> cardValues = new List<int>
        { 0, 0, 0, 0, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 10, 10, 10, 10, 11, 11, 11, 11};
        int sumOfBids = 0;
        int howMany = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (sortedCards[i, j] == null)
                {
                    List<GameObject> newList = new(enemyCards);
                    string objName = suitsNames[i] + valuesNames[j];
                    GameObject obj = GameObject.Find(objName);
                    newList.Add(obj);
                    int newBid = FindMaxBid2(newList);
                    //print(newBid);
                    sumOfBids += newBid;
                    howMany++;
                }
            }
        }
        maxBid = Mathf.RoundToInt(sumOfBids / howMany);
        return maxBid;
    }
    private int FindMaxBid2(List<GameObject> enemyCards)
    {
        int maxBid;
        GameObject[,] sortedCards = SetSortedCardsHorizontal(enemyCards);
        List<int> cardValues = new List<int>
        { 0, 0, 0, 0, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 10, 10, 10, 10, 11, 11, 11, 11};
        // count how many cards you can take
        int yourPoints = 0;
        int cardsYouCanTake = 0;
        int pointsFromMarriages = 0;
        int pointsFromHalves = 0;
        int[] howManyCards = new int[4];
        int halfPoints = 0;
        int marriages = 0;
        List<GameObject> halves = new();
        for (int i = 0; i < 4; i++)
        {
            int cardsFromI = SetYourPoits(sortedCards, cardValues, ref yourPoints, howManyCards, ref halfPoints, i);

            cardsYouCanTake += cardsFromI;

            AddPointsFromMarriages(sortedCards, ref pointsFromMarriages, howManyCards, ref marriages, i);
            // halves
            if (sortedCards[i, 2] != null && sortedCards[i, 3] == null) halves.Add(sortedCards[i, 2]);
            else if (sortedCards[i, 2] == null && sortedCards[i, 3] != null) halves.Add(sortedCards[i, 3]);
        }
        if (halves.Count >= 1) // it might be worth risking
        {
            // add minimal marriage points
            int minPoints = 120;
            int minSuit = -1;
            foreach (GameObject h in halves)
            {
                int suit = h.GetComponent<Selectable2>().suit;
                if (suit == 3 && minPoints > 40) { minPoints = 40; minSuit = 3; }
                else if (suit == 1 && minPoints > 60) { minPoints = 60; minSuit = 1; }
                else if (suit == 2 && minPoints > 80) { minPoints = 80; minSuit = 2; }
                else if (suit == 0 && minPoints > 100) { minPoints = 100; minSuit = 0; }
            }
            if (halves.Count >= 3)
                pointsFromHalves = minPoints;
            else if (halves.Count == 2 && (minPoints >= 60 || howManyCards[minSuit] >= 4))
                pointsFromHalves = minPoints;
            else if (halves.Count == 1 && minPoints >= 80 && howManyCards[minSuit] >= 5)
                pointsFromHalves = minPoints;
        }
        if (halfPoints == 2)
        {
            halfPoints = 0;
            cardsYouCanTake--;
        }
        else if (halfPoints == 3)
        {
            halfPoints = 1;
            cardsYouCanTake--;
        }
        for (int i = 0; i < cardsYouCanTake; i++)
        {
            // one value
            int val = cardValues.Min();
            yourPoints += val;
            cardValues.Remove(val);
            if (halfPoints == 1)
            {
                halfPoints = 0;
                continue;
            }
            // second value
            val = cardValues.Min();
            yourPoints += val;
            cardValues.Remove(val);
        }
        if (yourPoints > 120) yourPoints = 120;
        maxBid = yourPoints + pointsFromMarriages + pointsFromHalves;
        return ((maxBid / 10) * 10);
    }
    public int SetNewMaxBid(int enemy) // after getting a musk
    {
        List<GameObject> enemyCards = new();
        List<GameObject[]> cardsToGive = new();
        List<GameObject[]> safeCardsToGive = new();
        int maxBid = 0;
        int maxSafeBid = 0;
        foreach (Transform tran in thousand.players[enemy].transform)
        {
            GameObject card = tran.gameObject;
            enemyCards.Add(card);
        }
        for (int i = 0; i < enemyCards.Count; i++)
        {
            if (enemyCards[i].GetComponent<Selectable2>().value == 5) continue;
            for (int j = 0;  j < enemyCards.Count; j++)
            {
                if (i == j) continue;
                if (enemyCards[j].GetComponent<Selectable2>().value == 5) continue;
                // create the list without two cards
                List<GameObject> newList = new(enemyCards);
                newList.Remove(enemyCards[i]); newList.Remove(enemyCards[j]);

                int newMaxBid = FindMaxBid(newList);
                //print(newMaxBid);
                if (newMaxBid > maxBid)
                {
                    maxBid = newMaxBid;
                    cardsToGive.Clear();
                    cardsToGive.Add(new GameObject[2] { enemyCards[i], enemyCards[j] });
                }
                else if (newMaxBid == maxBid) 
                { 
                    cardsToGive.Add(new GameObject[2] { enemyCards[i], enemyCards[j] });
                }
                if (newMaxBid > maxSafeBid)
                {
                    if (enemyCards[i].GetComponent<Selectable2>().value != 2 && enemyCards[i].GetComponent<Selectable2>().value != 3 &&
                        enemyCards[j].GetComponent<Selectable2>().value != 2 && enemyCards[j].GetComponent<Selectable2>().value != 3)
                    {
                        maxSafeBid = newMaxBid;
                        safeCardsToGive.Clear();
                        safeCardsToGive.Add(new GameObject[2] { enemyCards[i], enemyCards[j] });
                    }
                }
                else if (newMaxBid == maxSafeBid)
                {
                    safeCardsToGive.Add(new GameObject[2] { enemyCards[i], enemyCards[j] });
                }
            }
        }

        // set marriagesSuits
        SetMarriagesSuits(enemy);

        // add additional points to safetogive if card is from your marriage
        foreach (GameObject card in enemyCards)
        {
            int cardSuit = card.GetComponent<Selectable2>().suit;
            Enemy currentEnemy = thousand.players[enemy].GetComponent<Enemy>();
            if (currentEnemy.marriagesSuits[cardSuit] == 1) card.GetComponent<Selectable2>().safeToGive++;
        }
        int maxScore = 0;
        for (int i = 0; i < 4; i++)
        {
            if (thousand.players[enemy].GetComponent<Enemy>().marriagesSuits[i] == 1 && i == 0) maxScore += 100;
        }

        // from list with maxBid choose the weakest cards to give
        //print("maxBid: " + maxBid + " maxSafeBid: " + maxSafeBid);
        GameObject[] choosenCards = new GameObject[2];
        if (maxSafeBid != 0 && maxSafeBid + 10 >= maxBid)
        {
            choosenCards = ChooseCardsToGive(safeCardsToGive);
            print("safe cards given");
        }
        else
        {
            choosenCards = ChooseCardsToGive(cardsToGive);
        }
        print(choosenCards[0].name + " " + choosenCards[1].name);
        thousand.players[enemy].GetComponent<Enemy>().cardsToGive[0] = choosenCards[0];
        thousand.players[enemy].GetComponent<Enemy>().cardsToGive[1] = choosenCards[1];

        // simulate a game
        if (enemy == 1) maxBid = SimulateAGame(choosenCards[0], choosenCards[1], enemy, enemyCards);
        else maxBid = SimulateAGame(choosenCards[1], choosenCards[0], enemy, enemyCards);

        // if maxSafeBid != null && maxSafeBid + 10 >= maxBid
        if (FindObjectOfType<Settings>().gamePoints[enemy] + thousand.bid >= 1000) maxBid = thousand.bid;
        else if (FindObjectOfType<Settings>().gamePoints[enemy] + maxBid >= 1000) maxBid = 1000 - FindObjectOfType<Settings>().gamePoints[enemy];
        print("is going to get: " + ((maxBid / 10) * 10));
        return ((maxBid / 10) * 10);
    }
    public void SetMarriagesSuits(int enemy)
    {
        //also set alone 10-s
        List<GameObject> enemyCards = new();
        foreach (Transform tran in thousand.players[enemy].transform)
        {
            GameObject card = tran.gameObject;
            enemyCards.Add(card);
        }
        GameObject[,] sortedCards = new GameObject[4, 6]
        {
            { null, null, null, null, null, null },
            { null, null, null, null, null, null },
            { null, null, null, null, null, null },
            { null, null, null, null, null, null }
        };
        int[] howManyCards = new int[4] { 0, 0, 0, 0 };
        for (int i = 0; i < 4; i++)
        {
            foreach (GameObject card in enemyCards)
            {
                if (card.GetComponent<Selectable2>().suit == i)
                {
                    sortedCards[i, card.GetComponent<Selectable2>().value] = card;
                    howManyCards[i]++;
                }
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (enemy <= 3)
            {
                if (sortedCards[i, 2] != null && sortedCards[i, 3] != null) thousand.players[enemy].GetComponent<Enemy>().marriagesSuits[i] = 1;
                else thousand.players[enemy].GetComponent<Enemy>().marriagesSuits[i] = 0;
                if (sortedCards[i, 4] != null && howManyCards[i] == 2 && sortedCards[i, 5] == null) thousand.players[enemy].GetComponent<Enemy>().alone10[i] = 1;
                else thousand.players[enemy].GetComponent<Enemy>().alone10[i] = 0;
            }
            else //simulation
            {
                if (sortedCards[i, 2] != null && sortedCards[i, 3] != null) thousand.players[enemy].GetComponent<Simulated>().marriagesSuits[i] = 1;
                else thousand.players[enemy].GetComponent<Simulated>().marriagesSuits[i] = 0;
                if (sortedCards[i, 4] != null && howManyCards[i] == 2 && sortedCards[i, 5] == null) thousand.players[enemy].GetComponent<Simulated>().alone10[i] = 1;
                else thousand.players[enemy].GetComponent<Simulated>().alone10[i] = 0;
            }
            
        }   
    }
    public bool PlayerHasMarriage()
    {
        GameObject[,] sortedCards = new GameObject[4, 6]
        {
            { null, null, null, null, null, null },
            { null, null, null, null, null, null },
            { null, null, null, null, null, null },
            { null, null, null, null, null, null }
        };
        for (int i = 0; i < 4; i++)
        {
            foreach (GameObject card in thousand.playerCards)
            {
                if (card.GetComponent<Selectable2>().suit == i)
                {
                    sortedCards[i, card.GetComponent<Selectable2>().value] = card;
                }
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (sortedCards[i, 2] != null && sortedCards[i, 3] != null) return true;
        }
        return false;
    }
    private GameObject[] ChooseCardsToGive(List<GameObject[]> cardsToGive)
    {
        GameObject[] choosenCards = new GameObject[2];
        int safeToGivePoints;
        int minPoints = 100;
        for (int i = 0; i < cardsToGive.Count; i++)
        {
            //if possible avoid giving kings and queens
            safeToGivePoints = cardsToGive[i][0].GetComponent<Selectable2>().safeToGive + cardsToGive[i][1].GetComponent<Selectable2>().safeToGive;
            if (safeToGivePoints < minPoints)
            {
                minPoints = safeToGivePoints;
                choosenCards[0] = cardsToGive[i][0];
                choosenCards[1] = cardsToGive[i][1];
            }
        }
        return choosenCards;
    }
    private int SimulateAGame(GameObject card1, GameObject card2, int declarer, List<GameObject> enemyCards)
    {
        // set cards for other enemy (bot) [4]
        if(declarer == 1)
        {
            foreach (Transform tran in thousand.players[3].transform)
            {
                GameObject card = tran.gameObject;
                Instantiate(card, thousand.players[4].transform);
            }
            Instantiate(card1, thousand.players[4].transform);
        }
        else
        {
            foreach (Transform tran in thousand.players[1].transform)
            {
                GameObject card = tran.gameObject;
                Instantiate(card, thousand.players[4].transform);
            }
            Instantiate(card1, thousand.players[4].transform);
        }
        // set cards for a player [5]
        foreach (Transform tran in thousand.players[0].transform)
        {
            GameObject card = tran.gameObject;
            Instantiate(card, thousand.players[5].transform);
        }
        Instantiate(card2, thousand.players[5].transform);
        // set cards for playing enemy = declarer
        List<GameObject> newEnemyCards = new(enemyCards); newEnemyCards.Remove(card1); newEnemyCards.Remove(card2);
        foreach (GameObject card in newEnemyCards)
        {
            Instantiate(card, thousand.players[6].transform);
        }
        SetMarriagesSuits(4); SetMarriagesSuits(5); SetMarriagesSuits(6);
        // perform a simulation
        Dictionary<int, int> turns;
        if (declarer == 1)
        {
            turns = new()
            {
                { 0, 6 }, // declarer
                { 1, 4 }, // bot
                { 2, 5 } // player
            };
        }
        else
        {
            turns = new()
            {
                { 0, 6 }, // declarer
                { 1, 5 }, // player
                { 2, 4 } // bot
            };
        }
        int turn = 0;
        int points = 0;
        GameObject cardToRemove = null;
        //print("simulation loop begin");
        while (thousand.players[6].transform.childCount > 0)
        {
            for (int i = 0; i < 3; i++)
            {
                //print("turn " + turn);
                GameObject choosenCard = ChooseACard(turns[turn]);
                //print("choosen card " + choosenCard.name);
                cardsOnStack.Add(choosenCard);
                if (turn == 0) cardToRemove = choosenCard;
                playerNumbers.Add(turn);
                turn++;
                if (turn == 3) turn = 0;
            }
            // check who won
            turn = WhoWon();
            //print("who won " + turn);
            if (turn == 0)
            {
                foreach (GameObject card in cardsOnStack)
                {
                    points += cardPoints[card.GetComponent<Selectable2>().value];
                } 
            }
            if (CanDeclareMarriage(cardToRemove, 6) && playerNumbers[0] == 0)
            {
                //print("marriage declared");
                if (cardToRemove.name is "HQ(Clone)" or "HK(Clone)") points += 100;
                else if (cardToRemove.name is "DQ(Clone)" or "DK(Clone)") points += 80;
                else if (cardToRemove.name is "CQ(Clone)" or "CK(Clone)") points += 60;
                else if (cardToRemove.name is "SQ(Clone)" or "SK(Clone)") points += 40;
            }
            foreach (GameObject card in cardsOnStack)
            {
                removedCards.Add(card);
                card.transform.SetParent(SimulateRemovedCards.transform);
            }
            cardsOnStack.Clear();
            playerNumbers.Clear();
            SetMarriagesSuits(4); SetMarriagesSuits(5); SetMarriagesSuits(6);
        }
        foreach(GameObject card in removedCards) Destroy(card);
        removedCards.Clear();
        marriageNumber = -1;
        print("simulated points " + points);
        return points;
    }
    private int FindMaxBid(List<GameObject> enemyCards)
    {
        int maxBid;
        GameObject[,] sortedCards = new GameObject[4, 6]
        {
            { null, null, null, null, null, null },
            { null, null, null, null, null, null },
            { null, null, null, null, null, null },
            { null, null, null, null, null, null }
        };
        List<int> cardValues = new List<int>
        { 0, 0, 0, 0, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 10, 10, 10, 10, 11, 11, 11, 11};
        int aceCount = 0;
        for (int i = 0; i < 4; i++)
        {
            foreach (GameObject card in enemyCards)
            {
                if (card.GetComponent<Selectable2>().suit == i)
                {
                    sortedCards[i, card.GetComponent<Selectable2>().value] = card;
                    if (card.GetComponent<Selectable2>().value == 5) aceCount++;
                }
            }
        }
        // count how many cards you can take
        int yourPoints = 0;
        int cardsYouCanTake = 0;
        int pointsFromMarriages = 0;
        int[] howManyCards = new int[4];
        int halfPoints = 0;
        for (int i = 0; i < 4; i++)
        {
            int cardsFromI = 0;

            for (int j = 5; j >= 0; j--)
            {
                if (sortedCards[i, j] != null)
                {
                    cardValues.Remove(cardPoints[sortedCards[i, j].GetComponent<Selectable2>().value]);
                    howManyCards[i]++;
                    if (j == 5) { cardsFromI++; yourPoints += 11; }
                    else if (j == 4 && cardsFromI == 1) { cardsFromI++; yourPoints += 10; }
                    else if (j == 3 && cardsFromI == 2) { cardsFromI++; yourPoints += 4; }
                    else if (cardsFromI > 2) { cardsFromI++; yourPoints += cardPoints[sortedCards[i, j].GetComponent<Selectable2>().value]; }
                }
            }
            if (cardsFromI == 1 && howManyCards[i] == 5) // unprotected 10
            {
                cardsFromI += 4; // 4 - 0.5
                halfPoints++;
                yourPoints += 19;
                cardValues.Remove(10);
            }
            else if (cardsFromI == 1 && howManyCards[i] == 4) // there is high? chance you will get this card
            {
                cardsFromI += 2; // 3 - 1
                yourPoints += 19;
                cardValues.Remove(10);
                if (sortedCards[i, 0] == null) cardValues.Remove(0);
                else if (sortedCards[i, 1] == null) cardValues.Remove(2);
                else if (sortedCards[i, 2] == null) cardValues.Remove(3);
                else if (sortedCards[i, 3] == null) cardValues.Remove(4);
            }
            cardsYouCanTake += cardsFromI;
            // add points from marriages, but can they declare more than one marriage? they might have chance if they have some cards from those marriages colors
            if (pointsFromMarriages == 0) // one marriage
            {
                if (i == 0 && sortedCards[i, 2] != null && sortedCards[i, 3] != null) pointsFromMarriages += 100;
                else if (i == 1 && sortedCards[i, 2] != null && sortedCards[i, 3] != null) pointsFromMarriages += 60;
                else if (i == 2 && sortedCards[i, 2] != null && sortedCards[i, 3] != null) pointsFromMarriages += 80;
                else if (i == 3 && sortedCards[i, 2] != null && sortedCards[i, 3] != null) pointsFromMarriages += 40;
            }
            else // more than one
            {
                if (howManyCards[i] >= 3 && aceCount != 0) // must have at least 3 cards from that color and at least one Ace
                {
                    if (i == 1 && sortedCards[i, 2] != null && sortedCards[i, 3] != null) pointsFromMarriages += 60;
                    else if (i == 2 && sortedCards[i, 2] != null && sortedCards[i, 3] != null) pointsFromMarriages += 80;
                    else if (i == 3 && sortedCards[i, 2] != null && sortedCards[i, 3] != null) pointsFromMarriages += 40;
                }
                else if (pointsFromMarriages == 60 && i == 2 && sortedCards[i, 2] != null && sortedCards[i, 3] != null) pointsFromMarriages = 80;
            }
        }
        if (halfPoints == 2)
        {
            halfPoints = 0;
            cardsYouCanTake--;
        }
        else if (halfPoints == 3)
        {
            halfPoints = 1;
            cardsYouCanTake--;
        }
        for (int i = 0; i < cardsYouCanTake; i++)
        {
            // one value
            int val = cardValues.Min();
            yourPoints += val;
            cardValues.Remove(val);
            if (halfPoints == 1)
            {
                halfPoints = 0;
                continue;
            }
            // second value
            val = cardValues.Min();
            yourPoints += val;
            cardValues.Remove(val);
        }
        if (yourPoints > 120) yourPoints = 120;
        maxBid = yourPoints + pointsFromMarriages;
        return maxBid;
    }
    public IEnumerator TakeTheCardForThePlayer(GameObject cardToTake)
    {
        cardToTake.transform.SetParent(thousand.players[0].transform);
        thousand.playerCards.Add(cardToTake);
        thousand.SortPlayerCards();
        yield return StartCoroutine(thousand.MoveCardsAfterDragging2());
        yield return StartCoroutine(thousand.RotateCards(new List<GameObject> { cardToTake }));
        thousand.MoveCardsAfterDragging();
    }
    private int ValueToBid(int enemy)
    {
        int actualBid = FindObjectOfType<Bidding>().currentBid;
        if (actualBid + 10 <= thousand.players[enemy].GetComponent<Enemy>().maxBid) return actualBid + 10;
        else return 0;
    }
    static GameObject[,] SetSortedCardsVertical(List<GameObject> enemyCards)
    {
        GameObject[,] sortedCards = new GameObject[6, 4]
        {
                { null, null, null, null },
                { null, null, null, null },
                { null, null, null, null },
                { null, null, null, null },
                { null, null, null, null },
                { null, null, null, null }
        };
        foreach (GameObject card in enemyCards)
        {
            sortedCards[card.GetComponent<Selectable2>().value, card.GetComponent<Selectable2>().suit] = card;
        }

        return sortedCards;
    }
    private GameObject PlayLowestCardFromRandomSuit(List<GameObject> enemyCards, int enemy)
    {
        int minNumber = 6;
        List<GameObject> minCards = new();

        foreach (GameObject card in enemyCards)
        {
            int cardVal = card.GetComponent<Selectable2>().value;
            if (cardVal < minNumber)
            {
                minNumber = cardVal;
                minCards.Clear();
                minCards.Add(card);
            }
            else if (cardVal == minNumber) minCards.Add(card);
        }
        GameObject choosenCard = minCards[Random.Range(0, minCards.Count)];
        GameObject[,] sortedCards = SetSortedCardsVertical(enemyCards);

        GameObject enemyPlayer = thousand.players[enemy];
        IEnemyData chosenComponent;
        if (enemy <= 3)
        {
            chosenComponent = enemyPlayer.GetComponent<Enemy>();
        }
        else // simulation
        {
            chosenComponent = enemyPlayer.GetComponent<Simulated>();
        }
        return TryToAvoidGiving10orMarriage(chosenComponent, choosenCard, sortedCards);
    }
    private GameObject TryToAvoidGiving10orMarriage(IEnemyData enemyData, GameObject chosenCard, GameObject[,] sortedCards, int tryCount = 0)
    {
        if (tryCount >= 2) return chosenCard;
        int currentSuit = chosenCard.GetComponent<Selectable2>().suit;
        if (enemyData.alone10[currentSuit] == 1)
        {
            //print("trying not to give alone 10");
            for (int i = 0; i < 5; i++) // not 6, because it is not worth giving ace
            {
                for (int j = 3; j >= 0; j--)
                {
                    if (j == currentSuit) continue;
                    if (sortedCards[i, j] != null)
                    {
                        chosenCard = sortedCards[i, j];
                        // if it is another alone 10 try again
                        TryToAvoidGiving10orMarriage(enemyData, chosenCard, sortedCards, ++tryCount);
                        i = 5;
                        break;
                    }
                }
            }
        }

        currentSuit = chosenCard.GetComponent<Selectable2>().suit;
        int currentValue = chosenCard.GetComponent<Selectable2>().value;
        if ((currentValue == 2 || currentValue == 3) && enemyData.marriagesSuits[currentSuit] > 0)
        {
            //print("trying not to give marriage1");
            for (int i = 0; i < 5; i++) // not 6, because it is not worth giving ace
            {
                for (int j = 3; j >= 0; j--)
                {
                    if (i == 2 && j == currentSuit) continue;
                    if (i == 3 && j == currentSuit) continue;
                    if (sortedCards[i, j] != null)
                    {
                        chosenCard = sortedCards[i, j];
                        return chosenCard;
                    }
                }
            }
        }

        return chosenCard;
    }
    private GameObject PlayMinimalGreater(List<GameObject> enemyCards, int suit, int value, int enemy)
    {
        GameObject chosenCard = null;
        int minNumber = 6;
        foreach (GameObject card in enemyCards)
        {
            int cardValue = card.GetComponent<Selectable2>().value;
            if (cardValue > value && card.GetComponent<Selectable2>().suit == suit)
            {
                if (cardValue < minNumber)
                {
                    minNumber = cardValue;
                    chosenCard = card;

                    IEnemyData enemyData;
                    if (enemy <= 3) enemyData = thousand.players[enemy].GetComponent<Enemy>();
                    else enemyData = thousand.players[enemy].GetComponent<Simulated>();

                    chosenCard = TryToAvoidGivingMarriage(enemyData, enemyCards, value, enemy, chosenCard, minNumber);
                }
            }
        }
        return chosenCard;

        GameObject TryToAvoidGivingMarriage(IEnemyData enemyData, List<GameObject> enemyCards, int value, int enemy, GameObject choosenCard, int minNumber)
        {
            // if selected card is from your marriage
            int newSuit = choosenCard.GetComponent<Selectable2>().suit;
            if ((minNumber == 2 || minNumber == 3) && enemyData.marriagesSuits[newSuit] > 0)
            {
                //try to give something else
                List<GameObject> newCards = new(enemyCards);
                GameObject[,] sortedCards = SetSortedCardsVertical(enemyCards);
                newCards.Remove(sortedCards[2, newSuit]);
                newCards.Remove(sortedCards[3, newSuit]);
                GameObject temp = PlayMinimalGreater(newCards, newSuit, value, enemy);
                if (temp != null) choosenCard = temp;
            }

            return choosenCard;
        }
    }
    private GameObject ThereIsNoMarriage(List<GameObject> enemyCards, int enemy)
    {
        int[] greatestGameCards = SetGreatestCards();
        // if you have any then play greatest cards that are in the game
        List<GameObject> greatestCards = new();
        for (int i = 0; i < 4; i++)
        {
            foreach (GameObject card in enemyCards)
            {
                if (card.GetComponent<Selectable2>().suit == i && card.GetComponent<Selectable2>().value == greatestGameCards[i])
                    greatestCards.Add(card);
            }
        }
        if (greatestCards.Count() != 0)
        {
            // find the lowest
            //return greatestCards.OrderBy(g => g.GetComponent<Selectable2>().value).First();
            int minNumber = 6;
            GameObject lowestCard = null;
            foreach (GameObject card in greatestCards)
            {
                if (card.GetComponent<Selectable2>().value < minNumber)
                {
                    minNumber = card.GetComponent<Selectable2>().value;
                    lowestCard = card;
                }
            }
            if (CanDeclareMarriage(lowestCard, enemy))
            {
                if (enemy <= 3) DeclareMarriage(lowestCard, enemy);
                else marriageNumber = lowestCard.GetComponent<Selectable2>().suit;
            }
            return lowestCard;
        }
        else
        { // list is empty
          //check if you have marriage
            List<GameObject> possibleMarriages = new();
            foreach (GameObject card in enemyCards)
            {
                if (card.GetComponent<Selectable2>().value == 2 && CanDeclareMarriage(card, enemy))
                {
                    possibleMarriages.Add(card);
                }
            }
            // if you have marriage then declare
            if (possibleMarriages.Count != 0)
            {
                //choose the best marriage
                return ChooseBestMarriage(possibleMarriages, enemy);
            }
            else
            {
                // if you don't then play the lowest random card
                return PlayLowestCardFromRandomSuit(enemyCards, enemy);
            }
        }

        int[] SetGreatestCards()
        {
            // set greatest cards in the game
            int[] greatestCard = { 5, 5, 5, 5 };
            List<int>[] values = new List<int>[4] { new(), new(), new(), new() };
            foreach (GameObject card in removedCards)
            {
                values[card.GetComponent<Selectable2>().suit].Add(card.GetComponent<Selectable2>().value);
            }
            for (int i = 0; i < 4; i++)
            {
                values[i].Sort();
                values[i].Reverse();
                foreach (int val in values[i])
                {
                    if (val == greatestCard[i]) greatestCard[i]--;
                }
            }

            return greatestCard;
        }
    }
    private GameObject ChooseBestMarriage(List<GameObject> possibleMarriages, int enemy)
    {
        //choose the best marriage
        Dictionary<string, GameObject> marriages = new();
        foreach (GameObject card in possibleMarriages) marriages.Add(card.name, card);
        if (marriages.ContainsKey("HQ") || marriages.ContainsKey("HQ(Clone)")) return SetAndReturn("HQ", 0, enemy, marriages);
        else if (marriages.ContainsKey("DQ") || marriages.ContainsKey("DQ(Clone)")) return SetAndReturn("DQ", 2, enemy, marriages);
        else if (marriages.ContainsKey("CQ") || marriages.ContainsKey("CQ(Clone)")) return SetAndReturn("CQ", 1, enemy, marriages);
        else return SetAndReturn("SQ", 3, enemy, marriages);

        GameObject SetAndReturn(string name, int number ,int enemy, Dictionary<string, GameObject> marriages)
        {
            if (enemy <= 3)
            {
                DeclareMarriage(marriages[name], enemy);
                return marriages[name];
            }
            else
            {
                marriageNumber = number;
                string clone = name + "(Clone)";
                return marriages[clone];
            }
        }
    }
    private GameObject ChooseACard(int enemy)
    {
        GameObject choosenCard = null;
        List<GameObject> enemyCards = new();
        foreach (Transform tran in thousand.players[enemy].transform)
        {
            GameObject card = tran.gameObject;
            enemyCards.Add(card);
        }
        if (cardsOnStack.Count == 0)
        { // is starting a new game
            if (marriageNumber == -1)
            { // there is no marriage
                return ThereIsNoMarriage(enemyCards, enemy);
            }
            else
            { // there is a marriage
                // if you have the greatest card from that marriage, play that card

                // update greatest cards in the game
                int[] greatestCard = { 5, 5, 5, 5 };
                List<int>[] values = new List<int>[4] { new(), new(), new(), new() };
                foreach (GameObject card in removedCards)
                {
                    values[card.GetComponent<Selectable2>().suit].Add(card.GetComponent<Selectable2>().value);
                }
                for (int i = 0; i < 4; i++)
                {
                    values[i].Sort();
                    values[i].Reverse();
                    foreach (int val in values[i])
                    {
                        if (val == greatestCard[i]) greatestCard[i]--;
                    }
                }

                foreach(GameObject card in enemyCards)
                {
                    if (card.GetComponent<Selectable2>().suit == marriageNumber && greatestCard[marriageNumber] == card.GetComponent<Selectable2>().value)
                    {
                        // you have the greatest card from declared marriage
                        return card;
                    }
                }
                // you don't

                //check if there are any cards from declared marriage
                int cardsFdm = 0;
                foreach (GameObject card in removedCards)
                {
                    if (card.GetComponent<Selectable2>().suit == marriageNumber)
                    {
                        cardsFdm++;
                    }
                }
                if (cardsFdm != 6)
                { // there are still cards from declared marriage but you don't have the greatest
                    // check for safe marriage
                    List<GameObject> safeMarriages = new();
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject possibleMarriages = null;
                        int hasOtherCards = 0;
                        foreach (GameObject card in enemyCards)
                        {
                            if (card.GetComponent<Selectable2>().suit == i)
                            {
                                if (card.GetComponent<Selectable2>().value == 2 && CanDeclareMarriage(card, enemy)) // if has queen that can declare
                                {
                                    possibleMarriages = card;
                                }
                                else if (card.GetComponent<Selectable2>().value == 4) hasOtherCards++; // if has 10
                                else if (card.GetComponent<Selectable2>().value == 5) hasOtherCards++; // if has Ace
                            }
                        }
                        if (possibleMarriages != null)
                        {
                            if (hasOtherCards == 2) safeMarriages.Add(possibleMarriages);
                            else if (hasOtherCards == 1 || hasOtherCards == 0)
                            {
                                foreach (GameObject card in removedCards)
                                {
                                    if (card.GetComponent<Selectable2>().suit == i)
                                    {
                                        if (card.GetComponent<Selectable2>().value == 4) hasOtherCards++; // if 10 is in removed cards
                                        else if (card.GetComponent<Selectable2>().value == 5) hasOtherCards++; // if Ace in removed cards
                                    }
                                }
                                if (hasOtherCards == 2) safeMarriages.Add(possibleMarriages);
                            }
                        }
                    }
                    // if you have safe marriage then declare (safe marriage (marriage and 10 and Ace) or (marriage, but king that is the greatest card in the game)
                    if (safeMarriages.Count != 0)
                    { // you have safe marriages (might be more than one)
                      //chose best marriage and declare
                        return ChooseBestMarriage(safeMarriages, enemy);
                        // if you declared safe marriage then play cards from that new marriagie until you have (1) left or 0 left if there are still many cards in the game
                        // that is done by playing greatest cards from declared marriage
                    }
                    else
                    { // you don't have safe marriage
                        // best move is probably declaring unsfe marriage if you have one
                        //check if you have marriage
                        List<GameObject> possibleMarriages = new();
                        foreach (GameObject card in enemyCards)
                        {
                            if (card.GetComponent<Selectable2>().value == 2 && CanDeclareMarriage(card, enemy))
                            {
                                possibleMarriages.Add(card);
                            }
                        }
                        // if you have marriage then declare
                        if (possibleMarriages.Count != 0)
                        {
                            //choose the best marriage
                            return ChooseBestMarriage(possibleMarriages, enemy);
                        }
                        else
                        {
                            // second best move is to calculate that is it better to play greatest card in the game if you have one or just playing your lowest card

                            //check if you have one of the greatest cards that are in the game
                            List<GameObject> greatestCards = new();
                            int[] greatestSuits = new int[4] { 0, 0, 0, 0 };
                            for (int i = 0; i < 4; i++)
                            {
                                foreach (GameObject card in enemyCards)
                                {
                                    if (card.GetComponent<Selectable2>().suit == i && card.GetComponent<Selectable2>().value == greatestCard[i])
                                    {
                                        greatestCards.Add(card);
                                        greatestSuits[i]++;
                                    }    
                                }
                            }
                            if (greatestCards.Count() != 0)
                            {
                                // what are the chances that other players don't have that color and will take the card
                                // calculate chances that you can safely play card
                                int[] howManyCards = new int[4] { 0, 0, 0, 0 };
                                float[] chances = new float[4] { 0, 0, 0, 0 };
                                //print("how many cards:");
                                for (int i = 0; i < 4; i++)
                                {
                                    foreach (GameObject card in removedCards)
                                    {
                                        if (card.GetComponent<Selectable2>().suit == i) howManyCards[i]++;
                                    }
                                    foreach (GameObject card in enemyCards)
                                    {
                                        if (card.GetComponent<Selectable2>().suit == i) howManyCards[i]++;
                                    }
                                    //print(howManyCards[i]);
                                }
                                int maxIndex = -1;
                                float maxChance = 0;
                                //print("Chances: ");
                                for (int i = 0; i < 4; i++)
                                {
                                    howManyCards[i] = 6 - howManyCards[i];
                                    chances[i] = howManyCards[i] / 6.0f;
                                    if (chances[i] > maxChance && greatestSuits[i] > 0)
                                    {
                                        maxChance = chances[i];
                                        maxIndex = i;
                                    }
                                    //print(chances[i] + " " + (greatestSuits[i] > 0));
                                }
                                //print("max chance: " + maxChance);
                                if (maxChance >= 0.5f) // choose the lowest card from greatest cards, but there is at most only one card from each suit
                                {
                                    foreach (GameObject card in greatestCards)
                                    {
                                        if (card.GetComponent<Selectable2>().suit == maxIndex) return card;
                                    }
                                }
                                else
                                {
                                    return PlayLowestCardFromRandomSuit(enemyCards, enemy);
                                }
                                print("something is wrong!");
                                return PlayLowestCardFromRandomSuit(enemyCards, enemy);
                            }
                            else
                            {
                                return PlayLowestCardFromRandomSuit(enemyCards, enemy);
                            }
                        }
                    }
                }
                else
                { // there are no cards from declared marriage
                  // then update and play the greatest card in the game
                    // if you have any then play greatest cards that are in the game
                    return ThereIsNoMarriage(enemyCards, enemy);
                }
                //return null;
            }
        }
        else
        { // is continuing a game
            GameObject firstCard = cardsOnStack[0];
            int firstSuit = firstCard.GetComponent<Selectable2>().suit;
            int firstValue = firstCard.GetComponent<Selectable2>().value;
            // breaking marriages

            // check if you have cards in this suit
            bool hasPrevSuitCards = false;
            bool hasMarriageCards = false;
            foreach(GameObject child in enemyCards)
            {
                if (child.GetComponent<Selectable2>().suit == firstSuit) hasPrevSuitCards = true;
                if (child.GetComponent<Selectable2>().suit == marriageNumber) hasMarriageCards = true;
                if (hasPrevSuitCards && hasMarriageCards) break;
            }
            if (cardsOnStack.Count == 1) //DONE
            { // there is only one card on the stack
                if (hasPrevSuitCards) ///////////////////////////////////////////////////////////////// if doesnt have to give greater
                {
                    // check what is the greatest card
                    int greatestCard = 5;
                    List<int> values = new();

                    foreach (GameObject card in removedCards)
                    {
                        if (card.GetComponent<Selectable2>().suit == firstSuit)
                        {
                            values.Add(card.GetComponent<Selectable2>().value);
                        }
                    }
                    values.Sort();
                    values.Reverse();
                    foreach (int val in values) if (val == greatestCard) greatestCard--;
                    // check if you have the greatest card that is in the game (exp 10 if Ace is in the pile)
                    foreach (GameObject card in enemyCards)
                    {
                        if (card.GetComponent<Selectable2>().suit == firstSuit && card.GetComponent<Selectable2>().value == greatestCard)
                        {
                            // if so play that card
                            return card;
                        }
                    }
                    // else play the lowest card 
                    int minNumber = 6;
                    int amoutOfAces = 0;
                    int amoutOf10s = 0;
                    GameObject cardOf10 = null;
                    foreach (GameObject card in enemyCards)
                    {
                        if (card.GetComponent<Selectable2>().value == 5) amoutOfAces++;
                        if (card.GetComponent<Selectable2>().value == 4) amoutOf10s++;
                        if (card.GetComponent<Selectable2>().value == 4 && card.GetComponent<Selectable2>().suit == firstSuit) cardOf10 = card;
                        if (card.GetComponent<Selectable2>().suit == firstSuit && card.GetComponent<Selectable2>().value < minNumber)
                        {
                            minNumber = card.GetComponent<Selectable2>().value;
                            choosenCard = card;
                        }
                    }
                    // try to avoid breaking marriage if you chave chance to declare
                    if (enemy <= 3)
                    {
                        if (cardOf10 != null)
                        {
                            if ((choosenCard.GetComponent<Selectable2>().value == 2 || choosenCard.GetComponent<Selectable2>().value == 3) && thousand.players[enemy].GetComponent<Enemy>().marriagesSuits[firstSuit] > 0)
                            {
                                print("trying not to give marriage4");
                                if (amoutOfAces > 0 || amoutOf10s >= 2)
                                {
                                    choosenCard = cardOf10;
                                }
                            }
                        }
                    }
                    else // simulation
                    {
                        if (cardOf10 != null)
                        {
                            if ((choosenCard.GetComponent<Selectable2>().value == 2 || choosenCard.GetComponent<Selectable2>().value == 3) && thousand.players[enemy].GetComponent<Simulated>().marriagesSuits[firstSuit] > 0)
                            {
                                print("trying not to give marriage4");
                                if (amoutOfAces > 0 || amoutOf10s >= 2)
                                {
                                    choosenCard = cardOf10;
                                }
                            }
                        }
                    }
                    return choosenCard;
                }
                else
                {
                    if (hasMarriageCards)
                    {
                        // if has the greatest marrige card play it, else play lowest marriage card
                        // check what is the greatest card
                        int greatestCard = 5;
                        List<int> values = new();

                        foreach (GameObject card in removedCards)
                        {
                            if (card.GetComponent<Selectable2>().suit == marriageNumber)
                            {
                                values.Add(card.GetComponent<Selectable2>().value);
                            }
                        }
                        values.Sort();
                        values.Reverse();
                        foreach (int val in values) if (val == greatestCard) greatestCard--;
                        // check if you have the greatest card that is in the game (exp 10 if Ace is in the pile)
                        foreach (GameObject card in enemyCards)
                        {
                            if (card.GetComponent<Selectable2>().suit == marriageNumber && card.GetComponent<Selectable2>().value == greatestCard)
                            {
                                // if so play that card
                                return card;
                            }
                        }
                        // else play lowest marriage card
                        int minNumber = 6;
                        foreach (GameObject card in enemyCards)
                        {
                            if (card.GetComponent<Selectable2>().suit == marriageNumber && card.GetComponent<Selectable2>().value < minNumber)
                            {
                                minNumber = card.GetComponent<Selectable2>().value;
                                choosenCard = card;
                            }
                        }
                        return choosenCard;
                    }
                    else
                    {
                        return PlayLowestCardFromRandomSuit(enemyCards, enemy);
                    }
                }
            }
            else //DONE
            { // there are two cards on stack
                GameObject secondCard = cardsOnStack[1];
                int secondSuit = secondCard.GetComponent<Selectable2>().suit;
                int secondValue = secondCard.GetComponent<Selectable2>().value;
                if (hasPrevSuitCards) //DONE
                {
                    //set ready the lowest card
                    int minNumber = 6;
                    GameObject lowestCard = null;
                    GameObject safeLowestCard1 = null;
                    GameObject safeLowestCard2 = null;
                    foreach (GameObject card in enemyCards)
                    {
                        if (card.GetComponent<Selectable2>().suit == firstSuit && card.GetComponent<Selectable2>().value <= minNumber)
                        {
                            if(enemy <= 3)
                            {
                                if ((card.GetComponent<Selectable2>().value != 2 && card.GetComponent<Selectable2>().value != 3) || thousand.players[enemy].GetComponent<Enemy>().marriagesSuits[card.GetComponent<Selectable2>().suit] == 0)
                                {
                                    //print("trying not to give marriage3");
                                    //try to give something else
                                    safeLowestCard1 = card;
                                }
                                if (thousand.players[enemy].GetComponent<Enemy>().alone10[card.GetComponent<Selectable2>().suit] == 0)
                                {
                                    //print("trying not to give alone 10 3");
                                    //try to give something else
                                    safeLowestCard2 = card;
                                }
                            }
                            else // simulation
                            {
                                if ((card.GetComponent<Selectable2>().value != 2 && card.GetComponent<Selectable2>().value != 3) || thousand.players[enemy].GetComponent<Simulated>().marriagesSuits[card.GetComponent<Selectable2>().suit] == 0)
                                    safeLowestCard1 = card;
                                if (thousand.players[enemy].GetComponent<Simulated>().alone10[card.GetComponent<Selectable2>().suit] == 0)
                                    safeLowestCard2 = card;
                            }
                            
                        }
                        if (card.GetComponent<Selectable2>().suit == firstSuit && card.GetComponent<Selectable2>().value < minNumber)
                        {
                            minNumber = card.GetComponent<Selectable2>().value;
                            lowestCard = card;
                        }
                    }
                    if (enemy <= 3)
                    {
                        if (safeLowestCard2 != null && thousand.players[enemy].GetComponent<Enemy>().alone10[lowestCard.GetComponent<Selectable2>().suit] == 1) lowestCard = safeLowestCard2;
                        if (safeLowestCard1 != null && (lowestCard.GetComponent<Selectable2>().value == 2 || lowestCard.GetComponent<Selectable2>().value == 3) && thousand.players[enemy].GetComponent<Enemy>().marriagesSuits[lowestCard.GetComponent<Selectable2>().suit] > 0) lowestCard = safeLowestCard1;
                    }
                    else // simulation
                    {
                        if (safeLowestCard2 != null && thousand.players[enemy].GetComponent<Simulated>().alone10[lowestCard.GetComponent<Selectable2>().suit] == 1) lowestCard = safeLowestCard2;
                        if (safeLowestCard1 != null && (lowestCard.GetComponent<Selectable2>().value == 2 || lowestCard.GetComponent<Selectable2>().value == 3) && thousand.players[enemy].GetComponent<Simulated>().marriagesSuits[lowestCard.GetComponent<Selectable2>().suit] > 0) lowestCard = safeLowestCard1;
                    }
                    // check which of the two cards is greater
                    if (secondSuit == marriageNumber && firstSuit != marriageNumber)
                    { // give the lowest card
                        return lowestCard;
                    }
                    else if (firstValue >= secondValue || (secondValue >= firstValue && firstSuit != secondSuit))
                    { // check if you can take
                      // if so, play minimal greater card to take
                      // else give the lowest card
                        choosenCard = PlayMinimalGreater(enemyCards, firstSuit, firstValue, enemy);
                        if (choosenCard != null) return choosenCard;
                        else return lowestCard;
                    }
                    else if (secondValue > firstValue && firstSuit == secondSuit)
                    { // secondValue > firstValue, equal suits
                        // check if you can take
                        // if so, play minimal greater card to take
                        // else give the lowest card
                        choosenCard = PlayMinimalGreater(enemyCards, firstSuit, secondValue, enemy);
                        if (choosenCard != null) return choosenCard;
                        else return lowestCard;
                    }
                    else
                    {
                        print("ten przypadek nie powinien zachodziæ");
                        return lowestCard;
                    }
                }
                else //DONE
                {//else check if you have marriage cards
                    if (hasMarriageCards && secondSuit == marriageNumber)
                    {
                        // if has greater and doesn't have first color cards then steal
                        choosenCard = PlayMinimalGreater(enemyCards, marriageNumber, secondValue, enemy);
                        if (choosenCard != null) return choosenCard;
                        // else play lowest marriage card
                        int minNumber = 6;
                        foreach (GameObject card in enemyCards)
                        {
                            if (card.GetComponent<Selectable2>().suit == marriageNumber && card.GetComponent<Selectable2>().value < minNumber)
                            {
                                minNumber = card.GetComponent<Selectable2>().value;
                                choosenCard = card;
                            }
                        }
                        return choosenCard;
                    }
                    else if (hasMarriageCards && secondSuit != marriageNumber)
                    {
                        // if so play lowest marriage card
                        int minNumber = 6;
                        foreach (GameObject card in enemyCards)
                        {
                            if (card.GetComponent<Selectable2>().suit == marriageNumber && card.GetComponent<Selectable2>().value < minNumber)
                            {
                                minNumber = card.GetComponent<Selectable2>().value;
                                choosenCard = card;
                            }
                        }
                        return choosenCard;
                    }
                    else
                    {
                        return PlayLowestCardFromRandomSuit(enemyCards, enemy);
                    }
                }
            }
        }
        //return null;
    }
}
