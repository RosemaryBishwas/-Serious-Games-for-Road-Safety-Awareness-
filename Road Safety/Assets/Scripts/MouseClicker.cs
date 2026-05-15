using UnityEngine;
using UnityEngine.InputSystem;
public class MouseClicker : MonoBehaviour
{
    [SerializeField]
    private Camera m_Camera;

    [Header("Start Rules")]
    public bool showRulesAtStart = true;
    public string title = "Road Safety Rules";
    [TextArea(4, 8)]
    public string rulesText =
        "1. Safely reach the gift shop.\n" +
        "2. Follow the traffic lights.\n" +
        "3. Do not walk in front of moving vehicles.\n" +
        "4. Reach the gift shop to complete the mission.";
    public string startButtonText = "Start";

    [Header("Mission Destination")]
    public bool enableGiftShopMission = true;
    public Transform giftShopDestination;
    public string giftShopNameContains = "Gift";
    public string fallbackShopNameContains = "Gift Shop";
    public string secondFallbackShopNameContains = "Fruits";
    public float destinationReachDistance = 4f;
    public string missionCompleteTitle = "Mission Successful";
    public string missionCompleteMessage = "Reach Destination";
    public bool showSuccessRay = false;
    public Color successRayColor = new Color(0.15f, 1f, 0.25f);
    public float successRayWidth = 0.12f;

    private bool mousePress = false;
    private bool gameStarted = false;
    private bool missionCompleted = false;
    private Transform playerTransform;
    private Collider giftShopDestinationCollider;
    private LineRenderer successRayRenderer;

    void Start()
    {
        if (m_Camera == null)
        {
            m_Camera = Camera.main;
        }

        playerTransform = FindPlayerTransform();
        giftShopDestination = giftShopDestination != null ? giftShopDestination : FindGiftShopDestination();
        giftShopDestinationCollider = FindDestinationCollider(giftShopDestination);

        if (showRulesAtStart)
        {
            PauseForRules();
        }
        else
        {
            gameStarted = true;
        }
    }

    void Update()
    {
        if (!gameStarted)
        {
            return;
        }

        CheckMissionDestination();

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            mousePress = true;
        }
    }

    void FixedUpdate()
    {
        if (!gameStarted || !mousePress || m_Camera == null)
        {
            return;
        }

        mousePress = false;
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        Vector3 mousePosition = mouse.position.ReadValue();
        Ray ray = m_Camera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Clicked on: " + hit.collider.gameObject.name);
            GOInteraction aGOI = hit.collider.gameObject.GetComponent<GOInteraction>();
            if (aGOI)
            {
                aGOI.Interaction = true;
            }
        }
    }

    void OnGUI()
    {
        if (missionCompleted)
        {
            DrawMissionCompleteScreen();
            return;
        }

        if (gameStarted || !showRulesAtStart)
        {
            return;
        }

        DrawRulesScreen();
    }

    private void PauseForRules()
    {
        gameStarted = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void StartGame()
    {
        gameStarted = true;
        Time.timeScale = 1f;
    }

    private void CheckMissionDestination()
    {
        if (!enableGiftShopMission || missionCompleted)
        {
            return;
        }

        if (playerTransform == null)
        {
            playerTransform = FindPlayerTransform();
        }

        if (giftShopDestination == null)
        {
            giftShopDestination = FindGiftShopDestination();
            giftShopDestinationCollider = FindDestinationCollider(giftShopDestination);
        }

        if (playerTransform == null || giftShopDestination == null)
        {
            return;
        }

        if (HasPlayerHadAccident())
        {
            return;
        }

        if (HasReachedGiftShop())
        {
            CompleteMission();
        }
    }

    public void CompleteMission()
    {
        missionCompleted = true;
        gameStarted = false;
        DrawSuccessRay();
        Vector3 successPosition = giftShopDestination != null
            ? giftShopDestination.position
            : transform.position;
        RoadSafetyAudio.PlaySuccess(successPosition);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Mission successful: reached the gift shop destination.");
    }

    private Transform FindPlayerTransform()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    private Transform FindGiftShopDestination()
    {
        Transform destination = FindObjectByNameContains(giftShopNameContains);
        if (destination != null)
        {
            return destination;
        }

        destination = FindObjectByNameContains(fallbackShopNameContains);
        if (destination != null)
        {
            return destination;
        }

        return FindObjectByNameContains(secondFallbackShopNameContains);
    }

    private Collider FindDestinationCollider(Transform destination)
    {
        if (destination == null)
        {
            return null;
        }

        Collider destinationCollider = destination.GetComponent<Collider>();
        if (destinationCollider != null)
        {
            return destinationCollider;
        }

        return destination.GetComponentInChildren<Collider>();
    }

    private bool HasReachedGiftShop()
    {
        if (giftShopDestinationCollider != null)
        {
            Vector3 closestPoint =
                giftShopDestinationCollider.ClosestPoint(playerTransform.position);

            return Vector3.Distance(playerTransform.position, closestPoint) <=
                   destinationReachDistance;
        }

        return Vector3.Distance(playerTransform.position, giftShopDestination.position) <=
               destinationReachDistance;
    }

    private bool HasPlayerHadAccident()
    {
        PlayerAccident playerAccident =
            playerTransform != null
            ? playerTransform.GetComponent<PlayerAccident>()
            : null;

        return playerAccident != null && playerAccident.HasAccidentHappened;
    }

    private void DrawSuccessRay()
    {
        if (!showSuccessRay || playerTransform == null || giftShopDestination == null)
        {
            return;
        }

        if (successRayRenderer == null)
        {
            GameObject rayObject = new GameObject("Mission Success Ray");
            successRayRenderer = rayObject.AddComponent<LineRenderer>();
            Shader lineShader = Shader.Find("Sprites/Default");
            if (lineShader == null)
            {
                lineShader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            if (lineShader != null)
            {
                successRayRenderer.material = new Material(lineShader);
            }

            successRayRenderer.positionCount = 2;
            successRayRenderer.useWorldSpace = true;
        }

        successRayRenderer.startColor = successRayColor;
        successRayRenderer.endColor = successRayColor;
        successRayRenderer.startWidth = successRayWidth;
        successRayRenderer.endWidth = successRayWidth;
        successRayRenderer.SetPosition(0, playerTransform.position + Vector3.up * 1.1f);
        successRayRenderer.SetPosition(1, giftShopDestination.position + Vector3.up * 1.1f);
        successRayRenderer.enabled = true;
    }

    private Transform FindObjectByNameContains(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            return null;
        }

        GameObject[] sceneObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject sceneObject in sceneObjects)
        {
            if (sceneObject.name.ToLower().Contains(searchText.ToLower()))
            {
                return sceneObject.transform;
            }
        }

        return null;
    }

    private void DrawRulesScreen()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        GUIStyle rulesStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 22,
            wordWrap = true,
            normal = { textColor = Color.white }
        };

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold
        };

        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.72f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;

        float panelWidth = Mathf.Min(620f, Screen.width - 40f);
        float panelHeight = 420f;
        Rect panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        GUILayout.BeginArea(panelRect);
        GUILayout.Label(title, titleStyle, GUILayout.Height(60f));
        GUILayout.Space(20f);
        GUILayout.Label(rulesText, rulesStyle, GUILayout.Height(220f));
        GUILayout.Space(24f);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(startButtonText, buttonStyle, GUILayout.Width(180f), GUILayout.Height(56f)))
        {
            StartGame();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void DrawMissionCompleteScreen()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 42,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        GUIStyle messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 26,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.35f, 1f, 0.45f) }
        };

        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.72f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;

        float panelWidth = Mathf.Min(620f, Screen.width - 40f);
        float panelHeight = 260f;
        Rect panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        GUILayout.BeginArea(panelRect);
        GUILayout.FlexibleSpace();
        GUILayout.Label(missionCompleteTitle, titleStyle, GUILayout.Height(70f));
        GUILayout.Space(16f);
        GUILayout.Label(missionCompleteMessage, messageStyle, GUILayout.Height(80f));
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }
}
