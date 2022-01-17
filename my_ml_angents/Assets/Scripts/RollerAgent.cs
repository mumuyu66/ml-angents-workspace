using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RollerAgent: Agent
{
    public Rigidbody rbody;
    public GameObject target;

    public void Start()
    {
        rbody = this.gameObject.GetComponent<Rigidbody>();    
    }
    public override void OnEpisodeBegin()
    {
        if (transform.position.y < 0)
        {
            transform.position = new Vector3(0, 0.5f, 0);
        }
        transform.position = new Vector3(Random.value * 8 - 4,0.5f,Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(target.transform.position);
        sensor.AddObservation(rbody.velocity.x);
        sensor.AddObservation(rbody.velocity.z);
    }

    public float forceMultiplier = 10;
    public override void OnActionReceived(float[] vectorAction)
    {
        rbody.AddForce(vectorAction[0] * forceMultiplier, 0, vectorAction[1] * forceMultiplier);
        if (Vector3.Distance(target.transform.position, transform.position) < 1.42f)
        {
            AddReward(1.0f);
            EndEpisode();
        }

        if (transform.position.y < 0)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }
}
