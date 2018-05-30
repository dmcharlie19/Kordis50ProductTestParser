using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Menu();
        }

        public static void Menu()
        {
            Console.Clear();
            Console.WriteLine("1 - зашифровать");
            Console.WriteLine("2 - дешифровать");
            Console.WriteLine("3 - выход");

            while (true)
            {
                char key = (char)Console.Read();
                switch (key)
                {
                    case '1':
                        Shifr();
                        break;
                    case '2':
                        Deshifr();
                        break;
                    case '3':
                        Environment.Exit(0);
                        break;
                }
            }
 
        }

        public static void Deshifr()
        {
            string alphabet = "абвгдежзийклмнопрстуфхцчшщъыьэюя";
            Console.Clear();
            int n=0;
            Console.WriteLine("введите зшифрованное сообщение");
            string str = "";
            while (str == "")
                str = Console.ReadLine();

            string result="";
            while (n != alphabet.Length)
            {
                Console.Write("смещение "); Console.WriteLine(alphabet.Length - n);
                for (int i = 0; i < str.Length; i++)
                {
                    int index = 0;

                    for (int j = 0; j < alphabet.Length; j++)
                    {
                        if (str[i] == ' ')
                        {
                            result = result + ' ';
                            break;
                        }

                        if (alphabet[j] == str[i])
                        {

                            index = j + n;
                            if (index >= alphabet.Length) index = index - alphabet.Length;
                            result = result + alphabet[index];

                            break;
                        }
                    }

                }
                Console.WriteLine(result);
                Console.WriteLine();
                result = "";
                n++;
            }
                Console.WriteLine("1 -в меню");
            while ((Console.ReadKey().Key != ConsoleKey.D1))
            { }
            Menu();
        }
        public static void Shifr()
        {
            string alphabet = "абвгдежзийклмнопрстуфхцчшщъыьэюя";
            int n, k;
            string ch="";
            Console.Clear();

            Console.WriteLine("введите сообщение");
            string str="";
            while (str=="")
                str = Console.ReadLine();

            while (true)
            {
                Console.WriteLine("введите смещение");
                ch = Console.ReadLine();
                bool isNum = int.TryParse(ch, out n);
                if (isNum)                
                    break;
                else
                    Console.WriteLine("введите число");         
            }
            Console.WriteLine(n);
            string result = "";

            for (int i = 0; i < str.Length; i++)
            {
                int index = 0;
                
                for (int j = 0; j < alphabet.Length; j++)
                {
                    if (alphabet.IndexOf(str[i])==-1)
                    {
                        result = result + str[i];
                        break;
                    }

                    if (alphabet[j] == str[i])
                    {

                        index = j + n;
                        if (index >= alphabet.Length) index = index - alphabet.Length;
                        result = result + alphabet[index];
                        break;
                    }
                }

            }
            
            Console.WriteLine(result);
            Console.WriteLine("1 -в меню");

            while ((Console.ReadKey().Key != ConsoleKey.D1))
            { }
            Menu();
        }
    }
}
