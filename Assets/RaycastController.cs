using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
// Code adapted from: https://www.youtube.com/watch?v=x0LUiE0dxP0 

public class RaycastController : MonoBehaviour{

    [Header("Controls Selection")]
    public bool usingKeyboard;

    [Header("0 to 60 Timer")]
    public bool enableTimer;
    
    [Header("Tokyo Drift Mode")]

    public bool tokyoDriftMode=false;

    [Header("Car Components ")]
    public Rigidbody rb;    
    public List<GameObject> springs;
    public List<GameObject> wheelObjects;
    public List<GameObject> meshes;

    // public static RaycastController cc;

    [Header("Engine")]
    public AnimationCurve engineCurve;

    [Header("Transmission")]
    public List<float> gearRatios;
    public float primaryGearRatio;
    public float finalDriveRatio;
    public float idleRPM = 1500;
    public float maxRPM = 14000;
    public float auxillaryLoss = 0.15f;
    public float maxEngineBrakingTorque = 5;
    public float totalDrivetrainInertia = 1.5f;


    private int currentGear = 1;
    private float engineRPM;
    private float shiftUp;
    private float shiftDown;
    private float engineTorque;
    private float wheelTorque;
    private float engineBraking;




    [Header("Centre of mass")]    
    public GameObject COM_Finder;

    [Header("Suspension Settings")]
    public float naturalLength;
    public float springTravel;
    public float springStiffness;    
    public float dampingCoefficient;

    [Header("Bump stops")] 
    public float bumpStiffness;
    public float bumpTravel;

    [Header("Anti Roll bars")]
    public float antiRollStiffness;
    private float[] antiRollForces = new float[] {0,0};
    
    private float pitch;
    private float lateralRoll;
    private float longitudinalRoll;
    
    private Suspension[] suspensions = new Suspension[4];
    
    [Header("Wheel")]
    public float wheelRadius = 0.23f;
    public float wheelMass = 5;
    public float brakeBias;  
       
    
    private Dictionary<string, float> lateralConstants = new Dictionary<string,float>(){
        {"B", 11.45f},
        {"C", 1.551f},
        {"D", 1790},
        {"E", 0.1859f},
        {"c", 0.00151f},
        {"m", 2.533E-7f}
    };

        
    private Dictionary<string, float> longitudinalConstants = new Dictionary<string, float>(){
        {"B", 11.93f},
        {"C", 1.716f},
        {"D", 1711},
        {"E", 0.3398f},
        {"c", 0.00179f},
        {"m", 3.62E-7f}     
    };
    
    private Wheel[] wheels = new Wheel[4];

    [Header("Steering")]
    public float steerAngle = 20f;
    public float steerSpeed = 10f;

    private float wheel_x;
    private float wheel_y;
    private float wheel_z;
    
    private float steerInput;
    private float wheelAngleLeft;
    private float wheelAngleRight;
    private float steerAngleLeft;
    private float steerAngleRight;

    
    [Header("Ackermann Steering")]
    public bool enableAckermannSteering = true;
    public float wheelBase = 1.5f;
    public float rearTrack = 1.085f;
    public float turnRadius = 3.14f;
    private Vector3 steeringForce;

    private NewControls keys;
    private float throttle;
    private float brake;
    private float userInput;

    private float theTime = 0f;
    private bool timerOn = false;
    private bool speedReached = false;

    [UPyPlot.UPyPlotController.UPyProbe]
    private float RL_LateralForce;

    [UPyPlot.UPyPlotController.UPyProbe]
    private float FL_LateralForce;

    private float gearTimer;

    private float speed;
    private float drag;
    private float lift;

    private float brakeBiasUp;
    private float brakeBiasDown;
    private float brakeBiasTimer;
    
    // needed for engine sound
    private float previousSpeed;
    private bool isAcclerating;
    
    private float COM_height;
    private float frontRCheight;
    private float rearRCheight;
    private float COMLateralVelocity;
    private float COMLateralVelocityPrevious;
    private float COMLateralAcceleration;
    private float massFront;
    private float massRear;

    private float elasticLoadTransferFront; 
    private float elasticLoadTransferRear;

    private float geometricLoadTransferFront;
    private float geometricLoadTransferRear;
    private float longitudnialLoadTransfer;

    private float totalLoadTransferFront;
    private float totalLoadTransferRear;
    
    private float rollCenterHeightFront; // 0.030602
    private float rollCenterHeightRear; // 0.060289

    private float rollStiffnessFront;
    private float rollStiffnessRear;

    private float trackFront;
    private float trackRear;

    private float loadFront;
    private float loadRear;

    private float baseLoadFront;
    private float baseLoadRear;

    private float wheelVerticalLoad;

    private float COMlongitudinalVelocity;
    private float COMlongitudinalVelocityPrevious;
    private float COMlongitudinalAcceleration;


    // front: 1.055
    // rear: 1.101
    


    void OnValidate(){
        keys = new NewControls();
        rb.centerOfMass = COM_Finder.transform.localPosition;
             
                           
        
        for (int i = 0; i < 4; i++){
            suspensions[i] = new Suspension(i, naturalLength, springTravel, springStiffness, dampingCoefficient, bumpStiffness, bumpTravel, wheelRadius);                     
            wheels[i] = new Wheel(i, wheelObjects[i], meshes[i], rb, wheelRadius, wheelMass, brakeBias, totalDrivetrainInertia, longitudinalConstants, lateralConstants);
            
        }
        
                
    }

    

    void OnEnable(){
        
        keys.Enable();

        engineCurve.keys = new Keyframe[1];
        engineCurve.AddKey(3000,38.216f);
        engineCurve.AddKey(3500,39.063f);
        engineCurve.AddKey(4000,35.961f);
        engineCurve.AddKey(4500,38.928f);
        engineCurve.AddKey(5000,40.732f);
        engineCurve.AddKey(5500,41.690f);
        engineCurve.AddKey(6000,41.301f);
        engineCurve.AddKey(6500,37.138f);
        engineCurve.AddKey(7000,39.776f);
        engineCurve.AddKey(7500,38.928f);
        engineCurve.AddKey(8000,44.328f);
        engineCurve.AddKey(8500,48.926f);
        engineCurve.AddKey(9000,47.236f);
        engineCurve.AddKey(9500,45.724f);
        engineCurve.AddKey(10000,47.711f);
        engineCurve.AddKey(10500,48.212f);
        engineCurve.AddKey(11000,48.099f);
        engineCurve.AddKey(11500,45.760f);
        engineCurve.AddKey(12000,44.328f);
        engineCurve.AddKey(12500,42.954f);
        engineCurve.AddKey(13000,40.536f);
        engineCurve.AddKey(13500,38.243f);
        engineCurve.AddKey(14000,35.198f);
        
        
        
    }

    void OnDisable(){
        keys.Disable();
    }

    

    void Start(){
        //

        // cc=this;
        rb.inertiaTensor = new Vector3(123.1586f,61.15857f,112f);
        rb.inertiaTensorRotation = Quaternion.Euler(33.5407f,0,0);
        COM_height = COM_Finder.transform.position.y;
        massFront = rb.mass *  Mathf.Abs((COM_Finder.transform.position.z - springs[2].transform.position.z)/(springs[0].transform.position.z - springs[2].transform.position.z));
        massRear = rb.mass *  Mathf.Abs((COM_Finder.transform.position.z - springs[0].transform.position.z)/(springs[0].transform.position.z - springs[2].transform.position.z));

        rollCenterHeightFront = 0.030602f;
        rollCenterHeightRear = 0.060289f;
        rollStiffnessFront = 1;
        rollStiffnessRear = 1;

        trackFront = Mathf.Abs(wheelObjects[1].transform.position.x - wheelObjects[0].transform.position.x);
        trackRear = Mathf.Abs(wheelObjects[3].transform.position.x - wheelObjects[2].transform.position.x);

        baseLoadFront = massFront * 9.81f/2;
        baseLoadRear = massRear * 9.81f/2;

        
    }

    void Update(){

        if(usingKeyboard){

            steerInput = keys.Track.Steering.ReadValue<float>();
            throttle = keys.Track.Throttle.ReadValue<float>();
            brake = keys.Track.Brake.ReadValue<float>();

            steerInput = Mathf.Clamp(steerInput, -1,1);
            throttle = Mathf.Clamp(throttle, 0,1);
            brake = Mathf.Clamp(brake, 0,1);
        }
        else{

            throttle = keys.Track.Throttle.ReadValue<float>();
            brake = keys.Track.Brake.ReadValue<float>();
            brakeBiasUp = keys.Track.BrakeBiasUp.ReadValue<float>();
            brakeBiasDown = keys.Track.BrakeBiasDown.ReadValue<float>();

            // Clamp values 
            throttle = Mathf.Clamp(throttle, -0.336f,0.0895f); 
            brake = Mathf.Clamp(brake, -0.4513f,-0.0761f);
            
            // Normalise Values
            throttle = (throttle - -0.336f)/(0.0895f - -0.336f);
            brake = -(brake- - 0.4513f)/(-0.4513f - -0.0761f);

            steerInput = keys.Track.Steering.ReadValue<float>();
            steerInput = Mathf.Clamp(steerInput, -1,1);
        }
        
      
        shiftUp = keys.Track.ShiftUp.ReadValue<float>();
        shiftDown = keys.Track.ShiftDown.ReadValue<float>();
        
        if(throttle > Mathf.Abs(brake)){
            userInput = throttle;
        }
        else{
            userInput = -brake;
        }


        if(shiftUp == 1 & gearTimer > 0.2f){
            currentGear += 1;
            gearTimer = 0;
                     
        }
        else if(shiftDown == 1 & gearTimer > 0.2f){
            currentGear -= 1;
            gearTimer = 0;
                       
        }
        else{
            currentGear += 0;
        }

        if(brakeBiasUp > 0 & brakeBiasTimer > 0.2f){
            brakeBias += 0.1f;
            brakeBiasTimer = 0;
            // Debug.Log("brake bias increased");
        }
        else if(brakeBiasDown > 0 & brakeBiasTimer > 0.2f){
            brakeBias -= 0.1f;
            brakeBiasTimer = 0;
        }
      
        

        brakeBias = Mathf.Clamp(brakeBias, 0,1);
        for(int i = 0; i<4; i++){
            wheels[i].brakeBias = brakeBias;
        }

        // Debug.Log($" Brake bias = {wheels[0].brakeBias}, brakeBiasUp = {brakeBiasUp}, brakeBiasDown = {brakeBiasDown}, timer = {brakeBiasTimer}");
        gearTimer += Time.deltaTime;
        brakeBiasTimer += Time.deltaTime;


        currentGear = Mathf.Clamp(currentGear, 1,5);


        
        engineRPM = Mathf.Clamp(engineRPM, idleRPM, 14000);

        engineTorque = (1-auxillaryLoss) * (engineCurve.Evaluate(engineRPM) * userInput);
        engineBraking = maxEngineBrakingTorque * (1 - userInput);      
       

        ApplySteering();     
         
    }

    void FixedUpdate(){

        // COM_height = 0.252f;
        COM_height = COM_Finder.transform.position.y - transform.position.y;

        COMLateralVelocityPrevious = COMLateralVelocity;
        COMlongitudinalVelocityPrevious = COMlongitudinalVelocity;

        // COMLateralVelocity = COM_Finder.transform.InverseTransformDirection(rb.GetPointVelocity(COM_Finder.transform.position)).x;
        // COMLateralVelocity = rb.GetPointVelocity(COM_Finder.transform.position).x;
        COMLateralVelocity = COM_Finder.transform.InverseTransformDirection(rb.velocity).x;  
        COMlongitudinalVelocity = COM_Finder.transform.InverseTransformDirection(rb.velocity).z;               
        COMLateralAcceleration = (COMLateralVelocity - COMLateralVelocityPrevious)/Time.fixedDeltaTime;
        COMlongitudinalAcceleration = (COMlongitudinalVelocity -COMlongitudinalVelocityPrevious)/Time.fixedDeltaTime;
        // COMLateralAcceleration = Mathf.Clamp(COMLateralAcceleration, -5,5);
        
        elasticLoadTransferFront = Suspension.elasticLoadTransferFront(rollStiffnessFront,
                                                                        rollStiffnessRear,
                                                                        massFront,massRear,
                                                                        COM_height,
                                                                        rollCenterHeightFront,
                                                                        rollCenterHeightRear,
                                                                        COMLateralAcceleration, 
                                                                        trackFront);

        elasticLoadTransferRear = Suspension.elasticLoadTransferRear(rollStiffnessFront, 
                                                                        rollStiffnessRear,
                                                                        massFront, 
                                                                        massRear, 
                                                                        COM_height, 
                                                                        rollCenterHeightFront, 
                                                                        rollCenterHeightRear, 
                                                                        COMLateralAcceleration, 
                                                                        trackRear);

        geometricLoadTransferFront = Suspension.geometricLoadTransferFront(massFront, COMLateralAcceleration, rollCenterHeightFront, trackFront);
        geometricLoadTransferRear = Suspension.geometricLoadTransferRear(massRear, COMLateralAcceleration, rollCenterHeightRear, trackRear);
        longitudnialLoadTransfer = Suspension.LongitudinalLoadTransfer(rb.mass, COMlongitudinalAcceleration, COM_height, wheelBase);
        
        totalLoadTransferFront = (elasticLoadTransferFront + geometricLoadTransferFront);
        totalLoadTransferRear = (elasticLoadTransferRear + geometricLoadTransferRear);
        

        // Debug.Log($"W_geometric_front = {geometricLoadTransferFront}, W_geometric_rear = { geometricLoadTransferRear}, W_elastic_front = {elasticLoadTransferFront}, W_elastic_rear = {elasticLoadTransferRear},  lateral acceleration = {COMLateralAcceleration}, massFront = {massFront}, massRear = {massRear}, tf = {trackFront}, tr = {trackRear}");
        //Debug.Log($"com height = {COM_height}, track front = {trackFront}, track rear = {trackRear}");

        for(int i = 0; i<springs.Count; i++){   

            bool contact = Physics.Raycast(springs[i].transform.position, -transform.up, out RaycastHit hit, naturalLength + springTravel + wheelRadius);
            if(COMLateralVelocity >= 0f){
                if(i == 0){
                    wheelVerticalLoad = baseLoadFront + totalLoadTransferFront - longitudnialLoadTransfer;
                }
                else if( i == 1){
                    wheelVerticalLoad = baseLoadFront - totalLoadTransferFront - longitudnialLoadTransfer;
                }
                else if(i == 2){
                    wheelVerticalLoad = baseLoadRear + totalLoadTransferRear + longitudnialLoadTransfer;
                }
                else{
                    wheelVerticalLoad = baseLoadRear - totalLoadTransferRear + longitudnialLoadTransfer;
                }

            }
            else{
                if(i == 0| i == 1){wheelVerticalLoad = baseLoadFront;}
                else{ wheelVerticalLoad = baseLoadRear;}
            }
            

            // Debug.Log($" wheel id {i}: vertical load = {wheelVerticalLoad}, lateral acceleration = {COMLateralAcceleration}");

            if(contact){            
                
                if(i == 2 | i == 3){
                    wheels[i].wheelTorque = (engineTorque - engineBraking) * gearRatios[currentGear + 1] *primaryGearRatio * finalDriveRatio;
                }

                // Force vectors from suspension, wheel and anti rollbars.
                Vector3 suspensionForceVector = suspensions[i].getUpdatedForce(hit, Time.fixedDeltaTime, contact);          
                Vector3 wheelForceVector = wheels[i].getUpdatedForce(userInput, gearRatios[currentGear + 1], finalDriveRatio, primaryGearRatio, hit, Time.fixedDeltaTime, wheelVerticalLoad);            
                Vector3 antiRollForceVector = getAntiRollForce(suspensions[2], suspensions[3], antiRollStiffness, i) * hit.normal;

                rb.AddForceAtPosition(wheelForceVector + suspensionForceVector, hit.point + new Vector3 (0,0f,0)); 
                
                

                

                float averageRearRPM = (9.5493f)*(wheels[2].omega + wheels[3].omega)/2;
                if(currentGear != 0){
                    engineRPM = averageRearRPM * (gearRatios[currentGear + 1] * primaryGearRatio * finalDriveRatio);
                }
                // Debug.Log($"Engine RPM = {engineRPM}, Engine Torque = {engineTorque}, Current Gear = {currentGear}, User Input = {userInput}");
                                
            }
            else{
                suspensions[i].contact = false;
            }
        }

        showTimer();
        
        previousSpeed = speed;
        speed = rb.velocity.magnitude;

        if(speed >= previousSpeed){isAcclerating = true;}

        drag = (5f * 1.225f * Mathf.Pow(speed,2) * 0.947f)/2;
        lift = (0.17f * 1.225f * Mathf.Pow(speed,2) * 0.947f)/2;
        // Debug.Log($" Drag = {drag}, Lift = {lift}");

        // rb.AddForceAtPosition( -drag*transform.forward, COM_Fidner.transform.position);
        // rb.AddForceAtPosition( lift*transform.up, COM_Fidner.transform.position);
        FL_LateralForce = wheels[0].lateralForce;
        RL_LateralForce = wheels[3].lateralForce;

        

        

        
    }
    

    void showTimer(){
        float carSpeed = rb.velocity.z;
        if(carSpeed > 0 & speedReached == false){
            timerOn = true;
        }

        if(timerOn == true){
            theTime += Time.fixedDeltaTime;
            
            if(carSpeed >= 26.8f){
                speedReached = true;
                timerOn = false;
            }
        }

        if(enableTimer == true){
            Debug.Log($"Timer = {theTime}");
        }
    }

    // void OnDrawGizmos(){

    //     Gizmos.color = Color.white;
    //     Gizmos.DrawWireSphere( transform.TransformPoint(rb.centerOfMass),0.2f);
        
    //     for(int i = 0; i < springs.Count; i++){
        
                        
    //         // Gizmos.color = Color.blue;
    //         Ray ray = new Ray(springs[i].transform.position, -transform.up);           
    //         // Gizmos.DrawLine(ray.origin, -suspensions[i].springLength * transform.up + springs[i].transform.position);

            
    //         Gizmos.color = Color.yellow;
    //         // Gizmos.DrawLine(-suspensions[i].springLength * transform.up + springs[i].transform.position, -suspensions[i].springLength * transform.up + springs[i].transform.position + transform.up * -wheelRadius);
            
        
    //         Gizmos.color = Color.white;

    //         Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].wheelObject.transform.right * (wheels[i].lateralForce/1000));
    //         Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].wheelObject.transform.forward * (wheels[i].longitudinalForce/1000));
    //         Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].wheelObject.transform.up * wheels[i].verticalLoad/1000);
    //         Gizmos.DrawRay(COM_Finder.transform.position, -drag * transform.forward /1000);
    //         Gizmos.color = Color.yellow;
    //         if(i == 2 | i == 3){
    //             Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].wheelObject.transform.up * antiRollForces[i-2]/1000);
    //         }

    //         // Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].forceVector/1000 );
            
    //     }
        
    // }

    void ApplySteering(){

         // Applies Ackermann sterring if it is enabled.
        if(enableAckermannSteering){
            //Steering right
            if(steerInput > 0){
                steerAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack/2))) * steerInput;
                steerAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack/2))) * steerInput;

            }//Steering left            
            else if (steerInput < 0){
                steerAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack/2))) * steerInput;
                steerAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack/2))) * steerInput;
                

            } // Not steering
            else{
                steerAngleLeft = 0;
                steerAngleRight = 0;

            }
            
        }
        // If Ackermann steering is disabled, both wheels have the same steer angle.
        else{

            steerAngleLeft = steerAngle * steerInput;
            steerAngleRight = steerAngle * steerInput;           

        }

        // wheelAngleLeft = Mathf.Lerp(wheelAngleLeft, steerAngleLeft, steerSpeed * Time.deltaTime);
        // wheelAngleRight = Mathf.Lerp(wheelAngleRight, steerAngleRight, steerSpeed * Time.deltaTime);
        wheelAngleLeft=steerAngleLeft;
        wheelAngleRight=steerAngleRight;


        wheelObjects[0].transform.localRotation = Quaternion.Euler(
            wheelObjects[0].transform.localRotation.x, 
            wheelObjects[0].transform.localRotation.y + wheelAngleLeft,
            wheelObjects[0].transform.localRotation.z );
        
        wheelObjects[1].transform.localRotation = Quaternion.Euler(
            wheelObjects[1].transform.localRotation.x, 
            wheelObjects[1].transform.localRotation.y + wheelAngleRight,
            wheelObjects[1].transform.localRotation.z );

    }


    // Use these for the UI:
    public Suspension[] getSuspensions(){
        return suspensions;
    }

    public Wheel[] getWheels(){
        return wheels;
    }
   
    public float getSteeringAngleL(){return steerAngleLeft;}
    public float getSteeringAngleR(){return steerAngleRight;}
    public float getAccel(){return userInput;}

    public float getEngineRPM(){return engineRPM;}
    public int getCurrentGear(){return currentGear;}
    public bool checkIfAcclerating(){return isAcclerating;}


    public float getAntiRollForce(Suspension leftSuspension, Suspension rightSuspension, float antiRollStiffness, float wheelId){

        float travelLeft = 0;
        float travelRight = 0;

        if(leftSuspension.contact){
            travelLeft = (leftSuspension.springLength)/leftSuspension.maxLength;
        }

        if(rightSuspension.contact){
            travelRight = (rightSuspension.springLength)/rightSuspension.maxLength;
        }        
        

        float forceLeft = (travelLeft - travelRight) * antiRollStiffness;
        float forceRight = -forceLeft;

        if(wheelId == 2){
            return forceLeft;
        }
        else if (wheelId == 3){
            return forceRight;
        }
        else{
            return 0;
        }



        
        

    }

   

}

