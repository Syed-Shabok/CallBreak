using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "ScoreBoard", menuName = "ScoreBoard", order = 3)]
public class ScoreBoard : ScriptableObject
{   
    [SerializeField]
    private List<float> mainPlayerPoints = new List<float>();
    [SerializeField]
    private List<float> player2Points = new List<float>();
    [SerializeField]
    private List<float> player3Points = new List<float>();
    [SerializeField]
    private List<float> player4Points = new List<float>();


    public void SetPlayerPoints(string playerName, float points)
    {
        if(playerName == "Main-Player")
        {
            mainPlayerPoints.Add(points);
        }
        else if(playerName == "Player 2")
        {
            player2Points.Add(points);
        }
        else if(playerName == "Player 3")
        {
            player3Points.Add(points);
        }
        else
        {
            player4Points.Add(points);
        }
    }

    public float GetPlayerPoints(string playerName, int leadNumber)
    {
        if(playerName == "Main-Player")
        {
            return mainPlayerPoints[leadNumber];
        }
        else if(playerName == "Player 2")
        {
            return player2Points[leadNumber];
        }
        else if(playerName == "Player 3")
        {
            return player3Points[leadNumber];
        }
        else
        {
            return player4Points[leadNumber];
        }
    }

    public void ClearRecord()
    {   
        Debug.Log("ClearRecord() function was executed.");

        mainPlayerPoints.Clear();
        player2Points.Clear();
        player3Points.Clear();
        player4Points.Clear();
    }
}
