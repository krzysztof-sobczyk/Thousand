using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 previousPos;
    private Quaternion startRotation;
    public bool enableMovement = false;
    public bool placed1 = false;
    public bool placed2 = false;
    public bool onTable = false;
    public bool released = false;
    public bool exited = true;
    private Thousand thousand;

    private void Start()
    {
        thousand = FindObjectOfType<Thousand>();
    }
    public void SavePosition()
    {
        startPos = transform.localPosition;
        startRotation = transform.rotation;
    }
    private void OnMouseDown()
    {
        exited = true;
        released = false;
        previousPos = transform.localPosition;
        if (placed1)
        {
            placed1 = false;
            thousand.players[1].GetComponent<Enemy>().hasCard = false;
        }
        else if (placed2)
        {
            placed2 = false;
            thousand.players[3].GetComponent<Enemy>().hasCard = false;
        }
    }
    private void OnMouseDrag()
    {
        if(enableMovement && thousand.canDragCards)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            gameObject.transform.position = new Vector3(mousePos.x, mousePos.y, 0.0f);
            gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, -20.0f);

            Vector3 direction = transform.localPosition - previousPos;
            if (direction.magnitude > 0.01f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
    private void OnMouseUp()
    {
        released = true;
        if (!placed1 && !placed2 && !onTable && exited && enableMovement)
        {
            PerformAMove();
        }
        else
        {
            StartCoroutine(SkipAFrame());
        }
    }
    private IEnumerator SkipAFrame()
    {
        bool p1 = placed1;
        bool p2 = placed2;
        yield return null;
        if (p1 != placed1) thousand.placed[0] = gameObject;
        else if (p1 == placed1 == false) thousand.placed[0] = null;
        if (p2 != placed2) thousand.placed[1] = gameObject;
        else if (p2 == placed2 == false) thousand.placed[1] = null;
        if (placed1 || placed2)
        {
            thousand.MoveCardsExcluding();
        }
    }
    private void PerformAMove()
    {
        StartCoroutine(thousand.MoveCards(new List<GameObject> { gameObject }, new List<Vector3> { startPos }, 0.5f));
        if (!thousand.players[1].GetComponent<Enemy>().hasCard) thousand.placed[0] = null;
        if (!thousand.players[3].GetComponent<Enemy>().hasCard) thousand.placed[1] = null;
        thousand.MoveCardsExcluding();
    }
    private void OnMouseEnter()
    {
        if (enableMovement && thousand.canDragCards && !placed1 && !placed2 && !onTable && gameObject.transform.parent.gameObject == thousand.players[0])
        {
            gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y + 5.0f, -10.0f);
        }
    }
    private void OnMouseExit()
    {
        if (enableMovement && thousand.canDragCards && !placed1 && !placed2 && !onTable && gameObject.transform.parent.gameObject == thousand.players[0])
        {
            gameObject.transform.localPosition = startPos;
            gameObject.transform.rotation = startRotation;
        }
    }
}
