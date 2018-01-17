using System;
using UnityEngine;

/// <summary>
/// Custom class for simple unpowered traps
/// Mortelentus 2017 - v1.0
/// We inherit Block class. This is the Basic Vanilla block without any special behaviour.
/// In the XML, Block classes are always referenced by removing the block word. In this case it would be "TrapTutorial, Mods"
/// </summary>
public class BlockGuppyTrap : BlockPressurePlate
{
    private string openSound;
    private string closeSound;

     public override bool OnEntityCollidedWithBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos,
        BlockValue _blockValue, Entity _targetEntity)
    {

        Debug.Log("PLayer on block!");
      //  OperateTrap(_world, _clrIdx, _blockPos, _blockValue, _targetEntity, false);
        if (!base.OnEntityCollidedWithBlock(_world, _clrIdx, _blockPos, _blockValue, _targetEntity))
        {
            return false;

        }

       
      //  _world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
        return true;
    }

    private bool LG(WorldBase worldBase, int num, Vector3i blockPos, BlockValue blockValue, bool flag )
    {
        Debug.Log("lg iN!");
        ChunkCluster chunkCluster = worldBase.ChunkClusters[num];
        if (chunkCluster == null)
        {
            return false;
        }
        if (chunkCluster.GetChunkSync(World.toChunkXZ(blockPos.x), World.toChunkY(blockPos.y), World.toChunkXZ(blockPos.z)) == null)
        {
            return false;
        }
        bool flag2 = (blockValue.meta & 1) != 0;
        bool isTriggered = (blockValue.meta & 2) != 0;
        if (Steam.Network.IsServer)
        {
            TileEntityPoweredTrigger tileEntityPoweredTrigger = worldBase.GetTileEntity(num, blockPos) as TileEntityPoweredTrigger;
            if (tileEntityPoweredTrigger != null)
            {
                tileEntityPoweredTrigger.IsTriggered = isTriggered;
            }
        }

        BlockEntityData _ebcd = worldBase.ChunkClusters[num].GetBlockEntity(blockPos);
        if (isTriggered)
        {
            PlayAnimation(_ebcd, true);
        }
        else
        {
            PlayAnimation(_ebcd, false);
        }
        return true;
    }
    
    public override void OnBlockValueChanged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue,
        BlockValue _newBlockValue)
    {
        // the animations need to be triggered here so that they are shown to all players
        // what happens is that, everytime an action occurs, we update the blockvale.
        // that causes this function to be triggered for ALL players, and every client can then play the correct animation locally.
        base.OnBlockValueChanged(_world, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);

        if (!(this.shape is BlockShapeModelEntity) || _oldBlockValue.type == _newBlockValue.type && (int)_oldBlockValue.meta == (int)_newBlockValue.meta || _newBlockValue.ischild)
            return;

        // trigger animation
        //playAnimation(_world, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);


        this.LG(_world, _clrIdx, _blockPos, _newBlockValue, false);
    
    }


    // custom function to play the animation.
    private void playAnimation(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _blockValue)
    {
        // we get the block entity data so that we can find the object transform and search for an animator.
        BlockEntityData _ebcd = _world.ChunkClusters[_clrIdx].GetBlockEntity(_blockPos);
        Animator[] componentsInChildren;
        if (_ebcd == null || !_ebcd.bHasTransform ||
            (componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>(false)) == null)
        {
            return;
        }

        Debug.Log("Playing Animation");
        // once we find an animator, lets use the correct trigger for the action.
        foreach (Animator animator in componentsInChildren)
        {
            if (IsTrapFired(_blockValue.meta) && !IsTrapFired(_oldBlockValue.meta))
            {
                // if the trap was triggered while ready, we'll play open animation
                // if you remember the animator, we used the trigger openT to run the open animation.
                // if a open sound exists, it will also be played!
                Audio.Manager.BroadcastPlay(_blockPos.ToVector3(), openSound);
                animator.SetBool("Active", true);
                animator.SetTrigger("Active");
                Debug.Log("True!");
            }
            else if (!IsTrapFired(_blockValue.meta) && IsTrapFired(_oldBlockValue.meta))
            {
                // if the trap is being reseted by a player, we'll play close animation
                // if you remember the animator, we used the trigger closeT to run the open animation.
                // if a close sound exists, it will also be played!
                Audio.Manager.BroadcastPlay(_blockPos.ToVector3(), closeSound);
                Debug.Log("False!");
                animator.SetBool("Active", false);
            }
        }
    }

   private void PlayAnimation(BlockEntityData _ebcd, bool blPlay )
    {
        Animator[] componentsInChildren;
        if (_ebcd == null || !_ebcd.bHasTransform ||
            (componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>(false)) == null)
        {
            return;
        }

        // we get the block entity data so that we can find the object transform and search for an animator.
        foreach (Animator animator in componentsInChildren)
        {
            if (blPlay)
            {
                animator.SetBool("Active", true);
                animator.SetTrigger("Active");
            }
            else
            {
                animator.SetBool("Active", false);
                //animator.SetTrigger("Active");

            }
        }
    }
    // custom function to operate the trap. If it is resulting from a collision "fireTrap" will be true, if its player interaction it will be false.
    private void OperateTrap(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _entity, bool fireTrap)
    {
        // here's where we do operation actions. This will only run if the originator is alive!
        if (fireTrap)
        {
            // if the operation results from block collision, we will try to "open" the trap.
            // but only if it's not already opened (waiting for reset)
            if (!IsTrapFired(_blockValue.meta))
            {

                // On the Collider, we want to do our next check at the delay interval, which is about 10 ticks.
               // nextCheck = Time.time + this.Delay;

                #region Fire trap;                                                         
                // I will be using the bit 0, and set it to 1 here. This will let everyone know that the trap is open (waiting for reset)
                _blockValue.meta = (byte)(_blockValue.meta | (1 << 0));
                // as soon as we "commit" this information, all clients will be informed and the correct animations will play
                _world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
                #endregion;
            }
        }
        else
        {
            // if the operation results from player interaction (reseting the trap)
            if (IsTrapFired(_blockValue.meta))
            {
                #region Reset trap;                                                
                // I will be using the bit 0, and reset it to 0. This will let everyone know that the trap is ready and will fire if collided.
                _blockValue.meta = (byte)(_blockValue.meta & ~(1 << 0));
                // as soon as we "commit" this information, all clients will be informed and the correct animations will play
                _world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
                #endregion;
            }
        }
    }

 

    /// <summary>
    /// Check if trap is already triggered
    /// </summary>
    /// <param name="_metadata"></param>
    /// <returns>True if triggered</returns>
    public static bool IsTrapFired(byte _metadata)
    {
        return ((int)_metadata & 1 << 0) != 0;
    }

 
}