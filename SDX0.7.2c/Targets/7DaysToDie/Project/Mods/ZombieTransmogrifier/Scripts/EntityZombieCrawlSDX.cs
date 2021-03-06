﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityZombieCrawlSDX : EntityZombieCrawl
{
    // Stores when to do the next light check and what the current light level is
    // Determines if they run or not.
    private float nextCheck = 0;
    byte lightLevel;

    // Caching the walk types and approach speed
    private int intWalkType = 0;
    private float flApproachSpeed = 0.0f;

    // set to true if you want the zombies to run in the dark.
    bool blRunInDark = true;
    public static System.Random random = new System.Random();

    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);

        // This is the distributed random heigh multiplier. Add or adjust values as you see fit. By default, it's just a small adjustment.
        float[] numbers = new float[9] { 0.8f, 0.8f, 0.9f, 0.9f, 1.0f, 1.0f, 1.0f, 1.1f, 1.1f };
        int randomIndex = random.Next(0, numbers.Length);

        // scale down the zombies, or upscale them
        this.gameObject.transform.localScale = new Vector3(numbers[randomIndex], numbers[randomIndex], numbers[randomIndex]);
    }

    // Update the Approach speed, and add a randomized speed to it
    public override float GetApproachSpeed()
    {
        // default approach speed of this new class is 0, so if we are already above that, just re-use the value.
        if (flApproachSpeed > 0.0f)
            return flApproachSpeed;

        // Find the default approach speed from the base class to give us a reference.
        float fDefaultSpeed = base.GetApproachSpeed();

        // if it's greater than 1, just use the base value in the XML. 
        // This would otherwise make the football and wights run even faster than they do now.
        if (fDefaultSpeed > 1.0f)
            return fDefaultSpeed;


        // Set the minimum speed and maxSpeed of the bonus we want to give the zombie
        float minSpeed = 0.0f;
        float maxSpeed = 0.3f;
        
     
        // We want to cap the low and top ends. The maxSpeed is the fatest speed boost possible.
        minSpeed = Math.Max(minSpeed, 0.0f);
        maxSpeed = Math.Min(maxSpeed, 1.0f);

        // Grabs a random multiplier for the speed
        float fRandomMultiplier = UnityEngine.Random.Range( minSpeed, maxSpeed );
     
        // If the zombies are set never to run, still apply the multiplier, but don't bother doing calulations based on the night speed.
        if (GamePrefs.GetInt(EnumGamePrefs.ZombiesRun) == 1)
            flApproachSpeed = this.speedApproach + fRandomMultiplier;
        else
        {
            // Rnadomize the zombie speeds types If you have the blRunInDark set to true, then it'll randomize it too.
            if (blRunInDark && this.world.IsDark() || lightLevel < EntityZombieSDX.LightThreshold || this.Health < this.GetMaxHealth() * 0.4)
                flApproachSpeed = this.speedApproachNight + fRandomMultiplier;

            // If it's night time, then use the speedApproachNight value
            if (this.world.IsDark())
                flApproachSpeed = this.speedApproachNight + fRandomMultiplier;
            else
                flApproachSpeed = this.speedApproach + fRandomMultiplier;
        }

        // Cap the top end of the speed to be 1.35 or less, otherwise animations may go wonky.
        return Math.Min( flApproachSpeed, 1.35f);

    }
    // Randomize the Walk types.
    public override int GetWalkType()
    {
        // Grab the current walk type in the baes class
        int WalkType = base.GetWalkType();

        // If the WalkType is 4, then just return, since this is the crawler animation
        if (WalkType == 4)
            return WalkType;

        // If the WalkType is greater than the default, then return the already randomized one
        if (intWalkType > 0)
            return intWalkType;

        // Grab a random walk type, and store it for this instance.
        intWalkType = EntityZombieSDX.GetRandomWalkType();

        // Grab a random walk type
        return intWalkType;

    }

    // Calls the base class, but also does an update on how much light is on the current entity.
    // This only determines if the zombies run in the dark, if enabled.
    public override void OnUpdateLive()
    {
        base.OnUpdateLive();

        if (nextCheck < Time.time)
        {
            nextCheck = Time.time + EntityZombieSDX.CheckDelay;
            Vector3i v = new Vector3i(this.position);
            if (v.x < 0) v.x -= 1;
            if (v.z < 0) v.z -= 1;
            lightLevel = GameManager.Instance.World.ChunkClusters[0].GetLight(v, Chunk.LIGHT_TYPE.SUN);
        }

    }

}

