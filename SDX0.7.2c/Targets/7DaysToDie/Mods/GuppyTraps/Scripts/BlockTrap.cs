using System;
using UnityEngine;

/// <summary>
/// Custom class for simple unpowered traps
/// Mortelentus 2017 - v1.0
/// We inherit Block class. This is the Basic Vanilla block without any special behaviour.
/// In the XML, Block classes are always referenced by removing the block word. In this case it would be "TrapTutorial, Mods"
/// </summary>
public class BlockGuppyTrap : Block
{
    private string openSound;
    private string closeSound;
    private int intAnimation = 0;
  
    private Animator[] animators;

    // public override bool OnEntityCollidedWithBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos,
    //    BlockValue _blockValue, Entity _targetEntity)
    //{

    //}

    public override BlockValue OnBlockPlaced(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, System.Random _rnd)
    {
        BlockEntityData _ebdc = _world.ChunkClusters[_clrIdx].GetBlockEntity(_blockPos);
        animators = _ebdc.transform.GetComponentsInChildren<Animator>(false);


        return _blockValue;
    }

    //private bool LG(WorldBase worldBase, int num, Vector3i blockPos, BlockValue blockValue, bool flag )
    //{
    //    Debug.Log("lg iN!");
    //    ChunkCluster chunkCluster = worldBase.ChunkClusters[num];
    //    if (chunkCluster == null)
    //    {
    //        return false;
    //    }
    //    if (chunkCluster.GetChunkSync(World.toChunkXZ(blockPos.x), World.toChunkY(blockPos.y), World.toChunkXZ(blockPos.z)) == null)
    //    {
    //        return false;
    //    }
    //    bool isTriggered = (blockValue.meta & 2) != 0;
    //    if (Steam.Network.IsServer)
    //    {
    //        TileEntityPoweredTrigger tileEntityPoweredTrigger = worldBase.GetTileEntity(num, blockPos) as TileEntityPoweredTrigger;
    //        if (tileEntityPoweredTrigger != null)
    //        {
    //            tileEntityPoweredTrigger.IsTriggered = isTriggered;
    //        }
    //    }

    //    if (isTriggered)
    //        this.intAnimation = 1;
    //    else
    //        this.intAnimation = 0;

    //    return true;
    //}

   
    public override void OnEntityWalking(WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue, Entity entity)
    {
        Debug.Log("I'm walkin' here...");
        if (!(entity is EntityAlive))
        {
            return ;
        }
        EntityAlive entityAlive = (EntityAlive)entity;
        if (entityAlive.IsDead())
        {
            return ;
        }

        if (this.animators.Length == 0)
            return;
        foreach (Animator animator in this.animators)
        {

            Debug.Log(animator.name.ToString());
            Debug.Log("Found the Animator");
            animator.SetTrigger("Active");
        }
    }
    // custom function to play the animation.
    //private void playAnimation(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _blockValue)
    //{
    //    // we get the block entity data so that we can find the object transform and search for an animator.
    //    BlockEntityData _ebcd = _world.ChunkClusters[_clrIdx].GetBlockEntity(_blockPos);
    //    Animator[] componentsInChildren;
    //    if (_ebcd == null || !_ebcd.bHasTransform ||
    //        (componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>(false)) == null)
    //    {
    //        return;
    //    }

    //    Debug.Log("Playing Animation");
    //    // once we find an animator, lets use the correct trigger for the action.
    //    foreach (Animator animator in componentsInChildren)
    //    {
    //        if (IsTrapFired(_blockValue.meta) && !IsTrapFired(_oldBlockValue.meta))
    //        {
    //            // if the trap was triggered while ready, we'll play open animation
    //            // if you remember the animator, we used the trigger openT to run the open animation.
    //            // if a open sound exists, it will also be played!
    //            Audio.Manager.BroadcastPlay(_blockPos.ToVector3(), openSound);
    //            animator.SetBool("Active", true);
    //          //  animator.SetTrigger("Active");
    //            Debug.Log("True!");
    //        }
    //        else if (!IsTrapFired(_blockValue.meta) && IsTrapFired(_oldBlockValue.meta))
    //        {
    //            // if the trap is being reseted by a player, we'll play close animation
    //            // if you remember the animator, we used the trigger closeT to run the open animation.
    //            // if a close sound exists, it will also be played!
    //            Audio.Manager.BroadcastPlay(_blockPos.ToVector3(), closeSound);
    //            Debug.Log("False!");
    //            animator.SetBool("Active", false);
    //        }
    //    }
    //}

    //private void PlayAnimation(BlockEntityData _ebcd, int intMeta )
    //{
    //    Animator[] componentsInChildren;
    //    if (_ebcd == null || !_ebcd.bHasTransform ||
    //        (componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>(false)) == null)
    //    {
    //        return;
    //    }

    //    Debug.Log("Animation Meta: " + intMeta);
    //    // we get the block entity data so that we can find the object transform and search for an animator.
    //    foreach (Animator animator in componentsInChildren)
    //    {
    //        animator.SetInteger("Meta", intMeta);
    //    }
    //}
    //// custom function to operate the trap. If it is resulting from a collision "fireTrap" will be true, if its player interaction it will be false.
    //private void OperateTrap(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _entity, bool fireTrap)
    //{
    //    // here's where we do operation actions. This will only run if the originator is alive!
    //    if (fireTrap)
    //    {
    //        // if the operation results from block collision, we will try to "open" the trap.
    //        // but only if it's not already opened (waiting for reset)
    //        if (!IsTrapFired(_blockValue.meta))
    //        {

    //            // On the Collider, we want to do our next check at the delay interval, which is about 10 ticks.
    //           // nextCheck = Time.time + this.Delay;

    //            #region Fire trap;                                                         
    //            // I will be using the bit 0, and set it to 1 here. This will let everyone know that the trap is open (waiting for reset)
    //            _blockValue.meta = (byte)(_blockValue.meta | (1 << 0));
    //            // as soon as we "commit" this information, all clients will be informed and the correct animations will play
    //            _world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
    //            #endregion;
    //        }
    //    }
    //    else
    //    {
    //        // if the operation results from player interaction (reseting the trap)
    //        if (IsTrapFired(_blockValue.meta))
    //        {
    //            #region Reset trap;                                                
    //            // I will be using the bit 0, and reset it to 0. This will let everyone know that the trap is ready and will fire if collided.
    //            _blockValue.meta = (byte)(_blockValue.meta & ~(1 << 0));
    //            // as soon as we "commit" this information, all clients will be informed and the correct animations will play
    //            _world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
    //            #endregion;
    //        }
    //    }
    //}

 

    /// <summary>
    /// Check if trap is already triggered
    /// </summary>
    /// <param name="_metadata"></param>
    /// <returns>True if triggered</returns>
    //public static bool IsTrapFired(byte _metadata)
    //{
    //    return ((int)_metadata & 1 << 0) != 0;
    //}

 
}