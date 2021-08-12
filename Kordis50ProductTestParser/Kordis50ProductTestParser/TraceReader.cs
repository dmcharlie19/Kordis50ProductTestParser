/*-----------------------------------------------------------------------------
TraceReader.cs

Класс отвечающий за чтение трассировки из файла лога.
Отслеживается время и прогресс выполнения теста, а также его результат
-----------------------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Kordis50ProductTestParser
{
    class TraceReader
    {
        private static TestExecutionStates _currentState;    // текущее состояние теста
        private static int _lineCounter;                     // счетчик строк в файле трассировки
        private static Process _rttLoggerProcess; 

        // Имена временных файлов,необходимых для работы
        public const string LOG_FILE_NAME = "logFile.txt";
        public const string LOG_FILE_NAME_COPY = "logFileNameCopy.txt";

        public static int ReadTrace()
        {
            TestExecutionParameters parameters = TestParametersReader.GetTestExecutionParameters();
            var initialDateTime = DateTime.Now;
            int returnCode = SystemParameters.RETURN_CODE_SUCCESS;

            // Запуск процесса rttLogger
            _rttLoggerProcess = new Process();

            returnCode = StartRttLoggerProcess();
            if (SystemParameters.FAILED(returnCode))
                return returnCode;

            // Подготовка параметров
            _lineCounter = 0;
            _currentState = TestExecutionStates.NotStarted;
            File.Delete(LOG_FILE_NAME_COPY);

            // Ожидаем начало выполнения теста
            if (!WaitForState(TestExecutionStates.Started, parameters.StartTimeout))
            {
                // Тест не запустился за отведенное время
                Console.WriteLine("ERROR: timeout start, timeout={0}, duration={1}", parameters.StartTimeout, DateTime.Now - initialDateTime);
                returnCode = SystemParameters.RETURN_CODE_FAILED_START_TEST;
            }

            if (SystemParameters.SUCCESSED(returnCode))
            {
                // Тест начал выполняться, ожидаем его завершения
                if (!WaitForState(TestExecutionStates.StopFailed, parameters.EndTimeout))
                {
                    // Тест не завершился за отведенное время
                    Console.WriteLine("ERROR:  timeout end, timeout={0}, duration={1}", parameters.EndTimeout, DateTime.Now - initialDateTime);
                    returnCode = SystemParameters.RETURN_CODE_FAILED_STOP_TEST;
                }
            }

            if (SystemParameters.SUCCESSED(returnCode))
            {
                // Тест завершился. Проверяем код результата
                if (_currentState == TestExecutionStates.StopSucceeded)
                {
                    // Тест выполнен успешно
                    Console.WriteLine("test PASSED, dt={0}", DateTime.Now);
                    returnCode = SystemParameters.RETURN_CODE_SUCCESS;
                }
                else
                {
                    // Тест завершился ошибкой
                    Console.WriteLine("ERROR: test FAILED, dt={0}", DateTime.Now);
                    returnCode = SystemParameters.RETURN_CODE_TEST_FAILED;
                }
            }

            try
            {
                // Остановка процесса
                StopRttLoggerProcess();
            }
            catch
            {
                // Если не удалось завершить процесс JLinkRTTLogger, значит он уже завершен.
            }

            SaveLogFile();
            File.Delete(LOG_FILE_NAME);

            return returnCode;
        }

        // Обработчик выходного потока данных процесса Rtt Logger, необходим для корректной работы процесса.
        private static void OutputStreamHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Ничего не делаем
        }

        // Запуск и настройка процесса Rtt Logger
        private static int StartRttLoggerProcess()
        {
            TryKillExistRTTLoggerProcess();

            try
            {
                File.Delete(LOG_FILE_NAME);
            }
            catch
            {
                Console.Error.WriteLine("ERROR! try to kill old RTT Logger Process");
                return SystemParameters.RETURN_CODE_FAIL;
            }

            _rttLoggerProcess.StartInfo.FileName = SystemParameters.RTT_LOGGER_PATH;
            _rttLoggerProcess.StartInfo.UseShellExecute = false;
            _rttLoggerProcess.StartInfo.RedirectStandardInput = true;
            _rttLoggerProcess.StartInfo.RedirectStandardOutput = true;
            _rttLoggerProcess.OutputDataReceived += OutputStreamHandler;
            _rttLoggerProcess.Start();

            // Запускаем ассинхронное чтение выходного потока данных процесса
            _rttLoggerProcess.BeginOutputReadLine();

            StreamWriter streamWriter = _rttLoggerProcess.StandardInput;

            streamWriter.WriteLine("nRF52832_xxAA");
            streamWriter.WriteLine("SWD");
            streamWriter.WriteLine(4000);
            streamWriter.WriteLine();
            streamWriter.WriteLine(0);
            streamWriter.WriteLine(LOG_FILE_NAME);

            Thread.Sleep(1500);
            streamWriter.Close();

            return SystemParameters.RETURN_CODE_SUCCESS;
        }

        public static void StopRttLoggerProcess()
        {
            try
            {
                _rttLoggerProcess.CancelOutputRead();
                _rttLoggerProcess.Kill();
                _rttLoggerProcess.WaitForExit(30000);
            }
            catch
            {
                Console.WriteLine("Failed Stop Rtt Logger Process");
            }
        }

        private static bool WaitForState(TestExecutionStates state, uint timeoutSeconds)
        {
            bool isStateReached = false;
            DateTime initialDateTime = DateTime.Now;

            while ((DateTime.Now - initialDateTime).TotalSeconds < timeoutSeconds)
            {
                if (File.Exists(LOG_FILE_NAME))
                {
                    File.Copy(LOG_FILE_NAME, LOG_FILE_NAME_COPY);
                    StreamReader sr = new StreamReader(LOG_FILE_NAME_COPY);

                    string line;
                    int i = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Анализируем только новые записанные строки
                        i++;
                        if (i > _lineCounter)
                        {
                            _lineCounter++;

                            if(line != "" && line[0] == 'E')
                                System.Console.Error.WriteLine("    "+line);
                            else
                                System.Console.WriteLine("    " + line);

                            if (TestStateAnalizer(line) >= state)
                            {
                                // Если ожидаемое состояние достигнуто
                                isStateReached = true;
                                break;
                            }
                        }
                    }

                    sr.Close();
                    Thread.Sleep(500);
                    File.Delete(LOG_FILE_NAME_COPY);
                    if (isStateReached)
                        return true;

                    // Если количество строк в логе больше максимального,
                    // делаем вывод о зависании встроенного ПО
                    if (i > SystemParameters.MAX_LINES_IN_LOG)
                        return false;
                }

            }
            return false;
        }

        private static TestExecutionStates TestStateAnalizer(string str)
        {
            TestExecutionParameters parameters = new TestExecutionParameters();

            if (str.Contains(parameters.START_TEST_VALUE))
                _currentState = TestExecutionStates.Started;

            else if (str.Contains(parameters.END_TEST_FAILED_VALUE))
                _currentState = TestExecutionStates.StopFailed;

            else if (str.Contains(parameters.END_TEST_SUCCEEDED_VALUE))
                _currentState = TestExecutionStates.StopSucceeded;

            return _currentState;
        }

        private static void SaveLogFile()
        {
            if (!Directory.Exists("Logs"))
                Directory.CreateDirectory("Logs");

            TestExecutionParameters parameters = TestParametersReader.GetTestExecutionParameters();

            string date = DateTime.Now.ToString();
            date = date.Replace('.', '_');
            date = date.Replace(':', '_');
            date = date.Replace(' ', '_');

            string path = @"Logs\" + parameters.TestName + "_" + date + ".log";

            if (File.Exists(LOG_FILE_NAME))
                File.Copy(LOG_FILE_NAME, path);

            File.Delete(LOG_FILE_NAME);
        }

        private static void TryKillExistRTTLoggerProcess()
        {
            Process[] processes = Process.GetProcessesByName("JLinkRTTLogger"); // Получим все процессы Google Chrome

            foreach (Process process in processes) // В цикле их переберём
            {
                process.Kill(); // завершим процесс
                process.WaitForExit();
            }
        }
    }
}
