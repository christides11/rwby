using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.Debugging
{
    [System.Serializable]
    public class ConsoleInput
    {
        public string[] input;

        public ConsoleInput(string[] input)
        {
            this.input = input;
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = input[i].ToLower();
            }
        }

        public override string ToString()
        {
            string output = "";
            for(int i = 0; i < input.Length; i++)
            {
                output += input[i];
                if(i < (input.Length - 1))
                {
                    output += ", ";
                }
            }
            return output;
        }
    }
}