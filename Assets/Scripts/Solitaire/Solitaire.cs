using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Solitaire : MonoBehaviour
{
    public GameObject cardPrefab;
    public Sprite emptyPlace;
    public String[] suits = { "C", "D", "H", "S" };
    public String[] ranks = {"A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"};
    public Sprite[] cardFaces;
    public Sprite cardBack;
    public GameObject[] foundationPositions;
    public GameObject[] tableauPositions;
    public GameObject[] freecellPositions;
    public GameObject deckPosition;
    public GameObject wastePosition;
    public List<string> deck;
    public List<string> waste;
    public List<string>[] foundations;
    public List<string>[] tableaus;
    public List<string>[] freecells;
    public List<string> foundation0 = new List<string>();
    public List<string> foundation1 = new List<string>();
    public List<string> foundation2 = new List<string>();
    public List<string> foundation3 = new List<string>();
    public List<string> freecell0 = new List<string>();
    public List<string> freecell1 = new List<string>();
    public List<string> freecell2 = new List<string>();
    public List<string> freecell3 = new List<string>();
    public List<string> tableau0 = new List<string>();
    public List<string> tableau1 = new List<string>();
    public List<string> tableau2 = new List<string>();
    public List<string> tableau3 = new List<string>();
    public List<string> tableau4 = new List<string>();
    public List<string> tableau5 = new List<string>();
    public List<string> tableau6 = new List<string>();
    public List<string> tableau7 = new List<string>();
    private System.Random rng = new System.Random();
    private Vector3 cardOffset = new Vector3(0f, -.4f, -0.1f);
    private float zOffset = -.3f;
    public GameObject winPopup;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tableaus = new List<string>[] { tableau0, tableau1, tableau2, tableau3, tableau4, tableau5, tableau6, tableau7 };
        foundations = new List<string>[] { foundation0, foundation1, foundation2, foundation3 };
        freecells = new List<string>[] {freecell0, freecell1, freecell2, freecell3};
        PlayGame();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlayGame()
    {
        deck = GenerateDeck();
        foreach (string card in deck)
        {
            Debug.Log(card);
        }
        Deal();
    }

    List<string> GenerateDeck()
    {
        List<string> newDeck = new List<string>();
        foreach (string suit in suits)
        {
            foreach (string rank in ranks)
            {
                newDeck.Add(suit + rank);
            }
        }
        //shuffle
        newDeck = newDeck.OrderBy(x => rng.Next()).ToList();
        return newDeck;
    }

    void Deal()
    {
        Debug.Log("Dealing cards...");

        // FreeCell: 4 columns get 7 cards, 4 get 6 cards
        int[] tableauSizes = { 7, 7, 7, 7, 6, 6, 6, 6 };
        int deckIndex = 0;

        for (int t = 0; t < tableaus.Length; t++)
        {
            tableaus[t].Clear();
            for (int i = 0; i < tableauSizes[t]; i++)
            {
                string card = deck[deckIndex];
                deckIndex++;
                tableaus[t].Add(card);
            }
        }

        // Remove dealt cards from deck entirely (FreeCell has no draw pile)
        deck.RemoveRange(0, deckIndex);

        // Now instantiate cards visually
        for (int t = 0; t < tableauPositions.Length; t++)
        {
            GameObject tabPosition = tableauPositions[t];
            Vector3 currentPosition = tabPosition.transform.position + new Vector3(0, 0, -.1f);

            foreach (string card in tableaus[t])
            {
                CreateCard(card, currentPosition, tabPosition.transform, true);
                currentPosition += cardOffset;
            }
        }
    }


    void CreateCard(string cardName, Vector3 position, Transform parent, bool isFaceUp)
    {
        Debug.Log("Creating card " + cardName + " at " + position);
        GameObject newCard = Instantiate(cardPrefab, position, Quaternion.identity, parent);
        newCard.name = cardName;
        Sprite cardFace = cardFaces.FirstOrDefault(s => s.name == cardName);
        newCard.GetComponent<CardSprite>().cardFace = cardFace;
        newCard.GetComponent<CardSprite>().isFaceUp = isFaceUp;
    }

    public void DrawFromDeck()
    {
        Debug.Log("Drawing from deck");
        if (deck.Count == 0)
        {
            while (waste.Count > 0)
            {
                string card = waste.Last();
                waste.RemoveAt(waste.Count - 1);
                deck.Add(card);
            }
            foreach (Transform child in wastePosition.transform)
            {
                Destroy(child.gameObject);
            }
            zOffset = -.3f;
            deckPosition.transform.GetComponent<SpriteRenderer>().sprite = cardBack;
            return;
        }

        // need to reset x position for all cards not in the drawn set of 3
        foreach (Transform child in wastePosition.transform)
        {
            child.transform.position = new Vector3(wastePosition.transform.position.x, child.transform.position.y, child.transform.position.z);
        }
        int cardsToDraw = Math.Min(3, deck.Count);
        for (int i = 0; i < cardsToDraw; i++)
        {
            string card = deck.Last();
            deck.RemoveAt(deck.Count - 1);
            waste.Add(card);
            CreateCard(card, wastePosition.transform.position + new Vector3(i * 0.3f, 0, zOffset), wastePosition.transform, true);
            zOffset -= .3f;
        }
        
        Debug.Log("Deck count: " + deck.Count);
        if (deck.Count == 0)
        {
            // show empty deck
            deckPosition.transform.GetComponent<SpriteRenderer>().sprite = emptyPlace;
        }
    }

    public bool IsValidMove(GameObject cardObject, GameObject targetObject)
    {
        if (cardObject == targetObject || cardObject == null || targetObject == null) return false;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        ResolveTarget(targetObject, out GameObject clickedTag, out int foundationIndex, out int tabIndex, out int freecellIndex, mousePosition);

        // foundation -> tab/freecell
        if (cardObject.transform.parent.CompareTag("Foundation"))
        {
            if (clickedTag.transform.CompareTag("Tableau") && tabIndex >= 0)
            {
                Debug.Log("can place on tab from found: " + CanPlaceOnTableau(cardObject.name, tabIndex));
                return CanPlaceOnTableau(cardObject.name, tabIndex);
            }
            if (clickedTag.CompareTag("FreeCell") && freecellIndex >= 0)
            {
                // freecell must be empty
                return freecells[freecellIndex].Count == 0;
            }
            Debug.Log("bad found to tab click");
            return false;
        }

        // tab -> tab/foundation/freecell
        if (cardObject.transform.parent.CompareTag("Tableau"))
        {
            if (clickedTag.transform.CompareTag("Tableau") && tabIndex >= 0)
            {
                Debug.Log("can place on tab from tab: " + CanPlaceOnTableau(cardObject.name, tabIndex));
                return CanPlaceOnTableau(cardObject.name, tabIndex);
            }
            if (clickedTag.transform.CompareTag("Foundation") && foundationIndex >= 0)
            {
                if (IsBlocked(cardObject))
                {
                    Debug.Log("Blocked from tab->tab/foundation");
                    return false;
                }
                Debug.Log("can place on found from tab: " + CanPlaceOnFoundation(cardObject.name, foundationIndex));
                return CanPlaceOnFoundation(cardObject.name, foundationIndex);
            }
            if (clickedTag.CompareTag("FreeCell") && freecellIndex >= 0)
            {
                // Only top card can move, freecell must be empty
                //if (!IsLastInTab(cardObject)) return false;
                return freecells[freecellIndex].Count == 0;
            }
            Debug.Log("Bad tab to tab/found click");
            return false;
        }
        // freecell -> tab/foundation/freecell
        if (cardObject.transform.parent.CompareTag("FreeCell"))
        {
            if (clickedTag.transform.CompareTag("Tableau") && tabIndex >= 0)
            {
                Debug.Log("can place on tab from freecell: " + CanPlaceOnTableau(cardObject.name, tabIndex));
                return CanPlaceOnTableau(cardObject.name, tabIndex);
            }
            if (clickedTag.transform.CompareTag("Foundation") && foundationIndex >= 0)
            {
                if (IsBlocked(cardObject))
                {
                    Debug.Log("Blocked from freecell->foundation");
                    return false;
                }
                Debug.Log("can place on found from freecell: " + CanPlaceOnFoundation(cardObject.name, foundationIndex));
                return CanPlaceOnFoundation(cardObject.name, foundationIndex);
            }
            if (clickedTag.CompareTag("FreeCell") && freecellIndex >= 0)
            {
                // Only top card can move, freecell must be empty
                //if (!IsLastInTab(cardObject)) return false;
                return freecells[freecellIndex].Count == 0;
            }
            Debug.Log("Bad tab to tab/found click");
            return false;
        }
        Debug.Log("nothing matched. returning false");
        return false;
    }

    public void MoveCardsAbove(GameObject origParent, int originalTabIndex, int destTabIndex, int cardsToMoveCount, GameObject clickedTag, GameObject cardObject)
    {
        if (originalTabIndex == -1 || cardsToMoveCount <= 1) return;
        List<string> origTab = tableaus[originalTabIndex];
        int origCount = origTab.Count;
        int origIndex = origCount - cardsToMoveCount + 1;
        for (int i = 0; i < cardsToMoveCount -1 ; i++)
        {
            string movingCardName = origTab[origIndex];
            origTab.RemoveAt(origIndex);
            tableaus[destTabIndex].Add(movingCardName);
            GameObject movingCardObj = null;
            foreach (Transform child in origParent.transform)
            {
                if (child.gameObject.name == movingCardName)
                {
                    movingCardObj = child.gameObject;
                    break;
                }
            }
            if(movingCardObj!=null)
            {
                movingCardObj.transform.parent = clickedTag.transform;
                movingCardObj.transform.position = cardObject.transform.position + (cardOffset * (i + 1));
            }
        }
    }
    
    public void PlaceCard(GameObject cardObject, GameObject targetObject)
    {
        if (cardObject == targetObject || cardObject == null || targetObject == null) 
            return;

        // Resolve target
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        ResolveTarget(targetObject, out GameObject clickedTag, 
                    out int foundationIndex, out int tableauIndex, out int freecellIndex, mousePosition);

        GameObject originalParent = cardObject.transform.parent.gameObject;

        int originalTabIndex = -1;
        int cardsToMoveCount = 1;

        //
        // REMOVE CARD FROM ORIGINAL LOCATION
        //

        // Coming from tableau
        if (originalParent.CompareTag("Tableau"))
        {
            for (int i = 0; i < tableaus.Length; i++)
            {
                if (tableaus[i].Contains(cardObject.name))
                {
                    originalTabIndex = i;
                    int cardPos = tableaus[i].IndexOf(cardObject.name);
                    cardsToMoveCount = tableaus[i].Count - cardPos;
                    tableaus[i].Remove(cardObject.name);
                    break;
                }
            }
        }

        // Coming from foundation
        else if (originalParent.CompareTag("Foundation"))
        {
            foreach (List<string> foundation in foundations)
            {
                if (foundation.Contains(cardObject.name))
                {
                    foundation.Remove(cardObject.name);
                    break;
                }
            }
        }

        // Coming from waste
        else if (originalParent.CompareTag("Waste"))
        {
            waste.Remove(cardObject.name);
        }

        // Coming from freecell
        else if (originalParent.CompareTag("FreeCell"))
        {
            foreach (List<string> fc in freecells)
            {
                if (fc.Contains(cardObject.name))
                {
                    fc.Remove(cardObject.name);
                    break;
                }
            }
        }

        //
        // ADDING CARD TO DESTINATION
        //

        // → TABLEAU
        if (clickedTag.CompareTag("Tableau"))
        {
            int destIndex = Array.IndexOf(tableauPositions, clickedTag);

            tableaus[destIndex].Add(cardObject.name);

            if (tableaus[destIndex].Count == 1)
                cardObject.transform.position = clickedTag.transform.position + new Vector3(0, 0, -.03f);
            else
                cardObject.transform.position = targetObject.transform.position + cardOffset;

            cardObject.transform.parent = clickedTag.transform;

            // Move any cards that were stacked above original card
            MoveCardsAbove(originalParent, originalTabIndex, destIndex, cardsToMoveCount, clickedTag, cardObject);

            return;
        }

        // → FOUNDATION
        if (clickedTag.CompareTag("Foundation"))
        {
            foundations[foundationIndex].Add(cardObject.name);

            cardObject.transform.parent = clickedTag.transform;

            // Ensure correct Z-order: newer cards go on top
            float z = -0.03f * foundations[foundationIndex].Count;
            cardObject.transform.position = clickedTag.transform.position + new Vector3(0, 0, z);

            return;
        }

        // → FREECELL
        if (clickedTag.CompareTag("FreeCell"))
        {
            // FreeCell logic: only 1 card allowed
            if (freecells[freecellIndex].Count == 0)
            {
                freecells[freecellIndex].Add(cardObject.name);
                cardObject.transform.parent = clickedTag.transform;
                cardObject.transform.position = clickedTag.transform.position + new Vector3(0, 0, -.03f);
            }
            return;
        }
    }


    public bool IsLastInTab(GameObject cardObject)
    {
        Transform parent = cardObject.transform.parent;
        if (!parent.CompareTag("Tableau"))
            return false;

        // top card has the highest Z (closest to camera)
        float cardZ = cardObject.transform.position.z;

        foreach (Transform child in parent)
        {
            if (child != cardObject && child.position.z > cardZ)
                return false;
        }

        return true;
    }


    public bool IsBlocked(GameObject cardObject)
    {
        foreach (Transform child in cardObject.transform.parent)
        {
            if (child.gameObject != cardObject && child.position.z < cardObject.transform.position.z)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsAlternatingColor(string card1, string card2)
    {
        if (card1 == null || card2 == null) return false;
        char suit1 = card1[0];
        char suit2 = card2[0];
        bool isRed1 = (suit1 == 'D' || suit1 == 'H');
        bool isRed2 = (suit2 == 'D' || suit2 == 'H');
        return isRed1 != isRed2;
    }

    public bool IsSameSuit(string card1, string card2)
    {
        if (card1 == null || card2 == null) return false;
        return card1[0] == card2[0];
    }

    public bool IsOneRankHigher(string card1, string card2)
    {
        if (card1 == null || card2 == null) return false;
        int rank1 = Array.IndexOf(ranks, card1.Substring(1));
        int rank2 = Array.IndexOf(ranks, card2.Substring(1));
        Debug.Log("rank1: " + rank1);
        Debug.Log("rank2: " + rank2);
        return rank1 == (rank2 + 1) % ranks.Length;
    }

    public bool IsOneRankLower(string card1, string card2)
    {
        if (card1 == null || card2 == null) return false;
        int rank1 = Array.IndexOf(ranks, card1.Substring(1));
        int rank2 = Array.IndexOf(ranks, card2.Substring(1));
        return (rank1 + 1) % ranks.Length == rank2;
    }

    public bool CanPlaceOnFoundation(string card, int foundationIndex)
    {
        if (foundations[foundationIndex].Count == 0)
        {
            return card.Substring(1) == "A";
        }
        string topCard = foundations[foundationIndex].Last();
        Debug.Log("topCard: " + topCard + ", card: " + card);
        Debug.Log("IsSameSuit: " + IsSameSuit(card, topCard));
        Debug.Log("IsOneRankHigher: " + IsOneRankHigher(card, topCard));
        return IsSameSuit(card, topCard) && IsOneRankHigher(card, topCard);
    }

    public bool CanPlaceOnTableau(string card, int tableauIndex)
    {
        if (tableaus[tableauIndex].Count == 0)
        {
            return card.Substring(1) == "K";
        }
        string topCard = tableaus[tableauIndex].Last();
        return IsAlternatingColor(card, topCard) && IsOneRankLower(card, topCard);
    }

    void ResolveTarget(GameObject toLocation, out GameObject clickedTag,
                   out int foundationIndex, out int tableauIndex, out int freecellIndex,
                   Vector2 mousePosition)
    {
        foundationIndex = -1;
        tableauIndex = -1;
        freecellIndex = -1;

        // If we clicked a card, use its parent
        if (toLocation.CompareTag("Card"))
            clickedTag = toLocation.transform.parent.gameObject;
        else
            clickedTag = toLocation;

        // If it wasn't a FreeCell, test if mouse is over a freecell explicitly
        if (!clickedTag.CompareTag("FreeCell"))
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
            Collider2D hit = Physics2D.OverlapPoint(worldPosition);

            if (hit != null && hit.CompareTag("FreeCell"))
                clickedTag = hit.gameObject;
        }

        // Now identify indices
        if (clickedTag.CompareTag("Foundation"))
            foundationIndex = Array.IndexOf(foundationPositions, clickedTag);

        else if (clickedTag.CompareTag("Tableau"))
            tableauIndex = Array.IndexOf(tableauPositions, clickedTag);

        else if (clickedTag.CompareTag("FreeCell"))
            freecellIndex = Array.IndexOf(freecellPositions, clickedTag);
    }
    public void CheckForWin()
    {
        bool won = true;
        foreach (List<string> foundation in foundations)
        {
            if (foundation.Count < 13)
            {
                won = false;
                break;
            }
        }

        if (won)
        {
            Debug.Log("You Win!");
            if (winPopup != null)
                winPopup.SetActive(true);
        }
    }
}
