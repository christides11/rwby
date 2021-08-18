using System;

namespace rwby.Debugging
{
    public class StringArrayParser : ICParser
    {
        public int Priority { get { return 0; } }

        public bool CanParse(Type type)
        {
            if (type == typeof(string[]))
            {
                return true;
            }
            return false;
        }

        public object Parse(string value, Type type, Func<string, Type, object> recursiveParser)
        {
            string[] str = value.Split(',');
            for(int i = 0; i < str.Length; i++)
            {
                str[i] = str[i].Trim();
            }
            return str;
        }
    }
}