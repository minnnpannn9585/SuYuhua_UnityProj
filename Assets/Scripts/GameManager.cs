using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI currentText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
    public GameObject continueButton;
    public GameObject retryButton;
    public GameObject gameCompletePanel;

    public string[] scriptLines;  // 完整剧本

    private int currentLineIndex = 0;  // 当前行索引
    private int currentCharIndex = 0;  // 当前字符索引
    private int score = 0;
    private bool isGameActive = true;
    private List<char> currentLineLetters = new List<char>();  // 当前行的所有字母（过滤后）
    private int currentInputIndex = 0;  // 当前输入到的位置

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        score = 0;
        currentLineIndex = 0;
        currentCharIndex = 0;
        currentInputIndex = 0;
        currentLineLetters.Clear();
        isGameActive = true;

        UpdateScoreText();
        feedbackText.text = "";
        continueButton.SetActive(false);
        retryButton.SetActive(false);
        gameCompletePanel.SetActive(false);

        LoadCurrentLine();
    }
    
    private void LoadCurrentLine()
    {
        if (currentLineIndex >= scriptLines.Length)
        {
            GameComplete();
            return;
        }

        currentLineLetters.Clear();
        currentInputIndex = 0;
        
        foreach (char c in scriptLines[currentLineIndex])
        {
            if (char.IsLetter(c))
            {
                currentLineLetters.Add(char.ToLower(c));
            }
        }

        UpdateDisplayText();
    }
    
    private void UpdateDisplayText()
    {
        string originalText = scriptLines[currentLineIndex];

        string processedText = "";
        int letterIndex = 0;

        foreach (char c in originalText)
        {
            if (char.IsLetter(c))
            {
                if (letterIndex < currentInputIndex)
                {
                    processedText += $"<color=green>{c}</color>";
                }
                else
                {
                    processedText += $"<color=#D3D3D3>{c}</color>";
                }
                letterIndex++;
            }
            else
            {
                // 标点和空格保持浅灰色
                processedText += $"<color=#D3D3D3>{c}</color>";
            }
        }

        currentText.text = processedText;
    }

    public void ProcessInput(char inputChar)
    {
        if (!isGameActive || currentInputIndex >= currentLineLetters.Count)
            return;
        
        feedbackText.text = "";
        retryButton.SetActive(false);

        // 转换为小写进行比较
        char lowerInput = char.ToLower(inputChar);

        if (lowerInput == currentLineLetters[currentInputIndex])
        {
            // 输入正确
            currentInputIndex++;
            score++;
            UpdateScoreText();
            UpdateDisplayText();

            // 检查是否完成当前行
            if (currentInputIndex >= currentLineLetters.Count)
            {
                OnCurrentLineComplete();
            }
        }
        else
        {
            // 输入错误
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "<color=red>wrong! try again!</color>";
            retryButton.SetActive(true);
        }
    }
    
    private void OnCurrentLineComplete()
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = "<color=green>finish! prepare for the next line</color>";
        continueButton.SetActive(true);
    }
    
    public void ContinueToNextLine()
    {
        if (!isGameActive) return;

        currentLineIndex++;
        continueButton.SetActive(false);
        feedbackText.text = "";
        LoadCurrentLine();
    }
    
    public void ResetCurrentLine()
    {
        currentInputIndex = 0;
        feedbackText.text = "";
        retryButton.SetActive(false);
        UpdateDisplayText();
    }
    
    private void GameComplete()
    {
        isGameActive = false;
        gameCompletePanel.SetActive(true);
        feedbackText.text = "";
        continueButton.SetActive(false);
    }
    
    public void RestartGame()
    {
        InitializeGame();
    }
    
    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
}