using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Code adapted from:
// https://www.youtube.com/watch?v=N6l0e7dDGl8 

public class NotificationTriggerEvent : MonoBehaviour{

    [Tooltip("UI element that displays the notification")]
    [Header("UI Content")]            
    [SerializeField] private Text notificationTextUI;
    // [SerializeField] private Image characterIconUI;

    [Tooltip("Original template had an option for an icon/image")]  
    [Header("Message Customisation")]    
    [SerializeField] private Sprite yourIcon;
    

    [Header("Notification Removal")]
    
    [SerializeField] private bool disableAfterTimer = false;
    [SerializeField] private float disableTimer = 1.0f;

    [Header("Notification Animation")]
    [SerializeField] private Animator notificationAnim;
    


    public void showNotification(string message){
        StartCoroutine(EnableNotification(message));

    }

    IEnumerator EnableNotification(string notificationMessage){

        notificationAnim.Play("NotificationFadeIn");
        notificationTextUI.text = notificationMessage;
        // characterIconUI.sprite = yourIcon;

        if(disableAfterTimer){
            yield return new WaitForSeconds(disableTimer);
        }
        RemoveNotification();
    }

    void RemoveNotification(){
        notificationAnim.Play("NotificationFadeOut");
        
    }


}
