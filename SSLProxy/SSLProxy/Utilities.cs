﻿using System;
using System.IO;
using System.Threading;
using Socket = Chilkat.Socket;

namespace SSLProxy
{
    partial class Program
    {
        internal static Socket InitializeSocketLibrary()
        {
            Chilkat.Socket listenSocket = new Chilkat.Socket();
            if (listenSocket.UnlockComponent("Start my 30-day Trial") != true)
            {
                throw new Exception("Failed to unlock component");
            }

            return listenSocket;
        }

        internal static void LogPacketLength(String fileName, String logData, FileMode fm, FileAccess fa, FileShare fs)
        {
            bool writeCompleted = false;
            do
            {
                try
                {
                    using (FileStream fileStream = new FileStream(fileName, fm, fa, fs))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            writer.WriteLine(logData);
                            writer.Close();
                            writeCompleted = true;
                        }
                    }
                }
                catch (Exception)
                {
                    Thread.Sleep(1);
                }
            } while (!writeCompleted);
        }


        static void Log(params string[] message)
        {
            return;
            for (int i = 0; i < message.Length; i++)
            {
                Console.Write(message[i] + " ");
            }
            Console.WriteLine();
        }

    }
}
