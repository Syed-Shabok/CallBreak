using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckScript : MonoBehaviour
{
    [SerializeField]
    private List<Card> cardDeck = new List<Card>(); //List of all cards in the Deck.

    [SerializeField]
    private List<Sprite> cardImages = new List<Sprite>(); //List of card images, assigned in the inspector

    //Creates Playing Cards and assignes them to this Deck.
    public void InitializeDeck()
    {
        cardDeck.Clear();

        int imageIndex = 0; 

        foreach (Suit suitGroup in (Suit[])System.Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rankGroup in (Rank[])System.Enum.GetValues(typeof(Rank)))
            {
                Card newCard = new Card { suit = suitGroup, rank = rankGroup, cardImage = cardImages[imageIndex]};
                cardDeck.Add(newCard);

                ++imageIndex;
            }
        }
    }
    
    public Suit GetCardSuit(int index)
    {
        Debug.Log("GetCardSuit() Function has run");
        return cardDeck[index].suit;
    }

    public Rank GetCardRank(int index)
    {   
        Debug.Log("GetCardRank() Function has run");
        return cardDeck[index].rank;
    }

    public Sprite GetCardImage(int index)
    {
        Debug.Log("GetCardImage() Function has run");
        return cardDeck[index].cardImage;
    }

    //Used by the DealCards() function in the GameManager to get cards from this Deck.
    public Card GetCard()
    {
        if (cardDeck.Count == 0)
        {
            Debug.LogWarning("CardDeck is empty. Returning null.");
            return null; // Return null if deck is empty to avoid crashing.
        }
        
        return Pop();
    }

    //Called in the GameManager Script at the start of the game, which Shuffles this Deck.
    public void ShuffleDeck() 
    {   
        Debug.Log("Deck has been shuffled.");

        for (int i = cardDeck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1); // Random index within range
            Card temp = cardDeck[i]; // Swap elements
            cardDeck[i] = cardDeck[randomIndex];
            cardDeck[randomIndex] = temp;
        }
    }

    public void DistribuiteRandomly(List<PlayerScript> players)
    {
        int playerCount = players.Count;
        int currentPlayer = 0;
        
        foreach (Card card in cardDeck)
        {
            players[currentPlayer].AddCardToHand(card); // Add card to player's hand
            currentPlayer = (currentPlayer + 1) % playerCount;
        }

        cardDeck.Clear();

        Debug.Log("Cards have been distributed Randomly.");
    } 

    public void DistributeFairlyByRank(List<PlayerScript> players)
    {   
        // Step 1: Group cards by rank
        Dictionary<Rank, List<Card>> cardsByRank = new Dictionary<Rank, List<Card>>();
        //string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

        // Initialize rank groups
        foreach (Rank rankGroup in (Rank[])System.Enum.GetValues(typeof(Rank)))
        {
            cardsByRank[rankGroup] = new List<Card>();
        }

        // Populate rank groups with cards
        foreach (Card card in cardDeck)
        {
            cardsByRank[card.rank].Add(card);
        }

        //Distribute cards in a round-robin fashion
        int playerCount = players.Count;
        int currentPlayer = 0;

        foreach (Rank rankGroup in (Rank[])System.Enum.GetValues(typeof(Rank)))
        {
            List<Card> rankCards = cardsByRank[rankGroup];
            foreach (Card card in rankCards)
            {
                players[currentPlayer].AddCardToHand(card); // Add card to player's hand
                currentPlayer = (currentPlayer + 1) % playerCount;
            }
        }

        //Clear the deck after distribution
        cardDeck.Clear();

        Debug.Log("Cards have been distributed fairly by Rank.");
    }

    public void DistributeFairlyBySuit(List<PlayerScript> players)
    {   
        //Group cards by suit
        Dictionary<Suit, List<Card>> cardsBySuit = new Dictionary<Suit, List<Card>>();
        //string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades"};

        // Initialize suit groups
        foreach (Suit suitGroup in (Suit[])System.Enum.GetValues(typeof(Suit)))
        {
            cardsBySuit[suitGroup] = new List<Card>();
        }

        // Populate suit groups with cards
        foreach (Card card in cardDeck)
        {
            cardsBySuit[card.suit].Add(card);
        }

        //Distribute cards in a round-robin fashion
        int playerCount = players.Count;
        int currentPlayer = 0;

        foreach (Suit suitGroup in (Suit[])System.Enum.GetValues(typeof(Suit)))
        {
            List<Card> suitCards = cardsBySuit[suitGroup];
            foreach (Card card in suitCards)
            {
                players[currentPlayer].AddCardToHand(card); // Add card to player's hand
                currentPlayer = (currentPlayer + 1) % playerCount;
            }
        }

        //Clear the deck after distribution
        cardDeck.Clear();

        Debug.Log("Cards have been distributed fairly by Suit.");
    }


    //Removes a Card from the top of the Deck and gives it to the function caller. 
    private Card Pop()
    {
        if (cardDeck.Count == 0)
        {
            throw new System.InvalidOperationException("Cannot pop from an empty list.");
        }

        Card lastCard = cardDeck[cardDeck.Count - 1];
        cardDeck.RemoveAt(cardDeck.Count - 1); // Remove last element
        return lastCard;
    }
}
