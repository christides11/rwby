
using UnityEngine;
using Fusion;

[OrderBefore(typeof(NetworkTransformAnchor))]
[ScriptHelp(BackColor = EditorHeaderBackColor.Steel)]
public class ControllerPrototype : Fusion.NetworkBehaviour {

  protected NetworkCharacterController _ncc;
  protected NetworkRigidbody _nrb;
  protected NetworkRigidbody2D _nrb2d;
  protected NetworkTransform _nt;

  public bool TransformLocal = false;

  /// <summary>
  /// If object is not using <see cref="NetworkCharacterController"/>, this controls how much change is applied to the transform/rigidbody.
  /// </summary>
  [DrawIf(nameof(ShowSpeed), DrawIfHideType.Hide, DoIfCompareOperator.NotEqual)]
  public float Speed = 6f;

  bool HasNCC => GetComponent<NetworkCharacterController>();

  bool ShowSpeed => this && !TryGetComponent<NetworkCharacterController>(out _);

  public override void Spawned() {

    _ncc = GetComponent<NetworkCharacterController>();
    _nrb = GetComponent<NetworkRigidbody>();
    _nrb2d = GetComponent<NetworkRigidbody2D>();
    _nt = GetComponent<NetworkTransform>();
  }

  public override void FixedUpdateNetwork() {
    if (Runner.Config.PhysicsEngine == NetworkProjectConfig.PhysicsEngines.None) {
      return;
    }

    Vector3 direction = default;
    if (GetInput(out NetworkInputPrototype input)) {
      //Debug.Log("There's input " + input.Buttons + " " + Runner.IsClient);

      if (input.IsDown(NetworkInputPrototype.BUTTON_FORWARD)) {
        direction += TransformLocal ? transform.forward : Vector3.forward;
      }

      if (input.IsDown(NetworkInputPrototype.BUTTON_BACKWARD)) {
        direction -= TransformLocal ? transform.forward : Vector3.forward;
      }

      if (input.IsDown(NetworkInputPrototype.BUTTON_LEFT)) {
        direction -= TransformLocal ? transform.right : Vector3.right;
      }

      if (input.IsDown(NetworkInputPrototype.BUTTON_RIGHT)) {
        direction += TransformLocal ? transform.right : Vector3.right;
      }

      direction = direction.normalized;

      if (input.IsDown(NetworkInputPrototype.BUTTON_JUMP)) {
        if (_ncc) {
          _ncc.Jump();
        } else {
          direction += (TransformLocal ? transform.up : Vector3.up);
        }
      }
    }

    if (_ncc) {
      _ncc.Move(direction);
    } else if (_nrb && !_nrb.Rigidbody.isKinematic) {
      _nrb.Rigidbody.AddForce(direction * Speed);
    } else if (_nrb2d && !_nrb2d.Rigidbody.isKinematic) {
      Vector2 direction2d = new Vector2(direction.x, direction.y + direction.z);
      _nrb2d.Rigidbody.AddForce(direction2d * Speed);
    } else {
      transform.position += (direction * Speed * Runner.DeltaTime);
    }
  }
}
