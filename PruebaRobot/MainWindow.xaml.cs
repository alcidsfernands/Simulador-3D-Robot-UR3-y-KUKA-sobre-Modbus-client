using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using CoppeliaLib;
using System.Timers;
using System.Windows.Threading;
using System.Globalization;

namespace Modbus_client
{

	public partial class MainWindow : Window
	{
		private Cliente clie = null;
		private Cliente clie2 = null;
		private Peticion_datos peticionRegistro = null;
		private ushort nMensaje = 0;
		private FunCoppelia sim = null;
		private static System.Timers.Timer aTimer;
		bool conectadoPrimeiraVez = false;
		bool conectadoPrimeiraVez2 = false;
		int ip=502;
		bool ur3_check_aux ;
		bool kuka_check_aux;

		float axi0;
		float axi1;
		float axi2;
		float axi3;
		float axi4;
		float axi5;
		double deb;
		public MainWindow()
			
		{
			InitializeComponent();
			nRegistro_multiploReg_interno.Text = string.Empty;
			sim = new FunCoppelia();

			aTimer = new System.Timers.Timer(500);
			aTimer.Elapsed += OnTimedEvent;
			aTimer.AutoReset = true;
			aTimer.Enabled = true;

			
		}
		private void conectar_Click(object sender, RoutedEventArgs e)
		{
            if (ur3_select.IsChecked == true)
            {
				ip = 502;
            }
			if (kuca_select.IsChecked == true)
			{
				ip = 7000;
			}
			clie = new Cliente(IP_dir1.Text, ip);
			if (clie.conectarAlServidor())
			{
				BitmapImage bi1 = new BitmapImage();
				bi1.BeginInit();
				if (IP_dir1.Text == "172.21.10.2")
				{
					bi1.UriSource = new Uri("/KEY0.CC-Ur3-Robot-Ur3e.png", UriKind.Relative);
				}
				else
					bi1.UriSource = new Uri("/w-Workstation Filled-100.png", UriKind.Relative);
				bi1.EndInit();
				Device1.Stretch = Stretch.Fill;
				Device1.Source=bi1;
				SetTimer();
				desconectar1.Visibility = Visibility.Visible;
				conectar1.Visibility = Visibility.Hidden;
				conectadoPrimeiraVez = true;
			}
		}

		private void conectar2_Click(object sender, RoutedEventArgs e)
		{
			if (ur3_select.IsChecked == true)
			{
				ip = 502;
			}
			if (kuca_select.IsChecked == true)
			{
				ip = 7000;
			}
			clie2 = new Cliente(IP_dir2.Text, ip);
			if (clie2.conectarAlServidor())
			{
				BitmapImage bi1 = new BitmapImage();
				bi1.BeginInit();
                if (IP_dir2.Text == "172.21.10.2")
                {
					bi1.UriSource = new Uri("/KEY0.CC-Ur3-Robot-Ur3e.png", UriKind.Relative);
				}
				else
					bi1.UriSource = new Uri("/w-Workstation Filled-100.png", UriKind.Relative);
				bi1.EndInit();
				Device1.Stretch = Stretch.Fill;
				Device2.Source = bi1;
				desconectar2.Visibility = Visibility.Visible;
				conectar2.Visibility = Visibility.Hidden;
				conectadoPrimeiraVez2 = true;
			}
		}



		private void peticion_salidas_discretas_Click(object sender, RoutedEventArgs e)
		{
			if (clie!=null)
			{

				ushort nSalidas = (ushort)Convert.ToUInt16(Ultimo_Registro.Text);
				int nBytesEnterosSalidas = nSalidas / 8;
				int nBytesIncompletosSalidas = nSalidas % 8 > 0 ? 1 : 0;
				int nBytesSalidas = nBytesEnterosSalidas + nBytesIncompletosSalidas;

				peticionRegistro = new Peticion_datos();
				byte[] bytes_datos = new byte[12];
				byte[] bytes_respuesta = new byte[256];

				bytes_datos =(byte[]) peticionRegistro.PETICION(nMensaje, 1, Convert.ToInt16(primeiro_registro.Text), Convert.ToInt16(Ultimo_Registro.Text));
				int res=clie.enviaDatos(bytes_datos,12);

				if (res == 12)
				{
					res = clie.recebeDatos(bytes_respuesta, 256);
					

					if (res == 9 + nBytesSalidas)
					{
						List<datosGrid> lista = new List<datosGrid>();
						datosGrid elemento;
						bool[] temp;

						int k = Convert.ToInt32(primeiro_registro.Text) - 1;
						int maxBits;

						for (int i = 0; i < bytes_respuesta[8]; i++)
						{
							temp = extraeBits(bytes_respuesta[9 + i], 8);
							if (i < nBytesEnterosSalidas) maxBits = 8;
							else maxBits = nSalidas % 8;
							for (int j = 0; j < 8; j++)
							{
								elemento = new datosGrid();
								elemento.nElemento = k++;
								elemento.Estado = temp[j];
								lista.Add(elemento);

							}
						}
					   respuesta_salidas_discretas.ItemsSource = lista;
					}
				}


			}
			return;
		}
		private bool[] extraeBits(byte valor, int n)
		{
			bool[] sol = new bool[8];
			byte mascara = 0x01;
			for (int i = 0; i < n; i++)
			{
				if ((valor & mascara) != 0) sol[i] = true;
				else sol[i] = false;
				mascara = (byte)(mascara << 1);
			}
			for (int i = n; i < 8; i++) sol[i] = false;
			return (sol);
		}
        private void peticion_entrada_discretas_Click(object sender, RoutedEventArgs e)
        {

			ushort nEntradas = (ushort)Convert.ToUInt16(Ultimo_entrada.Text);
			int nBytesEnterosEntradas = nEntradas / 8;
			int nBytesIncompletosEntradas = nEntradas % 8 > 0 ? 1 : 0;
			int nBytesEntradas = nBytesEnterosEntradas + nBytesIncompletosEntradas;

			Peticion_datos peticionEntradas = new Peticion_datos();
			byte[] bytes_datos = new byte[12];
			byte[] bytes_respuesta = new byte[256];

			bytes_datos = (byte[])peticionEntradas.PETICION(nMensaje, 2, Convert.ToInt16(primeira_entrada.Text), Convert.ToInt16(Ultimo_entrada.Text));
			int res = clie.enviaDatos(bytes_datos, 12);

			if (res == 12)
			{
				res = clie.recebeDatos(bytes_respuesta, 256);

				if (res == 9 + nBytesEntradas)
				{
					List<datosGrid> lista = new List<datosGrid>();
					datosGrid elemento;
					bool[] temp;

					int k = Convert.ToInt32(primeira_entrada.Text) - 1;
					int maxBits;

					for (int i = 0; i < bytes_respuesta[8]; i++)
					{
						temp = extraeBits(bytes_respuesta[9 + i], 8);
						if (i < nBytesEnterosEntradas) maxBits = 8;
						else maxBits = nEntradas % 8;
						for (int j = 0; j < 8; j++)
						{
							elemento = new datosGrid();
							elemento.nElemento = k++;
							elemento.Estado = temp[j];
							lista.Add(elemento);

						}
					}
					respuesta_entrada_discretas.ItemsSource = lista;
				}
			}
		}
        private void peticion_registros_internos_Click(object sender, RoutedEventArgs e)
        {
			if (clie != null)
			{
				ushort nRegistros = (ushort)Convert.ToUInt16(nRegistro_interno.Text);
				int nBytesEnterosRegistro = nRegistros / 8;
				int nBytesIncompletosRegistro = nRegistros % 8 > 0 ? 1 : 0;
				int nBytesRegistro = nBytesEnterosRegistro + nBytesIncompletosRegistro;

				peticionRegistro = new Peticion_datos();
				byte[] bytes_datos = new byte[12];
				byte[] bytes_respuesta = new byte[256];

				bytes_datos = (byte[])peticionRegistro.PETICION(nMensaje, 3, Convert.ToInt32(primeiro_Registro_interno.Text), Convert.ToInt16(nRegistro_interno.Text));
				int res = clie.enviaDatos(bytes_datos, 12);


				if (res == 12)
				{
					res = clie.recebeDatos(bytes_respuesta, bytes_respuesta.Length);

					if (res == 9 +bytes_respuesta[8])
					{
						List<datosGrid_reg> lista = new List<datosGrid_reg>();
						int k = Convert.ToInt32(primeiro_Registro_interno.Text) - 1;
						int J=0;

						for (int i = 0; i <bytes_respuesta[8]/2; i++)
                        {
							datosGrid_reg elemento = new datosGrid_reg();
							elemento.Registro = 40001+k;
							elemento.Valor = Convert.ToString(bytes_respuesta[9 + J], 16);

							lista.Add(elemento);
							
							J = J + 2;
							k++;

						}
						dg_registros_internos.ItemsSource = lista;
						
					}
					
				}


			}
			return;
		}

		private void SetTimer()
		{
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromMilliseconds(10);
			timer.Tick += new EventHandler(resetAxis);
			timer.Start();
		}

		private void resetAxis(Object source, EventArgs e)
        {
			KRC6_AXI_0.Angle = axi0;
			KRC6_AXI_1.Angle = axi1;
			KRC6_AXI_2.Angle = axi2;
			KRC6_AXI_3.Angle = axi3;
			KRC6_AXI_4.Angle = axi4;
			KRC6_AXI_5.Angle = axi5;
			//KRC6_AXI_0.Angle = deb;

			AXI_0.Value = axi0;
			AXI_1.Value = axi1;
			AXI_2.Value = axi2;
			AXI_3.Value = axi3;
			AXI_4.Value = axi4;
			AXI_5.Value = axi5;

			 ur3_check_aux = (bool)ur3_select.IsChecked;
			 kuka_check_aux = (bool)kuca_select.IsChecked;
		}

		



		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			if (conectadoPrimeiraVez)
			{

				if (ur3_check_aux == true)
				{
					if (clie == null)
					{
						clie = new Cliente(IP_dir1.Text, 502);
					}
                    else { 
					peticionRegistro = new Peticion_datos();
					byte[] bytes_datos = new byte[12];
					byte[] bytes_respuesta = new byte[256];
					bytes_datos = (byte[])peticionRegistro.PETICION(nMensaje, 3, 271, 6);
					int res = clie.enviaDatos(bytes_datos, 12);


						if (res == 12)
						{
							res = clie.recebeDatos(bytes_respuesta, bytes_respuesta.Length);

							int nn = Convert.ToInt32(string.Concat(Convert.ToString(bytes_respuesta[9], 16), Convert.ToString(bytes_respuesta[10], 16)), 16) / 57;

							if (res == 9 + bytes_respuesta[8])
							{

								axi0 = (float)Convert.ToInt32(string.Concat(Convert.ToString(bytes_respuesta[9], 16), Convert.ToString(bytes_respuesta[10], 16)), 16) * (float)(57.2957795130823 / 1000);
								axi1 = (float)Convert.ToUInt32(string.Concat(Convert.ToString(bytes_respuesta[11], 16), Convert.ToString(bytes_respuesta[12], 16)), 16) * (float)(57.2957795130823 / 1000);
								axi2 = (float)Convert.ToInt32(string.Concat(Convert.ToString(bytes_respuesta[13], 16), Convert.ToString(bytes_respuesta[14], 16)), 16) * (float)(57.2957795130823 / 1000);
								axi3 = (float)Convert.ToInt32(string.Concat(Convert.ToString(bytes_respuesta[15], 16), Convert.ToString(bytes_respuesta[16], 16)), 16) * (float)(57.2957795130823 / 1000);
								axi4 = (float)Convert.ToInt32(string.Concat(Convert.ToString(bytes_respuesta[17], 16), Convert.ToString(bytes_respuesta[18], 16)), 16) * (float)(57.2957795130823 / 1000);
								axi5 = (float)Convert.ToInt32(string.Concat(Convert.ToString(bytes_respuesta[19], 16), Convert.ToString(bytes_respuesta[20], 16)), 16) * (float)(57.2957795130823 / 1000);
							}
						}
					}
				}
				if (kuka_check_aux == true)
				{

					if (clie == null)
					{
						clie = new Cliente(IP_dir1.Text, 7000);
					}
					if (clie != null)
					{

					
					byte[] kukadatos = new byte[21];
					byte[] kukadatos_res = new byte[180];
					kukadatos[0] = 0x00;
					kukadatos[1] = 0x00;
					kukadatos[2] = 0x00;
					kukadatos[3] = 0x11;
					kukadatos[4] = 0x00;
					kukadatos[5] = 0x00;
					kukadatos[6] = 0x0E;
					kukadatos[7] = 0x24;
					kukadatos[8] = 0x41;
					kukadatos[9] = 0x58;
					kukadatos[10] = 0x49;
					kukadatos[11] = 0x53;
					kukadatos[12] = 0x5f;
					kukadatos[13] = 0x41;
					kukadatos[14] = 0x43;
					kukadatos[15] = 0x54;
					kukadatos[16] = 0x5f;
					kukadatos[17] = 0x4d;
					kukadatos[18] = 0x45;
					kukadatos[19] = 0x41;
					kukadatos[20] = 0x53;
					/*
					kukadatos[0] = kukadatos[1] = kukadatos[2] = 0x00;
					kukadatos[3] = 0x07;
					kukadatos[4] = kukadatos[5] = 0x00;
					kukadatos[6] = 0x06;
					kukadatos[7] = 0x50;
					kukadatos[8] = 0x4f;
					kukadatos[9] = 0x53;
					kukadatos[10] = 0x49;*/


					int res = clie.enviaDatos(kukadatos, 21);

					char aux = '0';
					string aux2 = string.Empty;

					foreach (byte valor in kukadatos)
					{
						aux = (char)valor;
						aux2 += aux;
					}
					if (res == 21)
					{
						res = clie.recebeDatos(kukadatos_res, kukadatos_res.Length);
						if (res > 1)
						{


							aux = '0';
							char cha = '0';
							aux2 = string.Empty;
							foreach (byte valor in kukadatos_res)
							{

								aux = (char)valor;
								aux2 += aux;
							}
							string[] words = aux2.Split(':', ' ', ',');
							/*	for(int b = 0; b < words.Length; b++)
								{
									foreach(char nn in words[b])
									{
										if (nn == '.') cha = ',';
									}
								}*/

							NumberFormatInfo provider = new NumberFormatInfo();
							provider.NumberDecimalSeparator = ".";
							//KRC6_AXI_0.Angle = Convert.ToDouble(words[3], provider);
							deb = Convert.ToDouble(words[3], provider);

							axi0 = (float)Convert.ToDouble(words[3], provider);
							axi1 = 360.0f + (float)Convert.ToDouble(words[6], provider);
							axi2 = 360.0f + (float)Convert.ToDouble(words[9], provider);
							axi3 = (float)Convert.ToDouble(words[12], provider);
							axi4 = (float)Convert.ToDouble(words[15], provider);
							axi5 = (float)Convert.ToDouble(words[18], provider);
						}
					}
				}
			
				}

			}
			
		}

		private void peticion_registros_entrada_Click(object sender, RoutedEventArgs e)
        {
			if (clie != null)
			{
				ushort nRegistros = (ushort)Convert.ToUInt16(nRegistro_entrada.Text);
				int nBytesEnterosRegistro = nRegistros / 8;
				int nBytesIncompletosRegistro = nRegistros % 8 > 0 ? 1 : 0;
				int nBytesRegistro = nBytesEnterosRegistro + nBytesIncompletosRegistro;

				peticionRegistro = new Peticion_datos();
				byte[] bytes_datos = new byte[12];
				byte[] bytes_respuesta = new byte[256];

				bytes_datos = (byte[])peticionRegistro.PETICION(nMensaje, 4, Convert.ToInt32(primeiro_Registro_entrada.Text), Convert.ToInt16(nRegistro_entrada.Text));
				int res = clie.enviaDatos(bytes_datos, 12);


				if (res == 12)
				{
					res = clie.recebeDatos(bytes_respuesta, bytes_respuesta.Length);

					if (res == 9 + bytes_respuesta[8])
					{

						List<datosGrid_reg> lista = new List<datosGrid_reg>();
						int k = Convert.ToInt32(primeiro_Registro_entrada.Text) - 1;
						int J = 0;

						for (int i = 0; i < bytes_respuesta[8] / 2; i++)
						{

							datosGrid_reg elemento = new datosGrid_reg();
							elemento.Registro = 30001 + k;
							elemento.Valor = Convert.ToString(bytes_respuesta[9 + J], 16);

							lista.Add(elemento);

							J = J + 2;
							k++;

						}
						dg_registros_entrada.ItemsSource = lista;


					}

				}


			}
			return;
		}


        private void peticion_multiploReg_interno_Click(object sender, RoutedEventArgs e)
        {
			if (clie != null)
			{
				ushort nRegistros = (ushort)Convert.ToUInt16(nRegistro_multiploReg_interno.Text);
				int nBytesEnterosRegistro = nRegistros / 8;
				int nBytesIncompletosRegistro = nRegistros % 8 > 0 ? 1 : 0;
				int nBytesRegistro = nBytesEnterosRegistro + nBytesIncompletosRegistro;
				int h = 0;
				byte[] array_parcial_valor = new byte[2];
				byte[] Array_ValorSacado = new byte[nRegistros * 2];
				DatosGrid_Mod_Reg DG_ValorSacar = new DatosGrid_Mod_Reg();

                for (int v=0;v<nRegistros; v++)
                {
					DG_ValorSacar = (DatosGrid_Mod_Reg)dg_ValorMultiploReg_interno.Items[v];

                    if (DG_ValorSacar.Valor>=0)
                    {
					
							ushort ValorSacado = (ushort)Convert.ToUInt32(DG_ValorSacar.Valor);

							array_parcial_valor = BitConverter.GetBytes(ValorSacado);
							Array.Reverse(array_parcial_valor, 0, 2);
							Array.Copy(array_parcial_valor, 0, Array_ValorSacado, h, 2);
							h = h + 2;
					
                   
					}


				}


				peticionRegistro = new Peticion_datos();
				byte[] bytes_datos = new byte[13+Array_ValorSacado.Length];
				byte[] bytes_respuesta = new byte[256];
				byte[] toStringByte1 = new byte[2];
				byte[] toStringByte2 = new byte[2];

				bytes_datos = (byte[])peticionRegistro.PETICION_MULTIPLE(nMensaje, 16, Convert.ToInt32(primeiro_multiploReg_interno.Text), Convert.ToInt16(nRegistro_multiploReg_interno.Text), Array_ValorSacado);
				int res = clie.enviaDatos(bytes_datos, 13+nRegistros*2);


				if (res == 13 + nRegistros * 2)
				{
					res = clie.recebeDatos(bytes_respuesta, bytes_respuesta.Length);
						toStringByte1[0] = bytes_respuesta[8];
						toStringByte1[1] = bytes_respuesta[9];
						toStringByte2[0] = bytes_respuesta[10];
						toStringByte2[1] = bytes_respuesta[11];
					    pReg_Modificado.Text = (toStringByte1[1] + 40001).ToString(); //            Convert.ToString( Convert.ToInt32(Convert.ToString(toStringByte1[0],2))+40001);
						nReg_Modificados.Text = toStringByte2[1].ToString();  // Convert.ToString(Convert.ToInt32(Convert.ToString(toStringByte2[0],16)));


				}


			}
			return;

			
	}

        private void nRegistro_multiploReg_interno_TextChanged(object sender, TextChangedEventArgs e)
        {
			List<DatosGrid_Mod_Reg> lista = new List<DatosGrid_Mod_Reg>();
			Peticion_datos PEETICION_MultRegInt = new Peticion_datos();

			if (nRegistro_multiploReg_interno.Text != null & nRegistro_multiploReg_interno.Text != string.Empty)
			{
				int k = Convert.ToInt32(primeiro_multiploReg_interno.Text) - 1;

				
				int nReg_mod = Convert.ToInt32(nRegistro_multiploReg_interno.Text);

				int J = 0;

				for (int i = 0; i < nReg_mod; i++)
				{
					DatosGrid_Mod_Reg elemento = new DatosGrid_Mod_Reg();
					
					elemento.Registro = 40001 + i +k;
					elemento.Valor =0;
					lista.Add(elemento);
				}
				dg_ValorMultiploReg_interno.ItemsSource = lista;


			}
		}

        private void nRegistro_multiploReg_interno_Error(object sender, ValidationErrorEventArgs e)
        {

		}

		private void Iniciar_button_Click(object sender, RoutedEventArgs e)
		{

			int res = sim.inicializarCoppelia(cv_Contenetor);

			 res = sim.cargarCoppelia("C:/temp/Inicio1.ttt");
		}

		private void atualizar_button2_Click(object sender, RoutedEventArgs e)
		{
			int res = sim.cargarCoppelia("C:/temp/Inicio1.ttt");
		}

		private void Terminar_button_Click(object sender, RoutedEventArgs e)
        {
			int res;
			res = sim.terminarCoppelia();
			//sim = null;
			//return;
		}




 
        private void AXI_1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
			Rot_AXI_0.Angle= AXI_0.Value;
			Rot_AXI_1.Angle = AXI_1.Value;
			Rot_AXI_2.Angle = AXI_2.Value;
			Rot_AXI_3.Angle = AXI_3.Value;
			Rot_AXI_4.Angle = AXI_4.Value;
			Rot_AXI_5.Angle = AXI_5.Value;
			
			
			Axi_BoxValue_0.Text =Convert.ToString( AXI_0.Value);
			Axi_BoxValue_1.Text = Convert.ToString(AXI_1.Value);
			Axi_BoxValue_2.Text = Convert.ToString(AXI_2.Value);
			Axi_BoxValue_3.Text = Convert.ToString(AXI_3.Value);
			Axi_BoxValue_4.Text = Convert.ToString(AXI_4.Value);
			Axi_BoxValue_5.Text = Convert.ToString(AXI_5.Value);

			float[] ang = new float[6];
			ang[0] = (float)AXI_0.Value* (float)0.0174533;
			ang[1] = (float)AXI_1.Value* (float)0.0174533;
			ang[2] = (float)AXI_2.Value* (float)0.0174533;
			ang[3] = (float)AXI_3.Value * (float)0.0174533;
			ang[4] = (float)AXI_4.Value * (float)0.0174533; 
			ang[5] = (float)AXI_5.Value * (float)0.0174533;
			int res = sim.ponPosicionRobotUR3(1, ang);
		}

        private void desconectar1_Click(object sender, RoutedEventArgs e)
        {
			clie.cierraCliente();
			desconectar1.Visibility = Visibility.Hidden;
			conectar1.Visibility = Visibility.Visible;
			conectadoPrimeiraVez = false;
        }

        private void desconectar2_Click(object sender, RoutedEventArgs e)
        {
			clie2.cierraCliente();
			desconectar2.Visibility = Visibility.Hidden;
			conectar2.Visibility = Visibility.Visible;
			conectadoPrimeiraVez2 = false;
		}

        private void kuca_select_Checked(object sender, RoutedEventArgs e)
        {
			MainViewport3D2.Visibility = Visibility.Visible;
			MainViewport3D.Visibility = Visibility.Hidden;

		}

        private void ur3_select_Checked(object sender, RoutedEventArgs e)
        {
			MainViewport3D2.Visibility = Visibility.Hidden; 
			MainViewport3D.Visibility = Visibility.Visible;
		}

        private void PLC_select_Checked(object sender, RoutedEventArgs e)
        {
			MainViewport3D2.Visibility = Visibility.Hidden;
			MainViewport3D.Visibility = Visibility.Hidden;
		}

        private void IP_dir1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void x_plus_Click(object sender, RoutedEventArgs e)
        {
			Rot_RobtX.Angle = Rot_RobtX.Angle + 2;
        }

        private void x_sub_Click(object sender, RoutedEventArgs e)
        {
			Rot_RobtX.Angle = Rot_RobtX.Angle - 2;
		}

        private void y_plus_Click(object sender, RoutedEventArgs e)
        {
			Rot_RobtY.Angle = Rot_RobtY.Angle + 2;
		}

        private void y_sub_Click(object sender, RoutedEventArgs e)
        {
			Rot_RobtY.Angle = Rot_RobtY.Angle - 2;
		}

 
    }


    public class datosGrid
	{
		public int nElemento { get; set; }
		public bool Estado { get; set; }
	}
	public class datosGrid_reg
    {
		public int Registro { get; set; }
		public string Valor{ get; set; }
	//	public byte Estado { get; set;}
	}
	public class DatosGrid_Mod_Reg
    {
		public int Registro { get; set; }
		public int Valor { get; set; }
		
	}
}
