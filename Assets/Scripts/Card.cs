using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Suit { Spades, Hearts, Clubs, Diamonds };
public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace };

[System.Serializable]
public class Card
{
    public Suit suit;
    public Rank rank;
    public Sprite cardImage;

    public Suit GetCardSuit()
    {
        return this.suit;
    }

    public Rank GetCardRank()
    {
        return this.rank;
    }

}
