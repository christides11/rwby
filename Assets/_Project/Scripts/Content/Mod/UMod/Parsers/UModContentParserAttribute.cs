using System;

namespace rwby
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UModContentParserAttribute : Attribute
    {
        public string parsetPath;
        public string parserNickname;

        public UModContentParserAttribute(string parserPath, string nickname)
        {
            this.parsetPath = parserPath;
            this.parserNickname = nickname;
        }
    }
}