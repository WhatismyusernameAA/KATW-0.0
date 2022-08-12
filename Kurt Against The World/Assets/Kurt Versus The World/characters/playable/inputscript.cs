using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inputscript : MonoBehaviour
{
    public string[] ActionNames;
    public KeyCode[] HotKeys;

    Dictionary<string, KeyCode> Actions;

    private void Awake()
    {
        Actions = new Dictionary<string, KeyCode>();

        if (ActionNames.Length != HotKeys.Length)
            Debug.LogWarning("Number of Actions is less or more than the number of hotkeys. This can lead to some inputs not being used.");
        for (int i = 0; i < HotKeys.Length; i++)
        {
            if (i <= ActionNames.Length - 1) Actions.Add(ActionNames[i], HotKeys[i]);
            else Actions.Add("DefaultKey " + i, HotKeys[i]);

        }
    }

    public bool GetInput(string Name)
    {
        bool Output = false;

        if (Actions.ContainsKey(Name))
        {
            KeyCode CurrentKey;
            Actions.TryGetValue(Name, out CurrentKey);

            if (Input.GetKey(CurrentKey) == true) Output = true;
            else Output = false;
        }
        else Debug.LogError("Input Actions does not contain " + Name + "!");

        return Output;
    }

    public bool GetInputDown(string Name)
    {
        bool Output = false;

        if (Actions.ContainsKey(Name))
        {
            KeyCode CurrentKey;
            Actions.TryGetValue(Name, out CurrentKey);

            if (Input.GetKeyDown(CurrentKey) == true) Output = true;
            else Output = false;

        }
        else Debug.LogError("Input Actions does not contain " + Name + "!");

        return Output;
    }

    public bool GetInputUp(string Name)
    {
        bool Output = false;

        if (Actions.ContainsKey(Name))
        {
            KeyCode CurrentKey;
            Actions.TryGetValue(Name, out CurrentKey);

            if (Input.GetKeyUp(CurrentKey) == true) Output = true;
            else Output = false;

        }
        else Debug.LogError("Input Actions does not contain " + Name + "!");

        return Output;
    }
}
