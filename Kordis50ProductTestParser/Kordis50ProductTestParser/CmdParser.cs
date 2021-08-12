/*-----------------------------------------------------------------------------
CmdParser.cs

Класс для получения параметров теста из аргументов командной строки
-----------------------------------------------------------------------------*/
using System;

namespace Kordis50ProductTestParser
{
    class CmdParser
    {
        public static CommandLineArgumentsParsingResult GetParameterUnsignedIntegerValueFromStringArgument(String argumentString, String parameterName, out uint parameter)
        {
            uint value = 0;
            parameter = value;

            if (argumentString.Length == 0 || parameterName.Length == 0)
            {
                return CommandLineArgumentsParsingResult.IncorrectSyntax;
            }

            if (!argumentString.Contains(parameterName))
                return CommandLineArgumentsParsingResult.ArgumentNotFound;

            var index = argumentString.IndexOf(parameterName) + parameterName.Length;
            if (index < 0)
                return CommandLineArgumentsParsingResult.IncorrectSyntax;

            var parameterValueText = argumentString.Substring(index);
            try
            {
                parameter = Convert.ToUInt32(parameterValueText);
            }
            catch (System.Exception)
            {
                return CommandLineArgumentsParsingResult.IncorrectSyntax;
            }

            return CommandLineArgumentsParsingResult.Success;
        }

        public static CommandLineArgumentsParsingResult GetStringArgument(String argumentString, String parameterName, out string parameter)
        {
            string value = "";
            parameter = value;

            if (argumentString.Length == 0 || parameterName.Length == 0)
            {
                return CommandLineArgumentsParsingResult.IncorrectSyntax;
            }

            if (!argumentString.Contains(parameterName))
                return CommandLineArgumentsParsingResult.ArgumentNotFound;

            var index = argumentString.IndexOf(parameterName) + parameterName.Length;
            if (index < 0)
                return CommandLineArgumentsParsingResult.IncorrectSyntax;

            parameter = argumentString.Substring(index);

            return CommandLineArgumentsParsingResult.Success;
        }
    }
}
