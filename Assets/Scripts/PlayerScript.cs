using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{   
    [SerializeField]
    protected List<CardScript> handCards = new List<CardScript>();  //List of all cards in this players hand.
    [SerializeField]
    private Dictionary<Suit, List<CardScript>> suitCards =  new Dictionary<Suit, List<CardScript>>
                                                {
                                                    { Suit.Hearts, new List<CardScript>() },
                                                    { Suit.Diamonds, new List<CardScript>() },
                                                    { Suit.Clubs, new List<CardScript>() },
                                                    { Suit.Spades, new List<CardScript>() }
                                                };
    [SerializeField]
    private int numberOfCards = 0;

    [SerializeField]
    private GameObject cardPrefab;                                            

    [SerializeField]
    private float playerPridiction = 0; 
    [SerializeField]
    private float playerScore = 0;
    [SerializeField]
    private float currentLeadPoints = 0;

    private List<Card> unorganizedCardList = new List<Card>();
    
    [SerializeField]
    private Transform cardPlacementLocation;

    [SerializeField]
    private CardsOnTableScript cardsOnTable;  //Referance to the CardsOnTable Scriptable Object.


    [SerializeField]
    private RemainingCards remainingCards;  //Referance to the Remaining Cards Script.


    //Variables used for Desicion Making:
    private List<CardScript> tableCards = new List<CardScript>();
    private Suit requiredSuit;
    private Rank highestRankOnTable;

    [SerializeField]
    private TextMeshProUGUI playerScoreText;
    [SerializeField]
    private List<TextMeshProUGUI> leadPointTexts =  new List<TextMeshProUGUI>();
    [SerializeField]
    private TextMeshProUGUI totalPointsText;
    [SerializeField]
    private ScoreBoard scoreBoard;

    public void SetPlayerPridiction(int value)
    {
        playerPridiction = value;
        ShowPlayerScore();
    }

    public float GetPlayerPridiction()
    {
        return playerPridiction;
    }

    public void UpdatePlayerScore(int score)
    {
        playerScore += score;
        ShowPlayerScore();
    }

    private void ShowPlayerScore()
    {
        playerScoreText.text = $"({playerScore}/{playerPridiction})";
    }

    public void SetCurrentLeadPoints(int leadNumber, string playerName)
    {
        if(playerPridiction == playerScore)
        {
            currentLeadPoints = playerScore;
        }
        else if(playerPridiction < playerScore)
        {
            currentLeadPoints = playerPridiction + (playerScore - playerPridiction) / 10;
        }
        else
        {
            currentLeadPoints = -playerPridiction;
        }    

        //leadPointTexts[leadNumber].text = $"{currentLeadPoints}";
        scoreBoard.SetPlayerPoints(this.gameObject.name, currentLeadPoints);

        UpdateScoreBoard(leadNumber);

        playerScore = 0;
        //ShowPlayerScore();
    }

    public float CalculateTotalPoints()
    {
        float totalPoints = 0;

        for(int i = 0; i < 5; ++i)
        {
            totalPoints += scoreBoard.GetPlayerPoints(this.gameObject.name, i);
        }

        totalPointsText.text =  totalPoints.ToString("F1");

        return totalPoints;
    }


    private void UpdateScoreBoard(int leadNumber)
    {   
        Debug.Log("UpdateScoreBoard() function has run");
        Debug.Log($"Lead Number = {leadNumber}.");

        for(int i = 0; i < leadNumber + 1; ++i)
        {   
            float score = scoreBoard.GetPlayerPoints(this.gameObject.name, i);

            Debug.Log($"lead-{i} score for player: {this.gameObject.name} set to: {score}");

            leadPointTexts[i].text = $"{score}";
        }
    }

    public void AddCardToHand(Card card)
    {   
        unorganizedCardList.Add(card);
    
        if(unorganizedCardList.Count == 13)
        {
            OrganizeCardsBySuit();
            //OrganizeHandCards();   
        }
    }

    private void OrganizeCardsBySuit()
    {
        Debug.Log("OrganizeCardsBySuit() has run");
        
        Dictionary<Suit, List<Card>> cardsBySuit = new Dictionary<Suit, List<Card>>();

        foreach(Suit suitGroup in (Suit[])System.Enum.GetValues(typeof(Suit)))
        {
            cardsBySuit[suitGroup] = new List<Card>();
        }

        foreach(Card card in unorganizedCardList)
        {
            cardsBySuit[card.GetCardSuit()].Add(card);
        }

        foreach(Suit suitGroup in (Suit[])System.Enum.GetValues(typeof(Suit)))
        {
            SortByCardRank(cardsBySuit[suitGroup]);
        }

        int index = 0;
        
        foreach(Suit suitGroup in (Suit[])System.Enum.GetValues(typeof(Suit)))
        {   
            //Debug.Log($"Adding {suitGroup} cards to screen");
            foreach(Card card in cardsBySuit[suitGroup])
            {
                handCards[index].SetCard(card);

                handCards[index].GetComponent<Renderer>().enabled = true;

                //Setting this player as owner of all cards in their hand.
                handCards[index].SetOwnerOfCard(this);

                //Inserting player's cards into the Remaining Cards Script for AI purpose.
                remainingCards.SetCard(handCards[index]);

                ++index;
            } 
        }

        UpdateSuitWiseLists();
    }

    private void SortByCardRank(List<Card> cardList)
    {   
        //Fair Card Distribution Attempt.

        cardList.Sort((cardA, cardB) =>
        {
            int rankA = PlayerScript.GetRankValue(cardA.GetCardRank());
            int rankB = PlayerScript.GetRankValue(cardB.GetCardRank());
            return rankA.CompareTo(rankB); // Use .CompareTo for ascending order
        });
    }

    private void UpdateSuitWiseLists()
    {
        foreach(Suit suitGroup in (Suit[])System.Enum.GetValues(typeof(Suit)))
        {
            suitCards[suitGroup].Clear();
        }
        
        foreach (var card in handCards)
        {   
            Suit suitGroup = card.GetCardSuit();    
            
            switch (suitGroup)
            {
                case Suit.Hearts:
                    suitCards[suitGroup].Add(card);
                    break;
                case Suit.Diamonds:
                    suitCards[suitGroup].Add(card);
                    break;
                case Suit.Clubs:
                    suitCards[suitGroup].Add(card);
                    break;
                case Suit.Spades:
                    suitCards[suitGroup].Add(card);
                    break;
            }
        }

        Debug.Log("Suit-wise lists updated.");
    }

    private bool Chances()
    {
        return UnityEngine.Random.value < 0.3f; // Returns true if random value is less than 0.3, else false
    }


    public void MakePridictionOfWins()
    {   
        float possibleWins = GetPossibleWins();
                
        if((int) Math.Floor(possibleWins) == 0)
        {
            this.SetPlayerPridiction(1);
        }
        else
        {   
            if(Chances())
            {
                possibleWins += 2;
            }
            else
            {
                possibleWins += 1;
            }

            this.SetPlayerPridiction((int) Math.Floor(possibleWins));
        }
        
    }

    private float GetPossibleWins()
    {   
        Debug.Log($"Checking cards of {this.gameObject.name}:");

        float possibleWins = 0;

        foreach(Suit suitGroup in (Suit[])System.Enum.GetValues(typeof(Suit)))
        {
            if(suitGroup == Suit.Spades)
            {
                continue;
            }
            else
            {
                possibleWins += CheckCardsOfSuit(suitGroup);
            }
        }
        
        float spadeWins = 0;
        spadeWins += CheckCardsOfSuit(Suit.Spades);

        if(spadeWins == 0)
        {      
            if(GetNumberOfCards(Suit.Spades) >= 4)
            {
                Debug.Log($"Player has {GetNumberOfCards(Suit.Spades)} Spades cards: +1");
                
                ++possibleWins;
            }
        }
        else
        {
            possibleWins += spadeWins;
        }

        return possibleWins;
    }

    private float CheckCardsOfSuit(Suit suitName)
    {
        float winChance = 0;

        Debug.Log($"Checking {suitName} cards:");

        if(HasRankOfSuit(suitName, Rank.Ace))
        {   
            Debug.Log($"{this.gameObject.name} has {Rank.Ace} of {suitName}: +1");

            ++winChance;
            
            if(HasRankOfSuit(suitName, Rank.King))
            {   
                Debug.Log($"{this.gameObject.name} also has {Rank.King} of {suitName}: +1");

                ++winChance;
            }
        }
        else if(HasRankOfSuit(suitName, Rank.King))
        {   
            Debug.Log($"{this.gameObject.name} has {Rank.King} of {suitName}: +0.5");

            winChance += 0.5f;

            if(HasRankOfSuit(suitName, Rank.Queen))
            {
                Debug.Log($"{this.gameObject.name} also has {Rank.Queen} of {suitName}: +0.5");
                
                winChance += 0.5f;
            }
            else if(GetNumberOfCards(suitName) >= 3)
            {
                Debug.Log($"{this.gameObject.name} has a total of {GetNumberOfCards(suitName)} {suitName} cards: +0.5");

                winChance += 0.5f;
            }
        }

        return winChance;
    }


    private bool HasRankOfSuit(Suit suitGroup, Rank rankGroup)
    {
        return suitCards[suitGroup].Any(card => card.GetCardRank() == rankGroup);
    }

    private int GetNumberOfCards(Suit suitGroup)
    {
        return suitCards[suitGroup].Count;
    }

   
    //Removes a selected card from the handCards list (i.e. players hand).
    public void RemoveCardFromHand(CardScript selectedCard)
    {
        if(handCards.Contains(selectedCard))
        {
            handCards.Remove(selectedCard);

            UpdateSuitWiseLists();

            //Removes selected card from the remaining cards list. 
            remainingCards.RemoveCard(selectedCard);
            
            //Changes the onTable bool value to true. 
            selectedCard.SetCardOnTable();
            
            //Places this card on the table and adds it to CardsOnTable Scriptable Object's list of tableCards.
            cardsOnTable.PlaceCardOnTable(selectedCard, cardPlacementLocation);

            // Destroy(selectedCard.gameObject);
            selectedCard.gameObject.SetActive(false);

            Debug.Log($"{this.gameObject.name} placed the card {selectedCard.GetCardRank()} of {selectedCard.GetCardSuit()}.");
        }
        else
        {
            Debug.Log($"{selectedCard.GetCardRank()} of {selectedCard.GetCardSuit()} card is not in {this.gameObject.name}'s hand.");
        }
    }


    //Makes oponent players place random cards from thier hand on the table.
    //And adds it to CardsOnTable Scriptable Object's list of tableCards.
    public void RemoveRandomCard()
    {   
        int r = UnityEngine.Random.Range(0, handCards.Count);
        Debug.Log($"Value of r is: {r}.");

        CardScript selectedCard = handCards[r];

        if(handCards.Contains(selectedCard))
        {
            handCards.Remove(selectedCard);

            UpdateSuitWiseLists();

            //Removes selected card from the remaining cards list. 
            remainingCards.RemoveCard(selectedCard);

            //Changes the onTable bool value to true.
            selectedCard.SetCardOnTable();
            
            //Places this card on the table
            cardsOnTable.PlaceCardOnTable(selectedCard, cardPlacementLocation);    

            //Destroy(selectedCard.gameObject);
            selectedCard.gameObject.SetActive(false);

            Debug.Log($"{this.gameObject.name} placed the card {selectedCard.GetCardRank()} of {selectedCard.GetCardSuit()}.");
        }
        else
        {
            Debug.Log($"Tried to remove card that is not in player's hand.");
        }
    }

    public void MainPlayerTurn()
    {   
        this.tableCards = cardsOnTable.GetCardsOnTable();

        SetPlayableCards(GetPlayableCards());
    }

    public void DisablePlayerCards()
    {
        for(int i = 0; i < handCards.Count; ++i)
        {
            handCards[i].SetClickable(false);
        }
    }

    private List<CardScript> GetPlayableCards()
    {   
        if(tableCards.Count == 0)
        {
            return new List<CardScript>();
        }

        List<CardScript> requiredSuitCardsOnHand = GetRequiredSuitCards();
            
        if(requiredSuitCardsOnHand.Count == 0)
        {
            List<CardScript> spadeCardOnHand = GetAvailableSpades();

            if(spadeCardOnHand.Count == 0)
            {
                return handCards;
            }
            else
            {
                return spadeCardOnHand;
            }

        }
        else
        {   
            // Debug.Log("The requiredSuitCardsOnHand cards in GetPlayableCards are: ");
            // for(int i = 0; i < requiredSuitCardsOnHand.Count; ++i)
            // {
            //     Debug.Log($"{requiredSuitCardsOnHand[i].GetCardRank()} of {requiredSuitCardsOnHand[i].GetCardSuit()}");
            // }

            this.highestRankOnTable = GetHighestRankOnTable();
            
            List<CardScript> sutableRankCards = GetSutableRankCards(requiredSuitCardsOnHand);

            if(sutableRankCards.Count == 0)
            {   
                Debug.Log("sutableRankCards is empty");
                return requiredSuitCardsOnHand;
            }
            else
            {
                return sutableRankCards;
            }
        }
    }

    private void SetPlayableCards(List<CardScript> playableCards)
    {   
        Debug.Log("SetPlayableCards() has run.");

        if(playableCards.Count == 0)
        {
            for (int i = 0; i < handCards.Count; ++i)
            {   
                if(GameManager.GetCurrentRound() == 0 && handCards[i].GetCardSuit() == Suit.Spades)
                {
                    //Bacuse of this condition, players can not use Spade cards in the first turn of the first round.
                    handCards[i].SetClickable(false);
                }
                else
                {
                    handCards[i].SetClickable(true);
                }
            }
            return;
        }

        // Debug.Log("The Playable cards are: ");
        // for(int i = 0; i < playableCards.Count; ++i)
        // {
        //     Debug.Log($"{playableCards[i].GetCardRank()} of {playableCards[i].GetCardSuit()}");
        // }

        for (int i = 0; i < handCards.Count; ++i)
        {   
            if (playableCards.Contains(handCards[i]))
            {
                handCards[i].SetClickable(true);
            }
            else
            {
                handCards[i].SetClickable(false);
            }
        }
    }

    public void OponentTurn()
    {   
        if(!handCards.Any())
        {
            Debug.Log("Round Over.");
            //Need to start new round.
            GameObject.Find("GameManager").GetComponent<GameManager>().ShowScoreBoard();
            //return;
        }
        else
        {
            MakeDescision();
        }
        GameObject.Find("GameManager").GetComponent<GameManager>().NextTurn();
    }

    public static int GetRankValue(Rank rank)
    {
        return (int)rank;
    }

    private void MakeDescision()
    {   
        Debug.Log("MakeDescision() has run.");

        this.tableCards = cardsOnTable.GetCardsOnTable();

        // If player has the first turn of the current round.
        if(tableCards.Count == 0)
        {   
            Debug.Log($"{this.gameObject.name} has first turn for round: {GameManager.GetCurrentRound()}.");
            
            List<CardScript> bestCardsOwned = GetBestCards(this.handCards);

            // If player does not have any best cards, place the the lowest ranked card among most common suits owned by the player.
            if(bestCardsOwned == null)
            {   
                Debug.Log($"{this.gameObject.name} does not have any top cards.");

                //Currently, if there are multiple suits that are most common, players will place which ever is first list. 
                CardScript cardToPlay = GetLowestRankOfMostCommonSuit();
                RemoveCardFromHand(cardToPlay);
            }
            // If players has one or more best cards.
            else
            {   
                Debug.Log($"{this.gameObject.name}'s top cards are: ");
                PrintCardList(bestCardsOwned);

                CardScript cardToPlay = WhichCardToPlay(bestCardsOwned);

                // If player has a best card that has a low change of getting trumped (i.e. winning the current round).
                if(cardToPlay != null)
                {
                    Debug.Log($"Least risky card to play is: {cardToPlay.GetCardRank()} of {cardToPlay.GetCardSuit()}");
                    
                    RemoveCardFromHand(cardToPlay);
                }
                // If all of players best cards have high chances of getting trumped (i.e. losing the current round). 
                else
                {   
                    //Play lowest ranked card from the most common suit in this player hand.
                    Debug.Log($"{this.gameObject.name}'s top cards are too risky to play");
                    
                    cardToPlay = GetLowestRankOfMostCommonSuit();
                    
                    Debug.Log($"Instead playing: {cardToPlay.GetCardRank()} of {cardToPlay.GetCardSuit()}");

                    RemoveCardFromHand(cardToPlay);
                }
            }
        }
        // If player does not have the first turn of the current round.
        else
        {   
            Debug.Log($"{this.gameObject.name} has turn: {GameManager.GetCurrentTurn()} for round: {GameManager.GetCurrentRound()}.");
            
            List<CardScript> requiredSuitCardsOnHand = GetRequiredSuitCards();
            
            // If player does not have any cards of the required suit.
            if(requiredSuitCardsOnHand.Count == 0)
            {
                Debug.Log($"{this.gameObject.name} does not have any {this.requiredSuit} cards.");
                
                List<CardScript> spadeCardsOnHand = GetAvailableSpades();

                // If player does not have any Spade cards.
                if(spadeCardsOnHand.Count == 0)
                {
                    Debug.Log($"{this.gameObject.name} does not have any Spade cards");

                    // Player will instead place lowest ranked card amoung the most common suit(s).
                    CardScript cardToPlay = GetLowestRankOfMostCommonSuit();

                    Debug.Log($"Instead playing: {cardToPlay.GetCardRank()} of {cardToPlay.GetCardSuit()}");

                    RemoveCardFromHand(cardToPlay); 
                }
                // If player has any Spade card(s).
                else
                {
                    // If current hand has already been Trumped.
                    if(CheckIfAlreadyTrumped())
                    {
                        PlaceAppropriateSpadeCard(spadeCardsOnHand);
                    }
                    // If previous player(s) have not yet placed any Trump cards.
                    else
                    {
                        PlaceLowestRankedSpadeCard(spadeCardsOnHand);
                    }
                }
            }
            // If player has card(s) of the required suit. 
            else
            {
                this.highestRankOnTable = GetHighestRankOnTable();

                List<CardScript> sutableRankCards = GetSutableRankCards(requiredSuitCardsOnHand);

                // Checks if player can win current round with available list of sutableRankCards.
                if(CheckIfWinable(sutableRankCards))
                {   
                    // If player has the last turn of the current round.
                    if(GameManager.GetCurrentTurn() == 3)
                    {
                        Debug.Log("This is the last turn of this round, so there is no risk factor.");
                        Debug.Log($"{this.gameObject.name} can place the lowest ranked of sutable cards without losing.");
                        Debug.Log($"So {this.gameObject.name} is playing: " + GiveCardInfo(sutableRankCards[0]));

                        RemoveCardFromHand(sutableRankCards[0]);

                        return;
                    }

                    List<CardScript> bestAmongSutableCards = GetBestCards(sutableRankCards);

                    // If there are no best cards amoung the player's sutableRankCards.
                    if(bestAmongSutableCards == null)
                    {
                        Debug.Log($"{this.gameObject.name} does not have any top sutable cards.");
                        Debug.Log($"So {this.gameObject.name} is playing: " + GiveCardInfo(sutableRankCards[0]));

                        RemoveCardFromHand(sutableRankCards[0]); //----if player has an ace and others are unlikely to use trump cards, then he should play that card instead of playing the lowest rank of the sutableRankCards.
                    }
                    // If there are best cards amoung the player's sutableRankCards.
                    else
                    {
                        Debug.Log($"{this.gameObject.name}'s top sutable cards are: ");
                        PrintCardList(bestAmongSutableCards);

                        CardScript cardToPlay = WhichCardToPlay(bestAmongSutableCards);

                        // If one of the bestAmongSutableCards cards has low chance of getting trumpped. 
                        if(cardToPlay != null)
                        {
                            Debug.Log($"Least risky sutable card to play is: " + GiveCardInfo(cardToPlay));
                            
                            RemoveCardFromHand(cardToPlay);
                        }
                        // If all of the bestAmongSutableCards cards have high chance of getting trumpped. 
                        else
                        {   
                            Debug.Log($"{this.gameObject.name}'s top sutable cards are too risky to play");
                            Debug.Log($"So {this.gameObject.name} is playing: " + GiveCardInfo(sutableRankCards[0]));

                            RemoveCardFromHand(sutableRankCards[0]); 
                        }
                    }
                }
                // If player can not win current round with thier cards in the sutableRankCards list.
                else
                {
                    Debug.Log($"{this.gameObject.name} does not have required cards to win ths round");
                    Debug.Log($"So {this.gameObject.name} is playing: " + GiveCardInfo(sutableRankCards[0]));

                    RemoveCardFromHand(sutableRankCards[0]); 
                }

                
            }
        }
    }

    // Sets the required suit for the current round. And returns the list of cards of the required suit owned by the player.
    private List<CardScript> GetRequiredSuitCards()
    {   
        // Used for error handeling. 
        if (tableCards == null || tableCards.Count == 0)
        {
            Debug.LogWarning("GetRequiredSuitCards: tableCards is empty. Returning an empty list.");
            return new List<CardScript>(); // Return an empty list if there are no table cards
        }

        this.requiredSuit = tableCards[0].GetCardSuit();

        Debug.Log($"Required Suit is: {this.requiredSuit}");

        List<CardScript> cardsOfRequiredSuit = new List<CardScript>();

        for(int i = 0; i < handCards.Count; ++i)
        {   
            Suit currentCardSuit = handCards[i].GetCardSuit();
            
            if(currentCardSuit == requiredSuit)
            {
                cardsOfRequiredSuit.Add(handCards[i]);
            }
        }

        return cardsOfRequiredSuit;
    }

    // Returns the highest ranked card on the table. 
    private Rank GetHighestRankOnTable()
    {
        int highestValue = 0;
        Rank highestRank = Rank.Two;

        for(int i = 0; i < tableCards.Count; ++i)
        {   
            Suit currentCardSuit = tableCards[i].GetCardSuit();

            if(currentCardSuit == requiredSuit)
            {   
                int currentCardValue = GetRankValue(tableCards[i].GetCardRank());

                if(currentCardValue > highestValue)
                {
                    highestValue = currentCardValue;
                    highestRank = tableCards[i].GetCardRank();
                }
            }
        }

        return highestRank;
    }

    // Returns list of cards owned by player that are higher ranked than the current highest ranked card on the table. 
    // If player does not have any higher ranked cards, then ir returns all of the given availableSuitCards.
    private List<CardScript> GetSutableRankCards(List<CardScript> availableSuitCards)
    {   
        Debug.Log("GetSutableRankCards() function has run.");

        List<CardScript> sutableRankCards = new List<CardScript>();

        for(int i = 0; i < availableSuitCards.Count; ++i)
        {   
            int currentCardValue = GetRankValue(availableSuitCards[i].GetCardRank());

            if(currentCardValue > GetRankValue(highestRankOnTable))
            {
                sutableRankCards.Add(availableSuitCards[i]);
            }
        }

        if(sutableRankCards.Count != 0)
        {
            return sutableRankCards;
        }
        else
        {
            return availableSuitCards;
        }
        
    } 

    // Returns the list of Spade cards owned by the player. 
    private List<CardScript> GetAvailableSpades()
    {
        List<CardScript> availableSpadeCards = new List<CardScript>();

        for(int i = 0; i < handCards.Count; ++i)
        {   
            Suit currentCardSuit = handCards[i].GetCardSuit();
            
            if(currentCardSuit == Suit.Spades)
            {
                availableSpadeCards.Add(handCards[i]);
            }
        }

        return availableSpadeCards;
    }


    // Makes the player place the lowest ranked Spade card owned by them.
    private void PlaceLowestRankedSpadeCard(List<CardScript> availableSpades)
    {
        int lowestValue = 15;
        CardScript worstSpadeCard = null;
        
        // Finding the lowest ranked spade card.
        for(int i = 0; i < availableSpades.Count; ++i)
        {   
            int currentCardValue = GetRankValue(availableSpades[i].GetCardRank());

            if(currentCardValue < lowestValue)
            {
                lowestValue = currentCardValue;
                worstSpadeCard = availableSpades[i];
            }
        }

        Debug.Log($"{this.gameObject.name} placing: {worstSpadeCard.GetCardRank()} of {worstSpadeCard.GetCardSuit()}");

        RemoveCardFromHand(worstSpadeCard);
    }

    // Checks if player can win the current round with thier available sutableCardsList. 
    private bool CheckIfWinable(List<CardScript> sutableCardsList)
    {   
        this.requiredSuit = tableCards[0].GetCardSuit();

        if(requiredSuit != Suit.Spades)  // Skips of required suit is not Spade.
        {
            this.tableCards = cardsOnTable.GetCardsOnTable();

            // This checks if current hand/round has already been trumped by another player.
            for(int i = 0; i < tableCards.Count; ++i)
            {
                if(tableCards[i].GetCardSuit() == Suit.Spades)
                {
                    return false;   //If current hand/round has already been trumped then player can not win if they can't use trump cards themselves.
                }
            }
        }
        
        this.highestRankOnTable = GetHighestRankOnTable();

        for(int i = 0; i < sutableCardsList.Count; ++i)
        {   
            Suit currentCardSuit = sutableCardsList[i].GetCardSuit();

            if(currentCardSuit == requiredSuit)
            {   
                int currentCardValue = GetRankValue(sutableCardsList[i].GetCardRank());

                if(currentCardValue > GetRankValue(highestRankOnTable))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Returns a list of the current highest ranked cards of each suit that are in the given cardList parameter.
    // Returns null if the cardList does not have a highest ranked card any suit.
    private List<CardScript> GetBestCards(List<CardScript> cardList)
    {
        List<CardScript> bestRemainingCards = remainingCards.GetBestRemainingCards();

        List<CardScript> bestCardsOwned = new List<CardScript>();

        for(int i = 0; i < bestRemainingCards.Count; ++i)
        {
            if(cardList.Contains(bestRemainingCards[i]))
            {
                bestCardsOwned.Add(bestRemainingCards[i]);
            }
        }

        if(bestCardsOwned.Count == 0)
        {
            return null;
        }
        else
        {
            return bestCardsOwned;
        }
    }

    // Checks if the current round has been Trumped with a Spade card.
    private bool CheckIfAlreadyTrumped()
    {
        for(int i = 0; i < tableCards.Count; ++i)
        {
            if(tableCards[i].GetCardSuit() == Suit.Spades)
            {
                Debug.Log("Already Trumped.");
                return true;
            }
        }

        Debug.Log("Not Trumped.");
        return false;
    }

    // Makes the player place the appropriate Spade card based on the current Spade cards placed on the table.
    private void PlaceAppropriateSpadeCard(List<CardScript> availableSpades)
    {
        Debug.Log("PlaceAppropriateSpadeCard() has run.");
        
        List<CardScript> bestRemainingCards = remainingCards.GetBestRemainingCards();

        // If player has the best remaining Spade card, then they will place that card.
        for(int i = 0; i < bestRemainingCards.Count; ++i)
        {
            if(availableSpades.Contains(bestRemainingCards[i]))
            {
                RemoveCardFromHand(bestRemainingCards[i]);
                return;
            }
        }

        CardScript bestSpadeOnTable = null;
        int highestRank = 0;

        // Finding the highest ranked Spade card currently on the table.
        for(int i = 0; i < tableCards.Count; ++i)
        {
            if(tableCards[i].GetCardSuit() == Suit.Spades && GetRankValue(tableCards[i].GetCardRank()) > highestRank)
            {
                highestRank = GetRankValue(tableCards[i].GetCardRank());
                bestSpadeOnTable = tableCards[i]; 
            }
        }

        // Player will place their lowest ranked Spade card that is higher ranked than the current highest ranked Spade card on the table.
        for(int i = 0; i < availableSpades.Count; ++i)
        {
            if(GetRankValue(availableSpades[i].GetCardRank()) > GetRankValue(bestSpadeOnTable.GetCardRank()))
            {
                RemoveCardFromHand(availableSpades[i]);
                return;
            }
        }

        // If player does not have a higher ranked Spade card than the current higest ranked Spade card on the table, then they will place the lowest ranked Spade card owned by them.
        PlaceLowestRankedSpadeCard(availableSpades);
    } 

    // Returns the chance of getting trumped for a given best card owned by the player.
    private float CheckChanceOfGettingTrumped(CardScript bestCard)
    {
        int currentRound = GameManager.GetCurrentRound();
        int currentTurn = GameManager.GetCurrentTurn();

        int suitCardsOwned = 0;  //number of cards that are the same suit as this bestCard, that are owned by this player. 
        for(int i = 0; i < handCards.Count; ++i)
        {
            if(bestCard.GetCardSuit() == handCards[i].GetCardSuit())
            {
                ++suitCardsOwned;
            }
        }

        int unplayedSuitCards = remainingCards.RemainingCardsOfSuit(bestCard.GetCardSuit());
        int suitCardsOwnedByOthers = unplayedSuitCards - suitCardsOwned;

        int spadeCardsOwned = 0;  //number of spade cards owned by this player.
        for(int i = 0; i < handCards.Count; ++i)
        {
            if(handCards[i].GetCardSuit() == Suit.Spades)
            {
                ++spadeCardsOwned;
            }
        }

        int unplayedSpadeCards = remainingCards.RemainingCardsOfSuit(Suit.Spades);
        int spadeCardsOwnedByOthers = unplayedSpadeCards - spadeCardsOwned;

        float chanceOfGettingTrumped = 0;

        if(currentRound > 5)
        {   
            Debug.Log($"Current round is: {currentRound}, trump chance: +1");
            ++chanceOfGettingTrumped;
        }
        else
        {   
            Debug.Log($"Current round is: {currentRound}, trump chance: -1");
            --chanceOfGettingTrumped;
        }

        if(currentTurn < 2)
        {   
            Debug.Log($"Current turn is: {currentTurn}, trump chance: +1");
            ++chanceOfGettingTrumped;
        }
        else
        {   
            Debug.Log($"Current turn is: {currentTurn}, trump chance: -1");
            --chanceOfGettingTrumped;
        }

        if(suitCardsOwnedByOthers <= 5 )
        {   
            Debug.Log($"Suites owned by others: {suitCardsOwnedByOthers}, trump chance: +1");
            ++chanceOfGettingTrumped;
        }
        else
        {   
            Debug.Log($"Suites owned by others: {suitCardsOwnedByOthers}, trump chance: -1");
            --chanceOfGettingTrumped;
        }

        if(spadeCardsOwnedByOthers > 5)
        {   
            Debug.Log($"Spades owned by others: {spadeCardsOwnedByOthers}, trump chance: +1");
            ++chanceOfGettingTrumped;
        }
        else
        {   
            Debug.Log($"Spades owned by others: {spadeCardsOwnedByOthers}, trump chance: -1");
            --chanceOfGettingTrumped;
        }

        if(spadeCardsOwnedByOthers == 0)  //If there are no more Spade cards left player can not be trumpped
        {
            chanceOfGettingTrumped -= 10;
        }

        Debug.Log(GiveCardInfo(bestCard) + $" trump chance is: {chanceOfGettingTrumped}");

        return chanceOfGettingTrumped;
    }

    // Returns a single card among the given listOfBestCards, that has the best change of winning the current round.
    private CardScript WhichCardToPlay(List<CardScript> listOfBestCards)
    {   
        List<CardScript> cardList = new List<CardScript>();

        // This condition prevents opponenets from using Spade cards in the first turn of the first round.
        if(GameManager.GetCurrentRound() == 0)
        {   
            for(int i = 0; i < listOfBestCards.Count; ++i)
            {
                if(listOfBestCards[i].GetCardSuit() != Suit.Spades)
                {
                    cardList.Add(listOfBestCards[i]);
                }
                else
                {
                    Debug.Log("Can not use spade cards in first round");
                }
            }

            if(cardList.Count == 0)
            {   
                //If card list becomes empty after removing a top spade card.
                return null;
            }

        }
        else
        {
            cardList = listOfBestCards;
        }
        
        List<float> posibilityOfLosing = new List<float>();
        
        for(int i = 0; i < cardList.Count; ++i)
        {
            //Spade cards can not be trumpped, so player is guarenteed to win if they have the current top most Spade Card.
            if(cardList[i].GetCardSuit() == Suit.Spades)  
            {   
                return cardList[i];
            }
            
            posibilityOfLosing.Add(CheckChanceOfGettingTrumped(cardList[i]));
        }

        float minPossibility = posibilityOfLosing.Min();


        //Players take more risk when round number is low
        if(GameManager.GetCurrentRound() < 6)
        {   
            Debug.Log($"Round number:{GameManager.GetCurrentRound()} is low, taking more risk.");

            if(minPossibility > 2)
            {
                return null;
            }
            else
            {
                int index = posibilityOfLosing.IndexOf(minPossibility);
                return cardList[index];   
            }
        }
        else  //Players take less risk when round number is high.
        {   
            Debug.Log($"Round number:{GameManager.GetCurrentRound()} is high, taking less risk.");

            if(minPossibility > 0)
            {
                return null;
            }
            else
            {
                int index = posibilityOfLosing.IndexOf(minPossibility);
                return cardList[index];   
            }
        }
    }

    // Returns the lowest ranked card among the most common suits that is owned by the player.
    // If there are multiple suits that are most common, players will place the lowest ranked cards among the most common suits.
    private CardScript GetLowestRankOfMostCommonSuit()
    {
        Debug.Log("GetLowestRankOfMostCommonSuit() has run.");
        
        List<CardScript> largestList = suitCards[Suit.Hearts];

        foreach(Suit suitGroup in (Suit[])System.Enum.GetValues(typeof(Suit)))
        {   
            // Ignores the list of Spade cards owned by the player.
            if(suitGroup == Suit.Spades)
            {
                continue;
            }
            else
            {
                if(suitCards[suitGroup].Count > largestList.Count)
                {
                    largestList = suitCards[suitGroup];
                }
            }
        }
        //Even if spade cards are the most common, player should try and hold on to them for later trump chances.
        //If there are no other suit cards left use the spade cards. 
        if(largestList.Count == 0)
        {
            Debug.Log($"No other suits left, so playing Spades");
            largestList = suitCards[Suit.Spades];
        }

        CardScript cardToGive = largestList[0];

        //The following code lines are useful when there are multiple suits that are most common.
        //These make sure that among the most common suit cards, the card with the lest rank is returned.
        foreach(Suit suitGroup in (Suit[])System.Enum.GetValues(typeof(Suit)))
        {   
            // Ignores the list of Spade cards owned by the player.
            if(suitGroup == Suit.Spades)
            {
                continue;
            }
            else
            {
                if(largestList.Count == suitCards[suitGroup].Count)
                {
                    List<CardScript> currentSuitList = suitCards[suitGroup];
                    if(GetRankValue(cardToGive.GetCardRank()) > GetRankValue(currentSuitList[0].GetCardRank()))
                    {
                        Debug.Log($"{this.gameObject.name}'s {suitGroup} cards has a lower ranked card");
                        cardToGive = currentSuitList[0];
                    }
                }
            }
        }

        Debug.Log($"{cardToGive.GetCardRank()} of {cardToGive.GetCardSuit()} card is the lowest ranked among most common suits");
        return cardToGive;
    }

    public static void PrintCardList(List<CardScript> cardList)
    {   
        /*************************************************************************************
        -> This function is meant for Debigging purposes.
        **************************************************************************************/

        for(int i = 0; i < cardList.Count; ++i)
        {
            Debug.Log(GiveCardInfo(cardList[i]));
        }
    }
    
    public static string GiveCardInfo(CardScript card)
    {   
        /*************************************************************************************
        -> This function is meant for Debigging purposes.
        **************************************************************************************/

        string cardInfo = card.GetCardRank()+ " of " + card.GetCardSuit();

        return cardInfo;
    }

}
