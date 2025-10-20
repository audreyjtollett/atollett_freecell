using UnityEngine;
using UnityEngine.InputSystem;

public class SolitaireInput : MonoBehaviour
{
    private Solitaire solitaire;
    private GameObject selectedCard;
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
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
        Collider2D hit = Physics2D.OverlapPoint(worldPosition);
        if (hit != null)
        {
            if (hit.gameObject.CompareTag("Deck"))
            {
                Debug.Log("Deck clicked");
                solitaire.DrawFromDeck();
            }
            if (hit.gameObject.CompareTag("Card"))
            {
                Debug.Log("clicked: " + hit.name);
                if (selectedCard != null)
                {
                    // check if valid move
                    if (solitaire.IsValidMove(selectedCard, hit.gameObject))
                    {
                        // make the move
                        solitaire.PlaceCard(selectedCard, hit.gameObject);
                        // deselect card
                        selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                        selectedCard = null;
                        return;
                    }
                }
                if (!hit.gameObject.GetComponent<CardHandler>().isFaceUp && solitaire.IsLastInTab(hit.gameObject))
                {
                    hit.gameObject.GetComponent<CardHandler>().isFaceUp = true;
                }
                else if (hit.gameObject.GetComponent<CardHandler>().isFaceUp)
                {
                    if (hit.gameObject.transform.parent.CompareTag("Waste") && solitaire.IsBlocked(hit.gameObject))
                    {
                        return;
                    }
                    selectedCard = hit.gameObject;
                    selectedCard.GetComponent<SpriteRenderer>().color = Color.gray;
                }
            }
            if (hit.gameObject.CompareTag("Foundation"))
            {
                Debug.Log("foundation clicked: " + hit.name);
                // check if valid move
                if (solitaire.IsValidMove(selectedCard, hit.gameObject))
                {
                    // make the move
                    solitaire.PlaceCard(selectedCard, hit.gameObject);
                    // deselect card
                    selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                    selectedCard = null;
                    return;
                }
            }
            if (hit.gameObject.CompareTag("Tableau"))
            {
                Debug.Log("tab clicked: " + hit.name);
                // check if valid move
                if (solitaire.IsValidMove(selectedCard, hit.gameObject))
                {
                    // make the move
                    solitaire.PlaceCard(selectedCard, hit.gameObject);
                    // deselect card
                    selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                    selectedCard = null;
                    return;
                }
            }
        }
    }
}
