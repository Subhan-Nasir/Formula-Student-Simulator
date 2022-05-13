using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class CheckpointsManager : MonoBehaviour
{

    public TMP_Text checkpointMissedWarning;
    private List<CheckpointSingle> checkpointList;
    private int nextCheckpointIndex;
    private double timer;
    private double previousLaptime;
    private bool startCrossed;
    private NotificationTriggerEvent notificationManager;
    private int lapNumber;
    // public GameObject ui;
    private bool wrongCheckpointPassed = false;
    private bool restartingLap; // allows player to restart lap if they pass the wrong checkpoint;
    private bool correctCheckpointPassed;
    private Rigidbody car; 
    


   
    void Awake(){
        // Transform checkpointsTransform = transform.Find("Checkpoints");
        Transform checkpointsTransform = gameObject.transform;
        checkpointList = new List<CheckpointSingle>();
        foreach(Transform t in checkpointsTransform){
            CheckpointSingle checkpoint = t.GetComponent<CheckpointSingle>();
            checkpoint.SetCheckpointsManager(this);
            checkpointList.Add(checkpoint);
        }
        nextCheckpointIndex = 0;      
        startCrossed = false; 
        notificationManager = GameObject.Find("NotificationPanel").GetComponent<NotificationTriggerEvent>(); 
        lapNumber = 1;
        checkpointMissedWarning.text = "";
        car = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();

      

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
        int checkpointIndex = checkpointList.IndexOf(c);
        if(checkpointIndex == nextCheckpointIndex){ // Correct checkpoint
            Debug.Log($"Checkpoint {checkpointIndex} passed at: " + TimeSpan.FromSeconds(timer).ToString(@"mm\:ss\:fff") + " with speed: " + car.velocity.magnitude +" m/s");   

                     
            // Increments for each checkpoint and resets to 0 once all checkpoints 
            // are cleared.
            nextCheckpointIndex = (nextCheckpointIndex + 1) %  checkpointList.Count; 

            if(nextCheckpointIndex == 1 ){
                if(startCrossed == true & wrongCheckpointPassed == false){ // new laptime set
                    Debug.Log($"LAP TIME = {timer}");
                    previousLaptime = timer;
                    notificationManager.showNotification("New Laptime: " + TimeSpan.FromSeconds(timer).ToString(@"mm\:ss\:fff"));
                    lapNumber += 1;
                }                
                else{ // started first lap
                    startCrossed = true;
                }                
                timer = 0;                          
            }            
        }
        else if(checkpointIndex == 0 & wrongCheckpointPassed == true){ // Resetting lap after wrong checkpoint.
            timer = 0;
            nextCheckpointIndex = 1;
            wrongCheckpointPassed = false;
            checkpointMissedWarning.text = "";
        }
        else{ // Wrong checkpoint
            
            if(wrongCheckpointPassed == false){
                notificationManager.showNotification("Wrong Checkpoint.\nRestart the lap or find correct checkpoint.");
                checkpointMissedWarning.text = "CHECKPOINT MISSED";
            }
            
            wrongCheckpointPassed = true;            
        }


    }

    public double getTimer(){ return timer;}
    public double getPreviousLaptime(){return previousLaptime;}
    public int getLapNumber(){return lapNumber;}

}
