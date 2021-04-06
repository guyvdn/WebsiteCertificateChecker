using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace WebsiteCertificateChecker
{
    internal static class Program
    {
        private const int LineWidth = 80;
        private static int _errorCount;

        private static void Main()
        {
            WriteLine(" Website Certificate Checker ", ConsoleColor.DarkCyan);
            WriteLine(new string('=', LineWidth), ConsoleColor.DarkCyan);

            foreach (var website in WebsitesToCheck())
            {
                CheckWebsite(website);
            }

            WriteLine($"Found {_errorCount} websites with errors", ConsoleColor.DarkCyan);

            if (_errorCount > 0)
                Console.ReadLine();
        }

        private static IEnumerable<string> WebsitesToCheck()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
                .Build();

            return configuration.GetSection("Websites").Get<List<string>>();
        }

        private static void CheckWebsite(string website)
        {
            WriteLine("Checking website: " + website, ConsoleColor.Yellow);

            var request = WebRequest.CreateHttp(website);

            request.ServerCertificateValidationCallback += ServerCertificateValidationCallback;

            try
            {
                using var response = (HttpWebResponse)request.GetResponse();
                WriteLine("Certificate OK", ConsoleColor.DarkGreen);
            }
            catch
            {
                _errorCount++;
                WriteLine("Certificate NOT OK", ConsoleColor.DarkRed);
            }

            WriteLine(new string('-', LineWidth), ConsoleColor.DarkCyan);
        }

        private static bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            WriteLine($"Certificate valid from {certificate.GetEffectiveDateString()} until {certificate.GetExpirationDateString()}", ConsoleColor.DarkGray);
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        private static void WriteLine(object value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ResetColor();
        }
    }
}
