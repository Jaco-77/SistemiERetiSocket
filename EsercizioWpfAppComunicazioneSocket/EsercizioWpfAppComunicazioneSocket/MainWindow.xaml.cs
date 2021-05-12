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

// Aggiunte
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static System.Threading.Thread;

namespace EsercizioWpfAppComunicazioneSocket
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    
    // Bucci Jacopo 4 L Progetto 1

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Si imposta l'indirizzo ip della macchina attualmente in uso (quello usato è quello di default) e la porta che si vuole utilizzare
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 60000);

            // Si dichiara e inizializza il thread per il ricevere i messaggi
            Thread t1 = new Thread(new ParameterizedThreadStart(SocketReceive));

            // Sempre lo stesso thread si fa partire
            t1.Start(localEndPoint);
        }

        /// <summary>
        /// Metodo async per la ricezione dei mess
        /// </summary>
        /// <param name="sourceEndPoint"></param>
        public async void SocketReceive(object sourceEndPoint)
        {
            // Gli viene assegnato l'IP del destinatario
            IPEndPoint sourceEP = (IPEndPoint)sourceEndPoint;

            // Si dichiara e inizializza il socket che si dovrà utilizzare per la ricezione
            Socket t = new Socket(sourceEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            // 
            t.Bind(sourceEP);

            // Viene impostat la lunghezza massima del messaggio in arrivo
            byte[] byteRicevuti = new byte[256];

            // La stringa di default se il mess è vuoto
            string message = "";

            // Contatore per i bytes ricevuti
            int bytes = 0;

            // Per usare il metodo in parallelo con async
            await Task.Run(() =>
            {
                // Ciclo infinito per essere sempre pornti alla ricezione
                while (true)
                {
                    // Controlla se t è pronto
                    if (t.Available > 0)
                    {
                        // Si cancella il contenuto
                        message = "";

                        // Legge i bytes in ricezione
                        bytes = t.Receive(byteRicevuti, byteRicevuti.Length, 0);

                        // Converte in Ascii i bytes ricevuti
                        message += Encoding.ASCII.GetString(byteRicevuti, 0, bytes);

                        // Se possibile fa scrivere sulla list box il messaggio
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            // Aggiunge alla listbox il contenuto del messaggio
                            lblRicezione.Content = message;
                        }));
                    }
                }
            });
        }

        /// <summary>
        /// Metodo invia del messaggio
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            // Se ci sono errori avviso l'utente
            try
            {
                // Controllo se le textbox sono state riempite
                if (txtDestPort.Text.Length > 0 && txtDestPort.Text.Length > 0 && txtMsg.Text.Length > 0)
                {
                    // Converte l'IP del destinatario inserito in un IP
                    IPAddress ipDest = IPAddress.Parse(txtIPAdd.Text);

                    // Converte il numero della porta appena inserita
                    int portDest = int.Parse(txtDestPort.Text);

                    // Imposta un IPEndPoint per il destinatario
                    IPEndPoint remoteEndpoint = new IPEndPoint(ipDest, portDest);

                    // Si dichiara e inizializza il socket che si dovrà utilizzare per l'invio
                    Socket s = new Socket(ipDest.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

                    // Si convertino in byte il messaggio da inviare
                    byte[] byteInviati = Encoding.ASCII.GetBytes(txtMsg.Text);

                    // Invia il messaggio
                    s.SendTo(byteInviati, remoteEndpoint);

                    txtMsg.Clear();
                }
                else
                {
                    // Avviso l'utente
                    throw new Exception("Errore, inserisci qualcosa");
                }
            }
            // Avviso l'utente se la conversione è andata male
            catch (FormatException ex)
            {
                MessageBox.Show("ERRORE, riprova attenzione ai numeri inseriti\n" + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // Avviso l'utente se è andato male qualcosa
            catch (Exception ex)
            {
                MessageBox.Show("ERRORE, riprova\n" + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}