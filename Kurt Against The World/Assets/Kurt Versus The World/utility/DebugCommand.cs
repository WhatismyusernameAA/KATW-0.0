using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommandBase
{
    string _commandID;
    string _commandDescription;
    string _commandFormat;

    public string commandID { get { return _commandID; } }
    public string commandDescription { get { return _commandDescription; } }
    public string commandFormat { get { return _commandDescription; } }

    public DebugCommandBase(string setID, string setDescription, string setFormat)
    {
        _commandID = setID;
        _commandDescription = setDescription;
        _commandFormat = setFormat;
    }
}

public class DebugCommand : DebugCommandBase
{
    public Action command;
    public DebugCommand(string ID, string Description, string Format, Action command) : base (ID,Description,Format)
    {
        this.command = command;
    }

    public void Invoke()
    {
        command.Invoke();
    }
}
