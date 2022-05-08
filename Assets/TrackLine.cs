using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackLine : MonoBehaviour{

    // https://www.youtube.com/watch?v=MxU_6ya95LI&t=244s

    public LineRenderer lineRenderer;
    public GameObject trackPath;


    // Start is called before the first frame update
    void Start(){
        lineRenderer = GetComponent<LineRenderer>();

        trackPath = this.gameObject;

        int num_of_path = trackPath.transform.childCount;
        lineRenderer.positionCount = num_of_path + 1;
        for(int x = 0; x<num_of_path; x++){
            lineRenderer.SetPosition(x, new Vector3 (
                trackPath.transform.GetChild(x).transform.position.x,
                trackPath.transform.position.y + 2,
                trackPath.transform.GetChild(x).transform.position.z));
        };

        lineRenderer.SetPosition(num_of_path, lineRenderer.GetPosition(0));
          
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
