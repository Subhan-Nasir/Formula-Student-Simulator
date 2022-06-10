using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MotionSystems;

// Code adapted from: https://www.youtube.com/watch?v=x0LUiE0dxP0&list=PLcbsEpz1iFyjjddSqLxnnGSJthfCcmsav 
// The scipt will be very different because of the added physics models but this tutorial series should give you 
// a good understanding of how raycast works for a car. 

public class RaycastController : MonoBehaviour{

    [Header("Controls Selection")]
    public bool usingKeyboard;


    [Tooltip("Times how long it takes to get to a given speed (in m/s)")]
    [Header("Speed Timer")]    
    public bool enableTimer;
    public float targetSpeed;
    public enum SpeedUnits{MetersPerSecond,KilometersPerHour,MilesPerHour};
    public SpeedUnits targetSpeedUnits; 

    
    // [Header("Tokyo Drift Mode")]
    // public bool tokyoDriftMode=false;

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


    [Tooltip("Use empty GameObject to define centre of mass")]
    [Header("Centre of mass")]    
    public GameObject COM_Finder;

    [Tooltip("Stiffness and damping will change dynamically due to double wishbone geometry")]
    [Header("Suspension Settings")]    
    public float frontNaturalLength;
    public float frontSpringTravel;
    public float frontSpringStiffness;
    public float frontDampingCoefficient;

    [Header(" ")]
    public float rearNaturalLength;
    public float rearSpringTravel;
    public float rearSpringStiffness;
    public float rearDampingCoefficient;

    
    [Tooltip("Activates higher stiffness when suspension is close to bottoming out")]
    [Header("Bump stops")]     
    public float bumpStiffness;
    public float bumpTravel;

    [Tooltip("Not currently used, was only for early versions. Kept it just in case")]
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
    public float brakeBias = 0.55f;
    public enum TyrePressureEnum {_8_PSI = 4, _10_PSI = 10, _12_PSI = 12, _14_PSI = 14};
    public TyrePressureEnum selectedTyrePressure;
    [HideInInspector]
    public int tyrePressure = 12;

    public float staticCamberAngleFront = -3.15f;
    public float staticCamberAngleRear = -1.755f;
    public float toeAngleFront = 0f;
    public float toeAngleRear = 0f;
    public float maxBrakingTorque = 1000;
    [Tooltip("Scales down tyre tyre forces. Real tyre forces are about 20%-30% lower than testing data")]
    public float tyreEfficiency = 0.7f;  
    
       
    // Old tyre constants
    // private Dictionary<string, float> lateralConstants = new Dictionary<string,float>(){
    //     {"B", 11.45f},
    //     {"C", 1.551f},
    //     {"D", 1790},
    //     {"E", 0.1859f},
    //     {"a_1_lat", 0.00151f},
    //     {"a_2_lat", 2.533E-7f}
    // };

        
    // private Dictionary<string, float> longitudinalConstants = new Dictionary<string, float>(){
    //     {"B", 11.93f},
    //     {"C", 1.716f},
    //     {"D", 1711},
    //     {"E", 0.3398f},
    //     {"a_1_long", 0.00179f},
    //     {"a_2_long", 3.62E-7f}     
    // };
    

    // private Dictionary<int, Dictionary<string, float>> lateralConstants = new Dictionary<int, Dictionary<string, float>>(){
    //     {8, new Dictionary<string, float>(){ 
    //         {"B", 45.79f},
    //         {"C", 0.04851f},
    //         {"D", 5010},
    //         {"E", 1.231f},
    //         {"a_1", 0.01512f}, 
    //         {"a_2", 2.348E-6f},
    //         {"a_3", -0.1836f},
    //         {"a_4", 0}
    //     }},

    //     {10, new Dictionary<string, float>(){
    //         {"B", 45.79f},
    //         {"C", 0.04851f},
    //         {"D", 5010},
    //         {"E", 1.231f},
    //         {"a_1", 0.01512f}, 
    //         {"a_2", 2.348E-6f},
    //         {"a_3", -0.1836f},
    //         {"a_4", 0}         
    //     }},


    //     {12, new Dictionary<string, float>(){
    //         {"B", 45.79f},
    //         {"C", 0.04851f},
    //         {"D", 5010},
    //         {"E", 1.231f},
    //         {"a_1", 0.01512f}, 
    //         {"a_2", 2.348E-6f},
    //         {"a_3", -0.1836f},
    //         {"a_4", 0}           
    //     }},


    //     {14, new Dictionary<string, float>(){
    //         {"B", 45.79f},
    //         {"C", 0.04851f},
    //         {"D", 5010},
    //         {"E", 1.231f},
    //         {"a_1", 0.01512f}, 
    //         {"a_2", 2.348E-6f},
    //         {"a_3", -0.1836f},
    //         {"a_4", 0}          
    //     }}


    // };

    // private Dictionary<int, Dictionary<string, float>> longitudinalConstants = new Dictionary<int, Dictionary<string, float>>(){
    //     {8, new Dictionary<string, float>(){ 
    //         {"B", 15f},
    //         {"C", 1.5f},
    //         {"D", 5510},
    //         {"E", 0.5002f},
    //         {"a_1", 0.0005491f},
    //         {"a_2", 1.126E-7f},
    //         {"a_3", -4.656E-5f}
    //     }},

    //     {10, new Dictionary<string, float>(){
    //         {"B", 15f},
    //         {"C", 1.5f},
    //         {"D", 5510},
    //         {"E", 0.5002f},
    //         {"a_1", 0.0005491f},
    //         {"a_2", 1.126E-7f},
    //         {"a_3", -4.656E-5f}           
    //     }},


    //     {12, new Dictionary<string, float>(){
    //         {"B", 15f},
    //         {"C", 1.5f},
    //         {"D", 5510},
    //         {"E", 0.5002f},
    //         {"a_1", 0.0005491f},
    //         {"a_2", 1.126E-7f},
    //         {"a_3", -4.656E-5f}            
    //     }},


    //     {14, new Dictionary<string, float>(){
    //         {"B", 15f},
    //         {"C", 1.5f},
    //         {"D", 5510},
    //         {"E", 0.5002f},
    //         {"a_1", 0.0005491f},
    //         {"a_2", 1.126E-7f},
    //         {"a_3", -4.656E-5f}            
    //     }}


    // };

    // Define the tyre constants here, for each pressure value.
    // Only pressures of 8, 10, 12 and 14 PSI can be selected.
    private Dictionary<int, Dictionary<string, float>> lateralConstants = new Dictionary<int, Dictionary<string, float>>(){
        
        // Lateral constants for 8 PSI.
        {8, new Dictionary<string, float>(){ 
            {"B", 45.79f},
            {"C", 0.04851f},
            {"D", 5010},
            {"E", 1.231f},
            {"a_1", 0.01512f}, 
            {"a_2", 2.348E-6f},
            {"a_3", -0.1836f},
            {"a_4", 0}
        }},
        // Lateral constants for 10 PSI.
        {10, new Dictionary<string, float>(){
            {"B", 45.79f},
            {"C", 0.04851f},
            {"D", 5010},
            {"E", 1.231f},
            {"a_1", 0.01512f}, 
            {"a_2", 2.348E-6f},
            {"a_3", -0.1836f},
            {"a_4", 0}         
        }},

        // Lateral constants for 12 PSI.
        {12, new Dictionary<string, float>(){
            {"B", 3.93293878e+01f},
            {"C", 1.52163961e+00f},
            {"D", 1.36857062e+00f},
            {"E",  3.52479236e-01f},
            {"a_1", 1.84800021e+00f}, 
            {"a_2", 1.41761371e-04f},
            {"a_3", 1.99001968e-01f},
            {"a_4", 4.95424590e-03f}         
        }},

        // Lateral constants for 14 PSI.
        {14, new Dictionary<string, float>(){
            {"B", 45.79f},
            {"C", 0.04851f},
            {"D", 5010},
            {"E", 1.231f},
            {"a_1", 0.01512f}, 
            {"a_2", 2.348E-6f},
            {"a_3", -0.1836f},
            {"a_4", 0}          
        }}


    };

    private Dictionary<int, Dictionary<string, float>> longitudinalConstants = new Dictionary<int, Dictionary<string, float>>(){
        // Longitudinal constants for 8 PSI
        {8, new Dictionary<string, float>(){ 
            {"B", 15f},
            {"C", 1.5f},
            {"D", 5510},
            {"E", 0.5002f},
            {"a_1", 0.0005491f},
            {"a_2", 1.126E-7f},
            {"a_3", -4.656E-5f}
        }},
        // Longitudinal constants for 10 PSI
        {10, new Dictionary<string, float>(){
            {"B", 15f},
            {"C", 1.5f},
            {"D", 5510},
            {"E", 0.5002f},
            {"a_1", 0.0005491f},
            {"a_2", 1.126E-7f},
            {"a_3", -4.656E-5f}           
        }},

        // Longitudinal constants for 12 PSI
        {12, new Dictionary<string, float>(){
            {"B", 15f},
            {"C", 1.5f},
            {"D", 5510},
            {"E", 0.5002f},
            {"a_1", 0.0005491f},
            {"a_2", 1.126E-7f},
            {"a_3", -4.656E-5f}     
        }},

        // Longitudinal constants for 14 PSI
        {14, new Dictionary<string, float>(){
            {"B", 15f},
            {"C", 1.5f},
            {"D", 5510},
            {"E", 0.5002f},
            {"a_1", 0.0005491f},
            {"a_2", 1.126E-7f},
            {"a_3", -4.656E-5f}            
        }}


    };

    

    private Wheel[] wheels = new Wheel[4];

    [Tooltip("Used when anti-Ackermann is disabled.")]
    [Header("Steering")]
    public float parallelSteerAngle = 20f; 
    [Tooltip("steerSpeed isn't used anymore, it was orignally used to smoothly steer the wheels but introduced a delay.")]
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

    // Reads and stores user inputs
    private NewControls keys;
    private float throttle;
    private float brake;
    private float userInput;

    // For measuring 0-60, 0-100 times etc.
    private float theTime = 0f;
    private bool timerOn = false;
    private bool speedReached = false;

    // Demo for the live plotting feature.
    // Any variable with the tag [UPyPlot.UPyPlotController.UPyProbe] wil be plotted.
    // Open one of two python scripts before starting:
    //      1) Plots a separate graph for each variable
    //      2) Plots all variables in the same graph
    // It cannot plot multiple variables in multiple plots.
    // Use GitHub link to find out how to use it: https://github.com/guiglass/UPyPlot 
    [UPyPlot.UPyPlotController.UPyProbe]
    private float RL_LateralForce;

    [UPyPlot.UPyPlotController.UPyProbe]
    private float FL_LateralForce;

    // Records time since previous gear shift.
    private float gearTimer;

    private float speed;
    private float drag;
    private float lift;
    
    // 1 if input to increase brake bias was received, 0 otherwise 
    private float brakeBiasUp;
    // 1 if input to decrease brake bias was received, 0 otherwise 
    private float brakeBiasDown;
    // Time since the most recent brake bias change.
    private float brakeBiasTimer;
    
    // needed for engine sound
    private float previousSpeed;
    private bool isAcclerating;
    
    // To store Centre of mass height
    private float COM_height;
    // To store Roll centre heights
    private float frontRCheight;
    private float rearRCheight;
    
    // To store velocities at centre of mass
    private float COMLateralVelocity;
    private float COMLateralVelocityPrevious;
    private float COMLateralAcceleration;

    private float COMlongitudinalVelocity;
    private float COMlongitudinalVelocityPrevious;
    private float COMlongitudinalAcceleration;


    // To store mass distribution between front and rear 
    private float massFront; 
    private float massRear;

    // To store load transfer variables
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

    private float baseLoadFront; // m*g/2 if evenly distrubted weight, m*g if all weight is in front and none in rear
    private float baseLoadRear;  // m*g/2 if evenly distrubted weight, m*g if all weight is in rear and none in front

    private float[] wheelVerticalLoad = new float[4];

    
    // To store wheel rates. 
    // Wheel rates are effective/equivalent stiffnesses of the double wishbone when converted to vertical springs.
    // They are dynamic values and change with wheel travel, camber, load transfer etc.
    private float wheelRateFL;
    private float wheelRateFR;
    private float wheelRateRL;
    private float wheelRateRR;

    private float[] wheelTravels = new float[4];
       
    // Not used anymore but it might be useful later.
    // The "History" is used to track previous values of variables.
    // It uses a queue structure. The last item in queue is the most recent value.
    // Wheen adding an item to a full queue, the first item in the queue gets removed (pop-ed)
    // and the new item is added at the end, similar to a real queue.

    // Useses a queue of "Vector3"s to track values. 
    // The "(3)" means the queue is up to 3 items long. 
    // If it was 5 inside the round brakets, the queue would be up to 5 items long. 
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

    // Tied to gearDelayTimer, which measures time since previous gear change. 
    // When gearDelayOn is true, the car won't change gears.
    // This stops user from spamming gear change input.
    // Allows for minimum time between gear changes.
    // When gearDelayTimer is high enough. gearDelayOn is set to false.
    private bool gearDelayOn = false;
    private float gearDelayTimer = 0;

    private bool lockedDiff;

    // UI element that shows notifications
    // This instance is used to show when brake bias is being changed.
    private NotificationTriggerEvent notification;

    private float maxDamperForce;
    private float maxSpringForce;

    private float[] camberAngles = new float[4];


    // For the motion platform
    // Vehicle body object
    private Rigidbody m_Rigidbody;

    // ForceSeatMI API
    private ForceSeatMI_Unity m_Api;
    private ForceSeatMI_Vehicle m_vehicle;
    // private ForceSeatMI_Unity.ExtraParameters m_extraParameters;

    private float pitchAngle;
    private float rollAngleAvg;
    // private ForceSeatMI_Unity.ExtraParameters m_extraParameters;


    void OnValidate(){
        
        keys = new NewControls(); // Initalises inptu system.
        rb.centerOfMass = COM_Finder.transform.localPosition; // Finds coordinates of GameObject used to locate COM.
        
        // Finds value of tyrePressure based on dropdown in the inspector.
        if(selectedTyrePressure == TyrePressureEnum._8_PSI){
            tyrePressure = (int)TyrePressureEnum._8_PSI;
        }
        else if(selectedTyrePressure == TyrePressureEnum._10_PSI){
            tyrePressure = (int)TyrePressureEnum._10_PSI;
        }
        else if(selectedTyrePressure == TyrePressureEnum._12_PSI){
            tyrePressure = (int)TyrePressureEnum._12_PSI;
        }
        else if(selectedTyrePressure == TyrePressureEnum._12_PSI){
            tyrePressure = (int)TyrePressureEnum._12_PSI;
        }
        else{
            tyrePressure = 12;
        }
                     
                           
        // Initalises all 4 wheels and suspensions.
        for (int i = 0; i < 4; i++){
            
            // suspensions[i] = new Suspension(i, naturalLength, springTravel, springStiffness, dampingCoefficient, bumpStiffness, bumpTravel, wheelRadius);                     
            wheels[i] = new Wheel(i, wheelObjects[i], meshes[i], rb, wheelRadius, wheelMass, brakeBias, drivetrainInertia,idleRPM, maxRPM, tyrePressure, tyreEfficiency, maxBrakingTorque, longitudinalConstants, lateralConstants);
            
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

        // Adds all the points of the engine curve.
        // If you want to modifiy engine curve, replace the points here.
        // First number is RPM and second number is engine torque.
        // E.g., at 3000 RPM, the engine torque is 38.216 Nm.
        // Interpolation is carreid out automatically.
        // E.g., use:
        // x =  engineCurve.Evaluate(3100)
        // to find engine torque at 3100 RPM via interpolation and store it in the variable x. 
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

    private void OnDestroy()
    {
        // ForceSeatMI - BEGIN
        m_Api.End();
        // ForceSeatMI - END
    }


    void Start(){

        // For motion platform.
        // Their website has a ~50-Page pdf explaining how to set it up in Unity, Unreal Engine etc.
        // Read it to find out how to add extra paramters, change Settings or view diagnostics.
        m_Rigidbody = GetComponent<Rigidbody>();

        // ForceSeatMI - BEGIN
        m_Api             = new ForceSeatMI_Unity();
        m_vehicle         = new ForceSeatMI_Vehicle(m_Rigidbody);
        // m_extraParameters = new ForceSeatMI_Unity.ExtraParameters();

        m_vehicle.SetGearNumber(currentGear);

        // Dont need to enable this line, its there just in case
        // m_Api.SetAppID(""); // "If you have dedicated app id, remove ActivateProfile calls from your code" - Motion platform company
        m_Api.ActivateProfile("SDK - Vehicle Telemetry ACE");
        m_Api.SetTelemetryObject(m_vehicle);
        m_Api.Pause(false);
        m_Api.Begin();


        
        // INERTIA TENSOR:
        // User can manually define rotational insertia of a Rigidbody in Unity.
        // This has two Components "inertiaTensor" and "inertiaTensor".
        // Despite the name, "intertiaTensor" is just a 3D vector.
        // It just takes the diagonal terms of the real 3x3 intertia tensor.
        // "inertiaTensorRotation" is also a 3D vector.
        // It takes in the upper off-diagonal terms of the 3x3 inertia tensor.
        // Lower off-diagional is not needed as the 3x3 tensor is symmetrical.
        // Fusion and Unity use different axis ssytems to a conversion is needed. 

        // Fusion's axis system:    left(X),      backwards(Y),   up(Z)        
        // XX, YY, ZZ  =            1.606e2,      3.168e1,        1.752e2
        // XY, XZ, YZ  =            1.262,        2.116e-1,       3.529
        // Values above were taken from fusion CAD files, in fusion's axis system.
        
        // Unity's axis system:         right(X),       up(Y),      forwards(Z)                            
        // tensor -->    XX, ZZ, YY  =  1.606e2,        1.752e2,    3.168e1
        // rotation -->  XZ, XY, YZ  =  2.116e-1,       1.262,      3.529
        // Values above are same as fusion CAD but rearraged for Unity's axis system.

        // For "inertiaTensor", input XX, ZZ, YY (fusion axis) in that order 
        // For "inertiaTensorRotation" input XZ, XY, YZ (fusion axis) in that order.

        // Old values
        // rb.inertiaTensor = new Vector3(1.606E2f, 1.752E2f, 3.168E1f);
        // rb.inertiaTensorRotation = Quaternion.Euler(2.116E-1f, 1.262f, 3.529f);
        
        // New values from report
        rb.inertiaTensor = new Vector3(138.5f, 151.5f, 26.96f);
        rb.inertiaTensorRotation = Quaternion.Euler(0.2698f, 1.09f, 3.62f);



        // Fins height of com using the empty GameObject and the car's y coordinates.
        COM_height = COM_Finder.transform.position.y - transform.position.y;

        // Calculates the mass at the front and rear, based on COM location.
        massFront = rb.mass *  Mathf.Abs((COM_Finder.transform.position.z - springs[2].transform.position.z)/(springs[0].transform.position.z - springs[2].transform.position.z));
        massRear = rb.mass *  Mathf.Abs((COM_Finder.transform.position.z - springs[0].transform.position.z)/(springs[0].transform.position.z - springs[2].transform.position.z));

        // calcualtes front and rear track based on wheel locaitons.
        trackFront = Mathf.Abs(wheelObjects[1].transform.position.x - wheelObjects[0].transform.position.x);
        trackRear = Mathf.Abs(wheelObjects[3].transform.position.x - wheelObjects[2].transform.position.x);

        // Roll stiffnesses
        rollStiffnessFront = Mathf.Pow(trackFront,2) * (frontSpringStiffness)/(360/Mathf.PI);
        rollStiffnessRear =  Mathf.Pow(trackRear,2) * (rearSpringStiffness)/(360/Mathf.PI);

        // Similar to massFront and massRear but for loads in Newtons.
        baseLoadFront = massFront * 9.81f/2;
        baseLoadRear = massRear * 9.81f/2;

        // This is how you add a new value to the queue for the "History" class.
        FLHistory.AddEntry(FLHub.transform.localPosition);
        FRHistory.AddEntry(FRHub.transform.localPosition);

        // Finds the game object called "NotificationPanel" and assigns it to a variable.
        notification = GameObject.Find("NotificationPanel").GetComponent<NotificationTriggerEvent>(); 
    }

    void Update(){
        // Update gets called once per frame, which means inconsistent timesteps.
        // DON'T USE "Update()" FOR ANY PHYSICS CALCULATIONS. Use "FixedUpdate()" as it has a fixed tiemstep.
        // "Update()" is for non-Physics calculations such as reading inputs.
 
        // Enable usingKeyboard when testing on keyboard
        // The wheel and pedals readings have different values so they are calibrated and normalized differently.
        // After normalizing:
            // steerInput should be a value between -1 and 1 (full left turn to full right turn). 
            // throttle should be a value between 0 and 1 ( no throttle to full pressed throttle)
            // brake should be a value between 0 and 1 (no brake to fully pressed brake)
        if(usingKeyboard){

            // Reads user input values from keyboard
            steerInput = keys.Track.Steering.ReadValue<float>();
            throttle = keys.Track.Throttle.ReadValue<float>();
            brake = keys.Track.Brake.ReadValue<float>();

            // Clamps user input values.
            // For keyboard the values aleady within the correct range etc so no need for normalizing.
            // Clamps there to prevent any bugs/glithces.
            steerInput = Mathf.Clamp(steerInput, -1,1);
            throttle = Mathf.Clamp(throttle, 0,1);
            brake = Mathf.Clamp(brake, 0,1);

            // If the R key (brake bias up button) it is 1, otherwise 0.             
            brakeBiasUp = keys.Track.BrakeBiasUp.ReadValue<float>();
            // If the F key (brake bias down button) is pushed, it is 1, otherwise 0.             
            brakeBiasDown = keys.Track.BrakeBiasDown.ReadValue<float>();
            // Note: The bindings for brake bias up and down can be changed.

        }
        else{

            // Reads user input values from pedals and steering wheel.
            throttle = keys.Track.Throttle.ReadValue<float>();
            brake = keys.Track.Brake.ReadValue<float>();
            brakeBiasUp = keys.Track.BrakeBiasUp.ReadValue<float>();
            brakeBiasDown = keys.Track.BrakeBiasDown.ReadValue<float>();

            // Clamp values close to their maximum and minimum before normalizing.
            // This is used for calibrating.
            // Different steering wheels and pedals will have different ranges of values.
            throttle = Mathf.Clamp(throttle, -0.336f,0.0895f); 
            brake = Mathf.Clamp(brake, -0.4513f,-0.0761f);
            
            // Normalise Values to the correct range and sign.
            throttle = (throttle - -0.336f)/(0.0895f - -0.336f);
            brake = -(brake- - 0.4513f)/(-0.4513f - -0.0761f);

            steerInput = keys.Track.Steering.ReadValue<float>();
            steerInput = Mathf.Clamp(steerInput, -1,1);
        }
        
        // On keyboard, E key for shift up and Q key for shift down.
        // On steering wheel pedal shifters, right pedal for gear up and left pedal for gear down
        // These bindings can be changed.
        shiftUp = keys.Track.ShiftUp.ReadValue<float>();
        shiftDown = keys.Track.ShiftDown.ReadValue<float>();
        
        // The brake input gets ignored if it is smaller than the throttle input. 
        if(throttle > Mathf.Abs(brake)){
            userInput = throttle;
        }
        else{
            userInput = -brake;
        }

        // If shift up or down key is pressed and it has been 0.2s since the last gear change,
        // the car will go up or down into the next gear,
        // otherwise that input gets ignored to ensure at least 0.2s between gear shifts.
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

        // Similar logic to the gear changes and their timeout but for brake bias.
        // Also a notification will show up for 2s when you change brake bias, showing the new value to 2 decimal places.
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
        // Updates the timers for gear and brake bias changes.
        gearTimer += Time.deltaTime; 
        brakeBiasTimer += Time.deltaTime;

        if(gearDelayOn == true){
            // If there was a gear change recently (last 0.2s)
            gearDelayTimer += Time.deltaTime;
            userInput = -brake; // allows brake but no throttle
        }

        if(gearDelayTimer >= 0.2f){
            // If theere wasn't a gear change in the past 0.2s,
            // the timeout ends and user can now change gears,
            // the timer also resets to 0.
            gearDelayOn = false;
            gearDelayTimer = 0;
        }

        // No neutral gear or clutch due to conflict with tyre model.
        // Real car doesn't have a reverse gear so it was excluded.
        // Reverse can be easily added via a negative gear ratio but 
        // neutral will need a clucth model that is compatiable with tyre model. 
        currentGear = Mathf.Clamp(currentGear, 1,5);
        
        // Clamps engine rpm between idle and max value before using power curve.
        // If clutch was successfully implemented, you could clamp between 0 and max.
        engineRPM = Mathf.Clamp(engineRPM, idleRPM, maxRPM);

        // Calculates engine torque from power curve and subtracts auxillary losses.
        engineTorque = (1-auxillaryLoss) * (engineCurve.Evaluate(engineRPM) * userInput);

        // Calculates engine braking toqrue from user input.
        // Engine braking is highest when throttle isn't pressed 
        // and 0 when throttle is fully pressed.
        engineBraking = maxEngineBrakingTorque * (1 - userInput); 
       
        // Calculates steering angles and applies them to the wheels.
        // Anti-Ackermann is used if selected, otherwise parallel steering is used.
        ApplySteering();     
    }


    // For the motion platform. Takes in steering, throttle, brake and handbrake.
    // Car has no handbrake so its valeu is always 0.
    // Also uses currentGear to move/shake seat on gear changes.
    private void Move(float steering, float accel, float footbrake, float handbrake)
    {
         
        // ForceSeatMI - BEGIN
        if (m_vehicle != null && m_Api != null)
        {
            m_vehicle.SetGearNumber(currentGear);

            // Motion platform allows for these extra parameters but we didn't really need them.
            // Side to side movement and pushing the seat back and forth was enough.
            // Roll might be useful for getting a feel for load transfer but 
            // this needs to be tuned/calibrated properly with the motion platform software. 

            // "Use extra parameters to generate custom effects, for exmp. vibrations. They will NOT be
            // filtered, smoothed or processed in any way." - Motion platform company.
            // m_extraParameters.yaw     = 0;
            // m_extraParameters.pitch   = pitchAngle;
            // m_extraParameters.roll    = rollAngleAvg
            // ;
            // m_extraParameters.right   = 0;
            // m_extraParameters.up      = 0;
            // m_extraParameters.forward = 0;

            // m_Api.AddExtra(m_extraParameters);
            m_Api.Update(Time.fixedDeltaTime);
        }
        // ForceSeatMI - END
    }


    void FixedUpdate(){
        // Exclusively used for physics based calcualtes which need a small, fixed tiemstep.
        // DON'T use this if it's not physics related, use "Update()" instead. 
        // Timestep can be selected from the Unity window: Edit > Project Settings > Time > Fixed Timestep
       

        // For motion platform.
        Move(steerInput, throttle, brake, 0);

        // For the demonstration of the live graph plotting tool. 
        Vector3 FLposition = FLHub.transform.localPosition;
        Vector3 FRposition = FRHub.transform.localPosition;

        
        updateWheelTravels(); // Wheel travel is the vertical displacement of the wheel (from rest)
        updateRackTravel(); // Rack travel is the displacement of the steering rack.
        updateWheelRates(); // Wheel rate is the vertical spring equivalent stiffness of a double wishbone setup.
        updateRollStiffnesses(); 
        updateRollAngles();
        updatePitch();
        updateLoadTransfers();
        updateRollcentreHeights();
        updateCOMaccleerations();
        updateVerticalLoad();
        updateCamberAngles();
        // calculateDiffTorques();

        // Sets the torque value of that drives the rear wheels.
        wheels[2].wheelTorque = 0.5f*(engineTorque - engineBraking) * gearRatios[currentGear + 1] *primaryGearRatio * finalDriveRatio;
        wheels[3].wheelTorque = 0.5f*(engineTorque - engineBraking) * gearRatios[currentGear + 1] *primaryGearRatio * finalDriveRatio;       
            
        
        // For each wheel/suspension. 
        for(int i = 0; i<springs.Count; i++){   

            // Darws a raycast from suspension, downwards, up to certain length.
            // contact is true if raycast hits anything, flase otherwise
            // "hit" variable contains position, velocity, normal, etc of the hit point. 
            // used for calculating wheel tarvel, local wheel velocities etc.            
            bool contact = Physics.Raycast(springs[i].transform.position, -transform.up, out RaycastHit hit, suspensions[i].naturalLength + suspensions[i].springTravel + wheelRadius);
            
            if(contact){            
                
                // Force vectors from suspension, wheel and anti rollbars.
                // Anti-rollbars are currently not being used so they will return 0.
                // They were orignally used becuase we didn't have dynamic and accurate wheel rates.
                Vector3 suspensionForceVector = suspensions[i].getUpdatedForce(hit, Time.fixedDeltaTime, contact);          
                Vector3 wheelForceVector = wheels[i].getUpdatedForce(userInput, gearRatios[currentGear + 1], finalDriveRatio, primaryGearRatio, hit, Time.fixedDeltaTime, wheelVerticalLoad[i], camberAngles[i]);            
                Vector3 antiRollForceVector = getAntiRollForce(suspensions[2], suspensions[3], antiRollStiffness, i) * hit.normal;

                // Applies the wheel and suspension forces to the car, at the hit point + some offset.
                // Small offset was applied due to an instability glitch/issue. 
                // Offset of 0.44 is 2x the wheel radius. 
                rb.AddForceAtPosition(wheelForceVector +suspensionForceVector, hit.point + new Vector3 (0,0.44f,0)); 
                
                // Calculates average angular velocity of rear wheels and converts from rad/s to RPM.
                float averageRearRPM = (9.5493f)*(wheels[2].omega + wheels[3].omega)/2;
                if(currentGear != 0){
                    engineRPM = averageRearRPM * (gearRatios[currentGear + 1] * primaryGearRatio * finalDriveRatio);
                }

                engineRPM = Mathf.Clamp(engineRPM, idleRPM, 14000);

                
                                
            }
            else{
                suspensions[i].contact = false;
            }

            // Was used for recording maximum spring and damper forces during runs, not needed anymore.
            if(suspensions[i].damperForce > maxDamperForce){
                maxDamperForce = suspensions[i].damperForce;
            }
            if(suspensions[i].springForce > maxSpringForce){
                maxSpringForce = suspensions[i].springForce;
            }

            
        }

        showTimer(); // Shows a timer if the 0-60, 0-100 etc timer is enabled.
        // When the timer stop increasing, the car has reached the target speed.
        // Timer starts when the car starts moving so move the car to a clear spot before starting. 
        
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
        
       float totalVerticalLoad = 0;
       for(int i = 0; i<4; i++){
           totalVerticalLoad += wheels[i].verticalLoad;
       }
        //Debug.Log($"Total vertical load = {totalVerticalLoad}");



    }

    
    void updateWheelTravels(){        
        for(int i = 0; i<4; i++){
            wheelTravels[i] = 1000 * (suspensions[i].naturalLength - suspensions[i].springLength); // mm
        }
    }

    void updateRollcentreHeights(){
        // calcualtes roll centre heights based on equation from Lola software.

        float rollcentreFL = 29.05f - 0.03924f*(wheelTravels[0]) + 0.003029f*rackTravel - 0.0006878f*Mathf.Pow(wheelTravels[0],2) + 0.000353f*wheelTravels[0]*rackTravel - 0.0000917f*Mathf.Pow(rackTravel,2) + 8.601E-6f*Mathf.Pow(wheelTravels[0],3) + 1.04E-6f*(Mathf.Pow(wheelTravels[0],2))*rackTravel + 6.995E-6f*(wheelTravels[0]*Mathf.Pow(rackTravel,2)); 
        float rollcentreFR =  29.05f - 0.03924f*(wheelTravels[1]) + 0.003029f*rackTravel - 0.0006878f*Mathf.Pow(wheelTravels[1],2) + 0.000353f*wheelTravels[1]*rackTravel - 0.0000917f*Mathf.Pow(rackTravel,2) + 8.601E-6f*Mathf.Pow(wheelTravels[1],3) + 1.04E-6f*(Mathf.Pow(wheelTravels[1],2))*rackTravel + 6.995E-6f*(wheelTravels[1]*Mathf.Pow(rackTravel,2));
        float rollcentreRL = -0.0005f * Mathf.Pow(wheelTravels[2],2) - 1.2702f * wheelTravels[2] + 69.395f;
        float rollcentreRR = -0.0005f * Mathf.Pow(wheelTravels[3],2) - 1.2702f * wheelTravels[3] + 69.395f;
        
        rollCentreHeightFront = (rollcentreFL + rollcentreFR)/2000; // average and conversion to metres 
        rollCentreHeightRear = (rollcentreRL + rollcentreRR)/2000; // average and conversion to metres 

    }

    void updateWheelRates(){
        // Calculates wheel rates based on equations from Lola software

        wheelRateFL = 1000*(47.15f + 0.02513f*wheelTravels[0] + 0.003504f*rackTravel + 0.0008393f*Mathf.Pow(wheelTravels[0],2) + 0.0002012f*wheelTravels[0]*rackTravel + 0.0001072f*Mathf.Pow(rackTravel,2) - 4.053E-5f*Mathf.Pow(wheelTravels[0],3) - 5.693E-6f*Mathf.Pow(wheelTravels[0],2)*rackTravel + 5.118E-6f*wheelTravels[0]*Mathf.Pow(rackTravel,2)); 
        wheelRateFR = 1000*(47.15f + 0.02513f*wheelTravels[1] + 0.003504f*rackTravel + 0.0008393f*Mathf.Pow(wheelTravels[1],2) + 0.0002012f*wheelTravels[1]*rackTravel + 0.0001072f*Mathf.Pow(rackTravel,2) - 4.053E-5f*Mathf.Pow(wheelTravels[1],3) - 5.693E-6f*Mathf.Pow(wheelTravels[1],2)*rackTravel + 5.118E-6f*wheelTravels[1]*Mathf.Pow(rackTravel,2)); 
        wheelRateRL = 1000*(-4E-5f*Mathf.Pow(wheelTravels[2] ,3) + 0.0009f*Mathf.Pow(wheelTravels[2] ,2) - 0.029f*wheelTravels[2] + 41.945f);
        wheelRateRR = 1000*(-4E-5f*Mathf.Pow(wheelTravels[3] ,3) + 0.0009f*Mathf.Pow(wheelTravels[3] ,2) - 0.029f*wheelTravels[3] + 41.945f);

        // Updates the suspension stiffesses to the new wheel rate values.
        // dampingCoefficient of 10% of spring stiffness works quite well and is stable.
        // This can be changed by just diving or miltiplying a different number for each wheel in the lines below. 
        // If you want a static dampingCoefficient, select a value that will work across the entire range of wheel rate values.
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


       
        COM_height = COM_Finder.transform.position.y - transform.position.y;

        // Global vectors
        velocityVector = rb.GetPointVelocity(COM_Finder.transform.position);
        accelerationVector = (velocityVector - previousVelocityVector)/Time.fixedDeltaTime;        

        // Local accel vectors
        COMLateralAcceleration = Vector3.Dot(accelerationVector, transform.right.normalized);
        COMlongitudinalAcceleration = Vector3.Dot(accelerationVector, transform.forward.normalized);

        previousVelocityVector = velocityVector;
       

    }

    void updateRollAngles(){
        // Calculates roll angles based on wheel travels
        rollAngleFront = -Mathf.Rad2Deg * Mathf.Atan((wheelTravels[0] - wheelTravels[1])/1000*trackFront);
        rollAngleRear =  -Mathf.Rad2Deg * Mathf.Atan((wheelTravels[2] - wheelTravels[3])/1000*trackRear);
        rollAngleAvg = 0.5f*(rollAngleFront + rollAngleRear);
    }

    void updatePitch(){
        // Calculates pitch based on wheel travels on the wheel travels
        pitchAngle = Mathf.Rad2Deg * Mathf.Atan((wheelTravels[0] + wheelTravels[2])/wheelBase);

    }

    void updateLoadTransfers(){

        // Calculates load transfer for front and rear using static methods written in the suspension class.
        // Note: A "satic" method or variable means it is the same across all object of that class.  
        //       It is shared across all objects of that class and cannot be modified on a per instance/object basis.

        elasticLoadTransferFront = Suspension.transientElasticLoadTransfer(rollStiffnessFront, rollAngleFront, trackFront);
        elasticLoadTransferRear = Suspension.transientElasticLoadTransfer(rollStiffnessRear, rollAngleRear, trackRear);

        geometricLoadTransferFront = Suspension.geometricLoadTransferFront(massFront, COMLateralAcceleration, rollCentreHeightFront, trackFront);
        geometricLoadTransferRear = Suspension.geometricLoadTransferRear(massRear, COMLateralAcceleration, rollCentreHeightRear, trackRear);
        
        longitudnialLoadTransfer = Suspension.LongitudinalLoadTransfer(rb.mass, COMlongitudinalAcceleration, COM_height, wheelBase);
        // longitudnialLoadTransfer = Mathf.Lerp(previousLongitudinalLoadTransfer, longitudnialLoadTransfer, 0.002f);

        lateralLoadTransferFront = -( elasticLoadTransferFront + geometricLoadTransferFront);
        lateralLoadTransferRear = -( elasticLoadTransferRear + geometricLoadTransferRear);

        // Total measured and thoetretical values are just there for sanity checks, not acutally used in physics models.
        totalLateralLoadTransferMeasured = lateralLoadTransferFront + lateralLoadTransferRear;
        totalLateralLoadTransferTheoretical = (rb.mass * COMLateralAcceleration * COM_height)/(0.5f*(trackFront + trackRear));

        // Debug.Log($"theoretical = {totalLateralLoadTransferTheoretical}, measured = {totalLateralLoadTransferMeasured}");

        previousLongitudinalLoadTransfer = longitudnialLoadTransfer;
        
    }

    void updateVerticalLoad(){
        // Updates the vertical loads for each wheel using the orignal 
        // weight distribution (at rest) with lateral and longitudinal load transfers.
        
        // This vairable exists only inside this method and is a placeholder.
        // It will be updated 4 times by the for loop and gets removed after the method is finished. 
        float verticalLoad;

        for(int i = 0; i<4; i++){
            if(Mathf.Abs(COMLateralAcceleration) >= 0f){
                // Calculates vertical load of wheel i.
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

    void updateCamberAngles(){
        // static camber angle is just the inital camber value.

        camberAngles[0] = staticCamberAngleFront + calculateCamberChangeFront(wheelTravels[0], rackTravel) + rollAngleFront;
        camberAngles[1] = staticCamberAngleFront + calculateCamberChangeFront(wheelTravels[1], rackTravel) - rollAngleRear;

        camberAngles[2] = staticCamberAngleRear + calculateCamberChangeRear(wheelTravels[2]) + rollAngleFront;
        camberAngles[3] = staticCamberAngleRear + calculateCamberChangeRear(wheelTravels[3]) - rollAngleRear;


    }



    float calculateCamberChangeFront(float wheelTravel, float rackTravel){

        // Equation form Lola software.
        return -3.191f - 0.02382f*wheelTravel + 0.04161f*rackTravel - 0.0004034f*Mathf.Pow(wheelTravel,2) - 6.873E-5f*wheelTravel*rackTravel + 0.0004845f*Mathf.Pow(rackTravel, 2) + 3.172f;
    }

    float calculateCamberChangeRear(float wheelTravel){
        // Equation from lola software.
        return -0.0004f * Mathf.Pow(wheelTravel,2) + 0.0016f*wheelTravel - 1.8333f + 1.824f;
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
        // Timer for 0-X speed tests in mph, m/s or km/h.
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
        // Gizmos used for debugging purposes.
        // Comment/uncomment certain sections of the code to hide/show certain gizmos.

        // Shows a small sphere at the COM and the local origin of the car. 
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere( transform.TransformPoint(rb.centerOfMass),0.1f);
        Gizmos.DrawWireSphere( transform.position,0.1f);
        
        for(int i = 0; i < springs.Count; i++){
        
            
            // Shows the current length of the spring.   
            // ----------------------------------------------------------         
            // Gizmos.color = Color.blue;
            // Ray ray = new Ray(springs[i].transform.position, -transform.up);           
            // Gizmos.DrawLine(ray.origin, -suspensions[i].springLength * transform.up + springs[i].transform.position);
         

            // Shows the minimum length of the spring.
            // ----------------------------------------------------------
            // Gizmos.color = Color.white;
            // Ray ray2 = new Ray(springs[i].transform.position, -transform.up);           
            // Gizmos.DrawLine(ray2.origin, -suspensions[i].minLength * transform.up + springs[i].transform.position);
            

            // Shows the wheel radius and calcualted locaitons.
            // You can add a similar line to show the actual wheel locations to look for discrepencies.
            // --------------------------------------------------------
            // Gizmos.color = Color.yellow;
            // Gizmos.DrawLine(-suspensions[i].springLength * transform.up + springs[i].transform.position, -suspensions[i].springLength * transform.up + springs[i].transform.position + transform.up * -wheelRadius);
            

            // Shows a small flat square at the hit point of the raycast.
            // --------------------------------------------------------
            // Gizmos.color = Color.white;
            // Gizmos.DrawWireCube(springs[i].transform.position - new Vector3(0, suspensions[i].springLength + wheelRadius, 0), new Vector3(0.1f, 0, 0.1f));
        

            // Shows the longitudinal and lateral velocities of the wheels as two lines, 
            // one going longitudinally and one going laterally,
            // the longer the lines, the larger the velocities.
            // Going 1m/s will show a 1m line. You can apply a scale factor in the last parameter (that controls length) 
            // by just multiplying by a number. 
            // --------------------------------------------------------
            // Gizmos.color = Color.yellow;
            // Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].longitudinalVelocity * wheels[i].wheelObject.transform.forward);
            // Gizmos.DrawRay(wheels[i].wheelObject.transform.position, -wheels[i].lateralVelocity * wheels[i].wheelObject.transform.right);
            

            // Shows the longitudinal and lateral tyre forces, similar to the previous section with velocities.
            // Lengths are scaled so that 1000N gives a 1m line.
            // If you want to see both velocities and forces at the same time,
            // change the colour for one of them.
            //--------------------------------------------------------
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].wheelObject.transform.right * (wheels[i].lateralForce/1000));
            Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].wheelObject.transform.forward * (wheels[i].longitudinalForce/1000));
            
            
            // Shows the vertical load on the wheel.
            // --------------------------------------------------------
            // Gizmos.color = Color.yellow;
            // Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].wheelObject.transform.up * wheels[i].verticalLoad/1000);
            
            

            // Shows the forces form the anti rollbar forces - no longer needed.
            // --------------------------------------------------------
            // Gizmos.color = Color.yellow;
            // if(i == 2 | i == 3){
            //     Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].wheelObject.transform.up * antiRollForces[i-2]/1000);
            // }



            // Shows the 2D force vector of the wheel (longitudinal and lateral only)
            // --------------------------------------------------------
            // Gizmos.DrawRay(wheels[i].wheelObject.transform.position, wheels[i].forceVector/1000);
            
        }

        // Shows the COM's longitudinal and lateral velocities.
        // --------------------------------------------------------
        // Gizmos.color = Color.white;
        // Gizmos.DrawRay(COM_Finder.transform.position, COMlongitudinalVelocity * transform.forward);
        // Gizmos.DrawRay(COM_Finder.transform.position, COMLateralVelocity* transform.right);

      
        
        
    }

    void ApplySteering(){       
        // Applies steering to the wheels 

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
        

        // Smoothly moves the wheel instead of making it instantly change direcitons.
        // This also adds a dealy as a side effect, 
        // so only use it for keyboard where the input can go from -1 to 1 instantly.
        // This is not needed for a steering wheel.
        // wheelAngleLeft = Mathf.Lerp(wheelAngleLeft, steerAngleLeft, steerSpeed * Time.deltaTime);
        // wheelAngleRight = Mathf.Lerp(wheelAngleRight, steerAngleRight, steerSpeed * Time.deltaTime);

        // Comment these lines out if you are using the Lerp above.
        // otherwise leave these enabled for instant steering response.
        // E.g., going from 1 to 0 will instantly rotate the wheels from pointing right to pointing forward.
        wheelAngleLeft=steerAngleLeft;
        wheelAngleRight=steerAngleRight;


        // Rotates the wheel according to the steering angle and toe.
        // Camber is not shown but still goes into the tyre model calculations and effects the resulting tyre forces.
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

    // Used for the UI to access data in this script:
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


    // No longer needed.
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

