/*-----------------------------------------------------------------------------
TestParametersReader.cs

 Разбор параметров командной строки, проверка их корректноти
-----------------------------------------------------------------------------*/
using System;
using System.IO;

namespace Kordis50ProductTestParser
{
    class TestParametersReader
    {
        /// <summary>
        /// Имя выполняемого теста
        /// </summary>
        public static String testName = "";

        public static int ReadTestParameters(string[] args)
        {
            parameters = new TestExecutionParameters();

            // Параметры берутся из командной строки
            if (args.Length == 0)
            {
                Console.WriteLine("There are no CMD line parameters");
                Console.WriteLine("Required argument:");
                Console.WriteLine("    <NameOfTest>");
                Console.WriteLine("    {0}<path to server>", CmdLineParameterNames.Server);
                Console.WriteLine("    {0}<path to hex on a server>", CmdLineParameterNames.ServerPath);
                Console.WriteLine("Optional arguments:");
                Console.WriteLine("    {0}<value in seconds>", CmdLineParameterNames.EndTimeout);
                Console.WriteLine("    {0}<value in seconds>", CmdLineParameterNames.EndTimeout);
                Console.WriteLine(@"example args of command line: testName -server:http://atfs16:8080/tfs -serverPath:$/Kordis50/Product/Firmware/Product_test_A835.hex -StartTimeout:180 -endTimeout:180");

                return SystemParameters.RETURN_CODE_FAIL;
            }

            // Разбор параметров командной строки
            for (uint i = 0; i < args.Length; i++)
            {
                uint value;
                string outString;

                if (CmdParser.GetParameterUnsignedIntegerValueFromStringArgument(args[i], CmdLineParameterNames.StartTimeout, out value) ==
                    CommandLineArgumentsParsingResult.Success)
                    parameters.StartTimeout = value;
                else if (CmdParser.GetParameterUnsignedIntegerValueFromStringArgument(args[i], CmdLineParameterNames.EndTimeout, out value) ==
                    CommandLineArgumentsParsingResult.Success)
                    parameters.EndTimeout = value;
                else if (CmdParser.GetStringArgument(args[i], CmdLineParameterNames.Server, out outString) ==
                     CommandLineArgumentsParsingResult.Success)
                    parameters.Server = outString;
                else if (CmdParser.GetStringArgument(args[i], CmdLineParameterNames.ServerPath, out outString) ==
                     CommandLineArgumentsParsingResult.Success)
                    parameters.ServerPath = outString;
                else if (i == 0)
                {
                    // Первый параметр командной строки - всегда имя теста
                    testName = args[0];
                    parameters.TestName = testName;
                    Console.WriteLine("TestName={0}", testName);
                }
                else
                {
                    Console.WriteLine("Unknown arg: {0}", args[i]);
                }
            }

            // Проверка корректности параметров
            if ((testName.Length == 0) || (parameters.Server == "") || (parameters.ServerPath == ""))
            {
                Console.WriteLine("ERROR: empty test name or server name, or path on a server");
                return SystemParameters.RETURN_CODE_FAIL;
            }

            Console.WriteLine("Initial parameters: \n\tParameters.StartTimeout={0}, \n\tParameters.EndTimeout={1}",
            parameters.StartTimeout, parameters.EndTimeout);

            return SystemParameters.RETURN_CODE_SUCCESS;
        }

        public static TestExecutionParameters GetTestExecutionParameters()
        {
            return parameters;
        }

        /// <summary>
        /// Основные параметры выполнения теста
        /// </summary>
        private static TestExecutionParameters parameters;

        /// <summary>
        /// Допустимые имена аргументов командной строки
        /// </summary>
        private struct CmdLineParameterNames
        {
            static public String StartTimeout = "-startTimeout:";
            static public String EndTimeout = "-endTimeout:";
            static public String Server = "-server:";
            static public String ServerPath = "-serverPath:";
        }
    }
}
