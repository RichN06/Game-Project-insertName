using UnityEngine;
using Unity.Cinemachine;

public class CharacterSwapManager : MonoBehaviour
{
    [Header("Team Setup Grid")]
    [SerializeField] private GameObject[] teamCharacters;
    [SerializeField] private int startingCharacterIndex = 0;

    [Header("Camera Target Synchronization")]
    [SerializeField] private CinemachineCamera freeLookCamera;

    private int activeIndex;
    private PlayerInputBridge inputBridge;

    void Start()
    {
        activeIndex = startingCharacterIndex;

        // Initalize team visibility grids (turn everyone off except the designated starter)
        for (int i = 0; i < teamCharacters.Length; i++)
        {
            if (teamCharacters[i] != null)
            {
                teamCharacters[i].SetActive(i == activeIndex);
            }
        }

        // Connect input broker system to capture number key requests
        inputBridge = teamCharacters[activeIndex].GetComponent<PlayerInputBridge>();
        if (inputBridge != null)
        {
            inputBridge.OnSwapCharacterTriggered += SwapToCharacter;
        }

        UpdateCameraTargets();
    }

    private void OnDisable()
    {
        // Unsub safely to secure clear garbage collection lanes
        if (inputBridge != null)
        {
            inputBridge.OnSwapCharacterTriggered -= SwapToCharacter;
        }
    }

    private void SwapToCharacter(int targetIndex)
    {
        // Validate array sizes and ensure players aren't trying to swap to the person they already are
        if (targetIndex < 0 || targetIndex >= teamCharacters.Length) return;
        if (targetIndex == activeIndex || teamCharacters[targetIndex] == null) return;

        Debug.Log($"Swapping from Character Index {activeIndex} to {targetIndex}!");

        GameObject currentCharacter = teamCharacters[activeIndex];
        GameObject targetCharacter = teamCharacters[targetIndex];

        // 1. Capture spatial position parameters from the old character model
        Vector3 position = currentCharacter.transform.position;
        Quaternion rotation = currentCharacter.transform.rotation;

        // 2. Fetch tracking velocity calculations from old Rigidbody to hand off inertia parameters
        Rigidbody currentRb = currentCharacter.GetComponent<Rigidbody>();
        Vector3 inheritedVelocity = currentRb != null ? currentRb.linearVelocity : Vector3.zero;

        // 3. Clean up subscription tracking links on old models
        if (inputBridge != null) inputBridge.OnSwapCharacterTriggered -= SwapToCharacter;

        // 4. Perform the mechanical GameObject flip swap states
        currentCharacter.SetActive(false);
        
        targetCharacter.transform.position = position;
        targetCharacter.transform.rotation = rotation;
        targetCharacter.SetActive(true);

        // 5. Re-inject momentum physics parameters onto the incoming player character
        Rigidbody targetRb = targetCharacter.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            targetRb.linearVelocity = inheritedVelocity;
        }

        // 6. Bind listener tracks onto the fresh active player character bridge instances
        inputBridge = targetCharacter.GetComponent<PlayerInputBridge>();
        if (inputBridge != null)
        {
            inputBridge.OnSwapCharacterTriggered += SwapToCharacter;
        }

        // 7. Track the active layout position indexing and point camera system profiles over
        activeIndex = targetIndex;
        UpdateCameraTargets();
    }

    private void UpdateCameraTargets()
    {
        if (freeLookCamera != null && teamCharacters[activeIndex] != null)
        {
            // Point the Cinemachine 3 target profiles toward our new entity transform references
            freeLookCamera.Target.TrackingTarget = teamCharacters[activeIndex].transform;
        }
    }
}