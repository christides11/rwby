using System;
using System.Text;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ContentGUID : IEquatable<ContentGUID>
    {
        public static readonly char[] byteToLetterLookup = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.".ToCharArray();
        
        public byte[] guid;
        
        public ContentGUID(byte length)
        {
            guid = new byte[length];
        }
        
        public ContentGUID(byte[] guid)
        {
            this.guid = new byte[guid.Length];
            for (int i = 0; i < guid.Length; i++)
            {
                this.guid[i] = guid[i];
            }
        }

        public ContentGUID(byte length, string input)
        {
            guid = BuildGUID(length, input);
        }

        public override string ToString()
        {
            return BuildString(guid);
        }

        public static byte[] BuildGUID(byte length, string input)
        {
            byte[] output = new byte[length];
            try
            {
                for (int i = 0; i < length; i++)
                {
                    if (i >= input.Length) break;
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
            if(length == 0) length = 8;
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

        public static string BuildString(byte[] guid)
        {
            try
            {
                StringBuilder sb = new StringBuilder("", guid.Length);

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

        public static ContentGUID StringToContentGUID(string value, byte maxLength = 8)
        {
            ContentGUID cGUID = new ContentGUID(maxLength);
            if (ContentGUID.TryBuildGUID(maxLength, value, out byte[] output))
            {
                cGUID.guid = output;
            }

            return cGUID;
        }

        public bool Equals(ContentGUID other)
        {
            for (int i = 0; i < guid.Length; i++)
            {
                if (guid[i] != other.guid[i]) return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is ContentGUID other && Equals(other);
        }

        public override int GetHashCode()
        {
            int value=0;
            for (var i = 0;i< this.guid.Length; i++)
            {
                value=HashCode.Combine(this.guid[i],value);
            }

            return value;
        }
        
        public static bool operator ==(ContentGUID x, ContentGUID y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ContentGUID x, ContentGUID y)
        {
            return !(x == y);
        }
    }
}