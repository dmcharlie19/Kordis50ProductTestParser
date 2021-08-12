/*-----------------------------------------------------------------------------
Program.cs

-----------------------------------------------------------------------------*/
using System;

namespace Kordis50ProductTestParser
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Kordis50ProductTestParser start, dt={0}", DateTime.Now);

            // Разбор параметров командной строки
            int res = TestParametersReader.ReadTestParameters(args);
            if (SystemParameters.FAILED(res))
            {
                System.Console.Error.WriteLine("Failed");
#if DEBUG
                Console.ReadKey(true);
#endif
                return SystemParameters.RETURN_CODE_FAILED_ARG_PARAMETR;
            }

            // Прошивка платы
            res = FlashProgrammer.ProgrammFlash();
            if (SystemParameters.FAILED(res))
            {
                System.Console.Error.WriteLine("Failed");
#if DEBUG
                Console.ReadKey(true);
#endif
                return SystemParameters.RETURN_CODE_FAILED_PROGRAMM_FIRMWARE;
            }

            // Чтение и анализ трассировки
            res = TraceReader.ReadTrace();
            if (SystemParameters.FAILED(res))
            {
                System.Console.Error.WriteLine("Failed");
#if DEBUG
                Console.ReadKey(true);
#endif
                return res;
            }

#if DEBUG
            Console.ReadKey(true);
#endif
            return SystemParameters.RETURN_CODE_SUCCESS;
        }
    }
}
