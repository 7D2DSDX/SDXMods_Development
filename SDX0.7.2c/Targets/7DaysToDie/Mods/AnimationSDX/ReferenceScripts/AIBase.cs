using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AINamespace{
	public class AIBase : MonoBehaviour {

		public GameObject moveTargetPfb;
		public GameObject agentMarkerPfb;
		[HideInInspector]
		public NavMeshAgent agent;
		[HideInInspector]
		public AIMotor motor;
		[HideInInspector]
		public HumanSheet humanSheet;
		[HideInInspector]
		public Animator animator;
		[HideInInspector]
		public AnimationStateMachine stateMachine;
		[HideInInspector]
		public AnimationEvents animationEvents;
		[HideInInspector]
		public AIBehaviourTree behaviourTree;
		[HideInInspector]
		public AISenses senses;
		[HideInInspector]
		public AIVision vision;
		[HideInInspector]
		public AINavigation navigation;
		[HideInInspector]
		public AICombat combat;

		public bool isZombie;
		public bool showMoveTarget;
		public bool showAgent;
		public bool showNearVision;
		public bool showMidVision;
		public bool showFarVision;
		public bool showVisionTargets;

		public void Setup(HumanSheet humanSheetIn, bool isZombie, GameObject squadDeployment = null){
			this.humanSheet = humanSheetIn;
			this.isZombie = isZombie;
			animator = transform.parent.GetComponent<Animator> ();
			stateMachine = transform.parent.GetComponent<AnimationStateMachine> ();
			stateMachine.Setup (this);
			animationEvents = transform.parent.GetComponent<AnimationEvents> ();
			animationEvents.Setup (this);
			agent = transform.parent.GetComponent<NavMeshAgent> ();
			navigation = GetComponent<AINavigation> ();
			navigation.Setup (showAgent);
			senses = GetComponent<AISenses> ();
			senses.Setup ();
			vision = transform.parent.GetComponentInChildren<AIVision> ();
			vision.Setup (this, showVisionTargets, showNearVision, showMidVision, showFarVision);
			combat = GetComponent<AICombat> ();
			combat.Setup (this);
			if (isZombie) {
				//Debug.Log ("zombie perception : " + humanSheet.stats.perception);
				motor = new ZombieMotor(this, transform.parent.GetComponent<Animator> ());
				behaviourTree = new BTZombie ();
			} else {
				//Debug.Log ("swat perception : " + humanSheet.stats.perception);
				motor = new HumanMotor(this, transform.parent.GetComponent<Animator> ());
				behaviourTree = new BTSwat ();
			}
			behaviourTree.squadDeployment = squadDeployment;
			behaviourTree.Setup (humanSheet, this);
			//motor.UpdateAvatarMask ();
			agent.updatePosition = false;
			//agent.updateRotation = false;
			behaviourTree.StartTree ();
		}

		void FixedUpdate(){
			stateMachine.UpdateStateMachine ();
			if (behaviourTree.memory.GetIsDead () || stateMachine.stateBlocked) {
				motor.Stop ();
			}else{
				navigation.CalculateMoveSpeed ();
				if (behaviourTree.memory.GetCombatLockState () == AIBehaviourTree.CombatLock.CloseCombat) {
					combat.UpdateCombat ();
					combat.Movement ();
				} else {
					navigation.Movement ();
				}
			}
		}

	}
}