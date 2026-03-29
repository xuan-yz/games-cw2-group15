using UnityEngine;

public class BossManager : MonoBehaviour 
{
        public GameObject player;

         public UnityEngine.AI.NavMeshAgent agent;

    public BossBase currentState;

    public BossPhase1 phase1 = new BossPhase1();
    public BossPhase2 phase2 = new BossPhase2();





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        currentState = phase1;

        currentState.enterState(this);


    }
    void Update()
    {
        currentState.updateState(this);

    }

    public void switchState(BossBase state)
    {
        currentState = state;
        state.enterState(this);
    }

    void OnCollisionEnter(Collision collision)
{

}
}
