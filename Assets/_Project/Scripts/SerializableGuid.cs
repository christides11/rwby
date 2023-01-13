using System;
using UnityEngine;

/// <summary>
/// Serializable wrapper for System.Guid.
/// Can be implicitly converted to/from System.Guid.
///
/// Author: Searous
/// </summary>
[Serializable]
public struct SerializableGuid : ISerializationCallbackReceiver {
    private Guid guid;
    [SerializeField] private string serializedGuid;
    
    public SerializableGuid(Guid guid) {
        this.guid = guid;
        serializedGuid = null;
    }
    public override bool Equals(object obj) {
        return obj is SerializableGuid guid &&
               this.guid.Equals(guid.guid);
    }
    
    public override int GetHashCode() {
        return -1324198676 + guid.GetHashCode();
    }
    
    public void OnAfterDeserialize() {
        try {
            guid = Guid.Parse(serializedGuid);
        } catch {
            guid = Guid.Empty;
            Debug.LogWarning($"Attempted to parse invalid GUID string '{serializedGuid}'. GUID will set to System.Guid.Empty");
        }
    }
    
    public void OnBeforeSerialize() {
        serializedGuid = guid.ToString();
    }
    
    public override string ToString() => guid.ToString();
    
    public static bool operator ==(SerializableGuid a, SerializableGuid b) => a.guid == b.guid;
    public static bool operator !=(SerializableGuid a, SerializableGuid b) => a.guid != b.guid;
    public static implicit operator SerializableGuid(Guid guid) => new SerializableGuid(guid);
    public static implicit operator Guid(SerializableGuid serializable) => serializable.guid;
    public static implicit operator SerializableGuid(string serializedGuid) => new SerializableGuid(Guid.Parse(serializedGuid));
    public static implicit operator string(SerializableGuid serializedGuid) => serializedGuid.ToString();
}