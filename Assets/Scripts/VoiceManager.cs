using UnityEngine;
using Meta.WitAi;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;
using UnityEngine.InputSystem;
using Meta.WitAi.Data;
using Meta.WitAi.Data.Configuration;

public class VoiceManager : MonoBehaviour
{
    [SerializeField] private WitConfiguration witConfiguration;
    private Wit wit;
    private SpellSystem spellSystem;
    private bool isRecording = false;

    // Reference to the input action for the "A" button
    [SerializeField] private InputActionReference aButtonAction;

    private void Start()
    {
        // Get references
        spellSystem = FindObjectOfType<SpellSystem>();
        wit = GetComponent<Wit>();

        if (spellSystem == null)
        {
            Debug.LogError("SpellSystem not found in the scene.");
        }

        if (wit == null)
        {
            Debug.LogError("Wit component not found on the GameObject.");
        }

        // Set up the input action for the "A" button
        if (aButtonAction != null)
        {
            aButtonAction.action.Enable();
            Debug.Log("A Button Action Reference assigned.");
            aButtonAction.action.performed += OnAButtonPressed;
            aButtonAction.action.canceled += OnAButtonReleased;
        }
        else
        {
            Debug.LogError("A Button Action Reference not assigned!");
        }

        // Setup Wit callbacks
        wit.VoiceEvents.OnResponse.AddListener(OnWitResponse);
    }

    private void OnDestroy()
    {
        // Clean up input action listeners
        if (aButtonAction != null)
        {
            aButtonAction.action.performed -= OnAButtonPressed;
            aButtonAction.action.canceled -= OnAButtonReleased;
            aButtonAction.action.Disable();
        }
    }

    private void OnAButtonPressed(InputAction.CallbackContext context)
    {
        if (!isRecording)
        {
            isRecording = true;
            wit.Activate();
            Debug.Log("Started recording...");
        }
    }

    private void OnAButtonReleased(InputAction.CallbackContext context)
    {
        if (isRecording)
        {
            isRecording = false;
            wit.Deactivate();
            Debug.Log("Stopped recording.");
        }
    }

    private void OnWitResponse(WitResponseNode response)
    {
        // Get the intent
        WitResponseNode intentsNode = response["intents"];
        if (intentsNode == null || intentsNode.Count == 0)
        {
            Debug.Log("No intent found in response");
            return;
        }

        string intentName = intentsNode[0]["name"].Value;
        Debug.Log($"Intent: {intentName}");
        if (intentName == "cast_spell")
        {
            // Get the spell name from entities
            WitResponseNode entitiesNode = response["entities"];
            // Debug the full entities structure
            Debug.Log($"Full Entities Structure: {entitiesNode.ToString()}");

            // The correct path is "spell_name:spell_name"
            if (entitiesNode != null && entitiesNode["spell_name:spell_name"] != null)
            {
                string spellName = entitiesNode["spell_name:spell_name"][0]["value"].Value;
                Debug.Log($"Spell name extracted: {spellName}");

                // Find and cast the spell
                SpellData spellData = spellSystem.GetSpellByName(spellName);
                if (spellData != null)
                {
                    Debug.Log($"Casting spell: {spellName}");
                    spellSystem.SelectSpell(spellData);
                }
                else
                {
                    Debug.LogWarning($"Spell '{spellName}' not found in the SpellSystem.");
                }
            }
            else
            {
                Debug.LogWarning("No spell name found in the entities");
            }
        }
    }
}