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
    public bool isDialogueActive = false;

    private void Awake()
    {
        if (textBox == null)
            Debug.LogError("textBox is not assigned in Dialogue script on " + gameObject.name);
        if (nameBox == null)
            Debug.LogError("nameBox is not assigned in Dialogue script on " + gameObject.name);
        if (animController == null)
            Debug.LogError("animController is not assigned in Dialogue script on " + gameObject.name);
        if (skipButton == null)
            Debug.LogError("skipButton is not assigned in Dialogue script on " + gameObject.name);
        if (skipPrompt == null)
            Debug.LogError("skipPrompt is not assigned in Dialogue script on " + gameObject.name);

        if (skipButton != null)
            skipButton.onClick.AddListener(Skip);
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.F))
            Skip();
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
                break;
            }
        }
    }
    IEnumerator TypeMany()
    {
        if (sentences == null)
        {
            Debug.LogError("Sentences array is null in TypeMany! Aborting dialogue.");
            yield break;
        }

        Debug.Log("TypeMany started. sentences: " + sentences.Length +
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
                if (animController != null)
                    animController.SetTrigger("Disappear");
                else
                    Debug.LogWarning("animController is null; cannot trigger Disappear animation.");
                isDialogueActive = false;

                if (skipButton != null)
                    skipButton.gameObject.SetActive(false);
                else
                    Debug.LogWarning("skipButton is null; cannot hide skip button.");

                if (skipPrompt != null)
                    skipPrompt.gameObject.SetActive(false);
                else
                    Debug.LogWarning("skipPrompt is null; cannot hide skip prompt.");
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

    public void Say(string[] _text, string _characterName = null, float _duration = 0)
    {
        if (_text == null || _text.Length == 0)
        {
            Debug.LogError("Cannot start dialogue with null or empty sentences array!");
            return;
        }
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

    public void Say(string _text, string _characterName = null, float _duration = 0)
    {
        if (string.IsNullOrEmpty(_text))
        {
            Debug.LogError("Cannot start dialogue with null or empty text!");
            return;
        }
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
        StopAllCoroutines();
        isDialogueActive = false;
        animController.SetTrigger("Disappear");
        sentences = null;
        UpdateName();
        textBox.text = "";
        skipButton.gameObject.SetActive(false);
        skipPrompt.gameObject.SetActive(false);
    }
}