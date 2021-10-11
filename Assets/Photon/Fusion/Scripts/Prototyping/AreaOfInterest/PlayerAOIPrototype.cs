
using Fusion;
using UnityEngine;

/// <summary>
/// Prototyping component for Fusion. Updates the Player's AOI every tick to be a radius around this object.
/// </summary>
[ScriptHelp(BackColor = EditorHeaderBackColor.Steel)]
public class PlayerAOIPrototype : NetworkBehaviour {

  protected enum AuthorityType { InputAuthority = 1, StateAuthority = 2 }

  [SerializeField]
  protected AuthorityType TargetPlayerBy = AuthorityType.InputAuthority;

  [SerializeField]
  protected bool DrawAreaOfInterestRadius;

  /// <summary>
  /// Radius around this GameObject that defines the Area Of Interest for the InputAuthority of the object.
  /// The InputAuthority player of this <see cref="NetworkObject"/>, 
  /// will receive updates for any other <see cref="NetworkObject"/> within this radius. 
  /// </summary>
  public float Radius = 32f;

  public override void FixedUpdateNetwork() {
    var target = TargetPlayerBy == AuthorityType.InputAuthority ? Object.InputAuthority : Object.StateAuthority;

    if (target) {
      Runner.AddPlayerAreaOfInterest(target, position: transform.position, radius: Radius);
    }
  }

  private void OnDrawGizmos() {
    if (DrawAreaOfInterestRadius) {
      var baseColor = Gizmos.color;

      Gizmos.color = Color.white;

      Gizmos.DrawWireSphere(transform.position, Radius);

      Gizmos.color = baseColor;
    }
  }
}