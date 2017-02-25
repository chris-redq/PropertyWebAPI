//-----------------------------------------------------------------------
// <copyright file="CasesController.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CreateKey
{
    class Program
    {
        static int GetRandomKeySize()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            byte[] buff = new byte[4];
            rng.GetBytes(buff);

            return ((BitConverter.ToInt32(buff, 0) % 32)+32);
        }

        static String CreateKey(int numBytes)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            
            byte[] buff = new byte[numBytes];

            rng.GetBytes(buff);
            return BytesToHexString(buff);
        }

        static String EncodeKey(string key, string salt)
        {
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] plainKeyWithSaltBytes = new byte[key.Length + salt.Length];
            byte[] plainKey = Encoding.ASCII.GetBytes(key);
            byte[] plainSalt = Encoding.ASCII.GetBytes(salt);

            for (int i = 0; i < plainKey.Length; i++)
            {
                plainKeyWithSaltBytes[i] = plainKey[i];
            }
            for (int i = 0; i < plainSalt.Length; i++)
            {
                plainKeyWithSaltBytes[plainKey.Length + i] = plainSalt[i];
            }

            return BytesToHexString(algorithm.ComputeHash(plainKeyWithSaltBytes));
        }

        static String BytesToHexString(byte[] bytes)
        {
            StringBuilder hexString = new StringBuilder(64);

            for (int counter = 0; counter < bytes.Length; counter++)
            {
                hexString.Append(String.Format("{0:X2}", bytes[counter]));
            }
            return hexString.ToString();
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:\n   Createkey /k <key size in bytes> <salt>\n   Createkey /e <original key> <salt>");
        }

        static void Main(string[] args)
        {
            String[] commandLineArgs = System.Environment.GetCommandLineArgs();
            if (commandLineArgs.Length < 3)
                PrintUsage();
            else
            {   switch (commandLineArgs[1])
                {
                    case "/k":
                        if (commandLineArgs.Length < 4)
                        {
                            PrintUsage();
                        }
                        else
                        {
                            try
                            {
                                string key = CreateKey(System.Convert.ToInt32(commandLineArgs[2]));
                                string salt = commandLineArgs[3];
                                Console.WriteLine("Key: " + key);
                                Console.WriteLine("Salt: " + salt);
                                Console.WriteLine("Encoded Key: " + EncodeKey(key, salt));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error:" + e.Message);
                                PrintUsage();
                            }
                        }
                        break;
                    case "/r":
                        try
                        {
                            string key = CreateKey(System.Convert.ToInt32(GetRandomKeySize()));
                            string salt = commandLineArgs[2];
                            Console.WriteLine("Key: " + key);
                            Console.WriteLine("Salt: " + salt);
                            Console.WriteLine("Encoded Key: " + EncodeKey(key, salt));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error:" + e.Message);
                            PrintUsage();
                        }
                        break;
                    case "/e":
                        if (commandLineArgs.Length < 4)
                        {
                            PrintUsage();
                        }
                        else
                        {
                            string key = commandLineArgs[2];
                            string salt = commandLineArgs[3];
                            Console.WriteLine("Key: " + key);
                            Console.WriteLine("Salt: " + salt);
                            Console.WriteLine("Encoded Key: " + EncodeKey(key, salt));
                        }
                        break;
                    default:
                        PrintUsage();
                        break;
                }
            }
        }
    }
}
