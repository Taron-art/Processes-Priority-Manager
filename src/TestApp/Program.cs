﻿namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                Console.WriteLine(arg);
            }

            Console.ReadLine();
        }
    }
}
