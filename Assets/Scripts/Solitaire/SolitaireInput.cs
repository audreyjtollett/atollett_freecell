using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SolitaireInput : MonoBehaviour
{
    private Solitaire solitaire;
    private GameObject selectedCard = null;

    void Start()
    {
        solitaire = FindAnyObjectByType<Solitaire>();
    }

    void OnBurst(InputValue value)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit == null) return;


        //
        // --- DECK ---
        //
        if (hit.CompareTag("Deck"))
        {
            solitaire.DrawFromDeck();
            solitaire.CheckForWin();
            return;
        }

        //
        // --- CARD CLICKED ---
        //
        if (hit.CompareTag("Card"))
        {
            Debug.Log("Card clicked: " + hit.name);

            // If a card is already selected, try to move to this card
            if (selectedCard != null)
            {
                if (selectedCard == hit.gameObject)
                {
                    // deselect
                    selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                    selectedCard = null;
                    solitaire.CheckForWin();
                    return;
                }

                if (solitaire.IsValidMove(selectedCard, hit.gameObject))
                {
                    solitaire.PlaceCard(selectedCard, hit.gameObject);
                    selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                    selectedCard = null;
                    solitaire.CheckForWin();
                    return;
                }
            }

            // Flip if face down AND is last in tableau
            CardSprite cardSprite = hit.gameObject.GetComponent<CardSprite>();
            if (!cardSprite.isFaceUp && solitaire.IsLastInTab(hit.gameObject))
            {
                cardSprite.isFaceUp = true;
                solitaire.CheckForWin();
                return;
            }

            // Select if face up
            if (cardSprite.isFaceUp)
            {
                // Do not select blocked waste cards
                if (hit.transform.parent.CompareTag("Waste") && solitaire.IsBlocked(hit.gameObject))
                {
                    solitaire.CheckForWin();
                    return;
                }

                Debug.Log("Card selected: " + hit.name);
                selectedCard = hit.gameObject;
                selectedCard.GetComponent<SpriteRenderer>().color = Color.gray;
            }
            solitaire.CheckForWin();
            return;
        }

        //
        // --- TABLEAU CLICKED ---
        //
        if (hit.CompareTag("Tableau"))
        {
            Debug.Log("Tableau clicked: " + hit.name);

            if (selectedCard != null &&
                solitaire.IsValidMove(selectedCard, hit.gameObject))
            {
                solitaire.PlaceCard(selectedCard, hit.gameObject);
                selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                selectedCard = null;
            }
            solitaire.CheckForWin();
            return;
        }

        //
        // --- FOUNDATION CLICKED ---
        //
        if (hit.CompareTag("Foundation"))
        {
            Debug.Log("Foundation clicked: " + hit.name);

            if (selectedCard != null &&
                solitaire.IsValidMove(selectedCard, hit.gameObject))
            {
                solitaire.PlaceCard(selectedCard, hit.gameObject);
                selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                selectedCard = null;
            }
            solitaire.CheckForWin();
            return;
        }

        //
        // --- FREECELL CLICKED (NEW BLOCK) ---
        //
        if (hit.CompareTag("FreeCell"))
        {
            Debug.Log("FreeCell clicked: " + hit.name);

            if (selectedCard != null &&
                solitaire.IsValidMove(selectedCard, hit.gameObject))
            {
                solitaire.PlaceCard(selectedCard, hit.gameObject);
                selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                selectedCard = null;
            }
            solitaire.CheckForWin();
            return;
        }
    }
}
