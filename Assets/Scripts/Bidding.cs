using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bidding : MonoBehaviour
{
    [SerializeField] private Text declarerText;
    [SerializeField] private Text currentBidText;
    [SerializeField] private GameObject PlayerBidHUD;
    [SerializeField] private GameObject BidInput;
    [SerializeField] private GameObject InfoHUD;
    [SerializeField] private GameObject ElectedDeclarer;
    [SerializeField] private GameObject[] PassImages;
    [SerializeField] private GameObject[] BidImages;
    [SerializeField] private GameObject Bid10Obj;
    private Thousand thousand;
    private Table table;
    public int currentBid;
    private int declarer = 0;
    private int declarerPlayerTurn;
    public bool[] pass = new bool[4];
    private bool oneTime = false;
    private int turn;
    private bool bid10OneTime = true;
    private void Start()
    {
        PlayerBidHUD.SetActive(false);
        oneTime = false;
        thousand = FindObjectOfType<Thousand>();
        table = FindObjectOfType<Table>();
        currentBid = 100;
        declarerPlayerTurn = FindObjectOfType<Settings>().whoMust;
        declarer = declarerPlayerTurn;
        turn = declarerPlayerTurn;
        table.TurnPointer.GetComponent<SpriteRenderer>().color = new Color(0.2204321f, 0.6842156f, 0.9137466f);
        table.TurnPointer.SetActive(true);
        DeclarerChanged();
        Bid10Obj.SetActive(true);
        BidInput.SetActive(true);
        StartCoroutine(WaitAndSkipTurn());
    }
    private void OnEnable()
    {
        for (int i = 0; i < 4; i++) pass[i] = false;
        pass[2] = true;
    }

    private void Update()
    {
        currentBidText.text = currentBid.ToString();
        declarerText.text = thousand.players[declarer].name;
        if ((currentBid == 350) && !oneTime)////////////////////////////////////////////
        {
            oneTime = true;
            OnEveryonePassed();
        }
    }
    private void DeclarerChanged()
    {
        if(declarer == 0 || turn != 0 || pass[0])
        {
            PlayerBidHUD.SetActive(false);
        }
        else if (!pass[0] && turn == 0)
        {
            PlayerBidHUD.SetActive(true);
        }
    }
    private IEnumerator ColorChange()
    {
        Text textCol = BidInput.transform.GetChild(2).gameObject.GetComponent<Text>();
        textCol.color = Color.red;
        float elapsedTime = 0f;
        float durationTime = 1.5f;
        while (elapsedTime < durationTime)
        {
            textCol.color = Color.Lerp(Color.red, Color.black, elapsedTime / durationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        textCol.color = Color.black;
    }
    private void ClearBid()
    {
        var input = BidInput.GetComponent<InputField>();
        input.text = "";
    }
    public void OnEveryonePassed()
    {
        PlayerBidHUD.SetActive(false);
        table.everyonePassed = true;
        string elDec = "The declarer is: " + declarerText.text + "\nwith the bid of: " + currentBid.ToString();
        InfoHUD.SetActive(false);
        ElectedDeclarer.SetActive(true);
        ElectedDeclarer.GetComponent<Text>().text = elDec;
        foreach (GameObject card in thousand.playerCards)
            card.GetComponent<Draggable>().enableMovement = false;
        StartCoroutine(FadeOut());
        table.TurnPointer.SetActive(false);
        table.TurnPointer.transform.localPosition = Vector3.zero;
        declarerPlayerTurn++;
        if (declarerPlayerTurn == 2) declarerPlayerTurn = 3;
        if (declarerPlayerTurn == 4) declarerPlayerTurn = 0;
        FindObjectOfType<Settings>().whoMust = declarerPlayerTurn;
    }
    private IEnumerator FadeOut()
    {
        Text textCol = ElectedDeclarer.GetComponent<Text>();
        Color startCol = new Color(1, 1, 1, 1);
        Color endCol = new Color(1, 1, 1, 0);
        textCol.color = startCol;
        float elapsedTime = 0f;
        float durationTime = 1.25f;
        yield return new WaitForSeconds(2.5f);
        while (elapsedTime < durationTime)
        {
            textCol.color = Color.Lerp(startCol, endCol, elapsedTime / durationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        textCol.color = endCol;
        textCol.color = startCol;
        ElectedDeclarer.SetActive(false);
        table.declarerNumber = declarer;
        thousand.OnDeclarerSelected(currentBid);
    }
    private IEnumerator WaitAndSkipTurn()
    {
        yield return new WaitForSeconds(0.75f);
        NextTurn(true);
    }
    private IEnumerator BidImageFade(int player, int bid)
    {
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

    //public functions
    public void NextTurn(bool skipping = false)
    {
        if (!skipping)
        {
            int ct = 0;
            if (pass[0]) ct++;
            if (pass[1]) ct++;
            if (pass[3]) ct++;
            if (ct >= 2)
            {
                OnEveryonePassed();
                return;
            }
        }

        turn++;
        if(turn == 4) turn = 0;
        if (pass[turn])
        {
            turn++;
            if (turn == 2) turn = 3;
            if (turn == 4) turn = 0;
        }
        DeclarerChanged();
        StartCoroutine(table.TurnPointerMove(turn));
        table.HandleBids(turn);

        if (!table.PlayerHasMarriage() && currentBid >= 120 && bid10OneTime) 
        { 
            Bid10Obj.SetActive(false); 
            BidInput.SetActive(false);
            bid10OneTime = false; 
        }
    }
    public void Bid10()
    {
        currentBid += 10;
        declarer = 0;
        StartCoroutine(BidImageFade(0, currentBid));
        DeclarerChanged();
        NextTurn();
    }
    public void Pass()
    {
        pass[0] = true;
        PassImages[0].SetActive(true);
        DeclarerChanged();
        NextTurn();
    }
    public void SubmitBid()
    {
        var input = BidInput.GetComponent<InputField>();
        int bid = int.Parse(input.text);
        bool valChanged = false;
        if(bid % 10 != 0)
        {
            input.text = ((bid + 5) / 10 * 10).ToString();
            valChanged = true;
            StartCoroutine(ColorChange());
        }
        if (bid > 350)
        {
            input.text = "350";
            if (!valChanged)
                StartCoroutine(ColorChange());
        }
        else if (bid <= currentBid)
        {
            input.text = (currentBid + 10).ToString();
            if (!valChanged)
                StartCoroutine(ColorChange());
        }
        else if (bid > 120 && !table.PlayerHasMarriage())
        {
            input.text = "120";
            if (!valChanged)
                StartCoroutine(ColorChange());
        }
        else if (!valChanged)
        {
            currentBid = bid;
            declarer = 0;
            StartCoroutine(BidImageFade(0, bid));
            DeclarerChanged();
            NextTurn();
            ClearBid();
        }
    }
    public void EnemyBid(int enemy, int bid)
    {
        currentBid = bid;
        declarer = enemy;
        int index;
        if (enemy == 1) index = 1;
        else index = 2;
        StartCoroutine(BidImageFade(index, bid));
        DeclarerChanged();
        NextTurn();
    }
    public void OnEnemyPass(int enemy)
    {
        int index;
        if (enemy == 1) index = 1;
        else index = 2;
        PassImages[index].SetActive(true);
    }
}
