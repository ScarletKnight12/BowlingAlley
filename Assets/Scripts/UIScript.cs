/*
 Class corresponding to home screen related functionalities. Attached to buttons of the home screen.
 */
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Collections;

public class UIScript : MonoBehaviour
{
    
    public static string inputText = "Guest";
    public TMP_InputField inputField;
    public TextMeshProUGUI highScoresText;
    public Canvas CanvasA, CanvasB;
    
    string filePath;

#region unity methods
    // Start is called before the first frame update
    void Start()
    {

         filePath = Application.persistentDataPath + "/bowling_data.txt";
    }

    #endregion

    public void loadBowlingScene() {
        inputText = inputField.text;
        if (inputText == "" || inputText == null)
        {
            inputText = "Guest";
        }
        SceneManagerScript.loadBowlingScene();
    }

    public void exitApplication() {
        Application.Quit();
    }

    //Function which gets top 10 scores from score records and sets them in UI.  
    public void getTopScores() {

        if (File.Exists(filePath)) {
            //fetch score records from filepath.
            List<ScoreRecord> scoreRecords = new List<ScoreRecord>();
            List<ScoreRecord> resultRecords = new List<ScoreRecord>();
            string[] scores = File.ReadAllLines(filePath);
            foreach (string score in scores) {
                string[] datarr = score.Split(',');
                scoreRecords.Add(new ScoreRecord(datarr[0], int.Parse(datarr[1])));
            }

            for (int i = Mathf.Max(0, scoreRecords.Count - 10); i < scoreRecords.Count; i++) {
                resultRecords.Add(scoreRecords[i]);
            }

            //sort score records.
            resultRecords.Sort((x, y) => y.getScore().CompareTo(x.getScore()));
            
            //write top 10 scores to UI Text area reference.
            string resultText = "Top Scores for last 10 games:\n";
            for (int i = 0; i < 10; i++) {
                if (i < resultRecords.Count) {
                    resultText += "" + (i+1) + ". " + resultRecords[i].getPlayerName() + "    " + resultRecords[i].getScore() + "\n";
                }
            }

            
            CanvasA.gameObject.SetActive(false);
            CanvasB.gameObject.SetActive(true);


            highScoresText.SetText(resultText);
        }
        else
        {
            CanvasA.gameObject.SetActive(false);
            CanvasB.gameObject.SetActive(true);
            highScoresText.SetText("No Scores Available...");
        }
    }

    public void getBackHome()
    {
        CanvasA.gameObject.SetActive(true);
        CanvasB.gameObject.SetActive(false);

    }

}
