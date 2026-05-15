using UnityEngine;
using System.Collections;

public class TrafficLightController : MonoBehaviour
{
    private static TrafficLightController displaySource;

    public Renderer redLight;
    public Renderer yellowLight;
    public Renderer greenLight;

    public Material redOn, redOff;
    public Material yellowOn, yellowOff;
    public Material greenOn, greenOff;

    public float redTime = 15f;
    public float greenTime = 15f;
    public float yellowTime = 5f;

    [Header("Traffic Instruction UI")]
    public bool showInstructionUI = true;
    public bool showInTopRight = true;
    public string redInstruction = "STOP";
    public string yellowInstruction = "BE READY";
    public string greenInstruction = "MOVE";

    [Header("Traffic Light World Label")]
    public bool showWorldLightLabel = true;
    public Vector3 worldLabelOffset = new Vector3(0f, 4.6f, 0f);
    public float worldLabelSize = 0.28f;

    public enum LightState { Red, Yellow, Green }
    public LightState currentState;

    private TextMesh worldLightLabel;

    void Awake()
    {
        if (displaySource == null)
        {
            displaySource = this;
        }
    }

    void Start()
    {
        CreateWorldLightLabel();
        StartCoroutine(TrafficCycle());
    }

    void OnDestroy()
    {
        if (displaySource == this)
        {
            displaySource = null;
        }
    }

    void OnGUI()
    {
        if (!showInstructionUI || displaySource != this)
        {
            return;
        }

        DrawInstructionUI();
    }

    void LateUpdate()
    {
        if (worldLightLabel == null || Camera.main == null)
        {
            return;
        }

        worldLightLabel.transform.rotation = Quaternion.LookRotation(
            worldLightLabel.transform.position - Camera.main.transform.position);
    }

    IEnumerator TrafficCycle()
    {
        while (true)
        {
            // GREEN
            SetLight(LightState.Green);
            yield return new WaitForSeconds(greenTime);

            // YELLOW
            SetLight(LightState.Yellow);
            yield return new WaitForSeconds(yellowTime);

            // RED
            SetLight(LightState.Red);
            yield return new WaitForSeconds(redTime);
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

        UpdateWorldLightLabel();
    }

    void CreateWorldLightLabel()
    {
        if (!showWorldLightLabel || worldLightLabel != null)
        {
            return;
        }

        GameObject labelObject = new GameObject("Traffic Light Status Label");
        labelObject.transform.SetParent(transform);
        labelObject.transform.localPosition = worldLabelOffset;
        labelObject.transform.localRotation = Quaternion.identity;

        worldLightLabel = labelObject.AddComponent<TextMesh>();
        worldLightLabel.anchor = TextAnchor.MiddleCenter;
        worldLightLabel.alignment = TextAlignment.Center;
        worldLightLabel.characterSize = worldLabelSize;
        worldLightLabel.fontSize = 64;
        worldLightLabel.fontStyle = FontStyle.Bold;

        UpdateWorldLightLabel();
    }

    void UpdateWorldLightLabel()
    {
        if (!showWorldLightLabel || worldLightLabel == null)
        {
            return;
        }

        worldLightLabel.text = GetWorldLabelText();
        worldLightLabel.color = GetInstructionColor();
        worldLightLabel.gameObject.SetActive(true);
    }

    string GetWorldLabelText()
    {
        return GetInstructionText();
    }

    void DrawInstructionUI()
    {
        string instruction = GetInstructionText();
        Color instructionColor = GetInstructionColor();

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        float boxWidth = 130f;
        float boxHeight = 42f;
        float margin = 20f;

        Rect boxRect = showInTopRight
            ? new Rect(Screen.width - boxWidth - margin, margin, boxWidth, boxHeight)
            : new Rect(margin, margin, boxWidth, boxHeight);

        Color previousColor = GUI.color;

        GUI.color = new Color(0f, 0f, 0f, 0.68f);
        GUI.DrawTexture(boxRect, Texture2D.whiteTexture);

        Rect colorBarRect = new Rect(boxRect.x, boxRect.y, 6f, boxRect.height);
        GUI.color = instructionColor;
        GUI.DrawTexture(colorBarRect, Texture2D.whiteTexture);

        GUI.color = previousColor;
        GUI.Label(boxRect, instruction, labelStyle);
    }

    string GetInstructionText()
    {
        if (currentState == LightState.Red)
        {
            return redInstruction;
        }

        if (currentState == LightState.Yellow)
        {
            return yellowInstruction;
        }

        return greenInstruction;
    }

    Color GetInstructionColor()
    {
        if (currentState == LightState.Red)
        {
            return new Color(1f, 0.1f, 0.08f);
        }

        if (currentState == LightState.Yellow)
        {
            return new Color(1f, 0.82f, 0.08f);
        }

        return new Color(0.1f, 0.85f, 0.25f);
    }
}
