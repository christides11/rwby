using System;

namespace rwby.Debugging
{
    public class PrimitiveParser : ICParser
    {
        public int Priority { get { return 0; } }

        public bool CanParse(Type type)
        {
            if(type == typeof(int) || type == typeof(float) || type == typeof(bool))
            {
                return true;
            }
            return false;
        }

        public object Parse(string value, Type type, Func<string, Type, object> recursiveParser)
        {
            if(type == typeof(int))
            {
                if(int.TryParse(value, out int result))
                {
                    return result;
                }
            }else if(type == typeof(float))
            {
                if(float.TryParse(value, out float result)){
                    return result;
                }
            }else if (type == typeof(bool))
            {
                return (value.ToLower() == "true" || value.ToLower() == "t");
            }
            return null;
        }
    }
}