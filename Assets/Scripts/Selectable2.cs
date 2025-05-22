using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable2 : MonoBehaviour
{
    public bool faceUp = false;
    public int suit;
    public int value;
    public int safeToGive;
    private void Start()
    {
        if (CompareTag("Card"))
        {

        }
    }
}
