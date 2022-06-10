using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to keep track of past X values of a variable.
class History<T>
{
    Queue<T> data; // Queue that stores the data. 
    public int MaxCapacity { get; private set; } // How many past values you want to store;

    public History(int maxCapacity) 
    {
        MaxCapacity = maxCapacity; 
        data = new Queue<T>(maxCapacity);
    }

    // Adds value to back of queue.
    // Removes first item if queue was full before adding.
    public void AddEntry(T newData)
    {
        if (data.Count >= MaxCapacity)
        {
            data.Dequeue();
        }
        data.Enqueue(newData);
    }

    // Returns the first item in the queue
    public T Peek()
    {
        return data.Peek();
    }

    // Returns the entire queue as an array.
    public T[] getArray(){
        return data.ToArray();
    }
    
}