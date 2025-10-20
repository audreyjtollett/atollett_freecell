using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class Solitaire : MonoBehaviour
{
    public Sprite[] cardSprites; // Assign in inspector
    public GameObject cardPrefab; // Assign in inspector
    public Sprite emptySpace;
    public Sprite cardBack;
    public string[] suits = { "H", "D", "C", "S" };
    public string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
    public GameObject[] foundationPositions;
    public GameObject[] tableauPositions;
    public GameObject deckPosition;
    public GameObject wastePosition;
    List<string> deck;
    List<string> waste = new List<string>();
    List<string>[] foundations;
    List<string>[] tableaus;
    List<string> foundation0 = new List<string>();
    List<string> foundation1 = new List<string>();
    List<string> foundation2 = new List<string>();
    List<string> foundation3 = new List<string>();
    List<string> tableau0 = new List<string>();
    List<string> tableau1 = new List<string>();
    List<string> tableau2 = new List<string>();
    List<string> tableau3 = new List<string>();
    List<string> tableau4 = new List<string>();
    List<string> tableau5 = new List<string>();
    List<string> tableau6 = new List<string>();
    private System.Random random = new System.Random();
    public Vector3 cardOffset = new Vector3(0, -0.3f, -.1f);
    public float zoffset = -0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foundations = new List<string>[] { foundation0, foundation1, foundation2, foundation3 };
        tableaus = new List<string>[] { tableau0, tableau1, tableau2, tableau3, tableau4, tableau5, tableau6 };
        deck = GenerateDeck();
        //shuffle
        deck = deck.OrderBy(x => random.Next()).ToList();
        foreach (string card in deck)
        {
            Debug.Log(card);
        }
        StartCoroutine(Deal());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool IsValidMove(GameObject fromLocation, GameObject toLocation)
    {
        if (fromLocation == null || toLocation == null || fromLocation == toLocation) return false;
        ResolveTarget(toLocation, out GameObject clickedTag, out int foundationIndex, out int tabIndex);
        // waste -> tab/foundation
        if (fromLocation.transform.parent.CompareTag("Waste"))
        {
            if (clickedTag.transform.CompareTag("Tableau") && tabIndex >= 0)
            {
                Debug.Log("moving from waste to tab: " + CanPlaceOnTableau(fromLocation.name, tabIndex));
                return CanPlaceOnTableau(fromLocation.name, tabIndex);
            }
            if (clickedTag.transform.CompareTag("Foundation") && foundationIndex >= 0)
            {
                Debug.Log("moving from waste to foundation: " + CanPlaceOnFoundation(fromLocation.name, foundationIndex));
                return CanPlaceOnFoundation(fromLocation.name, foundationIndex);
            }
        }

        // tab -> tab/foundation
        if (fromLocation.transform.parent.CompareTag("Tableau"))
        {
            if (clickedTag.transform.CompareTag("Tableau") && tabIndex >= 0)
            {
                Debug.Log("moving from tab to tab: " + CanPlaceOnTableau(fromLocation.name, tabIndex));
                return CanPlaceOnTableau(fromLocation.name, tabIndex);
            }
            if (clickedTag.transform.CompareTag("Foundation") && foundationIndex >= 0)
            {
                if (IsBlocked(fromLocation))
                {
                    Debug.Log("blocked");
                    return false;
                }
                Debug.Log("moving from tab to foundation: " + CanPlaceOnFoundation(fromLocation.name, foundationIndex));
                return CanPlaceOnFoundation(fromLocation.name, foundationIndex);
            }
        }
        // foundation -> tab
        if (fromLocation.transform.parent.CompareTag("Foundation"))
        {
            if (clickedTag.transform.CompareTag("Tableau") && tabIndex >= 0)
            {
                Debug.Log("moving from foundation to tab: " + CanPlaceOnTableau(fromLocation.name, tabIndex));
                return CanPlaceOnTableau(fromLocation.name, tabIndex);
            }
        }
        return false;
    }

    public void PlaceCard(GameObject fromLocation, GameObject toLocation)
    {
        if (fromLocation == null || toLocation == null) return;
        ResolveTarget(toLocation, out GameObject clickedTag, out int foundationIndex, out int tabIndex);
        // if coming from tab, need to remove card and all cards on top of it from their original tab

        // if coming from waste, need to remove card from waste
        if (fromLocation.transform.parent.CompareTag("Waste"))
        {
            waste.Remove(fromLocation.name);
        }

        // if coming from foundation, remove card from correct foundation

        // if moving to tab, add the card to the correct tab
        if (clickedTag.transform.CompareTag("Tableau"))
        {
            // add it to the right tab
            int tableauIndex = System.Array.IndexOf(tableauPositions, clickedTag.transform.gameObject);
            tableaus[tableauIndex].Add(fromLocation.name);
            // move the card position
            if (tableaus[tableauIndex].Count == 1)
            {
                fromLocation.transform.position = toLocation.transform.position + new Vector3(0f, 0f, -.1f);
            }
            else
            {
                fromLocation.transform.position = toLocation.transform.position + cardOffset;
            }
        }
        // move all other cards on top of the original cardObject (probably put this in a helper function)

        // update parent
        fromLocation.transform.parent = clickedTag.transform;
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
        return newDeck;
    }

    IEnumerator Deal()
    {
        int tabIndex = 0;
        int cardIndex = 0;
        for (int i = deck.Count-1; i >=0 ; i--)
        {
            string card = deck[i];
            if (tabIndex > 6) break;
            deck.RemoveAt(i);
            tableaus[tabIndex].Add(card);
            if (tabIndex == cardIndex)
            {
                tabIndex++;
                cardIndex = 0;
            }
            else cardIndex++;
        }

        foreach (GameObject tabPosition in tableauPositions)
        {
            int index = System.Array.IndexOf(tableauPositions, tabPosition);
            Vector3 currentPosition = tabPosition.transform.position + new Vector3(0, 0, -0.1f);
            foreach (string card in tableaus[index])
            {
                CreateCard(card, currentPosition, tabPosition.transform, card == tableaus[index].Last());
                currentPosition += cardOffset;
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    void CreateCard(string card, Vector3 position, Transform parent, bool isFaceUp)
    {
        GameObject newCard = Instantiate(cardPrefab, position, Quaternion.identity, parent);
        newCard.name = card;
        newCard.GetComponent<SpriteRenderer>().sprite = cardSprites.First(s => s.name == card);
        newCard.GetComponent<CardHandler>().cardFront = cardSprites.First(s => s.name == card);
        newCard.GetComponent<CardHandler>().isFaceUp = isFaceUp;
    }

    public void DrawFromDeck()
    {
        if (deck.Count == 0)
        {
            while (waste.Count > 0)
            {
                string card = waste[waste.Count - 1];
                waste.RemoveAt(waste.Count - 1);
                deck.Add(card);
                foreach (Transform child in wastePosition.transform)
                {
                    Destroy(child.gameObject);
                }
                zoffset = -0.1f;
                deckPosition.GetComponent<SpriteRenderer>().sprite = cardBack;
            }
            return;
        }
        int cardsToDraw = Mathf.Min(3, deck.Count);
        Vector3 currentPosition = wastePosition.transform.position + new Vector3(0, 0, zoffset);
        for (int i = 0; i < cardsToDraw; i++)
        {
            string card = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            waste.Add(card);
            CreateCard(card, currentPosition, wastePosition.transform, true);
            currentPosition += new Vector3(0.3f, 0, -.1f);
            zoffset -= 0.1f;
        }
        if (deck.Count == 0)
        {
            deckPosition.GetComponent<SpriteRenderer>().sprite = emptySpace;
        }
    }

    public bool IsLastInTab(GameObject card)
    {
        foreach (List<string> tab in tableaus)
        {
            if (tab.Count > 0 && tab.Last() == card.name)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsBlocked(GameObject card)
    {
        foreach (Transform child in card.transform.parent)
        {
            if (child.gameObject != card && child.position.z < card.transform.position.z)
                return true;
        }
        return false;
    }

    public bool IsAlternatingColor(string card1, string card2)
    {
        char suit1 = card1[0];
        char suit2 = card2[0];
        bool isRed1 = (suit1 == 'D' || suit1 == 'H');
        bool isRed2 = (suit2 == 'D' || suit2 == 'H');
        return isRed1 != isRed2;
    }

    public bool IsSameSuit(string card1, string card2)
    {
        return card1[0] == card2[0];
    }

    public bool IsOneRankLower(string card1, string card2)
    {
        string rank1 = card1.Substring(1);
        string rank2 = card2.Substring(1);
        int index1 = System.Array.IndexOf(ranks, rank1);
        int index2 = System.Array.IndexOf(ranks, rank2);
        Debug.Log("index1: " + index1);
        Debug.Log("index2: " + index2);
        return index1 + 1 == index2;
    }

    public bool IsOneRankHigher(string card1, string card2)
    {
        string rank1 = card1.Substring(1);
        string rank2 = card2.Substring(1);
        int index1 = System.Array.IndexOf(ranks, rank1);
        int index2 = System.Array.IndexOf(ranks, rank2);
        return index1 == index2 + 1;
    }

    public bool CanPlaceOnFoundation(string card, int foundationIndex)
    {
        List<string> foundation = foundations[foundationIndex];
        if (foundation.Count == 0)
        {
            return card.EndsWith("A");
        }
        string top = foundation.Last();
        return IsOneRankHigher(card, top) && IsSameSuit(card, top);
    }

    public bool CanPlaceOnTableau(string card, int tabIndex)
    {
        List<string> tab = tableaus[tabIndex];
        if (tab.Count == 0)
        {
            Debug.Log("tab count is 0");
            return card.EndsWith("K");
        }
        string top = tab.Last();
        Debug.Log("is one lower: " + IsOneRankLower(card, top));
        Debug.Log("is alternating: " + IsAlternatingColor(card, top));
        return IsOneRankLower(card, top) && IsAlternatingColor(card, top);
    }
    
    void ResolveTarget(GameObject toLocation, out GameObject clickedTag, out int foundationIndex, out int tabIndex)
    {
        clickedTag = toLocation.transform.CompareTag("Card") ? toLocation.transform.parent.gameObject : toLocation;       
        foundationIndex = -1;
        tabIndex = -1;
        if (clickedTag.transform.CompareTag("Foundation"))
        {
            foundationIndex = System.Array.IndexOf(foundationPositions, clickedTag);
        }
        if (clickedTag.transform.CompareTag("Tableau"))
        {
            tabIndex = System.Array.IndexOf(tableauPositions, clickedTag);
        }
    }
}
