using UnityEngine;
using UnityEditor;
using Unity.Cinemachine;

public static class CinemachineSetupTool
{
    [MenuItem("Tools/Setup Cinemachine Camera")]
    public static void SetupCinemachineCamera()
    {
        // 1. Add CinemachineBrain to Main Camera
        var mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("No Main Camera found in scene.");
            return;
        }

        if (mainCam.GetComponent<CinemachineBrain>() == null)
        {
            Undo.AddComponent<CinemachineBrain>(mainCam.gameObject);
            Debug.Log("Added CinemachineBrain to Main Camera.");
        }

        // 2. Create CM Camera
        var cmGo = new GameObject("CM Camera");
        Undo.RegisterCreatedObjectUndo(cmGo, "Create CM Camera");

        var cmCamera = cmGo.AddComponent<CinemachineCamera>();

        // Add CinemachineFollow
        var follow = cmGo.AddComponent<CinemachineFollow>();
        follow.FollowOffset = new Vector3(0f, 0f, -10f);
        follow.TrackerSettings.PositionDamping = new Vector3(1f, 1f, 0f);

        // Add CinemachineImpulseListener
        cmGo.AddComponent<CinemachineImpulseListener>();

        // 3. Find Player and set as Follow target
        var player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            cmCamera.Follow = player.transform;
            Debug.Log($"CM Camera follow target set to '{player.name}'.");
        }
        else
        {
            Debug.LogWarning("PlayerController not found in scene. Set the Follow target manually.");
        }

        // 4. Create CameraShake object
        var shakeGo = new GameObject("CameraShake");
        Undo.RegisterCreatedObjectUndo(shakeGo, "Create CameraShake");
        shakeGo.AddComponent<CameraShake>();

        Debug.Log("Cinemachine setup complete: CM Camera + CameraShake created.");
    }
}
