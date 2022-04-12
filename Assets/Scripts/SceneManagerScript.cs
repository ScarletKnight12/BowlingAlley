/*
 Manager class which provides static methods to load different scenes.
 */

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    //Manager class which provides static methods to load different scenes.
    public static void loadBowlingScene() {
        SceneManager.LoadSceneAsync("BowlingScene");
    }

    public static void loadHomeScene() {
        SceneManager.LoadSceneAsync("HomeScene");
    }

}
