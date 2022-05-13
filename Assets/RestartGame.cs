using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class RestartGame : MonoBehaviour
{
    // Start is called before the first frame update
    public void whenButtonClick(){
        
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);      

    }
}
