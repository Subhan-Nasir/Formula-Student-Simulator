using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
// Code adapted from: https://www.youtube.com/watch?v=x0LUiE0dxP0 

public class RaycastController : MonoBehaviour{

    [Header("Controls Selection")]
    public bool usingKeyboard;


    [Tooltip("Times how long it takes to get to a given speed (in m/s)")]
    [Header("Speed Timer")]    
    public bool enableTimer;
    public float targetSpeed;
    public enum SpeedUnits{MetersPerSecond,KilometersPerHour,MilesPerHour};
    public SpeedUnits targetSpeedUnits; 

    
    [Header("Tokyo Drift Mode")]
    public bool tokyoDriftMode=false;

    [Header("Car Components ")]
    public Rigidbody rb;    
    public List<GameObject> springs;
    public List<GameObject> wheelObjects;
    public List<GameObject> meshes;

    public GameObject FLHub;
    public GameObject FRHub;
    // public static RaycastController cc;
    
    [Header("Engine")]
    public AnimationCurve engineCurve;

    [Header("Transmission")]
    public List<float> gearRatios;
    public float primaryGearRatio;
    public float finalDriveRatio;
    public float idleRPM = 1650;
    public float maxRPM = 14000;
    public float auxillaryLoss = 0.15f;
    public float maxEngineBrakingTorque = 5;
    public float drivetrainInertia = 1.5f;


    private int currentGear = 1;
    private float engineRPM;
    private float shiftUp;
    private float shiftDown;
    private float engineTorque;
    private float wheelTorque;
    private float engineBraking;

    [Header("Differential")]
    public float TBR = 2.9f; 
    public float diffPreLoadTorque = 10;
    

    

    private float diffHousingTorque; 
    private float differentialTorque;
    private float lockingCoefficient;
    private float criticalHousingTorque;




    [Header("Centre of mass")]    
    public GameObject COM_Finder;

    [Header("Suspension Settings")]
    // public float naturalLength;
    // public float springTravel;
    // public float springStiffness;    
    // public float dampingCoefficient;

    public float frontNaturalLength;
    public float frontSpringTravel;
    public float frontSpringStiffness;
    public float frontDampingCoefficient;

    [Header(" ")]
    public float rearNaturalLength;
    public float rearSpringTravel;
    public float rearSpringStiffness;
    public float rearDampingCoefficient;

    

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
    public float maxBrakingTorque = 1000;
    public float tyreEfficiency = 0.7f;  
    public float toeAngleFront = 1f;
    public float toeAngleRear = -1f;
       
    
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
    public float parallelSteerAngle = 20f;
    public float steerSpeed = 10f;

    private float wheel_x;
    private float wheel_y;
    private float wheel_z;
    
    private float steerInput;
    private float wheelAngleLeft;
    private float wheelAngleRight;
    private float steerAngleLeft;
    private float steerAngleRight;

    
    [Header("Anti-Ackermann Steering")]
    public bool enableAntiAckermann = true;
    public float innerSteerAngle = 19;
    public float outerSteerAngle = 23;

    [Header("Load transfer")]
    public float wheelBase = 1.5f;
    public float rearTrack = 1.085f;
    // public float turnRadius = 3.14f;

    [Header("Rack Travel (mm)")]   
    public float maxRackTravel = 40;
    private float rackTravel;


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

    private float lateralLoadTransferFront;
    private float lateralLoadTransferRear;
    
    private float rollCentreHeightFront; // 0.030602
    private float rollCentreHeightRear; // 0.060289

    private float rollStiffnessFront;
    private float rollStiffnessRear;

    private float trackFront;
    private float trackRear;

    private float loadFront;
    private float loadRear;

    private float baseLoadFront;
    private float baseLoadRear;

    private float[] wheelVerticalLoad = new float[4];

    private float COMlongitudinalVelocity;
    private float COMlongitudinalVelocityPrevious;
    private float COMlongitudinalAcceleration;

    private float wheelRateFL;
    private float wheelRateFR;
    private float wheelRateRL;
    private float wheelRateRR;

    private float[] wheelTravels = new float[4];

    // private float velocitySum = 0;

    // front: 1.055
    // rear: 1.101
    

    private History<Vector3> FLHistory = new History<Vector3>(3);
    private History<Vector3> FRHistory = new History<Vector3>(3);

    private float totalLateralLoadTransferMeasured;
    private float totalLateralLoadTransferTheoretical;

    private float rollAngleFront;
    private float rollAngleRear;

    private Vector3[] currentPosition = new Vector3[4];
    private Vector3[] previousPosition = new Vector3[4];
    private Vector3[] globalVelocity = new Vector3[4];
    private Vector3[] localVelocity = new Vector3[4];
    private float[] lateralVelocity = new float[4];
    
    private Vector3 fLcurrentPosition;
    private Vector3 fLpreviousPosition;
    private Vector3 fLglobalVelocity;
    private Vector3 fLlocalVelocity;
    private float fLlateralVelocity;

    private Vector3 fRcurrentPosition;
    private Vector3 fRpreviousPosition;
    private Vector3 fRglobalVelocity;
    private Vector3 fRlocalVelocity;
    private float fRlateralVelocity;

    private Vector3 rLcurrentPosition;
    private Vector3 rLpreviousPosition;
    private Vector3 rLglobalVelocity;
    private Vector3 rLlocalVelocity;
    private float rLlateralVelocity;

    private Vector3 rRcurrentPosition;
    private Vector3 rRpreviousPosition;
    private Vector3 rRglobalVelocity;
    private Vector3 rRlocalVelocity;
    private float rRlateralVelocity;

    private History<float> lateralVelocityHisotry = new History<float>(5);
    private float lateralV;
    private float counter;

    private Vector3 velocityVector;
    private Vector3 previousVelocityVector;
    private Vector3 accelerationVector;

    private float previousLongitudinalLoadTransfer;

    private bool gearDelayOn = false;
    private float gearDelayTimer = 0;

    private bool lockedDiff;

    private NotificationTriggerEvent notification;

    private float maxDamperForce;
    private float maxSpringForce;
    void OnValidate(){
        keys = new NewControls();
        rb.centerOfMass = COM_Finder.transform.localPosition;
        
             
                           
        
        for (int i = 0; i < 4; i++){
            
            // suspensions[i] = new Suspension(i, naturalLength, springTravel, springStiffness, dampingCoefficient, bumpStiffness, bumpTravel, wheelRadius);                     
            wheels[i] = new Wheel(i, wheelObjects[i], meshes[i], rb, wheelRadius, wheelMass, brakeBias, drivetrainInertia,idleRPM, maxRPM, tyreEfficiency, maxBrakingTorque, longitudinalConstants, lateralConstants);
            
            if(i == 0| i == 1){
                suspensions[i] = new Suspension(i, frontNaturalLength, frontSpringTravel, frontSpringStiffness, frontDampingCoefficient, bumpStiffness, bumpTravel, wheelRadius);
            }
            else{
                suspensions[i] = new Suspension(i, rearNaturalLength, rearSpringTravel, rearSpringStiffness, rearDampingCoefficient, bumpStiffness, bumpTravel, wheelRadius);

            }


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
        // rb.inertiaTensor = new Vector3(123.1586f,61.15857f,112f);
        // rb.inertiaTensor = new Vector3(250,250,75);


        // // // rb.inertiaTensorRotation = Quaternion.Euler(33.5407f,0,0);
        // rb.inertiaTensorRotation = Quaternion.Euler(0,0,0);
        // x, y, z >>> right, up, forward
        // x, z, y,
        // rb.inertiaTensor = new Vector3(276.4f, 346.2f, 76.94f);
        // rb.inertiaTensorRotation = Quaternion.Euler(219.8f, 289.6f, 216.7f);

        // rb.inertiaTensor = new Vector3 (350, 350, 100);
        // rb.inertiaTensorRotation =  Quaternion.Euler(350,350,350);

        // INERTIA TENSOR:
        // Fusion:        left,      backwards,   up         (X, Y, Z)
        // XX, YY, ZZ  =  1.606e2,   3.168e1,     1.752e2
        // XY, XZ, YZ  =  1.262,     2.116e-1,    3.529
        
        // Unity:                       right,      up,         forwards (x,y,z)                            
        // tensor -->    XX, ZZ, YY  =  1.606e2,    1.752e2,    3.168e1
        // rotation -->  XZ, XY, YZ  =  2.116e-1,   1.262,      3.529

        rb.inertiaTensor = new Vector3(1.606E2f, 1.752E2f, 3.168E1f);
        rb.inertiaTensorRotation = Quaternion.Euler(2.116E-1f, 1.262f, 3.529f);




        COM_height = COM_Finder.transform.position.y - transform.position.y;


        massFront = rb.mass *  Mathf.Abs((COM_Finder.transform.position.z - springs[2].transform.position.z)/(springs[0].transform.position.z - springs[2].transform.position.z));
        massRear = rb.mass *  Mathf.Abs((COM_Finder.transform.position.z - springs[0].transform.position.z)/(springs[0].transform.position.z - springs[2].transform.position.z));

        trackFront = Mathf.Abs(wheelObjects[1].transform.position.x - wheelObjects[0].transform.position.x);
        trackRear = Mathf.Abs(wheelObjects[3].transform.position.x - wheelObjects[2].transform.position.x);

        rollStiffnessFront = Mathf.Pow(trackFront,2) * (frontSpringStiffness)/(360/Mathf.PI);
        rollStiffnessRear =  Mathf.Pow(trackRear,2) * (rearSpringStiffness)/(360/Mathf.PI);

        baseLoadFront = massFront * 9.81f/2;
        baseLoadRear = massRear * 9.81f/2;

        FLHistory.AddEntry(FLHub.transform.localPosition);
        FRHistory.AddEntry(FRHub.transform.localPosition);

        notification = GameObject.Find("NotificationPanel").GetComponent<NotificationTriggerEvent>(); 
    }

    void Update(){

        if(usingKeyboard){

            steerInput = keys.Track.Steering.ReadValue<float>();
            throttle = keys.Track.Throttle.ReadValue<float>();
            brake = keys.Track.Brake.ReadValue<float>();

            steerInput = Mathf.Clamp(steerInput, -1,1);
            throttle = Mathf.Clamp(throttle, 0,1);
            brake = Mathf.Clamp(brake, 0,1);

            brakeBiasUp = keys.Track.BrakeBiasUp.ReadValue<float>();
            brakeBiasDown = keys.Track.BrakeBiasDown.ReadValue<float>();
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
            gearDelayOn = true;
            
                     
        }
        else if(shiftDown == 1 & gearTimer > 0.2f){
            currentGear -= 1;
            gearTimer = 0;
            gearDelayOn = true;
            
            
                       
        }
        else{
            currentGear += 0;
        }

        if(brakeBiasUp > 0 & brakeBiasTimer > 0.2f){
            brakeBias += 0.05f;
            brakeBiasTimer = 0;
            brakeBias = Mathf.Clamp(brakeBias, 0,1);
            notification.showNotification("Brake bias changed to " + brakeBias.ToString("0.00"));            
            
        }
        else if(brakeBiasDown > 0 & brakeBiasTimer > 0.2f){
            brakeBias -= 0.05f;
            brakeBiasTimer = 0;
            brakeBias = Mathf.Clamp(brakeBias, 0,1);            
            notification.showNotification("Brake bias changed to " + brakeBias.ToString("0.00"));
        }
      
        

        brakeBias = Mathf.Clamp(brakeBias, 0,1);
        for(int i = 0; i<4; i++){
            wheels[i].brakeBias = brakeBias;
        }

        // Debug.Log($" Brake bias = {wheels[0].brakeBias}, brakeBiasUp = {brakeBiasUp}, brakeBiasDown = {brakeBiasDown}, timer = {brakeBiasTimer}");
        gearTimer += Time.deltaTime;
        brakeBiasTimer += Time.deltaTime;

        if(gearDelayOn == true){
            gearDelayTimer += Time.deltaTime;
            userInput = -brake; // allows brake but no throttle
        }

        if(gearDelayTimer >= 0.2f){
            gearDelayOn = false;
            gearDelayTimer = 0;
        }


        currentGear = Mathf.Clamp(currentGear, 1,5);


        
        engineRPM = Mathf.Clamp(engineRPM, idleRPM, maxRPM);

        engineTorque = (1-auxillaryLoss) * (engineCurve.Evaluate(engineRPM) * userInput);
        engineBraking = maxEngineBrakingTorque * (1 - userInput);      
       

        ApplySteering();     
    }

    void FixedUpdate(){

        Vector3 FLposition = FLHub.transform.localPosition;
        Vector3 FRposition = FRHub.transform.localPosition;

        updateWheelTravels();
        updateRackTravel();
        updateWheelRates();
        updateRollStiffnesses();
        updateRollAngles();
        updateLoadTransfers();
        updateRollcentreHeights();
        updateCOMaccleerations();
        updateVerticalLoad();
        // calculateDiffTorques();

        wheels[2].wheelTorque = 0.5f*(engineTorque - engineBraking) * gearRatios[currentGear + 1] *primaryGearRatio * finalDriveRatio;
        wheels[3].wheelTorque = 0.5f*(engineTorque - engineBraking) * gearRatios[currentGear + 1] *primaryGearRatio * finalDriveRatio;       
            
        
        
        for(int i = 0; i<springs.Count; i++){   

            bool contact = Physics.Raycast(springs[i].transform.position, -transform.up, out RaycastHit hit, suspensions[i].naturalLength + suspensions[i].springTravel + wheelRadius);
            // Debug.Log($" wheel id {i}: vertical load = {wheelVerticalLoad}, lateral acceleration = {COMLateralAcceleration}");

            if(contact){            
                
                // Force vectors from suspension, wheel and anti rollbars.
                Vector3 suspensionForceVector = suspensions[i].getUpdatedForce(hit, Time.fixedDeltaTime, contact);          
                Vector3 wheelForceVector = wheels[i].getUpdatedForce(userInput, gearRatios[currentGear + 1], finalDriveRatio, primaryGearRatio, hit, Time.fixedDeltaTime, wheelVerticalLoad[i]);            
                Vector3 antiRollForceVector = getAntiRollForce(suspensions[2], suspensions[3], antiRollStiffness, i) * hit.normal;

                rb.AddForceAtPosition(wheelForceVector +suspensionForceVector, hit.point + new Vector3 (0,0.44f,0)); 
                
                float averageRearRPM = (9.5493f)*(wheels[2].omega + wheels[3].omega)/2;
                if(currentGear != 0){
                    engineRPM = averageRearRPM * (gearRatios[currentGear + 1] * primaryGearRatio * finalDriveRatio);
                }

                engineRPM = Mathf.Clamp(engineRPM, idleRPM, 14000);

                // Debug.Log($"Engine RPM = {engineRPM}, Engine Torque = {engineTorque}, Current Gear = {currentGear}, User Input = {userInput}");
                                
            }
            else{
                suspensions[i].contact = false;
            }

            if(suspensions[i].damperForce > maxDamperForce){
                maxDamperForce = suspensions[i].damperForce;
            }
            if(suspensions[i].springForce > maxSpringForce){
                maxSpringForce = suspensions[i].springForce;
            }

            // Debug.Log($"ID: {i}, max spring force = {maxSpringForce}, max damper force = {maxDamperForce}");
        }

        showTimer();
        
        previousSpeed = speed;
        speed = rb.velocity.magnitude;

        if(speed >= previousSpeed){isAcclerating = true;}

        drag = (0.39f * 1.225f * Mathf.Pow(speed,2) * 0.947f)/2;
        lift = (0.17f * 1.225f * Mathf.Pow(speed,2) * 0.947f)/2;
        // Debug.Log($" Drag = {drag}, Lift = {lift}");

        // rb.AddForceAtPosition( -drag*transform.forward, COM_Fidner.transform.position);
        // rb.AddForceAtPosition( lift*transform.up, COM_Fidner.transform.position);
        FL_LateralForce = wheels[0].lateralForce;
        RL_LateralForce = wheels[3].lateralForce;
        
       
        


    }

    
    void updateWheelTravels(){        
        for(int i = 0; i<4; i++){
            wheelTravels[i] = 1000 * (suspensions[i].naturalLength - suspensions[i].springLength); // mm
        }
    }

    void updateRollcentreHeights(){
        

        float rollcentreFL = 29.05f - 0.03924f*(wheelTravels[0]) + 0.003029f*rackTravel - 0.0006878f*Mathf.Pow(wheelTravels[0],2) + 0.000353f*wheelTravels[0]*rackTravel - 0.0000917f*Mathf.Pow(rackTravel,2) + 8.601E-6f*Mathf.Pow(wheelTravels[0],3) + 1.04E-6f*(Mathf.Pow(wheelTravels[0],2))*rackTravel + 6.995E-6f*(wheelTravels[0]*Mathf.Pow(rackTravel,2)); 
        float rollcentreFR =  29.05f - 0.03924f*(wheelTravels[1]) + 0.003029f*rackTravel - 0.0006878f*Mathf.Pow(wheelTravels[1],2) + 0.000353f*wheelTravels[1]*rackTravel - 0.0000917f*Mathf.Pow(rackTravel,2) + 8.601E-6f*Mathf.Pow(wheelTravels[1],3) + 1.04E-6f*(Mathf.Pow(wheelTravels[1],2))*rackTravel + 6.995E-6f*(wheelTravels[1]*Mathf.Pow(rackTravel,2));
        float rollcentreRL = -0.0005f * Mathf.Pow(wheelTravels[2],2) - 1.2702f * wheelTravels[2] + 69.395f;
        float rollcentreRR = -0.0005f * Mathf.Pow(wheelTravels[3],2) - 1.2702f * wheelTravels[3] + 69.395f;
        
        rollCentreHeightFront = (rollcentreFL + rollcentreFR)/2000; // average and conversion to metres 
        rollCentreHeightRear = (rollcentreRL + rollcentreRR)/2000; // average and conversion to metres 

    }

    void updateWheelRates(){
        
        wheelRateFL = 1000*(47.15f + 0.02513f*wheelTravels[0] + 0.003504f*rackTravel + 0.0008393f*Mathf.Pow(wheelTravels[0],2) + 0.0002012f*wheelTravels[0]*rackTravel + 0.0001072f*Mathf.Pow(rackTravel,2) - 4.053E-5f*Mathf.Pow(wheelTravels[0],3) - 5.693E-6f*Mathf.Pow(wheelTravels[0],2)*rackTravel + 5.118E-6f*wheelTravels[0]*Mathf.Pow(rackTravel,2)); 
        wheelRateFR = 1000*(47.15f + 0.02513f*wheelTravels[1] + 0.003504f*rackTravel + 0.0008393f*Mathf.Pow(wheelTravels[1],2) + 0.0002012f*wheelTravels[1]*rackTravel + 0.0001072f*Mathf.Pow(rackTravel,2) - 4.053E-5f*Mathf.Pow(wheelTravels[1],3) - 5.693E-6f*Mathf.Pow(wheelTravels[1],2)*rackTravel + 5.118E-6f*wheelTravels[1]*Mathf.Pow(rackTravel,2)); 
        wheelRateRL = 1000*(-4E-5f*Mathf.Pow(wheelTravels[2] ,3) + 0.0009f*Mathf.Pow(wheelTravels[2] ,2) - 0.029f*wheelTravels[2] + 41.945f);
        wheelRateRR = 1000*(-4E-5f*Mathf.Pow(wheelTravels[3] ,3) + 0.0009f*Mathf.Pow(wheelTravels[3] ,2) - 0.029f*wheelTravels[3] + 41.945f);

        suspensions[0].springStiffness = wheelRateFL;
        suspensions[0].dampingCoefficient = wheelRateFL/10;
        
        suspensions[1].springStiffness = wheelRateFR;
        suspensions[1].dampingCoefficient = wheelRateFR/10;
        
        suspensions[2].springStiffness = wheelRateRL;
        suspensions[2].dampingCoefficient = wheelRateRL/10;

        suspensions[3].springStiffness = wheelRateRR;
        suspensions[3].dampingCoefficient = wheelRateRR/10;
        
    } 


    void updateCOMaccleerations(){       


        // COM_height = 0.252f;
        COM_height = COM_Finder.transform.position.y - transform.position.y;

        // COMLateralVelocityPrevious = COMLateralVelocity;
        // COMlongitudinalVelocityPrevious = COMlongitudinalVelocity;

        // // COMLateralVelocity = COM_Finder.transform.InverseTransformDirection(rb.GetPointVelocity(COM_Finder.transform.position)).x;
        // // COMLateralVelocity = Mathf.Sign(steerInput) * Vector3.Project(rb.velocity, COM_Finder.transform.right.normalized).magnitude;
        // // COMLateralVelocity = transform.InverseTransformDirection(rb.velocity).x;         
        // // COMLateralVelocity = -Vector3.Dot(rb.velocity, transform.right.normalized);

        // // Vector3 temp = new Vector3(-rb.velocity.x, rb.velocity.y, rb.velocity.z);
        // // Vector3 rotatedVelocity = Quaternion.LookRotation(transform.right) * temp;
        // // COMLateralVelocity = rotatedVelocity.z;

        // COMlongitudinalVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(COM_Finder.transform.position)).z;

        // // fLcurrentPosition = wheels[0].wheelObject.transform.position;
        // // fLglobalVelocity = (fLcurrentPosition - fLpreviousPosition)/Time.fixedDeltaTime;
        // // float fLlateralVelocity = Vector3.Dot(fLglobalVelocity, wheels[0].wheelObject.transform.right);
        // // fLpreviousPosition = fLcurrentPosition;
        // // // Debug.Log(fLlateralVelocity);

        // // fRcurrentPosition = wheels[1].wheelObject.transform.position;
        // // fRglobalVelocity = (fRcurrentPosition - fRpreviousPosition)/Time.fixedDeltaTime;
        // // float fRlateralVelocity = Vector3.Dot(fRglobalVelocity, wheels[1].wheelObject.transform.right);
        // // fRpreviousPosition = fRcurrentPosition;
        // // // Debug.Log(fLlateralVelocity);

        // // rLcurrentPosition = wheels[2].wheelObject.transform.position;
        // // rLglobalVelocity = (rLcurrentPosition - rLpreviousPosition)/Time.fixedDeltaTime;
        // // float rLlateralVelocity = Vector3.Dot(rLglobalVelocity, wheels[2].wheelObject.transform.right);
        // // rLpreviousPosition = rLcurrentPosition;
        // // // Debug.Log(fLlateralVelocityrR

        // // rRcurrentPosition = wheels[3].wheelObject.transform.position;
        // // rRglobalVelocity = (rRcurrentPosition - rRpreviousPosition)/Time.fixedDeltaTime;
        // // float rRlateralVelocity = Vector3.Dot(rRglobalVelocity, wheels[3].wheelObject.transform.right);
        // // rRpreviousPosition = rRcurrentPosition;
        // // // Debug.Log(fLlateralVelocity);
        // // ///...
        // // COMLateralVelocity = -(fLlateralVelocity + fRlateralVelocity + rLlateralVelocity + rRlateralVelocity)/4;
        // // lateralVelocityHisotry.AddEntry(COMLateralVelocity);
        

        // // for(int i = 0; i<4; i++){
        // //     currentPosition[i] = wheels[i].wheelObject.transform.position;
        // //     globalVelocity[i] = (currentPosition[i] - previousPosition[i])/Time.fixedDeltaTime;
        // //     lateralVelocity[i] = Vector3.Dot(globalVelocity[i], wheels[3].wheelObject.transform.right);
        // //     previousPosition[i] = currentPosition[i];
        // // }
        
        // // float sum = 0;
        // // for( var i = 0; i < 4; i++) {
        // //     sum += lateralVelocity[i];
        // // }
        // // float average = sum / 4;

        // // Debug.Log($"Average = {average}");
        // // COMLateralVelocity = average;



        // // Debug.Log($"lateral velocities = {lateralVelocity[0]}, {lateralVelocity[1]}, {lateralVelocity[2]}, {lateralVelocity[3]}, average = {average}");
        // // Debug.Log($"position = {currentPosition}");

        

                
       
        // // COMLateralVelocity = rb.GetRelativePointVelocity(COM_Finder.transform.position).x;
        // // COMlongitudinalVelocity = rb.GetRelativePointVelocity(COM_Finder.transform.position).z;

        // // velocitySum = 0;
        // // for(int i = 0; i<4; i++){
        // //     velocitySum += wheels[i].lateralVelocity;

        // // }

        // // COMLateralVelocity = velocitySum/4;
     
        // // COMLateralVelocity = Vector3.Dot(rb.velocity, transform.right);
        // // COMlongitudinalVelocity = Vector3.Dot(rb.velocity, transform.forward);

        // // COMLateralAcceleration = (COMLateralVelocity - COMLateralVelocityPrevious)/Time.fixedDeltaTime;
        // // COMLateralAcceleration = Mathf.Clamp(COMLateralAcceleration, -5,5);
        // // float[] lateralVelocities = lateralVelocityHisotry.getArray();
        // // // Debug.Log(lateralVelocities.Length);

        // // if(Time.realtimeSinceStartup <= 1){
        // //     COMLateralAcceleration = 0;
        // // }
        // // else{
        // //     COMLateralAcceleration = (lateralVelocities[4] - lateralVelocities[0])/(4*Time.fixedDeltaTime);
        // //     // COMLateralAcceleration = 0;
        // // }

        
        
        
        
        // COMlongitudinalAcceleration = (COMlongitudinalVelocity -COMlongitudinalVelocityPrevious)/Time.fixedDeltaTime;
        // // Debug.Log($"Lateral velocity = {COMLateralVelocity}");
        


        // // Debug.Log($"acceleration = {COMLateralAcceleration}");
        // // Debug.Log($"{lateralVelocities[0]}, {lateralVelocities[1]} ,{lateralVelocities[2]} ,{lateralVelocities[3]}, {lateralVelocities[4]}, ");

        // // if(counter == 5){
        // //     counter = 0;
        // //     COMLateralAcceleration = (COMLateralVelocity - lateralV)/4*Time.fixedDeltaTime;
        // //     lateralV = COMLateralVelocity;
        // // }
        // // else{
        // //     counter += 1;            
        // // }

        // // Debug.Log($"lateral velocity = {COMLateralVelocity}, acceleration = {COMLateralAcceleration} ");


        velocityVector = rb.GetPointVelocity(COM_Finder.transform.position);
        accelerationVector = (velocityVector - previousVelocityVector)/Time.fixedDeltaTime;        

        // COMLateralVelocity = Vector3.Dot(velocityVector, transform.right.normalized);
        // COMlongitudinalVelocity = Vector3.Dot(velocityVector, transform.right.normalized);
        
        COMLateralAcceleration = Vector3.Dot(accelerationVector, transform.right.normalized);
        COMlongitudinalAcceleration = Vector3.Dot(accelerationVector, transform.forward.normalized);


        previousVelocityVector = velocityVector;

        // Debug.Log($"Longitudinal acceleration = {COMlongitudinalAcceleration}, lateral = {COMLateralAcceleration}");

        // Debug.Log($"Acceleration vector = ({accelerationVector.x}, {accelerationVector.y}, {accelerationVector.z})");
        

    }

    void updateRollAngles(){
        rollAngleFront = -Mathf.Rad2Deg * Mathf.Atan((wheelTravels[0] - wheelTravels[1])/1000*trackFront);
        rollAngleRear =  -Mathf.Rad2Deg * Mathf.Atan((wheelTravels[2] - wheelTravels[3])/1000*trackRear);
    }

    void updateLoadTransfers(){

        // elasticLoadTransferFront = Suspension.elasticLoadTransferFront(
        //     rollStiffnessFront,
        //     rollStiffnessRear,
        //     massFront,
        //     massRear,
        //     COM_height,
        //     rollCentreHeightFront,             
        //     rollCentreHeightRear,
        //     COMLateralAcceleration, 
        //     trackFront
        // );

        // elasticLoadTransferRear = Suspension.elasticLoadTransferRear(
        //     rollStiffnessFront, 
        //     rollStiffnessRear,
        //     massFront, 
        //     massRear, 
        //     COM_height, 
        //     rollCentreHeightFront, 
        //     rollCentreHeightRear, 
        //     COMLateralAcceleration, 
        //     trackRear
        // );

        elasticLoadTransferFront = Suspension.transientElasticLoadTransfer(rollStiffnessFront, rollAngleFront, trackFront);
        elasticLoadTransferRear = Suspension.transientElasticLoadTransfer(rollStiffnessRear, rollAngleRear, trackRear);

        geometricLoadTransferFront = Suspension.geometricLoadTransferFront(massFront, COMLateralAcceleration, rollCentreHeightFront, trackFront);
        geometricLoadTransferRear = Suspension.geometricLoadTransferRear(massRear, COMLateralAcceleration, rollCentreHeightRear, trackRear);
        
        longitudnialLoadTransfer = Suspension.LongitudinalLoadTransfer(rb.mass, COMlongitudinalAcceleration, COM_height, wheelBase);
        // longitudnialLoadTransfer = Mathf.Lerp(previousLongitudinalLoadTransfer, longitudnialLoadTransfer, 0.002f);

        lateralLoadTransferFront = ( elasticLoadTransferFront + geometricLoadTransferFront);
        lateralLoadTransferRear = ( elasticLoadTransferRear + geometricLoadTransferRear);

        totalLateralLoadTransferMeasured = lateralLoadTransferFront + lateralLoadTransferRear;
        totalLateralLoadTransferTheoretical = (rb.mass * COMLateralAcceleration * COM_height)/(0.5f*(trackFront + trackRear));

        // Debug.Log($"theoretical = {totalLateralLoadTransferTheoretical}, measured = {totalLateralLoadTransferMeasured}");

        previousLongitudinalLoadTransfer = longitudnialLoadTransfer;
        
    }

    void updateVerticalLoad(){
        float verticalLoad;
        for(int i = 0; i<4; i++){
            if(Mathf.Abs(COMLateralAcceleration) >= 0f){
                if(i == 0){
                    verticalLoad = baseLoadFront - lateralLoadTransferFront - longitudnialLoadTransfer;
                }
                else if( i == 1){
                    verticalLoad = baseLoadFront + lateralLoadTransferFront - longitudnialLoadTransfer;
                }
                else if(i == 2){
                    verticalLoad = baseLoadRear - lateralLoadTransferRear + longitudnialLoadTransfer;
                }
                else{
                    verticalLoad = baseLoadRear + lateralLoadTransferRear + longitudnialLoadTransfer;
                }

            }
            else{
                if(i == 0| i == 1){verticalLoad = baseLoadFront;}
                else{ verticalLoad = baseLoadRear;}
            }

            wheelVerticalLoad[i] = verticalLoad;
        }
    }



    void updateRollStiffnesses(){
        rollStiffnessFront = Mathf.Pow(trackFront,2) * 0.5f*(wheelRateFL + wheelRateFR)/(360/Mathf.PI);
        rollStiffnessRear =  Mathf.Pow(trackRear,2) *  0.5f*(wheelRateRL + wheelRateRR)/(360/Mathf.PI);
    }

    // void calculateDiffTorques(){


    //     diffHousingTorque = wheels[2].feedbackTorque + wheels[3].feedbackTorque;
    //     differentialTorque = wheels[2].feedbackTorque - wheels[3].feedbackTorque;
    //     lockingCoefficient = (TBR - 1)/(TBR+1);
    //     criticalHousingTorque = diffPreLoadTorque/lockingCoefficient;

    //     float feedbackRL= wheels[2].feedbackTorque;
    //     float feedbackRR = wheels[3].feedbackTorque;

    //     if(Mathf.Abs(diffHousingTorque) <= criticalHousingTorque){
    //         // Regime 1
    //         // Debug.Log("REGIME 1");
    //         if( Mathf.Abs(feedbackRL) <= Mathf.Abs(feedbackRR)){
    //             wheels[3].diffLongForceLimit = (feedbackRL + Mathf.Sign(feedbackRL)*diffPreLoadTorque - wheels[3].rollingResistance - wheels[3].brakingTorque)/-wheelRadius;
    //             wheels[2].diffLongForceLimit = 10000000;
    //         }
    //         else{
    //             wheels[2].diffLongForceLimit = (feedbackRR + Mathf.Sign(feedbackRR)*diffPreLoadTorque - wheels[2].rollingResistance - wheels[2].brakingTorque)/-wheelRadius;
    //             wheels[3].diffLongForceLimit = 100000000;
    //         }

    //         // if(differentialTorque >= 0.9*diffPreLoadTorque){
    //         //     lockedDiff = false;       
    //         // }
    //         // else{
    //         //     lockedDiff = true;

    //         // }

    //         lockedDiff = Mathf.Abs(differentialTorque) <= 0.5*diffPreLoadTorque;
    //         // Debug.Log($"Differential torque = {differentialTorque}");
    //     }
    //     else{
    //         // Regime 2
    //         // Debug.Log("REGIME 2");
    //         if( Mathf.Abs(feedbackRL) <=  Mathf.Abs(feedbackRR)){
    //             wheels[3].diffLongForceLimit = (feedbackRL *  TBR - wheels[3].rollingResistance - wheels[3].brakingTorque)/-wheelRadius;
    //             wheels[2].diffLongForceLimit = 1000000;
    //         }
    //         else{
    //             wheels[2].diffLongForceLimit = (feedbackRR *  TBR - wheels[2].rollingResistance - wheels[2].brakingTorque)/-wheelRadius;
    //             wheels[3].diffLongForceLimit = 1000000;
    //         }

    //         // if( Mathf.Max(feedbackRL/feedbackRR, feedbackRR/feedbackRL) >= 0.9*TBR){
    //         //    lockedDiff = false;
    //         // }
    //         // else{
    //         //     lockedDiff = true;
    //         // }

    //         lockedDiff = Mathf.Max(Mathf.Abs(feedbackRL/feedbackRR), Mathf.Abs(feedbackRR/feedbackRL)) <= 0.5*TBR;
    //         // Debug.Log($"Max ratio = {Mathf.Max(Mathf.Abs(feedbackRL/feedbackRR), Mathf.Abs(feedbackRR/feedbackRL))}");

    //     }

    //     if(lockedDiff == false){
    //         wheels[2].wheelTorque = 0.5f*(engineTorque - engineBraking) * gearRatios[currentGear + 1] *primaryGearRatio * finalDriveRatio;
    //         wheels[3].wheelTorque = 0.5f*(engineTorque - engineBraking) * gearRatios[currentGear + 1] *primaryGearRatio * finalDriveRatio;       
            
    //         wheels[2].lockedDiff = false;
    //         wheels[3].lockedDiff = false; 

    //     }
    //     else{

    //         float previousOmega = 0.5f*(wheels[2].omega + wheels[3].omega);
    //         float lockedAxleTorque = (engineTorque - engineBraking) * gearRatios[currentGear + 1] *primaryGearRatio * finalDriveRatio + feedbackRL + feedbackRR;
    //         float lockedAxleInertia = drivetrainInertia + wheels[2].momentOfInertia + wheels[3].momentOfInertia;
    //         float lockedAxleAlpha =  lockedAxleTorque / lockedAxleInertia;
    //         float lockedAxleOmega = previousOmega + lockedAxleAlpha*Time.fixedDeltaTime;


    //         wheels[2].lockedDiff = true;
    //         wheels[3].lockedDiff = true;

    //         wheels[2].diffControlledOmega = lockedAxleOmega;
    //         wheels[3].diffControlledOmega = lockedAxleOmega;

            

    //     }


    //     Debug.Log($"Diff housing torque = {Mathf.Abs(diffHousingTorque)}, critical = {criticalHousingTorque}");

    //     // Debug.Log($"Locked diff = {lockedDiff}, differential torque = {differentialTorque}, Max Ratio = {Mathf.Max(Mathf.Abs(feedbackRL/feedbackRR), Mathf.Abs(feedbackRR/feedbackRL))}");


    


    // }


    void showTimer(){
        float carSpeed = rb.velocity.z;
        float speedThreshold; // targetSpeed converted to m/s

        if(targetSpeedUnits == SpeedUnits.MetersPerSecond){ 
            speedThreshold = targetSpeed; //no conversion needed
        }
        else if(targetSpeedUnits == SpeedUnits.KilometersPerHour){
            speedThreshold = 0.277778f*targetSpeed; // KMH to m/s
        }
        else{
            speedThreshold = 0.44704f*targetSpeed; // MPH to m/s
        }


        if(carSpeed > 0 & speedReached == false){
            timerOn = true;
        }

        if(timerOn == true){
            theTime += Time.fixedDeltaTime;
            
            if(carSpeed >= speedThreshold){
                speedReached = true;
                timerOn = false;

                if(enableTimer == true){
                    Debug.Log($"SPEED REACHED: {carSpeed} m/s,  {carSpeed*2.23694f} mph, TIMER: {theTime}");
                }
                
            }
        }

        if(enableTimer == true){
            Debug.Log($"Timer = {theTime}, speed = {carSpeed} m/s, {carSpeed*2.23694f} mph");
        }
    }

    void OnDrawGizmos(){

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere( transform.TransformPoint(rb.centerOfMass),0.1f);
        Gizmos.DrawWireSphere( transform.position,0.1f);
        
        for(int i = 0; i < springs.Count; i++){
        
                        
            Gizmos.color = Color.blue;
            Ray ray = new Ray(springs[i].transform.position, -transform.up);           
            Gizmos.DrawLine(ray.origin, -suspensions[i].springLength * transform.up + springs[i].transform.position);

            Gizmos.color = Color.white;
            Ray ray2 = new Ray(springs[i].transform.position, -transform.up);           
            Gizmos.DrawLine(ray2.origin, -suspensions[i].minLength * transform.up + springs[i].transform.position);
            

            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(-suspensions[i].springLength * transform.up + springs[i].transform.position, -suspensions[i].springLength * transform.up + springs[i].transform.position + transform.up * -wheelRadius);
            
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(springs[i].transform.position - new Vector3(0, suspensions[i].springLength + wheelRadius, 0), new Vector3(0.1f, 0, 0.1f));
        
            // Gizmos.color = Color.yellow;
            // Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].longitudinalVelocity * wheels[i].wheelObject.transform.forward);
            // Gizmos.DrawRay(wheels[i].wheelObject.transform.position, -wheels[i].lateralVelocity * wheels[i].wheelObject.transform.right);

            // Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].wheelObject.transform.right * (wheels[i].lateralForce/1000));
            // Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].wheelObject.transform.forward * (wheels[i].longitudinalForce/1000));
            // Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].wheelObject.transform.up * wheels[i].verticalLoad/1000);
            // Gizmos.DrawRay(COM_Finder.transform.position, -drag * transform.forward /1000);
          
            // Gizmos.color = Color.yellow;
            // if(i == 2 | i == 3){
            //     Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].wheelObject.transform.up * antiRollForces[i-2]/1000);
            // }

            // Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].forceVector/1000 );
            
        }

        // Gizmos.color = Color.white;
        // Gizmos.DrawRay(COM_Finder.transform.position, COMlongitudinalVelocity * transform.forward);
        // Gizmos.DrawRay(COM_Finder.transform.position, COMLateralVelocity* transform.right);

      
        
        
    }

    void ApplySteering(){

        // // Applies Ackermann sterring if it is enabled.
        // if(enableAntiAckermann){
        //     //Steering right
        //     if(steerInput > 0){
        //         steerAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack/2))) * steerInput;
        //         steerAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack/2))) * steerInput;

        //     }//Steering left            
        //     else if (steerInput < 0){
        //         steerAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack/2))) * steerInput;
        //         steerAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack/2))) * steerInput;
                

        //     } // Not steering
        //     else{
        //         steerAngleLeft = 0;
        //         steerAngleRight = 0;

        //     }
            
        // }
        // // If Ackermann steering is disabled, both wheels have the same steer angle.
        // else{

        //     steerAngleLeft = steerAngle * steerInput;
        //     steerAngleRight = steerAngle * steerInput;           

        // }

        
        if(enableAntiAckermann){
            if(steerInput >= 0){
                steerAngleLeft = outerSteerAngle * steerInput;
                steerAngleRight = innerSteerAngle * steerInput;
            }
            else{
                steerAngleLeft = innerSteerAngle * steerInput;
                steerAngleRight = outerSteerAngle * steerInput;
            }
        }
        else{
            steerAngleLeft = parallelSteerAngle * steerInput;
            steerAngleRight = parallelSteerAngle * steerInput;
        }
        

        wheelAngleLeft = Mathf.Lerp(wheelAngleLeft, steerAngleLeft, steerSpeed * Time.deltaTime);
        wheelAngleRight = Mathf.Lerp(wheelAngleRight, steerAngleRight, steerSpeed * Time.deltaTime);
        wheelAngleLeft=steerAngleLeft;
        wheelAngleRight=steerAngleRight;


        wheelObjects[0].transform.localRotation = Quaternion.Euler(
            wheelObjects[0].transform.localRotation.x, 
            wheelObjects[0].transform.localRotation.y - toeAngleFront + wheelAngleLeft,
            wheelObjects[0].transform.localRotation.z );
        
        wheelObjects[1].transform.localRotation = Quaternion.Euler(
            wheelObjects[1].transform.localRotation.x, 
            wheelObjects[1].transform.localRotation.y + toeAngleFront + wheelAngleRight,
            wheelObjects[1].transform.localRotation.z );

        wheelObjects[2].transform.localRotation = Quaternion.Euler(
            wheelObjects[2].transform.localRotation.x, 
            wheelObjects[2].transform.localRotation.y - toeAngleRear,
            wheelObjects[2].transform.localRotation.z );
        
        wheelObjects[3].transform.localRotation = Quaternion.Euler(
            wheelObjects[3].transform.localRotation.x, 
            wheelObjects[3].transform.localRotation.y + toeAngleRear,
            wheelObjects[3].transform.localRotation.z );



    }

    void updateRackTravel(){
        rackTravel =  maxRackTravel * steerInput;       

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
    public float getUserInput(){return userInput;}

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

