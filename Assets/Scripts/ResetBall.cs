/*
 This script is attached to the ball in VR scene. It takes care of
 updating score, resetting ball, restting pins and audio for ball roll and pins falling.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class ResetBall : MonoBehaviour
{
    //Variables which are set from unity editor.
    public TextMeshProUGUI scoresPanel;
    public TextMeshProUGUI frameScoresPanel;
    public TextMeshProUGUI finalScoresPanel;
    public AudioSource pinsAudioSource;

    //Local variables to the script.
    public Vector3 initialPosition;
    public int throwNum = 0;

    //reference to pins.
    public GameObject[] pinGameObjects;

    //arrays to store initial position and rotation of pins.
    public List<Vector3> initialPositionArray;
    public List<Quaternion> initialRotationArray;

    //local variables related to scores.
    string[] scoresArray = new string[52];
    string[] framesArray = new string[52];
    int[] scores = new int[21];
    int[] framescores = new int[10];
    int scoreIndex = 0;
    string filePath;

#region unity methods
    void Awake()
    {
        for (int i = 0; i < 52; i++) {
            scoresArray[i] = " ";
            framesArray[i] = " ";
        }
        int frameStartIndex = 3;
        int frameDividerIndex = 1;
        for (int i = 0; i < 9; i++) {
            scoresArray[frameStartIndex] = "|";
            scoresArray[frameStartIndex + 1] = "|";
            scoresArray[frameDividerIndex] = "|";
            frameDividerIndex += 5;
            frameStartIndex += 5;
        }
        scoresArray[50] = "|";
        scoresArray[51] = "|";
        scoresArray[46] = "|";
        scoresArray[48] = "|";
        initializeScoresArrays();
    }

    void Start()
    {
        initialPosition = gameObject.transform.position;
        pinGameObjects = GameObject.FindGameObjectsWithTag("Pin");
        filePath = Application.persistentDataPath + "/bowling_data.txt"; 

        for (int i = 0; i < pinGameObjects.Length; i++) {
            initialPositionArray.Add(pinGameObjects[i].transform.position);
            initialRotationArray.Add(pinGameObjects[i].transform.rotation);
        }

        scoresPanel.SetText(string.Concat(scoresArray));
        frameScoresPanel.SetText(string.Concat(framesArray));
        finalScoresPanel.SetText("Player Name:" + UIScript.inputText);

        //increasing max angular velocity to counter the slowing of ball.
        this.gameObject.GetComponent<Rigidbody>().maxAngularVelocity = (float)(2.5 * this.gameObject.GetComponent<Rigidbody>().maxAngularVelocity);

    }

    void Update()
    {
     
    }

#endregion

    void OnCollisionEnter(Collision collision)
    {
        //reset the ball's position if you hit sidelane
        if (!isGameFinished() && collision.gameObject.CompareTag("SideLane"))
        {
            this.gameObject.transform.position = initialPosition;
            this.gameObject.transform.position = new Vector3(0, -40, 0);
            StartCoroutine(resetThrow());
        }
        //Play pins falling sound if ball hits pins.
        else if (collision.gameObject.CompareTag("Pin"))
        {
            if (!pinsAudioSource.isPlaying)
            {
                pinsAudioSource.Play();
            }
        }
    }

    private IEnumerator resetThrow() {
        scoresPanel.SetText("Updating score. Please wait.......");
        if (areAnyPinsDown())
        {
            //If any pins are down, we wait for 7 seconds to reset pins and update score.
            yield return new WaitForSeconds(7);
        }
        else {
            //if foul or gutter, reset after 1 second.
            yield return new WaitForSeconds(1); 
        }

        throwNum++;
        
        int numOfPinsDown = getCountOfPinsDownAndDeactivateFallenPins();

        scores[scoreIndex++] = numOfPinsDown;


        if (throwNum == 1)
        {
            if (numOfPinsDown == pinGameObjects.Length)
            {
                if (scoreIndex != 19)
                {
                    throwNum = 0;
                    scores[scoreIndex++] = 0;
                }
                resetPins();
            }
        }
        else if (throwNum == 2)
        {
            if (scoreIndex == 20)
            {
                if (scores[18] + scores[19] >= 10)
                {
                    resetPins();
                    throwNum = 2;
                }
            }
            else
            {
                resetPins();
            }
        }

        updateScoresPanel();
        resetBall();
        if (isGameFinished()) {
            finalScoresPanel.SetText("Final Score is : " + framescores[9]);
            gameObject.transform.position = Vector3.zero;
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            saveScoreToFile();
        }
    }

    
    bool approx(Quaternion quatA, Quaternion quatB, float acceptableRange)
    {
        return 1 - Mathf.Abs(Quaternion.Dot(quatA, quatB)) < acceptableRange;
    }

    private void resetBall() {
        gameObject.transform.position = initialPosition;
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    private void resetPins() {
        throwNum = 0;
        for (int i = 0; i < pinGameObjects.Length; i++) {
            pinGameObjects[i].SetActive(true);
            pinGameObjects[i].transform.position = initialPositionArray[i];
            pinGameObjects[i].transform.rotation = initialRotationArray[i];
            pinGameObjects[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            pinGameObjects[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }

    private void initializeScoresArrays() {
        for (int i = 0; i < 21; i++) {
            scores[i] = -1;
        }
        for (int i = 0; i < 10; i++) {
            framescores[i] = -1;
        }
    }

    private void updateScoresPanel() {
        int tempScoresIndex = 0;
        for (int i = 0; i < 52; i++)  {
            if (!scoresArray[i].Equals("|")) {
                if (scores[tempScoresIndex] != -1) {
                    string score;
                    if (scores[tempScoresIndex] == 10)
                    {
                        score = "X";
                    }
                    else {
                        score = "" + scores[tempScoresIndex];
                    }
                    tempScoresIndex++;
                    scoresArray[i] = score;
                }
            }
        }
        scoresPanel.SetText(string.Concat(scoresArray));
        fillCumulativeScores();
        updateFrameScoresUI();
    }

    private void fillCumulativeScores() {
        initializeFrameScores();
        assignCumulativeScores();
    }

    private void initializeFrameScores() {
        for (int i = 0; i < 10; i++)
        {
            if (i < 9)
            {
                int scoresIndex1 = 2 * i;
                int scoresIndex2 = 2 * i + 1;

                if (scores[scoresIndex1] != -1)
                {
                    if (scores[scoresIndex2] != -1)
                    {
                        framescores[i] = scores[scoresIndex1] + scores[scoresIndex2];
                    }
                    else
                    {
                        framescores[i] = scores[scoresIndex1];
                    }
                }
            }
            else
            {
                if (scores[18] != -1)
                {
                    framescores[i] = scores[18];
                    if (scores[19] != -1)
                    {
                        framescores[i] += scores[19];
                        if (scores[20] != -1)
                        {
                            framescores[i] += scores[20];
                        }
                    }
                }
            }
        }
    }

    private void assignCumulativeScores() {
        for (int i = 0; i < 9; i++) {
            if (framescores[i] != -1)
            {
                int scoreIndex1 = 2 * i;
                int scoreIndex2 = 2 * i + 1;
                if (scores[scoreIndex1] != -1)
                {
                    if (scores[scoreIndex1] == 10)
                    {
                        if (framescores[i + 1] != -1)
                        {
                            framescores[i] += framescores[i + 1];
                        }
                    }
                    else if (scores[scoreIndex2] != -1 && (scores[scoreIndex1] + scores[scoreIndex2] == 10)) {
                        if (scores[scoreIndex2 + 1] != -1) {
                            framescores[i] += scores[scoreIndex2 + 1];
                            /*
                            if (scores[scoreIndex2 + 2] != -1) {
                                framescores[i] += scores[scoreIndex2 + 2];
                            }
                            */
                        }
                    }
                }
                if (i > 0) {
                    framescores[i] += framescores[i - 1];
                }
            }
        }
        if (framescores[9] != -1) {
            framescores[9] += framescores[8];
        }
    }

    private void updateFrameScoresUI() {
        int initialFramesIndex = 0;
        //any value less than 51
        for (int i = 0; i < 47; i += 5)
        {
            if (framescores[initialFramesIndex] != -1)
            {
                framesArray[i] = "" + framescores[initialFramesIndex];
                framesArray[i + 1] = "";
                initialFramesIndex++;
            }
            else
            {
                break;
            }
        }
        frameScoresPanel.SetText(string.Concat(framesArray));
    }

    private bool areAnyPinsDown() {
        for (int i = 0; i < pinGameObjects.Length; i++)
        {
            if (pinGameObjects[i].activeInHierarchy && !approx(pinGameObjects[i].transform.rotation, initialRotationArray[i], 0.1f))
            {
                return true;
            }
        }
        return false;
    }

    private int getCountOfPinsDownAndDeactivateFallenPins() {
        int count = 0;

        for (int i = 0; i < pinGameObjects.Length; i++)
        {
            if (pinGameObjects[i].activeInHierarchy && !approx(pinGameObjects[i].transform.rotation, initialRotationArray[i], 0.1f))
            {
                pinGameObjects[i].SetActive(false);
                count++;
            }
        }

        for (int i = 0; i < pinGameObjects.Length; i++) {
            if (pinGameObjects[i].activeInHierarchy) {
                pinGameObjects[i].transform.position = initialPositionArray[i];
                pinGameObjects[i].transform.rotation = initialRotationArray[i];
            }
        }

        return count;
    }

    private bool isGameFinished() {
        if (scores[18] != -1 && scores[19] != -1) {
            if (scores[18] + scores[19] >= 10)
            {
                if (scores[20] != -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else {
                return true;
            }
        }
        return false;
    }

    private void saveScoreToFile() {
        string data = UIScript.inputText + "," + framescores[9];
        using (StreamWriter w = File.AppendText(filePath)) {
            w.WriteLine(data);
            w.Close();
        }
    }

}
