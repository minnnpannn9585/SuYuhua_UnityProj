 using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class RepeatPositionTwo
{
    public int lineIndex;       // 行索引
    public int letterIndex;     // 字母索引
    public int repeatCount = 5; // 重复次数（默认5次）
}

public class GameManagerTime : MonoBehaviour
{
    public static GameManagerTime Instance;

    // UI组件
    public TextMeshProUGUI currentText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
    public GameObject continueButton;
    public GameObject retryButton;
    public GameObject gameCompletePanel;
    public Slider repeatProgressBar;  // 重复输入进度条
    public Slider timeProgressBar;    // 时间流逝进度条（目标速度参考）
    public Slider typingProgressBar;  // 打字进度条（当前行完成度）

    // 游戏配置
    public string[] scriptLines;  // 完整剧本
    public List<RepeatPosition> requiredRepeatPositions;  // 重复位置配置
    public float targetSecondsPerChar = 1.5f;  // 每个字符的目标输入时间（秒）
    public float speedTolerance = 0.4f;  // 速度容忍度（±40%）

    // 游戏状态
    private int currentLineIndex = 0;
    private int score = 0;
    private bool isGameActive = true;
    private List<char> currentLineLetters = new List<char>();
    private int currentInputIndex = 0;
    private int currentRepeatCount = 0;
    private bool isInRepeatMode = false;
    private int requiredRepeats = 5;

    // 速度控制相关
    private float currentCharTimer;  // 当前字符的计时
    private bool isSpeedValid = true;

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

    private void Update()
    {
        // 仅在游戏激活且未完成当前行时更新计时器
        if (isGameActive && currentInputIndex < currentLineLetters.Count && !isInRepeatMode)
        {
            currentCharTimer += Time.deltaTime;
            
            // 更新时间流逝进度条（0~1代表目标时间的0%~100%）
            float timeProgress = Mathf.Clamp01(currentCharTimer / targetSecondsPerChar);
            timeProgressBar.value = timeProgress;

            // 超过最大容忍时间提示太慢
            if (currentCharTimer > targetSecondsPerChar * (1 + speedTolerance) && isSpeedValid)
            {
                feedbackText.text = "<color=red>Too slow! Hurry up!</color>";
                isSpeedValid = false;
            }
        }
    }

    private void InitializeGame()
    {
        score = 0;
        currentLineIndex = 0;
        currentInputIndex = 0;
        currentRepeatCount = 0;
        isInRepeatMode = false;
        currentLineLetters.Clear();
        isGameActive = true;
        currentCharTimer = 0;
        isSpeedValid = true;

        // 初始化UI
        UpdateScoreText();
        feedbackText.text = "";
        continueButton.SetActive(false);
        retryButton.SetActive(false);
        gameCompletePanel.SetActive(false);
        repeatProgressBar.gameObject.SetActive(false);
        timeProgressBar.gameObject.SetActive(true);  // 显示时间进度条
        typingProgressBar.gameObject.SetActive(true); // 显示打字进度条
        timeProgressBar.value = 0;
        typingProgressBar.value = 0;

        LoadCurrentLine();
    }

    private void LoadCurrentLine()
    {
        if (currentLineIndex >= scriptLines.Length)
        {
            GameComplete();
            return;
        }

        // 重置当前行状态
        currentLineLetters.Clear();
        currentInputIndex = 0;
        currentRepeatCount = 0;
        isInRepeatMode = false;
        currentCharTimer = 0;
        isSpeedValid = true;

        // 过滤当前行的字母（仅保留字母并转小写）
        foreach (char c in scriptLines[currentLineIndex])
        {
            if (char.IsLetter(c))
            {
                currentLineLetters.Add(char.ToLower(c));
            }
        }

        // 重置进度条
        repeatProgressBar.gameObject.SetActive(false);
        timeProgressBar.value = 0;
        typingProgressBar.value = 0;

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
                bool isRepeatLetter = IsRepeatPosition(currentLineIndex, letterIndex);
                
                if (letterIndex < currentInputIndex)
                {
                    processedText += $"<color=green>{c}</color>";
                }
                else
                {
                    string color = isRepeatLetter ? "#FFA500" : "#000000";
                    processedText += $"<color={color}>{c}</color>";
                }
                letterIndex++;
            }
            else
            {
                processedText += $"<color=#666666>{c}</color>"; // 标点符号灰色
            }
        }

        currentText.text = processedText;

        // 更新打字进度条（当前完成比例）
        if (currentLineLetters.Count > 0)
        {
            float typingProgress = (float)currentInputIndex / currentLineLetters.Count;
            typingProgressBar.value = typingProgress;
        }
    }

    private bool IsRepeatPosition(int lineIndex, int letterIndex)
    {
        if (requiredRepeatPositions == null) return false;
        
        foreach (var pos in requiredRepeatPositions)
        {
            if (pos.lineIndex == lineIndex && pos.letterIndex == letterIndex)
            {
                requiredRepeats = pos.repeatCount;
                return true;
            }
        }
        return false;
    }

    public void ProcessInput(char inputChar)
    {
        if (!isGameActive || currentInputIndex >= currentLineLetters.Count)
            return;
        
        feedbackText.text = "";
        retryButton.SetActive(false);

        char lowerInput = char.ToLower(inputChar);
        char targetChar = currentLineLetters[currentInputIndex];
        bool isRepeatPosition = IsRepeatPosition(currentLineIndex, currentInputIndex);

        if (lowerInput == targetChar)
        {
            if (isRepeatPosition)
            {
                // 处理重复输入模式
                isInRepeatMode = true;
                currentRepeatCount++;
                repeatProgressBar.gameObject.SetActive(true);
                repeatProgressBar.value = (float)currentRepeatCount / requiredRepeats;
                feedbackText.text = $"<color=yellow>Repeat: {currentRepeatCount}/{requiredRepeats}</color>";

                if (currentRepeatCount >= requiredRepeats)
                {
                    CompleteCharacterInput();
                    currentRepeatCount = 0;
                    isInRepeatMode = false;
                    repeatProgressBar.gameObject.SetActive(false);
                }
            }
            else
            {
                // 普通字符输入
                CompleteCharacterInput();
            }
        }
        else
        {
            // 输入错误处理
            if (isRepeatPosition || isInRepeatMode)
            {
                currentRepeatCount = 0;
                isInRepeatMode = false;
                repeatProgressBar.value = 0;
                repeatProgressBar.gameObject.SetActive(false);
            }
            
            // 重置时间计时器
            currentCharTimer = 0;
            timeProgressBar.value = 0;
            isSpeedValid = true;

            feedbackText.text = "<color=red>Wrong! Try again!</color>";
            retryButton.SetActive(true);
        }
    }

    private void CompleteCharacterInput()
    {
        // 检查输入速度是否符合要求
        CheckInputSpeed();
        
        currentInputIndex++;
        score = Mathf.Max(0, score); // 确保分数不为负
        UpdateScoreText();
        UpdateDisplayText();

        // 重置当前字符计时器
        currentCharTimer = 0;
        timeProgressBar.value = 0;
        isSpeedValid = true;

        // 检查是否完成当前行
        if (currentInputIndex >= currentLineLetters.Count)
        {
            OnCurrentLineComplete();
        }
    }

    private void CheckInputSpeed()
    {
        float minTime = targetSecondsPerChar * (1 - speedTolerance); // 最小允许时间（太快）
        float maxTime = targetSecondsPerChar * (1 + speedTolerance); // 最大允许时间（太慢）

        if (currentCharTimer < minTime)
        {
            feedbackText.text = "<color=orange>Too fast! Slow down</color>";
            score -= 1; // 太快扣分
        }
        else if (currentCharTimer > maxTime)
        {
            feedbackText.text = "<color=orange>Too slow! Speed up</color>";
            score -= 1; // 太慢扣分
        }
        else
        {
            feedbackText.text = "<color=green>Good speed!</color>";
            score += 2; // 速度适中额外加分
        }
    }

    private void OnCurrentLineComplete()
    {
        feedbackText.text = "<color=green>Line complete! Ready for next?</color>";
        continueButton.SetActive(true);
        timeProgressBar.gameObject.SetActive(false); // 完成行时隐藏时间进度条
    }

    public void ContinueToNextLine()
    {
        if (!isGameActive) return;

        currentLineIndex++;
        continueButton.SetActive(false);
        feedbackText.text = "";
        timeProgressBar.gameObject.SetActive(true); // 新行显示时间进度条
        LoadCurrentLine();
    }

    public void ResetCurrentLine()
    {
        currentInputIndex = 0;
        currentRepeatCount = 0;
        isInRepeatMode = false;
        currentCharTimer = 0;
        isSpeedValid = true;
        feedbackText.text = "";
        retryButton.SetActive(false);
        repeatProgressBar.gameObject.SetActive(false);
        timeProgressBar.value = 0;
        UpdateDisplayText();
    }

    private void GameComplete()
    {
        isGameActive = false;
        gameCompletePanel.SetActive(true);
        feedbackText.text = "";
        continueButton.SetActive(false);
        timeProgressBar.gameObject.SetActive(false);
        typingProgressBar.gameObject.SetActive(false);
        repeatProgressBar.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        gameCompletePanel.SetActive(false);
        InitializeGame();
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
}