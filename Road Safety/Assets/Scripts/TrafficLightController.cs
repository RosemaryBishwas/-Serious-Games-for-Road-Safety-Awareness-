using UnityEngine;
using System.Collections;

public class TrafficLightController : MonoBehaviour
{
    public Renderer redLight;
    public Renderer yellowLight;
    public Renderer greenLight;

    public Material redOn, redOff;
    public Material yellowOn, yellowOff;
    public Material greenOn, greenOff;

    public float redTime = 15f;
    public float greenTime = 15f;
    public float yellowTime = 5f;

    public enum LightState { Red, Yellow, Green }
    public LightState currentState;

    void Start()
    {
        StartCoroutine(TrafficCycle());
    }

    IEnumerator TrafficCycle()
    {
        while (true)
        {
            // RED
            SetLight(LightState.Red);
            yield return new WaitForSeconds(redTime);

            // GREEN
            SetLight(LightState.Green);
            yield return new WaitForSeconds(greenTime);

            // YELLOW
            SetLight(LightState.Yellow);
            yield return new WaitForSeconds(yellowTime);
        }
    }

    void SetLight(LightState state)
    {
        currentState = state;

        redLight.material = redOff;
        yellowLight.material = yellowOff;
        greenLight.material = greenOff;

        if (state == LightState.Red)
            redLight.material = redOn;

        else if (state == LightState.Yellow)
            yellowLight.material = yellowOn;

        else if (state == LightState.Green)
            greenLight.material = greenOn;
    }
}