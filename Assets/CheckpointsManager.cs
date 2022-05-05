using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CheckpointsManager : MonoBehaviour
{

    private List<CheckpointSingle> checkpointList;
    private int nextCheckpointIndex;
    private double timer;
    private double previousLaptime;
    private bool startCrossed;
    public NotificationTriggerEvent notificationManager;
    // public GameObject ui;

    void Awake(){
        Transform checkpointsTransform = transform.Find("Checkpoints");
        checkpointList = new List<CheckpointSingle>();
        foreach(Transform t in checkpointsTransform){
            CheckpointSingle checkpoint = t.GetComponent<CheckpointSingle>();
            checkpoint.SetCheckpointsManager(this);
            checkpointList.Add(checkpoint);
        }
        nextCheckpointIndex = 0;      
        startCrossed = false; 
        notificationManager = GameObject.Find("NotificationPanel").GetComponent<NotificationTriggerEvent>(); 
        // notificationManager = ui.GetComponent<NotificationTriggerEvent>();

    }
    // Start is called before the first frame update
    void Start(){
        
    }

    // Update is called once per frame
    void Update(){
        if(startCrossed){
            timer += (double) Time.deltaTime;            
        }

        // Debug.Log(Math.Round(timer, 2));

    }

    public void PlayerThroughCheckpoint(CheckpointSingle c){
        if(checkpointList.IndexOf(c) == nextCheckpointIndex){
            // Correct checkpoint
            // Debug.Log("Passed correct checkpoint");

            // Increments for each checkpoint and resets to 0 once all checkpoints 
            // are cleared.
            nextCheckpointIndex = (nextCheckpointIndex + 1) %  checkpointList.Count; 

            if(nextCheckpointIndex == 1){
                if(startCrossed == true){
                    Debug.Log($"LAP TIME = {timer}");
                    previousLaptime = timer;
                    notificationManager.showNotification("New Laptime: " + TimeSpan.FromSeconds(timer).ToString(@"mm\:ss\:fff"));
                }
                else{
                    startCrossed = true;
                }
                timer = 0;                
            }
        }
        else{
            Debug.Log("Wrong checkpoint");
            notificationManager.showNotification("Wrong Checkpoint");
        }


    }

    public double getTimer(){ return timer;}
    public double getPreviousLaptime(){return previousLaptime;}

}
