using UnityEngine;


public class ExampleMonoBehaviour : MonoBehaviour
{
    private Animator animator;                          // Reference to the Animator component on this gameobject.
    private AnimationSelect myStateMachine;    // Reference to a single StateMachineBehaviour.


    void Awake()
    {
        // Find a reference to the Animator component in Awake since it exists in the scene.
        animator = GetComponent<Animator>();
    }


    void Start()
    {
        // Find a reference to the ExampleStateMachineBehaviour in Start since it might not exist yet in Awake.
      
        myStateMachine = animator.GetBehaviour<AnimationSelect>();

        // Set the StateMachineBehaviour's reference to an ExampleMonoBehaviour to this.
        myStateMachine.exampleMb = this;
    }
}

public class AnimationSelect : StateMachineBehaviour
{

    public ExampleMonoBehaviour exampleMb;
    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("OnStateEnter!");
        animator.SetInteger("AttackIndex", UnityEngine.Random.Range(0, 6));

    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMachineEnter is called when entering a statemachine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
    //
    //}

    // OnStateMachineExit is called when exiting a statemachine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
    //
    //}
}