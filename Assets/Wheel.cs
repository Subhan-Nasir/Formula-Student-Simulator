using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;



// One object of the Wheel class is used to model each wheel of the car.
public class Wheel{
    // Don't attach this script to anything,
    // This is only used by the "RaycastController" script to model the tyre physics. 
    
    // id: 0 for fornt left,
    //     1 for front right,
    //     2 for rear left,
    //     3 for rear right
    public float id;

    // Wheel Object is an empty game object with just a Transform component.  
    // This rotates with the steering and toe but not the spinning of the wheel.
    // Therefore, the z direction is always forward and x direction is always lateral (in the Unity axis system).    
    public GameObject wheelObject;

    // This GameObject with a mesh is a child of the wheelObject.
    // The mesh is basically used to render the wheel in 3D.
    // Since it is a child, it will follow the locaiton and rotation of the parent.
    // A rotation about the local x axis is applied to show the spinning of the wheel. 
    // Steering is done by the parent so no need to define it for the child.
    public GameObject wheelMesh;

    // Rigidbody of the car, not the wheel. The wheel has no Rigidbody component. 
    // All forces are applied to the car at different locations.
    public Rigidbody rb;


    public float wheelRadius;
    public float wheelMass;
    public int tyrePressure;
    public float momentOfInertia;
    public float drivetrainInertia;  
    
    // Rolling resistnace coefficient.
    public float rrCoefficient = 0.0003f;

    

    public float slipAngle;
    public float slipRatio;

    // Wheel velocity in local space.
    public Vector3 wheelVelocityLS;

    public float longitudinalVelocity;
    public float lateralVelocity;

    public float wheelTorque;
    public float engineTorque;
    public float brakingTorque;
    public float rollingResistance;
    public float torque;

    public float brakeBias;

    public float lateralForce; //Sideways direction    
    public float longitudinalForce; // Forwards direction

    public float engineIdleRPM;
    public float engineMaxRPM;
    
    public Vector3 forceVector;

    // Tyre constants
    public float D_long;
    public float C_long;
    public float B_long;
    public float E_long;
    public float a_1_long;
    public float a_2_long;
    public float a_3_long;

    public float D_lat;
    public float C_lat;
    public float B_lat;
    public float E_lat;
    public float a_1_lat;
    public float a_2_lat;
    public float a_3_lat;
    public float a_4_lat;

    // Tyre static and dynamic limits.    
    public float fLongLimit;
    public float fLatLimit;
    public float fLongDynamicLimit;
    public float fLatDynamicLimit;

    // Angular accerleation and angular velocity.
    public float alpha;
    public float omega;

    public float verticalLoad;
    public float tyreEfficiency;
    
    // All the toqrues that act opposite to engine toque.
    // Includes force coming from the ground to the wheel (converted to torque by multiplying with wheel radius).
    // Tyre force acts on ground, ground creates equal and opposite reactive force that acts on the wheels.
    // Reactive force both pushes the car and changes the anuglar velocity of wheels.
    public float feedbackTorque;

    public float maxBrakingTorque;

    public float camberAngle;
    // public float diffLongForceLimit = 1000000;

    // public bool lockedDiff = false;
    // public float diffControlledOmega;


    public Wheel(float id, GameObject wheelObject, GameObject wheelMesh, Rigidbody rb, float wheelRadius, float wheelMass, float brakeBias, float drivetrainInertia, float engineIdleRPM, float engineMaxRPM, int tyrePressure, float tyreEfficiency, float maxBrakingTorque, Dictionary<int, Dictionary<string, float>> longitudinalConstants, Dictionary<int, Dictionary<string, float>> lateralConstants){
        // This is the constructor method.
        // All the parameters listed in the brackets above 
        // need to be provided to create a suspension class object.
        

        // Assigns values given in the constructor to the variables in the class.
        this.id = id;
        this.wheelObject = wheelObject;
        this.wheelMesh = wheelMesh;
        this.rb = rb;
        this.wheelRadius = wheelRadius;
        this.wheelMass = wheelMass;
        this.momentOfInertia = 0.5f * wheelMass * Mathf.Pow(wheelRadius, 2);
        this.brakeBias = brakeBias;
        this.drivetrainInertia = drivetrainInertia;
        this.engineIdleRPM = engineIdleRPM;
        this.engineMaxRPM = engineMaxRPM;
        this.tyrePressure = tyrePressure;
        this.tyreEfficiency = tyreEfficiency;
        this.maxBrakingTorque = maxBrakingTorque;


        this.B_long = longitudinalConstants[tyrePressure]["B"];
        this.C_long = longitudinalConstants[tyrePressure]["C"];
        this.D_long = longitudinalConstants[tyrePressure]["D"];      
        this.E_long = longitudinalConstants[tyrePressure]["E"];
        this.a_1_long = longitudinalConstants[tyrePressure]["a_1"];
        this.a_2_long = longitudinalConstants[tyrePressure]["a_2"];
        this.a_3_long = longitudinalConstants[tyrePressure]["a_3"];



        this.B_lat = lateralConstants[tyrePressure]["B"];
        this.C_lat = lateralConstants[tyrePressure]["C"];
        this.D_lat = lateralConstants[tyrePressure]["D"];
        this.E_lat = lateralConstants[tyrePressure]["E"];
        this.a_1_lat = lateralConstants[tyrePressure]["a_1"];
        this.a_2_lat = lateralConstants[tyrePressure]["a_2"];
        this.a_3_lat = lateralConstants[tyrePressure]["a_3"];
        this.a_4_lat = lateralConstants[tyrePressure]["a_4"];

        
        // if(id == 2| id == 3){
        //     lockedDiff = true;
        // }
        
             

    }

    // The folling methods are helper functions used to calculate tyre forces and motion.
    public float tyreEquationLongitudinal(float slip, float fDynamicLimit, float fStaticLimit, float verticalLoad, float B_long, float C_long, float D_long, float E_long, float a_1_long, float a_2_long, float a_3_long){        
        float force = (fDynamicLimit/fStaticLimit)*((a_1_long*verticalLoad - a_2_long*Mathf.Pow(verticalLoad, 2))*D_long)* Mathf.Sin( C_long * Mathf.Atan(B_long*Mathf.Pow(verticalLoad, a_3_long) * slip - E_long * ( (B_long*Mathf.Pow(verticalLoad, a_3_long)*slip) - Mathf.Atan(B_long*Mathf.Pow(verticalLoad, a_3_long)*slip))));
        
        return force;
    }

    public float tyreEquationLateral(float slip, float fDynamicLimit, float fStaticLimit, float verticalLoad, float camberAngle, float B_lat, float C_lat, float D_lat, float E_lat, float a_1_lat, float a_2_lat, float a_3_lat, float a_4_lat){

        float force = (fDynamicLimit/fStaticLimit)*((a_1_lat*verticalLoad - a_2_lat*Mathf.Pow(verticalLoad, 2))*D_lat*(1-a_4_lat*Mathf.Pow(camberAngle,2)))* Mathf.Sin( C_lat * Mathf.Atan(B_lat*Mathf.Pow(verticalLoad, a_3_lat) * slip - E_lat * ( (B_lat*Mathf.Pow(verticalLoad, a_3_lat)*slip) - Mathf.Atan(B_lat*Mathf.Pow(verticalLoad, a_3_lat)*slip))));
        return force;
    }


    
    public float tyreCurvePeakLateral(float a_1_lat, float a_2_lat, float a_4_lat, float verticalLoad, float camberAngle, float D_lat){

        return ((a_1_lat*verticalLoad - a_2_lat*Mathf.Pow(verticalLoad, 2))*D_lat*(1-a_4_lat*Mathf.Pow(camberAngle,2)));

    }

    public float tyreCurvePeakLongitudinal(float a_1_long, float a_2_long, float D_long, float verticalLoad){
        
        return (a_1_long*verticalLoad - a_2_long*Mathf.Pow(verticalLoad, 2))*D_long;
    }

    public float dynamicPeakLongitudinal(float fLat, float fLongLimit, float fLatLimit){
        float dynamicPeak = Mathf.Abs(fLongLimit) * Mathf.Sqrt(1 - Mathf.Pow(fLat/fLatLimit, 2) );
        return dynamicPeak;

    }

    public float dynamicPeakLateral(float fLong, float fLongLimit, float fLatLimit){
        float dynamicPeak = Mathf.Abs(fLatLimit) * Mathf.Sqrt(1 - Mathf.Pow(fLong/fLongLimit, 2) );
        return dynamicPeak;

    }

    public float calculateSlipRatio( float velocity, float omega, float radius){
        float slipRatio;
        float wR = omega * radius;

        if(Mathf.Abs(wR) >= Mathf.Abs(velocity)){
            slipRatio = (wR - velocity)/Mathf.Abs(wR);                
        }            
        else{
            slipRatio = (wR - velocity)/Mathf.Abs(velocity);
        }

        // slipRatio = Mathf.Clamp(slipRatio, -1,1);
        return slipRatio;
    }

    public float calculateSlipAngle(float vLong, float vLat, float threshold){
        float slipAngle;
        if(Mathf.Abs(vLong) < threshold){
            slipAngle = 0;
        }
        else{
            slipAngle = -Mathf.Atan(vLat / Mathf.Abs(vLong));
            
        }


        return slipAngle;
    }

   
    
    public float getRollingResistance(float longitudinalVelocity, float lateralVelocity, float verticalLoad, float radius, int tyrePressure){
        float speed = Mathf.Pow(Mathf.Pow(longitudinalVelocity, 2) + Mathf.Pow(lateralVelocity,2), 0.5f);        
        float rrCoefficient = 0.005f + ((1/(0.06895f*tyrePressure))*(0.01f + 0.0095f * Mathf.Pow(0.036f*speed,2))); 
        float rrForce = -(rrCoefficient) * verticalLoad;
        return radius * rrForce;

    }


    // Gets called every timestep to update tyre forces and RPM.
    public Vector3 getUpdatedForce(float userInput, float currentGearRatio, float finalDriveRatio, float primaryGearRatio, RaycastHit hit, float timeDelta, float verticalLoad, float camberAngle){
        
        
        // No negative vertical load.
        if(verticalLoad < 0){
            verticalLoad = 0;
        }
        
        this.verticalLoad = verticalLoad;
        this.camberAngle = camberAngle;
        
       
        // Move the wheel so that the bottom of the wheel touches the hit point.         
        wheelObject.transform.position = hit.point + hit.normal * wheelRadius;
        
        // Find the local velocity vector at the hit point.        
        wheelVelocityLS = wheelObject.transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));

        // Use compoennts of the local velocity to get longitudinal and lateral velocities.
        lateralVelocity = wheelVelocityLS.x;
        longitudinalVelocity = wheelVelocityLS.z;              
        
        // Get static limits.        
        fLongLimit = tyreCurvePeakLongitudinal(a_1_long, a_2_long, D_long, verticalLoad);
        fLatLimit = tyreCurvePeakLateral(a_1_lat, a_2_lat, a_4_lat, verticalLoad, camberAngle, D_lat);

        
        // Calculate dynamic limits from the static limits and forces from previous timestep.
        fLongDynamicLimit = dynamicPeakLongitudinal(lateralForce, fLongLimit, fLatLimit);
        fLatDynamicLimit = dynamicPeakLateral(longitudinalForce, fLongLimit, fLatLimit);

        // Calculate braking toque.
        // When speed is below 1m/s braking torque is gradually reduced,
        // otherwise the car would just rock back and forth.
        // This is done by simply multiplying by velocity when its lower than 1m/s.
        // At 0m/s there is 0% braking force, even when the brake is fully pressed.
        // At 0.5m/s there is 50% braking force, even when the brake is fully pressed.
        // At 1m/s or higher there is 100% braking force at when the brake is fully pressed.
        if(userInput <0){
            if(Mathf.Abs(longitudinalVelocity) > 1){
                if(id == 2 | id == 3){
                    brakingTorque = maxBrakingTorque* userInput * (1 - brakeBias);
                }
                else{
                    brakingTorque = maxBrakingTorque * userInput * brakeBias;
                }

                
            }
            else if (Mathf.Abs(longitudinalVelocity) <= 1){
                brakingTorque = maxBrakingTorque * userInput ;

                if(id == 2 | id == 3){
                    brakingTorque = maxBrakingTorque* userInput * longitudinalVelocity * (1 - brakeBias);
                }
                else{
                    brakingTorque = maxBrakingTorque * userInput * longitudinalVelocity * brakeBias;
                }
            }
            
            
        }
        else{
            brakingTorque = 0;
        }
     
        rollingResistance = getRollingResistance(longitudinalVelocity, lateralVelocity, verticalLoad, wheelRadius, tyrePressure);
     

        feedbackTorque = rollingResistance + brakingTorque - longitudinalForce*wheelRadius;
        torque = wheelTorque + feedbackTorque;

        // Drivetrain inertia only effects the rotation of rear wheels.        
        if(id == 2| id == 3){
            alpha = torque /(drivetrainInertia + momentOfInertia);
        }
        else{
            alpha = torque/momentOfInertia;
        }  
         

        omega += alpha * timeDelta;
        

        // if(lockedDiff == true){
        //     omega = diffControlledOmega;
        // }
        
        // Clamp wheels based on input and if they are front and rear.
        if(userInput >=0){

            // Rear wheels clamped between engine rpm range scaled by gear ratios.
            if(id == 2 | id == 3){
                omega = Mathf.Clamp(omega, (1/9.5493f) * engineIdleRPM/(currentGearRatio * finalDriveRatio * primaryGearRatio), (1/9.5493f) * engineMaxRPM/(currentGearRatio * finalDriveRatio * primaryGearRatio));
            }
            // Front wheels clamped between extreme values, essentially unclamped.            
            else{
                
                omega = Mathf.Clamp(omega, -100000000f,10000000000f);
            }
            
        }
        else{
            // Min value is 0 when fully pressing the brakes (i.e., userInput is -1).
            omega = Mathf.Clamp(omega, 0,10000000000f);
        }
      
       
        // Rotate the GameObject that has the wheel mesh to show the wheels spinning.
        wheelMesh.transform.Rotate(Mathf.Rad2Deg * omega * timeDelta, 0, 0, Space.Self); 
        

        slipRatio = calculateSlipRatio(longitudinalVelocity, omega, wheelRadius);        
        longitudinalForce = tyreEquationLongitudinal(slipRatio, fLongDynamicLimit, fLongLimit, verticalLoad, B_long, C_long, D_long, E_long, a_1_long, a_2_long, a_3_long);


        slipAngle = calculateSlipAngle(longitudinalVelocity, lateralVelocity, threshold: 0.1f);   
        lateralForce = tyreEquationLateral(slipAngle, fLatDynamicLimit, fLatLimit, verticalLoad, camberAngle, B_lat, C_lat, D_lat, E_lat, a_1_lat, a_2_lat, a_3_lat, a_4_lat);

        // Checks if any of the forces is Not a Number (NaN) due to divide by 0 or infinity errors. 
        if(float.IsNaN(longitudinalForce)){
            longitudinalForce = 0;
        }
        if(float.IsNaN(lateralForce)){
            lateralForce = 0;
        }

        
        // Clamp set to high values only generated by crashing.
        // Stops the car from flying off or falling through the map when crashing.
        longitudinalForce = Mathf.Clamp(longitudinalForce, -50000, 50000);
        lateralForce = Mathf.Clamp(lateralForce, -50000,50000);

        
        if(id == 0 | id == 1){
            longitudinalForce = 1.25f*longitudinalForce;
            lateralForce = 1.25f*lateralForce;
        }

        forceVector = longitudinalForce * wheelObject.transform.forward + lateralForce * wheelObject.transform.right;
                   
        return forceVector;


    }

}