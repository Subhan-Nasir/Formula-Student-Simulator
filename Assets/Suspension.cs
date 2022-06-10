using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Suspension{
    // Dont attach this script to any GameObject. 
    // This is only used by the "RaycastController" script to model the suspension physics. 


    // id: 0 for fornt left,
    //     1 for front right,
    //     2 for rear left,
    //     3 for rear right
    public float id;
    public float naturalLength;
    public float springTravel; // maximum distance the spring can move from rest.
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
    public float displacement;

    public float springForce;
    public float damperForce;
    public float force;
    public Vector3 forceVector; 
    public bool contact;

    

    public Suspension(float id, float restLength, float springTravel, float springStiffness, float dampingCoefficient, float bumpStiffness, float bumpTravel, float wheelRadius){
        // Constructor for this class.
        // All the parameters listed in the brackets above need to be provided to create a suspension class object.

        // Assigns values given in the constructor to the variables in the class.
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
        // Calculate spring displacement and velocity.
        previousLength = springLength;
        springLength = hit.distance - wheelRadius;
        springLength = Mathf.Clamp(springLength, minLength - bumpTravel, maxLength);
        springVelocity = (springLength - previousLength)/timeDelta;
        displacement = naturalLength - springLength;

        // Calculate spring force from displacement and velocity.
        // Bump stiffesses only used below min length becasue
        // bumps stops only activate when the spring is about to bottom out.
        if(springLength < minLength){
            springForce = springStiffness * displacement + bumpStiffness * (minLength - springLength);
            // Debug.Log($"Bumpstop activated for suspension {id}");
                       
        }
        else{
            springForce = springStiffness * displacement;
        }

               
        damperForce = dampingCoefficient * springVelocity;
        damperForce = Mathf.Clamp(damperForce, -50000, 50000);

        force = springForce - damperForce;

        forceVector = (springForce - damperForce) * hit.normal;
        
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

    
    
}
