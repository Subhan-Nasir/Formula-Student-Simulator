using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Barracuda;




public class OnnxTest : MonoBehaviour
{

    public NNModel camberModel;

    private Model runtimeModel;
    private IWorker worker; 
    private string outputLayerName;


    // private Tensor inputTensor = new Tensor(new int[] {0},new int[] {1},new int[] {1},new int[] {3});
    private Tensor inputTensor = new Tensor(new int[] {3,1});

    // 0.182f, 927.91f, 0.027f
    // Start is called before the first frame update
    void Start(){
        runtimeModel = ModelLoader.Load(camberModel);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
        outputLayerName = runtimeModel.outputs[runtimeModel.outputs.Count - 1];         
    }

    // Update is called once per frame
    void Update(){

        // inputTensor[0] = 1;
        // inputTensor[1] = 1;
        // inputTensor[2] = 1;
        Predict();
              
        
    }


    void Predict(){
        
        worker.Execute(inputTensor);
        Tensor outputTensor = worker.PeekOutput();
        // Debug.Log(outputTensor);
        Debug.Log("model worked");
    
    }


    public void OnDestroy(){
        worker?.Dispose();
        inputTensor.Dispose();
    }
}
