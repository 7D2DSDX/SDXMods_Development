using System;
using System.Collections.Generic;
using UnityEngine;

class MecanimSDX : MonoBehaviour, IAvatarController
{
    // If the displayLog is true, verbosely log out put. Disable in production!
    private bool blDisplayLog = true;


    // If the entity is visible
    protected bool m_bVisible;

    // Animator reference
    protected Animator anim;

    // Our character references
    protected global::EntityAlive entityAlive;
    protected Transform bipedTransform;
    protected Transform modelTransform;

    // Animator's current state
    protected AnimatorStateInfo currentBaseState;

    protected float idleTime;
    protected bool isInDeathAnim;

    // Support for letting entity shoot weapons
    private string RightHand = "RightHand";
    private Transform rightHandItemTransform;
    protected Animator rightHandAnimator;

    // Flag to determine if we are eating or not
    protected bool isEating = false;
    private float ActionTime = 0f;


    // Stores our hashes as ints, since that's what the Animator wants
    private HashSet<int> AttackHash;
    private HashSet<int> SpecialAttackHash;
    private HashSet<int> PainHash;
    private HashSet<int> DeathHash;
    private HashSet<int> IdleHash;
    private HashSet<int> MovementHash;

    // Maintain a list of strings of the same animation
    private List<String> AttackStrings;
    private List<String> SpecialAttackStrings;
    private List<String> PainStrings;
    private List<String> DeathStrings;
    private List<String> IdleStrings;
    private List<String> MovementStrings;

    // used to calculate how far way the player is
    private float lastPlayerX;
    private float lastPlayerZ;
    private float lastDistance;
    private bool isAlwaysWalk;


    private void Log(String strLog)
    {
        if (blDisplayLog)
        {
            Debug.Log(strLog);
        }
    }

  
    // The following are the number of indexes we are getting, populating from the XML files.
    private int AttackIndexes = 0;
    private int SpecialAttackIndexes = 0;
    private int SpecialSecondIndexes = 0;
    private int RagingIndexes = 0;
    private int ElectrocutionIndexes = 0;
    private int CrouchIndexes = 0;
    private int StunIndexes = 0;
    private int SleeperIndexes = 0;
    private int HarvestIndexes = 0;
    private int PainIndexes = 0;
    private int DeathIndexes = 0;
    private int RunIndexes = 0;
    private int WalkIndexes = 0;
    private int IdleIndexes = 0;
    private int JumpIndexes = 0;

    private HashSet<int> GenerateLists( EntityClass entityClass, String strAnimationType, List<String> list )
    {
        list = new List<String>();
        HashSet<int> hash = new HashSet<int>();

        if (entityClass.Properties.Values.ContainsKey( strAnimationType))
        {
            foreach (String strAnimationState in entityClass.Properties.Values[strAnimationType].Split(','))
            {
                AttackStrings.Add(strAnimationState.Trim());
                hash.Add( Animator.StringToHash(strAnimationState));

            }
        }

        return hash;

    }
    private MecanimSDX()
    {
        this.entityAlive = this.transform.gameObject.GetComponent<EntityAlive>();
        EntityClass entityClass = EntityClass.list[this.entityAlive.entityClass];

        // Grabs what Right Hand Joint we have in our XML
        if (entityClass.Properties.Values.ContainsKey("RightHandJointName"))
        {
            this.RightHand = entityClass.Properties.Values["RightHandJointName"];
            this.rightHandItemTransform = FindTransform(this.bipedTransform, this.bipedTransform, RightHand);
        }

        this.AttackHash = GenerateLists(entityClass, "Attacks", this.AttackStrings);


        // The following will read our Index values from the XML to determine the maximum attack animations.
        // The range should be 1-based, meaning a value of 1 will specify the index value 0.
        // <property name="AttackIndexes" value="20", means there are 20 animations, running from 0 to 19
        int.TryParse(entityClass.Properties.Values["AttackIndexes"], out this.AttackIndexes);
        int.TryParse(entityClass.Properties.Values["SpecialAttackIndexes"], out this.SpecialAttackIndexes);

        int.TryParse(entityClass.Properties.Values["SpecialSecondIndexes"], out this.SpecialSecondIndexes);
        int.TryParse(entityClass.Properties.Values["RagingIndexes"], out this.RagingIndexes);

        int.TryParse(entityClass.Properties.Values["ElectrocutionIndexes"], out this.ElectrocutionIndexes);
        int.TryParse(entityClass.Properties.Values["CrouchIndexes"], out this.CrouchIndexes);

        int.TryParse(entityClass.Properties.Values["StunIndexes"], out this.StunIndexes);
        int.TryParse(entityClass.Properties.Values["SleeperIndexes"], out this.SleeperIndexes);

        int.TryParse(entityClass.Properties.Values["HarvestIndexes"], out this.HarvestIndexes);
        int.TryParse(entityClass.Properties.Values["PainIndexes"], out this.PainIndexes);

        int.TryParse(entityClass.Properties.Values["DeathIndexes"], out this.DeathIndexes);
        int.TryParse(entityClass.Properties.Values["RunIndexes"], out this.RunIndexes);

        int.TryParse(entityClass.Properties.Values["WalkIndexes"], out this.WalkIndexes);
        int.TryParse(entityClass.Properties.Values["IdleIndexes"], out this.IdleIndexes);

        int.TryParse(entityClass.Properties.Values["JumpIndexes"], out this.JumpIndexes);
    }

   
    // Token: 0x060011F6 RID: 4598 RVA: 0x00081E3C File Offset: 0x0008003C
    private void Awake()
    {
        Log("Method: " + System.Reflection.MethodBase.GetCurrentMethod().Name);

        try
        {

            this.bipedTransform = base.transform.Find("Graphics");
            this.modelTransform = base.transform.Find("Graphics/Model").GetChild(0);
            if (this.modelTransform == null)
            {
                Log(" !! Model Transform is null!");
                isInDeathAnim = true;
                return;
            }

            //this bit is important for SDXers! It adds the component that links each collider with the Entity class so hits can be registered.
            AddTransformRefs(this.modelTransform);

            //if you're using A14 or haven't set specific tags for the collision in Unity un-comment this and it will set them all to being body contacts
            //using this method means things like head shot multiplers won't work but it will enable basic collision
            AddTagRecursively(this.modelTransform, "E_BP_Body");


            // Searchs for the animator
            this.anim = this.modelTransform.GetComponent<Animator>();
            if (this.anim == null)
            {
                Log("*** Animator Not Found! Invalid Class");
                throw (new Exception("Animator Not Found! Wrong class is being used! Try AnimationSDX instead..."));
            }

            this.anim.enabled = true;

            // This may allow us to swap out the Animator Controller, in the future, at run time, potentially allowing us to have multiple controllers for variety.
            //this.anim.runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load("Assets/BodyGuardTest/SDXAnimatorController");
            
            if (this.anim.runtimeAnimatorController)
            {
                Log("My Animator Controller has: " + this.anim.runtimeAnimatorController.animationClips.Length + " Animations");
            }
            else
            {
                Log("My Animator Controller is null!");
                throw (new Exception("Animator Controller is null!"));
            }

         
            
        }
        catch (Exception ex)
        {
            Log("Exception thrown in Awake() " + ex.ToString());
        }
    }


    // helper method to find particular transforms by name, in our game object
    private Transform FindTransform(Transform root, Transform t, string objectName)
    {
        if (t.name.Contains(objectName))
        {
            return t;
        }
        foreach (Transform tran in t)
        {
            Transform result = FindTransform(root, tran, objectName);
            if (result != null) return result;
        }
        return null;
    }

    // not Implemented
    public void RemoveLimb(EnumBodyPartHit _bodyPart, bool restoreState) { }

    // Triggers the Eating animation state
    public void StartEating()
    {
        if (!this.isEating)
        {
            this.anim.SetBool("IsEating", true);
            this.isEating = true;
        }
    }

    // Ends the eating animation state
    public void StopEating()
    {
        if (this.isEating)
        {
            this.anim.SetBool("IsEating", false);
            this.isEating = false;
        }
    }

    // Enables the entity, and checks if we have root motion enabled or not. 
    public void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
    {

        // If Root MOtion is enabled on this entity, initialize it.
        if (this.entityAlive.RootMotion)
        {
            global::AvatarRootMotion avatarRootMotion = this.bipedTransform.GetComponent<global::AvatarRootMotion>();
            if (avatarRootMotion == null)
            {
                avatarRootMotion = this.bipedTransform.gameObject.AddComponent<global::AvatarRootMotion>();
            }
            avatarRootMotion.Init(this, this.anim);
        }

        this.anim.SetBool("IsDead", this.entityAlive.IsDead());
        this.anim.SetBool("IsAlive", this.entityAlive.IsAlive());

        // Add in support for animations that can fire weapons
        if (this.rightHandItemTransform != null)
        {
            //Debug.Log("RIGHTHAND ITEM TRANSFORM");
            this.rightHandItemTransform.parent = this.rightHandItemTransform;
            Vector3 position = global::AnimationGunjointOffsetData.AnimationGunjointOffset[this.entityAlive.inventory.holdingItem.HoldType.Value].position;
            Vector3 rotation = global::AnimationGunjointOffsetData.AnimationGunjointOffset[this.entityAlive.inventory.holdingItem.HoldType.Value].rotation;
            this.rightHandItemTransform.localPosition = position;
            this.rightHandItemTransform.localEulerAngles = rotation;
        }
    }

    private void UpdateBaseState()
    {
        this.currentBaseState = this.anim.GetCurrentAnimatorStateInfo(0);
    }

    // Check if the Animation attack is still playing.
    public bool IsAnimationAttackPlaying()
    {
        return this.ActionTime > 0f || (!this.anim.IsInTransition(0)&& this.AttackHash.Contains(this.currentBaseState.fullPathHash));
    }

    // Picks a random attack index, and then fires off the attack trigger. 
    public void StartAnimationAttack()
    {
        if (!this.entityAlive.isEntityRemote)
        {
            this.ActionTime = 1f;
        }


        // Randomly set the index for the AttackIndex, which allows us different attacks
        SetRandomIndex("AttackIndex");
        this.anim.SetTrigger("Attack");
    }

    // Check if the Using animation state isplaying.
    public bool IsAnimationUsePlaying()
    {
        return false;
    }

    // Starts the Use animation
    public void StartAnimationUse()
    {
    }

    // Checks if the Special Attack animation is playing
    public bool IsAnimationSpecialAttackPlaying()
    {
        return this.IsAnimationAttackPlaying();
    }

    // starts any special attack that we have
    public void StartAnimationSpecialAttack(bool _b)
    {
        if (_b)
        {
            SetRandomIndex("SpecialAttackIndex");
            this.anim.SetTrigger("SpecialAttack");
        }
    }


    public bool IsAnimationSpecialAttack2Playing()
    {
        return this.IsAnimationAttackPlaying();
    }

    public void StartAnimationSpecialAttack2()
    {
        SetRandomIndex("SpecialSecondAttack");
        this.anim.SetTrigger("SpecialSecondAttack");
    }

    // Checks if any of the Raging Animations are playing
    public bool IsAnimationRagingPlaying()
    {
        return false;
    }

    public void StartAnimationRaging()
    {
        SetRandomIndex("RagingIndex");
        this.anim.SetTrigger("Raging");
    }

    public virtual bool IsAnimationElectrocutedPlaying()
    {
        return false;
    }

    public virtual void StartAnimationElectrocuted()
    {
        SetRandomIndex("ElectrocutionIndex");
        this.anim.SetTrigger("Electrocution");
    }

    public bool IsAnimationHarvestingPlaying()
    {
        return false;
    }

    // Starts a Harvest trigger, if there is one
    public void StartAnimationHarvesting()
    {
        SetRandomIndex("HarvestIndex");
        this.anim.SetTrigger("Harvest");
    }

    // Token: 0x06001210 RID: 4624 RVA: 0x000825BC File Offset: 0x000807BC
    public Texture2D GetTexture()
    {
        return null;
    }

    // Wakes up the entity and sets it alive
    public void SetAlive()
    {
        this.anim.SetBool("IsAlive", true);
        this.anim.SetBool("IsDead", false);
        this.anim.SetTrigger("Alive");
    }

   
    // Determines and triggers whether we fire off the drunk animation
    public void SetDrunk(float _numBeers)
    {
        if (_numBeers > 3)
        {
            SetRandomIndex("DrunkIndex");
            this.anim.SetTrigger("Drunk");
        }
    }

    // No implemented
    public void SetMinibikeAnimation(string _animSuffix, bool _isPlaying) { }
    public void SetMinibikeAnimation(string _animSuffix, float _amount) { }
    public void SetHeadAngles(float _nick, float _yaw) { }
    public void SetArmsAngles(float _rightArmAngle, float _leftArmAngle) { }
    public void SetAiming(bool _bEnable) { }
    public void SetWalkingSpeed(float _f) { }

// Determines if the entity is crouching or not  
    public void SetCrouching(bool _bEnable)
    {
        if ( _bEnable)
        {
            SetRandomIndex("CrouchIndex");
        }

        this.anim.SetBool("IsCrouching", _bEnable);

    }

    // Token: 0x0600121A RID: 4634 RVA: 0x00082614 File Offset: 0x00080814
    public void SetVisible(bool _b)
    {
        if (this.m_bVisible != _b )
        {
            this.m_bVisible = _b;
            Transform transform = this.bipedTransform;
            if (transform != null)
            {
                Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].enabled = _b;
                }
            }
        }
    }

    public void SetRagdollEnabled(bool _b)
    {
    }

    public void StartAnimationReloading()
    {
    }

    // starts the jumping animation
    public void StartAnimationJumping()
    {
        SetRandomIndex("JumpIndex");
        this.anim.SetTrigger("Jump");
    }

    // The shooting animation
    public void StartAnimationFiring()
    {
    }

    // ANimation for when the entity gets hit
    public void StartAnimationHit(global::EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random)
    {
        SetRandomIndex("PainIndex");
        this.anim.SetTrigger("Pain");

    }

    // Animation check to see for when the entity gets hit while running
    public bool IsAnimationHitRunning()
    {
        return false;
    }

    // Starts the death animation
    public void StartDeathAnimation(global::EnumBodyPartHit _bodyPart, int _movementState, float _random)
    {
        SetRandomIndex("DeathIndex");
        this.anim.SetBool("IsDead", true);

    }

    // Code from Mortelentus, that allows us to set an animator in the right hand for weapons
    public void SetInRightHand(Transform _transform)
    {
        if (this.rightHandItemTransform == null) return;
        this.idleTime = 0f;
        if (_transform != null)
        {
            _transform.parent = this.rightHandItemTransform;
        }
        this.rightHandItemTransform = _transform;
        this.rightHandAnimator = ((!(_transform != null)) ? null : _transform.GetComponent<Animator>());
        if (this.rightHandItemTransform != null)
        {
            Utils.SetLayerRecursively(this.rightHandItemTransform.gameObject, 0);
        }
    }

    // Reutrns the righthand transform
    public Transform GetRightHandTransform()
    {
        return this.rightHandItemTransform;
    }

    public Transform GetActiveModelRoot()
    {
        return (!this.modelTransform) ? this.bipedTransform : this.modelTransform;
    }

    public void SetLookPosition(Vector3 _pos)
    {
    }

    public void CrippleLimb(global::EnumBodyPartHit _bodyPart, bool restoreState)
    {
    }

    public void TurnIntoCrawler(bool restoreState)
    {
    }

    // starts the Stun Animation
    public void BeginStun(global::EnumEntityStunType stun, global::EnumBodyPartHit _bodyPart, bool _criticalHit, float random)
    {
        SetRandomIndex("StunIndex");
        this.anim.SetBool("IsStunned", true);
    }

    // Ends the stunned animation
    public void EndStun()
    {
        this.anim.SetBool("IsStunned", false);

    }

    public void PlayPlayerFPRevive()
    {
    }

    public void SetArchetypeStance(global::Archetype.StanceTypes stance)
    {
    }

    public void NotifyAnimatorMove(Animator instigator)
    {
        this.entityAlive.NotifyRootMotion(instigator);
    }

    // Sets the current animator State
    private void UpdateCurrentState()
    {
        this.currentBaseState = this.anim.GetCurrentAnimatorStateInfo(0);
    }

    // The Update route
    protected virtual void LateUpdate()
    {
        this.ActionTime -= Time.deltaTime;
        if (!this.m_bVisible && (this.entityAlive == null|| this.entityAlive.isEntityRemote))
        {
            Log("Entity isn't visible or enabled!");
            return;
        }
        if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
        {
            Log("Biped Transform was null!");
            return;
        }

        if (!(this.anim == null) && this.anim.avatar.isValid && this.anim.enabled)
        {
            UpdateCurrentState();
            float num = 0f;
            float num2 = 0f;
            if (!this.entityAlive.IsFlyMode.Value)
            {
                num = this.entityAlive.speedForward;
                num2 = this.entityAlive.speedStrafe;
            }
            float num3 = num2;
            if (num3 >= 1234f)
            {
                num3 = 0f;
            }

            this.anim.SetFloat("Forward", num);
            this.anim.SetFloat("Strafe", num3);
            float num4 = num * num + num3 * num3;
            if (!this.entityAlive.IsDead())
            {
                //SetRandomIndex("WalkIndex");
                //SetRandomIndex("RunIndex");
      
                Log("MovementState: " + ((num4 <= this.entityAlive.speedApproach * this.entityAlive.speedApproach) ? ((num4 <= this.entityAlive.speedWander * this.entityAlive.speedWander) ? ((num4 <= 0.001f) ? 0 : 2) : 1) : 0).ToString());
                this.anim.SetInteger("MovementState", (num4 <= this.entityAlive.speedApproach * this.entityAlive.speedApproach) ? ((num4 <= this.entityAlive.speedWander * this.entityAlive.speedWander) ? ((num4 <= 0.001f) ? 0 : 2) : 1) : 0);

            }
            if (Mathf.Abs(num) <= 0.01f && Mathf.Abs(num2) <= 0.01f)
            {
                //SetRandomIndex("IdleIndex");
                this.anim.SetBool("IsMoving", false);
            }
            else
            {
                this.idleTime = 0f;

                this.anim.SetBool("IsMoving", true);

                
            }
            bool flag = false;
            if (this.anim != null && this.isInDeathAnim && !this.anim.IsInTransition(0)) // this.DeathHash.Contains(this.currentBaseState.fullPathHash) )
            {
                flag = true;
            }
            if (this.anim != null && this.isInDeathAnim && flag && (this.currentBaseState.normalizedTime >= 1f || this.anim.IsInTransition(0)))
            {
                this.isInDeathAnim = false;
                if (this.entityAlive.HasDeathAnim)
                {
                    this.entityAlive.emodel.DoRagdoll(global::DamageResponse.New(true));
                }
            }
            this.anim.SetFloat("IdleTime", this.idleTime);
            this.idleTime += Time.deltaTime;

            return;
        }
    }

    /*
     * I'm not convinced this is the best way of handling getting all our index values, however, to maximize the flexibility, I can't see another way of doing it.
     * 
     * I've wrapped all the Index calls to this helper method, as well as the GetRandomIndex value, in the hopes of easier re-factoring in the future
     */
    public void SetRandomIndex( String strParam)
    {
        int intRandom = 0;
        switch( strParam)
        {
            case "AttackIndex":
                intRandom = GetRandomIndex( this.AttackIndexes);
                break;
            case "SpecialAttackIndex":
                intRandom = GetRandomIndex(this.SpecialAttackIndexes);
                break;
            case "SpecialSecondAttack":
                intRandom = GetRandomIndex(this.SpecialSecondIndexes);
                break;
            case "RagingIndex":
                intRandom = GetRandomIndex(this.RagingIndexes);
                break;
            case "ElectrocutionIndex":
                intRandom = GetRandomIndex(this.ElectrocutionIndexes);
                break;
            case "CrouchIndex":
                intRandom = GetRandomIndex(this.CrouchIndexes);
                break;
            case "StunIndex":
                intRandom = GetRandomIndex(this.StunIndexes);
                break;
            case "SleeperIndex":
                intRandom = GetRandomIndex(this.SleeperIndexes);
                break;
            case "HarvestIndex":
                intRandom = GetRandomIndex(this.HarvestIndexes);
                break;
            case "PainIndex":
                intRandom = GetRandomIndex(this.PainIndexes);
                break;
            case "DeathIndex":
                intRandom = GetRandomIndex(this.DeathIndexes);
                break;
            case "RunIndex":
                intRandom = GetRandomIndex(this.RunIndexes);
                break;
            case "WalkIndex":
                intRandom = GetRandomIndex(this.WalkIndexes);
                break;
            case "IdleIndex":
                intRandom = GetRandomIndex(this.IdleIndexes);
                break;
            case "JumpIndex":
                intRandom = GetRandomIndex(this.JumpIndexes);
                break;
            default:
                intRandom = 0;
                break;

        }

        this.anim.SetInteger(strParam, intRandom);
    }

    public int GetRandomIndex( int intMax )
    {
        return UnityEngine.Random.Range(0, intMax -1 );
    }


    // Sets the Sleeper index
    public void TriggerSleeperPose(int pose)
    {
        this.anim.SetInteger("SleeperIndex", UnityEngine.Random.Range(0, 4));
    }

    // Token: 0x06001231 RID: 4657 RVA: 0x00082ABC File Offset: 0x00080CBC
    private global::EntityClass GetAvailableTriggers()
    {
        return global::EntityClass.list[this.entityAlive.entityClass];
    }

    private void AddTransformRefs(Transform t)
    {

        if (t.GetComponent<Collider>() != null && t.GetComponent<RootTransformRefEntity>() == null)
        {
            RootTransformRefEntity root = t.gameObject.AddComponent<RootTransformRefEntity>();
            root.RootTransform = this.transform;
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

  
}


