﻿using MarketWorkBd.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarketWorkBd.Server
{
    public class Server
    {
        static TcpListener tcpListener; // сервер для прослушивания
        List<Client> clients = new List<Client>(); // все подключения

        /// <summary>
        /// Добавление подключенного клиента в коллекцию
        /// </summary>
        /// <param name="clientObject">Подключенный клиент</param>
        protected internal void AddConnection(Client clientObject)
        {
            clients.Add(clientObject);
        }

        /// <summary>
        /// Удаление соединения с клиентом по id
        /// </summary>
        /// <param name="id">ID удаляемого клиента</param>
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            Client client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }

        /// <summary>
        /// Прослушивание входящих подключений и обработка клиентов в отдельных потоках
        /// </summary>
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                MyLogger.getMyLoggerInstance().info("Сервер запущен. Ожидание подключений...");
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    MyLogger.getMyLoggerInstance().info("Подключаем клиента.");

                    Client clientObject = new Client(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        /// <summary>
        /// Присылает сообщения для клиента
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="id">Id клиента</param>
        protected internal void MessageToClient(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id) // если id клиента не равно id отправляющего
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                    MyLogger.getMyLoggerInstance().info("Передано сообщение: " + message) ;
                    break;
                }
            }
        }

        /// <summary>
        /// Отключение всех клиентов
        /// </summary>
        protected internal void Disconnect()
        {

            tcpListener.Stop(); //остановка сервера
            MyLogger.getMyLoggerInstance().info("Сервер остановлен.");
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }
    }
}
