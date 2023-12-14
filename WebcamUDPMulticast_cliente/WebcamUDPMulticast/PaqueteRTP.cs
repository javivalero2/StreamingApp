using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebcamUDPMulticast
{
    internal class CabeceraRTP
    {
        private byte[] _cabeceraRTP;
        private short numeroSecuencia;
        private int timeStamp;
        private int numeroImagen;
        private int longitudPayload;
        private short longitudImagen;
        private short numeroPaquetesEnLaImagen;
        public CabeceraRTP(byte[] paquete)
        {
            //Creamos la cabecera RTP.
            _cabeceraRTP = new byte[20];
            Array.Copy(paquete, 0, _cabeceraRTP, 0, 18);

            //Obtenemos el número de secuencia.
            numeroSecuencia = BitConverter.ToInt16(_cabeceraRTP, 2);

            //Obtenemos el timestamp.
            timeStamp = BitConverter.ToInt32(_cabeceraRTP, 4);

            //Obtenemos el número de imagen.
            numeroImagen = BitConverter.ToInt32(_cabeceraRTP, 8);

            //Obtenemos la longitud de la imagen.
            longitudImagen = BitConverter.ToInt16(_cabeceraRTP, 12);

            //Obtenemos el número de paquetes en la imagen.
            numeroPaquetesEnLaImagen = BitConverter.ToInt16(_cabeceraRTP, 14);

            //Obtenemos la longitud del payload.
            longitudPayload = BitConverter.ToInt32(_cabeceraRTP, 16);

        }

        public byte[] Cabecera
        {
            get { return _cabeceraRTP; }
        }

        public short NumeroSecuencia
        {
            get { return numeroSecuencia; }
        }

        public int TimeStamp
        {
            get { return timeStamp; }
        }

        public int NumeroImagen
        {
            get { return numeroImagen; }
        }

        public int LongitudPayload
        {
            get { return longitudPayload; }
        }
        
        public short LongitudImagen
        {
            get { return longitudImagen; }
        }
        
        public short NumeroPaquetesEnLaImagen
        {
            get { return numeroPaquetesEnLaImagen; }
        }
    }
}
