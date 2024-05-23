using System;
using UnityEngine;

[Serializable]
public sealed class Timer
{
    private float currentTime;

    public float tickTime;


    // Initialize
    public Timer(float tickTime)
    {
        this.tickTime = tickTime;
    }


    // Update
    /// <summary> true if ended </summary>
    public bool Tick()
    {
        currentTime += Time.deltaTime;
            
        if (currentTime >= tickTime)
        {
            Reset();
            return true;
        }

        return false;
    }

    public void Reset()
    {
        currentTime = 0;
    }
}