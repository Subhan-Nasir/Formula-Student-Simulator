using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Used to load different tracks and main menu.
public class LoadScenes : MonoBehaviour{  

    public GameObject UI; // UI element that has the Telemetry, speedmeter etc.
    public GameObject MainMenu; // UI element that has the main menu.

    [HideInInspector]
    public static bool gameIsPaused;

    private NewControls keys;
    private float menuKeyInput;
    private float previousMenuKeyInput;

    void OnValidate(){
        keys = new NewControls();
    }

    void OnEnable(){
        keys.Enable();
    }

    void OnDisable(){
        keys.Disable();
    }

    void Update(){
        menuKeyInput = keys.Track.MainMenuKey.ReadValue<float>();
        

        if((previousMenuKeyInput == 0 & menuKeyInput == 1 ) & gameIsPaused == false){
            PauseGame();
        }
        else if((previousMenuKeyInput == 0 & menuKeyInput == 1 ) & gameIsPaused == true){
            ResumeGame();
        }
        
        previousMenuKeyInput = menuKeyInput;
    }

    
    public void PauseGame(){
        Time.timeScale = 0;
        UI.SetActive(false);        
        MainMenu.SetActive(true);
        gameIsPaused = true;
    }

    public void ResumeGame(){
        MainMenu.SetActive(false);
        UI.SetActive(true);
        gameIsPaused = false;
        Time.timeScale = 1;

    }


    public void restartCurrentScene(){
        Time.timeScale = 1;
        gameIsPaused = false;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);      

    }

    public void loadSilverstone(){
        Time.timeScale = 1;
        gameIsPaused= false;
        Debug.Log("SILVERSTONE PRESSED");
        SceneManager.LoadScene("Silverstone");
    }

    public void loadNorthWeald(){
        Time.timeScale = 1;
        gameIsPaused = false;
        SceneManager.LoadScene("NorthWeald");
    }

    public void exitPressed(){
        Debug.Log("EXIT PRESSED");
        Application.Quit();
    }

    
   
    
}
