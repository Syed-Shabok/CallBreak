using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{   
    [SerializeField]
    //private CardDeck deck; //Referance to the CardDeck ScriptableObject.
    private DeckScript deck;

    [SerializeField]
    private CardsOnTableScript cardsOnTable;   //Referance to the CardsOnTable Scriptable Object. 

    [SerializeField]
    private ScoreBoard scoreRecord;  //Referance to the ScoreBoard Scriptable Object.

    [SerializeField]
    private RemainingCards remainingCards;  //Referance to the Remaining Cards Script.
    
    [SerializeField]
    //private PlayerScript mainPlayer;
    private List<PlayerScript> playerList = new List<PlayerScript>();  //List of all Players.

    [SerializeField]
    private int playerTurn = 0;    //Index of current player's turn.
    private static int turnsTaken = 0;   //This keeps track of the number of turns taken in current turn.
    private static int roundsRemaining = 13; // keeps track of the number of rounds remaining.
    private int leadNumber = 0;  //Keeps track of the leads being played

    [SerializeField]
    private TextMeshProUGUI GameUpdateText;
    [SerializeField]
    private List<TextMeshProUGUI> playerScoreTexts = new List<TextMeshProUGUI>();
    [SerializeField]
    private GameObject playerPridictionMenu;
    [SerializeField]
    private GameObject scoreBoard;
    

    //Variables used for Desicion Making:
    private List<CardScript> tableCards = new List<CardScript>();
    private Suit requiredSuit;
    private Rank highestRankOnTable;  
    
    // Start is called before the first frame update
    void Start()
    {   
        //Debug.Log($"PlayerPref : Lead number = {PlayerPrefs.GetInt("CurrentLeadNumber")}.");
        StartGame();
    }

    private void StartGame()
    {   
        leadNumber = PlayerPrefs.GetInt("CurrentLeadNumber");
        roundsRemaining = 13;

        Debug.Log($"Starting Lead no: {leadNumber}, round no: {GetCurrentRound()}");

        //Deck of card is created
        deck.InitializeDeck();
        // Deck gets shuffled
        deck.ShuffleDeck();

        int r = UnityEngine.Random.Range(0,3);

        if(r == 0)
        {
            deck.DistribuiteRandomly(playerList);
        }
        else if(r == 1)
        {
            deck.DistributeFairlyByRank(playerList);
        }
        else
        {
            deck.DistributeFairlyBySuit(playerList);
        }

        SelectRandomStartingPlayer();

        // StartPlayerTurn();
    }


    //Pulls out cards from the CardDeck ScriptableObject and gives it a player.
    public void DealCards(List<CardScript> hand)
    {
        Debug.Log("DealCards() Function has run");
        
        List<Card> organizedListOfCards = GetOrganizedCardList(hand.Count);
        
        for(int i = 0; i < hand.Count; ++i)
        {
            hand[i].SetCard(organizedListOfCards[i]);
        }
    }

        
    private List<Card> GetOrganizedCardList(int size)
    {
        List<Card> unorganizedCards = new List<Card>();

        for(int i = 0; i < size; ++i)
        {
            unorganizedCards.Add(deck.GetCard());
        } 

        List<Card> spadesCardList = new List<Card>();
        List<Card> heartsCardList = new List<Card>();
        List<Card> clubsCardList = new List<Card>();
        List<Card> diamondsCardList = new List<Card>();

        for(int i = 0; i < unorganizedCards.Count; ++i)
        {
            if(unorganizedCards[i].GetCardSuit() == Suit.Spades)
            {
                spadesCardList.Add(unorganizedCards[i]);
            }
            else if(unorganizedCards[i].GetCardSuit() == Suit.Hearts)
            {
                heartsCardList.Add(unorganizedCards[i]);
            }
            else if(unorganizedCards[i].GetCardSuit() == Suit.Clubs)
            {
                clubsCardList.Add(unorganizedCards[i]);
            }
            else            
            {
                diamondsCardList.Add(unorganizedCards[i]);
            }
        }

        SortByCardRank(spadesCardList);
        SortByCardRank(heartsCardList);
        SortByCardRank(clubsCardList);
        SortByCardRank(diamondsCardList);

        List<Card> organizedList = new List<Card>(); 

        for(int i = 0; i <  spadesCardList.Count; ++i)
        {
            organizedList.Add(spadesCardList[i]);
        }
        for(int i = 0; i < heartsCardList.Count; ++i)
        {
            organizedList.Add(heartsCardList[i]);
        }
        for(int i = 0; i < clubsCardList.Count; ++i)
        {
            organizedList.Add(clubsCardList[i]);
        }
        for(int i = 0; i < diamondsCardList.Count; ++i)
        {
            organizedList.Add(diamondsCardList[i]);
        }

        return organizedList;
    }
    
    private void SortByCardRank(List<Card> cardList)
    {   
        cardList.Sort((cardA, cardB) =>
        {
            int rankA = PlayerScript.GetRankValue(cardA.GetCardRank());
            int rankB = PlayerScript.GetRankValue(cardB.GetCardRank());
            return rankA.CompareTo(rankB); // Use .CompareTo for ascending order
        });
    }

    public void SetPlayerPridiction(UnityEngine.UI.Button button)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        
        int prediction;
        if (int.TryParse(buttonText.text, out prediction))
        {
            playerList[0].SetPlayerPridiction(prediction);
            Debug.Log($"Player Prediction Set: {prediction}");
        }

        for(int i = 1; i < playerList.Count; ++i)
        {
            playerList[i].MakePridictionOfWins();
        }

        playerPridictionMenu.SetActive(false);

        StartPlayerTurn();
    }

    private void SelectRandomStartingPlayer()
    {
        /*********************************************************************************
        -> Sets a random player to play first at the start of the game.

        - Generates a random integer between 0 and 3 and sets it as as the playerTurn.
        **********************************************************************************/
        
        playerTurn = UnityEngine.Random.Range(0, playerList.Count);

    }

    private void StartPlayerTurn()
    {   
        /*********************************************************************************
        -> Makes a player play thier turn.
        -> Keeps track of the turnsTaken variable and resets it at the end of each round 
           (i.e. when turnsTaken = 4).
        -> Calls function to find winner of each round at the end of the round.    
        -> Notifies the player on whos turn it is currently.
        -> Allows the Main-Player to give input when it is thier turn.
        -> Makes AI Oponents place a card when it is thier turn.
        **********************************************************************************/
        
        if(turnsTaken == 4)
        {
            turnsTaken = 0;
            StartCoroutine(DeclareWinnerAfterDelay(0.75f));
        }
        else
        {   
            if(playerList[playerTurn].gameObject.name == "Main-Player")
            {   
                GameUpdateText.text = "Your turn.";
                playerList[playerTurn].MainPlayerTurn();
            }
            else
            {   
                string playerName = playerList[playerTurn].gameObject.name;
                GameUpdateText.text = $"{playerName}'s turn."; 
                StartCoroutine(ExecuteOponentTurnAfterDelay(2.0f));
            }

            ++turnsTaken;
        }
    }

    private IEnumerator ExecuteOponentTurnAfterDelay(float delay)
    {   
        /*********************************************************************************
        -> Makes the AI oponents take thier turn after a some delay.
        **********************************************************************************/

        yield return new WaitForSeconds(delay);
        
        playerList[playerTurn].OponentTurn();

    }

    public void UpdatePlayerTurn(PlayerScript playerWhoWon)
    {
        playerTurn = playerList.IndexOf(playerWhoWon);
        Debug.Log($"Next Player index = {playerTurn}");
    }

    public void UpdateRoundsRemaining()
    {
        --roundsRemaining;

        if(roundsRemaining == 0)
        {
            StartCoroutine(ShowScoreBoard(2.0f));
        }
        else
        {
            StartCoroutine(StartNewRoundAfterDelay(3.0f));
        }
    }

    public void NextTurn()
    {   
        /*********************************************************************************
        -> Updates the playerTurn variable by looping through the playerList.
        -> Makes the next player take thier turn.

        - Called in the PlayerScript after an AI Oponent takes thier turn; (OponentTurn()).
        - Called in the CardScript after after Main Player takes their turn; (OnMouseDown()).
        **********************************************************************************/

        playerTurn = (playerTurn + 1) % playerList.Count;

        StartCoroutine(StartNewRoundAfterDelay(0.5f));
    }

    private IEnumerator DeclareWinnerAfterDelay(float delay)
    {   
        /*********************************************************************************
        -> Checks and decalres the winniner of a round after a certain delay.
        **********************************************************************************/

        yield return new WaitForSeconds(delay);
        //CheckWhoWon();
        cardsOnTable.CheckWhoWon();
        
    }

    private IEnumerator StartNewRoundAfterDelay(float delay)
    {   
        /*********************************************************************************
        -> Starts a new round by making players take a new turn, after some delay. 
        **********************************************************************************/

        yield return new WaitForSeconds(delay);
        StartPlayerTurn();
    }   

    public void ShowScoreBoard()
    {
        StartCoroutine(ShowScoreBoard(2.0f));
    }

    private IEnumerator ShowScoreBoard(float delay)
    {   
        bool gameFinished = false;

        for(int i = 0; i < playerList.Count; ++i)
        {
            playerList[i].SetCurrentLeadPoints(leadNumber, playerList[i].gameObject.name);
        }

        if(leadNumber == 4)
        {
          for(int i = 0; i < playerList.Count; ++i)
            {
                playerList[i].CalculateTotalPoints();
            }

            gameFinished = true;  
        }

        // ++leadNumber;
        // PlayerPrefs.SetInt("CurrentLeadNumber", leadNumber);

        yield return new WaitForSeconds(delay);
        
        if(!gameFinished)
        {
            scoreBoard.SetActive(true);
        }
        else
        {   
            scoreBoard.SetActive(true);

            Transform continueButton = scoreBoard.transform.Find("Continue Button");
            Transform exitButton = scoreBoard.transform.Find("Exit Button");
            Transform playAgainButton = scoreBoard.transform.Find("Play Again Button");

            continueButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(true);
            playAgainButton.gameObject.SetActive(true);

            CheckWinner();
        }
    }

    private void CheckWinner()
    {
        int winningIndex = 0;
        float highestScore = -100;

        for(int i = 0; i < playerList.Count; ++i)
        {   
            float playerTotal = playerList[i].CalculateTotalPoints();

            if(highestScore < playerTotal)
            {
                highestScore = playerTotal;
                winningIndex = i;
            }
        }

        GameUpdateText.text = $"{playerList[winningIndex].gameObject.name} has won the game.";
    }

    public void ContinuePlayingButton()
    {
        scoreBoard.SetActive(false);
        
        ++leadNumber;
        PlayerPrefs.SetInt("CurrentLeadNumber", leadNumber);

        ResetScene();
    }


    private void ResetScene()
    {
        // Reload the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public static int GetCurrentTurn()
    {
        int playerTurn = turnsTaken - 1;
        return playerTurn;
    }
    
    public static int GetCurrentRound()
    {
        int currentRound = 13 - roundsRemaining;

        return currentRound; 
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application is quitting!");

        // Reset PlayerPrefs or save data
        ClearAllRecords();
    }

    private void ClearAllRecords()
    {   
        Debug.Log("ClearAllRecords() Function has run.");

        PlayerPrefs.SetInt("CurrentLeadNumber", 0);
        PlayerPrefs.Save();

        
        cardsOnTable.ClearTable();
        scoreRecord.ClearRecord();
    }

    public void PlayAgainButton()
    {
        StartCoroutine(PlayAgainSequence());
    }

    private IEnumerator PlayAgainSequence()
    {
        ClearAllRecords(); // Reset data
        yield return null; // Ensure the reset operation is completed
        ResetScene(); // Reload the scene
    }
}
