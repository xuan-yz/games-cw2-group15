using UnityEngine;
using UnityEngine.AI;

public class BossPhase1 : BossBase
{

  private Transform player;
  private NavMeshAgent agent;
  private GunLogic gunLogic;

  private float aggroRange = 30f;

  private float fightRange = 3f;


  public GameObject BulletPrefab;

  private int health = 5; //TODO: ADD HEALTH MECHANIC LATER
  public override void enterState(BossManager e)
  {
    player = GameObject.FindGameObjectWithTag("Player").transform;
    agent = e.GetComponent<NavMeshAgent>();
    gunLogic = e.GetComponent<GunLogic>();


  }
  public override void updateState(BossManager e)
  {
    if (health == 5){
    e.switchState(e.phase2);
    }
    //if player too far do nothing
    if (Vector3.Distance(e.transform.position, e.player.transform.position) > aggroRange)
    {
      return;

    }
    //if player within fight range, fight
    else if (Vector3.Distance(e.transform.position, e.player.transform.position) < fightRange && (Vector3.Distance(e.transform.position, e.player.transform.position) > fightRange / 3))
    {

      agent.ResetPath();
      Vector3 direction = (player.position - e.transform.position).normalized;
      direction.y = 0;
      e.transform.rotation = Quaternion.LookRotation(direction);
      gunLogic.Punch();



    }
    //if neither go to player
    else
    {
      agent.SetDestination(GameObject.FindGameObjectWithTag("Player").transform.position);
    }

  }
  public override void OnCollsionEnter(BossManager e)
  {

  }


}



