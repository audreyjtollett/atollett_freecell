using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public GameObject deckPosition;
    public GameObject wastePosition;
    public List<string> deck;
    public List<string> waste;
    public List<string>[] foundations;
    public List<string>[] tableaus;
    public List<string> foundation0 = new List<string>();
    public List<string> foundation1 = new List<string>();
    public List<string> foundation2 = new List<string>();
    public List<string> foundation3 = new List<string>();
    public List<string> tableau0 = new List<string>();
    public List<string> tableau1 = new List<string>();
    public List<string> tableau2 = new List<string>();
    public List<string> tableau3 = new List<string>();
    public List<string> tableau4 = new List<string>();
    public List<string> tableau5 = new List<string>();
    public List<string> tableau6 = new List<string>();
    private System.Random rng = new System.Random();
    private Vector3 cardOffset = new Vector3(0f, -.3f, -0.1f);
    private float zOffset = -.3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tableaus = new List<string>[] { tableau0, tableau1, tableau2, tableau3, tableau4, tableau5, tableau6 };
        foundations = new List<string>[] { foundation0, foundation1, foundation2, foundation3 };
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
        int tabIndex = 0;
        int cardIndex = 0;
        for (int i = deck.Count - 1; i >= 0; i--)
        {
            string card = deck[i];
            if (tabIndex > 6)
            {
                break;
            }
            deck.RemoveAt(i);
            tableaus[tabIndex].Add(card);
            if (tabIndex == cardIndex)
            {
                cardIndex = 0;
                tabIndex++;
            }
            else cardIndex++;
        }

        foreach (GameObject tabPosition in tableauPositions)
        {
            Debug.Log("Dealing to tableau position " + tabPosition.name);
            int index = Array.IndexOf(tableauPositions, tabPosition);
            Vector3 currentPosition = tabPosition.transform.position + new Vector3(0, 0, -.1f);
            foreach (string card in tableaus[index])
            {
                Debug.Log("Dealing card " + card + " to tableau " + index);
                // create card
                CreateCard(card, currentPosition, tabPosition.transform, card == tableaus[index].Last());
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
        ResolveTarget(targetObject, out GameObject clickedTag, out int foundationIndex, out int tabIndex);

        // waste -> tab/foundation
        if (cardObject.transform.parent.CompareTag("Waste"))
        {
            if (clickedTag.transform.CompareTag("Tableau") && tabIndex >= 0)
            {
                Debug.Log("can place on tab: " + CanPlaceOnTableau(cardObject.name, tabIndex));
                return CanPlaceOnTableau(cardObject.name, tabIndex);
            }
            if (clickedTag.transform.CompareTag("Foundation") && foundationIndex >= 0)
            {
                Debug.Log("can place on found: " + CanPlaceOnFoundation(cardObject.name, foundationIndex));
                return CanPlaceOnFoundation(cardObject.name, foundationIndex);
            }
            return false;
        }

        // foundation -> tab
        if (cardObject.transform.parent.CompareTag("Foundation"))
        {
            if (clickedTag.transform.CompareTag("Tableau") && tabIndex >= 0)
            {
                Debug.Log("can place on tab from found: " + CanPlaceOnTableau(cardObject.name, tabIndex));
                return CanPlaceOnTableau(cardObject.name, tabIndex);
            }
            Debug.Log("bad found to tab click");
            return false;
        }

        // tab -> tab/foundation
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
                Debug.Log("can place on found from tab: " + CanPlaceOnFoundation(cardObject.name, tabIndex));
                return CanPlaceOnFoundation(cardObject.name, foundationIndex);
            }
            Debug.Log("Bad tab to tab/found click");
            return false;
        }
        Debug.Log("nothing matched. returning false");
        return false;
    }

    public void PlaceCard(GameObject cardObject, GameObject targetObject)
    {
        if (cardObject == targetObject || cardObject == null || targetObject == null) return;
        ResolveTarget(targetObject, out GameObject clickedTag, out int foundationIndex, out int tabIndex);
        // if coming from tab, need to remove card and all cards on top of it from their original tab

        if (cardObject.transform.parent.CompareTag("Waste"))
        {
            waste.Remove(cardObject.name);
        }
        // if coming from foundation, remove card from correct foundation

        // if moving to tab, add the card to the correct tab
        if (clickedTag.transform.CompareTag("Tableau"))
        {
            // add it to the right tab
            int tableauIndex = System.Array.IndexOf(tableauPositions, clickedTag);
            tableaus[tableauIndex].Add(cardObject.name);
            // move the card position
            if (tableaus[tableauIndex].Count == 1)
                cardObject.transform.position = targetObject.transform.position + new Vector3(0f, 0f, -.03f);
            else
                cardObject.transform.position = targetObject.transform.position + cardOffset;
            // update parent
            cardObject.transform.parent = clickedTag.transform;
            // move all other cards on top of the original cardObject (probably put this in a helper function)
        }
        // if moving to foundation, add card to correct foundation
    }

    public bool IsLastInTab(GameObject cardObject)
    {
        foreach(List<string> tab in tableaus)
        {
            if (tab.Count > 0 && tab.Last() == cardObject.name)
            {
                return true;
            }
        }
        return false;
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
        return rank1 == rank2 + 1;
    }

    public bool IsOneRankLower(string card1, string card2)
    {
        if (card1 == null || card2 == null) return false;
        int rank1 = Array.IndexOf(ranks, card1.Substring(1));
        int rank2 = Array.IndexOf(ranks, card2.Substring(1));
        return rank1 + 1 == rank2;
    }

    public bool CanPlaceOnFoundation(string card, int foundationIndex)
    {
        if (foundations[foundationIndex].Count == 0)
        {
            return card.Substring(1) == "A";
        }
        string topCard = foundations[foundationIndex].Last();
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

    void ResolveTarget(GameObject toLocation, out GameObject clickedTag, out int foundationIndex, out int tableauIndex)
    {
        clickedTag = toLocation.transform.CompareTag("Card") ? toLocation.transform.parent.gameObject : toLocation;
        foundationIndex = -1;
        tableauIndex = -1;
        if (clickedTag.transform.CompareTag("Foundation"))
            foundationIndex = System.Array.IndexOf(foundationPositions, clickedTag);
        else if (clickedTag.transform.CompareTag("Tableau"))
            tableauIndex = System.Array.IndexOf(tableauPositions, clickedTag);
    }
}
