using UnityEngine;

public class GiftShopDestination : MonoBehaviour
{
    public string playerTag = "Player";
    public MouseClicker missionController;
    public float reachDistance = 8f;
    public bool createGiftShopSign = false;
    public string signText = "GIFT SHOP";
    public Vector3 signOffset = new Vector3(0f, 8f, 0f);
    public float signSize = 2.5f;
    public string missionSuccessTitle = "Mission Successful";
    public string missionSuccessMessage = "Reach Destination";
    public bool showSuccessRay = false;
    public Color successRayColor = new Color(0.15f, 1f, 0.25f);
    public float successRayWidth = 0.14f;

    private bool missionCompleted;
    private Transform signTransform;
    private Transform playerTransform;
    private LineRenderer successRayRenderer;

    private void Awake()
    {
        if (missionController == null)
        {
            missionController = FindFirstObjectByType<MouseClicker>();
        }

        playerTransform = FindPlayerTransform();

        if (createGiftShopSign)
        {
            CreateGiftShopSign();
        }
    }

    private void Update()
    {
        if (missionCompleted)
        {
            return;
        }

        if (playerTransform == null)
        {
            playerTransform = FindPlayerTransform();
        }

        if (playerTransform == null || HasPlayerHadAccident(playerTransform.gameObject))
        {
            return;
        }

        if (Vector3.Distance(playerTransform.position, transform.position) <= reachDistance)
        {
            CompleteMission(playerTransform.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCompleteMission(other.gameObject);
    }

    private void OnGUI()
    {
        if (!missionCompleted)
        {
            return;
        }

        DrawMissionSuccessScreen();
    }

    private void LateUpdate()
    {
        if (signTransform == null || Camera.main == null)
        {
            return;
        }

        signTransform.rotation = Quaternion.LookRotation(
            signTransform.position - Camera.main.transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryCompleteMission(collision.gameObject);
    }

    private void TryCompleteMission(GameObject hitObject)
    {
        if (missionCompleted)
        {
            return;
        }

        GameObject playerObject = GetPlayerObject(hitObject);
        if (playerObject == null || HasPlayerHadAccident(playerObject))
        {
            return;
        }

        CompleteMission(playerObject);
    }

    private void CompleteMission(GameObject playerObject)
    {
        missionCompleted = true;
        playerTransform = playerObject.transform;

        if (missionController == null)
        {
            missionController = FindFirstObjectByType<MouseClicker>();
        }

        if (missionController != null)
        {
            missionController.giftShopDestination = transform;
            missionController.CompleteMission();
        }
        else
        {
            RoadSafetyAudio.PlaySuccess(transform.position);
        }

        DrawSuccessRay();
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Mission successful: player safely reached the gift shop.");
    }

    private Transform FindPlayerTransform()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        return playerObject != null ? playerObject.transform : null;
    }

    private GameObject GetPlayerObject(GameObject hitObject)
    {
        if (hitObject.CompareTag(playerTag))
        {
            return hitObject;
        }

        Transform root = hitObject.transform.root;
        return root.CompareTag(playerTag) ? root.gameObject : null;
    }

    private bool HasPlayerHadAccident(GameObject playerObject)
    {
        PlayerAccident playerAccident = playerObject.GetComponent<PlayerAccident>();
        return playerAccident != null && playerAccident.HasAccidentHappened;
    }

    private void DrawSuccessRay()
    {
        if (!showSuccessRay || playerTransform == null)
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
        successRayRenderer.SetPosition(1, transform.position + Vector3.up * 1.1f);
        successRayRenderer.enabled = true;
    }

    private void CreateGiftShopSign()
    {
        signTransform = transform.Find("Gift Shop Mission Sign");
        if (signTransform != null)
        {
            return;
        }

        GameObject signObject = new GameObject("Gift Shop Mission Sign");
        signObject.transform.SetParent(transform);
        signObject.transform.localPosition = signOffset;
        signObject.transform.localRotation = Quaternion.identity;
        signTransform = signObject.transform;

        TextMesh sign = signObject.AddComponent<TextMesh>();
        sign.text = signText;
        sign.anchor = TextAnchor.MiddleCenter;
        sign.alignment = TextAlignment.Center;
        sign.characterSize = signSize;
        sign.fontSize = 64;
        sign.color = new Color(1f, 0.25f, 0.65f);
    }

    private void DrawMissionSuccessScreen()
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
            fontSize = 28,
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
        GUILayout.Label(missionSuccessTitle, titleStyle, GUILayout.Height(70f));
        GUILayout.Space(16f);
        GUILayout.Label(missionSuccessMessage, messageStyle, GUILayout.Height(80f));
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }
}
