/*-----------------------------------------------------------------------------
TestExecutionParameters.cs

Параметры прохождения тестирования
-----------------------------------------------------------------------------*/
using System;

namespace Kordis50ProductTestParser
{
    public class TestExecutionParameters
    {
        public string TestName      = "";
        public uint StartTimeout    = 10;
        public uint EndTimeout      = 70;
        public string Server        = "";
        public string ServerPath    = "";

        public String START_TEST_VALUE			= "UnitTestSuite_Started";
        public String END_TEST_FAILED_VALUE		= "UnitTestSuite_Failed";
        public String END_TEST_SUCCEEDED_VALUE	= "UnitTestSuite_Success";
    }
}
