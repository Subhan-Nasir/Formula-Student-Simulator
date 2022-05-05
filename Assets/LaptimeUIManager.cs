using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class LaptimeUIManager : MonoBehaviour{

    public TMP_Text laptimeText;
    private string laptimeString;
    private double laptime;
    private double previousLaptime;
    private CheckpointsManager checkpointsManager;

    // Start is called before the first frame update
    void Start(){
        
        checkpointsManager = GameObject.Find("Ground").GetComponent<CheckpointsManager>();
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
    }
}
