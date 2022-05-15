using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadMainMenu : MonoBehaviour
{

    // Code adapted from: https://www.youtube.com/watch?v=JivuXdrIHK0

    public GameObject UI;
    public GameObject MainMenu;
    
    [HideInInspector]
    public static bool gameIsPaused;

    private NewControls keys;
    private float mainMenuKey;


    void OnValidate(){
        keys = new NewControls();
    }

    void OnEnable(){
        keys.Enable();
    }

    void OnDisable(){
        keys.Disable();
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update(){
        mainMenuKey = keys.Track.MainMenuKey.ReadValue<float>();

        if(mainMenuKey == 1 & gameIsPaused == false){
            PauseGame();
        }
        
        
    }


    void PauseGame(){
        Time.timeScale = 0;
        UI.SetActive(false);        
        MainMenu.SetActive(true);
        gameIsPaused = true;
    }

    void ResumeGame(){
        MainMenu.SetActive(false);
        UI.SetActive(true);
        gameIsPaused = false;
        Time.timeScale = 1;

    }
}
