#if !FUSION_DEV

#region Assets/Photon/FusionCodeGen/AssemblyInfo.cs

﻿[assembly: Fusion.NetworkAssemblyIgnore]

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaver.Cache.cs

#if FUSION_WEAVER && FUSION_HAS_MONO_CECIL
namespace Fusion.CodeGen {

  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.CompilerServices;
  using Mono.Cecil;
  using Mono.Cecil.Cil;
  using Mono.Cecil.Rocks;
  using static Fusion.CodeGen.ILWeaverOpCodes;

  partial class ILWeaver {

    private Dictionary<int, FixedBufferInfo> _fixedBuffers = new Dictionary<int, FixedBufferInfo>();
    private Dictionary<string, ElementReaderWriterInfo> _readerWriters = new Dictionary<string, ElementReaderWriterInfo>();
    private Dictionary<string, TypeDefinition> _unitySurrogateTypes = new Dictionary<string, TypeDefinition>();
    private Dictionary<string, TypeDefinition> _blittableType = new Dictionary<string, TypeDefinition>();
    private ElementReaderWriterInfo _instanceReaderWriter = new ElementReaderWriterInfo() {
      InitializeInstance = il => { },
      LoadInstance = il => il.Append(Ldarg_0()),
    };

    private TypeReference CacheGetBlittableType(ILWeaverAssembly asm, PropertyDefinition property, TypeReference elementType) {

      if (_blittableType.TryGetValue(elementType.FullName, out var blittableType)) {
        return blittableType;
      } else if (elementType.IsFloat() || elementType.IsVector2() || elementType.IsVector3()) {
        blittableType = new TypeDefinition("Fusion.CodeGen", $"Blittable{elementType.Name}@{elementType.Name}",
          TypeAttributes.NotPublic | TypeAttributes.AnsiClass | TypeAttributes.ExplicitLayout | TypeAttributes.Sealed | TypeAttributes.Serializable | TypeAttributes.BeforeFieldInit,
          asm.ValueType);

        blittableType.AddTo(asm.CecilAssembly);
        blittableType.AddInterface<INetworkStruct>(asm);
        blittableType.AddAttribute<NetworkStructWeavedAttribute, int>(asm, GetTypeWordCount(asm, elementType));

        if (elementType.IsFloat()) {
          blittableType.Fields.Add(new FieldDefinition("Value", FieldAttributes.Public, asm.WordSizedPrimitive));
        } else if (elementType.IsVector2()) {
          blittableType.Fields.Add(new FieldDefinition("X", FieldAttributes.Public, asm.WordSizedPrimitive));
          blittableType.Fields.Add(new FieldDefinition("Y", FieldAttributes.Public, asm.WordSizedPrimitive));
        } else if (elementType.IsVector3()) {
          blittableType.Fields.Add(new FieldDefinition("X", FieldAttributes.Public, asm.WordSizedPrimitive));
          blittableType.Fields.Add(new FieldDefinition("Y", FieldAttributes.Public, asm.WordSizedPrimitive));
          blittableType.Fields.Add(new FieldDefinition("Z", FieldAttributes.Public, asm.WordSizedPrimitive));
        }

        for (int i = 0; i < blittableType.Fields.Count; ++i) {
          blittableType.Fields[i].Offset = i * Allocator.REPLICATE_WORD_SIZE;
        }

        _blittableType.Add(elementType.FullName, blittableType);
        return blittableType;
      } else {
        return null;
      }
    }

    private FixedBufferInfo CacheGetFixedBuffer(ILWeaverAssembly asm, int wordCount) {
      if (!_fixedBuffers.TryGetValue(wordCount, out var entry)) {
        // fixed buffers could be included directly in structs, but then again it would be impossible to provide a custom drawer;
        // that's why there's this proxy struct
        var storageType = new TypeDefinition("Fusion.CodeGen", $"FixedStorage@{wordCount}",
          TypeAttributes.ExplicitLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit | TypeAttributes.Serializable,
          asm.ValueType);

        storageType.AddTo(asm.CecilAssembly);
        storageType.AddInterface<INetworkStruct>(asm);
        storageType.AddAttribute<NetworkStructWeavedAttribute, int>(asm, wordCount);

        FieldDefinition bufferField;
        if (Allocator.REPLICATE_WORD_SIZE == sizeof(int)) {
          bufferField = CreateFixedBufferField(asm, storageType, $"Data", asm.Import(typeof(int)), wordCount);
          bufferField.Offset = 0;

          // Unity debugger seems to copy only the first element of a buffer,
          // the rest is garbage when inspected; let's add some additional
          // fields to help it
          for (int i = 1; i < wordCount; ++i) {
            var unityDebuggerWorkaroundField = new FieldDefinition($"_{i}", FieldAttributes.Private | FieldAttributes.NotSerialized, asm.Import<int>());
            unityDebuggerWorkaroundField.Offset = Allocator.REPLICATE_WORD_SIZE * i;
            unityDebuggerWorkaroundField.AddTo(storageType);
          }

        }

        entry = new FixedBufferInfo() {
          Type = storageType,
          PointerField = bufferField
        };

        _fixedBuffers.Add(wordCount, entry);
      }
      return entry;
    }

    static FieldDefinition CreateFixedBufferField(ILWeaverAssembly asm, TypeDefinition type, string fieldName, TypeReference elementType, int elementCount) {
      var fixedBufferType = new TypeDefinition("", $"<{fieldName}>e__FixedBuffer", TypeAttributes.SequentialLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit | TypeAttributes.NestedPublic) {
        BaseType = asm.Import(typeof(ValueType)),
        PackingSize = 0,
        ClassSize = elementCount * elementType.GetPrimitiveSize(),
      };
      fixedBufferType.AddAttribute<CompilerGeneratedAttribute>(asm);
      fixedBufferType.AddAttribute<UnsafeValueTypeAttribute>(asm);
      fixedBufferType.AddTo(type);

      var elementField = new FieldDefinition("FixedElementField", FieldAttributes.Private, elementType);
      elementField.AddTo(fixedBufferType);

      var field = new FieldDefinition(fieldName, FieldAttributes.Public, fixedBufferType);
      field.AddAttribute<FixedBufferAttribute, TypeReference, int>(asm, elementType, elementCount);
      field.AddTo(type);

      return field;
    }


    private ElementReaderWriterInfo MakeElementReaderWriter(ILWeaverAssembly asm, PropertyDefinition property, TypeReference elementType) {

      var interfaceType = asm.Import(typeof(IElementReaderWriter<>)).MakeGenericInstanceType(elementType);

      if (TryGetNetworkWrapperType(elementType, out var wrapInfo)) {
        if (!property.DeclaringType.Is<NetworkBehaviour>()) {
          throw new ILWeaverException($"{elementType} needs wrapping - such types are only supported as NetworkBehaviour properties.");
        }

        var wordCount = GetTypeWordCount(asm, elementType);

        // let's add an interface!
        var behaviour = property.DeclaringType;
        if (!behaviour.Interfaces.Any(x => x.InterfaceType.FullName == interfaceType.FullName)) {
          Log.Debug($"Adding interface {behaviour} {interfaceType}");
          AddIElementReaderWriterImplementation(asm, behaviour, property, elementType, wordCount, isExplicit: true);
        }
        return _instanceReaderWriter;

      } else {
        var readerWriterName = "ReaderWriter@" + elementType.FullName.Replace(".", "_");

        int wordCount;

        if (property.PropertyType.IsString()) {
          wordCount = GetPropertyWordCount(asm, property);
          readerWriterName += $"@{wordCount}";
        } else {
          wordCount = GetTypeWordCount(asm, elementType);
          if (TryGetFloatAccuracy(property, out var accuracy)) {
            uint value = BitConverter.ToUInt32(BitConverter.GetBytes(accuracy), 0);
            readerWriterName += $"@{value:x}";
          }
        }

        if (_readerWriters.TryGetValue(readerWriterName, out var entry)) {
          return entry;
        }

        var readerWriterType = new TypeDefinition("Fusion.CodeGen", readerWriterName,
          TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.SequentialLayout | TypeAttributes.BeforeFieldInit, asm.ValueType);

        // without this, VS debugger will crash
        readerWriterType.PackingSize = 0;
        readerWriterType.ClassSize = 1;

        readerWriterType.AddTo(asm.CecilAssembly);

        AddIElementReaderWriterImplementation(asm, readerWriterType, property, elementType, wordCount);

        var instanceField = new FieldDefinition("Instance", FieldAttributes.Public | FieldAttributes.Static, interfaceType);
        instanceField.AddTo(readerWriterType);

        var initializeMethod = new MethodDefinition($"EnsureInitialized", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, asm.Import(typeof(void)));
        initializeMethod.AddAttribute<MethodImplAttribute, MethodImplOptions>(asm, MethodImplOptions.AggressiveInlining);
        initializeMethod.AddTo(readerWriterType);

        {
          var il = initializeMethod.Body.GetILProcessor();
          var ret = Instruction.Create(OpCodes.Ret);

          var tmpVar = new VariableDefinition(readerWriterType);
          il.Body.Variables.Add(tmpVar);

          il.Append(Ldsfld(instanceField));
          il.Append(Brtrue_S(ret));

          il.Append(Instruction.Create(OpCodes.Ldloca_S, tmpVar));
          il.Append(Instruction.Create(OpCodes.Initobj, readerWriterType));
          il.Append(Instruction.Create(OpCodes.Ldloc_0));
          il.Append(Instruction.Create(OpCodes.Box, readerWriterType));
          il.Append(Instruction.Create(OpCodes.Stsfld, instanceField));

          il.Append(ret);
        }

        entry = new ElementReaderWriterInfo() {
          InitializeInstance = il => il.Append(Call(initializeMethod)),
          LoadInstance = il => il.Append(Ldsfld(instanceField)),
          Type = readerWriterType
        };

        _readerWriters.Add(readerWriterName, entry);
        return entry;
      }
    }

    private void AddIElementReaderWriterImplementation(ILWeaverAssembly asm, TypeDefinition readerWriterType, PropertyDefinition property, TypeReference elementType, int elementWordCount, bool isExplicit = false) {

      var dataType = asm.Import(typeof(byte*));
      var indexType = asm.Import(typeof(int));
      var interfaceType = asm.Import(typeof(IElementReaderWriter<>)).MakeGenericInstanceType(elementType);
      
      readerWriterType.Interfaces.Add(new InterfaceImplementation(interfaceType));

      var visibility = isExplicit ? MethodAttributes.Private : MethodAttributes.Public;
      var namePrefix = isExplicit ? $"CodeGen@ElementReaderWriter<{elementType.FullName}>." : "";

      var readMethod = new MethodDefinition($"{namePrefix}Read",
        visibility | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
        elementType);

      readMethod.Parameters.Add(new ParameterDefinition("data", ParameterAttributes.None, dataType));
      readMethod.Parameters.Add(new ParameterDefinition("index", ParameterAttributes.None, indexType));
      readMethod.AddAttribute<MethodImplAttribute, MethodImplOptions>(asm, MethodImplOptions.AggressiveInlining);
      readMethod.AddTo(readerWriterType);

      var writeMethod = new MethodDefinition($"{namePrefix}Write",
        visibility | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
        asm.Void);

      writeMethod.Parameters.Add(new ParameterDefinition("data", ParameterAttributes.None, dataType));
      writeMethod.Parameters.Add(new ParameterDefinition("index", ParameterAttributes.None, indexType));
      writeMethod.Parameters.Add(new ParameterDefinition("val", ParameterAttributes.None, elementType));
      writeMethod.AddAttribute<MethodImplAttribute, MethodImplOptions>(asm, MethodImplOptions.AggressiveInlining);
      writeMethod.AddTo(readerWriterType);

      var getElementWordCountMethod = new MethodDefinition($"{namePrefix}GetElementWordCount",
        visibility | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
        asm.Import(typeof(int)));

      getElementWordCountMethod.AddAttribute<MethodImplAttribute, MethodImplOptions>(asm, MethodImplOptions.AggressiveInlining);
      
      if (isExplicit) {
        readMethod.Overrides.Add(interfaceType.GetGenericInstanceMethodOrThrow(nameof(IElementReaderWriter<int>.Read)));
        writeMethod.Overrides.Add(interfaceType.GetGenericInstanceMethodOrThrow(nameof(IElementReaderWriter<int>.Write)));
        getElementWordCountMethod.Overrides.Add(interfaceType.GetGenericInstanceMethodOrThrow(nameof(IElementReaderWriter<int>.GetElementWordCount)));
      }

      var getterMethodIL = readMethod.Body.GetILProcessor();
      var setterMethodIL = writeMethod.Body.GetILProcessor();

      InjectValueAccessor(asm, getterMethodIL, setterMethodIL, property, elementType, OpCodes.Ldarg_3, (il, offset) => {
        LoadArrayElementAddress(il, OpCodes.Ldarg_1, OpCodes.Ldarg_2, elementWordCount, offset);
      }, false);

      getElementWordCountMethod.AddTo(readerWriterType);
      {
        var il = getElementWordCountMethod.Body.GetILProcessor();
        il.Append(Ldc_I4(elementWordCount));
        il.Append(Ret());
      }
    }


    private string TypeNameToIdentifier(TypeReference type, string prefix) {
      string result = type.FullName;
      result = result.Replace("`1", "");
      result = result.Replace("`2", "");
      result = result.Replace("`3", "");
      result = result.Replace(".", "_");
      result = prefix + result;
      return result;
    }

    private TypeDefinition CacheGetUnitySurrogate(ILWeaverAssembly asm, PropertyDefinition property) {
      var type = property.PropertyType;

      GenericInstanceType baseType;
      string surrogateName;

      if (type.IsNetworkDictionary(out var keyType, out var valueType)) {
        keyType = asm.Import(keyType);
        valueType = asm.Import(valueType);
        var keyReaderWriterType = MakeElementReaderWriter(asm, property, keyType).Type;
        var valueReaderWriterType = MakeElementReaderWriter(asm, property, valueType).Type;
        baseType = asm.Import(typeof(Fusion.Internal.UnityDictionarySurrogate<,,,>)).MakeGenericInstanceType(keyType, keyReaderWriterType, valueType, valueReaderWriterType);
        surrogateName = "UnityDictionarySurrogate@" + keyReaderWriterType.Name + "@" + valueReaderWriterType.Name;
      } else if (type.IsNetworkArray(out var elementType)) {
        elementType = asm.Import(elementType);
        var readerWriterType = MakeElementReaderWriter(asm, property, elementType).Type;
        baseType = asm.Import(typeof(Fusion.Internal.UnityArraySurrogate<,>)).MakeGenericInstanceType(elementType, readerWriterType);
        surrogateName = "UnityArraySurrogate@" + readerWriterType.Name;
      } else if (type.IsNetworkList(out elementType)) {
        elementType = asm.Import(elementType);
        var readerWriterType = MakeElementReaderWriter(asm, property, elementType).Type;
        baseType = asm.Import(typeof(Fusion.Internal.UnityLinkedListSurrogate<,>)).MakeGenericInstanceType(elementType, readerWriterType);
        surrogateName = "UnityLinkedListSurrogate@" + readerWriterType.Name;
      } else {
        var readerWriterType = MakeElementReaderWriter(asm, property, property.PropertyType).Type;
        baseType = asm.Import(typeof(Fusion.Internal.UnityValueSurrogate<,>)).MakeGenericInstanceType(property.PropertyType, readerWriterType);
        surrogateName = "UnityValueSurrogate@" + readerWriterType.Name;
      }

      if (!_unitySurrogateTypes.TryGetValue(surrogateName, out var surrogateType)) {
        surrogateType = new TypeDefinition("Fusion.CodeGen", surrogateName,
          TypeAttributes.NotPublic | TypeAttributes.AnsiClass | TypeAttributes.Serializable | TypeAttributes.BeforeFieldInit,
          baseType);

        surrogateType.AddTo(asm.CecilAssembly);
        surrogateType.AddEmptyConstructor(asm);

        _unitySurrogateTypes.Add(surrogateName, surrogateType);
      }
      return surrogateType;
    }

    struct ElementReaderWriterInfo {
      public Action<ILProcessor> InitializeInstance;
      public Action<ILProcessor> LoadInstance;
      public TypeDefinition Type;
    }

    struct FixedBufferInfo {
      public FieldDefinition PointerField;
      public TypeDefinition Type;
    }
  }
}
#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaver.cs

#if FUSION_WEAVER && FUSION_HAS_MONO_CECIL
namespace Fusion.CodeGen {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using UnityEditor;
  using UnityEditor.Compilation;
  using UnityEngine;
  using System.Runtime.CompilerServices;
  using static Fusion.CodeGen.ILWeaverOpCodes;
  using Mono.Cecil;
  using Mono.Cecil.Cil;
  using Mono.Cecil.Rocks;
  using Mono.Collections.Generic;
  using CompilerAssembly = UnityEditor.Compilation.Assembly;
  using FieldAttributes = Mono.Cecil.FieldAttributes;
  using MethodAttributes = Mono.Cecil.MethodAttributes;
  using ParameterAttributes = Mono.Cecil.ParameterAttributes;

  public unsafe partial class ILWeaver {


    void AddUnmanagedType<T>() where T : unmanaged {
      AddUnmanagedType<T>(sizeof(T));
    }

    void AddUnmanagedType<T>(int size) where T : unmanaged {
      unsafe {
        var wordCount = Native.WordCount(size, Allocator.REPLICATE_WORD_SIZE);
        _typeData.Add(typeof(T).FullName, new TypeMetaData(wordCount));
      }
    }

    void SetDefaultTypeData() {
      _networkedBehaviourTypeData = new Dictionary<string, BehaviourMetaData>();
      _typeData = new Dictionary<string, TypeMetaData>();
      _rpcCount = new Dictionary<string, int>();

      _typeData.Add("NetworkedObject", new TypeMetaData(2));

      AddUnmanagedType<bool>(sizeof(int));

      AddUnmanagedType<byte>();
      AddUnmanagedType<sbyte>();

      AddUnmanagedType<Int16>();
      AddUnmanagedType<UInt16>();

      AddUnmanagedType<Int32>();
      AddUnmanagedType<UInt32>();

      AddUnmanagedType<Int64>();
      AddUnmanagedType<UInt64>();

      AddUnmanagedType<float>();
      AddUnmanagedType<double>();

      AddUnmanagedType<Vector2>();
      AddUnmanagedType<Vector3>();
      AddUnmanagedType<Vector4>();
      AddUnmanagedType<Quaternion>();
      AddUnmanagedType<Matrix4x4>();

      AddUnmanagedType<Vector2Int>();
      AddUnmanagedType<Vector3Int>();

      AddUnmanagedType<BoundingSphere>();
      AddUnmanagedType<Bounds>();
      AddUnmanagedType<Rect>();
      AddUnmanagedType<Color>();

      AddUnmanagedType<BoundsInt>();
      AddUnmanagedType<RectInt>();
      AddUnmanagedType<Color32>();
      
      AddUnmanagedType<Guid>();
    }

    public static bool IsEditorAssemblyPath(string path) {
      return path.Contains("-Editor") || path.Contains(".Editor");
    }

    struct WrapInfo {
      public MethodDefinition WrapMethod;
      public MethodDefinition UnwrapMethod;
      public TypeReference WrapperType;
      public TypeReference Type;
      public int MaxRawByteCount;
      public bool UnwrapByRef;
      public bool IsRaw => MaxRawByteCount > 0;
    }

    bool TryGetNetworkWrapperType(TypeReference type, out WrapInfo result) {
      if (type == null) {
        result = default;
        return false;
      }

      var definition = type.TryResolve();
      if (definition == null) {
        result = default;
        return false;
      }

      int rawByteCount = 0;

      if (definition.GetSingleOrDefaultMethodWithAttribute<NetworkSerializeMethodAttribute>(out var wrapAttribute, out var wrapMethod)) {
        wrapAttribute.TryGetAttributeProperty<int>(nameof(NetworkSerializeMethodAttribute.MaxSize), out rawByteCount);

        if (rawByteCount > 0) {
          try {
            wrapMethod.ThrowIfNotStatic()
                      .ThrowIfNotPublic()
                      .ThrowIfParameterCount(3)
                      .ThrowIfParameter(0, typeof(NetworkRunner))
                      .ThrowIfParameter(1, type)
                      .ThrowIfParameter(2, typeof(byte*))
                      .ThrowIfReturnType(typeof(int));
          } catch (Exception ex) {
            throw new ILWeaverException($"Method marked with {nameof(NetworkSerializeMethodAttribute)} has an invalid signature RAW", ex);
          }
        } else {
          try {
            wrapMethod.ThrowIfNotStatic()
                      .ThrowIfNotPublic()
                      .ThrowIfParameterCount(2)
                      .ThrowIfParameter(0, typeof(NetworkRunner))
                      .ThrowIfParameter(1, type);
          } catch (Exception ex) {
            throw new ILWeaverException($"Method marked with {nameof(NetworkSerializeMethodAttribute)} has an invalid signature", ex);
          }
        }
      }

      bool cacheUnwrap = false;

      if (definition.GetSingleOrDefaultMethodWithAttribute<NetworkDeserializeMethodAttribute>(out var unwrapAttribute, out var unwrapMethod)) {
        if (wrapMethod == null) {
          throw new ILWeaverException($"Method marked with {nameof(NetworkDeserializeMethodAttribute)}, but there is no method marked with {nameof(NetworkSerializeMethodAttribute)}: {unwrapMethod}");
        }

        if (rawByteCount > 0) {
          try {
            unwrapMethod.ThrowIfNotStatic()
                        .ThrowIfNotPublic()
                        .ThrowIfReturnType(typeof(int))
                        .ThrowIfParameterCount(3)
                        .ThrowIfParameter(0, typeof(NetworkRunner))
                        .ThrowIfParameter(1, typeof(byte*))
                        .ThrowIfParameter(2, type, isByReference: true);
            cacheUnwrap = true;
          } catch (Exception ex) {
            throw new ILWeaverException($"Method marked with {nameof(NetworkDeserializeMethodAttribute)} has an invalid signature RAW", ex);
          }
        } else {
          try {
            unwrapMethod.ThrowIfNotStatic()
                        .ThrowIfNotPublic()
                        .ThrowIfParameter(0, typeof(NetworkRunner))
                        .ThrowIfParameter(1, wrapMethod.ReturnType);
            
            if (unwrapMethod.Parameters.Count == 3) {
              unwrapMethod.ThrowIfReturnType(typeof(void))
                          .ThrowIfParameter(2, type, isByReference: true);
              cacheUnwrap = true;
            } else if (unwrapMethod.Parameters.Count == 2) {
              unwrapMethod.ThrowIfReturnType(type);
            } else {
              throw new ILWeaverException($"Expected 2 or 3 parameters");
            }
          } catch (Exception ex) {
            throw new ILWeaverException($"Method marked with {nameof(NetworkDeserializeMethodAttribute)} has an invalid signature", ex);
          }
          
        }
      } else if (wrapMethod != null) {
        throw new ILWeaverException($"Method marked with {nameof(NetworkSerializeMethodAttribute)}, but there is no method marked with {nameof(NetworkDeserializeMethodAttribute)}: {wrapMethod}");
      }


      if (wrapMethod != null && unwrapMethod != null) {
        result = new WrapInfo() {
          MaxRawByteCount = rawByteCount,
          UnwrapMethod    = unwrapMethod,
          WrapMethod      = wrapMethod,
          WrapperType     = rawByteCount > 0 ? null : wrapMethod.ReturnType,
          Type            = type,
          UnwrapByRef     = cacheUnwrap,
        };
        return true;
      }

      return TryGetNetworkWrapperType(definition.BaseType, out result);
    }

    TypeDefinition FindNetworkedBehaviourTypeDef(TypeDefinition type) {
      if (type == null || type.IsClass == false || type.BaseType == null) {
        return null;
      }

      if (type.BaseType.Name == nameof(NetworkBehaviour)) {
        return type.BaseType.TryResolve();
      }

      return FindNetworkedBehaviourTypeDef(type.BaseType.TryResolve());
    }

    class TypeMetaData {
      public TypeReference Reference;
      public TypeDefinition Definition;
      public int WordCount;

      public TypeMetaData() {
      }

      public TypeMetaData(int wordCount) {
        WordCount = wordCount;
      }
    }

    class BehaviourMetaData {
      public TypeDefinition Definition;
      public int BlockCount;
    }



    const string REF_FIELD_NAME = "Ref";
    const string PTR_FIELD_NAME = nameof(NetworkBehaviour.Ptr);
    const string OBJECT_FIELD_NAME = nameof(NetworkBehaviour.Object);
    const string RUNNER_FIELD_NAME = nameof(NetworkBehaviour.Runner);

    const string FIND_OBJECT_METHOD_NAME = nameof(NetworkRunner.FindObject);

    Dictionary<string, TypeMetaData> _typeData = new Dictionary<string, TypeMetaData>();
    Dictionary<string, BehaviourMetaData> _networkedBehaviourTypeData = new Dictionary<string, BehaviourMetaData>();
    Dictionary<string, int> _rpcCount = new Dictionary<string, int>();


    internal readonly ILWeaverLog Log;

    public ILWeaver(ILWeaverLog log) {
      Log = log;
      SetDefaultTypeData();
    }



    FieldReference GetFieldFromNetworkedBehaviour(ILWeaverAssembly assembly, TypeDefinition type, string fieldName) {
      if (type.Name == nameof(NetworkBehaviour)) {
        foreach (var fieldDefinition in type.Fields) {
          if (fieldDefinition.Name == fieldName) {
            return assembly.CecilAssembly.MainModule.ImportReference(fieldDefinition);
          }
        }
      }

      if (type.IsSubclassOf<NetworkBehaviour>()) {
        return GetFieldFromNetworkedBehaviour(assembly, type.BaseType.TryResolve(), fieldName);
      }

      Assert.Fail();
      return null;
    }

    MethodReference GetMetaAttributeConstructor(ILWeaverAssembly asm) {
      return asm.CecilAssembly.MainModule.ImportReference(typeof(NetworkBehaviourWeavedAttribute).GetConstructors()[0]);
    }

    TypeReference ImportType<T>(ILWeaverAssembly asm) {
      return asm.CecilAssembly.MainModule.ImportReference(typeof(T));
    }

    int GetNetworkBehaviourWordCount(ILWeaverAssembly asm, TypeDefinition type) {
      if (type.Name == nameof(NetworkBehaviour)) {
        return 0;
      }

      // has to be network behaviour
      Assert.Always(type.IsSubclassOf<NetworkBehaviour>());

      // make sure parent is weaved
      WeaveBehaviour(asm, type);

      // assert this.. but should always be true
      Assert.Always(type.HasAttribute<NetworkBehaviourWeavedAttribute>());

      // this always has to exist
      return _networkedBehaviourTypeData[type.FullName].BlockCount;
    }

    FieldDefinition FindBackingField(TypeDefinition type, string property) {
      // compute backing field name...
      var backingFieldName = $"<{property}>k__BackingField";

      // find backing field
      return type.Fields.FirstOrDefault(x => x.IsPrivate && x.Name == backingFieldName);
    }

    void LoadDataAddress(ILProcessor il, FieldReference field, int wordCount) {
      il.Append(Instruction.Create(OpCodes.Ldarg_0));
      il.Append(Instruction.Create(OpCodes.Ldfld, field));
      il.Append(Instruction.Create(OpCodes.Ldc_I4, wordCount * Allocator.REPLICATE_WORD_SIZE));
      il.Append(Instruction.Create(OpCodes.Add));
    }    

    int GetWordCount(ILWeaverAssembly asm, WrapInfo wrapInfo) {
      return wrapInfo.WrapperType != null ? GetTypeWordCount(asm, wrapInfo.WrapperType) : Native.WordCount(wrapInfo.MaxRawByteCount, Allocator.REPLICATE_WORD_SIZE);
    }

    int GetByteCount(ILWeaverAssembly asm, WrapInfo wrapInfo) {
      return GetWordCount(asm, wrapInfo) * Allocator.REPLICATE_WORD_SIZE;
    }

    int GetTypeWordCount(ILWeaverAssembly asm, TypeReference type) {
      if (type.IsPointer || type.IsByReference) {
        type = type.GetElementTypeEx();
      }

      // TODO: what is this?
      if (type.IsNetworkArray(out var elementType)) {
        type = elementType;
      } else if (type.IsNetworkList(out elementType)) {
        type = elementType;
      }

      if (_typeData.TryGetValue(type.FullName, out var data) == false) {
        var typeDefinition = type.TryResolve();
        if (typeDefinition == null) {
          throw new ILWeaverException($"Could not resolve type {type}");
        }

        if (typeDefinition.IsEnum) {
          _typeData.Add(type.FullName, data = new TypeMetaData {
            WordCount = 1,
            Definition = typeDefinition
          });
        } else if (typeDefinition.IsValueType && typeDefinition.Is<INetworkStruct>()) {
          // weave this struct
          WeaveStruct(asm, typeDefinition, type);

          // grab type data
          data = _typeData[type.FullName];
        } else if (type.IsFixedBuffer(out var size)) {
          _typeData.Add(type.FullName, data = new TypeMetaData {
            WordCount = Native.WordCount(size, Allocator.REPLICATE_WORD_SIZE),
            Definition = typeDefinition
          });
        } else if (TryGetNetworkWrapperType(type, out var wrapInfo)) {
          _typeData.Add(type.FullName, data = new TypeMetaData {
            WordCount = GetWordCount(asm, wrapInfo),
            Definition = typeDefinition
          });
        } else {
          if (type.IsValueType) {
            throw new ILWeaverException($"Value type {type} does not implement {nameof(INetworkStruct)}");
          } else {
            throw new ILWeaverException($"Reference type {type} is not supported");
          }
        }
      }

      return data.WordCount;
    }

    int GetByteCount(ILWeaverAssembly asm, TypeReference type) {
      return GetTypeWordCount(asm, type) * Allocator.REPLICATE_WORD_SIZE;
    }

    int GetPropertyWordCount(ILWeaverAssembly asm, PropertyDefinition property) {
      //if (property.PropertyType.Is<NetworkBehaviour>() || property.PropertyType.Is<NetworkObject>()) {
      //  return 2;
      //}

      if (property.PropertyType.IsString()) {
        if (property.DeclaringType.IsValueType) {
          return 1 + GetStringCapacity(property);
        } else {
          return 2 + GetStringCapacity(property);
        }
      }

      if (property.PropertyType.IsNetworkArray()) {
        return GetStaticArrayCapacity(property) * GetTypeWordCount(asm, GetStaticArrayElementType(property.PropertyType));
      }

      if (property.PropertyType.IsNetworkList()) {
        return NetworkLinkedList<int>.META_WORDS + (GetStaticListCapacity(property) * (GetTypeWordCount(asm, GetStaticListElementType(property.PropertyType)) + NetworkLinkedList<int>.ELEMENT_WORDS));
      }
      
      if (property.PropertyType.IsNetworkDictionary()) {
      
        var capacity = GetStaticDictionaryCapacity(property);

        return
          // meta data (counts, etc)
          NetworkDictionary<int, int>.META_WORD_COUNT                                           +
          
          // buckets
          (capacity)                                                                            +
          
          // entry
          
            // next
            (capacity) +
          
            // key
            (capacity * GetTypeWordCount(asm, GetStaticDictionaryKeyType(property.PropertyType))) +
          
            // value
            (capacity * GetTypeWordCount(asm, GetStaticDictionaryValType(property.PropertyType)));
      }
      

      return GetTypeWordCount(asm, property.PropertyType);
    }

    static void FloatDecompress(ILProcessor il, float accuracy) {
      if (accuracy == 0) {
        il.Append(Instruction.Create(OpCodes.Conv_R4));
      } else {
        il.Append(Instruction.Create(OpCodes.Conv_R4));
        il.Append(Instruction.Create(OpCodes.Ldc_R4, accuracy));
        il.Append(Instruction.Create(OpCodes.Mul));
      }
    }

    void FloatCompress(ILProcessor il, float accuracy) {
      il.Append(Instruction.Create(OpCodes.Ldc_R4, 1.0f / accuracy));
      il.Append(Instruction.Create(OpCodes.Mul));
      il.Append(Instruction.Create(OpCodes.Ldc_R4, 0.5f));
      il.Append(Instruction.Create(OpCodes.Add));
      il.Append(Instruction.Create(OpCodes.Conv_I4));
    }

    int GetStringCapacity(PropertyDefinition property) {
      return Math.Max(1, GetCapacity(property, 16));
    }

    int GetStaticArrayCapacity(PropertyDefinition property) {
      return Math.Max(1, GetCapacity(property, 1));
    }

    int GetStaticListCapacity(PropertyDefinition property) {
      return Math.Max(1, GetCapacity(property, 1));
    }
    
    int GetStaticDictionaryCapacity(PropertyDefinition property) {
      return Primes.GetNextPrime(Math.Max(1, GetCapacity(property, 1)));
    }
    
    int GetCapacity(PropertyDefinition property, int defaultCapacity) {
      int size;

      if (property.TryGetAttribute<CapacityAttribute>(out var attr)) {
        size = (int)attr.ConstructorArguments[0].Value;
      } else {
        size = defaultCapacity;
      }

      return size;
    }

    bool TryGetFloatAccuracy(PropertyDefinition property, out float accuracy ) {
      if (property.TryGetAttribute<AccuracyAttribute>(out var attr)) {
        var obj = attr.ConstructorArguments[0];

        // If the argument is a string, this is using a global Accuracy. Need to look up the value.
        if (obj.Value is string str) {
          accuracy = ILWeaverSettings.GetNamedFloatAccuracy(str);
        } else {
          var val = attr.ConstructorArguments[0].Value;
          if (val is float fval) {
            accuracy = fval;
          } else if (val is double dval) {
            accuracy = (float)dval;
          } else {
            throw new Exception($"Invalid argument type: {val.GetType()}");
          }
        }
        return true;
      } else {
        accuracy = 0.0f; ;
        return false;
      }
    }

    float GetFloatAccuracy(PropertyDefinition property) {
      float accuracy;

      if (property.TryGetAttribute<AccuracyAttribute>(out var attr)) {
        var obj = attr.ConstructorArguments[0];

        // If the argument is a string, this is using a global Accuracy. Need to look up the value.
        if (obj.Value is string str) {
          accuracy = ILWeaverSettings.GetNamedFloatAccuracy(str);
        } else {
          var val = attr.ConstructorArguments[0].Value;
          if (val is float fval) {
            accuracy = fval; 
          } else if (val is double dval) {
            accuracy = (float)dval;
          } else {
            throw new Exception($"Invalid argument type: {val.GetType()}");
          }
        }
      } else {
        accuracy = AccuracyDefaults.DEFAULT_ACCURACY;
      }

      return accuracy;
    }

    TypeReference GetStaticArrayElementType(TypeReference type) {
      return ((GenericInstanceType)type).GenericArguments[0];
    }
    
    TypeReference GetStaticListElementType(TypeReference type) {
      return ((GenericInstanceType)type).GenericArguments[0];
    }
    
    TypeReference GetStaticDictionaryKeyType(TypeReference type) {
      return ((GenericInstanceType)type).GenericArguments[0];
    }
    
    TypeReference GetStaticDictionaryValType(TypeReference type) {
      return ((GenericInstanceType)type).GenericArguments[1];
    }

    void LoadArrayElementAddress(ILProcessor il, OpCode arrayOpCode, OpCode indexOpCode, int elementWordCount, int wordOffset = 0) {
      il.Append(Instruction.Create(arrayOpCode));
      il.Append(Instruction.Create(indexOpCode));
      il.Append(Instruction.Create(OpCodes.Ldc_I4, elementWordCount * Allocator.REPLICATE_WORD_SIZE));
      il.Append(Instruction.Create(OpCodes.Mul));

      if (wordOffset != 0) {
        il.Append(Instruction.Create(OpCodes.Ldc_I4, wordOffset * Allocator.REPLICATE_WORD_SIZE));
        il.Append(Instruction.Create(OpCodes.Add));
      }

      il.Append(Instruction.Create(OpCodes.Add));
    }

    string GetCacheName(string name) {
      return $"cache_<{name}>";
    }

    string GetInspectorFieldName(string name) {
      return $"_{name}";
    }

    void InjectPtrNullCheck(ILWeaverAssembly asm, ILProcessor il, PropertyDefinition property) {
      if (ILWeaverSettings.NullChecksForNetworkedProperties()) {
        var nop = Instruction.Create(OpCodes.Nop);

        il.Append(Instruction.Create(OpCodes.Ldarg_0));
        il.Append(Instruction.Create(OpCodes.Ldfld, asm.NetworkedBehaviour.GetField(PTR_FIELD_NAME)));
        il.Append(Instruction.Create(OpCodes.Ldc_I4_0));
        il.Append(Instruction.Create(OpCodes.Conv_U));
        il.Append(Instruction.Create(OpCodes.Ceq));
        il.Append(Instruction.Create(OpCodes.Brfalse, nop));

        var ctor = typeof(InvalidOperationException).GetConstructors().First(x => x.GetParameters().Length == 1);
        var exnCtor = asm.Import(ctor);

        il.Append(Instruction.Create(OpCodes.Ldstr, $"Error when accessing {property.DeclaringType.Name}.{property.Name}. Networked properties can only be accessed when Spawned() has been called."));
        il.Append(Instruction.Create(OpCodes.Newobj, exnCtor));
        il.Append(Instruction.Create(OpCodes.Throw));
        il.Append(nop);
      }
    }

    

    void InjectValueAccessor(ILWeaverAssembly asm, ILProcessor getIL, ILProcessor setIL, PropertyDefinition property, TypeReference type, OpCode valueOpCode, Action<ILProcessor, int> addressLoader, bool injectNullChecks) {
      if (injectNullChecks) {
        InjectPtrNullCheck(asm, getIL, property);

        if (setIL != null) {
          InjectPtrNullCheck(asm, setIL, property);
        }
      }

      // for pointer types we can simply just return the address we loaded on the stack
      if (type.IsPointer || type.IsByReference) {
        // load address
        addressLoader(getIL, 0);

        // return
        getIL.Append(Instruction.Create(OpCodes.Ret));

        // this has to be null
        Assert.Check(setIL == null);
      } else if (property.PropertyType.IsString()) {

        if (property.DeclaringType.IsValueType) {

          addressLoader(getIL, 0);
          getIL.Append(Instruction.Create(OpCodes.Call, asm.ReadWriteUtils.GetMethod(nameof(ReadWriteUtilsForWeaver.ReadStringUtf32NoHash), 1)));
          getIL.Append(Instruction.Create(OpCodes.Ret));

          addressLoader(setIL, 0);
          setIL.Append(Instruction.Create(OpCodes.Ldc_I4, GetStringCapacity(property)));
          setIL.Append(Instruction.Create(valueOpCode));
          setIL.Append(Instruction.Create(OpCodes.Call, asm.ReadWriteUtils.GetMethod(nameof(ReadWriteUtilsForWeaver.WriteStringUtf32NoHash), 3)));
          setIL.Append(Instruction.Create(OpCodes.Ret));

        } else {

          var cache = new FieldDefinition(GetCacheName(property.Name), FieldAttributes.Private, asm.CecilAssembly.MainModule.ImportReference(typeof(string)));

          property.DeclaringType.Fields.Add(cache);

          addressLoader(getIL, 0);
          getIL.Append(Instruction.Create(OpCodes.Ldarg_0));
          getIL.Append(Instruction.Create(OpCodes.Ldflda, cache));
          getIL.Append(Instruction.Create(OpCodes.Call, asm.ReadWriteUtils.GetMethod(nameof(ReadWriteUtilsForWeaver.ReadStringUtf32WithHash), 2)));
          getIL.Append(Instruction.Create(OpCodes.Ret));

          addressLoader(setIL, 0);
          setIL.Append(Instruction.Create(OpCodes.Ldc_I4, GetStringCapacity(property)));
          setIL.Append(Instruction.Create(valueOpCode));
          setIL.Append(Instruction.Create(OpCodes.Ldarg_0));
          setIL.Append(Instruction.Create(OpCodes.Ldflda, cache));
          setIL.Append(Instruction.Create(OpCodes.Call, asm.ReadWriteUtils.GetMethod(nameof(ReadWriteUtilsForWeaver.WriteStringUtf32WithHash), 4)));
          setIL.Append(Instruction.Create(OpCodes.Ret));
        }
      }

      // for primitive values
      else if (type.IsPrimitive) {
        // floats
        if (type.IsFloat()) {
          var accuracy = GetFloatAccuracy(property);

          // getter
          addressLoader(getIL, 0);

          if (accuracy == 0) {
            getIL.Append(Instruction.Create(OpCodes.Ldind_R4));
          } else {
            getIL.Append(Instruction.Create(OpCodes.Ldind_I4));
            getIL.Append(Instruction.Create(OpCodes.Conv_R4));
            getIL.Append(Instruction.Create(OpCodes.Ldc_R4, accuracy));
            getIL.Append(Instruction.Create(OpCodes.Mul));
          }

          getIL.Append(Instruction.Create(OpCodes.Ret));

          // setter
          addressLoader(setIL, 0);

          if (accuracy == 0) {
            setIL.Append(Instruction.Create(valueOpCode));
            setIL.Append(Instruction.Create(OpCodes.Stind_R4));
          } else {
            setIL.Append(Instruction.Create(OpCodes.Ldc_R4, 1f / accuracy));
            setIL.Append(Instruction.Create(valueOpCode));
            var write = asm.ReadWriteUtils.GetMethod("WriteFloat");
            setIL.Append(Instruction.Create(OpCodes.Call, write));
          }

          setIL.Append(Instruction.Create(OpCodes.Ret));
        } else if (type.IsBool()) {
          {
            // return *ptr == 0 ? false : true
            var load_0 = Ldc_I4(0);
            addressLoader(getIL, 0);
            getIL.Append(Ldind_I4());
            getIL.Append(Brfalse_S(load_0));
            getIL.Append(Ldc_I4(1));
            getIL.Append(Ret());
            getIL.Append(load_0);
            getIL.Append(Ret());
          }
          {
            // *ptr = value ? 1 : 0;
            var store = Stind_I4();
            var load_1 = Ldc_I4(1);
            addressLoader(setIL, 0);
            setIL.Append(Instruction.Create(valueOpCode));
            setIL.Append(Brtrue_S(load_1));
            setIL.Append(Ldc_I4(0));
            setIL.Append(Br_S(store));
            setIL.Append(load_1);
            setIL.Append(store);
            setIL.Append(Ret());
          }
        } else {
          // byte, sbyte, short, ushort, int, uint
          addressLoader(getIL, 0);
          getIL.Append(Ldind(type));
          getIL.Append(Instruction.Create(OpCodes.Ret));

          addressLoader(setIL, 0);
          setIL.Append(Instruction.Create(valueOpCode));
          setIL.Append(Stind(type));
          setIL.Append(Instruction.Create(OpCodes.Ret));
        }
      } else if (TryGetNetworkWrapperType(type, out var wrapInfo)) {

        FieldDefinition field = null;
        //if (wrapInfo.UnwrapByRef) {
        //  cache = new FieldDefinition(GetCacheName(property.Name), FieldAttributes.Private, wrapInfo.Type);
        //}

        // getter
        using (var ctx = new MethodContext(asm, getIL.Body.Method, addressGetter: (x) => addressLoader(x, 0))) {
          if (field != null) {
            property.DeclaringType.Fields.Add(field);
            WeaveNetworkUnwrap(asm, getIL, ctx, wrapInfo, type, previousValue: field);
          } else {
            WeaveNetworkUnwrap(asm, getIL, ctx, wrapInfo, type);
          }
          getIL.Append(Ret());
        }


        if (setIL != null) {
          // setter
          using (var ctx = new MethodContext(asm, setIL.Body.Method, addressGetter: (x) => addressLoader(x, 0))) {
            WeaveNetworkWrap(asm, setIL, ctx, il => il.Append(Instruction.Create(valueOpCode)), wrapInfo);
            //if (cache != null) {
            //  setIL.Append(Ldarg_0());
            //  setIL.Append(Ldarg_1());
            //  setIL.Append(Stfld(cache));
            //}
            setIL.Append(Ret());
          }
        }
      }
      // other value types
      else if (type.IsValueType) {
        var resolvedPropertyType = type.TryResolve();
        if (resolvedPropertyType == null) {
          throw new ILWeaverException($"Can't resolve type for property {property.FullName} with type {property.PropertyType}");
        }

        if (resolvedPropertyType.IsEnum) {
          addressLoader(getIL, 0);
          var underlayingEnumType = resolvedPropertyType.GetEnumUnderlyingType();
          getIL.Append(Ldind(underlayingEnumType));
          getIL.Append(Instruction.Create(OpCodes.Ret));

          addressLoader(setIL, 0);
          setIL.Append(Instruction.Create(valueOpCode));
          setIL.Append(Stind(underlayingEnumType));
          setIL.Append(Instruction.Create(OpCodes.Ret));
        } else if (type.IsQuaternion()) {
          var accuracy = GetFloatAccuracy(property);

          // getter
          var read = asm.ReadWriteUtils.GetMethod("ReadQuaternion");

          addressLoader(getIL, 0);
          getIL.Append(Instruction.Create(OpCodes.Ldc_R4, accuracy));
          getIL.Append(Instruction.Create(OpCodes.Call, read));
          getIL.Append(Instruction.Create(OpCodes.Ret));

          // setter
          var write = asm.ReadWriteUtils.GetMethod("WriteQuaternion");

          addressLoader(setIL, 0);
          setIL.Append(Instruction.Create(OpCodes.Ldc_R4, accuracy == 0 ? 0 : (1f / accuracy)));
          setIL.Append(Instruction.Create(valueOpCode));
          setIL.Append(Instruction.Create(OpCodes.Call, write));
          setIL.Append(Instruction.Create(OpCodes.Ret));
        } else if (type.IsVector2()) {
          var accuracy = GetFloatAccuracy(property);

          // getter
          var read = asm.ReadWriteUtils.GetMethod("ReadVector2");

          addressLoader(getIL, 0);
          getIL.Append(Instruction.Create(OpCodes.Ldc_R4, accuracy));
          getIL.Append(Instruction.Create(OpCodes.Call, read));
          getIL.Append(Instruction.Create(OpCodes.Ret));

          // setter
          var write = asm.ReadWriteUtils.GetMethod("WriteVector2");

          addressLoader(setIL, 0);
          setIL.Append(Instruction.Create(OpCodes.Ldc_R4, accuracy == 0 ? 0 : (1f / accuracy)));
          setIL.Append(Instruction.Create(valueOpCode));
          setIL.Append(Instruction.Create(OpCodes.Call, write));
          setIL.Append(Instruction.Create(OpCodes.Ret));
        } else if (type.IsVector3()) {
          var accuracy = GetFloatAccuracy(property);

          // getter
          var read = asm.ReadWriteUtils.GetMethod("ReadVector3");

          addressLoader(getIL, 0);
          getIL.Append(Instruction.Create(OpCodes.Ldc_R4, accuracy));
          getIL.Append(Instruction.Create(OpCodes.Call, read));
          getIL.Append(Instruction.Create(OpCodes.Ret));

          // setter
          var write = asm.ReadWriteUtils.GetMethod("WriteVector3");

          addressLoader(setIL, 0);
          setIL.Append(Instruction.Create(OpCodes.Ldc_R4, accuracy == 0 ? 0 : (1f / accuracy)));
          setIL.Append(Instruction.Create(valueOpCode));
          setIL.Append(Instruction.Create(OpCodes.Call, write));
          setIL.Append(Instruction.Create(OpCodes.Ret));
        } else if (type.IsNetworkDictionary(out var keyType, out var valType)) {

          if (setIL != null) {
            throw new ILWeaverException($"NetworkDictionary properties can't have setters.");
          }

          var capacity  = GetStaticDictionaryCapacity(property);
          
          var keyInfo = MakeElementReaderWriter(asm, property, keyType);
          var valInfo = MakeElementReaderWriter(asm, property, valType);

          // load address
          keyInfo.InitializeInstance(getIL);
          valInfo.InitializeInstance(getIL);
          addressLoader(getIL, 0);
          getIL.Append(Instruction.Create(OpCodes.Ldc_I4, capacity));
          keyInfo.LoadInstance(getIL);
          valInfo.LoadInstance(getIL);

          var ctor = asm.CecilAssembly.MainModule.ImportReference(type.Resolve().GetConstructors().First(x => x.Parameters.Count > 1));
          ctor.DeclaringType = ctor.DeclaringType.MakeGenericInstanceType(keyType, valType);

          getIL.Append(Instruction.Create(OpCodes.Newobj, ctor));
          getIL.Append(Instruction.Create(OpCodes.Ret));
          
        } else if (type.IsNetworkList(out var elementType)) {
          if (setIL != null) {
            throw new ILWeaverException($"NetworkList properties can't have setters.");
          }
          
          var listCapacity      = GetStaticListCapacity(property);
          
          var elementInfo = MakeElementReaderWriter(asm, property, elementType);

          elementInfo.InitializeInstance(getIL);
          addressLoader(getIL, 0);
          getIL.Append(Instruction.Create(OpCodes.Ldc_I4, listCapacity));
          elementInfo.LoadInstance(getIL);
          
          var ctor = asm.CecilAssembly.MainModule.ImportReference(type.Resolve().GetConstructors().First(x => x.Parameters.Count > 1));
          ctor.DeclaringType = ctor.DeclaringType.MakeGenericInstanceType(elementType);

          getIL.Append(Instruction.Create(OpCodes.Newobj, ctor));
          getIL.Append(Instruction.Create(OpCodes.Ret));
          
        } else if (type.IsNetworkArray(out elementType)) {
          if (setIL != null) {
            throw new ILWeaverException($"NetworkArray properties can't have setters.");
          }

          var arrayLength = GetStaticArrayCapacity(property);
          var elementInfo = MakeElementReaderWriter(asm, property, elementType);

          // load address
          elementInfo.InitializeInstance(getIL);
          addressLoader(getIL, 0);
          getIL.Append(Instruction.Create(OpCodes.Ldc_I4, arrayLength));
          elementInfo.LoadInstance(getIL);

          var ctor = asm.CecilAssembly.MainModule.ImportReference(type.Resolve().GetConstructors().First(x => x.Parameters.Count > 1));
          ctor.DeclaringType = ctor.DeclaringType.MakeGenericInstanceType(elementType);

          getIL.Append(Instruction.Create(OpCodes.Newobj, ctor));
          getIL.Append(Instruction.Create(OpCodes.Ret));
        } else {
          addressLoader(getIL, 0);
          getIL.Append(Instruction.Create(OpCodes.Ldobj, type));
          getIL.Append(Instruction.Create(OpCodes.Ret));

          addressLoader(setIL, 0);
          setIL.Append(Instruction.Create(valueOpCode));
          setIL.Append(Instruction.Create(OpCodes.Stobj, type));
          setIL.Append(Instruction.Create(OpCodes.Ret));
        }
      }
    }

    void RemoveBackingField(PropertyDefinition property) {
      var backing = FindBackingField(property.DeclaringType, property.Name);
      if (backing != null) {
        property.DeclaringType.Fields.Remove(backing);
      }
    }

    (MethodDefinition getter, MethodDefinition setter) PreparePropertyForWeaving(PropertyDefinition property) {
      var getter = property.GetMethod;
      var setter = property.SetMethod;

      // clear getter
      getter.CustomAttributes.Clear();
      getter.Body.Instructions.Clear();

      // clear setter if it exists
      setter?.CustomAttributes?.Clear();
      setter?.Body?.Instructions?.Clear();

      return (getter, setter);
    }

    bool IsWeavableProperty(PropertyDefinition property) {
      return IsWeavableProperty(property, out _);
    }

    struct WeavablePropertyMeta {
      public string DefaultFieldName;
      public FieldDefinition BackingField;
      public bool ReatainIL;
      public string OnChanged;
    } 

    bool IsWeavableProperty(PropertyDefinition property, out WeavablePropertyMeta meta) {
      if (property.TryGetAttribute<NetworkedAttribute>(out var attr) == false) {
        meta = default;
        return false;
      }

      string onChanged;
      attr.TryGetAttributeProperty(nameof(NetworkedAttribute.OnChanged), out onChanged);

      // check getter ... it has to exist
      var getter = property.GetMethod;
      if (getter == null) {
        meta = default;
        return false;
      }

      // check setter ...
      var setter = property.SetMethod;
      if (setter == null) {
        // if it doesn't exist we allow either array or pointer
        if (property.PropertyType.IsByReference == false && property.PropertyType.IsPointer == false && property.PropertyType.IsNetworkArray() == false && property.PropertyType.IsNetworkDictionary() == false && property.PropertyType.IsNetworkList() == false) {
          throw new ILWeaverException($"Simple properties need a setter.");
        }
      }

      // check for backing field ...
      var backing = FindBackingField(property.DeclaringType, property.Name);
      if (backing == null) {
        var il = attr.Properties.FirstOrDefault(x => x.Name == "RetainIL");

        if (il.Argument.Value is bool retainIL && retainIL) {
          meta = new WeavablePropertyMeta() {
            ReatainIL = true,
            OnChanged = onChanged,
          };
          return true;
        }
      }

      meta = new WeavablePropertyMeta() {
        BackingField = backing,
        ReatainIL = false,
        OnChanged = onChanged,
      };

      attr.TryGetAttributeProperty(nameof(NetworkedAttribute.Default), out meta.DefaultFieldName);
      

      return true;
    }

    bool IsRpcCompatibleType(TypeReference property) {
      if (property.IsPointer) {
        return false;
      }

      if (property.IsNetworkArray() || property.IsNetworkList() || property.IsNetworkDictionary()) {
        return false;
      }

      if (property.IsString()) {
        return true;
      }

      if (property.IsValueType) {
        return true;
      }

      if (TryGetNetworkWrapperType(property, out var wrapInfo)) {
        if (wrapInfo.MaxRawByteCount > 0) {
          return true;
        } else {
          return IsRpcCompatibleType(wrapInfo.WrapperType);
        }
      }

      return false;
    }

    void WeaveInput(ILWeaverAssembly asm, TypeDefinition type) {
      if (type.TryGetAttribute<NetworkInputWeavedAttribute>(out var attribute)) {
        if (_typeData.ContainsKey(type.FullName) == false) {
          _typeData.Add(type.FullName, new TypeMetaData {
            WordCount = (int)attribute.ConstructorArguments[0].Value,
            Definition = type,
            Reference = type
          });
        }

        return;
      }

      using (Log.ScopeInput(type)) {

        int wordCount = WeaveStructInner(asm, type);

        // add new attribute
        type.AddAttribute<NetworkInputWeavedAttribute, int>(asm, wordCount);

        // track type data
        _typeData.Add(type.FullName, new TypeMetaData {
          WordCount = wordCount,
          Definition = type,
          Reference = type
        });
      }
    }

    

    MethodReference FindMethodInParent(ILWeaverAssembly asm, TypeDefinition type, string name, string stopAtType = null, int? argCount = null) {
      type = type.BaseType.Resolve();

      while (type != null) {
        if (type.Name == stopAtType || type.FullName == "System.Object") {
          return null;
        }

        var method = type.Methods.FirstOrDefault(x => x.Name == name && ((argCount == null) || (x.Parameters.Count == argCount.Value)));
        if (method != null) {
          return asm.CecilAssembly.MainModule.ImportReference(method);
        }

        type = type.BaseType.Resolve();
      }

      return null;
    }

    string InvokerMethodName(string method, Dictionary<string, int> nameCache) {
      nameCache.TryGetValue(method, out var count);
      nameCache[method] = ++count;
      return $"{method}@Invoker{(count == 1 ? "" : count.ToString())}";
    }

    bool HasRpcPrefixOrSuffix(MethodDefinition def) {
      return def.Name.StartsWith("rpc", StringComparison.OrdinalIgnoreCase) || def.Name.EndsWith("rpc", StringComparison.OrdinalIgnoreCase);
    }

    void WeaveRpcs(ILWeaverAssembly asm, TypeDefinition type, bool allowInstanceRpcs = true) {
      if (type.HasGenericParameters) {
        return;
      }

      // rpc list
      var rpcs = new List<(MethodDefinition, CustomAttribute)>();


      bool hasStaticRpc = false;

      foreach (var rpc in type.Methods) {
        if (rpc.TryGetAttribute<RpcAttribute>(out var attr)) {

          if (HasRpcPrefixOrSuffix(rpc) == false) {
            Log.Warn($"{rpc.DeclaringType}.{rpc.Name} name does not start or end with the \"Rpc\" prefix or suffix. Starting from Beta this will result in an error.");
          }

          if (rpc.IsStatic && rpc.Parameters.FirstOrDefault()?.ParameterType.FullName != asm.NetworkRunner.Reference.FullName) {
            throw new ILWeaverException($"{rpc}: Static RPC needs {nameof(NetworkRunner)} as the first parameter");
          }

          hasStaticRpc |= rpc.IsStatic;

          if (!allowInstanceRpcs && !rpc.IsStatic) {
            throw new ILWeaverException($"{rpc}: Instance RPCs not allowed for this type");
          }

          foreach (var parameter in rpc.Parameters) {
            if (rpc.IsStatic && parameter == rpc.Parameters[0]) {
              continue;
            }

            if (IsInvokeOnlyParameter(parameter)) {
              continue;
            }

            var parameterType = parameter.ParameterType.IsArray ? parameter.ParameterType.GetElementType() : parameter.ParameterType;

            if (IsRpcCompatibleType(parameterType) == false) {
              throw new ILWeaverException($"{rpc}: parameter {parameter.Name} is not Rpc-compatible.");
            }
          }

          if (!rpc.ReturnType.Is(asm.RpcInvokeInfo) && !rpc.ReturnType.IsVoid()) {
            throw new ILWeaverException($"{rpc}: RPCs can't return a value.");
          }

          rpcs.Add((rpc, attr));
        }
      }

      if (!rpcs.Any()) {
        return;
      }

      int instanceRpcKeys = GetInstanceRpcCount(type.BaseType);

      Dictionary<string, int> invokerNameCounter = new Dictionary<string, int>();

      foreach (var (rpc, attr) in rpcs) {
        int sources;
        int targets;

        if (attr.ConstructorArguments.Count == 2) {
          sources = attr.GetAttributeArgument<int>(0);
          targets = attr.GetAttributeArgument<int>(1);
        } else {
          sources = AuthorityMasks.ALL;
          targets = AuthorityMasks.ALL;
        }

        ParameterDefinition rpcTargetParameter = rpc.Parameters.SingleOrDefault(x => x.HasAttribute<RpcTargetAttribute>());
        if (rpcTargetParameter != null && !rpcTargetParameter.ParameterType.Is<PlayerRef>()) {
          throw new ILWeaverException($"{rpcTargetParameter}: {nameof(RpcTargetAttribute)} can only be used for {nameof(PlayerRef)} type argument");
        }

        attr.TryGetAttributeProperty<bool>(nameof(RpcAttribute.InvokeLocal), out var invokeLocal, defaultValue: true);
        attr.TryGetAttributeProperty<bool>(nameof(RpcAttribute.InvokeResim), out var invokeResim);
        attr.TryGetAttributeProperty<RpcChannel>(nameof(RpcAttribute.Channel), out var channel);
        attr.TryGetAttributeProperty<bool>(nameof(RpcAttribute.TickAligned), out var tickAligned, defaultValue: true);
        attr.TryGetAttributeProperty<RpcHostMode>(nameof(RpcAttribute.HostMode), out var hostMode);

        // rpc key
        int instanceRpcKey = -1;
        var returnsRpcInvokeInfo = rpc.ReturnType.Is(asm.RpcInvokeInfo);



        using (var ctx = new RpcMethodContext(asm, rpc, rpc.IsStatic)) {

          // local variables
          ctx.DataVariable = new VariableDefinition(asm.Import(typeof(byte)).MakePointerType());
          ctx.OffsetVariable = new VariableDefinition(asm.Import(typeof(int)));
          var message = new VariableDefinition(asm.SimulationMessage.Reference.MakePointerType());
          VariableDefinition localAuthorityMask = null;

          rpc.Body.Variables.Add(ctx.DataVariable);
          rpc.Body.Variables.Add(ctx.OffsetVariable);
          rpc.Body.Variables.Add(message);
          rpc.Body.InitLocals = true;

          // get il processes and our jump instruction
          var il = rpc.Body.GetILProcessor();
          var jmp = Nop();
          var inv = Nop();
          var prepareInv = Nop();

          Instruction targetedInvokeLocal = null;


          // instructions for our branch
          var ins = new List<Instruction>();

          if (returnsRpcInvokeInfo) {
            // find local variable that's used for return(default);
            ctx.RpcInvokeInfoVariable = new VariableDefinition(asm.RpcInvokeInfo);
            rpc.Body.Variables.Add(ctx.RpcInvokeInfoVariable);
            ins.Add(Ldloca(ctx.RpcInvokeInfoVariable));
            ins.Add(Initobj(ctx.RpcInvokeInfoVariable.VariableType));

            // fix each ret
            var instructions = il.Body.Instructions;
            for (int i = 0; i < instructions.Count; ++i) {
              var instruction = instructions[i];
              if (instruction.OpCode == OpCodes.Ret) {
                if (instructions[i - 1].IsLdlocWithIndex(out _)) {
                  // replace indexed load
                  instructions[i - 1].OpCode = OpCodes.Ldloc;
                  instructions[i - 1].Operand = ctx.RpcInvokeInfoVariable;
                } else if (instructions[i - 1].OpCode == OpCodes.Ldloc) {
                  // replace named load
                  instructions[i - 1].Operand = ctx.RpcInvokeInfoVariable;
                } else {
                  throw new ILWeaverException($"{rpc}: return pattern of {nameof(RpcInvokeInfo)} not recognised at {instruction}. Context: {string.Join("\n", instructions)}");
                }
              }
            }
          }

          if (rpc.IsStatic) {
            ins.Add(Ldsfld(asm.NetworkBehaviourUtils.GetField(nameof(NetworkBehaviourUtils.InvokeRpc))));
            ins.Add(Brfalse(jmp));
            ins.Add(Ldc_I4(0));
            ins.Add(Stsfld(asm.NetworkBehaviourUtils.GetField(nameof(NetworkBehaviourUtils.InvokeRpc))));
          } else {
            ins.Add(Ldarg_0());
            ins.Add(Ldfld(asm.NetworkedBehaviour.GetField(nameof(NetworkBehaviour.InvokeRpc))));
            ins.Add(Brfalse(jmp));
            ins.Add(Ldarg_0());
            ins.Add(Ldc_I4(0));
            ins.Add(Stfld(asm.NetworkedBehaviour.GetField(nameof(NetworkBehaviour.InvokeRpc))));
          }
          ins.Add(inv);


          // insert instruction into method body
          var prev = rpc.Body.Instructions[0]; //.OpCode == OpCodes.Nop ? rpc.Body.Instructions[1] :  rpc.Body.Instructions[0];

          for (int i = ins.Count - 1; i >= 0; --i) {
            il.InsertBefore(prev, ins[i]);
            prev = ins[i];
          }

          // jump target
          il.Append(jmp);

          

          var returnInstructions = returnsRpcInvokeInfo
            ? new[] { Ldloc(ctx.RpcInvokeInfoVariable), Ret() }
            : new[] { Ret() };

          var ret = returnInstructions.First();

          // check if runner's ok
          if (rpc.IsStatic) {
            il.AppendMacro(ctx.LoadRunner());
            var checkDone = Nop();
            il.Append(Brtrue_S(checkDone));
            il.Append(Ldstr(rpc.Parameters[0].Name));
            il.Append(Newobj(typeof(ArgumentNullException).GetConstructor(asm, 1)));
            il.Append(Throw());
            il.Append(checkDone);
          } else {
            il.Append(Ldarg_0());
            il.Append(Call(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized))));
          }

          il.AppendMacro(ctx.SetRpcInvokeInfoStatus(!invokeLocal, RpcLocalInvokeResult.NotInvokableLocally));

          // if we shouldn't invoke during resim
          if (invokeResim == false) {
            var checkDone = Nop();

            il.AppendMacro(ctx.LoadRunner());

            il.Append(Call(asm.NetworkRunner.GetProperty("Stage")));
            il.Append(Ldc_I4((int)SimulationStages.Resimulate));
            il.Append(Bne_Un_S(checkDone));

            il.AppendMacro(ctx.SetRpcInvokeInfoStatus(invokeLocal, RpcLocalInvokeResult.NotInvokableDuringResim));
            il.AppendMacro(ctx.SetRpcInvokeInfoStatus(RpcSendCullResult.NotInvokableDuringResim));
            il.Append(Br(ret));

            il.Append(checkDone);
          }

          if (!rpc.IsStatic) {
            localAuthorityMask = new VariableDefinition(asm.Import(typeof(int)));
            rpc.Body.Variables.Add(localAuthorityMask);
            il.Append(Ldarg_0());
            il.Append(Ldfld(asm.NetworkedBehaviour.GetField(OBJECT_FIELD_NAME)));
            il.Append(Call(asm.NetworkedObject.GetMethod(nameof(NetworkObject.GetLocalAuthorityMask))));
            il.Append(Stloc(localAuthorityMask));
          }

          // check if target is reachable or not
          if (rpcTargetParameter != null) {
            il.AppendMacro(ctx.LoadRunner());

            il.Append(Ldarg(rpcTargetParameter));
            il.Append(Call(asm.NetworkRunner.GetMethod(nameof(NetworkRunner.GetRpcTargetStatus))));
            il.Append(Dup());

            // check for being unreachable
            {
              var done = Nop();
              il.Append(Ldc_I4((int)RpcTargetStatus.Unreachable));
              il.Append(Bne_Un_S(done));
              il.Append(Ldarg(rpcTargetParameter));
              il.Append(Ldstr(rpc.ToString()));
              il.Append(Call(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.NotifyRpcTargetUnreachable))));
              il.Append(Pop()); // pop the GetRpcTargetStatus

              il.AppendMacro(ctx.SetRpcInvokeInfoStatus(invokeLocal, RpcLocalInvokeResult.TagetPlayerIsNotLocal));
              il.AppendMacro(ctx.SetRpcInvokeInfoStatus(RpcSendCullResult.TargetPlayerUnreachable));
              il.Append(Br(ret));

              il.Append(done);
            }

            // check for self
            {
              il.Append(Ldc_I4((int)RpcTargetStatus.Self));
              if (invokeLocal) {
                // straight to the invoke; this will prohibit any sending
                Log.Assert(targetedInvokeLocal == null);
                targetedInvokeLocal = Nop();
                il.Append(Beq(targetedInvokeLocal));
                il.AppendMacro(ctx.SetRpcInvokeInfoStatus(true, RpcLocalInvokeResult.TagetPlayerIsNotLocal));
              } else {
                // will never get called
                var checkDone = Nop();
                il.Append(Bne_Un_S(checkDone));

                if (NetworkRunner.BuildType == NetworkRunner.BuildTypes.Debug) {
                  il.Append(Ldarg(rpcTargetParameter));
                  il.Append(Ldstr(rpc.ToString()));
                  il.Append(Call(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.NotifyLocalTargetedRpcCulled))));
                }

                il.AppendMacro(ctx.SetRpcInvokeInfoStatus(RpcSendCullResult.TargetPlayerIsLocalButRpcIsNotInvokableLocally));
                il.Append(Br(ret));

                il.Append(checkDone);
              }
            }
          }

          // check if sender flags make sense
          if (!rpc.IsStatic) {
            var checkDone = Nop();

            il.Append(Ldloc(localAuthorityMask));
            il.Append(Ldc_I4(sources));
            il.Append(And());
            il.Append(Brtrue_S(checkDone));

            // source is not valid, notify
            il.Append(Ldstr(rpc.ToString()));
            il.Append(Ldarg_0());
            il.Append(Ldfld(asm.NetworkedBehaviour.GetField(OBJECT_FIELD_NAME)));
            il.Append(Ldc_I4(sources));
            il.Append(Call(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc))));

            il.AppendMacro(ctx.SetRpcInvokeInfoStatus(invokeLocal, RpcLocalInvokeResult.InsufficientSourceAuthority));
            il.AppendMacro(ctx.SetRpcInvokeInfoStatus(RpcSendCullResult.InsufficientSourceAuthority));

            il.Append(Br(ret));

            il.Append(checkDone);

            if (invokeLocal) {
              // how about the target? does it match only the local client?
              if (targets != 0 && (targets & AuthorityMasks.PROXY) == 0) {
                il.Append(Ldloc(localAuthorityMask));
                il.Append(Ldc_I4(targets));
                il.Append(And());
                il.Append(Ldc_I4(targets));
                il.Append(Beq(prepareInv));
              }
            }
          }

          // check if sending makes sense at all
          var afterSend = Nop();

          // if not targeted (already handled earlier) check if it can be sent at all
          if (rpcTargetParameter == null) {
            var checkDone = Nop();
            il.AppendMacro(ctx.LoadRunner());
            il.Append(Call(asm.NetworkRunner.GetMethod(nameof(NetworkRunner.HasAnyActiveConnections))));
            il.Append(Brtrue(checkDone));
            il.AppendMacro(ctx.SetRpcInvokeInfoStatus(RpcSendCullResult.NoActiveConnections));
            il.Append(Br(afterSend));
            il.Append(checkDone);
          }

          // create simulation message
          il.AppendMacro(ctx.LoadRunner());
          il.Append(Call(asm.NetworkRunner.GetProperty(nameof(NetworkRunner.Simulation))));
          il.Append(Ldc_I4(RpcHeader.SIZE));

          for (int i = 0; i < rpc.Parameters.Count; ++i) {
            var para = rpc.Parameters[i];

            if (rpc.IsStatic && i == 0) {
              Log.Assert(para.ParameterType.IsSame<NetworkRunner>());
              continue;
            }

            if (IsInvokeOnlyParameter(para)) {
              continue;
            }
            if (para == rpcTargetParameter) {
              continue;
            }

            if (para.ParameterType.IsString()) {
              il.Append(Ldarg(para));
              il.Append(Call(asm.Native.GetMethod(nameof(Native.GetLengthPrefixedUTF8ByteCount))));
              il.AppendMacro(AlignToWordSize());
            } else if (para.ParameterType.IsArray) {
              WeaveArrayByteSize(asm, il, para);
            } else {
              var size = GetByteCount(asm, para.ParameterType);
              il.Append(Ldc_I4(size));
            }
            il.Append(Add());
          }

          il.Append(Call(asm.SimulationMessage.GetMethod(nameof(SimulationMessage.Allocate), 2)));
          il.Append(Stloc(message));

          // get data for messages
          il.Append(Ldloc(message));
          il.Append(Call(asm.SimulationMessage.GetMethod(nameof(SimulationMessage.GetData), 1)));
          il.Append(Stloc(ctx.DataVariable));

          // create RpcHeader
          // il.Append(Ldloc(data));

          if (rpc.IsStatic) {
            il.Append(Ldstr(rpc.ToString()));
            il.Append(Call(asm.Import(typeof(NetworkBehaviourUtils).GetMethod(nameof(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow)))));
            il.Append(Call(asm.RpcHeader.GetMethod(nameof(RpcHeader.Create), 1)));
          } else {
            il.Append(Ldarg_0());
            il.Append(Ldfld(asm.NetworkedBehaviour.GetField(OBJECT_FIELD_NAME)));
            il.Append(Ldfld(asm.NetworkedObject.GetField(nameof(NetworkObject.Id))));

            il.Append(Ldarg_0());
            il.Append(Ldfld(asm.NetworkedBehaviour.GetField(nameof(NetworkBehaviour.ObjectIndex))));

            instanceRpcKey = ++instanceRpcKeys;
            il.Append(Ldc_I4(instanceRpcKey));
            il.Append(Call(asm.RpcHeader.GetMethod(nameof(RpcHeader.Create), 3)));
          }

          il.Append(Ldloc(ctx.DataVariable));
          il.Append(Call(asm.RpcHeader.GetMethod(nameof(RpcHeader.Write))));
          il.AppendMacro(AlignToWordSize());
          //il.Append(Stobj(asm.RpcHeader.Reference));

          //il.Append(Ldc_I4(RpcHeader.SIZE));
          il.Append(Stloc(ctx.OffsetVariable));

          // write parameters
          for (int i = 0; i < rpc.Parameters.Count; ++i) {
            var para = rpc.Parameters[i];

            if (rpc.IsStatic && i == 0) {
              continue;
            }
            if (IsInvokeOnlyParameter(para)) {
              continue;
            }
            if (para == rpcTargetParameter) {
              continue;
            }
            if (para.ParameterType.IsString()) {
              using (ctx.AddOffsetScope(il)) { 
                il.AppendMacro(ctx.LoadAddress());
                il.Append(Ldarg(para));
                il.Append(Call(asm.Native.GetMethod(nameof(Native.WriteLengthPrefixedUTF8))));
                il.AppendMacro(AlignToWordSize());
              };
            } else if (para.ParameterType.IsArray) {
              WeaveRpcArrayInput(asm, ctx, il, para);
            } else {
              if (!para.ParameterType.IsPrimitive && TryGetNetworkWrapperType(para.ParameterType, out var wrapInfo)) {
                WeaveNetworkWrap(asm, il, ctx, il => il.Append(Ldarg(para)), wrapInfo);
              } else {
                il.AppendMacro(ctx.LoadAddress());
                il.Append(Ldarg(para));
                il.Append(Stind_or_Stobj(para.ParameterType));
                il.AppendMacro(ctx.AddOffset(GetByteCount(asm, para.ParameterType)));
              }
            }
          }

          // update message offset
          il.Append(Ldloc(message));
          il.Append(Ldflda(asm.SimulationMessage.GetField(nameof(SimulationMessage.Offset))));
          il.Append(Ldloc(ctx.OffsetVariable));
          il.Append(Ldc_I4(8));
          il.Append(Mul());
          il.Append(Stind_I4());

          // send message

          il.AppendMacro(ctx.LoadRunner());

          if (rpcTargetParameter != null) {
            il.Append(Ldloc(message));
            il.Append(Ldarg(rpcTargetParameter));
            il.Append(Call(asm.SimulationMessage.GetMethod(nameof(SimulationMessage.SetTarget))));
          }

          if (channel == RpcChannel.Unreliable) {
            il.Append(Ldloc(message));
            il.Append(Call(asm.SimulationMessage.GetMethod(nameof(SimulationMessage.SetUnreliable))));
          }

          if (!tickAligned) {
            il.Append(Ldloc(message));
            il.Append(Call(asm.SimulationMessage.GetMethod(nameof(SimulationMessage.SetNotTickAligned))));
          }

          if (rpc.IsStatic) {
            il.Append(Ldloc(message));
            il.Append(Call(asm.SimulationMessage.GetMethod(nameof(SimulationMessage.SetStatic))));
          }

          // send the rpc
          il.Append(Ldloc(message));
          
          if (ctx.RpcInvokeInfoVariable != null) {
            il.Append(Ldloca(ctx.RpcInvokeInfoVariable));
            il.Append(Ldflda(asm.RpcInvokeInfo.GetField(nameof(RpcInvokeInfo.SendResult))));
            il.Append(Call(asm.NetworkRunner.GetMethod(nameof(NetworkRunner.SendRpc), 2)));
          } else {
            il.Append(Call(asm.NetworkRunner.GetMethod(nameof(NetworkRunner.SendRpc), 1)));
          }

          il.AppendMacro(ctx.SetRpcInvokeInfoStatus(RpcSendCullResult.NotCulled));

          il.Append(afterSend);

          // .. hmm
          if (invokeLocal) {

            if (targetedInvokeLocal != null) {
              il.Append(Br(ret));
              il.Append(targetedInvokeLocal);
            }

            if (!rpc.IsStatic) {
              var checkDone = Nop();
              il.Append(Ldloc(localAuthorityMask));
              il.Append(Ldc_I4(targets));
              il.Append(And());
              il.Append(Brtrue_S(checkDone));

              il.AppendMacro(ctx.SetRpcInvokeInfoStatus(true, RpcLocalInvokeResult.InsufficientTargetAuthority));

              il.Append(Br(ret));

              il.Append(checkDone);
            }

            il.Append(prepareInv);

            foreach (var param in rpc.Parameters) {
              if (param.ParameterType.IsSame<RpcInfo>()) {
                // need to fill it now
                il.AppendMacro(ctx.LoadRunner());
                il.Append(Ldc_I4((int)channel));
                il.Append(Ldc_I4((int)hostMode));
                il.Append(Call(asm.RpcInfo.GetMethod(nameof(RpcInfo.FromLocal))));
                il.Append(Starg_S(param));
              }
            }

            il.AppendMacro(ctx.SetRpcInvokeInfoStatus(true, RpcLocalInvokeResult.Invoked));

            // invoke
            il.Append(Br(inv));
          }

          foreach (var instruction in returnInstructions) {
            il.Append(instruction);
          }
        }

        var invoker = new MethodDefinition(InvokerMethodName(rpc.Name, invokerNameCounter), MethodAttributes.Family | MethodAttributes.Static, asm.Import(typeof(void)));
        using (var ctx = new RpcMethodContext(asm, invoker, rpc.IsStatic)) {

          // create invoker delegate
          if (rpc.IsStatic) {
            var runner = new ParameterDefinition("runner", ParameterAttributes.None, asm.NetworkRunner.Reference);
            invoker.Parameters.Add(runner);
          } else {
            var behaviour = new ParameterDefinition("behaviour", ParameterAttributes.None, asm.NetworkedBehaviour.Reference);
            invoker.Parameters.Add(behaviour);
          }
          var message = new ParameterDefinition("message", ParameterAttributes.None, asm.SimulationMessage.Reference.MakePointerType());
          invoker.Parameters.Add(message);

          // add attribute
          if (rpc.IsStatic) {
            Log.Assert(instanceRpcKey < 0);
            invoker.AddAttribute<NetworkRpcStaticWeavedInvokerAttribute, string>(asm, rpc.ToString());
          } else {
            Log.Assert(instanceRpcKey >= 0);
            invoker.AddAttribute<NetworkRpcWeavedInvokerAttribute, int, int, int>(asm, instanceRpcKey, sources, targets);
          }

          // put on type
          type.Methods.Add(invoker);

          // local variables
          ctx.DataVariable = new VariableDefinition(asm.Import(typeof(byte)).MakePointerType());
          ctx.OffsetVariable = new VariableDefinition(asm.Import(typeof(int)));
          var parameters = new VariableDefinition[rpc.Parameters.Count];

          for (int i = 0; i < parameters.Length; ++i) {
            invoker.Body.Variables.Add(parameters[i] = new VariableDefinition(rpc.Parameters[i].ParameterType));
          }

          invoker.Body.Variables.Add(ctx.DataVariable);
          invoker.Body.Variables.Add(ctx.OffsetVariable);
          invoker.Body.InitLocals = true;

          var il = invoker.Body.GetILProcessor();

          // grab data from message and store in local
          il.Append(Ldarg_1());
          il.Append(Call(asm.SimulationMessage.GetMethod(nameof(SimulationMessage.GetData), 1)));
          il.Append(Stloc(ctx.DataVariable));

          il.Append(Ldloc(ctx.DataVariable));
          il.Append(Call(asm.RpcHeader.GetMethod(nameof(RpcHeader.ReadSize))));
          il.AppendMacro(AlignToWordSize());
          il.Append(Stloc(ctx.OffsetVariable));

          for (int i = 0; i < parameters.Length; ++i) {
            var para = parameters[i];

            if (rpc.IsStatic && i == 0) {
              il.Append(Ldarg_0());
              il.Append(Stloc(para));
              continue;
            }

            if (rpcTargetParameter == rpc.Parameters[i]) {
              il.Append(Ldarg_1());
              il.Append(Ldfld(asm.SimulationMessage.GetField(nameof(SimulationMessage.Target))));
              il.Append(Stloc(para));
            } else if (para.VariableType.IsString()) {

              using (ctx.AddOffsetScope(il)) { 
                il.AppendMacro(ctx.LoadAddress());
                il.Append(Ldloca(para));
                il.Append(Call(asm.Native.GetMethod(nameof(Native.ReadLengthPrefixedUTF8))));
                il.AppendMacro(AlignToWordSize());
              };

            } else if (para.VariableType.IsSame<RpcInfo>()) {
              il.AppendMacro(ctx.LoadRunner());
              il.Append(Ldarg_1());
              il.Append(Ldc_I4((int)hostMode));
              il.Append(Call(asm.RpcInfo.GetMethod(nameof(RpcInfo.FromMessage))));
              il.Append(Stloc(para));

            } else if (para.VariableType.IsArray) {
              WeaveRpcArrayInvoke(asm, ctx, il, para);

            } else {
              if (!para.VariableType.IsPrimitive && TryGetNetworkWrapperType(para.VariableType, out var wrapInfo)) {
                WeaveNetworkUnwrap(asm, il, ctx, wrapInfo, para.VariableType, storeResult: il => il.Append(Stloc(para)));
              } else {
                il.AppendMacro(ctx.LoadAddress());
                il.Append(Ldind_or_Ldobj(para.VariableType));
                il.Append(Stloc(para));
                il.AppendMacro(ctx.AddOffset(GetByteCount(asm, para.VariableType)));
              }
            }
          }

          if (rpc.IsStatic) {
            il.Append(Ldc_I4(1));
            il.Append(Stsfld(asm.NetworkBehaviourUtils.GetField(nameof(NetworkBehaviour.InvokeRpc))));
          } else {
            il.Append(Ldarg_0());
            il.Append(Ldc_I4(1));
            il.Append(Stfld(asm.NetworkedBehaviour.GetField(nameof(NetworkBehaviour.InvokeRpc))));
          }

          if (!rpc.IsStatic) {
            il.Append(Ldarg_0());
            il.Append(Instruction.Create(OpCodes.Castclass, rpc.DeclaringType));
          }

          for (int i = 0; i < parameters.Length; ++i) {
            il.Append(Ldloc(parameters[i]));
          }
          il.Append(Call(rpc));
          if (returnsRpcInvokeInfo) {
            il.Append(Pop());
          }
          il.Append(Ret());
        }
      }

      {
        Log.Assert(_rpcCount.TryGetValue(type.FullName, out int count) == false || count == instanceRpcKeys);
        _rpcCount[type.FullName] = instanceRpcKeys;
      }
    }

    private int GetInstanceRpcCount(TypeReference type) {
      if (_rpcCount.TryGetValue(type.FullName, out int result)) {
        return result;
      }

      result = 0;

      var typeDef = type.TryResolve();
      if (typeDef != null) {

        if (typeDef.BaseType != null) {
          result += GetInstanceRpcCount(typeDef.BaseType);
        }

        result += typeDef.GetMethods()
          .Where(x => !x.IsStatic)
          .Where(x => x.HasAttribute<RpcAttribute>())
          .Count();
      }

      _rpcCount.Add(type.FullName, result);
      return result;
    }

    private bool IsInvokeOnlyParameter(ParameterDefinition para) {
      if (para.ParameterType.IsSame<RpcInfo>()) {
        return true;
      }
      return false;
    }



    void WeaveArrayByteSize(ILWeaverAssembly asm, ILProcessor il, ParameterDefinition para) {

      int size;

      var elementType = para.ParameterType.GetElementType();
      if (elementType.IsPrimitive) {
        size = elementType.GetPrimitiveSize();
      } else if (TryGetNetworkWrapperType(elementType, out var wrapInfo)) {
        size = GetByteCount(asm, wrapInfo);
      } else {
        size = GetByteCount(asm, elementType);
      }

      // array length
      il.Append(Ldarg(para));
      il.Append(Ldlen());
      il.Append(Conv_I4());

      il.Append(Ldc_I4(size));
      il.Append(Mul());

      if (elementType.IsPrimitive && (size % Allocator.REPLICATE_WORD_SIZE) != 0) {
        // need to align to word count boundaries
        il.AppendMacro(AlignToWordSize());
      }

      // store the length
      il.Append(Ldc_I4(sizeof(Int32)));
      il.Append(Add());
    }

    void WeaveRpcArrayInvoke(ILWeaverAssembly asm, RpcMethodContext ctx, ILProcessor il, VariableDefinition para) {
      var elementType = para.VariableType.GetElementType();

      var intType = asm.Import(typeof(Int32));


      // alloc result array
      il.AppendMacro(ctx.LoadAddress());
      il.Append(Ldind(intType));
      il.Append(Instruction.Create(OpCodes.Newarr, elementType));
      il.Append(Stloc(para));

      il.AppendMacro(ctx.AddOffset(sizeof(Int32)));

      if (elementType.IsPrimitive) {
        using (ctx.AddOffsetScope(il)) { 
          // dst
          il.Append(Ldloc(para));

          // src
          il.AppendMacro(ctx.LoadAddress());

          var memCpy = new GenericInstanceMethod(asm.Native.GetMethod(nameof(Native.CopyToArray), 2));
          memCpy.GenericArguments.Add(elementType);
          il.Append(Call(memCpy));

          if (elementType.IsPrimitive && (elementType.GetPrimitiveSize() % Allocator.REPLICATE_WORD_SIZE) != 0) {
            // need to align to word count boundaries
            il.AppendMacro(AlignToWordSize());
          }
        };
      } else {

        var arrayIndex = ctx.GetOrCreateVariable("ArrayIndex", asm.Import(typeof(int)));
        var exitCondition = Ldloc(arrayIndex);

        il.Append(Ldc_I4(0));
        il.Append(Stloc(arrayIndex));
        il.Append(Br_S(exitCondition));

        // prepare store
        var loopBody = il.AppendReturn(Nop());

        if (TryGetNetworkWrapperType(elementType, out var wrapInfo)) {

          WeaveNetworkUnwrap(asm, il, ctx, wrapInfo, elementType,
            prepareStore: il => {
              il.Append(Ldloc(para));
              il.Append(Ldloc(arrayIndex));
            },
            storeResult: il => {
              il.Append(Stelem(elementType));
            });

        } else {

          il.Append(Ldloc(para));
          il.Append(Ldloc(arrayIndex));

          il.AppendMacro(ctx.LoadAddress());
          il.Append(Ldind_or_Ldobj(elementType));
          il.Append(Stelem(elementType));

          il.AppendMacro(ctx.AddOffset(GetByteCount(asm, elementType)));
        }

        // increase index
        il.Append(Ldloc(arrayIndex));
        il.Append(Ldc_I4(1));
        il.Append(Add());
        il.Append(Stloc(arrayIndex));

        // exit condition
        il.Append(exitCondition);
        il.Append(Ldloc(para));
        il.Append(Ldlen());
        il.Append(Conv_I4());
        il.Append(Blt_S(loopBody));
      }
    }

    void WeaveRpcArrayInput(ILWeaverAssembly asm, RpcMethodContext ctx, ILProcessor il, ParameterDefinition para) {
      var elementType = para.ParameterType.GetElementType();

      // store the array size
      il.AppendMacro(ctx.LoadAddress());

      // array length
      il.Append(Ldarg(para));
      il.Append(Ldlen());
      il.Append(Conv_I4());

      il.Append(Stind_I4());

      il.AppendMacro(ctx.AddOffset(sizeof(Int32)));

      if (elementType.IsPrimitive) {
        using (ctx.AddOffsetScope(il)) { 
          // dst
          il.AppendMacro(ctx.LoadAddress());

          // src
          il.Append(Ldarg(para));
          var memCpy = new GenericInstanceMethod(asm.Native.GetMethod(nameof(Native.CopyFromArray), 2));
          memCpy.GenericArguments.Add(elementType);
          il.Append(Call(memCpy));

          if (elementType.IsPrimitive && (elementType.GetPrimitiveSize() % Allocator.REPLICATE_WORD_SIZE) != 0) {
            il.AppendMacro(AlignToWordSize());
          }
        };
      } else {

        var arrayIndex = ctx.GetOrCreateVariable("arrayIndex", asm.Import(typeof(int)));

        var exitCondition = Ldloc(arrayIndex);

        il.Append(Ldc_I4(0));
        il.Append(Stloc(arrayIndex));
        il.Append(Br_S(exitCondition));

        var loopBody = il.AppendReturn(Nop());


        if (TryGetNetworkWrapperType(elementType, out var wrapInfo)) {

          WeaveNetworkWrap(asm, il, ctx, il => {
            il.Append(Ldarg(para));
            il.Append(Ldloc(arrayIndex));
            il.Append(Ldelem(elementType));
          }, wrapInfo);

        } else {
          // get array elem
          il.AppendMacro(ctx.LoadAddress());

          il.Append(Ldarg(para));
          il.Append(Ldloc(arrayIndex));
          il.Append(Ldelem(elementType));
          il.Append(Stind_or_Stobj(elementType));
          il.AppendMacro(ctx.AddOffset(GetByteCount(asm, elementType)));
        }

        // increase index
        il.Append(Ldloc(arrayIndex));
        il.Append(Ldc_I4(1));
        il.Append(Add());
        il.Append(Stloc(arrayIndex));

        // exit condition
        il.Append(exitCondition);
        il.Append(Ldarg(para));
        il.Append(Ldlen());
        il.Append(Conv_I4());
        il.Append(Blt_S(loopBody));

      }
    }

    void WeaveNetworkWrap(ILWeaverAssembly asm, ILProcessor il, MethodContext ctx, Action<ILProcessor> emitGetValue, WrapInfo wrapInfo) {

      if (!wrapInfo.IsRaw) {
        il.AppendMacro(ctx.LoadAddress());
      }

      il.AppendMacro(ctx.LoadRunner());
      emitGetValue(il);

      if (wrapInfo.IsRaw) {
        il.AppendMacro(ctx.LoadAddress());
      }

      il.Append(Call(ctx.ImportReference(wrapInfo.WrapMethod)));

      if (wrapInfo.IsRaw) {
        // read bytes are on top of the stack
        il.Append(Ldc_I4(wrapInfo.MaxRawByteCount));
        var validateMethod = new GenericInstanceMethod(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.VerifyRawNetworkWrap)));
        validateMethod.GenericArguments.Add(ctx.ImportReference(wrapInfo.Type));
        il.Append(Call(validateMethod));

        if (ctx.HasOffset) {
          il.AppendMacro(AlignToWordSize());
          il.AppendMacro(ctx.AddOffset());
        } else {
          il.Append(Pop());
        }

      } else {
        var type = ctx.ImportReference(wrapInfo.WrapperType.GetElementType());
        il.Append(Stind_or_Stobj(type));
        
        if (ctx.HasOffset) {
          il.AppendMacro(ctx.AddOffset(GetByteCount(asm, type)));
        }
      }
    }

    void WeaveNetworkUnwrap(ILWeaverAssembly asm, ILProcessor il, MethodContext ctx, WrapInfo wrapInfo, TypeReference resultType, FieldDefinition previousValue = null, Action<ILProcessor> prepareStore = null, Action<ILProcessor> storeResult = null) {
      if (resultType == null) {
        throw new ArgumentNullException(nameof(resultType));
      }

      var wrapperType = wrapInfo.UnwrapMethod.Parameters[1].ParameterType;
      Log.Assert(!wrapperType.IsByReference);

      var typeToLoad = ctx.ImportReference(wrapperType);

      if (wrapInfo.IsRaw || wrapInfo.UnwrapByRef) {
        il.AppendMacro(ctx.LoadRunner());
        il.AppendMacro(ctx.LoadAddress());
        if (!wrapInfo.IsRaw) {
          il.Append(Ldind_or_Ldobj(typeToLoad));
        }

        VariableDefinition byRefTempVariable = null;
        if (previousValue == null) {
          byRefTempVariable = ctx.GetOrCreateVariable("RawUnwrap_tmp", ctx.ImportReference(wrapInfo.Type));
          il.Append(Ldloca_S(byRefTempVariable));
        } else {
          il.Append(Ldarg_0());
          il.Append(Ldflda(previousValue));
        }

        il.Append(Call(ctx.ImportReference(wrapInfo.UnwrapMethod)));

        if (wrapInfo.IsRaw) {
          // check if number of bytes checks out
          GenericInstanceMethod validateMethod;
          validateMethod = new GenericInstanceMethod(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.VerifyRawNetworkUnwrap), genericArgsCount: 1));
          validateMethod.GenericArguments.Add(resultType);
          il.Append(Ldc_I4(wrapInfo.MaxRawByteCount));
          il.Append(Call(validateMethod));

          if (ctx.HasOffset) {
            il.AppendMacro(AlignToWordSize());
            il.AppendMacro(ctx.AddOffset());
          } else {
            il.Append(Pop());
          }

        } else {
          il.AppendMacro(ctx.AddOffset(GetByteCount(asm, resultType)));
        }

        prepareStore?.Invoke(il);

        if (previousValue == null) {
          il.Append(Ldloc(byRefTempVariable));
        } else {
          il.Append(Ldarg_0());
          il.Append(Ldfld(previousValue));
        }

        if (!wrapInfo.UnwrapMethod.ReturnType.IsSame(resultType)) {
          il.Append(Cast(resultType));
        }

        storeResult?.Invoke(il);

      } else {

        // if not by ref, we don't handle the previous value field
        Log.Assert(previousValue == null);

        prepareStore?.Invoke(il);

        il.AppendMacro(ctx.LoadRunner());
        il.AppendMacro(ctx.LoadAddress());
        il.Append(Ldind_or_Ldobj(typeToLoad));

        il.Append(Call(ctx.ImportReference(wrapInfo.UnwrapMethod)));

        if (!wrapInfo.UnwrapMethod.ReturnType.IsSame(resultType)) {
          il.Append(Cast(resultType));
        }

        storeResult?.Invoke(il);

        il.AppendMacro(ctx.AddOffset(GetByteCount(asm, resultType)));
      }
    }

    void WeaveSimulation(ILWeaverAssembly asm, TypeDefinition type) {

      if (type.HasGenericParameters) {
        return;
      }

      WeaveRpcs(asm, type, allowInstanceRpcs: false);
    }


    private Instruction[] GetInlineFieldInit(MethodDefinition constructor, FieldDefinition field) {
      var instructions = constructor.Body.Instructions;
      int prevStfld = -1;
      for (int i = 0; i < instructions.Count; ++i) {
        var instruction = instructions[i];
        if (instruction.OpCode == OpCodes.Stfld) {
          // found the store
          if (instruction.Operand == field) {
            int start = prevStfld + 1;
            return instructions.Skip(start).Take(i - start + 1).ToArray();
          } else {
            prevStfld = i;
          }
        } else if (instruction.IsBaseConstructorCall(constructor.DeclaringType)) {
          // base constructor init
          break;
        }
      }
      return Array.Empty<Instruction>();
    }

    private Instruction[] RemoveInlineFieldInit(TypeDefinition type, FieldDefinition field) {
      var constructors = type.GetConstructors().Where(x => !x.IsStatic);
      if (!constructors.Any()) {
        return Array.Empty<Instruction>();
      }

      var firstConstructor = constructors.First();
      var firstInlineInit = GetInlineFieldInit(firstConstructor, field).ToArray();
      if (firstInlineInit.Length != 0) {
        Log.Debug($"Found {field} inline init: {(string.Join("; ", firstInlineInit.Cast<object>()))}");
      }

      foreach (var constructor in constructors.Skip(1)) {
        var otherInlineInit = GetInlineFieldInit(constructor, field);
        if (!firstInlineInit.SequenceEqual(otherInlineInit, new InstructionEqualityComparer())) {
          throw new ILWeaverException($"Expect inline init of {field} to be the same in all constructors," +
            $" but there's a difference between {firstConstructor} and {constructor}");
        }
      }

      foreach (var constructor in constructors) {
        Log.Debug($"Removing inline init of {field} from {constructor}");
        var il = constructor.Body.GetILProcessor();
        var otherInlineInit = GetInlineFieldInit(constructor, field);
        foreach (var instruction in otherInlineInit.Reverse()) {
          Log.Debug($"Removing {instruction}");
          il.Remove(instruction);
        }
      }

      return firstInlineInit;
    }

    private static bool IsMakeInitializerCall(Instruction instruction) {
      if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference method) {
        if (method.DeclaringType.IsSame<NetworkBehaviour>() && method.Name == nameof(NetworkBehaviour.MakeInitializer)) {
          return true;
        }
      }
      return false;
    }

    private static bool IsMakeRefOrMakePtrCall(Instruction instruction) {
      if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference method) {
        if (method.DeclaringType.IsSame<NetworkBehaviour>() && (
          method.Name == nameof(NetworkBehaviour.MakeRef) || method.Name == nameof(NetworkBehaviour.MakePtr)
          )) {
          return true;
        }
      }
      return false;
    }

    private void CheckIfMakeInitializerImplicitCast(Instruction instruction) {
      if (instruction.OpCode == OpCodes.Call && (instruction.Operand as MethodReference)?.Name == "op_Implicit") {
        // all good
      } else {
        throw new ILWeaverException($"Expected an implicit cast, got {instruction}");
      }
    }

    private void ReplaceBackingFieldInInlineInit(ILWeaverAssembly asm, FieldDefinition backingField, FieldDefinition field, ILProcessor il, Instruction[] instructions) {
      bool nextImplicitCast = false;
      foreach (var instruction in instructions) {
        if (nextImplicitCast) {
          CheckIfMakeInitializerImplicitCast(instruction);
          nextImplicitCast = false;
          il.Remove(instruction);
        } else if (instruction.OpCode == OpCodes.Stfld && instruction.Operand == backingField) {
          instruction.Operand = field;
        } else if (IsMakeInitializerCall(instruction)) {
          // dictionaries need one extra step, if using SerializableDictionary :(
          if (ILWeaverSettings.UseSerializableDictionaryForNetworkDictionaryProperties() && backingField.FieldType.IsNetworkDictionary(out var keyType, out var valueType)) {
            var m = new GenericInstanceMethod(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.MakeSerializableDictionary)));
            m.GenericArguments.Add(keyType);
            m.GenericArguments.Add(valueType);
            Log.Debug($"Inline init for {field}, replacing {instruction.Operand} with {m}");
            instruction.Operand = m;
          } else {
            // remove the op, it will be fine
            Log.Debug($"Inline init for {field}, removing {instruction}");
            il.Remove(instruction);
          }
          nextImplicitCast = true;
        }
      }
    }

    static IEnumerable<TypeDefinition> AllTypeDefs(TypeDefinition definitions) {
      yield return definitions;

      if (definitions.HasNestedTypes) {
        foreach (var nested in definitions.NestedTypes.SelectMany(AllTypeDefs)) {
          yield return nested;
        }
      }
    }

    static IEnumerable<TypeDefinition> AllTypeDefs(Collection<TypeDefinition> definitions) {
      return definitions.SelectMany(AllTypeDefs);
    }

    public bool Weave(ILWeaverAssembly asm) {
      // if we don't have the weaved assembly attribute, we need to do weaving and insert the attribute
      if (asm.CecilAssembly.HasAttribute<NetworkAssemblyWeavedAttribute>() != false) {
        return false;
      }

      using (Log.ScopeAssembly(asm.CecilAssembly)) {
        // grab main module .. this contains all the types we need
        var module = asm.CecilAssembly.MainModule;
        var moduleAllTypes = AllTypeDefs(module.Types).ToArray();

        // go through all types and check for network behaviours
        foreach (var t in moduleAllTypes) {
          if (t.IsValueType && t.Is<INetworkStruct>()) {
            try {
              WeaveStruct(asm, t, null);
            } catch (Exception ex) {
              throw new ILWeaverException($"Failed to weave struct {t}", ex);
            }
          }
        }

        foreach (var t in moduleAllTypes) {
          if (t.IsValueType && t.Is<INetworkInput>()) {
            try {
              WeaveInput(asm, t);
            } catch (Exception ex) {
              throw new ILWeaverException($"Failed to weave input {t}", ex);
            }
          }
        }

        foreach (var t in moduleAllTypes) {
          if (t.IsSubclassOf<NetworkBehaviour>()) {
            try {
              WeaveBehaviour(asm, t);
            } catch (Exception ex) {
              throw new ILWeaverException($"Failed to weave behaviour {t}", ex);
            }
          } else if (t.IsSubclassOf<SimulationBehaviour>()) {
            try {
              WeaveSimulation(asm, t);
            } catch (Exception ex) {
              throw new ILWeaverException($"Failed to weave behaviour {t}", ex);
            }
          }
        }

        // only if it was modified
        if (asm.Modified) {
          // add weaved assembly attribute to this assembly
          asm.CecilAssembly.CustomAttributes.Add(new CustomAttribute(typeof(NetworkAssemblyWeavedAttribute).GetConstructor(asm)));
        }

        return asm.Modified;
      }
    }
  }
}
#endif


#endregion


#region Assets/Photon/FusionCodeGen/ILWeaver.INetworkedStruct.cs

#if FUSION_WEAVER && FUSION_HAS_MONO_CECIL
namespace Fusion.CodeGen {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using UnityEditor;
  using UnityEditor.Compilation;
  using UnityEngine;
  using System.Runtime.CompilerServices;
  using static Fusion.CodeGen.ILWeaverOpCodes;
  using Mono.Cecil;
  using Mono.Cecil.Cil;
  using Mono.Cecil.Rocks;
  using Mono.Collections.Generic;
  using CompilerAssembly = UnityEditor.Compilation.Assembly;
  using FieldAttributes = Mono.Cecil.FieldAttributes;
  using MethodAttributes = Mono.Cecil.MethodAttributes;
  using ParameterAttributes = Mono.Cecil.ParameterAttributes;

  unsafe partial class ILWeaver {

    const int WordSize = Allocator.REPLICATE_WORD_SIZE;

    private bool IsTypeBlittable(ILWeaverAssembly asm, TypeReference type) {
      if (type.IsPrimitive) {
        return type.IsIntegral();
      } else if (!type.IsValueType) {
        return false;
      } else if (type.IsVector2() || type.IsVector3() || type.IsQuaternion()) {
        return false;
      } else if (type.IsNetworkArray() || type.IsNetworkDictionary() || type.IsNetworkList()) {
        return false;
      } else {
        return true;
      }
    }

    public void WeaveStruct(ILWeaverAssembly asm, TypeDefinition type, TypeReference typeRef) {
      ILWeaverException.DebugThrowIf(!type.Is<INetworkStruct>(), $"Not a {nameof(INetworkStruct)}");

      string typeKey;
      if (type.HasGenericParameters) {
        Log.Assert(typeRef?.IsGenericInstance == true);
        typeKey = typeRef.FullName;
      } else {
        typeKey = type.FullName;
      }

      if (type.TryGetAttribute<NetworkStructWeavedAttribute>(out var attribute)) {

        if (_typeData.ContainsKey(typeKey) == false) {
          var wordCount = attribute.GetAttributeArgument<int>(0);
          if (attribute.TryGetAttributeArgument(1, out bool value, false) && value) {
            Log.Assert(typeRef?.IsGenericInstance == true);
            foreach (var gen in ((GenericInstanceType)typeRef).GenericArguments) {
              if (gen.IsValueType && gen.Is<INetworkStruct>()) {
                wordCount += GetTypeWordCount(asm, gen);
              }
            }
          }

          _typeData.Add(typeKey, new TypeMetaData {
            WordCount = wordCount,
            Definition = type,
            Reference = type
          });
        }

        return;
      }

      using (Log.ScopeStruct(type)) {
        
        int wordCount = WeaveStructInner(asm, type);

        // track type data
        _typeData.Add(typeKey, new TypeMetaData {
          WordCount = wordCount,
          Definition = type,
          Reference = type
        });

        // add new attribute
        type.AddAttribute<NetworkStructWeavedAttribute, int>(asm, wordCount);
      }
    }

    int WeaveStructInner(ILWeaverAssembly asm, TypeDefinition type) {
      // flag asm as modified
      asm.Modified = true;

      // set as explicit layout
      type.IsExplicitLayout = true;

      // clear all backing fields
      foreach (var property in type.Properties) {
        if (!IsWeavableProperty(property, out var propertyInfo)) {
          continue;
        }

        property.ThrowIfStatic();

        if (IsTypeBlittable(asm, property.PropertyType)) {
          Log.Warn($"Networked property {property} should be replaced with a regular field. For structs, " +
            $"[Networked] attribute should to be applied only on collections, booleans, floats and vectors.");
        }

        int fieldIndex = type.Fields.Count;

        if (propertyInfo.BackingField != null) {
          if (!propertyInfo.BackingField.FieldType.IsValueType) {
            Log.Warn($"Networked property {property} has a backing field that is not a value type. To keep unmanaged status," +
              $" the accessor should follow \"{{ get => default; set {{}} }}\" pattern");
          }

          fieldIndex = type.Fields.IndexOf(propertyInfo.BackingField);
          if (fieldIndex >= 0) {
            type.Fields.RemoveAt(fieldIndex);
          }
        }

        try {
          var propertyWordCount = GetPropertyWordCount(asm, property);

          property.GetMethod?.RemoveAttribute<CompilerGeneratedAttribute>(asm);
          property.SetMethod?.RemoveAttribute<CompilerGeneratedAttribute>(asm);

          var getIL = property.GetMethod.Body.GetILProcessor();
          getIL.Clear();
          getIL.Body.Variables.Clear();

          var setIL = property.SetMethod?.Body.GetILProcessor();
          if (setIL != null) {
            setIL.Clear();
            setIL.Body.Variables.Clear();
          }

          var backingFieldName = $"_{property.Name}";
          var fixedBufferInfo = CacheGetFixedBuffer(asm, propertyWordCount);
          var surrogateType = CacheGetUnitySurrogate(asm, property);
          var storageField = new FieldDefinition($"_{property.Name}", FieldAttributes.Private, fixedBufferInfo.Type);

          int capacity;
          if (property.PropertyType.IsNetworkDictionary()) {
            capacity = GetStaticDictionaryCapacity(property);
          } else {
            capacity = GetCapacity(property, 1);
          }
          storageField.AddAttribute<SerializeField>(asm);
          storageField.AddAttribute<FixedBufferPropertyAttribute, TypeReference, TypeReference, int>(asm, property.PropertyType, surrogateType, capacity);

          type.Fields.Insert(fieldIndex, storageField);

          // move field attributes, if any
          if (propertyInfo.BackingField != null) {
            MoveBackingFieldAttributes(asm, propertyInfo.BackingField, storageField);
          }
          MovePropertyAttributesToBackingField(asm, property, storageField);

          InjectValueAccessor(asm, getIL, setIL, property, property.PropertyType, OpCodes.Ldarg_1, (il, offset) => {

            var m = new GenericInstanceMethod(asm.Native.GetMethod(nameof(Native.ReferenceToPointer)));
            m.GenericArguments.Add(storageField.FieldType);

            il.Append(Ldarg_0());
            il.Append(Ldflda(storageField));

            il.Append(Call(m));

          }, false);
        } catch (Exception ex) {
          throw new ILWeaverException($"Failed to weave property {property}", ex);
        }
      }

      // figure out word counts for everything
      var wordCount = 0;

      foreach (var field in type.Fields) {

        // skip statics
        if (field.IsStatic) {
          continue;
        }

        // set offset 
        field.Offset = wordCount * Allocator.REPLICATE_WORD_SIZE;

        try {
          // increase block count
          wordCount += GetTypeWordCount(asm, field.FieldType);
        } catch (Exception ex) {
          throw new ILWeaverException($"Failed to get word count of field {field}", ex);
        }
      }

      return wordCount;
    }
  }
}
#endif


#endregion


#region Assets/Photon/FusionCodeGen/ILWeaver.NetworkBehaviour.cs

#if FUSION_WEAVER && FUSION_HAS_MONO_CECIL
namespace Fusion.CodeGen {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using UnityEditor;
  using UnityEditor.Compilation;
  using UnityEngine;
  using System.Runtime.CompilerServices;
  using static Fusion.CodeGen.ILWeaverOpCodes;
  using Mono.Cecil;
  using Mono.Cecil.Cil;
  using Mono.Cecil.Rocks;
  using Mono.Collections.Generic;
  using CompilerAssembly = UnityEditor.Compilation.Assembly;
  using FieldAttributes = Mono.Cecil.FieldAttributes;
  using MethodAttributes = Mono.Cecil.MethodAttributes;
  using ParameterAttributes = Mono.Cecil.ParameterAttributes;
  using UnityEngine.Scripting;

  unsafe partial class ILWeaver {

    FieldDefinition AddNetworkBehaviourBackingField(ILWeaverAssembly asm, PropertyDefinition property) {

      TypeReference fieldType = property.PropertyType;
      if (fieldType.IsPointer || fieldType.IsByReference) {
        fieldType = fieldType.GetElementType();
      } else if (fieldType.IsNetworkArray(out var elementType) || fieldType.IsNetworkList(out elementType)) {
        fieldType = TypeReferenceRocks.MakeArrayType(elementType);
      } else if (fieldType.IsNetworkDictionary(out var keyType, out var valueType)) {
        if (ILWeaverSettings.UseSerializableDictionaryForNetworkDictionaryProperties()) {
          fieldType = TypeReferenceRocks.MakeGenericInstanceType(asm.Import(typeof(SerializableDictionary<,>)), keyType, valueType);
        } else {
          fieldType = TypeReferenceRocks.MakeGenericInstanceType(asm.Import(typeof(Dictionary<,>)), keyType, valueType);
        }
      }

      var field = new FieldDefinition(GetInspectorFieldName(property.Name), FieldAttributes.Private, fieldType);
      return field;
    }

    private void MoveBackingFieldAttributes(ILWeaverAssembly asm, FieldDefinition backingField, FieldDefinition storageField) {
      if (backingField.IsNotSerialized) {
        storageField.IsNotSerialized = true;
      }

      foreach (var attrib in backingField.CustomAttributes) {
        if (attrib.AttributeType.Is<CompilerGeneratedAttribute>() ||
            attrib.AttributeType.Is<System.Diagnostics.DebuggerBrowsableAttribute>()) {
          continue;
        }
        storageField.CustomAttributes.Add(attrib);
      }
    }

    private void MovePropertyAttributesToBackingField(ILWeaverAssembly asm, PropertyDefinition property, FieldDefinition field) {
      bool hasNonSerialized = false;

      foreach (var attribute in property.CustomAttributes) {
        if (attribute.AttributeType.IsSame<NetworkedAttribute>() ||
            attribute.AttributeType.IsSame<NetworkedWeavedAttribute>() ||
            attribute.AttributeType.IsSame<AccuracyAttribute>() ||
            attribute.AttributeType.IsSame<CapacityAttribute>()) {
          continue;
        }

        var attribDef = attribute.AttributeType.TryResolve();
        if (attribDef == null) {
          Log.Warn($"Failed to resolve {attribute.AttributeType}, not going to try to apply on {field}");
          continue;
        }


        if (attribDef.TryGetAttribute<UnityPropertyAttributeProxyAttribute>(out var proxy)) {
          Log.Debug($"Found proxy attribute {attribute.AttributeType}, applying to {field}");
          var attribTypeRef = proxy.GetAttributeArgument<TypeReference>(0);
          var attribTypeDef = attribTypeRef.Resolve();

          if (attribTypeDef.TryGetMatchingConstructor(attribute.Constructor.Resolve(), out var constructor)) {
            field.CustomAttributes.Add(new CustomAttribute(property.Module.ImportReference(constructor), attribute.GetBlob()));

            if (attribute.AttributeType.IsSame<UnityNonSerializedAttribute>()) {
              Log.Debug($"{field} marked as NonSerialized, SerializeField will not be applied");
              hasNonSerialized = true;
            }
          } else {
            Log.Warn($"Failed to find matching constructor of {attribTypeDef} for {attribute.Constructor} (field {field})");
          }

          continue;
        }

        if (attribDef.TryGetAttribute<AttributeUsageAttribute>(out var usage)) {
          var targets = usage.GetAttributeArgument<AttributeTargets>(0);
          if ((targets & AttributeTargets.Field) != AttributeTargets.Field) {
            Log.Debug($"Attribute {attribute.AttributeType} can't be applied on a field ({field}), skipping.");
            continue;
          }
        }

        Log.Debug($"Copying {attribute.AttributeType} to {field}");
        field.CustomAttributes.Add(new CustomAttribute(attribute.Constructor, attribute.GetBlob()));
      }

      if (!hasNonSerialized && !property.GetMethod.IsPrivate) {
        if (field.IsNotSerialized) {
          // prohibited
        } else if (field.HasAttribute<SerializeField>()) {
          // already added
        } else { 
          field.AddAttribute<SerializeField>(asm);
        }
      }
    }

    public void WeaveBehaviour(ILWeaverAssembly asm, TypeDefinition type) {
      if (type.HasGenericParameters) {
        return;
      }

      ILWeaverException.DebugThrowIf(!type.IsSubclassOf<NetworkBehaviour>(), $"Not a {nameof(NetworkBehaviour)}");

      if (type.TryGetAttribute<NetworkBehaviourWeavedAttribute>(out var weavedAttribute)) {
        int weavedSize = weavedAttribute.GetAttributeArgument<int>(0);

        if (_networkedBehaviourTypeData.TryGetValue(type.FullName, out var metaData)) {
          Debug.Assert(weavedSize < 0 || weavedSize == metaData.BlockCount);
        } else {
          _networkedBehaviourTypeData.Add(type.FullName, new BehaviourMetaData {
            Definition = type,
            BlockCount = weavedSize >= 0 ? weavedSize : GetNetworkBehaviourWordCount(asm, type)
          });
        }

        return;
      }

      // flag as modified
      asm.Modified = true;

      using (Log.ScopeBehaviour(type)) {

        var changed = asm.Import(typeof(Changed<>)).MakeGenericInstanceType(type);
        var changedDelegate = asm.Import(typeof(ChangedDelegate<>)).MakeGenericInstanceType(type);
        var networkBehaviourCallbacks = asm.Import(typeof(NetworkBehaviourCallbacks<>)).MakeGenericInstanceType(type);
        type.Fields.Add(new FieldDefinition("$IL2CPP_CHANGED", FieldAttributes.Static, changed));
        type.Fields.Add(new FieldDefinition("$IL2CPP_CHANGED_DELEGATE", FieldAttributes.Static, changedDelegate));
        type.Fields.Add(new FieldDefinition("$IL2CPP_NETWORK_BEHAVIOUR_CALLBACKS", FieldAttributes.Static, networkBehaviourCallbacks));

        // get block count of parent as starting point for ourselves
        var wordCount = GetNetworkBehaviourWordCount(asm, type.BaseType.Resolve());

        // this is the data field which holds this behaviours root pointer
        var dataField = GetFieldFromNetworkedBehaviour(asm, type, PTR_FIELD_NAME);

        // find onspawned method
        Func<string, (MethodDefinition, Instruction)> createOverride = (name) => {
          var result = type.Methods.FirstOrDefault(x => x.Name == name);
          if (result != null) {
            // need to find the placeholder method
            var placeholderMethodName = asm.NetworkedBehaviour.GetMethod(nameof(NetworkBehaviour.InvokeWeavedCode)).FullName;
            var placeholders = result.Body.Instructions
              .Where(x => x.OpCode == OpCodes.Call && x.Operand is MethodReference && ((MethodReference)x.Operand).FullName == placeholderMethodName)
              .ToArray();

            if (placeholders.Length != 1) {
              throw new ILWeaverException($"When overriding {name} in a type with [Networked] properties, make sure to call {placeholderMethodName} exactly once somewhere.");
            }

            var placeholder = placeholders[0];
            var il = result.Body.GetILProcessor();

            var jumpTarget = Nop();
            var returnTarget = Nop();

            // this is where to jump after weaved code's done
            il.InsertAfter(placeholder, returnTarget);
            il.InsertAfter(placeholder, Br(jumpTarget));

            il.Append(jumpTarget);
            return (result, Br(returnTarget));
          }

          result = new MethodDefinition(name, MethodAttributes.Public, asm.CecilAssembly.MainModule.ImportReference(typeof(void))) {
            IsVirtual = true,
            IsHideBySig = true,
            IsReuseSlot = true
          };

          var baseMethod = FindMethodInParent(asm, type, name, nameof(SimulationBehaviour));

          // call base method
          if (baseMethod != null) {
            if (baseMethod.DeclaringType.IsSame<NetworkBehaviour>()) {
              // don't call base method
              foreach (var parameter in baseMethod.Parameters) {
                result.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
              }
            } else {
              var bodyIL = result.Body.GetILProcessor();

              bodyIL.Append(Instruction.Create(OpCodes.Ldarg_0));

              foreach (var parameter in baseMethod.Parameters) {
                var p = new ParameterDefinition(parameter.ParameterType);
                result.Parameters.Add(p);
                bodyIL.Append(Ldarg(p));
              }

              bodyIL.Append(Instruction.Create(OpCodes.Call, baseMethod));
            }
          }

          type.Methods.Add(result);
          return (result, Ret());
        };

        var setDefaults = new Lazy<(MethodDefinition, Instruction)>(() => createOverride(nameof(NetworkBehaviour.CopyBackingFieldsToState)));
        var getDefaults = new Lazy<(MethodDefinition, Instruction)>(() => createOverride(nameof(NetworkBehaviour.CopyStateToBackingFields)));

        FieldDefinition lastAddedFieldWithKnownPosition = null;
        List<FieldDefinition> fieldsWithUncertainPosition = new List<FieldDefinition>();

        foreach (var property in type.Properties) {
          if (IsWeavableProperty(property, out var propertyInfo) == false) {
            continue;
          }

          property.ThrowIfStatic();

          if (!string.IsNullOrEmpty(propertyInfo.OnChanged)) {
            WeaveChangedHandler(asm, property, propertyInfo.OnChanged);
          }


          if (property.PropertyType.IsPointer || property.PropertyType.IsByReference) {
            var elementType = property.PropertyType.GetElementType();
            if (!IsTypeBlittable(asm, elementType)) {
              Log.Warn($"Property {property} type ({elementType}) is not safe to in pointer/reference properties. " +
                $"Consider wrapping it with a struct implementing INetworkStruct and a [Networked] property.");
            }
          }


          try {
            // try to maintain fields order
            int backingFieldIndex = type.Fields.Count;
            if (propertyInfo.BackingField != null) {
              backingFieldIndex = type.Fields.IndexOf(propertyInfo.BackingField);
              if (backingFieldIndex >= 0) {
                type.Fields.RemoveAt(backingFieldIndex);
              } else {
                Log.Warn($"Unable to find backing field for {property}");
                backingFieldIndex = type.Fields.Count;
              }
            }

            var readOnlyInit = GetReadOnlyPropertyInitializer(property);

            // prepare getter/setter methods


            var (getter, setter) = PreparePropertyForWeaving(property);

            // capture word count in case we re-use the lambda that is created later on ...
            var wordOffset = wordCount;

            // perform injection
            InjectValueAccessor(asm, getter.Body.GetILProcessor(), setter?.Body?.GetILProcessor(), property, property.PropertyType, OpCodes.Ldarg_1,
              (il, offset) => LoadDataAddress(il, dataField, wordOffset + offset), true);

            var propertyWordCount = GetPropertyWordCount(asm, property);

            // step up wordcount
            wordCount += propertyWordCount;

            // inject attribute to poll weaver data during runtime
            property.AddAttribute<NetworkedWeavedAttribute, int, int>(asm, wordOffset, propertyWordCount);



            if (property.HasAttribute<UnityNonSerializedAttribute>() || propertyInfo.BackingField?.IsNotSerialized == true) {
              // so the property is not serialized, so there will be no backing field.

              IEnumerable<Instruction> fieldInit = null;
              VariableDefinition[] fieldInitLocalVariables = null;

              if (readOnlyInit != null) {
                fieldInit = readOnlyInit.Value.Instructions;
                fieldInitLocalVariables = readOnlyInit.Value.Variables;
              } else {
                fieldInit = RemoveInlineFieldInit(type, propertyInfo.BackingField);
                fieldInitLocalVariables = Array.Empty<VariableDefinition>();
              }

              if (fieldInit?.Any() == true) {
                // need to patch defaults with this, but only during the initial set
                var il = setDefaults.Value.Item1.Body.GetILProcessor();
                var postInit = Nop();
                il.Append(Ldarg_1());
                il.Append(Brfalse(postInit));

                foreach (var loc in fieldInitLocalVariables) {
                  il.Body.Variables.Add(loc);
                }

                if (property.PropertyType.IsPointer ||
                    property.PropertyType.IsByReference) {
                  // load up address
                  il.Append(Ldarg_0());
                  il.Append(Call(getter));
                } else if (property.PropertyType.IsNetworkArray() ||
                  property.PropertyType.IsNetworkList() ||
                  property.PropertyType.IsNetworkDictionary()) {

                  var first = fieldInit.First();
                  // needs some special init
                  Log.AssertMessage(first.OpCode == OpCodes.Ldarg_0, $"Expected Ldarg_0, got: {first}");
                  il.Append(first);
                  il.Append(Call(getter));
                  fieldInit = fieldInit.Skip(1);
                }

                bool skipImplicitInitializerCast = false;
                foreach (var instruction in fieldInit) {
                  if (instruction.IsLdlocWithIndex(out var index)) {
                    var repl = Ldloc(fieldInitLocalVariables[index], il.Body.Method);
                    il.Append(repl);
                  } else if (skipImplicitInitializerCast) {
                    skipImplicitInitializerCast = false;
                    CheckIfMakeInitializerImplicitCast(instruction);
                  } else if (instruction.OpCode == OpCodes.Stfld && instruction.Operand == propertyInfo.BackingField) {
                    // arrays and dictionaries don't have setters
                    if (property.PropertyType.IsPointer || property.PropertyType.IsByReference) {
                      il.Append(Stind_or_Stobj(property.PropertyType.GetElementType()));
                    } else if (property.PropertyType.IsNetworkArray(out var elementType)) {
                      var m = new GenericInstanceMethod(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.InitializeNetworkArray)));
                      m.GenericArguments.Add(elementType);
                      il.Append(Ldstr(property.Name));
                      il.Append(Call(m));
                    } else if (property.PropertyType.IsNetworkList(out elementType)) {
                      var m = new GenericInstanceMethod(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.InitializeNetworkList)));
                      m.GenericArguments.Add(elementType);
                      il.Append(Ldstr(property.Name));
                      il.Append(Call(m));
                    } else if (property.PropertyType.IsNetworkDictionary(out var keyType, out var valueType)) {
                      var m = new GenericInstanceMethod(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.InitializeNetworkDictionary)));
                      m.GenericArguments.Add(TypeReferenceRocks.MakeGenericInstanceType(asm.Import(typeof(Dictionary<,>)), keyType, valueType));
                      m.GenericArguments.Add(keyType);
                      m.GenericArguments.Add(valueType);
                      il.Append(Ldstr(property.Name));
                      il.Append(Call(m));
                    } else {
                      il.Append(Call(setter));
                    }
                  } else if (IsMakeInitializerCall(instruction)) {
                    skipImplicitInitializerCast = true;
                  } else {
                    il.Append(instruction);
                  }
                }

                if (property.PropertyType.IsPointer || property.PropertyType.IsByReference) {
                  il.Append(Stind_or_Stobj(property.PropertyType.GetElementType()));
                }

                il.Append(postInit);
              }

            } else {

              FieldDefinition defaultField;

              if (string.IsNullOrEmpty(propertyInfo.DefaultFieldName)) {
                defaultField = AddNetworkBehaviourBackingField(asm, property);
                if (propertyInfo.BackingField != null) {
                  type.Fields.Insert(backingFieldIndex, defaultField);
                  MoveBackingFieldAttributes(asm, propertyInfo.BackingField, defaultField);

                  if (lastAddedFieldWithKnownPosition == null) {
                    // fixup fields that have been added without knowing their index
                    foreach (var f in fieldsWithUncertainPosition) {
                      type.Fields.Remove(f);
                    }

                    var index = type.Fields.IndexOf(defaultField);
                    fieldsWithUncertainPosition.Reverse();
                    foreach (var f in fieldsWithUncertainPosition) {
                      type.Fields.Insert(index, f);
                    }
                  }

                  lastAddedFieldWithKnownPosition = defaultField;

                } else { 
                  if (lastAddedFieldWithKnownPosition == null) {
                    // not sure where to put this... append
                    type.Fields.Add(defaultField);
                    fieldsWithUncertainPosition.Add(defaultField);
                  } else {
                    // add after the previous field
                    var index = type.Fields.IndexOf(lastAddedFieldWithKnownPosition);
                    Log.Assert(index >= 0);

                    type.Fields.Insert(index+1, defaultField);
                    lastAddedFieldWithKnownPosition = defaultField;
                  }
                }
                MovePropertyAttributesToBackingField(asm, property, defaultField);
              } else {
                defaultField = property.DeclaringType.GetFieldOrThrow(propertyInfo.DefaultFieldName);
              }

              // in each constructor, replace inline init, if present
              foreach (var constructor in type.GetConstructors()) {

                if (readOnlyInit != null) {
                  var il = constructor.Body.GetILProcessor();

                  Instruction before = il.Body.Instructions[0];
                  {
                    // find where to plug in; after last stfld, but before base constructor call
                    for (int i = 0; i < il.Body.Instructions.Count; ++i) {
                      var instruction = il.Body.Instructions[i];
                      if (instruction.IsBaseConstructorCall(type)) {
                        break;
                      } else if (instruction.OpCode == OpCodes.Stfld) {
                        before = il.Body.Instructions[i + 1];
                      }
                    }
                  }

                  // clone variables
                  var (instructions, variables) = CloneInstructions(readOnlyInit.Value.Instructions, readOnlyInit.Value.Variables);

                  foreach (var variable in variables) {
                    il.Body.Variables.Add(variable);
                  }

                  il.InsertBefore(before, Ldarg_0());
                  foreach (var instruction in instructions) {
                    il.InsertBefore(before, instruction);
                  }
                  il.InsertBefore(before, Stfld(defaultField));
                } else {
                  // remove the inline init, if present
                  var init = GetInlineFieldInit(constructor, propertyInfo.BackingField);
                  if (init.Length > 0) {
                    ReplaceBackingFieldInInlineInit(asm, propertyInfo.BackingField, defaultField, constructor.Body.GetILProcessor(), init);
                  }
                }
              }

              defaultField.AddAttribute<DefaultForPropertyAttribute, string, int, int>(asm, property.Name, wordOffset, propertyWordCount);

              {
                var il = setDefaults.Value.Item1.Body.GetILProcessor();

                if (property.PropertyType.IsByReference || property.PropertyType.IsPointer) {
                  il.Append(Ldarg_0());
                  il.Append(Call(getter));

                  il.Append(Ldarg_0());
                  il.Append(Ldfld(defaultField));

                  il.Append(Stind_or_Stobj(property.PropertyType.GetElementType()));

                } else if (property.PropertyType.IsNetworkDictionary(out var keyType, out var valueType)) {

                  var m = new GenericInstanceMethod(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.InitializeNetworkDictionary)));
                  m.GenericArguments.Add(defaultField.FieldType);
                  m.GenericArguments.Add(keyType);
                  m.GenericArguments.Add(valueType);

                  il.Append(Ldarg_0());
                  il.Append(Call(getter));
                  il.Append(Ldarg_0());
                  il.Append(Ldfld(defaultField));
                  il.Append(Ldstr(property.Name));
                  il.Append(Call(m));

                } else if (property.PropertyType.IsNetworkArray(out var elementType)) {

                  var m = new GenericInstanceMethod(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.InitializeNetworkArray)));
                  m.GenericArguments.Add(elementType);
                  il.Append(Ldarg_0());
                  il.Append(Call(getter));
                  il.Append(Ldarg_0());
                  il.Append(Ldfld(defaultField));
                  il.Append(Ldstr(property.Name));
                  il.Append(Call(m));

                } else if (property.PropertyType.IsNetworkList(out elementType)) {

                  var m = new GenericInstanceMethod(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.InitializeNetworkList)));
                  m.GenericArguments.Add(elementType);
                  il.Append(Ldarg_0());
                  il.Append(Call(getter));
                  il.Append(Ldarg_0());
                  il.Append(Ldfld(defaultField));
                  il.Append(Ldstr(property.Name));
                  il.Append(Call(m));


                } else {
                  Log.AssertMessage(setter != null, $"{property} expected to have a setter");
                  il.Append(Ldarg_0());
                  il.Append(Ldarg_0());
                  il.Append(Ldfld(defaultField));
                  il.Append(Call(setter));
                }
              }

              {
                var il = getDefaults.Value.Item1.Body.GetILProcessor();

                if (property.PropertyType.IsByReference || property.PropertyType.IsPointer) {
                  il.Append(Ldarg_0());
                  il.Append(Ldarg_0());
                  il.Append(Call(getter));
                  il.Append(Ldind_or_Ldobj(property.PropertyType.GetElementType()));
                  il.Append(Stfld(defaultField));

                } else if (property.PropertyType.IsNetworkDictionary(out var keyType, out var valueType)) {

                  var m = new GenericInstanceMethod(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.CopyFromNetworkDictionary)));
                  m.GenericArguments.Add(defaultField.FieldType);
                  m.GenericArguments.Add(keyType);
                  m.GenericArguments.Add(valueType);

                  il.Append(Ldarg_0());
                  il.Append(Call(getter));
                  il.Append(Ldarg_0());
                  il.Append(Ldflda(defaultField));
                  il.Append(Call(m));

                } else if (property.PropertyType.IsNetworkArray(out var elementType)) {

                  var m = new GenericInstanceMethod(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.CopyFromNetworkArray)));
                  m.GenericArguments.Add(elementType);

                  il.Append(Ldarg_0());
                  il.Append(Call(getter));
                  il.Append(Ldarg_0());
                  il.Append(Ldflda(defaultField));
                  il.Append(Call(m));

                } else if (property.PropertyType.IsNetworkList(out elementType)) {

                  var m = new GenericInstanceMethod(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.CopyFromNetworkList)));
                  m.GenericArguments.Add(elementType);

                  il.Append(Ldarg_0());
                  il.Append(Call(getter));
                  il.Append(Ldarg_0());
                  il.Append(Ldflda(defaultField));
                  il.Append(Call(m));

                } else {
                  Log.AssertMessage(getter != null, $"{property} expected to have a getter");
                  il.Append(Ldarg_0());
                  il.Append(Ldarg_0());
                  il.Append(Call(getter));
                  il.Append(Stfld(defaultField));
                }
              }
            }
          } catch (Exception ex) {
            throw new ILWeaverException($"Failed to weave property {property}", ex);
          }
        }

        if (setDefaults.IsValueCreated) {
          var (method, instruction) = setDefaults.Value;
          method.Body.GetILProcessor().Append(instruction);
        }
        if (getDefaults.IsValueCreated) {
          var (method, instruction) = getDefaults.Value;
          method.Body.GetILProcessor().Append(instruction);
        }

        // add meta attribute
        var metaAttribute = GetMetaAttributeConstructor(asm);
        var metaAttributeCtor = new CustomAttribute(metaAttribute);
        metaAttributeCtor.ConstructorArguments.Add(new CustomAttributeArgument(ImportType<int>(asm), wordCount));
        type.CustomAttributes.Add(metaAttributeCtor);

        // add to type data lookup
        _networkedBehaviourTypeData.Add(type.FullName, new BehaviourMetaData {
          Definition = type,
          BlockCount = wordCount
        });

        WeaveRpcs(asm, type);
      }
    }

    private void WeaveChangedHandler(ILWeaverAssembly asm, PropertyDefinition property, string handlerName) {

      // find the handler
      {
        foreach (var declaringType in property.DeclaringType.GetHierarchy()) {
          var candidates = declaringType.GetMethods()
            .Where(x => x.IsStatic)
            .Where(x => x.Name == handlerName)
            .Where(x => x.HasParameters && x.Parameters.Count == 1)
            .Where(x => {
              var parameterType = x.Parameters[0].ParameterType;
              if (!parameterType.IsGenericInstance) {
                return false;
              }
              var openGenericType = parameterType.GetElementType();
              if (!openGenericType.IsSame(typeof(Changed<>))) {
                return false;
              }
              var behaviourType = ((GenericInstanceType)parameterType).GenericArguments[0];
              if (!property.DeclaringType.Is(behaviourType)) {
                return false;
              }
              return true;
            })
            .ToList();

          if (candidates.Count > 1) {
            throw new ILWeaverException($"Ambiguous match for OnChanged handler for {property}: {string.Join("; ", candidates)}");
          } else if (candidates.Count == 1) {

            var handler = candidates[0];

            Log.Debug($"OnChanged handler for {property}: {handler}");
            // add preserve attribute, if not added already
            if (!handler.TryGetAttribute<PreserveAttribute>(out _)) {
              handler.AddAttribute<PreserveAttribute>(asm);
              Log.Debug($"Added {nameof(PreserveAttribute)} to {handler}");
            }
            return;
          }
        }
      }

      throw new ILWeaverException($"No match found for OnChanged handler for {property}");
    }

    struct ReadOnlyInitializer {
      public Instruction[] Instructions;
      public VariableDefinition[] Variables;
    }

    private ReadOnlyInitializer? GetReadOnlyPropertyInitializer(PropertyDefinition property) {
      if (property.PropertyType.IsPointer || property.PropertyType.IsByReference) {
        // need to check if there's MakeRef/Ptr before getter gets obliterated 
        var instructions = property.GetMethod.Body.Instructions;

        for (int i = 0; i < instructions.Count; ++i) {
          var instr = instructions[i];
          if (IsMakeRefOrMakePtrCall(instr)) {
            Log.Debug($"Property {property} has MakePtr/MakeRef init");
            // found it!
            if (i == 0) {
              // seems we're dealing with an empty MakeRef/MakePtr
              return null;
            } else {
              return new ReadOnlyInitializer() {
                Instructions = instructions.Take(i).ToArray(),
                Variables = property.GetMethod.Body.Variables.ToArray()
              };
            }
          }
        }
      }
      return null;
    }

    private (Instruction[], VariableDefinition[]) CloneInstructions(Instruction[] source, VariableDefinition[] sourceVariables) {

      var constructor = typeof(Instruction).GetConstructor(
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null,
        new[] { typeof(OpCode), typeof(object) }, null);

      // shallow copy
      var result = source.Select(x => (Instruction)constructor.Invoke(new[] { x.OpCode, x.Operand }))
        .ToArray();

      var variableMapping = new Dictionary<VariableDefinition, VariableDefinition>();

      // now need to resolve local variables and jump targets
      foreach (var instruction in result) {

        if (instruction.IsLdlocWithIndex(out var locIndex) || instruction.IsStlocWithIndex(out locIndex)) {
          var variable = sourceVariables[locIndex];
          if (!variableMapping.TryGetValue(variable, out var replacement)) {
            replacement = new VariableDefinition(variable.VariableType);
            variableMapping.Add(variable, replacement);
          }
          if (instruction.IsLdlocWithIndex(out _)) {
            instruction.OpCode = OpCodes.Ldloc;
          } else {
            instruction.OpCode = OpCodes.Stloc;
          }
          instruction.Operand = replacement;
        } else if (instruction.Operand is VariableDefinition variable) {
          if (!variableMapping.TryGetValue(variable, out var replacement)) {
            replacement = new VariableDefinition(variable.VariableType);
            variableMapping.Add(variable, replacement);
          }
          instruction.Operand = replacement;
        } else if (instruction.Operand is Instruction target) {
          var targetIndex = Array.IndexOf(source, target);
          Log.Assert(targetIndex >= 0);
          instruction.Operand = result[targetIndex];
        } else if (instruction.Operand is Instruction[] targets) {
          instruction.Operand = targets.Select(x => {
            var targetIndex = Array.IndexOf(source, x);
            Log.Assert(targetIndex >= 0);
            return result[targetIndex];
          });
        } else if (instruction.Operand is ParameterDefinition) {
          throw new NotSupportedException();
        }
      }

      return (result, variableMapping.Values.ToArray());
    }
  }
}
#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverAssembly.cs

#if FUSION_WEAVER && FUSION_HAS_MONO_CECIL
namespace Fusion.CodeGen {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using Assembly = UnityEditor.Compilation.Assembly;

  using Mono.Cecil;

  public class ILWeaverImportedType {
    public Type                 ClrType;
    public ILWeaverAssembly     Assembly;
    public List<TypeDefinition> BaseDefinitions;
    public TypeReference        Reference;

    Dictionary<string, FieldReference>  _fields        = new Dictionary<string, FieldReference>();
    Dictionary<(string, int?), MethodReference> _methods = new Dictionary<(string, int?), MethodReference>();
    Dictionary<string, MethodReference> _propertiesGet = new Dictionary<string, MethodReference>();
    Dictionary<string, MethodReference> _propertiesSet = new Dictionary<string, MethodReference>();

    public static implicit operator TypeReference(ILWeaverImportedType type) => type.Reference;

    public ILWeaverImportedType(ILWeaverAssembly asm, Type type) {
      ClrType    = type;
      Assembly   = asm;

      Type baseType = type;
      BaseDefinitions = new List<TypeDefinition>();
      // Store the type, and each of its base types - so we can later find fields/properties/methods in the base class.
      while (baseType != null) {
        BaseDefinitions.Add(asm.CecilAssembly.MainModule.ImportReference(baseType).Resolve());
        baseType = baseType.BaseType;
      }

      Reference = asm.CecilAssembly.MainModule.ImportReference(BaseDefinitions[0]);
    }

    public FieldReference GetField(string name) {
      bool found = _fields.TryGetValue(name, out var fieldRef);
      if (found == false) {
        for (int i = 0; i < BaseDefinitions.Count; ++i) {
          FieldDefinition typeDef = BaseDefinitions[i].Fields.FirstOrDefault(x => x.Name == name);
          if (typeDef != null) {
            fieldRef = Assembly.CecilAssembly.MainModule.ImportReference(typeDef);
            _fields.Add(name, fieldRef);
            return fieldRef;
          }
        }
      }
      return fieldRef;
    }

    public MethodReference GetProperty(string name) {
      bool found = _propertiesGet.TryGetValue(name, out var methRef);
      if (found == false) {
        for (int i = 0; i < BaseDefinitions.Count; ++i) {
          PropertyDefinition typeDef = BaseDefinitions[i].Properties.FirstOrDefault(x => x.Name == name);
          if (typeDef != null) {
            methRef = Assembly.CecilAssembly.MainModule.ImportReference(typeDef.GetMethod);
            _propertiesGet.Add(name, methRef);
            return methRef;
          }
        }
      }
      return methRef;
    }

    public MethodReference SetProperty(string name) {
      bool found = _propertiesSet.TryGetValue(name, out var methRef);
      if (found == false) {
        for (int i = 0; i < BaseDefinitions.Count; ++i) {
          PropertyDefinition def = BaseDefinitions[i].Properties.FirstOrDefault(x => x.Name == name);
          if (def != null) {
            methRef = Assembly.CecilAssembly.MainModule.ImportReference(def.SetMethod);
            _propertiesSet.Add(name, methRef);
            return methRef;
          }
        }
      }
      return methRef;
    }

    public MethodReference GetMethod(string name, int? argsCount = null, int? genericArgsCount = null) {
      bool found = _methods.TryGetValue((name, argsCount), out var methRef);
      if (found == false) {
        for (int i = 0; i < BaseDefinitions.Count; ++i) {
          
          var typeDef = BaseDefinitions[i].Methods.FirstOrDefault(
            x => x.Name == name &&
            (argsCount.HasValue == false || x.Parameters.Count == argsCount.Value) &&
            (genericArgsCount == null || x.GenericParameters.Count == genericArgsCount.Value));

          if (typeDef != null) {
            methRef = Assembly.CecilAssembly.MainModule.ImportReference(typeDef);
            _methods.Add((name, argsCount), methRef);
            return methRef;
          }
        }
      }
      if (methRef == null) {
        throw new InvalidOperationException($"Not found: {name}");
      }
      return methRef;
    }


    public GenericInstanceMethod GetGenericMethod(string name, int? argsCount = null, params TypeReference[] types) {
      var method  = GetMethod(name, argsCount);
      var generic = new GenericInstanceMethod(method);

      foreach (var t in types) {
        generic.GenericArguments.Add(t);
      }

      return generic;
    }
  }

  public class ILWeaverAssembly {
    public bool         Modified;
    public List<String> Errors = new List<string>();

    public AssemblyDefinition CecilAssembly;

    ILWeaverImportedType _networkRunner;
    ILWeaverImportedType _readWriteUtils;
    ILWeaverImportedType _nativeUtils;
    ILWeaverImportedType _rpcInfo;
    ILWeaverImportedType _rpcInvokeInfo;
    ILWeaverImportedType _rpcHeader;
    ILWeaverImportedType _networkBehaviourUtils;

    ILWeaverImportedType _simulation;
    ILWeaverImportedType _networkedObject;
    ILWeaverImportedType _networkedObjectId;
    ILWeaverImportedType _networkedBehaviour;
    ILWeaverImportedType _networkedBehaviourId;
    ILWeaverImportedType _simulationBehaviour;
    ILWeaverImportedType _simulationMessage;

    ILWeaverImportedType _object;
    ILWeaverImportedType _valueType;
    ILWeaverImportedType _void;
    ILWeaverImportedType _int;
    ILWeaverImportedType _float;

    Dictionary<Type, TypeReference> _types = new Dictionary<Type, TypeReference>();

    private ILWeaverImportedType MakeImportedType<T>(ref ILWeaverImportedType field) {
      return MakeImportedType(ref field, typeof(T));
    }

    private ILWeaverImportedType MakeImportedType(ref ILWeaverImportedType field, Type type) {
      if (field == null) {
        field = new ILWeaverImportedType(this, type);
      }
      return field;
    }

    public ILWeaverImportedType WordSizedPrimitive => MakeImportedType<int>(ref _int);

    public ILWeaverImportedType Void => MakeImportedType(ref _void, typeof(void));

    public ILWeaverImportedType Object => MakeImportedType<object>(ref _object);

    public ILWeaverImportedType ValueType => MakeImportedType<ValueType>(ref _valueType);

    public ILWeaverImportedType Float => MakeImportedType<float>(ref _float);

    public ILWeaverImportedType NetworkedObject => MakeImportedType<NetworkObject>(ref _networkedObject);

    public ILWeaverImportedType Simulation => MakeImportedType<Simulation>(ref _simulation);

    public ILWeaverImportedType SimulationMessage => MakeImportedType<SimulationMessage>(ref _simulationMessage);

    public ILWeaverImportedType NetworkedBehaviour => MakeImportedType<NetworkBehaviour>(ref _networkedBehaviour);

    public ILWeaverImportedType SimulationBehaviour => MakeImportedType<SimulationBehaviour>(ref _simulationBehaviour);

    public ILWeaverImportedType NetworkId => MakeImportedType<NetworkId>(ref _networkedObjectId);

    public ILWeaverImportedType NetworkedBehaviourId => MakeImportedType<NetworkBehaviourId>(ref _networkedBehaviourId);

    public ILWeaverImportedType NetworkRunner => MakeImportedType<NetworkRunner>(ref _networkRunner);

    public ILWeaverImportedType ReadWriteUtils => MakeImportedType(ref _readWriteUtils, typeof(ReadWriteUtilsForWeaver));
    
    public ILWeaverImportedType Native => MakeImportedType(ref _nativeUtils, typeof(Native));

    public ILWeaverImportedType NetworkBehaviourUtils => MakeImportedType(ref _networkBehaviourUtils, typeof(NetworkBehaviourUtils));

    public ILWeaverImportedType RpcHeader => MakeImportedType<RpcHeader>(ref _rpcHeader);

    public ILWeaverImportedType RpcInfo => MakeImportedType<RpcInfo>(ref _rpcInfo);

    public ILWeaverImportedType RpcInvokeInfo => MakeImportedType<RpcInvokeInfo>(ref _rpcInvokeInfo);

    public TypeReference Import(TypeReference type) {
      return CecilAssembly.MainModule.ImportReference(type);
    }

    public MethodReference Import(MethodInfo method) {
      return CecilAssembly.MainModule.ImportReference(method);
    }

    public MethodReference Import(MethodReference method) {
      return CecilAssembly.MainModule.ImportReference(method);
    }

    public MethodReference Import(ConstructorInfo method) {
      return CecilAssembly.MainModule.ImportReference(method);
    }

    public TypeReference Import(Type type) {
      if (_types.TryGetValue(type, out var reference) == false) {
        _types.Add(type, reference = CecilAssembly.MainModule.ImportReference(type));
      }

      return reference;
    }

    public void Dispose() {
      CecilAssembly?.Dispose();

      Modified = false;
      Errors.Clear();
      CecilAssembly    = null;
    }

    public TypeReference Import<T>() {
      return Import(typeof(T));
    }
  }
}
#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverAssemblyResolver.ILPostProcessor.cs

#if FUSION_WEAVER && FUSION_WEAVER_ILPOSTPROCESSOR && FUSION_HAS_MONO_CECIL

namespace Fusion.CodeGen {

  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using Mono.Cecil;

  internal class ILWeaverAssemblyResolver : IAssemblyResolver {
    private List<string> _lookInDirectories;
    private Dictionary<string, string> _assemblyNameToPath;
    private Dictionary<string, AssemblyDefinition> _resolvedAssemblies = new Dictionary<string, AssemblyDefinition>();
    private string _compiledAssemblyName;
    private ILWeaverLog _log;

    public AssemblyDefinition WeavedAssembly;

    public ILWeaverAssemblyResolver(ILWeaverLog log, string compiledAssemblyName, string[] references) {
      _log                  = log;
      _compiledAssemblyName = compiledAssemblyName;
      _assemblyNameToPath   = new Dictionary<string, string>();

      foreach (var referencePath in references) {
        var assemblyName = Path.GetFileNameWithoutExtension(referencePath);
        if (_assemblyNameToPath.TryGetValue(assemblyName, out var existingPath)) {
          _log.Warn($"Assembly {assemblyName} (full path: {referencePath}) already referenced by {compiledAssemblyName} at {existingPath}");
        } else {
          _assemblyNameToPath.Add(assemblyName, referencePath);
        }
      }

      _lookInDirectories = references.Select(x => Path.GetDirectoryName(x)).Distinct().ToList();
    }

    public void Dispose() {
    }

    public AssemblyDefinition Resolve(AssemblyNameReference name) {
      return Resolve(name, new ReaderParameters(ReadingMode.Deferred));
    }

    public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) {
      {
        if (name.Name == _compiledAssemblyName)
          return WeavedAssembly;

        var path = GetAssemblyPath(name);
        if (string.IsNullOrEmpty(path))
          return null;

        if (_resolvedAssemblies.TryGetValue(path, out var result))
          return result;

        parameters.AssemblyResolver = this;

        var pdb = path + ".pdb";
        if (File.Exists(pdb)) {
          parameters.SymbolStream = CreateAssemblyStream(pdb);
        }

        var assemblyDefinition = AssemblyDefinition.ReadAssembly(CreateAssemblyStream(path), parameters);
        _resolvedAssemblies.Add(path, assemblyDefinition);
        return assemblyDefinition;
      }
    }

    private string GetAssemblyPath(AssemblyNameReference name) {
      if (_assemblyNameToPath.TryGetValue(name.Name, out var path)) {
        return path;
      }

      // fallback for second-order references
      foreach (var parentDir in _lookInDirectories) {
        var fullPath = Path.Combine(parentDir, name.Name + ".dll");
        if (File.Exists(fullPath)) {
          _assemblyNameToPath.Add(name.Name, fullPath);
          return fullPath;
        }
      }

      return null;
    }

    private static MemoryStream CreateAssemblyStream(string fileName) {
      var bytes = File.ReadAllBytes(fileName);
      return new MemoryStream(bytes);
    }
  }
}

#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverAssemblyResolver.UnityEditor.cs

#if FUSION_WEAVER && !FUSION_WEAVER_ILPOSTPROCESSOR && FUSION_HAS_MONO_CECIL

namespace Fusion.CodeGen {
  using System;
  using System.Collections.Generic;
  using Mono.Cecil;
  using CompilerAssembly = UnityEditor.Compilation.Assembly;

  class ILWeaverAssemblyResolver : BaseAssemblyResolver {
    Dictionary<string, ILWeaverAssembly> _assemblies;
    Dictionary<string, ILWeaverAssembly> _assembliesByPath;

    public IEnumerable<ILWeaverAssembly> Assemblies => _assemblies.Values;

    public ILWeaverAssemblyResolver() {
      _assemblies = new Dictionary<string, ILWeaverAssembly>(StringComparer.Ordinal);
      _assembliesByPath = new Dictionary<string, ILWeaverAssembly>();
    }

    public sealed override AssemblyDefinition Resolve(AssemblyNameReference name) {
      if (_assemblies.TryGetValue(name.FullName, out var asm) == false) {
        asm = new ILWeaverAssembly();
        asm.CecilAssembly = base.Resolve(name, ReaderParameters(false, false));

        _assemblies.Add(name.FullName, asm);
      }

      return asm.CecilAssembly;
    }

    public void Clear() {
      _assemblies.Clear();
    }

    public bool Contains(CompilerAssembly compilerAssembly) {
      return _assembliesByPath.ContainsKey(compilerAssembly.outputPath);
    }

    public ILWeaverAssembly AddAssembly(string path, bool readWrite = true, bool readSymbols = true) {
      return AddAssembly(AssemblyDefinition.ReadAssembly(path, ReaderParameters(readWrite, readSymbols)), null);
    }

    public ILWeaverAssembly AddAssembly(CompilerAssembly compilerAssembly, bool readWrite = true, bool readSymbols = true) {
      return AddAssembly(AssemblyDefinition.ReadAssembly(compilerAssembly.outputPath, ReaderParameters(readWrite, readSymbols)), compilerAssembly);
    }

    public ILWeaverAssembly AddAssembly(AssemblyDefinition assembly, CompilerAssembly compilerAssembly) {
      if (assembly == null) {
        throw new ArgumentNullException(nameof(assembly));
      }

      if (_assemblies.TryGetValue(assembly.Name.FullName, out var asm) == false) {
        asm = new ILWeaverAssembly();
        asm.CecilAssembly = assembly;

        _assemblies.Add(assembly.Name.FullName, asm);

        if (compilerAssembly != null) {
          Assert.Always(_assembliesByPath.ContainsKey(compilerAssembly.outputPath) == false);
          _assembliesByPath.Add(compilerAssembly.outputPath, asm);
        }
      }

      return asm;
    }

    protected override void Dispose(bool disposing) {
      foreach (var asm in _assemblies.Values) {
        asm.CecilAssembly?.Dispose();
      }

      _assemblies.Clear();

      base.Dispose(disposing);
    }

    ReaderParameters ReaderParameters(bool readWrite, bool readSymbols) {
      ReaderParameters p;
      p = new ReaderParameters(ReadingMode.Immediate);
      p.ReadWrite = readWrite;
      p.ReadSymbols = readSymbols;
      p.AssemblyResolver = this;
      return p;
    }
  }
}

#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverBindings.ILPostProcessor.cs

#if FUSION_WEAVER_ILPOSTPROCESSOR
namespace Fusion.CodeGen {
  using System;
  using Unity.CompilationPipeline.Common.ILPostProcessing;

#if FUSION_WEAVER
  using System.Collections.Generic;
  using Unity.CompilationPipeline.Common.Diagnostics;
#if FUSION_HAS_MONO_CECIL
  using System.IO;
  using System.Linq;
  using Mono.Cecil;
  using Mono.Cecil.Cil;
  using System.Reflection;

  class ILWeaverBindings : ILPostProcessor {

    public override ILPostProcessor GetInstance() {
      return this;
    }

    static ILWeaverAssembly CreateWeaverAssembly(ILWeaverLog log, ICompiledAssembly compiledAssembly) {
      var resolver = new ILWeaverAssemblyResolver(log, compiledAssembly.Name, compiledAssembly.References);

      var readerParameters = new ReaderParameters {
        SymbolStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData.ToArray()),
        SymbolReaderProvider = new PortablePdbReaderProvider(),
        AssemblyResolver = resolver,
        ReadingMode = ReadingMode.Immediate,
        ReadWrite = true,
        ReadSymbols = true,
        ReflectionImporterProvider = new ReflectionImporterProvider(log)
      };

      var peStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PeData.ToArray());
      var assemblyDefinition = AssemblyDefinition.ReadAssembly(peStream, readerParameters);

      resolver.WeavedAssembly = assemblyDefinition;

      return new ILWeaverAssembly() {
        CecilAssembly = assemblyDefinition,
      };
    }

    public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly) {

      ILPostProcessResult result;
      var log = new ILWeaverLog();


      if (!ILWeaverSettings.ValidateConfig(out var status, out var readException)) {
        string message;
        DiagnosticType messageType;

        switch (status) {
          case ILWeaverSettings.ConfigStatus.NotFound: {

              var candidates = Directory.GetFiles("Assets", "*.fusion", SearchOption.AllDirectories);

              message = $"Fusion ILWeaver config error: {nameof(NetworkProjectConfig)} not found at {ILWeaverSettings.NetworkProjectConfigPath}. " +
                $"Implement {nameof(ILWeaverSettings)}.OverrideNetworkProjectConfigPath in Fusion.CodeGen.User.cs to change the config's location.";

              if (candidates.Any()) {
                message += $" Possible candidates are: {(string.Join(", ", candidates))}.";
              }

              messageType = DiagnosticType.Warning;
            }
            break;

          case ILWeaverSettings.ConfigStatus.ReadException:
            message = $"Fusion ILWeaver config error: reading file {ILWeaverSettings.NetworkProjectConfigPath} failed: {readException.Message}";
            messageType = DiagnosticType.Error;
            break;

          default:
            throw new NotSupportedException(status.ToString());
        }

        return new ILPostProcessResult(null, new List<DiagnosticMessage>() {
          new DiagnosticMessage() {
            DiagnosticType = messageType,
            MessageData = message,
          }
        }
        );
      }

      using (log.Scope($"Process {compiledAssembly.Name}")) {
        InMemoryAssembly resultAssembly = null;

        try {
          ILWeaverAssembly asm;
          ILWeaver weaver;

          using (log.Scope("Resolving")) {
            asm = CreateWeaverAssembly(log, compiledAssembly);
          }

          using (log.Scope("Init")) { 
            weaver = new ILWeaver(log);
          }

          weaver.Weave(asm);

          if (asm.Modified) {
            var pe = new MemoryStream();
            var pdb = new MemoryStream();
            var writerParameters = new WriterParameters {
              SymbolWriterProvider = new PortablePdbWriterProvider(),
              SymbolStream = pdb,
              WriteSymbols = true
            };

            using (log.Scope("Writing")) {
              asm.CecilAssembly.Write(pe, writerParameters);
              resultAssembly = new InMemoryAssembly(pe.ToArray(), pdb.ToArray());
            }
          }
        } catch (Exception ex) {
          log.Error($"Exception thrown when weaving {compiledAssembly.Name}");
          log.Exception(ex);
        } finally {
          log.FixNewLinesInMessages();
          result = new ILPostProcessResult(resultAssembly, log.Messages);
        }
      }

      return result;
    }


    public override bool WillProcess(ICompiledAssembly compiledAssembly) {

      if (!ILWeaverSettings.ValidateConfig(out _, out _)) {
        // need to go to the next stage for some assembly, main is good enough
        return compiledAssembly.Name == "Assembly-CSharp";
      }

      if (!ILWeaverSettings.IsAssemblyWeavable(compiledAssembly.Name)) {
        return false;
      }

      if (!ILWeaverSettings.ContainsRequiredReferences(compiledAssembly.References)) {
        return false;
      }

      return true;
    }

    class ReflectionImporterProvider : IReflectionImporterProvider {
      private ILWeaverLog _log;

      public ReflectionImporterProvider(ILWeaverLog log) {
        _log = log;
      }
      
      public IReflectionImporter GetReflectionImporter(ModuleDefinition module) {
        return new ReflectionImporter(_log, module);
      }
    }

    class ReflectionImporter : DefaultReflectionImporter {
      private ILWeaverLog _log;
      
      public ReflectionImporter(ILWeaverLog log, ModuleDefinition module) : base(module) {
        _log = log;
      }

      public override AssemblyNameReference ImportReference(AssemblyName name) {
        if (name.Name == "System.Private.CoreLib") {
          // seems weaver is run with .net core,  but we need to stick to .net framework
          var candidates = module.AssemblyReferences
            .Where(x => x.Name == "mscorlib" || x.Name == "netstandard")
            .OrderBy(x => x.Name)
            .ThenByDescending(x => x.Version)
            .ToList();

          // in Unity 2020.1 and .NET 4.x mode when building with IL2CPP apparently both mscrolib and netstandard can be loaded
          if (candidates.Count > 0) {
            return candidates[0];
          }
          
          throw new ILWeaverException("Could not locate mscrolib or netstandard assemblies");
        }
        
        return base.ImportReference(name);
      }
    }
  }
#else
  class ILWeaverBindings : ILPostProcessor {
    public override ILPostProcessor GetInstance() {
      return this;
    }

    public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly) {
      return new ILPostProcessResult(null, new List<DiagnosticMessage>() {
        new DiagnosticMessage() {
          DiagnosticType = DiagnosticType.Warning,
          MessageData = "Mono.Cecil not found, Fusion IL weaving is disabled. Make sure package com.unity.nuget.mono-cecil is installed."
        }
      });
    }

    public override bool WillProcess(ICompiledAssembly compiledAssembly) {
      return compiledAssembly.Name == "Assembly-CSharp";
    }
  }
#endif
#else
  class ILWeaverBindings : ILPostProcessor {
    public override ILPostProcessor GetInstance() {
      return this;
    }

    public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly) {
      throw new NotImplementedException();
    }

    public override bool WillProcess(ICompiledAssembly compiledAssembly) {
      return false;
    }
  }
#endif
}
#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverBindings.UnityEditor.cs

#if FUSION_WEAVER && !FUSION_WEAVER_ILPOSTPROCESSOR

namespace Fusion.CodeGen {
#if FUSION_HAS_MONO_CECIL
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using Mono.Cecil;
  using UnityEditor;
  using UnityEditor.Build;
  using UnityEditor.Build.Reporting;
  using UnityEditor.Compilation;
  using CompilerAssembly = UnityEditor.Compilation.Assembly;

  class ILWeaverBindings {

    static ILWeaver _weaver;


    [UnityEditor.InitializeOnLoadMethod]
    public static void InitializeOnLoad() {
      CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;
      EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state) {
      var projectConfig = NetworkProjectConfig.Global;

      // exit edit mode means play mode is about to start ...
      if (state == PlayModeStateChange.ExitingEditMode) {
        foreach (var assembly in CompilationPipeline.GetAssemblies()) {
          var name = Path.GetFileNameWithoutExtension(assembly.outputPath);
          if (ILWeaverSettings.IsAssemblyWeavable(name)) {
            OnCompilationFinished(assembly.outputPath, new CompilerMessage[0]);
          }
        }
      }
    }

    static void OnCompilationFinished(string path, CompilerMessage[] messages) {
#if FUSION_DEV
      Stopwatch sw = Stopwatch.StartNew();
      Log.Debug($"OnCompilationFinished({path})");
#endif

      // never modify editor assemblies
      if (ILWeaver.IsEditorAssemblyPath(path)) {
        return;
      }

      var projectConfig = NetworkProjectConfig.Global;
      if (projectConfig != null) {
        // name of assembly on disk
        var name = Path.GetFileNameWithoutExtension(path);
        if (!ILWeaverSettings.IsAssemblyWeavable(name)) {
          return;
        }
      }

      // errors means we should exit
      if (messages.Any(x => x.type == CompilerMessageType.Error)) {
#if FUSION_DEV
        Log.Error($"Can't execute ILWeaver on {path}, compilation errors exist.");
#endif
        return;
      }

      // grab compiler pipe assembly
      var asm = CompilationPipeline.GetAssemblies().First(x => x.outputPath == path);

      // needs to reference phoenix runtime
      if (ILWeaverSettings.ContainsRequiredReferences(asm.allReferences) == false) {
        return;
      }

      // perform weaving
      try {
        _weaver = _weaver ?? new ILWeaver(new ILWeaverLog());
        Weave(_weaver, asm);
      } catch (Exception ex) {
        UnityEngine.Debug.LogError(ex);
      }

#if FUSION_DEV
      UnityEngine.Debug.Log($"OnCompilationFinished took: {sw.Elapsed}");
#endif
    }


    static void Weave(ILWeaver weaver, Assembly compilerAssembly) {
      using (weaver.Log.Scope("Processing")) {

        using (var resolver = new ILWeaverAssemblyResolver()) {
          // if we're already weaving this don't do anything
          if (resolver.Contains(compilerAssembly)) {
            return;
          }

          // make sure we can load all dlls
          foreach (string path in compilerAssembly.allReferences) {
            resolver.AddSearchDirectory(Path.GetDirectoryName(path));
          }

          // make sure we have the runtime dll loaded
          if (!ILWeaverSettings.ContainsRequiredReferences(compilerAssembly.allReferences)) {
            throw new InvalidOperationException($"Weaving: Could not find required assembly references");
          }

          ILWeaverAssembly asm;

          using (weaver.Log.Scope("Resolving")) {
            asm = resolver.AddAssembly(compilerAssembly);
          }

          if (weaver.Weave(asm)) {

            using (weaver.Log.Scope("Writing")) {
              // write asm to disk
              asm.CecilAssembly.Write(new WriterParameters {
                WriteSymbols = true
              });
            }
          }
        }
      }
    }
  }
#else
  class ILWeaverBindings {
    [UnityEditor.InitializeOnLoadMethod]
    public static void InitializeOnLoad() {
      UnityEngine.Debug.LogError("Mono.Cecil not found, Fusion IL weaving is disabled. Make sure package com.unity.nuget.mono-cecil is installed.");
    }
  }
#endif
}
#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverException.cs

namespace Fusion.CodeGen {
  using System;
  using System.Diagnostics;

  public class ILWeaverException : Exception {
    public ILWeaverException(string error) : base(error) {
    }

    public ILWeaverException(string error, Exception innerException) : base(error, innerException) {
    }


    [Conditional("UNITY_EDITOR")]
    public static void DebugThrowIf(bool condition, string message) {
      if (condition) {
        throw new ILWeaverException(message);
      }
    }
  }
}

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverExtensions.cs

#if FUSION_WEAVER && FUSION_HAS_MONO_CECIL
namespace Fusion.CodeGen {

  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.CompilerServices;
  using System.Text;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;
  using Mono.Cecil;
  using Mono.Cecil.Cil;
  using Mono.Cecil.Rocks;

  public static class ILWeaverExtensions {

    public static bool IsIntegral(this TypeReference type) {
      switch (type.MetadataType) {
        case MetadataType.Byte:
        case MetadataType.SByte:
        case MetadataType.UInt16:
        case MetadataType.Int16:
        case MetadataType.UInt32:
        case MetadataType.Int32:
        case MetadataType.UInt64:
        case MetadataType.Int64:
          return true;
        default:
          return false;
      }
    }

    public static int GetPrimitiveSize(this TypeReference type) {
      switch (type.MetadataType) {
        case MetadataType.Byte:
        case MetadataType.SByte:
          return sizeof(byte);
        case MetadataType.UInt16:
        case MetadataType.Int16:
          return sizeof(short);
        case MetadataType.UInt32:
        case MetadataType.Int32:
          return sizeof(int);
        case MetadataType.UInt64:
        case MetadataType.Int64:
          return sizeof(long);
        case MetadataType.Single:
          return sizeof(float);
        case MetadataType.Double:
          return sizeof(double);
        default:
          throw new ArgumentException($"Unknown primitive type: {type}", nameof(type));
      }
    }

    public static bool IsString(this TypeReference type) {
      return type.MetadataType == MetadataType.String;
    }

    public static bool IsFloat(this TypeReference type) {
      return type.MetadataType == MetadataType.Single;
    }

    public static bool IsBool(this TypeReference type) {
      return type.MetadataType == MetadataType.Boolean;
    }

    public static bool IsVector2(this TypeReference type) {
      return type.FullName == typeof(UnityEngine.Vector2).FullName;
    }

    public static bool IsVector3(this TypeReference type) {
      return type.FullName == typeof(UnityEngine.Vector3).FullName;
    }

    public static bool IsQuaternion(this TypeReference type) {
      return type.FullName == typeof(UnityEngine.Quaternion).FullName;
    }

    public static bool IsVoid(this TypeReference type) {
      return type.MetadataType == MetadataType.Void;
    }

    public static TypeReference GetElementTypeEx(this TypeReference type) {
      if (type.IsPointer) {
        return ((Mono.Cecil.PointerType)type).ElementType;
      } else if (type.IsByReference) {
        return ((Mono.Cecil.ByReferenceType)type).ElementType;
      } else {
        return type.GetElementType();
      }
    }

    public static bool IsSubclassOf<T>(this TypeReference type) {
      return !IsSame<T>(type) && Is<T>(type);
    }

    public static bool IsGeneric(this TypeReference type, Type typeDefinition) {
      if (typeDefinition.IsGenericTypeDefinition == false) {
        throw new InvalidOperationException();
      }
      if (!type.IsGenericInstance) {
        return false;
      }

      return type.GetElementType().FullName == typeDefinition.FullName;
    }

    public static bool IsNetworkList(this TypeReference type) {
      return IsNetworkList(type, out _);
    }
    
    public static bool IsNetworkList(this TypeReference type, out TypeReference elementType) {
      if (!type.IsGenericInstance || type.GetElementType().FullName != typeof(NetworkLinkedList<>).FullName) {
        elementType = default;
        return false;
      }

      var git = (GenericInstanceType)type;
      elementType = git.GenericArguments[0];
      return true;
    }


    public static bool IsNetworkArray(this TypeReference type) {
      return IsNetworkArray(type, out _);
    }

    public static bool IsNetworkArray(this TypeReference type, out TypeReference elementType) {
      if (!type.IsGenericInstance || type.GetElementType().FullName != typeof(NetworkArray<>).FullName) {
        elementType = default;
        return false;
      }

      var git = (GenericInstanceType)type;
      elementType = git.GenericArguments[0];
      return true;
    }


    public static bool IsNetworkDictionary(this TypeReference type) {
      return IsNetworkDictionary(type, out _, out _);
    }

    public static bool IsNetworkDictionary(this TypeReference type, out TypeReference keyType, out TypeReference valueType) {
      if (!type.IsGenericInstance || type.GetElementType().FullName != typeof(NetworkDictionary<,>).FullName) {
        keyType = default;
        valueType = default;
        return false;
      }

      var git = (GenericInstanceType)type;
      keyType = git.GenericArguments[0];
      valueType = git.GenericArguments[1];
      return true;
    }

    public static bool Is<T>(this TypeReference type) {
      return Is(type, typeof(T));
    }

    public static bool Is(this TypeReference type, Type t) {
      if (IsSame(type, t)) {
        return true;
      }

      var resolvedType = TryResolve(type);

      if (t.IsInterface) {
        if (resolvedType == null) {
          return false;
        }

        foreach (var interf in resolvedType.Interfaces) {
          if (interf.InterfaceType.IsSame(t)) {
            return true;
          }
        }
        return false;
      } else {
        if (resolvedType?.BaseType == null) {
          return false;
        }
        return Is(resolvedType.BaseType, t);
      }
    }

    public static bool Is(this TypeReference type, TypeReference t) {
      if (IsSame(type, t)) {
        return true;
      }

      var resolvedType = TryResolve(type);

      if (t.TryResolve()?.IsInterface == true) {
        if (resolvedType == null) {
          return false;
        }

        foreach (var interf in resolvedType.Interfaces) {
          if (interf.InterfaceType.IsSame(t)) {
            return true;
          }
        }
        return false;
      } else {
        if (resolvedType?.BaseType == null) {
          return false;
        }
        return Is(resolvedType.BaseType, t);
      }
    }

    public static bool IsSame<T>(this TypeReference type) {
      return IsSame(type, typeof(T));
    }

    public static bool IsSame(this TypeReference type, Type t) {
      if (type == null) {
        throw new ArgumentNullException(nameof(type));
      }
      if (type.IsByReference) {
        type = type.GetElementType();
      }
      if (type.IsVoid() && t == typeof(void)) {
        return true;
      }
      if (type.IsValueType != t.IsValueType) {
        return false;
      }
      if (type.FullName != t.FullName) {
        return false;
      }
      return true;
    }

    public static bool IsSame(this TypeReference type, TypeOrTypeRef t) {
      if (t.Type != null) {
        return IsSame(type, t.Type);
      } else if (t.TypeReference != null) {
        return IsSame(type, t.TypeReference);
      } else {
        throw new InvalidOperationException();
      }
    }

    public static bool IsSame(this TypeReference type, TypeReference t) {
      if (type == null) {
        throw new ArgumentNullException(nameof(type));
      }
      if (type.IsByReference) {
        type = type.GetElementType();
      }
      if (type.IsValueType != t.IsValueType) {
        return false;
      }
      if (type.FullName != t.FullName) {
        return false;
      }
      return true;
    }


    public static IEnumerable<TypeDefinition> GetHierarchy(this TypeDefinition type, TypeReference stopAtBaseType = null) {
      if (stopAtBaseType?.IsSame(type) == true) {
        yield break;
      }

      for (; ; ) {
        yield return type;
        if (type.BaseType == null || stopAtBaseType?.IsSame(type.BaseType) == true) {
          break;
        }
        type = type.BaseType.Resolve();
      }
    }

    public static TypeDefinition TryResolve(this TypeReference type) {
      try {
        return type.Resolve();
      } catch {
        return null;
      }
    }

    public static bool IsFixedBuffer(this TypeReference type, out int size) {
      size = default;
      if (!type.IsValueType) {
        return false;
      }

      if (!type.Name.EndsWith("e__FixedBuffer")) {
        return false;
      }

      var definition = TryResolve(type);
      if (definition == null) {
        return false;
      }

      // this is a bit of a guesswork
      if (HasAttribute<CompilerGeneratedAttribute>(definition) &&
          HasAttribute<UnsafeValueTypeAttribute>(definition) &&
          definition.ClassSize > 0) {
        size = definition.ClassSize;
        return true;
      }

      return false;
    }

    public static bool HasAttribute<T>(this ICustomAttributeProvider type) where T : Attribute {
      for (int i = 0; i < type.CustomAttributes.Count; ++i) {
        if (type.CustomAttributes[i].AttributeType.Name == typeof(T).Name) {
          return true;
        }
      }

      return false;
    }

    public static bool TryGetAttribute<T>(this ICustomAttributeProvider type, out CustomAttribute attribute) where T : Attribute {
      for (int i = 0; i < type.CustomAttributes.Count; ++i) {
        if (type.CustomAttributes[i].AttributeType.Name == typeof(T).Name) {
          attribute = type.CustomAttributes[i];
          return true;
        }
      }

      attribute = null;
      return false;
    }

    public static T GetAttributeArgument<T>(this CustomAttribute attr, int index) {
      return (T)attr.ConstructorArguments[index].Value;
    }

    public static bool TryGetAttributeArgument<T>(this CustomAttribute attr, int index, out T value, T defaultValue = default) {
      if (index < attr.ConstructorArguments.Count) {
        if (attr.ConstructorArguments[index].Value is T t) {
          value = t;
          return true;
        }
      }

      value = defaultValue;
      return false;
    }

    public static T GetAttributeProperty<T>(this CustomAttribute attr, string name, T defaultValue = default) {
      attr.TryGetAttributeProperty(name, out var result, defaultValue);
      return result;
    }

    public static bool TryGetAttributeProperty<T>(this CustomAttribute attr, string name, out T value, T defaultValue = default) {
      if (attr.HasProperties) {
        var prop = attr.Properties.FirstOrDefault(x => x.Name == name);

        if (prop.Argument.Value != null) {
          value = (T)prop.Argument.Value;
          return true;
        }
      }

      value = defaultValue;
      return false;
    }

    static bool TryGetMatchingMethod(IEnumerable<MethodDefinition> methods, IList<ParameterDefinition> parameters, out MethodDefinition result) {
      foreach (var c in methods) {
        if (c.Parameters.Count != parameters.Count) {
          continue;
        }
        int i;
        for (i = 0; i < c.Parameters.Count; ++i) {
          if (!c.Parameters[i].ParameterType.IsSame(parameters[i].ParameterType)) {
            break;
          }
        }

        if (i == c.Parameters.Count) {
          result = c;
          return true;
        }
      }

      result = null;
      return false;
    }

    public static bool TryGetMatchingConstructor(this TypeDefinition type, MethodDefinition constructor, out MethodDefinition matchingConstructor) {
      return TryGetMatchingMethod(type.GetConstructors(), constructor.Parameters, out matchingConstructor);
    }

    public static bool TryGetMethod(this TypeDefinition type, string methodName, IList<ParameterDefinition> parameters, out MethodDefinition method) {
      var methods = type.Methods.Where(x => x.Name == methodName);
      
      if (TryGetMatchingMethod(methods, parameters, out method)) {
        return true;
      }

      //if (type.BaseType != null) {
      //  if (stopAtBaseType == null || !stopAtBaseType.IsSame(type.BaseType)) {
      //    return TryGetMethod(type.BaseType.Resolve(), methodName, parameters, out method, stopAtBaseType);
      //  }
      //}

      method = null;
      return false;
    }

    public static bool Remove(this FieldDefinition field) {
      return field.DeclaringType.Fields.Remove(field);
    }

    public static FieldDefinition GetFieldOrThrow(this TypeDefinition type, string fieldName) {
      foreach (var field in type.Fields) {
        if ( field.Name == fieldName ) {
          return field;
        }
      }
      throw new ArgumentOutOfRangeException(nameof(fieldName), $"Field {fieldName} not found in {type}");
    }

    public static MethodReference GetGenericInstanceMethodOrThrow(this GenericInstanceType type, string name) {
      var methodRef = type.Resolve().GetMethodOrThrow(name);

      var newMethodRef = new MethodReference(methodRef.Name, methodRef.ReturnType) {
        HasThis = methodRef.HasThis,
        ExplicitThis = methodRef.ExplicitThis,
        DeclaringType = type,
        CallingConvention = methodRef.CallingConvention,
      };

      foreach (var parameter in methodRef.Parameters) {
        newMethodRef.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, parameter.ParameterType));
      }

      foreach (var genericParameter in methodRef.GenericParameters) {
        newMethodRef.GenericParameters.Add(new GenericParameter(genericParameter.Name, newMethodRef));
      }

      return newMethodRef;
    }

    public static void AddInterface<T>(this TypeDefinition type, ILWeaverAssembly asm) {
      type.Interfaces.Add(new InterfaceImplementation(asm.Import(typeof(T))));
    }

    public static bool RemoveAttribute<T>(this IMemberDefinition member, ILWeaverAssembly asm) where T : Attribute {
      for (int i = 0; i < member.CustomAttributes.Count; ++i) {
        var attr = member.CustomAttributes[i];
        if ( attr.AttributeType.Is<T>() ) {
          member.CustomAttributes.RemoveAt(i);
          return true;
        }
      }
      return false;
    }

    public static CustomAttribute AddAttribute<T>(this IMemberDefinition member, ILWeaverAssembly asm) where T : Attribute {
      CustomAttribute attr;
      attr = new CustomAttribute(typeof(T).GetConstructor(asm));
      member.CustomAttributes.Add(attr);
      return attr;
    }

    public static CustomAttribute AddAttribute<T, A0>(this IMemberDefinition member, ILWeaverAssembly asm, A0 arg0) where T : Attribute {
      CustomAttribute attr;
      attr = new CustomAttribute(typeof(T).GetConstructor(asm, 1));
      attr.ConstructorArguments.Add(new CustomAttributeArgument(asm.ImportAttributeType<A0>(), arg0));
      member.CustomAttributes.Add(attr);
      return attr;
    }

    public static CustomAttribute AddAttribute<T, A0, A1>(this IMemberDefinition member, ILWeaverAssembly asm, A0 arg0, A1 arg1) where T : Attribute {
      CustomAttribute attr;
      attr = new CustomAttribute(typeof(T).GetConstructor(asm, 2));
      attr.ConstructorArguments.Add(new CustomAttributeArgument(asm.ImportAttributeType<A0>(), arg0));
      attr.ConstructorArguments.Add(new CustomAttributeArgument(asm.ImportAttributeType<A1>(), arg1));
      member.CustomAttributes.Add(attr);
      return attr;
    }

    public static CustomAttribute AddAttribute<T, A0, A1, A2>(this IMemberDefinition member, ILWeaverAssembly asm, A0 arg0, A1 arg1, A2 arg2) where T : Attribute {
      CustomAttribute attr;
      attr = new CustomAttribute(typeof(T).GetConstructor(asm, 3));
      attr.ConstructorArguments.Add(new CustomAttributeArgument(asm.ImportAttributeType<A0>(), arg0));
      attr.ConstructorArguments.Add(new CustomAttributeArgument(asm.ImportAttributeType<A1>(), arg1));
      attr.ConstructorArguments.Add(new CustomAttributeArgument(asm.ImportAttributeType<A2>(), arg2));
      member.CustomAttributes.Add(attr);
      return attr;
    }

    private static TypeReference ImportAttributeType<T>(this ILWeaverAssembly asm) {
      if (typeof(T) == typeof(TypeReference)) {
        return asm.Import<Type>();
      } else {
        return asm.Import<T>();
      }
    }

    public static MethodReference GetConstructor(this Type type, ILWeaverAssembly asm, int argCount = 0) {
      foreach (var ctor in type.GetConstructors()) {
        if (ctor.GetParameters().Length == argCount) {
          return asm.CecilAssembly.MainModule.ImportReference(ctor);
        }
      }

      throw new ILWeaverException($"Could not find constructor with {argCount} arguments on {type.Name}");
    }

    public static void AddTo(this MethodDefinition method, TypeDefinition type) {
      type.Methods.Add(method);
    }

    public static void AddTo(this PropertyDefinition property, TypeDefinition type) {
      type.Properties.Add(property);
    }


    public static void AddTo(this FieldDefinition field, TypeDefinition type) {
      type.Fields.Add(field);
    }

    public static void AddTo(this TypeDefinition type, AssemblyDefinition assembly) {
      assembly.MainModule.Types.Add(type);
    }

    public static void AddTo(this TypeDefinition type, TypeDefinition parentType) {
      parentType.NestedTypes.Add(type);
    }

    public static MethodDefinition GetMethodOrThrow(this TypeDefinition type, string methodName) {
      foreach (var method in type.Methods) {
        if (method.Name == methodName) {
          return method;
        }
      }
      throw new ArgumentOutOfRangeException(nameof(methodName), $"Method {methodName} not found in {type}");
    }

    public static Instruction AppendReturn(this ILProcessor il, Instruction instruction) {
      il.Append(instruction);
      return instruction;
    }

    public static void Clear(this ILProcessor il) {
      var instructions = il.Body.Instructions;
      foreach (var instruction in instructions.Reverse()) {
        il.Remove(instruction);
      }
    }

    public static MethodDefinition AddEmptyConstructor(this TypeDefinition type, ILWeaverAssembly asm) {
      var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
      var method = new MethodDefinition(".ctor", methodAttributes, asm.Import(typeof(void)));
      method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));

      MethodReference baseConstructor;

      if (type.BaseType.IsGenericInstance) {
        var gi = (GenericInstanceType)type.BaseType;
        baseConstructor = gi.GetGenericInstanceMethodOrThrow(".ctor");
      } else {
        if (!type.BaseType.Resolve().TryGetMatchingConstructor(method, out var baseConstructorDef)) {
          throw new ILWeaverException("Unable to find matching constructor.");
        }
        baseConstructor = baseConstructorDef;
      }

      

      method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, asm.Import(baseConstructor)));
      method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
      type.Methods.Add(method);
      return method;
    }

    public static void AppendMacro<T>(this ILProcessor il, in T macro) where T : struct, ILProcessorMacro {
      macro.Emit(il);
    }

    public class TypeOrTypeRef {

      public Type Type { get; }
      public TypeReference TypeReference { get; }


      public TypeOrTypeRef(Type type, bool isOut = false) {
        Type = type;
      }

      public TypeOrTypeRef(TypeReference type, bool isOut = false) {
        TypeReference = type;
      }

      public static implicit operator TypeOrTypeRef(Type type) {
        return new TypeOrTypeRef(type);
      }

      public static implicit operator TypeOrTypeRef(TypeReference type) {
        return new TypeOrTypeRef(type);
      }

      public override string ToString() {
        if (Type != null) {
          return Type.FullName;
        } else if (TypeReference != null) {
          return TypeReference.ToString();
        } else {
          return "AnyType";
        }
      }
    }


    public static bool GetSingleOrDefaultMethodWithAttribute<T>(this TypeDefinition type, out CustomAttribute attribute, out MethodDefinition method) where T : Attribute {

      MethodDefinition resultMethod = null;
      CustomAttribute resultAttribute = null;

      foreach (var m in type.Methods) {
        if (m.TryGetAttribute<T>(out var attr)) {
          if (resultMethod != null) {
            throw new ILWeaverException($"Only one method with attribute {typeof(T)} allowed per class: {type}");
          } else {
            resultMethod = m;
            resultAttribute = attr;
          }
        }
      }

      method = resultMethod;
      attribute = resultAttribute;
      return method != null;
    }


    public static PropertyDefinition ThrowIfStatic(this PropertyDefinition property) {
      if (property.GetMethod?.IsStatic == true ||
          property.SetMethod?.IsStatic == true) {
        throw new ILWeaverException($"Property is static: {property.FullName}");
      }
      return property;
    }

    public static MethodDefinition ThrowIfStatic(this MethodDefinition method) {
      if (method.IsStatic) {
        throw new ILWeaverException($"Method is static: {method.FullName}");
      }
      return method;
    }

    public static MethodDefinition ThrowIfNotStatic(this MethodDefinition method) {
      if (!method.IsStatic) {
        throw new ILWeaverException($"Method is not static: {method}");
      }
      return method;
    }

    public static MethodDefinition ThrowIfNotPublic(this MethodDefinition method) {
      if (!method.IsPublic) {
        throw new ILWeaverException($"Method is not public: {method}");
      }
      return method;
    }

    public static MethodDefinition ThrowIfReturnType(this MethodDefinition method, TypeOrTypeRef type) {
      if (!method.ReturnType.IsSame(type)) {

        throw new ILWeaverException($"Method has an invalid return type (expected {type}): {method}");
      }
      return method;
    }

    public static MethodDefinition ThrowIfParameterCount(this MethodDefinition method, int count) {
      if (method.Parameters.Count != count) {
        throw new ILWeaverException($"Method has invalid parameter count (expected {count}): {method}");
      }
      return method;
    }

    public static MethodDefinition ThrowIfParameter(this MethodDefinition method, int index, TypeOrTypeRef type = null, bool isByReference = false) {
      var p = method.Parameters[index];
      if (type != null && !p.ParameterType.IsSame(type)) {
        throw new ILWeaverException($"Parameter {p} ({index}) has an invalid type (expected {type}): {method}");
      }
      if (p.ParameterType.IsByReference != isByReference) {
        if (p.IsOut) {
          throw new ILWeaverException($"Parameter {p} ({index}) is a ref parameter: {method}");
        } else {
          throw new ILWeaverException($"Parameter {p} ({index}) is not a ref parameter: {method}");
        }
      }
      return method;
    }

    public static bool IsBaseConstructorCall(this Instruction instruction, TypeDefinition type) {
      if (instruction.OpCode == OpCodes.Call) {
        var m = ((MethodReference)instruction.Operand).Resolve();
        if (m.IsConstructor && m.DeclaringType.IsSame(type.BaseType)) {
          // base constructor init
          return true;
        }
      }
      return false;
    }

    public static bool IsLdloca(this Instruction instruction, out VariableDefinition variable, out bool isShort) {
      if (instruction.OpCode == OpCodes.Ldloca) {
        variable = (VariableDefinition)instruction.Operand;
        isShort = false;
        return true;
      }
      if (instruction.OpCode == OpCodes.Ldloca_S) {
        variable = (VariableDefinition)instruction.Operand;
        isShort = true;
        return true;
      }

      variable = default;
      isShort = default;
      return false;
    }

    public static bool IsLdlocWithIndex(this Instruction instruction, out int index) {
      if (instruction.OpCode == OpCodes.Ldloc_0) {
        index = 0;
        return true;
      }
      if (instruction.OpCode == OpCodes.Ldloc_1) {
        index = 1;
        return true;
      }
      if (instruction.OpCode == OpCodes.Ldloc_2) {
        index = 2;
        return true;
      }
      if (instruction.OpCode == OpCodes.Ldloc_3) {
        index = 3;
        return true;
      }
      index = -1;
      return false;
    }

    public static bool IsStlocWithIndex(this Instruction instruction, out int index) {
      if (instruction.OpCode == OpCodes.Stloc_0) {
        index = 0;
        return true;
      }
      if (instruction.OpCode == OpCodes.Stloc_1) {
        index = 1;
        return true;
      }
      if (instruction.OpCode == OpCodes.Stloc_2) {
        index = 2;
        return true;
      }
      if (instruction.OpCode == OpCodes.Stloc_3) {
        index = 3;
        return true;
      }
      index = -1;
      return false;
    }
  }

  public interface ILProcessorMacro {
    void Emit(ILProcessor il);
  }
}
#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverLog.cs

#if FUSION_WEAVER && FUSION_HAS_MONO_CECIL

namespace Fusion.CodeGen {

  using System;
  using System.Diagnostics;
  using System.Runtime.CompilerServices;
  using Mono.Cecil;
  using UnityEngine;

  public partial class ILWeaverLog {

    public void AssertMessage(bool condition, string message, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = default) {
      if (!condition) {
        AssertFailed("Assert failed: " + message, filePath, lineNumber);
      }
    }


    public void Assert(bool condition, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = default) {
      if (!condition) {
        AssertFailed("Assertion failed", filePath, lineNumber);
      }
    }

    [Conditional("FUSION_WEAVER_DEBUG")]
    public void Debug(string message, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = default) {
      Log(LogType.Log, message, filePath, lineNumber);
    }

    public void Warn(string message, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = default) {
      Log(LogType.Warning, message, filePath, lineNumber);
    }

    public void Error(string message, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = default) {
      Log(LogType.Error, message, filePath, lineNumber);
    }

    public void Exception(Exception ex) {
      LogExceptionImpl(ex);
    }

    partial void AssertFailed(string message, string filePath, int lineNumber);

    private void Log(LogType logType, string message, string filePath, int lineNumber) {
      LogImpl(logType, message, filePath, lineNumber);
    }

    partial void LogImpl(LogType logType, string message, string filePath, int lineNumber);
    partial void LogExceptionImpl(Exception ex);

#if !FUSION_WEAVER_DEBUG
    public struct LogScope : IDisposable {
      public void Dispose() {
      }
    }

    public LogScope Scope(string name) {
      return default;
    }

    public LogScope ScopeAssembly(AssemblyDefinition cecilAssembly) {
      return default;
    }

    public LogScope ScopeBehaviour(TypeDefinition type) {
      return default;
    }

    public LogScope ScopeInput(TypeDefinition type) {
      return default;
    }

    public LogScope ScopeStruct(TypeDefinition type) {
      return default;
    }

#else
    public LogScope Scope(string name, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = default) {
      return new LogScope(this, name, filePath, lineNumber);
    }

    public LogScope ScopeAssembly(AssemblyDefinition cecilAssembly, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = default) {
      return new LogScope(this, $"Assembly: {cecilAssembly.FullName}", filePath, lineNumber);
    }

    public LogScope ScopeBehaviour(TypeDefinition type, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = default) {
      return new LogScope(this, $"Behaviour: {type.FullName}", filePath, lineNumber);
    }

    public LogScope ScopeInput(TypeDefinition type, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = default) {
      return new LogScope(this, $"Input: {type.FullName}", filePath, lineNumber);
    }

    public LogScope ScopeStruct(TypeDefinition type, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = default) {
      return new LogScope(this, $"Struct: {type.FullName}", filePath, lineNumber);
    }


    partial void ScopeBeginImpl(ref LogScope scope, string filePath, int lineNumber);
    partial void ScopeEndImpl(ref LogScope scope);


#pragma warning disable CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
    public partial struct LogScope : IDisposable {
#pragma warning restore CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
      public string Message;

      public TimeSpan Elapsed => _stopwatch.Elapsed;

      private ILWeaverLog _log;
      private Stopwatch _stopwatch;

      public LogScope(ILWeaverLog log, string message, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = default) : this() {
        _log = log;
        Message = message;
        _stopwatch = Stopwatch.StartNew();
        _log.ScopeBeginImpl(ref this, filePath, lineNumber);
      }

      public void Dispose() {
        _stopwatch.Stop();
        _log.ScopeEndImpl(ref this);
      }

      partial void Init(string filePath, int lineNumber);
    }
#endif
  }
}

#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverLog.ILPostProcessor.cs

#if FUSION_WEAVER && FUSION_WEAVER_ILPOSTPROCESSOR && FUSION_HAS_MONO_CECIL

namespace Fusion.CodeGen {
  using System;
  using System.Collections.Generic;
  using Unity.CompilationPipeline.Common.Diagnostics;
  using UnityEngine;

  partial class ILWeaverLog {

    public List<DiagnosticMessage> Messages { get; } = new List<DiagnosticMessage>();

    partial void LogImpl(LogType logType, string message, string filePath, int lineNumber) {

      DiagnosticType diagnosticType;

      if (logType == LogType.Log) {
        // there are no debug diagnostic messages, make pretend warnings
        message = $"DEBUG: {message}";
        diagnosticType = DiagnosticType.Warning;
      } else if (logType == LogType.Warning) {
        diagnosticType = DiagnosticType.Warning;
      } else {
        diagnosticType = DiagnosticType.Error;
      }

      // newlines in messagedata will need to be escaped, but let's not slow things down now
      Messages.Add(new DiagnosticMessage() {
        File = filePath,
        Line = lineNumber,
        DiagnosticType = diagnosticType,
        MessageData = message
      });
    }

    partial void AssertFailed(string message, string filePath, int lineNumber) {
      Error(message, filePath, lineNumber);
      throw new AssertException();
    }

    partial void LogExceptionImpl(Exception ex) {
      var lines = ex.ToString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
      foreach (var line in lines) {
        Error(line, null, 0);
      }
    }

    public void FixNewLinesInMessages() {
      // fix the messages
      foreach (var msg in Messages) {
        msg.MessageData = msg.MessageData.Replace('\r', ';').Replace('\n', ';');
      }
    }



#if FUSION_WEAVER_DEBUG
#pragma warning disable CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
    partial struct LogScope {
#pragma warning restore CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
      public int LineNumber;
      public string FilePath;
    }

    partial void ScopeBeginImpl(ref LogScope scope, string filePath, int lineNumber) {
      scope.FilePath = filePath;
      scope.LineNumber = lineNumber;
      Log(LogType.Log, $"{scope.Message} start", scope.FilePath, scope.LineNumber);
    }

    partial void ScopeEndImpl(ref LogScope scope) {
      Log(LogType.Log, $"{scope.Message} end {scope.Elapsed}", scope.FilePath, scope.LineNumber);
    }
#endif
  }

}

#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverLog.UnityEditor.cs

#if FUSION_WEAVER && !FUSION_WEAVER_ILPOSTPROCESSOR && FUSION_HAS_MONO_CECIL

namespace Fusion.CodeGen {

  using UnityEngine;
  using System;

  partial class ILWeaverLog {

#if FUSION_WEAVER_DEBUG
    partial void LogExceptionImpl(Exception ex) {
      UnityEngine.Debug.unityLogger.LogException(ex);
    }

    partial void LogImpl(LogType logType, string message, string filePath, int lineNumber) {
      UnityEngine.Debug.unityLogger.Log(logType, message);
    }

    partial void ScopeBeginImpl(ref LogScope scope, string filePath, int lineNumber) {
      Log(LogType.Log, $"{scope.Message} start", default, default);
    }

    partial void ScopeEndImpl(ref LogScope scope) {
      Log(LogType.Log, $"{scope.Message} end {scope.Elapsed}", default, default);
    }

    partial void AssertFailed(string message, string filePath, int lineNumber) {
      Error(message, filePath, lineNumber);
      throw new AssertException();
    }
#endif
  }
}

#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverMethodContext.cs

#if FUSION_WEAVER && FUSION_HAS_MONO_CECIL

namespace Fusion.CodeGen {

  using System;
  using System.Collections.Generic;
  using Mono.Cecil;
  using Mono.Cecil.Cil;
  using Mono.Cecil.Rocks;
  using static Fusion.CodeGen.ILWeaverOpCodes;

  unsafe partial class ILWeaver {

    ILMacroStruct AlignToWordSize() => new[] {
      Ldc_I4(Allocator.REPLICATE_WORD_SIZE - 1),
      Add(),
      Ldc_I4(~(Allocator.REPLICATE_WORD_SIZE - 1)),
      And()
    };

    ILMacroStruct LoadFixedBufferAddress(FieldDefinition fixedBufferField) => new Action<ILProcessor>(il => {

      var elementField = fixedBufferField.FieldType.Resolve().Fields[0];

      int pointerLoc = il.Body.Variables.Count;
      il.Body.Variables.Add(new VariableDefinition(elementField.FieldType.MakePointerType()));
      int pinnedRefLoc = il.Body.Variables.Count;
      il.Body.Variables.Add(new VariableDefinition(elementField.FieldType.MakeByReferenceType().MakePinnedType()));

      il.Append(Ldflda(fixedBufferField));
      il.Append(Ldflda(elementField));
      il.Append(Stloc(il.Body, pinnedRefLoc));
      il.Append(Ldloc(il.Body, pinnedRefLoc));
      il.Append(Conv_U());
      il.Append(Stloc(il.Body, pointerLoc));
      il.Append(Ldloc(il.Body, pointerLoc));
    });

    struct ILMacroStruct : ILProcessorMacro {
      Action<ILProcessor> generator;
      Instruction[] instructions;
      public static implicit operator ILMacroStruct(Instruction[] instructions) {
        return new ILMacroStruct() {
          instructions = instructions
        };
      }

      public static implicit operator ILMacroStruct(Action<ILProcessor> generator) {
        return new ILMacroStruct() {
          generator = generator
        };
      }

      public void Emit(ILProcessor il) {
        if (generator != null) {
          generator(il);
        } else {
          foreach (var instruction in instructions) {
            il.Append(instruction);
          }
        }
      }
    }

    class MethodContext : IDisposable {
      public readonly ILWeaverAssembly Assembly;
      public readonly MethodDefinition Method;

      private Dictionary<string, VariableDefinition> _fields = new Dictionary<string, VariableDefinition>();
      private Action<ILProcessor> addressGetter;
      private bool runnerIsLdarg0 = false;

      public MethodContext(ILWeaverAssembly assembly, MethodDefinition method, bool staticRunnerAccessor = false, Action<ILProcessor> addressGetter = null) {
        if (assembly == null) {
          throw new ArgumentNullException(nameof(assembly));
        }
        if (method == null) {
          throw new ArgumentNullException(nameof(method));
        }

        this.Assembly = assembly;
        this.Method = method;
        this.runnerIsLdarg0 = staticRunnerAccessor;
        this.addressGetter = addressGetter;
      }

      public void Dispose() {
      }

      public VariableDefinition GetOrCreateVariable(string id, TypeReference type) {
        if (_fields.TryGetValue(id, out var val)) {
          if (!val.VariableType.IsSame(type)) {
            throw new ArgumentException($"Variable of with the same name {id} already exists, but has a different type: {type} vs {val.VariableType}", nameof(id));
          }
          return val;
        }
        var result = new VariableDefinition(type);
        _fields.Add(id, result);
        Method.Body.Variables.Add(result);
        return result;
      }

      public TypeReference ImportReference(TypeReference type) {
        if (type == null) {
          throw new ArgumentNullException(nameof(type));
        }
        return Method.Module.ImportReference(type);
      }

      public MethodReference ImportReference(MethodReference method) {
        if (method == null) {
          throw new ArgumentNullException(nameof(method));
        }
        return Method.Module.ImportReference(method);
      }

      public virtual ILMacroStruct LoadAddress() => addressGetter;

      public ForLoopMacro For(int start, int stop, Action<ILProcessor, VariableDefinition> body) => new ForLoopMacro(this, body, start, stop);

      public AddOffsetMacro AddOffset(int val) => AddOffset(Ldc_I4(val));

      public AddOffsetMacro AddOffset(VariableDefinition val) => AddOffset(Ldloc(val));

      public AddOffsetMacro AddOffset(Instruction val) => new AddOffsetMacro(this, instruction: val);

      public AddOffsetMacro AddOffset(Action<ILProcessor> generator) => new AddOffsetMacro(this, generator: generator);

      public AddOffsetMacro AddOffset() => new AddOffsetMacro(this, null, null);

      public ScopeRpcOffset AddOffsetScope(ILProcessor il) => new ScopeRpcOffset(il, this);

      public ILMacroStruct LoadRunner() {
        return runnerIsLdarg0 ?
          new[] { Ldarg_0() } :
          new[] { Ldarg_0(), Ldfld(Assembly.SimulationBehaviour.GetField(nameof(SimulationBehaviour.Runner))) };
      }

      public virtual bool HasOffset => false;

      protected virtual void EmitAddOffsetBefore(ILProcessor il) {
      }

      protected virtual void EmitAddOffsetAfter(ILProcessor il) {
      }

      public readonly struct ForLoopMacro : ILProcessorMacro {
        public readonly MethodContext Context;
        public readonly Action<ILProcessor, VariableDefinition> Generator;
        public readonly int Start;
        public readonly int Stop;

        public ForLoopMacro(MethodContext context, Action<ILProcessor, VariableDefinition> generator, int start, int stop) {
          Context = context;
          Generator = generator;
          Start = start;
          Stop = stop;
        }

        public void Emit(ILProcessor il) {
          var body = Context.Method.Body;
          var varId = body.Variables.Count;
          var indexVariable = new VariableDefinition(Context.Assembly.Import(typeof(int)));
          body.Variables.Add(indexVariable);

          il.Append(Ldc_I4(Start));
          il.Append(Stloc(body, varId));

          var loopConditionStart = Ldloc(body, varId);
          il.Append(Br_S(loopConditionStart));
          {
            var loopBodyBegin = il.AppendReturn(Nop());
            Generator(il, indexVariable);

            il.Append(Ldloc(body, varId));
            il.Append(Ldc_I4(1));
            il.Append(Add());
            il.Append(Stloc(body, varId));

            il.Append(loopConditionStart);
            il.Append(Ldc_I4(Stop));
            il.Append(Blt_S(loopBodyBegin));
          }
        }
      }

      public readonly struct AddOffsetMacro : ILProcessorMacro {
        public readonly MethodContext Context;
        public readonly Instruction Instruction;
        public readonly Action<ILProcessor> Generator;

        public AddOffsetMacro(MethodContext context, Instruction instruction = null, Action<ILProcessor> generator = null) {
          Context = context;
          Instruction = instruction;
          Generator = generator;
        }

        public void Emit(ILProcessor il) {
          if (Context.HasOffset) {
            Context.EmitAddOffsetBefore(il);
            if (Instruction != null) {
              il.Append(Instruction);
            } else if (Generator != null) {
              Generator(il);
            }
            Context.EmitAddOffsetAfter(il);
          }
        }
      }

      public readonly struct ScopeRpcOffset : IDisposable {
        public readonly MethodContext context;
        public readonly ILProcessor il;

        public ScopeRpcOffset(ILProcessor il, MethodContext context) {
          this.context = context;
          this.il = il;
          if (context.HasOffset) {
            context.EmitAddOffsetBefore(il);
          }
        }

        public void Dispose() {
          if (context.HasOffset) {
            context.EmitAddOffsetAfter(il);
          }
        }
      }
    }

    class RpcMethodContext : MethodContext {
      public VariableDefinition DataVariable;
      public VariableDefinition OffsetVariable;
      public VariableDefinition RpcInvokeInfoVariable;

      public RpcMethodContext(ILWeaverAssembly asm, MethodDefinition definition, bool staticRunnerAccessor)
        : base(asm, definition, staticRunnerAccessor) {
      }

      public override bool HasOffset => true;

      protected override void EmitAddOffsetBefore(ILProcessor il) {
        il.Append(Ldloc(OffsetVariable));
      }

      protected override void EmitAddOffsetAfter(ILProcessor il) {
        il.Append(Add());
        il.Append(Stloc(OffsetVariable));
      }

      public override ILMacroStruct LoadAddress() => new[] {
        Ldloc(DataVariable),
        Ldloc(OffsetVariable),
        Add(),
      };

      public ILMacroStruct SetRpcInvokeInfoVariable(string name, Instruction value) => RpcInvokeInfoVariable == null ? new Instruction[0] :
        new[] {
           Ldloca(RpcInvokeInfoVariable),
           value,
           Stfld(Assembly.RpcInvokeInfo.GetField(name)),
        };

      public ILMacroStruct SetRpcInvokeInfoVariable(string name, int value) => RpcInvokeInfoVariable == null ? new Instruction[0] :
        new[] {
           Ldloca(RpcInvokeInfoVariable),
           Ldc_I4(value),
           Stfld(Assembly.RpcInvokeInfo.GetField(name)),
        };
      public ILMacroStruct SetRpcInvokeInfoStatus(bool emitIf, RpcLocalInvokeResult reason) => RpcInvokeInfoVariable == null || !emitIf ? new Instruction[0] :
        new[] {
           Ldloca(RpcInvokeInfoVariable),
           Ldc_I4((int)reason),
           Stfld(Assembly.RpcInvokeInfo.GetField(nameof(RpcInvokeInfo.LocalInvokeResult)))
        };

      public ILMacroStruct SetRpcInvokeInfoStatus(RpcSendCullResult reason) => RpcInvokeInfoVariable == null ? new Instruction[0] :
        new[] {
           Ldloca(RpcInvokeInfoVariable),
           Ldc_I4((int)reason),
           Stfld(Assembly.RpcInvokeInfo.GetField(nameof(RpcInvokeInfo.SendCullResult)))
        };

    }
  }
}

#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverOpCodes.cs

#if FUSION_WEAVER && FUSION_HAS_MONO_CECIL
namespace Fusion.CodeGen {
  using System;
  using System.Diagnostics;

  using Mono.Cecil;
  using Mono.Cecil.Cil;

  static class ILWeaverOpCodes {
    // utils
    public static Instruction Nop()    => Instruction.Create(OpCodes.Nop);
    public static Instruction Ret()    => Instruction.Create(OpCodes.Ret);
    public static Instruction Dup()    => Instruction.Create(OpCodes.Dup);
    public static Instruction Pop()    => Instruction.Create(OpCodes.Pop);
    public static Instruction Ldnull() => Instruction.Create(OpCodes.Ldnull);
    public static Instruction Throw()  => Instruction.Create(OpCodes.Throw);

    public static Instruction Cast(TypeReference type) => Instruction.Create(OpCodes.Castclass, type);

    // breaks
    public static Instruction Brfalse(Instruction target)   => Instruction.Create(OpCodes.Brfalse, target);
    public static Instruction Brtrue(Instruction target)    => Instruction.Create(OpCodes.Brtrue, target);
    public static Instruction Brfalse_S(Instruction target) => Instruction.Create(OpCodes.Brfalse_S, target);
    public static Instruction Brtrue_S(Instruction target)  => Instruction.Create(OpCodes.Brtrue_S, target);
    public static Instruction Br_S(Instruction target)      => Instruction.Create(OpCodes.Br_S, target);
    public static Instruction Br(Instruction target)        => Instruction.Create(OpCodes.Br, target);
    public static Instruction Blt_S(Instruction target)     => Instruction.Create(OpCodes.Blt_S, target);
    public static Instruction Ble_S(Instruction target)     => Instruction.Create(OpCodes.Ble_S, target);
    public static Instruction Beq(Instruction target)       => Instruction.Create(OpCodes.Beq, target);
    public static Instruction Bne_Un_S(Instruction target)  => Instruction.Create(OpCodes.Bne_Un_S, target);
    public static Instruction Beq_S(Instruction target)     => Instruction.Create(OpCodes.Beq_S, target);

    // math
    public static Instruction Add() => Instruction.Create(OpCodes.Add);
    public static Instruction Sub() => Instruction.Create(OpCodes.Sub);
    public static Instruction Mul() => Instruction.Create(OpCodes.Mul);
    public static Instruction Div() => Instruction.Create(OpCodes.Div);
    public static Instruction And() => Instruction.Create(OpCodes.And);

    // obj
    public static Instruction Ldobj(TypeReference type) => Instruction.Create(OpCodes.Ldobj, type);
    public static Instruction Stobj(TypeReference type) => Instruction.Create(OpCodes.Stobj, type);

    public static Instruction Newobj(MethodReference constructor) => Instruction.Create(OpCodes.Newobj, constructor);

    public static Instruction Initobj(TypeReference type) => Instruction.Create(OpCodes.Initobj, type);

    // fields
    public static Instruction Ldflda(FieldReference field) => Instruction.Create(OpCodes.Ldflda, field);
    
    public static Instruction Ldfld(FieldReference  field) => Instruction.Create(OpCodes.Ldfld,  field);
    public static Instruction Stfld(FieldReference  field) => Instruction.Create(OpCodes.Stfld,  field);
    
    public static Instruction Ldsfld(FieldReference field) => Instruction.Create(OpCodes.Ldsfld, field);
    public static Instruction Stsfld(FieldReference field) => Instruction.Create(OpCodes.Stsfld, field);

    // locals

    public static Instruction Ldloc_or_const(VariableDefinition var, int val) => var != null ? Ldloc(var) : Ldc_I4(val);

    public static Instruction Ldloc(VariableDefinition var, MethodDefinition method) => Ldloc(method.Body, method.Body.Variables.IndexOf(var));

    public static Instruction Ldloc(VariableDefinition var)    => Instruction.Create(OpCodes.Ldloc, var);
    public static Instruction Ldloca(VariableDefinition var)   => Instruction.Create(OpCodes.Ldloca, var);
    public static Instruction Ldloca_S(VariableDefinition var) => Instruction.Create(OpCodes.Ldloca_S, var);
    public static Instruction Stloc(VariableDefinition var)    => Instruction.Create(OpCodes.Stloc, var);

    public static Instruction Stloc_0() => Instruction.Create(OpCodes.Stloc_0);
    public static Instruction Stloc_1() => Instruction.Create(OpCodes.Stloc_1);
    public static Instruction Stloc_2() => Instruction.Create(OpCodes.Stloc_2);
    public static Instruction Stloc_3() => Instruction.Create(OpCodes.Stloc_3);

    public static Instruction Ldloc_0() => Instruction.Create(OpCodes.Ldloc_0);
    public static Instruction Ldloc_1() => Instruction.Create(OpCodes.Ldloc_1);
    public static Instruction Ldloc_2() => Instruction.Create(OpCodes.Ldloc_2);
    public static Instruction Ldloc_3() => Instruction.Create(OpCodes.Ldloc_3);

    public static Instruction Stloc(MethodBody body, int index) {
      switch (index) {
        case 0:
          return Stloc_0();
        case 1:
          return Stloc_1();
        case 2:
          return Stloc_2();
        case 3:
          return Stloc_3();
        default:
          return Stloc(body.Variables[index]);
      }
    }

    public static Instruction Ldloc(MethodBody body, int index) {
      switch (index) {
        case 0:
          return Ldloc_0();
        case 1:
          return Ldloc_1();
        case 2:
          return Ldloc_2();
        case 3:
          return Ldloc_3();
        default:
          return Ldloc(body.Variables[index]);
      }
    }


    // ldarg
    public static Instruction Ldarg(ParameterDefinition arg) => Instruction.Create(OpCodes.Ldarg, arg);
    public static Instruction Ldarg_0() => Instruction.Create(OpCodes.Ldarg_0);
    public static Instruction Ldarg_1() => Instruction.Create(OpCodes.Ldarg_1);
    public static Instruction Ldarg_2() => Instruction.Create(OpCodes.Ldarg_2);
    public static Instruction Ldarg_3() => Instruction.Create(OpCodes.Ldarg_3);

    // starg

    public static Instruction Starg_S(ParameterDefinition arg) => Instruction.Create(OpCodes.Starg_S, arg);

    // array
    public static Instruction Ldlen()                    => Instruction.Create(OpCodes.Ldlen);

    public static Instruction Ldelem(TypeReference type) {
      switch (type.MetadataType) {
        case MetadataType.Byte:
          return Instruction.Create(OpCodes.Ldelem_U1);
        case MetadataType.SByte:
          return Instruction.Create(OpCodes.Ldelem_I1);
        case MetadataType.UInt16:
          return Instruction.Create(OpCodes.Ldelem_U2);
        case MetadataType.Int16:
          return Instruction.Create(OpCodes.Ldelem_I2);
        case MetadataType.UInt32:
          return Instruction.Create(OpCodes.Ldelem_U4);
        case MetadataType.Int32:
          return Instruction.Create(OpCodes.Ldelem_I4);
        case MetadataType.UInt64:
          return Instruction.Create(OpCodes.Ldelem_I8);
        case MetadataType.Int64:
          return Instruction.Create(OpCodes.Ldelem_I8);
        case MetadataType.Single:
          return Instruction.Create(OpCodes.Ldelem_R4);
        case MetadataType.Double:
          return Instruction.Create(OpCodes.Ldelem_R8);

        default:
          if (type.IsValueType) {
            return Instruction.Create(OpCodes.Ldelem_Any, type);
          } else {
            return Instruction.Create(OpCodes.Ldelem_Ref);
          }
      }
    }

    public static Instruction Stelem(TypeReference type) {
      switch (type.MetadataType) {
        case MetadataType.Byte:
        case MetadataType.SByte:
          return Instruction.Create(OpCodes.Stelem_I1);
        case MetadataType.UInt16:
        case MetadataType.Int16:
          return Instruction.Create(OpCodes.Stelem_I2);
        case MetadataType.UInt32:
        case MetadataType.Int32:
          return Instruction.Create(OpCodes.Stelem_I4);
        case MetadataType.UInt64:
        case MetadataType.Int64:
          return Instruction.Create(OpCodes.Stelem_I8);
        case MetadataType.Single:
          return Instruction.Create(OpCodes.Stelem_R4);
        case MetadataType.Double:
          return Instruction.Create(OpCodes.Stelem_R8);
        default:
          if (type.IsValueType) {
            return Instruction.Create(OpCodes.Stelem_Any, type);
          } else {
            return Instruction.Create(OpCodes.Stelem_Ref);
          }
      }
    }

    public static Instruction Ldelema(TypeReference arg) => Instruction.Create(OpCodes.Ldelema, arg);

    // conversions
    public static Instruction Conv_I4() => Instruction.Create(OpCodes.Conv_I4);
    public static Instruction Conv_U() => Instruction.Create(OpCodes.Conv_U);

    // functions
    public static Instruction Call(MethodReference  method) => Instruction.Create(OpCodes.Call,  method);
    public static Instruction Ldftn(MethodReference method) => Instruction.Create(OpCodes.Ldftn, method);

    // constants

    public static Instruction Ldstr(string value) => Instruction.Create(OpCodes.Ldstr, value);
    public static Instruction Ldc_R4(float value) => Instruction.Create(OpCodes.Ldc_R4, value);
    public static Instruction Ldc_R8(float value) => Instruction.Create(OpCodes.Ldc_R8, value);

    public static Instruction Ldc_I4(int value) {
      switch (value) {
        case 0: return Instruction.Create(OpCodes.Ldc_I4_0);
        case 1: return Instruction.Create(OpCodes.Ldc_I4_1);
        case 2: return Instruction.Create(OpCodes.Ldc_I4_2);
        case 3: return Instruction.Create(OpCodes.Ldc_I4_3);
        case 4: return Instruction.Create(OpCodes.Ldc_I4_4);
        case 5: return Instruction.Create(OpCodes.Ldc_I4_5);
        case 6: return Instruction.Create(OpCodes.Ldc_I4_6);
        case 7: return Instruction.Create(OpCodes.Ldc_I4_7);
        case 8: return Instruction.Create(OpCodes.Ldc_I4_8);
        default:
          return Instruction.Create(OpCodes.Ldc_I4, value);
      }
    }

    public static Instruction Stind_I4() => Instruction.Create(OpCodes.Stind_I4);
    public static Instruction Ldind_I4() => Instruction.Create(OpCodes.Ldind_I4);

    public static Instruction Stind_or_Stobj(TypeReference type) {
      if (type.IsPrimitive) {
        return Stind(type);
      } else {
        return Stobj(type);
      }
    }

    public static Instruction Ldind_or_Ldobj(TypeReference type) {
      if (type.IsPrimitive) {
        return Ldind(type);
      } else {
        return Ldobj(type);
      }
    }

    public static Instruction Stind(TypeReference type) {
      switch (type.MetadataType) {
        case MetadataType.Byte:
        case MetadataType.SByte:
          return Instruction.Create(OpCodes.Stind_I1);
        case MetadataType.UInt16:
        case MetadataType.Int16:
          return Instruction.Create(OpCodes.Stind_I2);
        case MetadataType.UInt32:
        case MetadataType.Int32:
          return Instruction.Create(OpCodes.Stind_I4);
        case MetadataType.UInt64:
        case MetadataType.Int64:
          return Instruction.Create(OpCodes.Stind_I8);
        case MetadataType.Single:
          return Instruction.Create(OpCodes.Stind_R4);
        case MetadataType.Double:
          return Instruction.Create(OpCodes.Stind_R8);
        default:
          throw new ILWeaverException($"Unknown primitive type {type.FullName}");
      }
    }

    public static Instruction Ldind(TypeReference type) {
      switch (type.MetadataType) {
        case MetadataType.Byte:
          return Instruction.Create(OpCodes.Ldind_U1);
        case MetadataType.SByte:
          return Instruction.Create(OpCodes.Ldind_I1);
        case MetadataType.UInt16:
          return Instruction.Create(OpCodes.Ldind_U2);
        case MetadataType.Int16:
          return Instruction.Create(OpCodes.Ldind_I2);
        case MetadataType.UInt32:
          return Instruction.Create(OpCodes.Ldind_U4);
        case MetadataType.Int32:
          return Instruction.Create(OpCodes.Ldind_I4);
        case MetadataType.UInt64:
          return Instruction.Create(OpCodes.Ldind_I8);
        case MetadataType.Int64:
          return Instruction.Create(OpCodes.Ldind_I8);
        case MetadataType.Single:
          return Instruction.Create(OpCodes.Ldind_R4);
        case MetadataType.Double:
          return Instruction.Create(OpCodes.Ldind_R8);
        default:
          throw new ILWeaverException($"Unknown primitive type {type.FullName}");
      }
    }
  }
}
#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverSettings.cs

#if FUSION_WEAVER
namespace Fusion.CodeGen {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  static partial class ILWeaverSettings {

    public const string RuntimeAssemblyName = "Fusion.Runtime";

    internal static float GetNamedFloatAccuracy(string tag) {
      float? result = null;
      GetAccuracyPartial(tag, ref result);
      if ( result == null ) {
        throw new ArgumentOutOfRangeException($"Unknown accuracy tag: {tag}");
      }
      return result.Value;
    }

    internal static bool NullChecksForNetworkedProperties() {
      bool result = true;
      NullChecksForNetworkedPropertiesPartial(ref result);
      return result;
    }

    internal static bool IsAssemblyWeavable(string name) {
      bool result = false;
      IsAssemblyWeavablePartial(name, ref result);
      return result;
    }

    internal static bool ContainsRequiredReferences(string[] references) {
      return Array.Find(references, x => x.Contains(RuntimeAssemblyName)) != null;
    }

    internal static bool UseSerializableDictionaryForNetworkDictionaryProperties() {
      bool result = true;
      UseSerializableDictionaryForNetworkDictionaryPropertiesPartial(ref result);
      return result;
    }

    static partial void GetAccuracyPartial(string tag, ref float? accuracy);

    static partial void IsAssemblyWeavablePartial(string name, ref bool result);

    static partial void UseSerializableDictionaryForNetworkDictionaryPropertiesPartial(ref bool result);

    static partial void NullChecksForNetworkedPropertiesPartial(ref bool result);
  }
}
#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverSettings.ILPostProcessor.cs

#if FUSION_WEAVER && FUSION_WEAVER_ILPOSTPROCESSOR
namespace Fusion.CodeGen {

  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Runtime.Serialization.Json;
  using System.Text;
  using System.Xml.Linq;
  using System.Xml.XPath;

  static partial class ILWeaverSettings {


    public enum ConfigStatus {
      Ok,
      NotFound,
      ReadException
    }

    const string DefaultNetworkProjectConfigPath = "Assets/Photon/Fusion/Resources/NetworkProjectConfig.fusion";

    static partial void OverrideNetworkProjectConfigPath(ref string path);

    public static string NetworkProjectConfigPath {
      get {
        var result = DefaultNetworkProjectConfigPath;
        OverrideNetworkProjectConfigPath(ref result);
        return result;
      }
    }

    static Lazy<XDocument> _config = new Lazy<XDocument>(() => {
      using (var stream = File.OpenRead(NetworkProjectConfigPath)) {
        var jsonReader = JsonReaderWriterFactory.CreateJsonReader(stream, new System.Xml.XmlDictionaryReaderQuotas());
        return XDocument.Load(jsonReader);
      }
    });

    static Lazy<AccuracyDefaults> _accuracyDefaults = new Lazy<AccuracyDefaults>(() => {

      var element = GetElementOrThrow(_config.Value.Root, nameof(NetworkProjectConfig.AccuracyDefaults));

      AccuracyDefaults defaults = new AccuracyDefaults();

      {
        var t = ParseAccuracies(element, nameof(AccuracyDefaults.coreKeys), nameof(AccuracyDefaults.coreVals));
        defaults.coreKeys = t.Item1;
        defaults.coreVals = t.Item2;
      }

      {
        var t = ParseAccuracies(element, nameof(AccuracyDefaults.tags), nameof(AccuracyDefaults.values));
        defaults.tags = t.Item1.ToList();
        defaults.values = t.Item2.ToList();
      }

      defaults.RebuildLookup();
      return defaults;
    });

    static partial void GetAccuracyPartial(string tag, ref float? accuracy) {
      try {
        if (_accuracyDefaults.Value.TryGetAccuracy(tag, out var result)) {
          accuracy = result._value;
        }
      } catch (Exception ex) {
        throw new Exception("Error getting accuracy " + tag, ex);
      }
    }

    static partial void NullChecksForNetworkedPropertiesPartial(ref bool result) {
      result = _nullChecksForNetworkedProperties.Value ?? result;
    }

    static partial void IsAssemblyWeavablePartial(string name, ref bool result) {
      result = _assembliesToWeave.Value.Contains(name);
    }

    static partial void UseSerializableDictionaryForNetworkDictionaryPropertiesPartial(ref bool result) {
      result = _useSerializableDictionary.Value ?? result;
    }

    public static bool ValidateConfig(out ConfigStatus errorType, out Exception error) {
      try {
        error = null;
        if (!File.Exists(NetworkProjectConfigPath)) {
          errorType = ConfigStatus.NotFound;
          return false;
        }

        _ = _config.Value;
        _ = _assembliesToWeave.Value;

        errorType = ConfigStatus.Ok;
        return true;
      } catch (Exception ex) {
        error = ex;
        errorType = ConfigStatus.ReadException;
        return false;
      }
    }

    static XElement GetElementOrThrow(XElement element, string name) {
      var child = element.Element(name);
      if (child == null) {
        throw new InvalidOperationException($"Node {name} not found in config. (path: {element.GetXPath()})");
      }
      return child;
    }

    static IEnumerable<XElement> GetElementsOrThrow(XElement element, string name) {
      return GetElementOrThrow(element, name).Elements();
    }

    static float GetElementAsFloat(XElement element, string name) {
      var child = GetElementOrThrow(element, name);
      try {
        return (float)child;
      } catch (Exception ex) {
        throw new InvalidOperationException($"Unable to cast {child.GetXPath()} to float", ex);
      }
    }

    static (string[], Accuracy[]) ParseAccuracies(XElement element, string keyName, string valueName) {
      var keys = GetElementsOrThrow(element, keyName).Select(x => x.Value).ToArray();
      var values = GetElementsOrThrow(element, valueName).Select(x => GetElementAsFloat(x, nameof(Accuracy._value)))
                      .Zip(keys, (val, key) => new Accuracy(key, val))
                      .ToArray();
      return (keys, values);
    }

    static Lazy<HashSet<string>> _assembliesToWeave = new Lazy<HashSet<string>>(() => {
      return new HashSet<string>(GetElementsOrThrow(_config.Value.Root, nameof(NetworkProjectConfig.AssembliesToWeave)).Select(x => x.Value), StringComparer.OrdinalIgnoreCase);
    });

    static Lazy<bool?> _nullChecksForNetworkedProperties = new Lazy<bool?>(() => {
      return (bool?)_config.Value.Root.Element(nameof(NetworkProjectConfig.NullChecksForNetworkedProperties));
    });

    static Lazy<bool?> _useSerializableDictionary = new Lazy<bool?>(() => {
      return (bool?)_config.Value.Root.Element(nameof(NetworkProjectConfig.UseSerializableDictionary));
    });


    public static string GetXPath(this XElement element) {
      var ancestors = element.AncestorsAndSelf()
        .Select(x => {
          int index = x.GetIndexPosition();
          string name = x.Name.LocalName;
          return (index == -1) ? $"/{name}" : $"/{name}[{index}]";
        });

      return string.Concat(ancestors.Reverse());
    }

    public static int GetIndexPosition(this XElement element) {
      if (element.Parent == null) {
        return -1;
      }

      int i = 0;
      foreach (var sibling in element.Parent.Elements(element.Name)) {
        if (sibling == element) {
          return i;
        }
        i++;
      }

      return -1;
    }
  }
}
#endif

#endregion


#region Assets/Photon/FusionCodeGen/ILWeaverSettings.UnityEditor.cs

#if FUSION_WEAVER && !FUSION_WEAVER_ILPOSTPROCESSOR
namespace Fusion.CodeGen {

  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;

  static partial class ILWeaverSettings {

    static partial void GetAccuracyPartial(string tag, ref float? accuracy) {
      if (NetworkProjectConfig.Global.AccuracyDefaults.TryGetAccuracy(tag, out var _acc)) {
        accuracy = _acc.Value;
      }
    }

    static partial void IsAssemblyWeavablePartial(string name, ref bool result) {
      if (Array.Find(NetworkProjectConfig.Global.AssembliesToWeave, x => string.Compare(name, x, true) == 0) != null) {
        result = true;
      }
    }

    static partial void UseSerializableDictionaryForNetworkDictionaryPropertiesPartial(ref bool result) {
      result = NetworkProjectConfig.Global.UseSerializableDictionary;
    }

    static partial void NullChecksForNetworkedPropertiesPartial(ref bool result) {
      result = NetworkProjectConfig.Global.NullChecksForNetworkedProperties;
    }
  }
}
#endif

#endregion


#region Assets/Photon/FusionCodeGen/InstructionEqualityComparer.cs

#if FUSION_WEAVER && FUSION_HAS_MONO_CECIL
namespace Fusion.CodeGen {
  using System.Collections.Generic;
  using Mono.Cecil.Cil;

  internal class InstructionEqualityComparer : IEqualityComparer<Instruction> {
    public bool Equals(Instruction x, Instruction y) {
      if (x.OpCode != y.OpCode) {
        return false;
      }

      if (x.Operand != y.Operand) {
        if (x.Operand?.GetType() != y?.Operand.GetType()) {
          return false;
        }
        // there needs to be a better way to do this
        if (x.Operand.ToString() != y.Operand.ToString()) {
          return false;
        }

      }

      return true;
    }

    public int GetHashCode(Instruction obj) {
      return obj.GetHashCode();
    }
  }
}
#endif

#endregion

#endif
