using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

/// <summary>
/// A simple example of Fusion input collection. This component should be on the same GameObject as the <see cref="NetworkRunner"/>.
/// </summary>
[ScriptHelp(BackColor = EditorHeaderBackColor.Steel)]
public class InputBehaviourPrototype : Fusion.Behaviour, INetworkRunnerCallbacks {

  public void OnInput(NetworkRunner runner, NetworkInput input) {
    var frameworkInput = new NetworkInputPrototype();

    if (Input.GetKey(KeyCode.W)) {
      frameworkInput.Buttons |= NetworkInputPrototype.BUTTON_FORWARD;
    }

    if (Input.GetKey(KeyCode.S)) {
      frameworkInput.Buttons |= NetworkInputPrototype.BUTTON_BACKWARD;
    }

    if (Input.GetKey(KeyCode.A)) {
      frameworkInput.Buttons |= NetworkInputPrototype.BUTTON_LEFT;
    }

    if (Input.GetKey(KeyCode.D)) {
      frameworkInput.Buttons |= NetworkInputPrototype.BUTTON_RIGHT;
    }

    if (Input.GetKey(KeyCode.Space)) {
      frameworkInput.Buttons |= NetworkInputPrototype.BUTTON_JUMP;
    }

    if (Input.GetKey(KeyCode.C)) {
      frameworkInput.Buttons |= NetworkInputPrototype.BUTTON_CROUCH;
    }

    if (Input.GetKey(KeyCode.E)) {
      frameworkInput.Buttons |= NetworkInputPrototype.BUTTON_ACTION1;
    }

    if (Input.GetKey(KeyCode.Q)) {
      frameworkInput.Buttons |= NetworkInputPrototype.BUTTON_ACTION2;
    }

    if (Input.GetKey(KeyCode.F)) {
      frameworkInput.Buttons |= NetworkInputPrototype.BUTTON_ACTION3;
    }

    if (Input.GetKey(KeyCode.G)) {
      frameworkInput.Buttons |= NetworkInputPrototype.BUTTON_ACTION4;
    }

    if (Input.GetKey(KeyCode.R)) {
      frameworkInput.Buttons |= NetworkInputPrototype.BUTTON_RELOAD;
    }

    if (Input.GetMouseButton(0)) {
      frameworkInput.Buttons |= NetworkInputPrototype.BUTTON_FIRE;
    }

    input.Set(frameworkInput);
  }

  public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {
  }

  public void OnConnectedToServer(NetworkRunner runner) { }
  
  public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
    // shutdown any client that has failed to connect
    //runner.Shutdown();
  }
  public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
  
  public void OnDisconnectedFromServer(NetworkRunner runner) {
    // shutdown any client that has disconnected from server
    //runner.Shutdown();
  }
  public void OnPlayerJoined(NetworkRunner          runner, PlayerRef            player)                                                           { }
  public void OnPlayerLeft(NetworkRunner            runner, PlayerRef            player)                                                           { }
  public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)                                                          { }
  public void OnObjectWordsChanged(NetworkRunner    runner, NetworkObject        obj, HashSet<int> changedWords, NetworkObjectMemoryPtr oldMemory) { }
  public void OnShutdown(NetworkRunner              runner, ShutdownReason       shutdownReason) { }
  public void OnSessionListUpdated(NetworkRunner    runner, List<SessionInfo>    sessionList)    {  }
  public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) {
  }

  public void OnSceneLoadDone(NetworkRunner runner) {
    
  }

  public void OnSceneLoadStart(NetworkRunner runner) {
  }

  public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {
  }
}

/// <summary>
/// Example definition of an INetworkStruct.
/// </summary>
public struct NetworkInputPrototype : INetworkInput {
  public const uint BUTTON_USE = 1 << 0;
  public const uint BUTTON_FIRE = 1 << 1;
  public const uint BUTTON_FIRE_ALT = 1 << 2;

  public const uint BUTTON_FORWARD = 1 << 3;
  public const uint BUTTON_BACKWARD = 1 << 4;
  public const uint BUTTON_LEFT = 1 << 5;
  public const uint BUTTON_RIGHT = 1 << 6;

  public const uint BUTTON_JUMP = 1 << 7;
  public const uint BUTTON_CROUCH = 1 << 8;
  public const uint BUTTON_WALK = 1 << 9;

  public const uint BUTTON_ACTION1 = 1 << 10;
  public const uint BUTTON_ACTION2 = 1 << 11;
  public const uint BUTTON_ACTION3 = 1 << 12;
  public const uint BUTTON_ACTION4 = 1 << 14;

  public const uint BUTTON_RELOAD = 1 << 15;

  public uint Buttons;
  public byte Weapon;
  public Angle Yaw;
  public Angle Pitch;

  public bool IsUp(uint button) {
    return IsDown(button) == false;
  }

  public bool IsDown(uint button) {
    return (Buttons & button) == button;
  }
}
