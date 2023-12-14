using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Touchless.Vision.Camera;
using System.Threading;



// UDP y Multicast.
using System.Net.Sockets;
using System.Net;
using System.IO;

// IMAGE
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.InteropServices;

namespace WebcamUDPMulticast
{
    public partial class Form1 : Form
    {
        private IPAddress multicastAddress = IPAddress.Parse("224.0.0.3");
        private float latenciaAnterior = 0;
        private int numeroPaquetesPerdidos = 0;
        private int numeroPaquetesTotalesPerdidos = 0;
        private float latenciaAcumulada = 0;
        private float jitterAcumulado = 0;
        private int paquetesR = 0;
        private int retardo = 0;
        byte[] imagen = null;

        

        public Form1()
        {
            InitializeComponent();            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Task t1 = new Task(visualizar_imagen);
            t1.Start();
          
        }

        private void button1_Click(object sender, EventArgs e)
        {
            

            
        }

        private void visualizar_imagen()
        {
            UdpClient udpClient = new UdpClient();
            udpClient.JoinMulticastGroup(multicastAddress);

            IPEndPoint remoteep = new IPEndPoint(IPAddress.Any, 8081);

            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); 
            udpClient.Client.Bind(remoteep);

            while (true)
            {
                try
                {

                    Byte[] paquete = udpClient.Receive(ref remoteep);

                    CabeceraRTP cabecera = new CabeceraRTP(paquete);
                    paquete = paquete.Skip(20).ToArray();

                    short numeroSecuenciaR = cabecera.NumeroSecuencia;
                    int   timeStampR       = cabecera.TimeStamp;
                    int   numeroImagenR    = cabecera.NumeroImagen;
                    int   longitudPayloadR = cabecera.LongitudPayload;
                    short longitudImagenR  = cabecera.LongitudImagen;
                    short numeroPaquetesEnLaImagenR = cabecera.NumeroPaquetesEnLaImagen;
;


                    //Primer paquete de la imagen.
                    if (numeroSecuenciaR == 0)
                    {
                        imagen = new byte[longitudImagenR];
                        Array.Copy(paquete, 0, imagen, 0, longitudPayloadR);
                        retardo = (DateTime.Now.Millisecond - timeStampR);
                        paquetesR++;
                    }
                    else
                    {
                        // Paquetes intermedios de la imagen.
                        if (numeroSecuenciaR < (numeroPaquetesEnLaImagenR - 1))
                        {
                            Array.Copy(paquete, 0, imagen, numeroSecuenciaR * 1400, longitudPayloadR);
                            retardo += (DateTime.Now.Millisecond - timeStampR);
                            paquetesR++;
                        }
                        // Último paquete de la imagen.
                        else if (numeroSecuenciaR == (numeroPaquetesEnLaImagenR - 1))
                        { 
                            Array.Copy(paquete, 0, imagen, numeroSecuenciaR * 1400, longitudPayloadR);
                            retardo += (DateTime.Now.Millisecond - timeStampR);
                            pictureBox1.Image = byteArrayToImage(imagen);
                            paquetesR++;

                            //Calculamos la latencia.
                            float latencia = retardo / numeroPaquetesEnLaImagenR;
                            latencia = Math.Abs(latencia);

                            retardo = 0;
                    
                            //Latencia acumulada.
                            latenciaAcumulada += latencia;

                            //Latencia media.
                            float latenciaMedia = latenciaAcumulada / numeroImagenR;
                            latenciaMedia = Math.Abs(latenciaMedia);

                            //Calculamos el Jitter.
                            float jitter = latencia - latenciaAnterior;
                            jitter = Math.Abs(jitter);
                            latenciaAnterior = latencia;

                            //Jitter acumulado.
                            jitterAcumulado += jitter;

                            //Jitter medio.
                            float jitterMedio = jitterAcumulado / numeroImagenR;
                            jitterMedio = Math.Abs(jitterMedio);

                            //Calculamos el número de paquetes perdidos.
                            if (paquetesR != numeroPaquetesEnLaImagenR)
                                numeroPaquetesPerdidos = numeroPaquetesEnLaImagenR - paquetesR;

                            numeroPaquetesTotalesPerdidos += numeroPaquetesPerdidos;
                            paquetesR = 0;

                            //Calculamos el porcentaje de paquetes perdidos.
                            //float porcentajePaquetesPerdidos = (numeroPaquetesPerdidos * 100) / numeroPaquetesEnLaImagenR;

                            this.Invoke((MethodInvoker)delegate
                            {
                                listBox1.Items.Add(latencia);
                                listBox2.Items.Add(jitter);
                                listBox3.Items.Add(numeroPaquetesTotalesPerdidos);
                                listBox4.Items.Add(latenciaMedia);
                                listBox5.Items.Add(jitterMedio);

                                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                                listBox2.SelectedIndex = listBox2.Items.Count - 1;
                                listBox3.SelectedIndex = listBox3.Items.Count - 1;
                                listBox4.SelectedIndex = listBox4.Items.Count - 1;
                                listBox5.SelectedIndex = listBox5.Items.Count - 1;

                        
                            });

                        }       
                    }

                    

                    


                    
                }
                catch (Exception x)
                {
                    x.ToString();
                }
            }


                
            }

        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            using (var ms = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(ms);
            }
        }

        
        private void button1_Click_1(object sender, EventArgs e)
        {
            UdpClient udpClientChat = new UdpClient();
            udpClientChat.JoinMulticastGroup(multicastAddress);
            IPEndPoint remoteepChat = new IPEndPoint(multicastAddress, 8082);

            byte[] mensaje = Encoding.Unicode.GetBytes(richTextBox1.Text);
            udpClientChat.Send(mensaje, mensaje.Length, remoteepChat);

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        
    }
}
