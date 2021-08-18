using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace rwby.Debugging
{
    public class ConsoleReader : MonoBehaviour
    {
        [SerializeField] private ConsoleInputProcessor inputProcessor;

        public async Task ReadCommandLine()
        {
            await Convert(System.Environment.CommandLine);
        }

        public async Task Convert(string input)
        {
            List<ConsoleInput> inputs = new List<ConsoleInput>();

            string[] inputLines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < inputLines.Length; i++)
            {
                inputs.Add(SplitInputLine(inputLines[i]));
            }

            await inputProcessor.Process(inputs);
        }

        private static ConsoleInput SplitInputLine(string inputLine)
        {
            List<string> splitInputs = new List<string>();

            bool insideList = false;
            bool insideParenth = false;
            string builtInput = "";
            for(int i = 0; i < inputLine.Length; i++)
            {
                if (insideList)
                {
                    if (inputLine[i] == ']')
                    {
                        insideList = false;
                        splitInputs.Add(builtInput);
                        builtInput = "";
                        continue;
                    }
                    builtInput += inputLine[i];
                    continue;
                }

                if (insideParenth)
                {
                    switch (inputLine[i])
                    {
                        case '\"':
                            insideParenth = false;
                            splitInputs.Add(builtInput);
                            builtInput = "";
                            break;
                        default:
                            builtInput += inputLine[i];
                            break;
                    }
                }
                else
                {
                    switch (inputLine[i])
                    {
                        case ' ':
                            // No input yet, just continue.
                            if (String.IsNullOrEmpty(builtInput))
                            {
                                continue;
                            }
                            // Space means we put the input into the list.
                            splitInputs.Add(builtInput);
                            builtInput = "";
                            break;
                        case '\"':
                            insideParenth = true;
                            break;
                        case '[':
                            insideList = true;
                            break;
                        default:
                            builtInput += inputLine[i];
                            break;
                    }
                }
            }
            if (!String.IsNullOrEmpty(builtInput))
            {
                splitInputs.Add(builtInput);
                builtInput = null;
            }
            return new ConsoleInput(splitInputs.ToArray());
        }
    }
}