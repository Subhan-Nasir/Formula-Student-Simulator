using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScenes : MonoBehaviour{

    public void restartCurrentScene(){

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);      

    }

    public void loadSilverstone(){
        SceneManager.LoadScene("Silverstone");
    }

    public void loadNorthWeald(){
        SceneManager.LoadScene("NorthWeald");
    }

   
    
}
