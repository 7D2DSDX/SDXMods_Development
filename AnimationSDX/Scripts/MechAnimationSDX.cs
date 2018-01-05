
using System;
using UnityEngine;
using System.Collections;
public class MechAnimationSDX : MonoBehaviour, IAvatarController
{

    Animator anim;

    bool isInDeathAnim = false;
    float idleTime;
    float AnimationTime = 1f;
    AnimatorStateInfo currentBaseState;


    private string MechAnimEnabled = "false";
    // Default animation strings.
    private string AnimationIdle = "";
    private string AnimationSecondIdle = "";
    private string AnimationMainAttack = "";
    private string AnimationSecondAttack = "";
    private string AnimationPain = "";
    private string AnimationJump = "";
    private string AnimationDeath = "";
    private string AnimationRun = "";
    private string AnimationWalk = "";
    private string AnimationSpecialAttack = "";

    private string strModelName = "";

    private EntityAlive entityAlive;
    private bool IsVisible;
    private bool HasDied;
    private bool isAlwaysWalk;
    private Transform ModelTransform;
    private Transform GraphicsTransform;
    private float lastPlayerX;
    private float lastPlayerZ;
    private float lastDistance;
    private float DoesntSeemToDoAnything;



    private bool blDisplayLog = true;
    private void Log(String strLog)
    {
        if (blDisplayLog)
        {
            Debug.Log(strLog);
        }
    }
    public MechAnimationSDX()
    {
        this.entityAlive = this.transform.gameObject.GetComponent<EntityAlive>();
        EntityClass entityClass = EntityClass.list[this.entityAlive.entityClass];

        if (entityClass.Properties.Values.ContainsKey("AnimationMainAttack"))
            this.AnimationMainAttack = entityClass.Properties.Values["AnimationMainAttack"];

        if (entityClass.Properties.Values.ContainsKey("AnimationSecondAttack"))
            this.AnimationSecondAttack = entityClass.Properties.Values["AnimationSecondAttack"];

        if (entityClass.Properties.Values.ContainsKey("AnimationIdle"))
            this.AnimationIdle = entityClass.Properties.Values["AnimationIdle"];

        if (entityClass.Properties.Values.ContainsKey("AnimationSecondIdle"))
            this.AnimationSecondIdle = entityClass.Properties.Values["AnimationSecondIdle"];

        if (entityClass.Properties.Values.ContainsKey("AnimationPain"))
            this.AnimationPain = entityClass.Properties.Values["AnimationPain"];

        if (entityClass.Properties.Values.ContainsKey("AnimationDeath"))
            this.AnimationDeath = entityClass.Properties.Values["AnimationDeath"];

        if (entityClass.Properties.Values.ContainsKey("AnimationRun"))
            this.AnimationRun = entityClass.Properties.Values["AnimationRun"];

        if (entityClass.Properties.Values.ContainsKey("AnimationWalk"))
            this.AnimationWalk = entityClass.Properties.Values["AnimationWalk"];

        if (entityClass.Properties.Values.ContainsKey("AnimationJump"))
            this.AnimationJump = entityClass.Properties.Values["AnimationJump"];

        if (entityClass.Properties.Values.ContainsKey("AnimationSpecialAttack"))
            this.AnimationSpecialAttack = entityClass.Properties.Values["AnimationSpecialAttack"];


        this.IsVisible = true;
    }

    void Awake()
    {
        Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        try
        {

            this.GraphicsTransform = this.transform.Find("Graphics");

            if (this.GraphicsTransform == null)
            {
                Log(" !! Graphics Transform null!");
                this.HasDied = true;
                return;
            }
            this.ModelTransform = this.GraphicsTransform.Find("Model").GetChild(0);
            if (this.ModelTransform == null)
            {
                Log(" !! Model Transform is null!");
                this.HasDied = true;
                return;
            }

            //this bit is important for SDXers! It adds the component that links each collider with the Entity class so hits can be registered.
            AddTransformRefs(this.ModelTransform);

            //if you're using A14 or haven't set specific tags for the collision in Unity un-comment this and it will set them all to being body contacts
            //using this method means things like head shot multiplers won't work but it will enable basic collision
            AddTagRecursively(this.ModelTransform, "E_BP_Body");
            Log("Searching for Idle");


            // Searchs for the animator
            this.anim = FindAnimator(this.ModelTransform);
            if (this.anim == null)
            {
                Log("*** Animator Not Found! Invalid Class");
                throw (new Exception("Animator Not Found! Wrong class is being used! Try AnimationSDX instead..."));
            }

            Log("Animator is: " + this.anim.enabled.ToString());
            
            this.anim.enabled = true;
            //else
            //{
            //    Log("Loading MyAnimationController");
            //    RuntimeAnimatorController runtimeAnimController = (RuntimeAnimatorController)Resources.Load("MyAnimationController");
            //    this.anim.runtimeAnimatorController = runtimeAnimController;
            //    Log("MyAnimationController Loaded");
            //}

            //Log("Number of Animations: " + this.anim.runtimeAnimatorController.animationClips.Length.ToString() );
            Log("Searching for Root Motion");
            if (this.entityAlive.RootMotion)
            {
                AvatarRootMotion avatarRootMotion = this.ModelTransform.GetComponent<AvatarRootMotion>();
                if (avatarRootMotion == null)
                {
                    Log("Root MOtion not detected. Creating AvatarRootMotion");
                    avatarRootMotion = this.ModelTransform.gameObject.AddComponent<AvatarRootMotion>();
                }

                Log("Initializing Root Motion");
                avatarRootMotion.Init(this, this.anim);
            }

        }
        catch (Exception ex)
        {
            Log("Exception thrown in Awake() " + ex.ToString());
        }
    }

    // Loop around the object, searching for the Animator, which will hold our references to the animation clips.
    private Animator FindAnimator(Transform t)
    {
        Animator localAnimator;
        if (t.GetComponent<Animator>() != null)
        {
            Log("Found Animator: " + t.name.ToString());
            return t.GetComponent<Animator>();
        }
        foreach (Transform tran in t)
        {
            localAnimator = FindAnimator(tran);
            if (localAnimator)
            {
                Log("Found Animator: " + tran.name.ToString());
                return localAnimator;
            }
        }

        return null;
    }


    private void AddTransformRefs(Transform t)
    {

        Log("Checking " + t.name + " tag " + t.tag);
        if (t.GetComponent<Collider>() != null && t.GetComponent<RootTransformRefEntity>() == null)
        {
            RootTransformRefEntity root = t.gameObject.AddComponent<RootTransformRefEntity>();
            root.RootTransform = this.transform;
            Log("Added root ref on " + t.name + " tag " + t.tag);
        }
        foreach (Transform tran in t)
        {
            AddTransformRefs(tran);
        }
    }

    void AddTagRecursively(Transform trans, string tag)
    {
        // If the objects are untagged, then tag them, otherwise ignore setting the tag.
        if (trans.gameObject.tag.Contains("Untagged"))
        {
            // Check to see if the part contains "head", and let it be a headshot tag
            // otherwise, fall back to default body
            if (trans.name.ToLower().Contains("head"))
                trans.gameObject.tag = "E_BP_Head";
            else
                trans.gameObject.tag = tag;
        }
        
        Log("Transoform Tag: " + trans.name + " : " + trans.tag);
        foreach (Transform t in trans)
            AddTagRecursively(t, tag);
    }

    public void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void SetAlwaysWalk(bool _b)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        this.isAlwaysWalk = _b;
    }

    public Texture2D SetSkinTexture(Texture2D _newTexture, bool _bMakeACopy)
    {
       
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return null;
    }

    public Texture2D GetTexture()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
      
        return null;
    }

    public void SetInRightHand(Transform _transform)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void SetDrunk(float _numBeers)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public virtual void SetMinibikeAnimation(string _anim, float _amount)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public virtual void SetMinibikeAnimation(string _anim, bool _isPlaying)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    // this method checks if the animator has the animation and is currently playing
    public bool isValidAnimation(string strAnimation)
    {
        if (this.anim == null || this.anim.parameters == null)
        {
            Log("WARNING: Animator is invalid or has no Parameters");
            return false;
        }

        return true;
        foreach (AnimatorControllerParameter param in this.anim.parameters)
        {
            if (param.name == strAnimation)
            {
                Log("Animation is a Trigger");
                return true;
            }

        }

        Log("WARNING: Trigger Name: " + strAnimation + " is not found in the Animator");
        return false;
    }
   
    // Check if the animation is valid and if it's currently active. 
    public bool IsValidAndPlaying( String strAnimation )
    {

        if ( isValidAnimation( strAnimation ))
        {
            if (this.anim.GetCurrentAnimatorStateInfo(0).IsName(strAnimation))
                return true;
        }

        return false;
    }
    public bool IsAnimationAttackPlaying()
    {

        // Checks to see if the animation is valid and playing, and returns either true or false
        return (IsValidAndPlaying(this.AnimationMainAttack) || IsValidAndPlaying(this.AnimationSecondAttack));


    }

    public void StartAnimationAttack()
    {

        // We check if the main and secondary attacks are set and configured. If they are, we do a 50/50 chance of either one playing.
        if (isValidAnimation(this.AnimationMainAttack) && isValidAnimation(this.AnimationSecondAttack))
        {
            if (UnityEngine.Random.value > 0.5)
                PlayAnimation(this.AnimationMainAttack);
            else
                PlayAnimation(this.AnimationSecondAttack);
        }
        // If one is missing, assume that the main attack is configured and the one we want to try to call
        else if (isValidAnimation(this.AnimationMainAttack))
        {
            PlayAnimation(this.AnimationMainAttack);
        }


    }

    public bool IsAnimationSpecialAttackPlaying()
    {
        return IsValidAndPlaying(this.AnimationSpecialAttack);

    }

    public void StartAnimationSpecialAttack(bool _b)
    {
        if (_b)
        {
                PlayAnimation(this.AnimationSecondAttack);
        }
    }

    public void StartAnimationSpecialAttack()
    {
           PlayAnimation(this.AnimationSpecialAttack);
    }

    public virtual bool IsAnimationSpecialAttack2Playing()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public virtual void StartAnimationSpecialAttack2()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public bool IsAnimationRagingPlaying()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public void StartAnimationRaging()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public bool IsAnimationHarvestingPlaying()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public void StartAnimationHarvesting()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void SetAnimationClimbing(bool _b)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public Transform GetFirstPersonCameraTransform()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return null;
    }

    public Transform GetRightHandTransform()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return null;
    }

    public bool IsAnimationUsePlaying()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        return false;
    }

    public void StartAnimationUse()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void SetRagdollEnabled(bool _b)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void StartAnimationFiring()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

    }

    public void StartAnimationReloading()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float random)
    {
        // If it's dead, or if there's no Animation pain, don't do anything.
        if (this.HasDied )
            return;

        PlayAnimation(this.AnimationPain);

    }

    public bool IsAnimationHitRunning()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        return false;
    }

    public void StartAnimationJumping()
    {
         PlayAnimation(this.AnimationJump);
    }

    public void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetAlive()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetLookPosition(Vector3 _pos)
    {
        //// Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetVisible(bool _b)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (_b == this.IsVisible && _b == this.GraphicsTransform.gameObject.activeSelf)
            return;
        this.IsVisible = _b;
        this.GraphicsTransform.gameObject.SetActive(_b);

    }

    public void SetWalkingSpeed(float _f)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetHeadAngles(float _nick, float _yaw)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetArmsAngles(float _rightArmAngle, float _leftArmAngle)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetAiming(bool _bEnable)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetCrouching(bool _bEnable)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void SetFPV(bool _fpv)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

 
    // We want to do some extra checking on the PlayAnimation, to make sure it's valid, that it isn't already playing, etc
    public void PlayAnimation(String strAnimation)
    {
        // If this animation is already playing, no need to re-start it.
        if (IsValidAndPlaying(strAnimation)) 
            return;

        this.anim.SetTrigger(strAnimation);

        return;


        //this.anim.CrossFade(strAnimation, 0.1f);
    }

    protected void Update()
    {
        if (!this.IsVisible || this.entityAlive == null )
            return;

        if (this.entityAlive.IsDead() && this.HasDied)
        {
            Log("Update: Entity is Dead");
            this.HasDied = true;

            // Stop the current animation
            this.anim.Stop();

            if (this.entityAlive.HasDeathAnim)
                this.entityAlive.emodel.DoRagdoll(DamageResponse.New(true));

            PlayAnimation(this.AnimationDeath);
            return;
        }   

        // If an animation is already playing, we don't need to process it any more until its done.
        if (IsValidAndPlaying(this.AnimationMainAttack))
            return;
        if (IsValidAndPlaying(this.AnimationSecondAttack))
            return;
        if (IsValidAndPlaying(this.AnimationDeath))
            return;
        if (IsValidAndPlaying(this.AnimationPain))
            return;

        float playerDistanceX = 0.0f;
        float playerDistanceZ = 0.0f;
        float encroached = this.lastDistance;

        // Calculates how far away the entity is to determine if we should be running or not.
        playerDistanceX = Mathf.Abs(this.entityAlive.position.x - this.entityAlive.lastTickPos[0].x) * 6f;
        playerDistanceZ = Mathf.Abs(this.entityAlive.position.z - this.entityAlive.lastTickPos[0].z) * 6f;

        if (!this.entityAlive.isEntityRemote)
        {
            if (Mathf.Abs(playerDistanceX - this.lastPlayerX) > 0.00999999977648258 || Mathf.Abs(playerDistanceZ - this.lastPlayerZ) > 0.00999999977648258)
            {
                encroached = Mathf.Sqrt(playerDistanceX * playerDistanceX + playerDistanceZ * playerDistanceZ);
                this.lastPlayerX = playerDistanceX;
                this.lastPlayerZ = playerDistanceZ;
                this.lastDistance = encroached;
            }
        }
        else if (playerDistanceX <= this.lastPlayerX && playerDistanceZ <= this.lastPlayerZ)
        {
            this.lastPlayerX *= 0.9f;
            this.lastPlayerZ *= 0.9f;
            this.lastDistance *= 0.9f;
        }
        else
        {
            encroached = Mathf.Sqrt((playerDistanceX * playerDistanceX + playerDistanceZ * playerDistanceZ));
            this.lastPlayerX = playerDistanceX;
            this.lastPlayerZ = playerDistanceZ;
            this.lastDistance = encroached;
        }

        Log("Encroached distance is: " + encroached);
        // If the entity is still too far away, we need to either walk or run there.
        if ((encroached > 0.150000005960464 || isAlwaysWalk))
        {
            // Run to the player or entity unless always walk is false.
            if (encroached > 1.0 )
            {
                Log("Runnning!");
                // Since the encroached is above 1, we want the zombie to run if need be, to get to the player faster.
                PlayAnimation(this.AnimationRun);
            }
            else
            {
                Log("walking");
                PlayAnimation(this.AnimationWalk);
            }

            // oh Hal... and Hal Code.....
            if (this.DoesntSeemToDoAnything > 0.0)
                return;
            this.DoesntSeemToDoAnything = 0.3f;
        }
        else
        {
            // Idle if we don't need to run or walk anywhere
            Log("Idling as default");
            PlayAnimation(this.AnimationIdle);
        }
    }

    private void SetCurrentState()
    {
        this.currentBaseState = this.anim.GetCurrentAnimatorStateInfo(0);
    }

   
    public Transform GetActiveModelRoot()
    {
        return this.ModelTransform;
    }

    public void RemoveLimb(EnumBodyPartHit _bodyPart, bool restoreState)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void CrippleLimb(EnumBodyPartHit _bodyPart, bool restoreState)
    {
    }

    public void TurnIntoCrawler(bool restoreState)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void BeginStun(EnumEntityStunType stun, EnumBodyPartHit _bodyPart, bool _criticalHit, float random)
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void EndStun()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void StartEating()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void StopEating()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public void PlayPlayerFPRevive()
    {
        // Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
    public void SetArchetypeStance(Archetype.StanceTypes stance)
    {

    }

    // sphereii code
    public bool IsAnimationElectrocutedPlaying()
    {
        return false;
    }

    public void StartAnimationElectrocuted()
    {
    }

    public void TriggerSleeperPose(int pose)
    {

    }

    public void NotifyAnimatorMove(UnityEngine.Animator test)
    {

    }
}
