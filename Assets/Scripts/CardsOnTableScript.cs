using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class CardsOnTableScript : MonoBehaviour
{
    [SerializeField]
    private List<CardScript> tableCards = new List<CardScript>();

    [SerializeField]
    private AudioSource audioSource; // Drag AudioSource in Inspector

    [SerializeField]
    private AudioClip cardPlaceSound; // Drag card placement sound here

    [SerializeField]
    Suit requiredSuit;
    [SerializeField]
    Rank highestRankOnTable;

    [SerializeField]
    TextMeshProUGUI gameUpdateText;

    [SerializeField]
    GameManager gameManager;


    //Adds a selected card to the tableCards list.
    public void AddCardToTable(CardScript Selectedcard)
    {   
        Debug.Log("AddCardToTable() function has run.");
        tableCards.Add(Selectedcard);
    }


    //Places the selected card on this players card placement location. 
    public void PlaceCardOnTable(CardScript selectedCard, Transform tableLocation)
    {   
        Debug.Log($"Placed {selectedCard.GetCardRank()} of {selectedCard.GetCardSuit()} on the table");

        GameObject cardToPlace =  Instantiate(selectedCard.gameObject, selectedCard.transform.position, quaternion.identity);

        cardToPlace.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

        cardToPlace.transform.DOMove(tableLocation.position, 0.75f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => {   
            });

        if (audioSource != null && cardPlaceSound != null)
        {
            audioSource.PlayOneShot(cardPlaceSound);
        } 

        CardScript placedCardScript = cardToPlace.GetComponent<CardScript>();
        if (placedCardScript != null)
        {
            AddCardToTable(placedCardScript);
        }
        else
        {
            Debug.LogError("Instantiated card does not have a CardScript component.");
        }           
    }

    private void GiveCardsToWinningPlayer(CardScript bestCard, PlayerScript winningPlayer)
    {
        MoveTowardsWinningCard(bestCard, winningPlayer);
    }

    private void MoveCardsToPlayer(PlayerScript playerToGive)
    {
        foreach(CardScript card in tableCards)
        {
            card.gameObject.transform.DOMove(playerToGive.gameObject.transform.position, 0.75f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => {
                ClearTable();   
            });
        }
    }

    private void MoveTowardsWinningCard(CardScript cardToMoveTowards, PlayerScript player)
    {   
        cardToMoveTowards.GetComponent<SpriteRenderer>().sortingOrder = 30;

        foreach(CardScript card in tableCards)
        {
            card.gameObject.transform.DOMove(cardToMoveTowards.gameObject.transform.position, 0.75f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => {
                MoveCardsToPlayer(player);
            });
        }
    }

    

    //Removed all cards from the table and clears the tableCard list.
    public void ClearTable()
    {
        Debug.Log("ClearTable() function was executed.");

        for(int i = 0; i < tableCards.Count; ++i)
        {
            Destroy(tableCards[i].gameObject);
        }

        tableCards.Clear();

    }

    public List<CardScript> GetCardsOnTable()
    {
        return tableCards;
    }


    public void CheckWhoWon()
    {   
        /*********************************************************************************
        -> Runs an Algorithm to check which player won the current round.
        -> Notifies the player on which player won.
        -> Clears all cards from the table. 
        -> Calls function to start a new round.
        **********************************************************************************/
        requiredSuit = tableCards[0].GetCardSuit();

        CardScript bestCard = tableCards[0];
        
        if(requiredSuit == Suit.Spades)
        {
            bestCard = GetBestCard(tableCards);
        }
        else
        {
            List<CardScript>spadeCardsOnTable = GetSpadeCardsOnTable();

            if(spadeCardsOnTable.Count == 0)
            {
                bestCard = GetBestCard(tableCards);
            }
            else
            {
                List<CardScript> listOfSpadeCards = GetSpadeCardsOnTable();
                
                bestCard = GetBestTrumpCard(listOfSpadeCards);
            }
        }

        gameUpdateText.text = $"{bestCard.GetCardOwnerName()} has won this round.";
        PlayerScript playerWhoWon = GameObject.Find(bestCard.GetCardOwnerName()).GetComponent<PlayerScript>();

        playerWhoWon.UpdatePlayerScore(1); //Increases player score by 1.

        gameManager.UpdatePlayerTurn(playerWhoWon);

        
        GiveCardsToWinningPlayer(bestCard, playerWhoWon);
        //ClearTable();

        gameManager.UpdateRoundsRemaining();
    }


    private CardScript GetBestCard(List<CardScript> listOfCards)
    {
        requiredSuit = tableCards[0].GetCardSuit();
        
        int highestValue = 0;
        CardScript bestCard = null;

        for(int i = 0; i < listOfCards.Count; ++i)
        {   
            Suit currentCardSuit = listOfCards[i].GetCardSuit();

            if(currentCardSuit == requiredSuit)
            {   
                int currentCardValue = PlayerScript.GetRankValue(listOfCards[i].GetCardRank());

                if(currentCardValue > highestValue)
                {
                    highestValue = currentCardValue;
                    this.highestRankOnTable = listOfCards[i].GetCardRank();
                    bestCard = listOfCards[i];
                }
            }
        }

        if (bestCard == null)
        {
            Debug.LogError("No best card found in the provided list.");
        }

        return bestCard;  
    }

    private List<CardScript> GetSpadeCardsOnTable()
    {
        requiredSuit = tableCards[0].GetCardSuit();

        List<CardScript> spadeCards = new List<CardScript>();

        for(int i = 0; i < tableCards.Count; ++i)
        {
            if(tableCards[i].GetCardSuit() == Suit.Spades)
            {
                spadeCards.Add(tableCards[i]);
            }
        }

        Debug.Log($"Found {spadeCards.Count} spade cards on the table.");

        return spadeCards;
    }


    private CardScript GetBestTrumpCard(List<CardScript> listOfSpadeCards)
    {   
        int highestValue = 0;
        CardScript bestCard = null;

        for(int i = 0; i < listOfSpadeCards.Count; ++i)
        {  
            int currentCardValue = PlayerScript.GetRankValue(listOfSpadeCards[i].GetCardRank());

            if(currentCardValue > highestValue)
            {
                highestValue = currentCardValue;
                this.highestRankOnTable = listOfSpadeCards[i].GetCardRank();
                bestCard = listOfSpadeCards[i];
            }
        }

        if (bestCard == null)
        {
            Debug.LogError("No best card found in the Trump Card list.");
        }

        return bestCard;        
    }

}
