using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RollerAgent: Agent
{
    public Rigidbody rbody;
    public GameObject target;
    public float forceMultiplier = 50;

    public void Start()
    {
        rbody = this.gameObject.GetComponent<Rigidbody>();    
    }
    public override void OnEpisodeBegin()
    {
       
        transform.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);

        target.transform.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(target.transform.position);
        sensor.AddObservation(rbody.velocity.x);
        sensor.AddObservation(rbody.velocity.z);
    }

    
    public override void OnActionReceived(ActionBuffers action)
    {
        rbody.AddForce(action.ContinuousActions[0] * forceMultiplier, 0, action.ContinuousActions[1] * forceMultiplier);
        Vector3 p1 = target.transform.position;
        Vector3 p2 = transform.position;
        AddReward(-0.001f);
        if (Vector2.Distance(new Vector2(p1.x,p1.z), new Vector2(p2.x,p2.z)) < 1.42f)
        {
            AddReward(1.0f);
            EndEpisode();
        }

        if (transform.localPosition.x > 5.5f || transform.localPosition.x < -5.5f || transform.localPosition.z > 5.5f || transform.localPosition.z < -5.5f)
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.ContinuousActions;
        discreteActionsOut[0] = Input.GetAxis("Horizontal");
        discreteActionsOut[1] = Input.GetAxis("Vertical");
    }
}
