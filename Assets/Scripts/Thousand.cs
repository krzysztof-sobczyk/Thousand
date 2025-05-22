using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Thousand : MonoBehaviour
{
    private static string[] suits = { "H", "C", "D", "S" };
    private static string[] values = { "9", "J", "Q", "K", "10", "A" };
    private List<string> deck;
    private List<string> cards = new();
    [SerializeField] private GameObject cardPrefab;
    public GameObject[] players;
    public GameObject[] piles;
    public List<GameObject> playerCards = new();
    private List<GameObject> muskCards = new();
    public Sprite[] cardFaces;
    [SerializeField] private GameObject table;
    [SerializeField] private GameObject BiddingPanel;
    [SerializeField] private GameObject ExchangingPanel;
    public int bid;
    public int phase = 0;
    public bool canDragCards = true;
    public GameObject[] placed = new GameObject[2];
    [SerializeField] private GameObject whoMustObj;

    Dictionary<string, int> suitsOrder = new Dictionary<string, int>
    {
        { "H", 0 },
        { "C", 1 },
        { "D", 2 },
        { "S", 3 }
    };
    Dictionary<string, int> valuesOrder = new Dictionary<string, int>
    {
        { "9", 0 },
        { "J", 1 },
        { "Q", 2 },
        { "K", 3 },
        { "0", 4 },
        { "A", 5 }
    };
    Dictionary<string, int> safeToGive = new Dictionary<string, int>
    {
        { "9", 0 },
        { "J", 1 },
        { "Q", 5 },
        { "K", 6 },
        { "0", 3 },
        { "A", 4 }
    };

    private void Start()
    {
        players[0].name = FindObjectOfType<Settings>().yourName;
        PlayCards();
    }
    public void PlayCards()
    {
        phase = 0;
        deck = GenerateDeck();
        Shuffle(deck);
        StartCoroutine(GameSequence());
    }
    public List<string> GenerateDeck()
    {
        List<string> newDeck = new List<string>();
        foreach (string s in suits)
        {
            foreach (string v in values)
            {
                newDeck.Add(s + v);
            }
        }
        return newDeck;
    }

    void Shuffle<T>(List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
    private IEnumerator GameSequence()
    {
        FindObjectOfType<Timer>().gameObject.SetActive(true);
        FindObjectOfType<Timer>().stop_clock = false;
        yield return StartCoroutine(GenerateCards());
        yield return StartCoroutine(DealCards());
        // adding player cards to the list
        for (int i = 0; i < players[0].transform.childCount; i++)
            playerCards.Add(players[0].transform.GetChild(i).gameObject);
        // adding musk to the list
        for (int i = 0; i < players[2].transform.childCount; i++)
        {
            var child = players[2].transform.GetChild(i).gameObject;
            muskCards.Add(child);
        }
        SortPlayerCards();
        yield return new WaitForSeconds(0.25f);
        MoveEnemyCards(1);
        MoveEnemyCards(3);
        //move musk
        StartCoroutine(MoveCards(muskCards, SetCardDestinations(muskCards, -18f)));
        //rotate player cards
        yield return StartCoroutine(RotateCards(playerCards, 0.5f));
        //move player cards
        yield return StartCoroutine(MoveCards(playerCards, SetFancyDestinations(playerCards), 0.4f, SetFancyRotations(playerCards)));
        yield return FadeOutWhoMust();
        foreach (GameObject card in playerCards)
            card.GetComponent<Draggable>().enableMovement = true;
        BiddingPanel.SetActive(true);
        phase = 1;
    }
    private IEnumerator GameSequence2()
    {
        // move musk 2
        yield return StartCoroutine(MoveCards(muskCards, SetCardDestinations(muskCards, -65, -200, 0)));
        yield return StartCoroutine(RotateCards(muskCards, 0.75f, false));
        //
        int declarer = table.GetComponent<Table>().declarerNumber;
        if (declarer == 0)
        {
            yield return StartCoroutine(MoveMuskToThePlayer2());
            //resort player cards
            SortPlayerCards();
            yield return StartCoroutine(MoveCards(playerCards, SetFancyDestinations(playerCards), 0.5f, SetFancyRotations(playerCards)));
            ExchangingPanel.SetActive(true);
            phase = 3;
        }
        else
        {
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(RotateCards(muskCards));
            MoveMuskToTheEnemy(declarer);
            List<GameObject> enemyCards = new();
            List<Vector3> destinations = new();
            List<float> rotations = new();
            for (int i = 0; i < players[declarer].transform.childCount; i++)
            {
                GameObject card = players[declarer].transform.GetChild(i).gameObject;
                enemyCards.Add(card);
                rotations.Add(90);
                destinations.Add(Vector3.zero);
            }
            yield return StartCoroutine(MoveCards(enemyCards, destinations, 0.5f, rotations));
            ShuffleEnemyCards(declarer);
            yield return StartCoroutine(players[declarer].GetComponent<Enemy>().Exchanging());
        }
        foreach (GameObject card in playerCards)
            card.GetComponent<Draggable>().enableMovement = true;
    }

    private IEnumerator GenerateCards()
    {
        float yOffset = 0.0f;
        int i = 0;
        foreach (string card in deck)
        {
            GameObject newCard = Instantiate(cardPrefab, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - yOffset, 0), Quaternion.identity, table.transform);
            newCard.name = card;
            newCard.GetComponent<Selectable2>().suit = suitsOrder[card[0].ToString()];
            newCard.GetComponent<Selectable2>().value = valuesOrder[card[^1].ToString()];
            newCard.GetComponent<Selectable2>().safeToGive = safeToGive[card[^1].ToString()];
            yOffset += 0.015f;
            cards.Add(card);
            yield return new WaitForSeconds(0.08f);
            i++;
        }
    }

    private IEnumerator DealCards()
    {
        int turn = 0;
        int k = 4;
        cards.Reverse();
        foreach (string cardS in cards)
        {
            GameObject card = table.transform.Find(cardS).gameObject;
            if (turn == k) turn = 0;
            if (turn == 0)
            {
                card.transform.parent = players[0].transform;
                StartCoroutine(MoveCards(new List<GameObject> { card }, new List<Vector3> { Vector3.zero }, 0.2f));
            }
            else if (turn == 1)
            {
                card.transform.parent = players[1].transform;
                StartCoroutine(MoveCards(new List<GameObject> { card }, new List<Vector3> { Vector3.zero }, 0.2f, new List<float> { 90 }));
            }
            else if (turn == 2)
            {
                if (players[2].transform.childCount < 3)
                {
                    card.transform.parent = players[2].transform;
                    StartCoroutine(MoveCards(new List<GameObject> { card }, new List<Vector3> { Vector3.zero }, 0.2f));
                }
                else
                {
                    k = 3;
                    card.transform.parent = players[3].transform;
                    StartCoroutine(MoveCards(new List<GameObject> { card }, new List<Vector3> { Vector3.zero }, 0.2f, new List<float> { 90 }));
                }
            }
            else
            {
                card.transform.parent = players[3].transform;
                StartCoroutine(MoveCards(new List<GameObject> { card }, new List<Vector3> { Vector3.zero }, 0.2f, new List<float> { 90 }));
            }
            turn++;
            yield return new WaitForSeconds(0.1f);
        }
    }
    public IEnumerator MoveCards(List<GameObject> objects, List<Vector3> destinations, float durationTime = 0.6f, List<float> rotdest = null)
    {
        bool add0 = false;
        if (rotdest == null)
        {
            rotdest = new List<float>();
            add0 = true;
        }
        float elapsedTime = 0f;
        List<Vector3> startPositions = new();
        List<float> startRotation = new();
        // the same start positions
        foreach (var obj in objects)
        {
            startPositions.Add(obj.transform.localPosition);
            startRotation.Add(obj.transform.eulerAngles.z);
            if (add0) rotdest.Add(0);
        }
        // all at the same time
        while (elapsedTime < durationTime)
        {
            float t = elapsedTime / durationTime;
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject card = objects[i];
                Vector3 spos = startPositions[i];
                Vector3 dest = destinations[i];
                float srot = startRotation[i];
                card.transform.localPosition = Vector3.Lerp(spos, dest, t);
                card.transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(srot, rotdest[i], t));
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].transform.localPosition = destinations[i];
            objects[i].transform.eulerAngles = new Vector3(0, 0, rotdest[i]);
            objects[i].GetComponent<Draggable>().SavePosition();
        }
    }
    public IEnumerator RotateCards(List<GameObject> cards, float durationTime = 0.6f, bool atTheSameTime = true, float zRot = 0)
    {
        float elapsedTime = 0f;
        durationTime /= 2f;
        if (atTheSameTime)
        {
            while (elapsedTime < durationTime)
            {
                float t = elapsedTime / durationTime;
                for (int i = 0; i < cards.Count; i++)
                {
                    GameObject card = cards[i];
                    if (zRot == 0)
                        card.transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(0, 90, t), zRot);
                    else
                        card.transform.eulerAngles = new Vector3(Mathf.LerpAngle(0, 90, t), 0, zRot);
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            foreach (GameObject card in cards)
            {
                card.transform.eulerAngles = new Vector3(0, 90, zRot);
                // set face
                if (!card.GetComponent<Selectable2>().faceUp) card.GetComponent<Selectable2>().faceUp = true;
                else card.GetComponent<Selectable2>().faceUp = false;
            }

            elapsedTime = 0f;
            while (elapsedTime < durationTime)
            {
                float t = elapsedTime / durationTime;
                for (int i = 0; i < cards.Count; i++)
                {
                    GameObject card = cards[i];
                    if (zRot == 0)
                        card.transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(90, 0, t), zRot);
                    else
                        card.transform.eulerAngles = new Vector3(Mathf.LerpAngle(90, 0, t), 0, zRot);
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            foreach (GameObject card in cards)
            {
                card.transform.eulerAngles = new Vector3(0, 0, zRot);
            }
        }
        else
        {
            for (int i = 0; i < cards.Count; i++)
            {
                GameObject card = cards[i];
                elapsedTime = 0f;
                while (elapsedTime < durationTime)
                {
                    float t = elapsedTime / durationTime;
                    if (zRot == 0)
                        card.transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(0, 90, t), zRot);
                    else
                        card.transform.eulerAngles = new Vector3(Mathf.LerpAngle(0, 90, t), 0, zRot);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                card.transform.eulerAngles = new Vector3(0, 90, zRot);
                // set face
                if (!card.GetComponent<Selectable2>().faceUp) card.GetComponent<Selectable2>().faceUp = true;
                else card.GetComponent<Selectable2>().faceUp = false;

                elapsedTime = 0f;
                while (elapsedTime < durationTime)
                {
                    float t = elapsedTime / durationTime;
                    if (zRot == 0)
                        card.transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(90, 0, t), zRot);
                    else
                        card.transform.eulerAngles = new Vector3(Mathf.LerpAngle(90, 0, t), 0, zRot);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                card.transform.eulerAngles = new Vector3(0, 0, zRot);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void SortPlayerCards()
    {
        // sorting by suits
        playerCards.Sort((a, b) => suitsOrder[a.name[0].ToString()].CompareTo(suitsOrder[b.name[0].ToString()]));

        // sorting by values
        List<GameObject> newPlayerCards = new();
        List<GameObject> temp = new();
        for (int i = 0; i < playerCards.Count - 1; i++)
        {
            if (playerCards[i].name[0] == playerCards[i + 1].name[0])
            {
                temp.Add(playerCards[i]);
            }
            else
            {
                temp.Add(playerCards[i]);
                temp.Sort((a, b) => valuesOrder[a.name[^1].ToString()].CompareTo(valuesOrder[b.name[^1].ToString()]));
                foreach (GameObject card in temp) newPlayerCards.Add(card);
                temp.Clear();
            }
        }
        temp.Add(playerCards[^1]);
        temp.Sort((a, b) => valuesOrder[a.name[^1].ToString()].CompareTo(valuesOrder[b.name[^1].ToString()]));
        foreach (GameObject card in temp) newPlayerCards.Add(card);
        playerCards = newPlayerCards;
        // adjusting order in the scene
        foreach (GameObject card in playerCards) card.transform.parent = null;
        foreach (GameObject card in playerCards) card.transform.parent = players[0].transform;
    }

    private List<Vector3> SetCardDestinations(List<GameObject> xPlayerCards, float fixedNumb = -36f, float yStartPos = 0f, float zAdder = 1f)
    {
        List<Vector3> positions = new();
        float xStartPos = Mathf.Round(fixedNumb * xPlayerCards.Count);
        float xOffset = 0f;
        float zOffset = 0f;
        float adder = Math.Abs(2 * xStartPos) / (xPlayerCards.Count - 1);
        if (xPlayerCards.Count == 1) xStartPos = 0;
        foreach (GameObject card in xPlayerCards)
        {
            positions.Add(new Vector3(0 + xStartPos + xOffset, 0.0f + yStartPos, 0.0f - zOffset));
            xOffset += adder;
            zOffset += zAdder;
        }
        return positions;
    }
    private List<Vector3> SetFancyDestinations(List<GameObject> xPlayerCards, float fixedNumb = -36f, float yStartPos = 0f, float zAdder = 1f)
    {
        List<Vector3> positions = new();
        float xStartPos = Mathf.Round(fixedNumb * xPlayerCards.Count);
        float xOffset = 0f;
        float yOffset = 0f;
        float zOffset = 0f;
        float adder = Math.Abs(2 * xStartPos) / (xPlayerCards.Count - 1);
        if (xPlayerCards.Count == 1) xStartPos = 0;
        for (int i = 0; i < xPlayerCards.Count; i++)
        {
            yOffset = -3f * i * (i - xPlayerCards.Count + 1);
            positions.Add(new Vector3(0 + xStartPos + xOffset, 0.0f + yStartPos + yOffset, 0.0f - zOffset));
            xOffset += adder;
            zOffset += zAdder;
        }
        return positions;
    }
    private List<float> SetFancyRotations(List<GameObject> xPlayerCards, float fixedNumb = 4.1f)
    {
        List<float> rotations = new();
        float rot = Mathf.Round(fixedNumb * (xPlayerCards.Count / 2));
        float rotOffset = 0f;
        float adder = Math.Abs(2 * rot) / (xPlayerCards.Count - 1);
        for (int i = 0; i < xPlayerCards.Count; i++)
        {
            rotations.Add(rot + rotOffset);
            rotOffset -= adder;
        }
        return rotations;
    }
    private List<Vector3> SetEnemyDestinations(List<GameObject> xPlayerCards, int enemy, float fixedNumb = -28f, float zAdder = 1f)
    {
        List<Vector3> positions = new();
        float yStartPos = Mathf.Round(fixedNumb * xPlayerCards.Count);
        float xOffset = 0f;
        float yOffset = 0f;
        float zOffset = 0f;
        float adder = Math.Abs(2 * yStartPos) / (xPlayerCards.Count - 1);
        if (xPlayerCards.Count == 1) yStartPos = 0;
        for (int i = 0; i < xPlayerCards.Count; i++)
        {
            if (enemy == 1)
                xOffset = -3f * i * (i - xPlayerCards.Count + 1);
            else
                xOffset = 3f * i * (i - xPlayerCards.Count + 1);
            positions.Add(new Vector3(0 + xOffset, 0.0f + yStartPos + yOffset, 0.0f - zOffset));
            yOffset += adder;
            zOffset += zAdder;
        }
        return positions;
    }
    private List<float> SetEnemyRotations(List<GameObject> xPlayerCards, int enemy, float fixedNumb = 4.1f)
    {
        List<float> rotations = new();
        float rot = Mathf.Round(fixedNumb * (xPlayerCards.Count / 2));
        float rotOffset = 90f;
        float adder = Math.Abs(2 * rot) / (xPlayerCards.Count - 1);
        if (enemy == 3)
        {
            for (int i = 0; i < xPlayerCards.Count; i++)
            {
                rotations.Add(rot + rotOffset);
                rotOffset -= adder;
            }
        }
        else
        {
            for (int i = 0; i < xPlayerCards.Count; i++)
            {
                rotations.Add(rotOffset - rot);
                rotOffset += adder;
            }
        }
        return rotations;
    }
    public void MoveCardsAfterDragging()
    {
        StartCoroutine(MoveCards(playerCards, SetFancyDestinations(playerCards), 0.5f, SetFancyRotations(playerCards)));
    }
    public void MoveEnemyCards(int enemy)
    {
        List<GameObject> enemyCards = new();
        foreach (Transform tran in players[enemy].transform)
        {
            GameObject card = tran.gameObject;
            enemyCards.Add(card);
        }
        StartCoroutine(MoveCards(enemyCards, SetEnemyDestinations(enemyCards, enemy), 0.5f, SetEnemyRotations(enemyCards, enemy)));
    }
    public IEnumerator MoveCardsAfterDragging2()
    {
        yield return StartCoroutine(MoveCards(playerCards, SetFancyDestinations(playerCards), 0.6f, SetFancyRotations(playerCards)));
    }
    public void MoveCardsExcluding()
    {
        List<GameObject> newList = new(playerCards);
        newList.Remove(placed[0]); newList.Remove(placed[1]);
        StartCoroutine(MoveCards(newList, SetFancyDestinations(newList), 0.5f, SetFancyRotations(newList)));
    }
    public void OnDeclarerSelected(int newBid)
    {
        bid = newBid;
        BiddingPanel.SetActive(false);
        phase = 2;
        StartCoroutine(GameSequence2());
    }
    private IEnumerator MoveMuskToThePlayer2()
    {
        yield return new WaitForSeconds(1f);
        foreach (var card in muskCards)
        {
            card.transform.parent = players[0].transform;
            playerCards.Add(card);
        }
        muskCards.Clear();
    }
    private void MoveMuskToTheEnemy(int enemy)
    {
        foreach (var card in muskCards)
        {
            card.transform.parent = players[enemy].transform;
        }
        muskCards.Clear();
    }

    // used to rotate given cards
    public IEnumerator RotateEnemyCards(GameObject card, int enemy)
    {
        List<GameObject> enemyCards = new();
        foreach (Transform tran in players[enemy].transform)
        {
            GameObject crd = tran.gameObject;
            enemyCards.Add(crd);
        }
        yield return StartCoroutine(MoveCards(enemyCards, SetEnemyDestinations(enemyCards, enemy), 0.6f, SetEnemyRotations(enemyCards, enemy)));

        yield return StartCoroutine(RotateCards(new List<GameObject> { card }, 0.6f, true, 90));
        MoveEnemyCards(enemy);
    }
    private void ShuffleEnemyCards(int enemy)
    {
        List<GameObject> enemyCards = new();
        foreach (Transform tran in players[enemy].transform)
        {
            GameObject card = tran.gameObject;
            enemyCards.Add(card);
        }
        System.Random random = new();
        int n = enemyCards.Count;
        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            (enemyCards[n], enemyCards[k]) = (enemyCards[k], enemyCards[n]);
        }
        foreach (GameObject card in enemyCards) card.transform.parent = null;
        foreach (GameObject card in enemyCards) card.transform.parent = players[enemy].transform;
    }
    private IEnumerator FadeOutWhoMust()
    {
        whoMustObj.SetActive(true);
        Text textCol = whoMustObj.GetComponent<Text>();
        textCol.text = players[FindObjectOfType<Settings>().whoMust].name + "\nmust score at least 100 points";
        Color startCol = new Color(textCol.color.r, textCol.color.g, textCol.color.b, 1);
        Color endCol = new Color(textCol.color.r, textCol.color.g, textCol.color.b, 0);
        textCol.color = startCol;
        float elapsedTime = 0f;
        float durationTime = 0.75f;
        yield return new WaitForSeconds(1.5f);
        while (elapsedTime < durationTime)
        {
            textCol.color = Color.Lerp(startCol, endCol, elapsedTime / durationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        textCol.color = endCol;
        textCol.color = startCol;
        whoMustObj.SetActive(false);
    }
}
