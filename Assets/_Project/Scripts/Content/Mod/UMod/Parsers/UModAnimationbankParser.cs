using System;

namespace rwby
{
    [UModContentParser("Animationbank", "Animationbanks")]
    [System.Serializable]
    public class UModAnimationbankParser : UModContentParser<IAnimationbankDefinition>
    {
        public override int parserType
        {
            get { return (int)ContentType.Animationbank; }
        }
    }
}