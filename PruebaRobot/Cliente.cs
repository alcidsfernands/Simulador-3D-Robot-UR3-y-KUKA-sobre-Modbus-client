using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Media;
using System.Windows;

namespace Modbus_client
{
    class Cliente
    {
        private Socket clienteTCP = null;
        private IPEndPoint ipep = null;
        private bool conectado = false;

        public Cliente(string dirIP, Int32 puerto)
        {
            try
            {
                IPAddress direccion = IPAddress.Parse(dirIP);
                ipep = new IPEndPoint(direccion, puerto);
                clienteTCP = new Socket(direccion.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clienteTCP.SendTimeout = 500;
                clienteTCP.ReceiveTimeout = 1000;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error general al preparar la conexión al servidor:\n" + ex.ToString(), "Error Cliente TCP");
                throw;   
            }
        }

        public bool conectarAlServidor()
        {
            try
            {
                clienteTCP.Connect(ipep);
            }
            catch (SocketException ex_sock)
            {
                if (ex_sock.SocketErrorCode == SocketError.ConnectionRefused)
                {
                 MessageBox.Show("Error al conetarse al servidor:\nAtes de poner en marcha el cliente se debe poner en funcionamiento el servidor en el nodo cuya IP es:"+ipep.Address.ToString() + ", y el puerto" + ipep.Port.ToString() + "debe poder ser accedido (atencion al \"firewall\")","Error cliente TCP");
                }
                else
                { MessageBox.Show("Error al conetarse al servidor:\n" + ex_sock.ToString(), "Error cliente TCP"); }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Erro general al conectarse  al servido:\n" + ex.ToString(), "Error cliente TCP");
                throw;
            }
            finally
            { 
            if (clienteTCP != null && clienteTCP.Connected) conectado = true;
            else conectado = false;

            }
            return (conectado);
        }
        public void cierraCliente()
        {
            if (clienteTCP != null && conectado)
            {
                clienteTCP.Close();
            }
            clienteTCP = null;
            conectado = false;
            return;
        }
        public int enviaDatos(byte[] datos, int dim)
        {
            try
            {
                if (conectado)
                {
                    int res = clienteTCP.Send(datos, dim, SocketFlags.None);
                    if (res == dim)
                        return (res);
                    else if (res == 0)
                    {
                        clienteTCP = null;
                        conectado = false;
                        return (-1);
                    }
                    else
                        return (-2);
                }
                else
                    return (-1);

            }
            catch (SocketException ex_sock)
            {
                if (ex_sock.SocketErrorCode == SocketError.TimedOut)
                    return (0);
                else if (ex_sock.SocketErrorCode == SocketError.ConnectionReset)
                {
                    clienteTCP = null;
                    conectado = false;
                    return (-1);
                }
                else
                    return (-2);
            }
            catch (Exception ex)
            {
                return (-2);
            }
        }
        public int recebeDatos(byte[] datos, int dimMax)
        {
            try
            {
                if (conectado)
                {
                    int res = clienteTCP.Receive(datos, dimMax, SocketFlags.None);
                    if (res > 0)
                        return (res);
                    else if (res == 0)
                    {
                        clienteTCP = null;
                        conectado = false;
                        return (-1);
                    }
                    else
                        return (-2);
                }
                else
                    return (-1);
            }
            catch (SocketException ex_sock)
            {
                if (ex_sock.SocketErrorCode == SocketError.TimedOut)
                    return (0);
                else if (ex_sock.SocketErrorCode == SocketError.ConnectionReset)
                {
                    clienteTCP = null;
                    conectado = false;
                    return (-1);
                }
                else
                    return (-2);
            }
            catch (Exception ex)
            {
                return (-2);
            }
        }
    }
}
