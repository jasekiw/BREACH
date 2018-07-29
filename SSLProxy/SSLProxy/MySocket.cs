using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SSLProxy
{
    class MySocket
    {
        private Socket _socket;
        private  ManualResetEvent connectDone = new ManualResetEvent(false);
        private bool _isConnected;
        public bool IsConnected => _isConnected;

        private string _lastErrorText = "";
        public string LastErrorText => _lastErrorText;
        private string name = "SOCKET";

        public int MaxReadIdleMs
        {
            get => _socket.ReceiveTimeout;
            set => _socket.ReceiveTimeout = value;
        }

        public int MaxSendIdleMs
        {
            get => _socket.SendTimeout;
            set => _socket.SendTimeout = value;
        }


        private bool _asyncReceiveFinished;
        public bool AsyncReceiveFinished => _asyncReceiveFinished;
        
        private byte[] _asyncReceivedBytes;
        public byte[] AsyncReceivedBytes => _asyncReceivedBytes;

        public MySocket(Socket socket, string name)
        {
            socket.Blocking = false;
            _isConnected = true;
            _socket = socket;
            this.name = name;
        }


        public MySocket()
        {

        }


        public MySocket(string name)
        {
            this.name = name;
        }


        public bool Connect(string strIp, int port, bool ssl, int maxWait)
        {
            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the   
                // remote device is "host.contoso.com".  
                IPAddress ipAddress = IPAddress.Parse(strIp);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                _socket.BeginConnect(remoteEP, ConnectCallback, _socket);
                connectDone.WaitOne();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                _lastErrorText = e.ToString();
                throw e;
                return false;
            }
        }


        public bool BindAndListen(int port, int backlog)
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            IPAddress ipAddress = IPAddress.Loopback;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                _socket.Blocking = true;
                _socket.Bind(localEndPoint);
                _socket.Listen(backlog);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString(), e.StackTrace);
                _lastErrorText = e.ToString();
                throw e;
                return false;
            }
        }


        public MySocket AcceptNextConnection(int maxWait)
        {
            return new MySocket(_socket.Accept(), "accepted_connection");
        }



        public bool SendBytes(byte[] bytes)
        {
            // Connect to a remote device.  
            try
            {

                Send(_socket, bytes);
                
                // Write the response to the console.  
                //Console.WriteLine("Response received : {0}", response);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                _lastErrorText = e.ToString();
                throw e;
                return false; 
            }
        }

        public void Close(int maxWait)
        {
            // Release the socket.  
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _isConnected = false;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;
               
                // Complete the connection.  
                client.EndConnect(ar);
                this._isConnected = true;
                connectDone.Set();
                Console.WriteLine("Socket connected to {0}",client.RemoteEndPoint.ToString());

                
            }
            catch (Exception e)
            {
                _lastErrorText = e.ToString();
                Console.WriteLine(e.ToString(), e.StackTrace);
                throw e;
            }
        }


        public bool AsyncReceiveBytes()
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = _socket;

                // Begin receiving the data from the remote device.  
                _socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString(), e.StackTrace);
                _lastErrorText = e.ToString();
                throw e;
                return false;
            }
        }

    

        private void ReceiveCallback(IAsyncResult ar)
        {
          
                Console.WriteLine(this.name + " started receiving bytes");
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    byte[] temp = state.total;
                    state.total = new byte[state.total.Length + bytesRead];
                    Array.Copy(temp, state.total, temp.Length);
                    Array.Copy(state.buffer, 0, state.total, temp.Length, bytesRead);

//                    Console.WriteLine("BeginReceive Again");
                // Get the rest of the data.  
                // inserted large buffer to prevent this.
                //                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                    Console.WriteLine(this.name + " finished receiving bytes");
                    _asyncReceivedBytes = state.total;
                    _asyncReceiveFinished = true;
            }
                else
                {
                    Console.WriteLine(this.name + " finished receiving bytes");
                    _asyncReceivedBytes = state.total;
                    _asyncReceiveFinished = true;
                }
       
        }

        private void Send(Socket client, byte[] byteData)
        {
            // Convert the string data to byte data using ASCII encoding.  
            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

            }
            catch (Exception e)
            {
                _lastErrorText = e.ToString();
                Console.WriteLine(e.ToString(), e.StackTrace);
                throw e;
            }
        }


    }

    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 8192;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        public byte[] total = new byte[0];
        
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }
}

