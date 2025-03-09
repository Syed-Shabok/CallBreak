using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems; // Required for Pointer Events


[System.Serializable]
[RequireComponent(typeof(SpriteRenderer))]
public class CardScript : MonoBehaviour, IPointerClickHandler
{   
    [SerializeField]
    private Suit cardSuit;
    [SerializeField]
    private Rank cardRank;
    [SerializeField]
    private Sprite cardSprite;
    [SerializeField]
    private bool isClickable = false;
    [SerializeField]
    private bool onTable = false;
    [SerializeField]
    private PlayerScript cardOwner; //This is a referance to the player who is holding this card.

    private void Awake()
    {
       if (GetComponent<SpriteRenderer>() == null)
       {
            gameObject.AddComponent<SpriteRenderer>();
            Debug.Log("SpriteRenderer component added automatically at runtime.");
       }

       if (GetComponent<BoxCollider2D>() == null)
       {
            gameObject.AddComponent<BoxCollider2D>(); // Ensure collider exists
       }
    }

    //Sets all the values of this card.
    public void SetCard(Card givenCard)
    {
        Debug.Log("SetCard() Function has run");

        cardSuit = givenCard.suit;
        cardRank = givenCard.rank;
        cardSprite = givenCard.cardImage;

        GetComponent<SpriteRenderer>().sprite = cardSprite;
    }

    public CardScript GiveCard(Card givenCard)
    {
        Debug.Log("GiveCard() Function has run");

        cardSuit = givenCard.suit;
        cardRank = givenCard.rank;
        cardSprite = givenCard.cardImage;

        GetComponent<SpriteRenderer>().sprite = cardSprite;

        return this;
    }

    //Sets the owner of this card. 
    public void SetOwnerOfCard(PlayerScript player)
    {
        cardOwner = player;
    }

    public string GetCardOwnerName()
    {
        return cardOwner.gameObject.name;
    }

    //Returns the card suit.
    public Suit GetCardSuit()
    {
        return cardSuit;
    }

    //Returns the card rank.
    public Rank GetCardRank()
    {
        return cardRank;
    }

    public void SetClickable(bool condition)
    {
        isClickable = condition;

        if(!isClickable)
        {
            ChangeColorToGray();
        }
        else
        {
            ChangeColorToWhite();
        }
    }

    private void ChangeColorToGray()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color grayColor = new Color(127f / 255f, 127f / 255f, 127f / 255f);
        spriteRenderer.color = grayColor;
    }

    private void ChangeColorToWhite()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;
    }

    public void SetCardOnTable()
    {
        onTable = true;
    }

   public void OnPointerClick(PointerEventData eventData)
    {
        if (!isClickable)
        {
            Debug.Log("Can not play card");
            return;
        }

        DetectTopCardAndPlay(eventData);
    }

    private void DetectTopCardAndPlay(PointerEventData eventData)
    {
        Vector2 clickPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        Collider2D[] colliders = Physics2D.OverlapPointAll(clickPosition);

        CardScript topCard = null;
        int highestOrder = int.MinValue;

        foreach (Collider2D col in colliders)
        {
            CardScript card = col.GetComponent<CardScript>();
            if (card != null && card.isClickable)
            {
                int sortingOrder = card.GetComponent<SpriteRenderer>().sortingOrder;
                if (sortingOrder > highestOrder)
                {
                    highestOrder = sortingOrder;
                    topCard = card;
                }
            }
        }

        if (topCard == this)
        {
            PlayCard();
        }
    }

    private void PlayCard()
    {
        if (onTable)
        {
            Debug.Log("Clicked on a Table Card.");
            return;
        }

        if (cardOwner != null)
        {
            cardOwner.RemoveCardFromHand(this);
        }

        cardOwner.DisablePlayerCards();
        GameObject.Find("GameManager").GetComponent<GameManager>().NextTurn();
    }

}
