using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public class UIScript : MonoBehaviour
{


    public static string inputText = "Guest";

    public TMP_InputField inputField;

    public TextMeshProUGUI highScoresText;

    string filePath;

    // Start is called before the first frame update
    void Start()
    {
        filePath = Application.persistentDataPath + "/bowling_data.txt";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

    public void getTopScores() {
        if (File.Exists(filePath)) {
            List<ScoreRecord> scoreRecords = new List<ScoreRecord>();
            string[] scores = File.ReadAllLines(filePath);
            foreach (string score in scores) {
                string[] datarr = score.Split(',');
                scoreRecords.Add(new ScoreRecord(datarr[0], int.Parse(datarr[1])));
            }
            scoreRecords.Sort((x, y) => y.getScore().CompareTo(x.getScore()));
            string resultText = "";
            for (int i = 0; i < 3; i++) {
                if (i < scoreRecords.Count) {
                    resultText += scoreRecords[i].getPlayerName() + "\t" + scoreRecords[i].getScore() + "\n";
                }
            }
            highScoresText.SetText(resultText);
        }
    }

}
