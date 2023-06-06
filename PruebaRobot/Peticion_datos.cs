using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Modbus_client
{
    public class Peticion_datos
    {

        public Array PETICION(ushort nMensaje, int cod_funcion, int primeiro_registro, int nRegistro )
        {

            ushort primeraSalida = (ushort)(primeiro_registro - 1);
            ushort nSalidas = Convert.ToUInt16(nRegistro);
   
            byte[] array_parcial = new byte[2];
            byte[] peticion = new byte[12];

            array_parcial=BitConverter.GetBytes(nMensaje);
            Array.Copy(array_parcial, 0, peticion, 0, 2);
   

            peticion[2] = peticion[3] = 0;

            array_parcial = BitConverter.GetBytes((ushort)6);
            Array.Reverse(array_parcial, 0, 2);
            Array.Copy(array_parcial, 0, peticion, 4, 2);

            peticion[6] =(byte)22;
            peticion[7] = (byte)cod_funcion;

            array_parcial = BitConverter.GetBytes(primeraSalida);
            Array.Reverse(array_parcial, 0, 2);
            Array.Copy(array_parcial, 0, peticion, 8, 2);

            array_parcial = BitConverter.GetBytes(nSalidas);
            Array.Reverse(array_parcial, 0, 2);
            Array.Copy(array_parcial, 0, peticion, 10, 2);

            return peticion;
        }
        public Array PETICION_MULTIPLE(ushort nMensaje, int cod_funcion, int primeiro_registro, int nRegistro,byte[] Reg)
        {

            ushort primeraSalida = (ushort)(primeiro_registro - 1);
            ushort nSalidas = Convert.ToUInt16(nRegistro);
            ushort nByte_datos = (ushort)(nRegistro * 2);

            byte[] array_parcial = new byte[2];
            byte[] peticion = new byte[13+ Reg.Length];

            array_parcial = BitConverter.GetBytes(nMensaje);
            Array.Copy(array_parcial, 0, peticion, 0, 2);


            peticion[2] = peticion[3] = 0;

            array_parcial = BitConverter.GetBytes((ushort)(7+Reg.Length));
            Array.Reverse(array_parcial, 0, 2);
            Array.Copy(array_parcial, 0, peticion, 4, 2);

            peticion[6] = (byte)22;
            peticion[7] = (byte)cod_funcion;

            array_parcial = BitConverter.GetBytes(primeraSalida);
            Array.Reverse(array_parcial, 0, 2);
            Array.Copy(array_parcial, 0, peticion, 8, 2);

            array_parcial = BitConverter.GetBytes(nSalidas);
            Array.Reverse(array_parcial, 0, 2);
            Array.Copy(array_parcial, 0, peticion, 10, 2);

            peticion[12]= (byte)nByte_datos;
      
            Array.Copy(Reg, 0, peticion, 13, Reg.Length);


            return peticion;
        }


    }
}
