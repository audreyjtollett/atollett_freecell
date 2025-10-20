using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SolitaireInput : MonoBehaviour
{
    private Solitaire solitaire;
    private GameObject selectedCard = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        solitaire = FindAnyObjectByType<Solitaire>();   
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void OnBurst(InputValue value)
    {
        Debug.Log("Burst");
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
        Debug.Log(worldPosition);
        Collider2D hit = Physics2D.OverlapPoint(worldPosition);
        if (hit != null)
        {
            if (hit.CompareTag("Deck"))
            {
                solitaire.DrawFromDeck();
            }
            if (hit.CompareTag("Card"))
            {
                Debug.Log("Card clicked: " + hit.name);
                if (selectedCard != null)
                {
                    if (selectedCard == hit.gameObject)
                    {
                        selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                        selectedCard = null;
                        return;
                    }
                    // check if move is valid
                    // if valid, move card
                    if (solitaire.IsValidMove(selectedCard, hit.gameObject))
                    {
                        solitaire.PlaceCard(selectedCard, hit.gameObject);
                        selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                        selectedCard = null;
                        return;
                    }
                }
                // if face down, flip it
                if (!hit.gameObject.GetComponent<CardSprite>().isFaceUp && solitaire.IsLastInTab(hit.gameObject))
                {
                    hit.gameObject.GetComponent<CardSprite>().isFaceUp = true;
                    return;
                }
                // if face up, select it
                else if (hit.gameObject.GetComponent<CardSprite>().isFaceUp)
                {
                    if (hit.gameObject.transform.parent.CompareTag("Waste") && solitaire.IsBlocked(hit.gameObject))
                    {
                        return;
                    }
                    Debug.Log("Card selected: " + hit.name);
                    selectedCard = hit.gameObject;
                    selectedCard.GetComponent<SpriteRenderer>().color = Color.gray;
                }
            }
            if (hit.CompareTag("Tableau"))
            {
                Debug.Log("Tableau clicked: " + hit.name);
                if (solitaire.IsValidMove(selectedCard, hit.gameObject))
                {
                    solitaire.PlaceCard(selectedCard, hit.gameObject);
                    selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                    selectedCard = null;
                    return;
                }
            }
            if (hit.CompareTag("Foundation"))
            {
                Debug.Log("Foundation clicked: " + hit.name);
                if (solitaire.IsValidMove(selectedCard, hit.gameObject))
                {
                    solitaire.PlaceCard(selectedCard, hit.gameObject);
                    selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                    selectedCard = null;
                    return;
                }
            }
        }
    }
}
