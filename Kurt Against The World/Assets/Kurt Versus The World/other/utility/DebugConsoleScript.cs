using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugConsoleScript : MonoBehaviour
{
    public inputscript customInputButtons;
    public bool isOpen;
    string input;

    #region commands
    public static DebugCommand YUJI_BOBO;
    public List<object> readyCommands;
    #endregion

    private void Awake()
    {
        #region setting commands
        YUJI_BOBO = new DebugCommand("yuji_bobo", "yuji bobo. thats basically it", "yuji_bobo", () =>
          {
              AudioSource yujiBobo = GameObject.Find("yuji bobo").GetComponent<AudioSource>();
              if (yujiBobo) yujiBobo.Play();
          });
        #endregion

        readyCommands = new List<object>
        {
            YUJI_BOBO
        };
    }
    
    private void Update()
    {
        if (!customInputButtons) return;

        if (customInputButtons.GetInputDown("Open"))
            SetOpenDebugConsole(!isOpen);

        if (customInputButtons.GetInputDown("Enter"))
            EvaluateInput();
        
    }

    private void OnGUI()
    {
        if (!isOpen) return;

        #region draw console
        float y = 0f;
        GUI.backgroundColor = new Color(0, 0, 5f, 0.5f);
        GUI.Box(new Rect(0, y, Screen.width, 30), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);
        #endregion
    }

    public void SetOpenDebugConsole(bool inputBool)
    {
        isOpen = inputBool;
    }

    public void EvaluateInput()
    {
        Debug.Log("Input Evaluated!");
        for(int i = 0; i < readyCommands.Count; i++)
        {
            DebugCommandBase currentCommandBase = readyCommands[i] as DebugCommandBase;
            DebugCommand currentCommand = readyCommands[i] as DebugCommand;

            if(currentCommandBase == null)
            {
                Debug.LogWarning("current command is no where to be found!");
                return;
            }

            if (input.Contains(currentCommandBase.commandID))
            {
                if(currentCommand != null) currentCommand.Invoke();
            }
        }

        input = "";
    }
}
