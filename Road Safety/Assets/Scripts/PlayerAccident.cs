using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerAccident : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallDuration = 0.45f;
    public float fallAngle = 90f;
    public float knockbackDistance = 1.2f;
    public float liftAmount = 0f;
    public float surfaceOffset = 0.03f;
    public float surfaceCheckDistance = 5f;
    public LayerMask surfaceLayers = ~0;

    [Header("Optional")]
    public Behaviour[] extraComponentsToDisable;
    public string accidentAnimationTrigger = "Accident";
    public bool keepCameraAngleAfterAccident = true;
    public Transform cameraTargetToKeepStill;
    public bool freezePhysicsAfterAccident = true;

    [Header("Mission Failed UI")]
    public bool showMissionFailedScreen = true;
    public float missionFailedDelay = 0.75f;
    public string diedMessage = "You Died";
    public string missionFailedMessage = "Mission Failed";
    public string restartButtonText = "Restart";

    private bool hasAccidentHappened;
    private bool isLyingOnSurface;
    private bool showRestartOption;
    private CharacterController characterController;
    private Animator animator;
    private Rigidbody[] rigidbodies;
    private Vector3 finalLyingPosition;
    private Quaternion finalLyingRotation;
    private Quaternion cameraTargetFinalRotation;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        cameraTargetToKeepStill = FindCameraTarget();
    }

    private void LateUpdate()
    {
        if (!isLyingOnSurface)
        {
            return;
        }

        transform.SetPositionAndRotation(finalLyingPosition, finalLyingRotation);
        KeepCameraTargetRotation(cameraTargetFinalRotation);
    }

    private void OnGUI()
    {
        if (!showRestartOption)
        {
            return;
        }

        DrawMissionFailedScreen();
    }

    public void TriggerAccident(Vector3 hitDirection)
    {
        if (hasAccidentHappened)
        {
            return;
        }

        hasAccidentHappened = true;
        StartCoroutine(Fall(hitDirection));
    }

    private IEnumerator Fall(Vector3 hitDirection)
    {
        DisablePlayerControl();

        Vector3 flatHitDirection = Vector3.ProjectOnPlane(hitDirection, Vector3.up).normalized;
        if (flatHitDirection.sqrMagnitude < 0.01f)
        {
            flatHitDirection = -transform.forward;
        }

        if (animator != null && HasAnimatorParameter(accidentAnimationTrigger, AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(accidentAnimationTrigger);
        }

        Quaternion startRotation = transform.rotation;
        Vector3 startPosition = transform.position;
        Quaternion cameraTargetStartRotation = cameraTargetToKeepStill != null
            ? cameraTargetToKeepStill.rotation
            : Quaternion.identity;

        Vector3 fallAxis = Vector3.Cross(Vector3.up, flatHitDirection).normalized;
        Quaternion targetRotation = Quaternion.AngleAxis(fallAngle, fallAxis) * startRotation;
        Vector3 targetPosition = startPosition + flatHitDirection * knockbackDistance + Vector3.up * liftAmount;

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fallDuration);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            KeepOnSurface(false);
            KeepCameraTargetRotation(cameraTargetStartRotation);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        transform.position = targetPosition;
        KeepOnSurface(true);
        finalLyingPosition = transform.position;
        finalLyingRotation = transform.rotation;
        cameraTargetFinalRotation = cameraTargetStartRotation;
        FreezePhysics();
        isLyingOnSurface = true;
        KeepCameraTargetRotation(cameraTargetFinalRotation);

        if (showMissionFailedScreen)
        {
            yield return new WaitForSeconds(missionFailedDelay);
            ShowRestartOption();
        }
    }

    private void DisablePlayerControl()
    {
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        FreezePhysics();

        MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            if (component != this)
            {
                component.enabled = false;
            }
        }

        if (extraComponentsToDisable == null)
        {
            return;
        }

        foreach (Behaviour component in extraComponentsToDisable)
        {
            if (component != null)
            {
                component.enabled = false;
            }
        }
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (animator == null || string.IsNullOrEmpty(parameterName))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == parameterType && parameter.name == parameterName)
            {
                return true;
            }
        }

        return false;
    }

    private Transform FindCameraTarget()
    {
        Transform target = transform.Find("PlayerCameraRoot");
        if (target != null)
        {
            return target;
        }

        target = transform.Find("CinemachineCameraTarget");
        if (target != null)
        {
            return target;
        }

        return null;
    }

    private void KeepCameraTargetRotation(Quaternion rotationToKeep)
    {
        if (!keepCameraAngleAfterAccident || cameraTargetToKeepStill == null)
        {
            return;
        }

        cameraTargetToKeepStill.rotation = rotationToKeep;
    }

    private void KeepOnSurface(bool allowLowering)
    {
        if (!TryGetSurfaceHeight(out float surfaceHeight))
        {
            return;
        }

        float lowestPoint = GetLowestVisiblePoint();
        float surfaceCorrection = surfaceHeight + surfaceOffset - lowestPoint;
        if (surfaceCorrection > 0f || allowLowering)
        {
            transform.position += Vector3.up * surfaceCorrection;
        }
    }

    private bool TryGetSurfaceHeight(out float surfaceHeight)
    {
        Vector3 rayStart = transform.position + Vector3.up * surfaceCheckDistance;
        RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, surfaceCheckDistance * 2f, surfaceLayers, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0)
        {
            surfaceHeight = transform.position.y;
            return false;
        }

        System.Array.Sort(hits, (first, second) => first.distance.CompareTo(second.distance));
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            surfaceHeight = hit.point.y;
            return true;
        }

        surfaceHeight = transform.position.y;
        return false;
    }

    private float GetLowestVisiblePoint()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return transform.position.y;
        }

        float lowestPoint = float.MaxValue;
        foreach (Renderer childRenderer in renderers)
        {
            lowestPoint = Mathf.Min(lowestPoint, childRenderer.bounds.min.y);
        }

        return lowestPoint;
    }

    private void FreezePhysics()
    {
        if (!freezePhysicsAfterAccident || rigidbodies == null)
        {
            return;
        }

        foreach (Rigidbody body in rigidbodies)
        {
            if (body == null)
            {
                continue;
            }

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = false;
            body.isKinematic = true;
        }
    }

    private void ShowRestartOption()
    {
        showRestartOption = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void DrawMissionFailedScreen()
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 42,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.25f, 0.2f) }
        };

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold
        };

        Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(screenRect, Texture2D.whiteTexture);
        GUI.color = previousColor;

        float panelWidth = Mathf.Min(460f, Screen.width - 40f);
        float panelHeight = 260f;
        Rect panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        GUILayout.BeginArea(panelRect);
        GUILayout.FlexibleSpace();
        GUILayout.Label(diedMessage, titleStyle, GUILayout.Height(60f));
        GUILayout.Label(missionFailedMessage, subtitleStyle, GUILayout.Height(45f));
        GUILayout.Space(24f);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(restartButtonText, buttonStyle, GUILayout.Width(180f), GUILayout.Height(56f)))
        {
            RestartLevel();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }
}
