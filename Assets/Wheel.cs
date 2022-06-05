using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;




public class Wheel{

    // void Start(){
        // wc=this;
    // }
    
    public float id;
    public GameObject wheelObject;
    public GameObject wheelMesh;
    public Rigidbody rb;
    public float wheelRadius;
    public float wheelMass;
    public int tyrePressure;
    public float momentOfInertia;
    public float drivetrainInertia;  
    
    public float rrCoefficient = 0.0003f;

    // public static Wheel wc;


    public float slipAngle;
    public float slipRatio;
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

    public float fLongLimit;
    public float fLatLimit;
    public float fLongDynamicLimit;
    public float fLatDynamicLimit;


    public float alpha;
    public float omega;

    public float verticalLoad;
    public float tyreEfficiency;
    
    public float feedbackTorque;
    public float maxBrakingTorque;

    public float camberAngle;
    // public float diffLongForceLimit = 1000000;

    // public bool lockedDiff = false;
    // public float diffControlledOmega;


    public Wheel(float id, GameObject wheelObject, GameObject wheelMesh, Rigidbody rb, float wheelRadius, float wheelMass, float brakeBias, float drivetrainInertia, float engineIdleRPM, float engineMaxRPM, int tyrePressure, float tyreEfficiency, float maxBrakingTorque, Dictionary<int, Dictionary<string, float>> longitudinalConstants, Dictionary<int, Dictionary<string, float>> lateralConstants){
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


    public float tyreEquationLongitudinal(float slip, float fDynamicLimit, float fStaticLimit, float verticalLoad, float B_long, float C_long, float D_long, float E_long, float a_1_long, float a_2_long, float a_3_long){        
        float force = (fDynamicLimit/fStaticLimit)*((a_1_long*verticalLoad - a_2_long*Mathf.Pow(verticalLoad, 2))*D_long)* Mathf.Sin( C_long * Mathf.Atan(B_long*Mathf.Pow(verticalLoad, a_3_long) * slip - E_long * ( (B_long*Mathf.Pow(verticalLoad, a_3_long)*slip) - Mathf.Atan(B_long*Mathf.Pow(verticalLoad, a_3_long)*slip))));
        
        return force;
    }

    public float tyreEquationLateral(float slip, float fDynamicLimit, float fStaticLimit, float verticalLoad, float camberAngle, float B_lat, float C_lat, float D_lat, float E_lat, float a_1_lat, float a_2_lat, float a_3_lat, float a_4_lat){

        float force = (fDynamicLimit/fStaticLimit)*((a_1_lat*verticalLoad - a_2_lat*Mathf.Pow(verticalLoad, 2))*D_lat*(1-a_4_lat*Mathf.Pow(camberAngle,2)))* Mathf.Sin( C_lat * Mathf.Atan(B_lat*Mathf.Pow(verticalLoad, a_3_lat) * slip - E_lat * ( (B_lat*Mathf.Pow(verticalLoad, a_3_lat)*slip) - Mathf.Atan(B_lat*Mathf.Pow(verticalLoad, a_3_lat)*slip))));
        return force;
    }


    // public float complexTyreEquationNew(float slip, float fDynamicLimit, float fStaticLimit, float B, float C, float D, float E, float a_1, float a_2){
    //     float force = (fDynamicLimit/fStaticLimit)*((a_1*verticalLoad - a_2*Mathf.Pow(verticalLoad, 2))*D)* Mathf.Sin( C * Mathf.Atan(B * slip - E * ( (B*slip) - Mathf.Atan(B*slip))));
    //     return force;

    // }

    // public float tyreCurvePeak(float a_1, float a_2, float D, float verticalLoad){
    //     return (a_1*verticalLoad - a_2*Mathf.Pow(verticalLoad, 2))*D;
    // }

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

    // public float getRollingResistance(float verticalLoad, float omega, float radius, float coefficient){
    //     return (verticalLoad * omega * radius * coefficient);
    // }
    
    public float getRollingResistance(float longitudinalVelocity, float lateralVelocity, float verticalLoad, float radius, int tyrePressure){
        float speed = Mathf.Pow(Mathf.Pow(longitudinalVelocity, 2) + Mathf.Pow(lateralVelocity,2), 0.5f);        
        float rrCoefficient = 0.005f + ((1/(0.06895f*tyrePressure))*(0.01f + 0.0095f * Mathf.Pow(0.036f*speed,2))); 
        float rrForce = -(rrCoefficient) * verticalLoad;
        return radius * rrForce;

    }

    public Vector3 getUpdatedForce(float userInput, float currentGearRatio, float finalDriveRatio, float primaryGearRatio, RaycastHit hit, float timeDelta, float verticalLoad, float camberAngle){
        

        
        // Debug.Log(userInput);
        if(verticalLoad < 0){
            verticalLoad = 0;
        }
        
        this.verticalLoad = verticalLoad;
        this.camberAngle = camberAngle;
        
       
        
        wheelObject.transform.position = hit.point + hit.normal * wheelRadius;
        wheelVelocityLS = wheelObject.transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));

        lateralVelocity = wheelVelocityLS.x;
        longitudinalVelocity = wheelVelocityLS.z;              
        
       

        // fLongLimit = tyreEfficiency * tyreCurvePeak(a_1_long, a_2_long, D_long, verticalLoad);
        // fLatLimit = tyreEfficiency * tyreCurvePeak(a_1_lat, a_2_lat, D_lat, verticalLoad);

        fLongLimit = tyreCurvePeakLongitudinal(a_1_long, a_2_long, D_long, verticalLoad);
        fLatLimit = tyreCurvePeakLateral(a_1_lat, a_2_lat, a_4_lat, verticalLoad, camberAngle, D_lat);

        
        
        fLongDynamicLimit = dynamicPeakLongitudinal(lateralForce, fLongLimit, fLatLimit);
        fLatDynamicLimit = dynamicPeakLateral(longitudinalForce, fLongLimit, fLatLimit);

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
        

        if(userInput >=0){
            if(id == 2 | id == 3){
                omega = Mathf.Clamp(omega, (1/9.5493f) * engineIdleRPM/(currentGearRatio * finalDriveRatio * primaryGearRatio), (1/9.5493f) * engineMaxRPM/(currentGearRatio * finalDriveRatio * primaryGearRatio));
            }
            else{
                omega = Mathf.Clamp(omega, -100000000f,10000000000f);
            }
            
        }
        else{
            omega = Mathf.Clamp(omega, 0,10000000000f);
        }

        
          


        wheelMesh.transform.Rotate(Mathf.Rad2Deg * omega * timeDelta, 0, 0, Space.Self); 
        
        slipRatio = calculateSlipRatio(longitudinalVelocity, omega, wheelRadius);        
        // longitudinalForce = complexTyreEquation(slipRatio, fLongDynamicLimit, C_long, B_long, E_long); 
        // longitudinalForce = complexTyreEquationNew(slipRatio, fLongDynamicLimit, fLongLimit, B_long, C_long, D_long, E_long, a_1_long, a_2_long);       
        longitudinalForce = tyreEquationLongitudinal(slipRatio, fLongDynamicLimit, fLongLimit, verticalLoad, B_long, C_long, D_long, E_long, a_1_long, a_2_long, a_3_long);


        slipAngle = calculateSlipAngle(longitudinalVelocity, lateralVelocity, threshold: 0.1f);     
        // lateralForce = complexTyreEquation(slipAngle, fLatDynamicLimit, C_lat, B_lat, E_lat);
        // lateralForce = complexTyreEquationNew(slipAngle, fLatDynamicLimit, fLatLimit, B_long, C_long, D_long, E_long, a_1_long, a_2_long);
        
        
        lateralForce = tyreEquationLateral(slipAngle, fLatDynamicLimit, fLatLimit, verticalLoad, camberAngle, B_lat, C_lat, D_lat, E_lat, a_1_lat, a_2_lat, a_3_lat, a_4_lat);



        if(float.IsNaN(longitudinalForce)){
            longitudinalForce = 0;
        }
        if(float.IsNaN(lateralForce)){
            lateralForce = 0;
        }

        // if(id == 2| id == 3){
        //     longitudinalForce = Mathf.Clamp(longitudinalForce, -Mathf.Abs(diffLongForceLimit), Mathf.Abs(diffLongForceLimit));
        // }
        

        
        longitudinalForce = Mathf.Clamp(longitudinalForce, -50000, 50000);
        lateralForce = Mathf.Clamp(lateralForce, -50000,50000);

        // Debug.Log($"Wheel {id}: Slip Ratio = {slipRatio}, Slip Angle = {slipAngle}, Flong = {longitudinalForce}, Flat = {lateralForce}");
        // Debug.Log($"Wheel {id}: Camber = {camberAngle}");

        forceVector = longitudinalForce * wheelObject.transform.forward + lateralForce * wheelObject.transform.right;
        
       
        // Debug.Log($"Wheel id = {id}, Longitudinal Velocity = {longitudinalVelocity}, wR = {omega*wheelRadius}, slip ratio = {slipRatio} ");
        
        // Debug.Log($" Wheel id = {id}, Limits = ({fLongLimit},{fLatLimit}), Dynamic Limits = ({fLongDynamicLimit},{fLatDynamicLimit}), Forces = ({longitudinalForce},{lateralForce}), Load = {verticalLoad}");
        // Debug.Log($"Wheel id = {id}, Longitudinal Velocity = {longitudinalVelocity}, Lateral Velocity = {lateralVelocity}, slip ratio = {slipRatio}, slip angle = {slipAngle}");


        


        return forceVector;


    }

    // public float getSlipAngle(){return slipAngle;}

    // public float getSlipRatio(){return slipRatio;}
}