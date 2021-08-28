
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#if !FUSION_ODIN_EDITOR_ONLY
using Sirenix.Serialization;
#endif
#endif

namespace Fusion {

  /// <summary>
  /// Alternate base class to <see cref="Fusion.Behaviour"/> for use with Odin Inspector. Defaults to the Odin GUI handling when Odin is present.
  /// </summary>
#if ODIN_INSPECTOR
  [HideMonoScript]
#endif
  public abstract unsafe class BehaviourOdin : Fusion.Behaviour {

  }

  /// <summary>
  /// Alternate base class to <see cref="SimulationBehaviour"/> for use with Odin Inspector. Defaults to the Odin GUI handling when Odin is present.
  /// </summary>
#if ODIN_INSPECTOR
  [HideMonoScript]
#endif
  public abstract unsafe class SimulationBehaviourOdin : SimulationBehaviour {

  }

  /// <summary>
  /// Alternate base class to <see cref="NetworkBehaviour"/> for use with Odin Inspector. Defaults to the Odin GUI handling when Odin is present.
  /// </summary>
#if ODIN_INSPECTOR
  [HideMonoScript]
#endif
  public abstract unsafe class NetworkBehaviourOdin : NetworkBehaviour {

  }

#if ODIN_INSPECTOR
  [ShowOdinSerializedPropertiesInInspector]
  [HideMonoScript]
#endif
  public abstract unsafe class BehaviourSerializedOdin : Fusion.Behaviour

#if ODIN_INSPECTOR && !FUSION_ODIN_EDITOR_ONLY

    , ISerializationCallbackReceiver, 
    
    ISupportsPrefabSerialization {

    [SerializeField, HideInInspector]
    private SerializationData serializationData;

    SerializationData ISupportsPrefabSerialization.SerializationData { get { return this.serializationData; } set { this.serializationData = value; } }

    void ISerializationCallbackReceiver.OnAfterDeserialize() {
      UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize() {
      UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
    }
#else
  {
#endif
  }

#if ODIN_INSPECTOR
  [ShowOdinSerializedPropertiesInInspector]
  [HideMonoScript]
#endif
  public abstract unsafe class SimulationBehaviourSerializedOdin : SimulationBehaviour

#if ODIN_INSPECTOR && !FUSION_ODIN_EDITOR_ONLY
    , ISerializationCallbackReceiver, ISupportsPrefabSerialization {

    [SerializeField, HideInInspector]
    private SerializationData serializationData;

    SerializationData ISupportsPrefabSerialization.SerializationData { get { return this.serializationData; } set { this.serializationData = value; } }

    void ISerializationCallbackReceiver.OnAfterDeserialize() {
      UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize() {
      UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
    }
#else
  {
#endif
  }

#if ODIN_INSPECTOR
  [ShowOdinSerializedPropertiesInInspector]
  [HideMonoScript]
#endif
  public abstract unsafe class NetworkBehaviourSerializedOdin : NetworkBehaviour

#if ODIN_INSPECTOR && !FUSION_ODIN_EDITOR_ONLY
    , ISerializationCallbackReceiver, ISupportsPrefabSerialization {

    [SerializeField, HideInInspector]
    private SerializationData serializationData;

    SerializationData ISupportsPrefabSerialization.SerializationData { get { return this.serializationData; } set { this.serializationData = value; } }

    void ISerializationCallbackReceiver.OnAfterDeserialize() {
      UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize() {
      UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
    }
#else
  {
#endif
  }

}
