
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LearnLineClient.Shared;
using Newtonsoft.Json;

namespace LearnLineClient.Services
{
    /// <summary>
    /// Service de communication TCP avec le serveur LearnLine
    /// Ce service gère toutes les communications entre le client et le serveur
    /// </summary>
    public class TcpClientService
    {
        // ═══════════════════════════════════════════════════════════════
        // CONFIGURATION DU SERVEUR
        // ═══════════════════════════════════════════════════════════════
        // ⚠️ IMPORTANT: Changez cette IP pour connecter plusieurs machines
        // Sur votre réseau local, remplacez par l'IP du serveur (ex: 192.168.1.10)
        public string ServerIpAddress { get; set; } = "127.0.0.1"; // Localhost par défaut
        public int ServerPort { get; set; } = 8888; // Port TCP du serveur

        private TcpClient? _client;
        private NetworkStream? _stream;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Établit la connexion avec le serveur TCP
        /// </summary>
        public async Task<bool> ConnectToServerAsync()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(ServerIpAddress, ServerPort);
                _stream = _client.GetStream();

                Console.WriteLine($"[CLIENT TCP] ✅ Connecté au serveur {ServerIpAddress}:{ServerPort}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT TCP] ❌ Erreur de connexion: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Envoie une requête au serveur et attend la réponse
        /// </summary>
        public async Task<TcpResponse> SendRequestAsync(TcpRequest request)
        {
            await _sendLock.WaitAsync();
            try
            {
                // Vérifier si la connexion est active
                if (_client == null || _stream == null || !_client.Connected)
                {
                    // Tenter de se reconnecter
                    bool connected = await ConnectToServerAsync();
                    if (!connected)
                    {
                        return new TcpResponse
                        {
                            Success = false,
                            Message = "Impossible de se connecter au serveur"
                        };
                    }
                }

                // Sérialiser la requête en JSON
                string jsonRequest = JsonConvert.SerializeObject(request);

                // IMPORTANT : Ajouter \n à la fin pour que le serveur puisse lire la ligne
                byte[] dataToSend = Encoding.UTF8.GetBytes(jsonRequest + "\n");

                Console.WriteLine($"[CLIENT TCP] 📤 Envoi: {request.RequestType} ({dataToSend.Length} bytes)");

                // Envoyer directement les données (SANS préfixe de longueur)
                if (_stream != null)
                {
                    await _stream.WriteAsync(dataToSend, 0, dataToSend.Length);
                    await _stream.FlushAsync();
                }

                // Recevoir la réponse (une ligne terminée par \n)
                if (_stream != null)
                {
                    var reader = new StreamReader(_stream, Encoding.UTF8);
                    string? jsonResponse = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(jsonResponse))
                    {
                        Console.WriteLine("[CLIENT TCP] ⚠️ Aucune réponse reçue");
                        return new TcpResponse
                        {
                            Success = false,
                            Message = "Aucune réponse reçue du serveur"
                        };
                    }

                    Console.WriteLine($"[CLIENT TCP] 📥 Réponse: {jsonResponse.Substring(0, Math.Min(100, jsonResponse.Length))}...");

                    TcpResponse? response = JsonConvert.DeserializeObject<TcpResponse>(jsonResponse);

                    if (response == null)
                    {
                        Console.WriteLine("[CLIENT TCP] ❌ Réponse invalide");
                        return new TcpResponse
                        {
                            Success = false,
                            Message = "Réponse invalide du serveur"
                        };
                    }

                    Console.WriteLine($"[CLIENT TCP] ✅ Succès: {response.Success} - {response.Message}");

                    return response;
                }

                return new TcpResponse
                {
                    Success = false,
                    Message = "Stream non disponible"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT TCP] ❌ Erreur: {ex.Message}");

                // Fermer et réinitialiser la connexion
                try
                {
                    _stream?.Close();
                    _client?.Close();
                }
                catch { }

                _stream = null;
                _client = null;

                return new TcpResponse
                {
                    Success = false,
                    Message = $"Erreur de communication: {ex.Message}"
                };
            }
            finally
            {
                _sendLock.Release();
            }
        }

        /// <summary>
        /// Ferme la connexion avec le serveur
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
                Console.WriteLine("[CLIENT TCP] 🔌 Déconnecté du serveur");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT TCP] ⚠️ Erreur lors de la déconnexion: {ex.Message}");
            }
        }
    }
}