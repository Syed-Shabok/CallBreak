using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemainingCards : MonoBehaviour
{   
    private Dictionary<Suit, List<CardScript>> suitCards =  new Dictionary<Suit, List<CardScript>>
                                                {
                                                    { Suit.Hearts, new List<CardScript>() },
                                                    { Suit.Diamonds, new List<CardScript>() },
                                                    { Suit.Clubs, new List<CardScript>() },
                                                    { Suit.Spades, new List<CardScript>() }
                                                };

    [SerializeField]
    private List<CardScript> remainingHearts = new List<CardScript>();
    [SerializeField]
    private List<CardScript> remainingDiamonds = new List<CardScript>();
    [SerializeField]
    private List<CardScript> remainingClubs = new List<CardScript>();
    [SerializeField]
    private List<CardScript> remainingSpades = new List<CardScript>();

    
    public void SetCard(CardScript card)
    {
        suitCards[card.GetCardSuit()].Add(card);
        
        if(suitCards[card.GetCardSuit()].Count == 13)
        {
            SortByCardRank(suitCards[card.GetCardSuit()]);
        }
    }
    

    private void SortByCardRank(List<CardScript> cardList)
    {   
        cardList.Sort((cardA, cardB) =>
        {
            int rankA = PlayerScript.GetRankValue(cardA.GetCardRank());
            int rankB = PlayerScript.GetRankValue(cardB.GetCardRank());
            return rankA.CompareTo(rankB); // Use .CompareTo for ascending order
        });
    }

    public void RemoveCard(CardScript card)
    {
        suitCards[card.GetCardSuit()].Remove(card);
    }

    // Returns a list of the highest ranked cards of each suit that have not been played yet. 
    public List<CardScript> GetBestRemainingCards()
    {
        List<CardScript> bestCardsList = new List<CardScript>();

        foreach(Suit suitGroup in (Suit[])System.Enum.GetValues(typeof(Suit)))
        {
            if(suitCards[suitGroup].Count != 0)
            {
                bestCardsList.Add(suitCards[suitGroup][suitCards[suitGroup].Count - 1]);
            }
        }

        return bestCardsList;
    }

    // Returns the number of cards of the given parameter suit that have not yet been played.
    public int RemainingCardsOfSuit(Suit suit)
    {
        return suitCards[suit].Count;
    }
    
}
