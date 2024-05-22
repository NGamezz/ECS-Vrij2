using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class Command
{
    public Action Action { get; protected set; }
    public bool BackGroundSafe { get; protected set; }
    public Func<bool> Activation { get; protected set; }
    public bool SingleUse { get; protected set; }

    public Command ( Action action, bool backGroundSafe, Func<bool> activation, bool singleUse )
    {
        this.Action = action;
        this.BackGroundSafe = backGroundSafe;
        this.Activation = activation;
        SingleUse = singleUse;
    }
}

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; set; }

    private Thread inputDetectionThread;
    private readonly List<Command> boundCommands = new();

    private bool active = true;

    private void Awake ()
    {
        if ( Instance != null )
            Destroy(Instance);

        Instance = this;

        inputDetectionThread = new(CheckCurrentCommandsForActivation)
        {
            IsBackground = true,
        };
        inputDetectionThread.Start();
    }

    public void BindCommand ( Command command )
    {
        lock ( boundCommands )
        {
            if ( !boundCommands.Contains(command) )
                boundCommands.Add(command);
        }
    }

    public void UnBindCommand ( Command command )
    {
        lock ( boundCommands )
        {
            if ( boundCommands.Contains(command) )
                boundCommands.Remove(command);
        }
    }

    public void UnBindCommand ( int index )
    {
        lock ( boundCommands )
        {
            if ( index < boundCommands.Count )
                boundCommands.RemoveAt(index);
        }
    }

    public Command BindCommand ( Func<bool> activation, Action action, bool backGroundSafe, bool singleUse = false )
    {
        Command cmd = new(action, backGroundSafe, activation, singleUse);
        BindCommand(cmd);
        return cmd;
    }

    private void CheckCurrentCommandsForActivation ()
    {
        while ( active )
        {
            for ( int i = 0; i < boundCommands.Count; i++ )
            {
                if ( i > boundCommands.Count )
                    continue;

                var cmd = boundCommands[i];
                if ( cmd == null )
                    continue;

                if ( cmd.Activation() )
                {
                    if ( cmd.BackGroundSafe )
                        cmd.Action.Invoke();
                    else
                        MainThreadQueue.Instance.Enqueue(cmd.Action);

                    if ( cmd.SingleUse )
                        UnBindCommand(i);
                }
            }
        }
    }

    private void OnDisable ()
    {
        active = false;
        inputDetectionThread.Join();

        boundCommands.Clear();
        Destroy(Instance);
    }

    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState ( int nVirtKey );

    public static bool IsKeyPressed ( VirtualKeys testKey )
    {
        short result = GetAsyncKeyState((int)testKey);

        if ( result < 0 )
            return true;

        return false;
    }
}

public enum VirtualKeys
{
    KeyA = 0x41,
    KeyB = 0x42,
    KeyC = 0x43,
    KeyD = 0x44,
    KeyE = 0x45,
    KeyF = 0x46,
    KeyG = 0x47,
    KeyH = 0x48,
    KeyI = 0x49,
    KeyJ = 0x4A,
    KeyK = 0x4B,
    KeyL = 0x4C,
    KeyM = 0x4D,
    KeyN = 0x4E,
    KeyO = 0x4F,
    KeyP = 0x50,
    KeyQ = 0x51,
    KeyR = 0x52,
    KeyS = 0x53,
    KeyT = 0x54,
    KeyU = 0x55,
    KeyV = 0x56,
    KeyW = 0x57,
    KeyX = 0x58,
    KeyY = 0x59,
    KeyZ = 0x5A,
    LeftBracket = 0x5B,
    Backslash = 0x5C,
    RightBracket = 0x5D,
    Tilde = 0x5E,
}