using UnityEngine;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        // 检测键盘输入
        foreach (char c in Input.inputString)
        {
            // 只处理字母输入
            if (char.IsLetter(c))
            {
                GameManager.Instance.ProcessInput(c);
            }
            // 处理退格键 - 重置当前行
            else if (c == '\b')
            {
                GameManager.Instance.ResetCurrentLine();
            }
        }
    }
}