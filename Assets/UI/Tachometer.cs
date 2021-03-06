using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Tachometer : MonoBehaviour
{
    // Code for radial dial adapted from: https://www.youtube.com/watch?v=5xWDKJj1UGY 
    public Rigidbody target;
    public bool enableRadialIndicator;
    public bool enableNeedle;
    public Image radialIndicator;
    public float minRPM = 0.0f;
    public float maxRPM = 0.0f; 
    public float minRPMArrowAngle;
    public float maxRPMArrowAngle;
    public Color32 defaultDialColour;
    public Color32 redlineDialColour;
    public Color32 gearLabelColour;
    

    private Transform RPMLabelTemplateTransform;

    private float maxFillAmount;
    

    [Header("UI")]
    public Text speedLabel; // The label that displays the RPM;
    public RectTransform arrow; // The arrow in the Tachometer
    public Text gearLabel;

    private float rpm = 0.0f;
    private int gear =0;
    private float speed = 0.0f;
    

    private void Awake() {
        RPMLabelTemplateTransform= transform.Find("RPMLabelTemplate");
        RPMLabelTemplateTransform.gameObject.SetActive(false);
        maxFillAmount = (Mathf.Abs(minRPMArrowAngle) + Mathf.Abs(maxRPMArrowAngle))/360;
        if(enableRadialIndicator == false){
            radialIndicator.gameObject.SetActive(false);
        } 
        if(enableNeedle == false){
            GameObject.Find("Needle").SetActive(false);
        }
        
               

        // minorTickTemplateTransform = transform.Find("MinorTickMark");
        
        

        CreateRPMLabels();
    }


    private void Update()
    {
        RaycastController carController=target.GetComponent<RaycastController>();
        rpm=carController.getEngineRPM()/1000;        
        gear=carController.getCurrentGear();

        radialIndicator.fillAmount = maxFillAmount * ((rpm-minRPM)/(maxRPM-minRPM));
        if(rpm >= maxRPM - 2){
            radialIndicator.color = redlineDialColour;
            gearLabel.color = new Color32( 254 , 24 , 0,255);
        }
        else{
            radialIndicator.color = defaultDialColour;
            gearLabel.color = gearLabelColour;
        }        

        //rpm = GameObject.Find("Raycast Reworked").GetComponent<RaycastController>().engineRPM/1000;
       // gear=GameObject.Find("Raycast Reworked").GetComponent<RaycastController>().currentGear;
        speed=target.velocity.magnitude*2.237f;

        if (speedLabel != null)
            speedLabel.text = Math.Round(speed,1).ToString() + "\nmph";
        if (arrow != null)
            arrow.localEulerAngles =
                new Vector3(0, 0, Mathf.Lerp(minRPMArrowAngle, maxRPMArrowAngle, rpm / maxRPM));
        if (gearLabel!=null)
            gearLabel.text=gear.ToString();
            
    }

    private void CreateRPMLabels(){
        int labelAmount=28;
        float totalAngleSize= minRPMArrowAngle-maxRPMArrowAngle;
        float rpmNormalised= rpm/maxRPM;

        // for (int i=0; i<=labelAmount; i++){
        //     Transform RPMLabelTransform = Instantiate(RPMLabelTemplateTransform,transform);
        //     float labelRPMNormalised= (float)i/labelAmount;
        //     RPMLabelTransform.eulerAngles = new Vector3(0,0,minRPMArrowAngle-labelRPMNormalised * totalAngleSize);
        //     RPMLabelTransform.Find("RPMLabelText").GetComponent<Text>().text=Mathf.RoundToInt(labelRPMNormalised * maxRPM).ToString();
        //     RPMLabelTransform.Find("RPMLabelText").eulerAngles= Vector3.zero;
        //     RPMLabelTransform.gameObject.SetActive(true);

        //     if (i>=labelAmount-2 && i<=labelAmount){
        //         RPMLabelTransform.Find("dashImage").GetComponent<Image>().color=new Color32( 254 , 9 , 0,255);
        //     }
        // }

       

        for (int i=0; i<=labelAmount; i++){
            Transform RPMLabelTransform = Instantiate(RPMLabelTemplateTransform,transform);
            float labelRPMNormalised= (float)i/labelAmount;
            RPMLabelTransform.eulerAngles = new Vector3(0,0,minRPMArrowAngle-labelRPMNormalised * totalAngleSize);
            if(i % 2 == 1){
                RPMLabelTransform.Find("RPMLabelText").GetComponent<Text>().text="";
                RPMLabelTransform.Find("RPMLabelText").eulerAngles= Vector3.zero;
                RPMLabelTransform.Find("dashImage").localScale = new Vector3(0.5f,1,1);

            }
            else{
                RPMLabelTransform.Find("RPMLabelText").GetComponent<Text>().text=Mathf.RoundToInt(labelRPMNormalised * maxRPM).ToString();
                RPMLabelTransform.Find("RPMLabelText").eulerAngles= Vector3.zero;
            }

            RPMLabelTransform.gameObject.SetActive(true);

            if (i>=labelAmount-4 && i<=labelAmount){
                RPMLabelTransform.Find("dashImage").GetComponent<Image>().color=new Color32( 254 , 9 , 0,255);
            }
        }


    }
}
