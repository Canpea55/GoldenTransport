using Cinemachine;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class LockTargetToCenter : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;
    private CinemachineOrbitalFollow orbitalFollow;
    private CinemachineRotationComposer rotationComposer;

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();

        // Get procedural components if available
        orbitalFollow = vcam.GetCinemachineComponent<CinemachineOrbitalFollow>();
        rotationComposer = vcam.GetCinemachineComponent<CinemachineRotationComposer>();
    }

    void LateUpdate()
    {
        // Force camera screen position to center if components exist
        if (rotationComposer != null)
        {
            rotationComposer.ScreenPosition = Vector2.zero; // (0,0) = center
        }

        // Remove any target offsets
        if (orbitalFollow != null)
        {
            orbitalFollow.TargetOffset = Vector3.zero;
        }
    }
}
