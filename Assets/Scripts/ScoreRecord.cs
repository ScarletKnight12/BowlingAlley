/*
    Data class to hold player name and his score.
 */
public class ScoreRecord
{


    private string playerName;
    private int score;

    public ScoreRecord(string playerName, int score) {
        this.playerName = playerName;
        this.score = score;
    }

 #region getter methods
    public string getPlayerName() {
        return playerName;
    }

    public int getScore() {
        return score;
    }
#endregion

}
