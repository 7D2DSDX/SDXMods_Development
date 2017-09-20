using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityZombieCrawlSDX : EntityZombieCrawl
{

    // Update the Approach speed, and add a randomized speed to it
    public override float GetApproachSpeed()
    {
        if (GamePrefs.GetInt(EnumGamePrefs.ZombiesRun) == 1)
        {
            return this.speedApproach * this.Stats.SpeedModifier.Value;
        }
        else
        {
            if (this.world.IsDark())
                return this.speedApproachNight * UnityEngine.Random.Range(0.2f, 1.2f);
            else
                return this.speedApproach * UnityEngine.Random.Range(0.2f, 1.2f);
        }
    }

    // Randomize the Walk types.
    public override int GetWalkType()
    {
        int WalkType = base.GetWalkType();
        if (WalkType == 4)
            return WalkType;

        return UnityEngine.Random.Range(1, 8);
    }

}

