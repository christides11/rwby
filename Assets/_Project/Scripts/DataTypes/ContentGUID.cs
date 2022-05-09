using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ContentGUID
    {
        public static readonly char[] byteToLetterLookup = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.".ToCharArray();

        public byte length;
        public byte[] guid;
        
        public ContentGUID(byte length)
        {
            this.length = length;
            guid = new byte[length];
        }

        public ContentGUID(byte length, string input)
        {
            this.length = length;
            guid = BuildGUID(length, input);
        }

        public ContentGUID(byte[] guid)
        {
            this.length = (byte)guid.Length;
            this.guid = guid;
        }

        public override string ToString()
        {
            return BuildString(length, guid);
        }

        public static byte[] BuildGUID(byte length, string input)
        {
            byte[] output = new byte[length];
            try
            {
                for (int i = 0; i < length; i++)
                {
                    output[i] = (byte)(Array.IndexOf(byteToLetterLookup, input[i])+1);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error building GUID: {e}");
            }
            return output;
        }

        public static bool TryBuildGUID(byte length, string input, out byte[] output)
        {
            output = new byte[length];
            if (input.Length > length) return false;
            if (String.IsNullOrWhiteSpace(input) || String.IsNullOrEmpty(input)) return true;
            try
            {
                for (int i = 0; i < input.Length; i++)
                {
                    output[i] = (byte)(Array.IndexOf(byteToLetterLookup, input[i])+1);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return false;
            }
            return true;
        }

        public static string BuildString(byte length, byte[] guid)
        {
            try
            {
                StringBuilder sb = new StringBuilder("", length);

                for (int i = 0; i < guid.Length; i++)
                {
                    if (guid[i] == 0) break;
                    if (guid[i] >= byteToLetterLookup.Length) throw new Exception($"GUID byte {guid[i]} out of range.");
                    sb.Append(byteToLetterLookup[guid[i] - 1]);
                }

                return sb.ToString();
            }
            catch(Exception e)
            {
                Debug.LogError($"Error building string: {e}");
                return "";
            }
        }

        #if UNITY_EDITOR
        public static string BuildString(byte length, SerializedProperty sp)
        {
            try
            {
                StringBuilder sb = new StringBuilder("", length);

                for (int i = 0; i < sp.arraySize; i++)
                {
                    byte value = (byte)sp.GetArrayElementAtIndex(i).intValue;
                    if (value == 0) break;
                    if (value >= byteToLetterLookup.Length) throw new Exception($"GUID byte {value} out of range.");
                    sb.Append(byteToLetterLookup[value - 1]);
                }

                return sb.ToString();
            }
            catch(Exception e)
            {
                Debug.LogError($"Error building string: {e}");
                return "";
            }
        }
        #endif

        public static implicit operator NetworkedContentGUID(ContentGUID cguid) =>
            new NetworkedContentGUID(cguid.guid);
    }
}