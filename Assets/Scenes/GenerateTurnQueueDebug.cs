using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTurnQueueDebug : MonoBehaviour
{
    public CachedInitiative[] unitInitiatives = new CachedInitiative[5];
    public CachedInitiative[] cacheCopy = new CachedInitiative[5];
    public List<CachedInitiative> turnQueue = new List<CachedInitiative>();
    private float turnThreshold = 100f;

    void Start()
    {
        for (int i = 0; i < 5; i++) {
            CachedInitiative newCache = new CachedInitiative
            {
                initiative = turnThreshold - (20 * i),
                speed = 20 + i,
                index = i
            };
            unitInitiatives[i] = newCache;
        }

        foreach (CachedInitiative unit in unitInitiatives) {
            double initiativeDifference = turnThreshold - unit.initiative;
            unit.ticksToNextTurn = initiativeDifference / unit.speed;
        }

        Array.Sort(unitInitiatives, delegate (CachedInitiative a, CachedInitiative b) {
            return a.ticksToNextTurn.CompareTo(b.ticksToNextTurn);
        });

        /*Debug.Log(unitInitiatives[0].index + "," + unitInitiatives[1].index + "," + unitInitiatives[2].index + "," + unitInitiatives[3].index + "," + unitInitiatives[4].index);
        Debug.Log(unitInitiatives[0].ticksToNextTurn + "," + unitInitiatives[1].ticksToNextTurn + "," + unitInitiatives[2].ticksToNextTurn + "," + unitInitiatives[3].ticksToNextTurn + "," + unitInitiatives[4].ticksToNextTurn);
        Debug.Log(unitInitiatives[0].initiative + "," + unitInitiatives[1].initiative + "," + unitInitiatives[2].initiative + "," + unitInitiatives[3].initiative + "," + unitInitiatives[4].initiative);*/

        GenerateTurnQueue();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateTurnQueue()
    {
        cacheCopy = new CachedInitiative[unitInitiatives.Length];
        Array.Copy(unitInitiatives, cacheCopy, cacheCopy.Length);

        /*Debug.Log(cacheCopy[0].index + "," + cacheCopy[1].index + "," + cacheCopy[2].index + "," + cacheCopy[3].index + "," + cacheCopy[4].index);
        Debug.Log(cacheCopy[0].ticksToNextTurn + "," + cacheCopy[1].ticksToNextTurn + "," + cacheCopy[2].ticksToNextTurn + "," + cacheCopy[3].ticksToNextTurn + "," + cacheCopy[4].ticksToNextTurn);
        Debug.Log(cacheCopy[0].initiative + "," + cacheCopy[1].initiative + "," + cacheCopy[2].initiative + "," + cacheCopy[3].initiative + "," + cacheCopy[4].initiative);*/

        turnQueue.Clear();
        for (int i = 0; i < 10; i++) {

            // Simulate taking a turn.
            turnQueue.Add(cacheCopy[0]);
            Debug.Log("Added unit index " + cacheCopy[0].index + " to turnQueue.");

            cacheCopy[0].initiative -= turnThreshold;

            foreach (CachedInitiative cache in cacheCopy) {
                double initiativeDifference = turnThreshold - cache.initiative;
                cache.ticksToNextTurn = initiativeDifference / cache.speed;
            }

            /*Debug.Log(cacheCopy[0].index + "," + cacheCopy[1].index + "," + cacheCopy[2].index + "," + cacheCopy[3].index + "," + cacheCopy[4].index);
            Debug.Log(cacheCopy[0].ticksToNextTurn + "," + cacheCopy[1].ticksToNextTurn + "," + cacheCopy[2].ticksToNextTurn + "," + cacheCopy[3].ticksToNextTurn + "," + cacheCopy[4].ticksToNextTurn);
            Debug.Log(cacheCopy[0].initiative + "," + cacheCopy[1].initiative + "," + cacheCopy[2].initiative + "," + cacheCopy[3].initiative + "," + cacheCopy[4].initiative);*/

            // Sort the new array.
            Array.Sort(cacheCopy, delegate (CachedInitiative a, CachedInitiative b) {
                return a.ticksToNextTurn.CompareTo(b.ticksToNextTurn);
            });

            /*Debug.Log(cacheCopy[0].index + "," + cacheCopy[1].index + "," + cacheCopy[2].index + "," + cacheCopy[3].index + "," + cacheCopy[4].index);
            Debug.Log(cacheCopy[0].ticksToNextTurn + "," + cacheCopy[1].ticksToNextTurn + "," + cacheCopy[2].ticksToNextTurn + "," + cacheCopy[3].ticksToNextTurn + "," + cacheCopy[4].ticksToNextTurn);
            Debug.Log(cacheCopy[0].initiative + "," + cacheCopy[1].initiative + "," + cacheCopy[2].initiative + "," + cacheCopy[3].initiative + "," + cacheCopy[4].initiative);*/

            // Advance to the next turn.
            if (cacheCopy[0].initiative >= turnThreshold) { return; }
            else {
                double ticksToAdvance = cacheCopy[0].ticksToNextTurn;
                foreach (CachedInitiative cache in cacheCopy) {
                    cache.initiative += cache.speed * ticksToAdvance;
                }
            }

            Debug.Log(cacheCopy[0].index + "," + cacheCopy[1].index + "," + cacheCopy[2].index + "," + cacheCopy[3].index + "," + cacheCopy[4].index);
            Debug.Log(cacheCopy[0].ticksToNextTurn + "," + cacheCopy[1].ticksToNextTurn + "," + cacheCopy[2].ticksToNextTurn + "," + cacheCopy[3].ticksToNextTurn + "," + cacheCopy[4].ticksToNextTurn);
            Debug.Log(cacheCopy[0].initiative + "," + cacheCopy[1].initiative + "," + cacheCopy[2].initiative + "," + cacheCopy[3].initiative + "," + cacheCopy[4].initiative);
            // Units are ready to advance to the next turn here, though ticksToNextTurn is out of date!
        }
    }
}
