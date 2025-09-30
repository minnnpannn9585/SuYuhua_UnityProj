using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI currentText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
    public GameObject continueButton;
    public GameObject retryButton;
    public GameObject gameCompletePanel;

    [Header("Script Settings")]
    public string[] scriptLines;  // 完整剧本
    public int maxCharactersPerLine = 50;  // 每行最大字符数，超过自动换行

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

    // 加载当前行并过滤出字母
    private void LoadCurrentLine()
    {
        if (currentLineIndex >= scriptLines.Length)
        {
            GameComplete();
            return;
        }

        currentLineLetters.Clear();
        currentInputIndex = 0;

        // 过滤出当前行的所有字母
        foreach (char c in scriptLines[currentLineIndex])
        {
            if (char.IsLetter(c))
            {
                currentLineLetters.Add(char.ToLower(c));
            }
        }

        UpdateDisplayText();
    }

    // 更新显示文本，处理自动换行
    private void UpdateDisplayText()
    {
        string originalText = scriptLines[currentLineIndex];
        string displayText = "";
        int charCount = 0;

        // 构建带有自动换行的显示文本
        foreach (char c in originalText)
        {
            displayText += c;
            charCount++;

            // 达到每行最大字符数且不是最后一个字符时添加换行
            if (charCount >= maxCharactersPerLine && c != originalText[originalText.Length - 1])
            {
                displayText += "\n";
                charCount = 0;
            }
        }

        // 构建富文本，已输入正确的字母显示为绿色
        string processedText = "";
        int letterIndex = 0;

        foreach (char c in displayText)
        {
            if (char.IsLetter(c))
            {
                if (letterIndex < currentInputIndex)
                {
                    // 已正确输入的字母
                    processedText += $"<color=green>{c}</color>";
                }
                else
                {
                    // 未输入的字母（浅灰色）
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

    // 处理玩家输入
    public void ProcessInput(char inputChar)
    {
        if (!isGameActive || currentInputIndex >= currentLineLetters.Count)
            return;

        // 隐藏所有反馈和按钮
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
            feedbackText.text = "<color=red>输入错误，请重新输入</color>";
            retryButton.SetActive(true);
        }
    }

    // 当前行完成
    private void OnCurrentLineComplete()
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = "<color=green>完成！准备下一段</color>";
        continueButton.SetActive(true);
    }

    // 继续到下一行
    public void ContinueToNextLine()
    {
        if (!isGameActive) return;

        currentLineIndex++;
        continueButton.SetActive(false);
        feedbackText.text = "";
        LoadCurrentLine();
    }

    // 重置当前行
    public void ResetCurrentLine()
    {
        currentInputIndex = 0;
        feedbackText.text = "";
        retryButton.SetActive(false);
        UpdateDisplayText();
    }

    // 游戏完成
    private void GameComplete()
    {
        isGameActive = false;
        gameCompletePanel.SetActive(true);
        feedbackText.text = "";
        continueButton.SetActive(false);
    }

    // 重新开始游戏
    public void RestartGame()
    {
        InitializeGame();
    }

    // 更新分数显示
    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
}