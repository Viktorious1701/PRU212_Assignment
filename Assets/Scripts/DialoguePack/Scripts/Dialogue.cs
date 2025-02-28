using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    [Header("Setup")]
    public TextMeshProUGUI textBox;
    public TextMeshProUGUI nameBox;
    public Animator animController;
    public Button skipButton;           // Reference to the UI skip button
    public TextMeshProUGUI skipPrompt;  // Reference to the "Press F to skip" text

    [Header("Input")]
    [HideInInspector]
    public string[] sentences;
    private int index;
    public float typingSpeed = 0.2f;    // Set to 0.2 as per your requirement
    public float duration = 4f;         // Set to 4 as per your requirement
    private bool active = true;
    private bool isDialogueActive = false;  // Tracks if dialogue is currently active

    private void Awake()
    {
        // Set up the skip button to call Skip() when clicked
        if (skipButton != null)
            skipButton.onClick.AddListener(Skip);
    }

    private void Update()
    {
        // Allow skipping with the "F" key when dialogue is active
        if (isDialogueActive && Input.GetKeyDown(KeyCode.F))
        {
            Skip();
        }
    }

    IEnumerator Type()
    {
        animController.ResetTrigger("Disappear");
        textBox.text = "";
        foreach (var letter in sentences[index].ToCharArray())
        {
            if (active)
            {
                textBox.text += letter;
                yield return new WaitForSeconds(typingSpeed);
                animController.ResetTrigger("Appear");
            }
            else
            {
                textBox.text = sentences[index];
                break; // Exit the loop if skipping to show full text
            }
        }
    }

    IEnumerator TypeMany()
    {
        Debug.Log("TypeMany started. sentences: " + (sentences != null ? sentences.Length.ToString() : "null") +
                  ", animController: " + (animController != null ? "set" : "null") +
                  ", skipButton: " + (skipButton != null ? "set" : "null"));
        for (int i = 0; i < sentences.Length + 1; i++)
        {
            Debug.Log("Loop iteration: " + i + ", index: " + index + ", sentences null? " + (sentences == null));
            if (index < sentences.Length)
            {
                StartCoroutine(Type());
            }
            yield return new WaitForSeconds(duration);
            if (index < sentences.Length)
            {
                index++;
            }
            else
            {
                animController.SetTrigger("Disappear");
                isDialogueActive = false;
                skipButton.gameObject.SetActive(false);
                skipPrompt.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateName(string name = null)
    {
        if (name == null)
        {
            nameBox.gameObject.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            if (!nameBox.gameObject.transform.parent.gameObject.active)
            {
                nameBox.gameObject.transform.parent.gameObject.SetActive(true);
            }
            nameBox.text = name;
        }
    }

    public void Say(string _text, string _characterName = null, float _duration = 0)
    {
        isDialogueActive = true;
        animController.SetTrigger("Appear");
        string[] phrase = { _text };
        sentences = phrase;
        index = 0;
        if (_duration > 0) duration = _duration;
        UpdateName(_characterName);
        skipButton.gameObject.SetActive(true);
        skipPrompt.gameObject.SetActive(true);
        StartCoroutine(Type());
    }

    public void Say(string[] _text, string _characterName = null, float _duration = 0)
    {
        isDialogueActive = true;
        animController.SetTrigger("Appear");
        sentences = _text;
        index = 0;
        if (_duration > 0) duration = _duration;
        UpdateName(_characterName);
        skipButton.gameObject.SetActive(true);
        skipPrompt.gameObject.SetActive(true);
        StartCoroutine(TypeMany());
    }

    public void Skip()
    {
        if (index < sentences.Length - 1)
        {
            if (active)
            {
                active = false; // Show full current line
            }
            else
            {
                index++;        // Move to next line
                StartCoroutine(Type());
                active = true;
            }
        }
        else
        {
            animController.SetTrigger("Disappear");
            isDialogueActive = false;
            skipButton.gameObject.SetActive(false);
            skipPrompt.gameObject.SetActive(false);
        }
    }

    public void Clear()
    {
        isDialogueActive = false;
        animController.SetTrigger("Disappear");
        sentences = null;
        UpdateName();
        textBox.text = "";
        skipButton.gameObject.SetActive(false);
        skipPrompt.gameObject.SetActive(false);
    }
}