using UnityEngine;

public class CarController : MonoBehaviour
{
    public float speed = 5f;

    public Transform stopPoint;
    public TrafficLightController trafficLight;

    public float stopDistance = 5f;

    private bool shouldStop = false;

    void Update()
    {
        CheckTrafficLight();

        if (!shouldStop)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    void CheckTrafficLight()
    {
        float distance = Vector3.Distance(transform.position, stopPoint.position);

        if (distance < stopDistance)
        {
            if (trafficLight.currentState == TrafficLightController.LightState.Red ||
                trafficLight.currentState == TrafficLightController.LightState.Yellow)
            {
                shouldStop = true;
            }
            else
            {
                shouldStop = false;
            }
        }
        else
        {
            shouldStop = false;
        }
    }
}