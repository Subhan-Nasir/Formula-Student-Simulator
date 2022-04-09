using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Suspension{

    public float id;
    public float naturalLength;
    public float springTravel;
    public float springStiffness;       
    public float dampingCoefficient;
    public float bumpStiffness; 
    public float bumpTravel;
    public float wheelRadius;
        
    public float minLength;
    public float maxLength;
    public float springLength;
    public float previousLength;
    public float springVelocity;

    public float springForce;
    public float damperForce;
    public float force;
    public Vector3 forceVector; 
    public bool contact;

    

    public Suspension(float id, float restLength, float springTravel, float springStiffness, float dampingCoefficient, float bumpStiffness, float bumpTravel, float wheelRadius){
        this.id = id;
        this.naturalLength = restLength;
        this.springTravel = springTravel;
        this.springStiffness = springStiffness;        
        this.dampingCoefficient = dampingCoefficient;
        this.bumpStiffness = bumpStiffness;
        this.bumpTravel = bumpTravel;
        this.wheelRadius = wheelRadius;
        
        this.minLength = restLength - springTravel;
        this.maxLength = restLength + springTravel;
        this.springLength = restLength;
        this.previousLength = restLength;
        
    
    }

    

    public Vector3 getUpdatedForce(RaycastHit hit, float timeDelta, bool contact){
        this.contact = contact;
        previousLength = springLength;
        springLength = hit.distance - wheelRadius;
        springLength = Mathf.Clamp(springLength, minLength - bumpTravel, maxLength);
        springVelocity = (springLength - previousLength)/timeDelta;

        if(springLength < minLength){
            springForce = springStiffness * (naturalLength - springLength) + bumpStiffness * (minLength - springLength);
            // Debug.Log($"Bumpstop activated for suspension {id}");
                       
        }
        else{
            springForce = springStiffness * (naturalLength - springLength);
        }

        // springForce = springStiffness * (naturalLength - springLength);  
        
        
        damperForce = dampingCoefficient * springVelocity;
        force = springForce - damperForce;
        forceVector = (springForce - damperForce) * hit.normal;

        
        

        // Debug.Log($"Spring id = {id}, Rest Length ={restLength}, current length = {springLength}, suspension force = {springForce - damperForce}");
        
        // Debug.Log($"Suspension {id}: force = {force}, force vector = {forceVector}, spring force = {springForce}, damper force = {damperForce}, length = {springLength}, velocity = {springVelocity}");
        return forceVector;

    }


    // Kf, Kr = front and rear roll stifness
    // Mf, Mr = front and rear mass
    // Hcg = Centre of mass height
    // HrcF, HrcR = front and rear roll centre height
    // a = acceleration
    // tf, tr = front and rear track

    public static float elasticLoadTransferFront( float Kf, float Kr, float Mf, float Mr, float Hcg, float HrcF, float HrcR, float a, float tf){
        float Welastic = (Kf/(Kf + Kr)) * ( (Mf * (Hcg - HrcF) + Mr * (Hcg -HrcR))*a/ tf);        
        return Welastic;
    }

    public static float transientElasticLoadTransfer(float rollStiffness, float rollAngle, float trackWidth){
        float  Welastic = rollAngle*rollStiffness/trackWidth;
        return Welastic;

    }

    public static float elasticLoadTransferRear(float Kf, float Kr, float Mf, float Mr, float Hcg, float HrcF, float HrcR, float a, float tr){
        float Welastic = (Kr/(Kf + Kr)) * ( (Mf * (Hcg - HrcF) + Mr * (Hcg -HrcR))*a/ tr);
        return Welastic;

    }

    public static float geometricLoadTransferFront( float Mf, float a, float Hrcf, float tf){
        float Wgeometric = (Mf * a * Hrcf)/tf;
        return Wgeometric;
    }

    public static float geometricLoadTransferRear(float Mr, float a, float Hrcr, float tr){
        float Wgeometric = (Mr * a * Hrcr )/tr;
        return Wgeometric;
    }
    public static float LongitudinalLoadTransfer(float M, float a, float Hcg, float wheelBase){
        float WLongitudinal = (M * a * Hcg )/wheelBase;
        return WLongitudinal;
    }

    public float calculateCamberFront(float x){
        float y;
        y = (4.3895E-9f) * Mathf.Pow(x,4) +  1.7466E-7f * Mathf.Pow(x,3) + (4.5144E-4f) * Mathf.Pow(x,2) + (2.6678E-2f) * x + 3.1363f;
        return y;
    }

    public float calculateCamberRear(float x){
        float y;
        y = (1.0222E-8f) * Mathf.Pow(x,4) - (7.5916E-7f) * Mathf.Pow(x,3) + (4.4086E-4f) * Mathf.Pow(x,2) - (2.7342E-3f) * x + (1.7161f);
        return y;
    }

    public float calculateSpringDisplacementFront(float x){
        float y;
        y = (-1.6983E-7f) * Mathf.Pow(x,4) - (4.2658E-5f) * Mathf.Pow(x,3) + (1.7252E-3f) * Mathf.Pow(x,2) + (9.7968E-1f) * x - (7.2350E-3f);
        return y;
    }

    public float calculateSpringDisplacementRear(float x){
        float y;
        y = -(1.7066E-7f) * Mathf.Pow(x,4) - (4.0199E-5f) * Mathf.Pow(x,3) + (1.5618E-3f) * Mathf.Pow(x,2) + (9.8534E-1f) * x - (1.8784E-1f);
        return y;
    }

    public float calculateToeFront(float x){

        float y;
        y = (7.4904E-9f) * Mathf.Pow(x,4) + (8.6268E-8f) * Mathf.Pow(x,3) + (1.0578E-3f) * Mathf.Pow(x,2) + (3.4286E-2f) * x + (3.2588f);
        return y;

    }






    
}
