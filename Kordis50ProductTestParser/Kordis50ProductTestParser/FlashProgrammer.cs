/*-----------------------------------------------------------------------------
FlashProgrammer.cs

Класс для загрузки прошивки в память МК.
Используется стороння программа nrfjprog.
-----------------------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Kordis50ProductTestParser
{
    class FlashProgrammer
    {
        public static int ProgrammFlash()
        {
            // 1. Очистка памяти
            int res = ExecuteNrfjprogProcess("--eraseall");
            if (res != 0)
                return SystemParameters.RETURN_CODE_FAIL;

            // 2. Загрузка прошивки в память
            string localFilePath = Path.GetTempPath() + "tempHex.hex";
            TestExecutionParameters parameters = TestParametersReader.GetTestExecutionParameters();

            try
            {
                // Получение ссылки на Team Foundation Server. 
                var tfs = new TeamFoundationServer(parameters.Server);

                // Получение ссылки на Version Control. 
                var versionControl = (VersionControlServer)tfs.GetService(typeof(VersionControlServer));
                versionControl.DownloadFile(parameters.ServerPath, 0, VersionSpec.Latest, localFilePath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"TFS error: {ex.Message}");
                return SystemParameters.RETURN_CODE_FAIL;
            }

            string arg = "--program " + localFilePath;
            res = ExecuteNrfjprogProcess(arg);

            // Удаляем временный файл
            File.Delete(localFilePath);

            if (res != 0)
                return SystemParameters.RETURN_CODE_FAIL;

            // 3. Перезапуск
            res = ExecuteNrfjprogProcess("--reset");
            if (res != 0)
                return SystemParameters.RETURN_CODE_FAIL;

            return SystemParameters.RETURN_CODE_SUCCESS;
        }

        private static int ExecuteNrfjprogProcess(string arg )
        {
            Process process = new Process();

            process.StartInfo.FileName = SystemParameters.NRFJPROG_PATH;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = false;
            process.StartInfo.Arguments = arg;

            process.Start();
            process.WaitForExit(20000);

            return process.ExitCode;
        }
    }
}
