using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class echoGameManager : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private GameObject goalObject;
    [SerializeField] private pulseEmitter pulseController;

    [Header("UI Objects")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Game Settings")]
    [SerializeField] private float gameTime = 180f; // 3 minutes
    [SerializeField] private float tutorialTime = 10f;
    [SerializeField] private float victoryDistance = 0.5f;

    private bool gameActive = false;
    private float timeRemaining;

    void Start()
    {
        pulseController.SetMaterialAlpha(0f);
        StartCoroutine(GameStartSequence());
    }

    public void FindGoalObject()
    {
        goalObject = GameObject.FindGameObjectWithTag("Goal").gameObject;
    }

    IEnumerator GameStartSequence()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            yield return new WaitForSeconds(tutorialTime);
            tutorialPanel.SetActive(false);
        }

        pulseController.SetMaterialAlpha(1f);
        StartGame();
    }

    void StartGame()
    {
        timeRemaining = gameTime;
        gameActive = true;

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (!gameActive) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            GameOver();
            return;
        }

        if (goalObject != null)
        {
            float distance = Vector3.Distance(Camera.main.transform.position, goalObject.transform.position);
            if (distance < victoryDistance)
            {
                Victory();
            }
        }
        else Debug.Log("Goal object not found!");
    }

    void Victory()
    {
        gameActive = false;

        pulseController.SetMaterialAlpha(0f);
        if (victoryPanel != null) victoryPanel.SetActive(true);

        OVRInput.SetControllerVibration(0.5f, 0.5f, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0.5f, 0.5f, OVRInput.Controller.LTouch);

        StartCoroutine(RestartAfterDelay(5.0f));
    }

    void GameOver()
    {
        gameActive = false;

        pulseController.SetMaterialAlpha(0f);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        OVRInput.SetControllerVibration(0.8f, 0.8f, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0.8f, 0.8f, OVRInput.Controller.LTouch);

        StartCoroutine(RestartAfterDelay(5.0f));
    }

    IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        pulseController.SetMaterialAlpha(0f);

        StartCoroutine(GameStartSequence());
    }
}
