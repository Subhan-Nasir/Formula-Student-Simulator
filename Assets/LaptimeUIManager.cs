using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

// Manages the UI element that shows the lap count, laptime, and previous lap time.
public class LaptimeUIManager : MonoBehaviour{

    // UI element that shows the laptime text, including previous laptime and checkpoint missed warning.
    public TMP_Text laptimeText;
    // UI element that shows the lap number text.
    public TMP_Text lapNumberText;
    private string laptimeString;
    private double laptime;
    private double previousLaptime;
    private CheckpointsManager checkpointsManager;
    public GameObject checkpointsObject;



    // Start is called before the first frame update
    void Start(){
        
        // checkpointsManager = GameObject.Find("Ground").GetComponent<CheckpointsManager>();
        checkpointsManager = checkpointsObject.GetComponent<CheckpointsManager>();
        laptimeString = "Laptime: ---" + "\nPrevious: ---";
        
        laptimeText.text = laptimeString;
        
    }

    // Update is called once per frame
    void Update(){
        
        laptime = checkpointsManager.getTimer();
        previousLaptime = checkpointsManager.getPreviousLaptime();
        laptimeString = "Laptime:\n" + TimeSpan.FromSeconds(laptime).ToString(@"mm\:ss\:fff");
        if(previousLaptime >0){
            laptimeString += "\nPrevious:\n" + TimeSpan.FromSeconds(previousLaptime).ToString(@"mm\:ss\:fff");
        }
        else{
            laptimeString += "\nPrevious:\n" + "--:--:--";
        }       
         
        laptimeText.text = laptimeString; 

        lapNumberText.text = "Lap: " + checkpointsManager.getLapNumber();

    }
}
