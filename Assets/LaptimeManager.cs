using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Records laptime 
public class LaptimeManager : MonoBehaviour
{
    private bool startCrossed = false;
    private float timer;

    void Update(){

        if(startCrossed == true){
            timer += Time.deltaTime;
        }

        // Debug.Log($"Timer = {timer}s");
    }

    
    private void OnTriggerEnter(Collider collision){

        if(collision.gameObject.tag == "Player"){
            Debug.Log("Start line crossed");
            startCrossed = true;
            timer = 0;
        }
        

    }
}
