using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AINamespace{
	public class AnimationStateMachine : MonoBehaviour {

		AIBase aiBase;

		int animState = -1;
		int prevState = -2;
		int animLayer = -1;
		int prevLayer = -2;

		public static int idle = Animator.StringToHash("Base.B_Idle");
		public static int unarmedIdle = Animator.StringToHash("Base.B_Unarmed.U_Idle");
		public static int unarmedWalk = Animator.StringToHash("Base.B_Unarmed.U_Walk");
		public static int unarmedRun = Animator.StringToHash("Base.B_Unarmed.U_Run");
		public static int unarmedCloseCombatIdle = Animator.StringToHash("Base.B_Unarmed.U_CloseCombat.U_CC_Idle");
		public static int unarmedCloseCombatDeath = Animator.StringToHash("Base.B_Unarmed.U_CloseCombat.U_CC_Death");
		public static int unarmedCloseCombatForward = Animator.StringToHash("Base.B_Unarmed.U_CloseCombat.U_CC_Forward");
		public static int unarmedCloseCombatBack = Animator.StringToHash("Base.B_Unarmed.U_CloseCombat.U_CC_Back");
		public static int unarmedCloseCombatLeft = Animator.StringToHash("Base.B_Unarmed.U_CloseCombat.U_CC_Left");
		public static int unarmedCloseCombatRight = Animator.StringToHash("Base.B_Unarmed.U_CloseCombat.U_CC_Right");
		public static int unarmedCloseCombatAttack = Animator.StringToHash("Base.B_Unarmed.U_CloseCombat.U_CC_Attack");
		public static int unarmedCloseCombatBlock = Animator.StringToHash("Base.B_Unarmed.U_CloseCombat.U_CC_Block");
		public static int unarmedCloseCombatHit = Animator.StringToHash("Base.B_Unarmed.U_CloseCombat.U_CC_Hit");

		float targetMatch = 0.5f;
		public bool stateBlocked = false;

		public void Setup(AIBase aiBase){
			this.aiBase = aiBase;
		}

		public void UpdateStateMachine () {
			if (aiBase.animator != null) {
				//Debug.Log ("human state machine updating");
				animLayer = GetDominantLayer ();
				//Debug.Log (animator.GetCurrentAnimatorStateInfo (0).ToString());
				if (animLayer != prevLayer) {
					LayerExit (prevLayer);
					LayerEnter ();
				} else {
					LayerUpdate (animLayer);
				}
				animState = aiBase.animator.GetCurrentAnimatorStateInfo (0).fullPathHash;
				if (animState != prevState) {
					StateExit (prevState);
					StateEnter (animState);
				} else {
					StateUpdate (animState);
				}
				prevState = animState;
			}
		}

		public bool CompareCurrentState(int stateNameIn){
			bool result = false;
			if(animState == stateNameIn){
				result = true;
			}
			return result;
		}

		public bool ComparePreviousState(int stateNameIn){
			bool result = false;
			if(prevState == stateNameIn){
				result = true;
			}
			return result;
		}

		void LayerEnter(){
		}

		void LayerUpdate(int animLayerIn){
		}

		void LayerExit(int animLayerIn){
		}

		void StateEnter(int animStateIn){
			//MatchCloseCombatTarget ();
		}

		void MatchCloseCombatTarget(){
			if (!aiBase.animator.IsInTransition (0)) {
				if (CompareCurrentState (unarmedCloseCombatForward)
					|| CompareCurrentState (unarmedCloseCombatBack)
					|| CompareCurrentState (unarmedCloseCombatLeft)
					|| CompareCurrentState (unarmedCloseCombatRight)) {         
					if (Vector3.Distance (aiBase.transform.parent.position, aiBase.navigation.moveTarget.transform.position) < targetMatch) {
						Debug.Log ("matching target");
						aiBase.animator.MatchTarget (aiBase.navigation.moveTarget.transform.position, aiBase.navigation.moveTarget.transform.rotation, AvatarTarget.Root, 
							new MatchTargetWeightMask (Vector3.one, 0f), 0f, 1f);
					}
				}
			}
		}

		void StateUpdate(int animStateIn){
			ExitCloseCombat();
		}

		void ExitCloseCombat(){
			stateBlocked = false;
			if(aiBase.motor.GetFightState() == 0){
				if (CompareCurrentState (unarmedCloseCombatForward)
					|| CompareCurrentState (unarmedCloseCombatBack)
					|| CompareCurrentState (unarmedCloseCombatLeft)
					|| CompareCurrentState (unarmedCloseCombatRight)
				) {
					if (aiBase.isZombie) {
						Debug.Log ("zombie state machine stopping");
					} else {
						Debug.Log ("human state machine stopping");
					}
					stateBlocked = true;
				}
			}
		}

		void StateExit(int animStateIn){
		}

		int GetDominantLayer() {
			int dominant_index = 0;
			float dominant_weight = 0f;
			float weight = 0f;
			for (int index = 0; index < aiBase.animator.layerCount; index++) {
				weight = aiBase.animator.GetLayerWeight(index);
				if (weight > dominant_weight) {
					dominant_weight = weight;
					dominant_index = index;
				}
			}
			return dominant_index;
		}
	}
}



