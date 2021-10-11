#if !FUSION_DEV

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

  unsafe partial class ILWeaver {


    void AddUnmanagedType<T>() where T : unmanaged {
      unsafe {
        var size = sizeof(T);
        var wordCount = Native.WordCount(size, Allocator.REPLICATE_WORD_SIZE);
        _typeData.Add(typeof(T).FullName, new TypeMetaData(wordCount));
      }
    }

    void SetDefaultTypeData() {
      _networkedBehaviourTypeData = new Dictionary<string, BehaviourMetaData>();
      _typeData = new Dictionary<string, TypeMetaData>();
      _rpcCount = new Dictionary<string, int>();

      _typeData.Add("NetworkedObject", new TypeMetaData(2));

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

    const string DEFAULTS_METHOD_NAME = nameof(NetworkBehaviour.Defaults);

    const string FIND_OBJECT_METHOD_NAME = nameof(NetworkRunner.FindObject);

    Dictionary<string, TypeMetaData> _typeData = new Dictionary<string, TypeMetaData>();
    Dictionary<string, BehaviourMetaData> _networkedBehaviourTypeData = new Dictionary<string, BehaviourMetaData>();
    Dictionary<string, int> _rpcCount = new Dictionary<string, int>();


    internal readonly ILWeaverLog Log;

    internal ILWeaver(ILWeaverLog log) {
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

    MethodReference GetConstructor<T>(ILWeaverAssembly asm, int argCount = 0) {
      foreach (var ctor in typeof(T).GetConstructors()) {
        if (ctor.GetParameters().Length == argCount) {
          return asm.CecilAssembly.MainModule.ImportReference(ctor);
        }
      }

      throw new ILWeaverException($"Could not find constructor with {argCount} arguments on {typeof(T).Name}");
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

    static int GetPrimitiveSize(TypeReference type) {
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

    int GetWordCount(ILWeaverAssembly asm, WrapInfo wrapInfo) {
      return wrapInfo.WrapperType != null ? GetTypeWordCount(asm, wrapInfo.WrapperType) : Native.WordCount(wrapInfo.MaxRawByteCount, Allocator.REPLICATE_WORD_SIZE);
    }

    int GetByteCount(ILWeaverAssembly asm, WrapInfo wrapInfo) {
      return GetWordCount(asm, wrapInfo) * Allocator.REPLICATE_WORD_SIZE;
    }

    int GetTypeWordCount(ILWeaverAssembly asm, TypeReference type) {
      if (type.IsPointer) {
        type = type.GetElementType();
      }

      if (type.IsNetworkArray()) {
        type = GetStaticArrayElementType(type);
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
          WeaveStruct(asm, typeDefinition);

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
      if (property.PropertyType.Is<NetworkBehaviour>() || property.PropertyType.Is<NetworkObject>()) {
        return 2;
      }

      if (property.PropertyType.IsString()) {
        return 2 + GetStringCapacity(property);
      }

      if (property.PropertyType.IsNetworkArray()) {
        return GetStaticArrayCapacity(property) * GetTypeWordCount(asm, GetStaticArrayElementType(property.PropertyType));
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
    
    TypeReference GetStaticDictionaryKeyType(TypeReference type) {
      return ((GenericInstanceType)type).GenericArguments[0];
    }
    
    TypeReference GetStaticDictionaryValType(TypeReference type) {
      return ((GenericInstanceType)type).GenericArguments[1];
    }

    void LoadArrayElementAddress(ILProcessor il, OpCode indexOpCode, int elementWordCount, int wordOffset = 0) {
      il.Append(Instruction.Create(OpCodes.Ldarg_0));
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
      if (NetworkRunner.BuildType == NetworkRunner.BuildTypes.Debug) {
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

    (FieldDefinition, FieldDefinition) MakeReaderWriterMethods(ILWeaverAssembly asm, PropertyDefinition property, TypeReference elementType, int elementWordCount, string suffix, ILProcessor getIL) {
      var dataType  = asm.CecilAssembly.MainModule.ImportReference(typeof(byte*));
      var indexType = asm.CecilAssembly.MainModule.ImportReference(typeof(int));
      
      var getterMethod = new MethodDefinition(property.Name + $"@{suffix}@Getter", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, elementType);
      getterMethod.Parameters.Add(new ParameterDefinition("data", ParameterAttributes.None, dataType));
      getterMethod.Parameters.Add(new ParameterDefinition("index", ParameterAttributes.None, indexType));

      var setterMethod = new MethodDefinition(property.Name + $"@{suffix}@Setter", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, asm.CecilAssembly.MainModule.ImportReference(typeof(void)));
      setterMethod.Parameters.Add(new ParameterDefinition("data", ParameterAttributes.None, dataType));
      setterMethod.Parameters.Add(new ParameterDefinition("index", ParameterAttributes.None, indexType));
      setterMethod.Parameters.Add(new ParameterDefinition("val", ParameterAttributes.None, elementType));

      var getterMethodIL = getterMethod.Body.GetILProcessor();
      var setterMethodIL = setterMethod.Body.GetILProcessor();

      var readerDelegateType = asm.CecilAssembly.MainModule.ImportReference(asm.CecilAssembly.MainModule.ImportReference(typeof(ElementReader<>)).MakeGenericInstanceType(elementType));
      var writerDelegateType = asm.CecilAssembly.MainModule.ImportReference(asm.CecilAssembly.MainModule.ImportReference(typeof(ElementWriter<>)).MakeGenericInstanceType(elementType));

      var readerDelegateCtor = asm.CecilAssembly.MainModule.ImportReference(readerDelegateType.Resolve().GetConstructors().First());
      readerDelegateCtor.DeclaringType = readerDelegateCtor.DeclaringType.MakeGenericInstanceType(elementType);

      var writerDelegateCtor = asm.CecilAssembly.MainModule.ImportReference(writerDelegateType.Resolve().GetConstructors().First());
      writerDelegateCtor.DeclaringType = writerDelegateCtor.DeclaringType.MakeGenericInstanceType(elementType);

      InjectValueAccessor(asm, getterMethodIL, setterMethodIL, property, elementType, OpCodes.Ldarg_2, (il, offset) => {
        if (ReferenceEquals(il, setterMethodIL)) {
          LoadArrayElementAddress(setterMethodIL, OpCodes.Ldarg_1, elementWordCount, offset);
        } else {
          LoadArrayElementAddress(getterMethodIL, OpCodes.Ldarg_1, elementWordCount, offset);
        }
      }, false);

      property.DeclaringType.Methods.Add(getterMethod);
      property.DeclaringType.Methods.Add(setterMethod);

      var getterCache = new FieldDefinition(GetCacheName(getterMethod.Name), FieldAttributes.Private | FieldAttributes.Static, readerDelegateType);
      var setterCache = new FieldDefinition(GetCacheName(setterMethod.Name), FieldAttributes.Private | FieldAttributes.Static, writerDelegateType);

      property.DeclaringType.Fields.Add(getterCache);
      property.DeclaringType.Fields.Add(setterCache);
      
      var nop = Instruction.Create(OpCodes.Nop);

      getIL.Append(Instruction.Create(OpCodes.Ldsfld, getterCache));
      getIL.Append(Instruction.Create(OpCodes.Ldnull));
      getIL.Append(Instruction.Create(OpCodes.Ceq));
      getIL.Append(Instruction.Create(OpCodes.Brfalse, nop));

      getIL.Append(Instruction.Create(OpCodes.Ldnull));
      getIL.Append(Instruction.Create(OpCodes.Ldftn, getterMethod));
      getIL.Append(Instruction.Create(OpCodes.Newobj, readerDelegateCtor));
      getIL.Append(Instruction.Create(OpCodes.Stsfld, getterCache));

      getIL.Append(Instruction.Create(OpCodes.Ldnull));
      getIL.Append(Instruction.Create(OpCodes.Ldftn, setterMethod));
      getIL.Append(Instruction.Create(OpCodes.Newobj, writerDelegateCtor));
      getIL.Append(Instruction.Create(OpCodes.Stsfld, setterCache));

      getIL.Append(nop);

      return (getterCache, setterCache);
    }

    void InjectValueAccessor(ILWeaverAssembly asm, ILProcessor getIL, ILProcessor setIL, PropertyDefinition property, TypeReference type, OpCode valueOpCode, Action<ILProcessor, int> addressLoader, bool injectNullChecks) {
      if (injectNullChecks) {
        InjectPtrNullCheck(asm, getIL, property);

        if (setIL != null) {
          InjectPtrNullCheck(asm, setIL, property);
        }
      }

      // for pointer types we can simply just return the address we loaded on the stack
      if (type.IsPointer) {
        // load address
        addressLoader(getIL, 0);

        // return
        getIL.Append(Instruction.Create(OpCodes.Ret));

        // this has to be null
        Assert.Check(setIL == null);

      } else if (property.PropertyType.IsString()) {
        var cache = new FieldDefinition(GetCacheName(property.Name), FieldAttributes.Private, asm.CecilAssembly.MainModule.ImportReference(typeof(string)));

        property.DeclaringType.Fields.Add(cache);

        getIL.Append(Ldarg_0());
        getIL.Append(Ldfld(asm.SimulationBehaviour.GetField(RUNNER_FIELD_NAME)));

        addressLoader(getIL, 0);
        getIL.Append(Instruction.Create(OpCodes.Ldarg_0));
        getIL.Append(Instruction.Create(OpCodes.Ldflda, cache));
        getIL.Append(Instruction.Create(OpCodes.Call, asm.NetworkRunner.GetMethod(nameof(NetworkRunner.GetString), 2)));
        getIL.Append(Instruction.Create(OpCodes.Ret));

        setIL.Append(Ldarg_0());
        setIL.Append(Ldfld(asm.SimulationBehaviour.GetField(RUNNER_FIELD_NAME)));

        addressLoader(setIL, 0);
        setIL.Append(Instruction.Create(OpCodes.Ldc_I4, GetStringCapacity(property)));
        setIL.Append(Instruction.Create(OpCodes.Ldarg_1));
        setIL.Append(Instruction.Create(OpCodes.Ldarg_0));
        setIL.Append(Instruction.Create(OpCodes.Ldflda, cache));
        setIL.Append(Instruction.Create(OpCodes.Call, asm.NetworkRunner.GetMethod(nameof(NetworkRunner.SetString), 4)));
        setIL.Append(Instruction.Create(OpCodes.Ret));
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
            setIL.Append(Instruction.Create(OpCodes.Ldarg_1));
            setIL.Append(Instruction.Create(OpCodes.Stind_R4));
          } else {
            setIL.Append(Instruction.Create(OpCodes.Ldc_R4, 1f / accuracy));
            setIL.Append(Instruction.Create(valueOpCode));
            var write = asm.ReadWriteUtils.GetMethod("WriteFloat");
            setIL.Append(Instruction.Create(OpCodes.Call, write));
          }

          setIL.Append(Instruction.Create(OpCodes.Ret));
        }

        // byte, sbyte, short, ushort, int, uint
        else {
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
        using (var ctx = new MethodContext(asm, property.GetMethod, addressGetter: (x) => addressLoader(x, 0))) {
          if (field != null) {
            property.DeclaringType.Fields.Add(field);
            WeaveNetworkUnwrap(asm, getIL, ctx, wrapInfo, property.PropertyType, previousValue: field);
          } else {
            WeaveNetworkUnwrap(asm, getIL, ctx, wrapInfo, property.PropertyType);
          }
          getIL.Append(Ret());
        }

        // setter
        using (var ctx = new MethodContext(asm, property.SetMethod, addressGetter: (x) => addressLoader(x, 0))) {
          WeaveNetworkWrap(asm, setIL, ctx, il => il.Append(Ldarg_1()), wrapInfo);
          //if (cache != null) {
          //  setIL.Append(Ldarg_0());
          //  setIL.Append(Ldarg_1());
          //  setIL.Append(Stfld(cache));
          //}
          setIL.Append(Ret());
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
        } else if (type.IsNetworkDictionary()) {
          
          
          var keyType      = GetStaticDictionaryKeyType(type);
          var keyWordCount = GetTypeWordCount(asm, keyType);
          
          var valType      = GetStaticDictionaryValType(type);
          var valWordCount = GetTypeWordCount(asm, valType);

          var capacity  = GetStaticDictionaryCapacity(property);
          
          var (keyReader, keyWriter) = MakeReaderWriterMethods(asm, property, keyType, keyWordCount, "Key", getIL);
          var (valReader, valWriter) = MakeReaderWriterMethods(asm, property, valType, valWordCount, "Val", getIL);
          
          // load address
          addressLoader(getIL, 0);
          getIL.Append(Instruction.Create(OpCodes.Ldc_I4, capacity));
          getIL.Append(Instruction.Create(OpCodes.Ldsfld, keyReader));
          getIL.Append(Instruction.Create(OpCodes.Ldsfld, keyWriter));
          getIL.Append(Instruction.Create(OpCodes.Ldsfld, valReader));
          getIL.Append(Instruction.Create(OpCodes.Ldsfld, valWriter));

          var ctor = asm.CecilAssembly.MainModule.ImportReference(type.Resolve().GetConstructors().First(x => x.Parameters.Count > 1));
          ctor.DeclaringType = ctor.DeclaringType.MakeGenericInstanceType(keyType, valType);

          getIL.Append(Instruction.Create(OpCodes.Newobj, ctor));
          getIL.Append(Instruction.Create(OpCodes.Ret));
          
        } else if (type.IsNetworkArray()) {
          var elementType = GetStaticArrayElementType(type);
          var elementWordCount = GetTypeWordCount(asm, elementType);
          var arrayLength = GetStaticArrayCapacity(property);

          var bytePointer = asm.CecilAssembly.MainModule.ImportReference(typeof(byte*));
          var intValue = asm.CecilAssembly.MainModule.ImportReference(typeof(int));

          var getterMethod = new MethodDefinition(property.Name + "@Getter", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, elementType);
          getterMethod.Parameters.Add(new ParameterDefinition("data", ParameterAttributes.None, bytePointer));
          getterMethod.Parameters.Add(new ParameterDefinition("index", ParameterAttributes.None, intValue));

          var setterMethod = new MethodDefinition(property.Name + "@Setter", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, asm.CecilAssembly.MainModule.ImportReference(typeof(void)));
          setterMethod.Parameters.Add(new ParameterDefinition("data", ParameterAttributes.None, bytePointer));
          setterMethod.Parameters.Add(new ParameterDefinition("index", ParameterAttributes.None, intValue));
          setterMethod.Parameters.Add(new ParameterDefinition("val", ParameterAttributes.None, elementType));

          var getterMethodIL = getterMethod.Body.GetILProcessor();
          var setterMethodIL = setterMethod.Body.GetILProcessor();

          var readerDelegateType = asm.CecilAssembly.MainModule.ImportReference(resolvedPropertyType.NestedTypes.First(x => x.Name == "Reader").MakeGenericInstanceType(elementType));
          var writerDelegateType = asm.CecilAssembly.MainModule.ImportReference(resolvedPropertyType.NestedTypes.First(x => x.Name == "Writer").MakeGenericInstanceType(elementType));

          var readerDelegateCtor = asm.CecilAssembly.MainModule.ImportReference(readerDelegateType.Resolve().GetConstructors().First());
          readerDelegateCtor.DeclaringType = readerDelegateCtor.DeclaringType.MakeGenericInstanceType(elementType);

          var writerDelegateCtor = asm.CecilAssembly.MainModule.ImportReference(writerDelegateType.Resolve().GetConstructors().First());
          writerDelegateCtor.DeclaringType = writerDelegateCtor.DeclaringType.MakeGenericInstanceType(elementType);

          InjectValueAccessor(asm, getterMethodIL, setterMethodIL, property, elementType, OpCodes.Ldarg_2, (il, offset) => {
            if (ReferenceEquals(il, setterMethodIL)) {
              LoadArrayElementAddress(setterMethodIL, OpCodes.Ldarg_1, elementWordCount, offset);
            } else {
              LoadArrayElementAddress(getterMethodIL, OpCodes.Ldarg_1, elementWordCount, offset);
            }
          }, false);

          property.DeclaringType.Methods.Add(getterMethod);
          property.DeclaringType.Methods.Add(setterMethod);

          var getterCache = new FieldDefinition(GetCacheName(getterMethod.Name), FieldAttributes.Private | FieldAttributes.Static, readerDelegateType);
          var setterCache = new FieldDefinition(GetCacheName(setterMethod.Name), FieldAttributes.Private | FieldAttributes.Static, writerDelegateType);

          property.DeclaringType.Fields.Add(getterCache);
          property.DeclaringType.Fields.Add(setterCache);

          var nop = Instruction.Create(OpCodes.Nop);

          getIL.Append(Instruction.Create(OpCodes.Ldsfld, getterCache));
          getIL.Append(Instruction.Create(OpCodes.Ldnull));
          getIL.Append(Instruction.Create(OpCodes.Ceq));
          getIL.Append(Instruction.Create(OpCodes.Brfalse, nop));

          getIL.Append(Instruction.Create(OpCodes.Ldnull));
          getIL.Append(Instruction.Create(OpCodes.Ldftn, getterMethod));
          getIL.Append(Instruction.Create(OpCodes.Newobj, readerDelegateCtor));
          getIL.Append(Instruction.Create(OpCodes.Stsfld, getterCache));

          getIL.Append(Instruction.Create(OpCodes.Ldnull));
          getIL.Append(Instruction.Create(OpCodes.Ldftn, setterMethod));
          getIL.Append(Instruction.Create(OpCodes.Newobj, writerDelegateCtor));
          getIL.Append(Instruction.Create(OpCodes.Stsfld, setterCache));

          getIL.Append(nop);

          // load address
          addressLoader(getIL, 0);
          getIL.Append(Instruction.Create(OpCodes.Ldc_I4, arrayLength));
          getIL.Append(Instruction.Create(OpCodes.Ldsfld, getterCache));
          getIL.Append(Instruction.Create(OpCodes.Ldsfld, setterCache));

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


    FieldDefinition AddInspectorField(ILWeaverAssembly asm, PropertyDefinition property) {
      var field = new FieldDefinition(GetInspectorFieldName(property.Name), FieldAttributes.Private, property.PropertyType);
      property.DeclaringType.Fields.Add(field);

      bool hasNonSerialized = false;

      foreach (var attribute in property.CustomAttributes) {
        if (attribute.AttributeType.IsSame<NetworkedAttribute>() || 
            attribute.AttributeType.IsSame<NetworkedWeavedAttribute>() ||
            attribute.AttributeType.IsSame<AccuracyAttribute>() ||
            attribute.AttributeType.IsSame<HideFromInspectorAttribute>() ||
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

      if (!hasNonSerialized) {
        AddAttribute<SerializeField>(asm, field);
      }

      return field;
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
    }

    bool IsWeavableProperty(PropertyDefinition property, out WeavablePropertyMeta meta) {
      if (property.TryGetAttribute<NetworkedAttribute>(out var attr) == false) {
        meta = default;
        return false;
      }

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
        if (property.PropertyType.IsPointer == false && property.PropertyType.IsNetworkArray() == false && property.PropertyType.IsNetworkDictionary() == false) {
          meta = default;
          return false;
        }
      }

      // check for backing field ...
      var backing = FindBackingField(property.DeclaringType, property.Name);
      if (backing == null) {
        var il = attr.Properties.FirstOrDefault(x => x.Name == "RetainIL");

        if (il.Argument.Value is bool retainIL && retainIL) {
          meta = new WeavablePropertyMeta() {
            ReatainIL = true
          };
          return true;
        }

        meta = default;
        return false;
      }

      meta = new WeavablePropertyMeta() {
        BackingField = backing,
        ReatainIL = false
      };

      attr.TryGetAttributeProperty(nameof(NetworkedAttribute.Default), out meta.DefaultFieldName);

      return true;
    }

    void AddAttribute<T>(ILWeaverAssembly asm, IMemberDefinition member) where T : Attribute {
      member.CustomAttributes.Add(new CustomAttribute(GetConstructor<T>(asm)));
    }

    void AddAttribute<T, A0>(ILWeaverAssembly asm, IMemberDefinition member, A0 arg0) where T : Attribute {
      CustomAttribute attr;
      attr = new CustomAttribute(GetConstructor<T>(asm, 1));
      attr.ConstructorArguments.Add(new CustomAttributeArgument(ImportType<A0>(asm), arg0));
      member.CustomAttributes.Add(attr);
    }

    void AddAttribute<T, A0, A1>(ILWeaverAssembly asm, IMemberDefinition member, A0 arg0, A1 arg1) where T : Attribute {
      CustomAttribute attr;
      attr = new CustomAttribute(GetConstructor<T>(asm, 2));
      attr.ConstructorArguments.Add(new CustomAttributeArgument(ImportType<A0>(asm), arg0));
      attr.ConstructorArguments.Add(new CustomAttributeArgument(ImportType<A1>(asm), arg1));
      member.CustomAttributes.Add(attr);
    }

    void AddAttribute<T, A0, A1, A2>(ILWeaverAssembly asm, IMemberDefinition member, A0 arg0, A1 arg1, A2 arg2) where T : Attribute {
      CustomAttribute attr;
      attr = new CustomAttribute(GetConstructor<T>(asm, 3));
      attr.ConstructorArguments.Add(new CustomAttributeArgument(ImportType<A0>(asm), arg0));
      attr.ConstructorArguments.Add(new CustomAttributeArgument(ImportType<A1>(asm), arg1));
      attr.ConstructorArguments.Add(new CustomAttributeArgument(ImportType<A2>(asm), arg2));
      member.CustomAttributes.Add(attr);
    }

    bool IsRpcCompatibleType(TypeReference property) {
      if (property.IsPointer) {
        return false;
      }

      if (property.IsNetworkArray()) {
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

    bool IsSimpleProperty(PropertyDefinition property) {
      if (property.PropertyType.IsPointer) {
        return false;
      }

      if (property.PropertyType.IsNetworkArray()) {
        return false;
      }

      if (property.PropertyType.IsNetworkDictionary()) {
        return false;
      }
      
      if (property.PropertyType.IsString()) {
        return true;
      }

      if (property.PropertyType.IsValueType) {
        return true;
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

        // flag asm as modified
        asm.Modified = true;

        // set as explicit layout
        type.IsExplicitLayout = true;

        // clear all backing fields
        foreach (var property in type.Properties) {
          if (IsWeavableProperty(property)) {
            RemoveBackingField(property);
          }
        }

        // figure out word counts for everything
        var wordCount = 0;

        foreach (var field in type.Fields) {
          // set offset 
          field.Offset = wordCount * Allocator.REPLICATE_WORD_SIZE;

          // increase block count
          wordCount += GetTypeWordCount(asm, field.FieldType);
        }

        // add new attribute
        AddAttribute<NetworkInputWeavedAttribute, int>(asm, type, wordCount);

        // track type data
        _typeData.Add(type.FullName, new TypeMetaData {
          WordCount = wordCount,
          Definition = type,
          Reference = type
        });
      }
    }

    void WeaveStruct(ILWeaverAssembly asm, TypeDefinition type) {
      if (type.TryGetAttribute<NetworkStructWeavedAttribute>(out var attribute)) {
        if (_typeData.ContainsKey(type.FullName) == false) {
          _typeData.Add(type.FullName, new TypeMetaData {
            WordCount = (int)attribute.ConstructorArguments[0].Value,
            Definition = type,
            Reference = type
          });
        }

        return;
      }

      using (Log.ScopeStruct(type)) {

        // flag asm as modified
        asm.Modified = true;

        // set as explicit layout
        type.IsExplicitLayout = true;

        // clear all backing fields
        foreach (var property in type.Properties) {
          if (IsWeavableProperty(property)) {
            RemoveBackingField(property);
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

        // add new attribute
        AddAttribute<NetworkStructWeavedAttribute, int>(asm, type, wordCount);

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

          if (!rpc.ReturnType.IsVoid()) {
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

        // rpc key
        int instanceRpcKey = -1;

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

          var ret = Ret();

          // check if runner's ok
          if (NetworkRunner.BuildType == NetworkRunner.BuildTypes.Debug) {
            if (rpc.IsStatic) {
              il.AppendMacro(ctx.LoadRunner());
              var checkDone = Nop();
              il.Append(Brtrue_S(checkDone));
              il.Append(Ldstr(rpc.Parameters[0].Name));
              il.Append(Newobj(GetConstructor<ArgumentNullException>(asm, 1)));
              il.Append(Throw());
              il.Append(checkDone);
            } else {
              il.Append(Ldarg_0());
              il.Append(Call(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized))));
            }
          }


          // if we shouldn't invoke during resim
          if (invokeResim == false) {
            il.AppendMacro(ctx.LoadRunner());

            il.Append(Call(asm.NetworkRunner.GetProperty("Stage")));
            il.Append(Ldc_I4((int)SimulationStages.Resimulate));
            il.Append(Instruction.Create(OpCodes.Beq, ret));
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
              } else if (NetworkRunner.BuildType == NetworkRunner.BuildTypes.Debug) {
                // will never get called
                var checkDone = Nop();
                il.Append(Bne_Un_S(checkDone));
                il.Append(Ldarg(rpcTargetParameter));
                il.Append(Ldstr(rpc.ToString()));
                il.Append(Call(asm.NetworkBehaviourUtils.GetMethod(nameof(NetworkBehaviourUtils.NotifyLocalTargetedRpcCulled))));
                il.Append(Br(ret));
                il.Append(checkDone);
              } else {
                // just don't call
                il.Append(Beq(ret));
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
            il.AppendMacro(ctx.LoadRunner());
            il.Append(Call(asm.NetworkRunner.GetMethod(nameof(NetworkRunner.HasAnyActiveConnections))));
            il.Append(Brfalse(afterSend));
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
          il.Append(Call(asm.NetworkRunner.GetMethod(nameof(NetworkRunner.SendRpc), 1)));

          il.Append(afterSend);

          // .. hmm
          if (invokeLocal) {

            if (targetedInvokeLocal != null) {
              il.Append(Br(ret));
              il.Append(targetedInvokeLocal);
            }

            if (!rpc.IsStatic) {
              il.Append(Ldloc(localAuthorityMask));
              il.Append(Ldc_I4(targets));
              il.Append(And());
              il.Append(Brfalse_S(ret));
            }

            il.Append(prepareInv);

            foreach (var param in rpc.Parameters) {
              if (param.ParameterType.IsSame<RpcInfo>()) {
                // need to fill it now
                il.AppendMacro(ctx.LoadRunner());
                il.Append(Ldc_I4((int)channel));
                il.Append(Call(asm.RpcInfo.GetMethod(nameof(RpcInfo.FromLocal))));
                il.Append(Starg_S(param));
              }
            }

            // invoke
            il.Append(Br(inv));
          }

          il.Append(ret);
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
            AddAttribute<NetworkRpcStaticWeavedInvokerAttribute, string>(asm, invoker, rpc.ToString());
          } else {
            Log.Assert(instanceRpcKey >= 0);
            AddAttribute<NetworkRpcWeavedInvokerAttribute, int, int, int>(asm, invoker, instanceRpcKey, sources, targets);
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
              il.Append(Ldarg_1());
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
        size = GetPrimitiveSize(elementType);
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

          if (elementType.IsPrimitive && (GetPrimitiveSize(elementType) % Allocator.REPLICATE_WORD_SIZE) != 0) {
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

          if (elementType.IsPrimitive && (GetPrimitiveSize(elementType) % Allocator.REPLICATE_WORD_SIZE) != 0) {
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


    void WeaveBehaviour(ILWeaverAssembly asm, TypeDefinition type) {
      if (type.HasGenericParameters) {
        return;
      }

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
        var setDefaults = type.Methods.FirstOrDefault(x => x.Name == DEFAULTS_METHOD_NAME);

        // base method
        var baseMethod = FindMethodInParent(asm, type, DEFAULTS_METHOD_NAME, nameof(NetworkBehaviour));

        foreach (var property in type.Properties) {
          if (IsWeavableProperty(property, out var propertyInfo) == false) {
            continue;
          }

          propertyInfo.BackingField?.Remove();

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
          AddAttribute<NetworkedWeavedAttribute, int, int>(asm, property, wordOffset, propertyWordCount);

          // setup simple field
          if (IsSimpleProperty(property) && property.HasAttribute<HideFromInspectorAttribute>() == false) {
            if (setDefaults == null) {
              setDefaults = new MethodDefinition(DEFAULTS_METHOD_NAME, MethodAttributes.Public, asm.CecilAssembly.MainModule.ImportReference(typeof(void)));
              setDefaults.IsVirtual = true;
              setDefaults.IsHideBySig = true;
              setDefaults.IsReuseSlot = true;

              // call base method
              if (baseMethod != null) {
                var bodyIL = setDefaults.Body.GetILProcessor();
                bodyIL.Append(Instruction.Create(OpCodes.Ldarg_0));
                bodyIL.Append(Instruction.Create(OpCodes.Call, baseMethod));
              }

              type.Methods.Add(setDefaults);
            }

            FieldDefinition defaultField;

            if (string.IsNullOrEmpty(propertyInfo.DefaultFieldName)) {
              defaultField = AddInspectorField(asm, property);
            } else {
              defaultField = property.DeclaringType.GetFieldOrThrow(propertyInfo.DefaultFieldName);
            }

            AddAttribute<DefaultForPropertyAttribute, string>(asm, defaultField, property.Name);

            {
              var il = setDefaults.Body.GetILProcessor();
              il.Append(Instruction.Create(OpCodes.Ldarg_0));
              il.Append(Instruction.Create(OpCodes.Ldarg_0));
              il.Append(Instruction.Create(OpCodes.Ldfld, defaultField));
              il.Append(Instruction.Create(OpCodes.Call, setter));
            }
          }
        }

        if (setDefaults != null) {
          setDefaults.Body.GetILProcessor().Append(Instruction.Create(OpCodes.Ret));
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

    internal bool Weave(ILWeaverAssembly asm) {
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
              WeaveStruct(asm, t);
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
          asm.CecilAssembly.CustomAttributes.Add(new CustomAttribute(GetConstructor<NetworkAssemblyWeavedAttribute>(asm)));
        }

        return asm.Modified;
      }
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

  class ILWeaverImportedType {
    public Type                 ClrType;
    public ILWeaverAssembly     Assembly;
    public List<TypeDefinition> BaseDefinitions;
    public TypeReference        Reference;

    Dictionary<string, FieldReference>  _fields        = new Dictionary<string, FieldReference>();
    Dictionary<(string, int?), MethodReference> _methods = new Dictionary<(string, int?), MethodReference>();
    Dictionary<string, MethodReference> _propertiesGet = new Dictionary<string, MethodReference>();
    Dictionary<string, MethodReference> _propertiesSet = new Dictionary<string, MethodReference>();

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

  class ILWeaverAssembly {
    public bool         Modified;
    public List<String> Errors = new List<string>();

    public AssemblyDefinition CecilAssembly;

    ILWeaverImportedType _networkRunner;
    ILWeaverImportedType _readWriteUtils;
    ILWeaverImportedType _nativeUtils;
    ILWeaverImportedType _rpcInfo;
    ILWeaverImportedType _rpcHeader;
    ILWeaverImportedType _networkBehaviourUtils;

    ILWeaverImportedType _simulation;
    ILWeaverImportedType _networkedObject;
    ILWeaverImportedType _networkedObjectId;
    ILWeaverImportedType _networkedBehaviour;
    ILWeaverImportedType _networkedBehaviourId;
    ILWeaverImportedType _simulationBehaviour;
    ILWeaverImportedType _simulationMessage;

    Dictionary<Type, TypeReference> _types = new Dictionary<Type, TypeReference>();

    public ILWeaverImportedType NetworkedObject {
      get {
        if (_networkedObject == null) {
          _networkedObject = new ILWeaverImportedType(this, typeof(NetworkObject));
        }

        return _networkedObject;
      }
    }

    public ILWeaverImportedType Simulation {
      get {
        if (_simulation == null) {
          _simulation = new ILWeaverImportedType(this, typeof(Simulation));
        }

        return _simulation;
      }
    }
    
    public ILWeaverImportedType SimulationMessage {
      get {
        if (_simulationMessage == null) {
          _simulationMessage = new ILWeaverImportedType(this, typeof(SimulationMessage));
        }

        return _simulationMessage;
      }
    }

    public ILWeaverImportedType NetworkedBehaviour {
      get {
        if (_networkedBehaviour == null) {
          _networkedBehaviour = new ILWeaverImportedType(this, typeof(NetworkBehaviour));
        }

        return _networkedBehaviour;
      }
    }

    public ILWeaverImportedType SimulationBehaviour {
      get {
        if (_simulationBehaviour == null) {
          _simulationBehaviour = new ILWeaverImportedType(this, typeof(SimulationBehaviour));
        }

        return _simulationBehaviour;
      }
    }

    public ILWeaverImportedType NetworkId {
      get {
        if (_networkedObjectId == null) {
          _networkedObjectId = new ILWeaverImportedType(this, typeof(NetworkId));
        }

        return _networkedObjectId;
      }
    }

    public ILWeaverImportedType NetworkedBehaviourId {
      get {
        if (_networkedBehaviourId == null) {
          _networkedBehaviourId = new ILWeaverImportedType(this, typeof(NetworkBehaviourId));
        }

        return _networkedBehaviourId;
      }
    }
    
    public ILWeaverImportedType NetworkRunner {
      get {
        if (_networkRunner == null) {
          _networkRunner = new ILWeaverImportedType(this, typeof(NetworkRunner));
        }

        return _networkRunner;
      }
    }

    public ILWeaverImportedType ReadWriteUtils {
      get {
        if (_readWriteUtils == null) {
          _readWriteUtils = new ILWeaverImportedType(this, typeof(ReadWriteUtilsForWeaver));
        }

        return _readWriteUtils;
      }
    }

    public ILWeaverImportedType Native {
      get {
        if (_nativeUtils == null) {
          _nativeUtils = new ILWeaverImportedType(this, typeof(Native));
        }

        return _nativeUtils;
      }
    }

    public ILWeaverImportedType NetworkBehaviourUtils {
      get {
        if (_networkBehaviourUtils == null) {
          _networkBehaviourUtils = new ILWeaverImportedType(this, typeof(NetworkBehaviourUtils));
        }

        return _networkBehaviourUtils;
      }
    }

    public ILWeaverImportedType RpcHeader {
      get {
        if (_rpcHeader == null) {
          _rpcHeader = new ILWeaverImportedType(this, typeof(RpcHeader));
        }

        return _rpcHeader;
      }
    }

    public ILWeaverImportedType RpcInfo {
      get {
        if (_rpcInfo == null) {
          _rpcInfo = new ILWeaverImportedType(this, typeof(RpcInfo));
        }

        return _rpcInfo;
      }
    }

    public MethodReference Import(MethodInfo method) {
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

        switch (status) {
          case ILWeaverSettings.ConfigStatus.NotFound: {

              var candidates = Directory.GetFiles("Assets", "*.fusion", SearchOption.AllDirectories);

              message = $"Fusion ILWeaver config error: {nameof(NetworkProjectConfig)} not found at {ILWeaverSettings.NetworkProjectConfigPath}, weaving stopped. " +
                $"Implement {nameof(ILWeaverSettings)}.OverrideNetworkProjectConfigPath in Fusion.CodeGen.User.cs to change the config's location.";

              if (candidates.Any()) {
                message += $" Possible candidates are: {(string.Join(", ", candidates))}.";
              }
            }
            break;

          case ILWeaverSettings.ConfigStatus.NotAYAMLFile:
            message = $"Fusion ILWeaver config error: file {ILWeaverSettings.NetworkProjectConfigPath} exists, but does not seem to be a valid text YAML file. " +
              $"Please make sure Asset Serialization is set to Force Text.";
            break;

          case ILWeaverSettings.ConfigStatus.ReadException:
            message = $"Fusion ILWeaver config error: reading file {ILWeaverSettings.NetworkProjectConfigPath} failed: {readException.Message}";
            break;

          default:
            throw new NotSupportedException(status.ToString());
        }

        return new ILPostProcessResult(null, new List<DiagnosticMessage>() {
          new DiagnosticMessage() {
            DiagnosticType = DiagnosticType.Warning,
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

  class ILWeaverException : Exception {
    public ILWeaverException(string error) : base(error) {
    }

    public ILWeaverException(string error, Exception innerException) : base(error, innerException) {
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
  using System.Threading.Tasks;
  using Mono.Cecil;
  using Mono.Cecil.Cil;
  using Mono.Cecil.Rocks;

  static class ILWeaverExtensions {

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

    public static bool IsNetworkArray(this TypeReference type) {
      if (!type.IsGenericInstance) {
        return false;
      }

      return type.GetElementType().FullName == typeof(NetworkArray<>).FullName;
    }

    public static bool IsNetworkDictionary(this TypeReference type) {
      if (!type.IsGenericInstance) {
        return false;
      }

      return type.GetElementType().FullName == typeof(NetworkDictionary<,>).FullName;
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

    public static bool TryGetMatchingConstructor(this TypeDefinition type, MethodDefinition constructor, out MethodDefinition matchingConstructor) {
      foreach (var c in type.GetConstructors() ) {
        if ( c.Parameters.Count != constructor.Parameters.Count ) {
          continue;
        }
        int i;
        for (i = 0; i < c.Parameters.Count; ++i) {
          if (!c.Parameters[i].ParameterType.IsSame(constructor.Parameters[i].ParameterType)) {
            break;
          }
        }

        if ( i == c.Parameters.Count ) {
          matchingConstructor = c;
          return true;
        }
      }

      matchingConstructor = null;
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

    public static Instruction AppendReturn(this ILProcessor il, Instruction instruction) {
      il.Append(instruction);
      return instruction;
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

  partial class ILWeaverLog {

    public void Assert(bool condition, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = default) {
      if (!condition) {
        AssertFailed(filePath, lineNumber);
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

    partial void AssertFailed(string filePath, int lineNumber);

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

    partial void AssertFailed(string filePath, int lineNumber) {
      Error("Assertion failed", filePath, lineNumber);
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

    partial void AssertFailed(string filePath, int lineNumber) {
      Error("Assertion failed", filePath, lineNumber);
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
  using static Fusion.CodeGen.ILWeaverOpCodes;

  unsafe partial class ILWeaver {

    ILMacroStruct AlignToWordSize() => new[] {
      Ldc_I4(Allocator.REPLICATE_WORD_SIZE - 1),
      Add(),
      Ldc_I4(~(Allocator.REPLICATE_WORD_SIZE - 1)),
      And()
    };

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
        return Method.Module.ImportReference(type);
      }
      public MethodReference ImportReference(MethodReference type) {
        return Method.Module.ImportReference(type);
      }

      public virtual ILMacroStruct LoadAddress() => addressGetter;

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

    // fields
    public static Instruction Ldflda(FieldReference field) => Instruction.Create(OpCodes.Ldflda, field);
    
    public static Instruction Ldfld(FieldReference  field) => Instruction.Create(OpCodes.Ldfld,  field);
    public static Instruction Stfld(FieldReference  field) => Instruction.Create(OpCodes.Stfld,  field);
    
    public static Instruction Ldsfld(FieldReference field) => Instruction.Create(OpCodes.Ldsfld, field);
    public static Instruction Stsfld(FieldReference field) => Instruction.Create(OpCodes.Stsfld, field);

    // locals

    public static Instruction Ldloc_or_const(VariableDefinition var, int val) => var != null ? Ldloc(var) : Ldc_I4(val);

    public static Instruction Ldloc(VariableDefinition var)    => Instruction.Create(OpCodes.Ldloc, var);
    public static Instruction Ldloca(VariableDefinition var)   => Instruction.Create(OpCodes.Ldloca, var);
    public static Instruction Ldloca_S(VariableDefinition var) => Instruction.Create(OpCodes.Ldloca_S, var);
    public static Instruction Stloc(VariableDefinition var)    => Instruction.Create(OpCodes.Stloc, var);

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

    internal static bool IsAssemblyWeavable(string name) {
      bool result = false;
      IsAssemblyWeavablePartial(name, ref result);
      return result;
    }

    internal static bool ContainsRequiredReferences(string[] references) {
      return Array.Find(references, x => x.Contains(RuntimeAssemblyName)) != null;
    }

    static partial void GetAccuracyPartial(string tag, ref float? accuracy);

    static partial void IsAssemblyWeavablePartial(string name, ref bool result); 
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
      NotAYAMLFile,
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

    static partial void IsAssemblyWeavablePartial(string name, ref bool result) {
      result = _assembliesToWeave.Value.Contains(name);
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
  }
}
#endif

#endregion

#endif
