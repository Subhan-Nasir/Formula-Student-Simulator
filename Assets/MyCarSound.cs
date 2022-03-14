// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class MyCarSound : MonoBehaviour
// {
//     public AudioSource[] audioSource;
//     private AudioSource engineAudioSource;
//     private AudioSource skidAudioSource;
//     private AudioSource tokyoDriftSource;

//     private float[] Volumes= new float[4];



//     public RaycastController carController;
//     private Wheel[] wheelList;
//     // Start is called before the first frame update
//     void Start()
//     {
//         audioSource=GetComponents<AudioSource>();
//         engineAudioSource=audioSource[0];
//         skidAudioSource=audioSource[1];
//         tokyoDriftSource=audioSource[2];
//         wheelList = carController.getWheels();
//         Debug.Log(audioSource[2]);
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         engineAudioSource.pitch=(7*carController.getEngineRPM()/12350)+0.06478f;  
        
        for(int i=0;i<4;i++){
            if((wheelList[i].slipRatio-0.15f)>=(wheelList[i].slipAngle-0.2f)){
                Volumes[i]=1.1f*((0.265f*wheelList[i].slipRatio)-0.015f);
            }
            else {
                Volumes[i]=1.1f*((0.164f*wheelList[i].slipAngle)-0.008f);
            }
        
        

            if( Mathf.Abs(wheelList[i].slipRatio) > 0.15f || (Mathf.Abs(wheelList[i].slipAngle) > 0.2f)){
                skidAudioSource.volume= Volumes[0]+Volumes[1]+Volumes[2]+Volumes[3];
                skidAudioSource.Play();
                Debug.Log("Slipping");
            }
            else{
             skidAudioSource.Stop();
            }

            if(carController.tokyoDriftMode==true){
             tokyoDriftSource.Play();

                //Debug.Log("LOL");


             }
        }



      
//     }
// }
