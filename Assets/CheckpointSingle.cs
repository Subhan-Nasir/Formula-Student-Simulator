using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
