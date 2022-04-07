using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreRecord
{

    private string playerName;

    private int score;

    public ScoreRecord(string playerName, int score) {
        this.playerName = playerName;
        this.score = score;
    }

    public string getPlayerName() {
        return playerName;
    }

    public int getScore() {
        return score;
    }

}
