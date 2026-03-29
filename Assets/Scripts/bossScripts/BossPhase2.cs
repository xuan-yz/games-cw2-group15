using UnityEngine;
using UnityEngine.AI;

public class BossPhase2 : BossBase
{
    private Transform player;
    private NavMeshAgent agent;

    private GunLogic gunLogic;

    private float runDistance = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  public override void enterState(BossManager e)
  {
    player = GameObject.FindGameObjectWithTag("Player").transform;
    agent = e.GetComponent<NavMeshAgent>();
    gunLogic = e.GetComponent<GunLogic>();


  }
  public override void updateState(BossManager e)
  {

        if(Vector3.Distance(e.transform.position, e.player.transform.position) > 30)
        {
            return;
            
        }


        else if ((Vector3.Distance(e.transform.position, e.player.transform.position) < runDistance) && (Vector3.Distance(e.transform.position, e.player.transform.position) > runDistance/2))
        {
           agent.ResetPath();
           Vector3 direction = (player.position - e.transform.position).normalized;
           direction.y = 0;
           e.transform.rotation = Quaternion.LookRotation(direction);
           gunLogic.Shoot();           
        }

        else if(Vector3.Distance(e.transform.position, e.player.transform.position) < runDistance/2){
            Vector3 directionAway = (e.transform.position - e.player.transform.position ).normalized;
            float fleeSpeed = 5f;
            e.transform.position += directionAway * fleeSpeed * Time.deltaTime;
            
            
        }

        else
        {
             agent.SetDestination(GameObject.FindGameObjectWithTag("Player").transform.position);
        }



  }
  public override void OnCollsionEnter(BossManager e)
  {

  }
}
