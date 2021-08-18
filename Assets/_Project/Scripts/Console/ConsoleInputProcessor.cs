using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rwby.Debugging
{
    public class ConsoleInputProcessor : MonoBehaviour
    {
        public ConsoleWindow consoleWindow;

        List<MethodInfo> staticCommandMembers = new List<MethodInfo>();
        List<MethodInfo> nonstaticCommandMembers = new List<MethodInfo>();

        List<ICParser> parsers = new List<ICParser>();

        private void Start()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach(Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach(Type type in types)
                {
                    BindingFlags publicStaticFlags = BindingFlags.Public | BindingFlags.Static;
                    MethodInfo[] publicStaticMethods = type.GetMethods(publicStaticFlags);

                    #region Statics
                    foreach (MethodInfo method in publicStaticMethods)
                    {
                        if(method.CustomAttributes.ToArray().Length > 0)
                        {
                            CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>();
                            if(attribute != null)
                            {
                                staticCommandMembers.Add(method);
                            }
                        }
                    }
                    #endregion

                    if(type.GetInterface("ICParser") == typeof(ICParser))
                    {
                        parsers.Add((ICParser)Activator.CreateInstance(type, null));
                    }
                }
            }

        }

        public async virtual Task Process(List<ConsoleInput> inputs)
        {
            foreach(ConsoleInput input in inputs)
            {
                await Process(input);
            }
        }

        public async virtual Task Process(ConsoleInput input)
        {
            if(input.input[0] == "help")
            {
                foreach(MethodInfo m in staticCommandMembers)
                {
                    CommandAttribute attribute = m.GetCustomAttribute<CommandAttribute>();

                    string callFormat = "";
                    callFormat += attribute.commandId + " ";
                    ParameterInfo[] pi = m.GetParameters();
                    for (int i = 0; i < pi.Length; i++)
                    {
                        callFormat += pi[i].Name + " ";
                    }
                    callFormat += "- " + attribute.commandDescrition;

                    consoleWindow.WriteLine(callFormat);
                }
            }

            foreach (MethodInfo m in staticCommandMembers)
            {
                CommandAttribute attribute = m.GetCustomAttribute<CommandAttribute>();

                // Check if it's this input.
                if(input.input[0] != attribute.commandId)
                {
                    continue;
                }
                if(m.GetParameters().Count() == 0 && input.input.Count()-1 == 0)
                {
                    m.Invoke(null, new object[0]);
                    return;
                }
                // Check if input has the same parameter count.
                if(input.input.Count()-1 != m.GetParameters().Count())
                {
                    continue;
                }

                InvokeMethod(m, input);
                return;
            }
        }

        private void InvokeMethod(MethodInfo m, ConsoleInput input)
        {
            List<object> parameters = new List<object>();
            var pInfo = m.GetParameters();
            // Conversion.
            for (int i = 0; i < pInfo.Count(); i++)
            {
                parameters.Add(FindParser(pInfo[i], input.input[i+1]));
            }
            m.Invoke(null, parameters.ToArray());
        }

        private object FindParser(ParameterInfo parameterInfo, string input)
        {
            foreach (var parser in parsers)
            {
                if (parser.CanParse(parameterInfo.ParameterType))
                {
                    return parser.Parse(input, parameterInfo.ParameterType, null);
                }
            }
            return input;
        }
    }
}