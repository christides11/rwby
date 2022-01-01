using System;

namespace rwby
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AddressablesContentParserAttribute : Attribute
    {
        public string parsetPath;

        public AddressablesContentParserAttribute(string parserPath)
        {
            this.parsetPath = parserPath;
        }
    }
}