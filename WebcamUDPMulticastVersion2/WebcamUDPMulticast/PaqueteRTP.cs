using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebcamUDPMulticast
{
    internal class PaqueteRTP
    {

        private byte[] _paquete;
        private byte[] _cabeceraRTP;

        public PaqueteRTP(byte[] payload, short numeroSecuencia, int timeStamp, int numeroImagen, short longitudImagen, short numeroPaquetesEnLaImagen)
        {
            _cabeceraRTP = new byte[20];
            _cabeceraRTP[0] = 0x80;
            _cabeceraRTP[1] = 0x00;
            Array.Copy(BitConverter.GetBytes(numeroSecuencia), 0, _cabeceraRTP, 2, 2);
            Array.Copy(BitConverter.GetBytes(timeStamp), 0, _cabeceraRTP, 4, 4);
            Array.Copy(BitConverter.GetBytes(numeroImagen), 0, _cabeceraRTP, 8, 4);
            Array.Copy(BitConverter.GetBytes(longitudImagen), 0, _cabeceraRTP, 12, 2);
            Array.Copy(BitConverter.GetBytes(numeroPaquetesEnLaImagen), 0, _cabeceraRTP, 14, 2);
            Array.Copy(BitConverter.GetBytes(payload.Length), 0, _cabeceraRTP, 16, 4);

            //Creamos el paquete RTP.
            _paquete = new byte[_cabeceraRTP.Length + payload.Length];
            Array.Copy(_cabeceraRTP, 0, _paquete, 0, _cabeceraRTP.Length);
            Array.Copy(payload, 0, _paquete, _cabeceraRTP.Length, payload.Length);

        }
        

        public byte[] Paquete
        {
            get { return _paquete; }
        }

        public byte[] CabeceraRTP
        {
            get { return _cabeceraRTP; }
        }
    }
}
