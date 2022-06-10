using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Code adpated from: 
// https://www.youtube.com/watch?v=IOYNg6v9sfc 
// The code for the checkpoint system is not exactly the same but it is similar.
public class CheckpointSingle : MonoBehaviour
{

    private CheckpointsManager checkpointsManager;

    private void OnTriggerEnter(Collider c){

        if(c.gameObject.tag == "Player"){
            checkpointsManager.PlayerThroughCheckpoint(this);
        }
    }

    public void SetCheckpointsManager(CheckpointsManager cm){
        this.checkpointsManager = cm;
    }

    
}
