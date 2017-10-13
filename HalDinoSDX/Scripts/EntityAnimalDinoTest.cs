using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public class EntityAnimalDinoTest : EntityAnimal
{
    public EntityAnimalDinoTest() : base()
    {

    }
    

    protected override void Awake()
    {
        base.Awake();
        
    }

    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
    {
        return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
    }
    public override Vector3 GetMapIconScale()
    {
        return new Vector3(0.45f, 0.45f, 1f);
    }
}
