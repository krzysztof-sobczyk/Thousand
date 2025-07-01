using System.Collections.Generic;
using UnityEngine;

public class UpdateSprite : MonoBehaviour
{
    public Sprite cardBack;
    public Sprite cardFront;
    private Selectable2 selectable;
    private Thousand thousand;
    private List<string> deck;

    void Start()
    {
        thousand = FindObjectOfType<Thousand>();
        selectable = GetComponent<Selectable2>();
        deck = thousand.GenerateDeck();
        int i = 0;
        foreach (var card in deck)
        {
            if (name == card)
            {
                cardFront = thousand.cardFaces[i];
            }
            i++;
        }
    }
    void Update()
    {
        if (!selectable.faceUp)
        {
            if(GetComponent<SpriteRenderer>().sprite != cardBack)
                GetComponent<SpriteRenderer>().sprite = cardBack;
        }
        else
        {
            if (GetComponent<SpriteRenderer>().sprite != cardFront)
                GetComponent<SpriteRenderer>().sprite = cardFront;
        }
    }
}
