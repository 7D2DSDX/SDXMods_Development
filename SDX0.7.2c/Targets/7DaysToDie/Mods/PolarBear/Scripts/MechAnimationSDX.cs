
using System;
using UnityEngine;
using System.Collections;
public class PolarBearAnim : AvatarAnimalController
{
    Animator polarbear;
    PolarBearAnim()
    {
        polarbear = this.modelTransform.GetComponent<Animator>();
    }

    protected virtual void LateUpdate()
    {
        if (!this.m_bVisible && (this.entityAlive == null || !this.entityAlive.RootMotion || this.entityAlive.isEntityRemote))
        {
            return;
        }
        if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
        {
            return;
        }
        if (!(this.anim == null) && this.anim.avatar.isValid && this.anim.enabled)
        {
            float FowardSpeed = 0f;
            float StrafeSpeed = 0f;
            if (!this.entityAlive.IsFlyMode.Value)
            {
                FowardSpeed = this.entityAlive.speedForward;
                StrafeSpeed = this.entityAlive.speedStrafe;
            }
            float num3 = StrafeSpeed;
            if (num3 >= 1234f)
            {
                num3 = 0f;
            }
            this.anim.SetFloat("Forward", FowardSpeed);
            this.anim.SetFloat("Strafe", num3);
            float TotalSpeed = FowardSpeed * FowardSpeed + num3 * num3;
            if (!this.entityAlive.IsDead())
            {
                this.anim.SetInteger("MovementState", (TotalSpeed <= this.entityAlive.speedApproach * this.entityAlive.speedApproach) ? ((TotalSpeed <= this.entityAlive.speedWander * this.entityAlive.speedWander) ? ((TotalSpeed <= 0.001f) ? 0 : 1) : 2) : 3);
            }
            if (Mathf.Abs(FowardSpeed) <= 0.01f && Mathf.Abs(StrafeSpeed) <= 0.01f)
            {
                walk();
            }
            else
            {
                this.idleTime = 0f;
                this.anim.SetBool("IsMoving", true);
            }
            bool flag = false;
            if (this.anim != null && this.isInDeathAnim && this.DeathHash.Contains(this.currentBaseState.fullPathHash) && !this.anim.IsInTransition(0))
            {
                flag = true;
            }
            if (this.anim != null && this.isInDeathAnim && flag && (this.currentBaseState.normalizedTime >= 1f || this.anim.IsInTransition(0)))
            {
                this.isInDeathAnim = false;
                if (this.entityAlive.HasDeathAnim)
                {
                    this.entityAlive.emodel.DoRagdoll(DamageResponse.New(true));
                }
            }
            this.anim.SetFloat("IdleTime", this.idleTime);
            this.idleTime += Time.deltaTime;
            return;
        }
    }

    public void Update()
    {


        if (Input.GetKey(KeyCode.Alpha1))
        {
            polarbear.SetBool("Walk", false);
            polarbear.SetBool("Look", true);
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            polarbear.SetBool("Walk", false);
            polarbear.SetBool("Look", false);
            polarbear.SetBool("Sniffing", true);
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            polarbear.SetBool("Running", false);
            polarbear.SetBool("Sniffs", true);
            polarbear.SetBool("Sniffing", false);
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            polarbear.SetBool("Sniffs", false);
            polarbear.SetBool("Running", false);
            polarbear.SetBool("Bite", true);
        }
        if (Input.GetKey(KeyCode.Alpha5))
        {
            polarbear.SetBool("Running", false);
            polarbear.SetBool("Bite", false);
            polarbear.SetBool("Roar", true);
            polarbear.SetBool("SwipeRight", false);
            polarbear.SetBool("SwipeLeft", false);
            polarbear.SetBool("DoubleSwipe", false);
            polarbear.SetBool("Hit", false);
        }
        if (Input.GetKey(KeyCode.Alpha6))
        {
            polarbear.SetBool("Roar", false);
            polarbear.SetBool("SwipeRight", true);
            polarbear.SetBool("SwipeLeft", false);
            polarbear.SetBool("DoubleSwipe", false);
            polarbear.SetBool("Hit", false);
        }
        if (Input.GetKey(KeyCode.Alpha7))
        {
            polarbear.SetBool("Roar", false);
            polarbear.SetBool("SwipeRight", false);
            polarbear.SetBool("SwipeLeft", true);
            polarbear.SetBool("DoubleSwipe", false);
            polarbear.SetBool("Hit", false);
        }
        if (Input.GetKey(KeyCode.Alpha8))
        {
            polarbear.SetBool("Roar", false);
            polarbear.SetBool("SwipeRight", false);
            polarbear.SetBool("SwipeLeft", false);
            polarbear.SetBool("DoubleSwipe", true);
            polarbear.SetBool("Hit", false);
        }
        if (Input.GetKey(KeyCode.Alpha9))
        {
            polarbear.SetBool("Roar", false);
            polarbear.SetBool("SwipeRight", false);
            polarbear.SetBool("SwipeLeft", false);
            polarbear.SetBool("DoubleSwipe", false);
            polarbear.SetBool("Hit", true);
        }
        if (Input.GetKey(KeyCode.Alpha0))
        {
            polarbear.SetBool("Hit", false);
            polarbear.SetBool("Idle", true);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            polarbear.SetBool("Idle", false);
            polarbear.SetBool("Lay", true);
        }
        if (Input.GetKey(KeyCode.W))
        {
            polarbear.SetBool("Lay", false);
            polarbear.SetBool("Sleep", true);
        }
        if (Input.GetKey(KeyCode.E))
        {
            polarbear.SetBool("Sleep", false);
            polarbear.SetBool("Wakeup", true);
        }
        if (Input.GetKey(KeyCode.R))
        {
            polarbear.SetBool("Wakeup", false);
            polarbear.SetBool("Eat", true);
            polarbear.SetBool("SniffsUp", false);
        }
        if (Input.GetKey(KeyCode.T))
        {
            polarbear.SetBool("Eat", false);
            polarbear.SetBool("SniffsUp", true);
        }
        if (Input.GetKey(KeyCode.Y))
        {
            polarbear.SetBool("Eat", false);
            polarbear.SetBool("SniffsUp", false);
            polarbear.SetBool("Idle2", true);
            polarbear.SetBool("Die", false); ;
        }
        if (Input.GetKey(KeyCode.U))
        {
            polarbear.SetBool("Idle2", false);
            polarbear.SetBool("Die", true); ;
        }
        if (Input.GetKey("up"))
        {
            polarbear.SetBool("Trotting", false);
            polarbear.SetBool("Running", true);
            polarbear.SetBool("Bite", false);
            polarbear.SetBool("Walk", false);
        }
        if (Input.GetKey("down"))
        {
            polarbear.SetBool("Sniffs", false);
            polarbear.SetBool("Trotting", true);
            polarbear.SetBool("Running", false);
            polarbear.SetBool("Walk", false);
        }
        if (Input.GetKey("left"))
        {
            polarbear.SetBool("Trotting", false);
            polarbear.SetBool("Running", false);
            polarbear.SetBool("turnleft", true);
            left();
        }
        if (Input.GetKey("right"))
        {
            polarbear.SetBool("Trotting", false);
            polarbear.SetBool("Running", false);
            polarbear.SetBool("turnright", true);
            right();
        }
        if (Input.GetKey(KeyCode.Keypad5))
        {
            polarbear.SetBool("Walk", true);
            polarbear.SetBool("Look", false);
            polarbear.SetBool("Running", false);
            polarbear.SetBool("Sniffing", false);
            polarbear.SetBool("Trotting", false);
        }
        if (Input.GetKey(KeyCode.Keypad4))
        {
            polarbear.SetBool("Walk", false);
            polarbear.SetBool("walkturnleft", true);
            walkleft();
        }
        if (Input.GetKey(KeyCode.Keypad6))
        {
            polarbear.SetBool("Walk", false);
            polarbear.SetBool("walkturnright", true);
            walkright();
        }
        if (Input.GetKey(KeyCode.N))
        {
            polarbear.SetBool("trotleft", true);
            polarbear.SetBool("Trotting", false);
            trot();
        }
        if (Input.GetKey(KeyCode.M))
        {
            polarbear.SetBool("trotright", true);
            polarbear.SetBool("Trotting", false);
            trot();
        }
    }




    public void walk()
    {
        polarbear.SetBool("Walk", true);
        polarbear.SetBool("rolling", false);
    }
    public void left()
    {
        polarbear.SetBool("Running", true);
        polarbear.SetBool("turnleft", false);
    }
    public void right()
    {
        polarbear.SetBool("Running", true);
        polarbear.SetBool("turnright", false);
    }
    public void walkleft()
    {
        polarbear.SetBool("Walk", true);
        polarbear.SetBool("walkturnleft", false);
    }
    public void walkright()
    {

        polarbear.SetBool("Walk", true);
        polarbear.SetBool("walkturnright", false);
    }

    public void trot()
    {

        polarbear.SetBool("Trotting", true);
        polarbear.SetBool("trotleft", false);
        polarbear.SetBool("trotright", false);
    }


}
