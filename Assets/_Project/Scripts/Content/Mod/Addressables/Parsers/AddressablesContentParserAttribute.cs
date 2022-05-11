using System;

namespace rwby
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AddressablesContentParserAttribute : Attribute
    {
        public string parsetPath;
        public string parserNickname;

        public AddressablesContentParserAttribute(string parserPath, string nickname)
        {
            this.parsetPath = parserPath;
            this.parserNickname = nickname;
        }
    }
}