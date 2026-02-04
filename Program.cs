// ============================================================
// FICHIER : Program.cs (SERVEUR)
// ROLE    : Point d'entrée du serveur LearnLine
//           - Demande le mot d'activation "LearnLine"
//           - Démarre le serveur TCP sur le port 8888
//           - Accepte les commandes "DB" et "exit" dans la console
// ============================================================
using System;
using System.Threading.Tasks;
using LearnLineServer.Data;
using LearnLineServer.Services;

namespace LearnLineServer
{
    class Program
    {
        // Mot d'activation requis pour démarrer le serveur
        private const string ACTIVATION_PASSWORD = "LearnLine";

        static async Task Main(string[] args)
        {
            // Afficher la bannière du serveur
            DisplayBanner();

            // =============================================
            // ÉTAPE 1 : Demander le mot d'activation
            // Le serveur ne démarre que si le mot est correct
            // Si incorrect, il demande à réessayer
            // =============================================
            RequestActivation();

            // =============================================
            // ÉTAPE 2 : Initialiser la base de données
            // =============================================
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[INITIALISATION] Base de données...");
            Console.ResetColor();

            // Créer le service de base de données (fichier JSON)
            var databaseService = new DatabaseService("LearnLine_Database.json");

            string dbFullPath = System.IO.Path.GetFullPath("LearnLine_Database.json");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[DATABASE] Base de données créée: {dbFullPath}");
            Console.WriteLine("[DATABASE] Toutes les tables ont été créées avec succès");
            Console.WriteLine("[DATABASE] Données d'exemple insérées (3 cours, 2 tests)");
            Console.ResetColor();

            // =============================================
            // ÉTAPE 3 : Démarrer le serveur TCP
            // =============================================
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[INITIALISATION] Serveur TCP...");
            Console.ResetColor();

            // Créer le service TCP
            var tcpServer = new TcpServerService(databaseService);

            // Démarrer le serveur TCP dans un thread séparé (pour ne pas bloquer la console)
            var serverTask = Task.Run(() => tcpServer.StartAsync());

            // Attendre un peu pour que le serveur soit bien démarré
            await Task.Delay(1000);

            // =============================================
            // ÉTAPE 4 : Afficher le tableau de statut du serveur
            // =============================================
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   SERVEUR LEARNLINE DÉMARRÉ                                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[SERVEUR] Port: 8888");
            Console.WriteLine("[SERVEUR] Protocole: TCP");
            Console.WriteLine("[SERVEUR] En attente de connexions...");
            Console.ResetColor();

            // =============================================
            // ÉTAPE 5 : Afficher le tableau des commandes disponibles
            // =============================================
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      COMMANDES DISPONIBLES                                   ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║  DB      - Afficher toute la base de données                                 ║");
            Console.WriteLine("║  CLIENTS - Afficher les clients connectés                                    ║");
            Console.WriteLine("║  STATS   - Afficher les statistiques du serveur                              ║");
            Console.WriteLine("║  HELP    - Afficher cette aide                                               ║");
            Console.WriteLine("║  CLEAR   - Effacer l'écran                                                   ║");
            Console.WriteLine("║  EXIT    - Arrêter le serveur                                                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // =============================================
            // ÉTAPE 6 : Boucle principale de la console du serveur
            // =============================================
            bool serverRunning = true;
            while (serverRunning)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("> ");
                Console.ResetColor();

                string command = Console.ReadLine();

                if (command == null) continue;

                command = command.Trim().ToUpper();

                switch (command)
                {
                    case "DB":
                        databaseService.DisplayDatabase();
                        break;

                    case "CLIENTS":
                        databaseService.DisplayConnectedClients();
                        break;

                    case "STATS":
                        databaseService.DisplayServerStats();
                        break;

                    case "HELP":
                        DisplayCommandsHelp();
                        break;

                    case "CLEAR":
                        Console.Clear();
                        break;

                    case "EXIT":
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[SERVEUR] Arrêt du serveur en cours...");
                        Console.ResetColor();

                        tcpServer.Stop();

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[SERVEUR] Le serveur a été fermé.");
                        Console.WriteLine("[SERVEUR] Tous les clients ont été déconnectés.");
                        Console.ResetColor();

                        serverRunning = false;
                        break;

                    default:
                        if (command.Length > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"  ✗ Commande inconnue: '{command}'");
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine("    Tapez 'HELP' pour voir les commandes disponibles.");
                            Console.ResetColor();
                        }
                        break;
                }
            }

            Console.WriteLine("\nAu revoir !");
        }

        // =============================================
        // MÉTHODE : Demander le mot d'activation
        // Boucle jusqu'à ce que le bon mot soit entré
        // =============================================
        private static void RequestActivation()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔══════════════════════════════════════════════════════╗");
            Console.WriteLine("║          ACTIVATION REQUISE                          ║");
            Console.WriteLine("║  Veuillez entrer le mot d'activation du serveur      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════╝");
            Console.ResetColor();

            bool activated = false;
            int attempts = 0;

            // Boucle : demander le mot jusqu'à ce qu'il soit correct
            while (!activated)
            {
                attempts++;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Mot d'activation : ");
                Console.ResetColor();

                // Lire le mot entré par l'utilisateur (masqué avec des *)
                string password = ReadPassword();

                if (password == ACTIVATION_PASSWORD)
                {
                    // Mot correct ! Le serveur peut démarrer
                    activated = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n✓ Activation réussie ! Le serveur va démarrer...\n");
                    Console.ResetColor();
                }
                else
                {
                    // Mot incorrect, demander à réessayer
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ Mot d'activation incorrect (tentative {attempts}). Réessayez.");
                    Console.ResetColor();
                }
            }
        }

        // Lire un mot de passe en masquant les caractères avec des *
        private static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true); // true = ne pas afficher le caractère

                // Si c'est une touche de caractère (pas Backspace, pas Enter)
                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape)
                {
                    if (key.Key == ConsoleKey.Backspace)
                    {
                        // Supprimer le dernier caractère
                        if (password.Length > 0)
                        {
                            password = password.Substring(0, password.Length - 1);
                            Console.Write("\b \b"); // Efface le dernier *
                        }
                    }
                    else
                    {
                        password += key.KeyChar;
                        Console.Write("*"); // Afficher un * à la place du caractère
                    }
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine(); // Passer à la ligne suivante après Enter
            return password;
        }

        // Afficher l'aide des commandes (même tableau que au démarrage)
        private static void DisplayCommandsHelp()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      COMMANDES DISPONIBLES                                   ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║  DB      - Afficher toute la base de données                                 ║");
            Console.WriteLine("║  CLIENTS - Afficher les clients connectés                                    ║");
            Console.WriteLine("║  STATS   - Afficher les statistiques du serveur                              ║");
            Console.WriteLine("║  HELP    - Afficher cette aide                                               ║");
            Console.WriteLine("║  CLEAR   - Effacer l'écran                                                   ║");
            Console.WriteLine("║  EXIT    - Arrêter le serveur                                                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        // Afficher la bannière du serveur au démarrage
        private static void DisplayBanner()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                              ║");
            Console.WriteLine("║        ██╗     ███████╗ █████╗ ██████╗ ███╗   ██╗            ║");
            Console.WriteLine("║        ██║     ██╔════╝██╔══██║██╔══██║████╗  ██║            ║");
            Console.WriteLine("║        ██║     █████╗  ███████║██║  ██║██╔██╗ ██║            ║");
            Console.WriteLine("║        ██║     ██╔══╝  ██╔══██║██║  ██║██║╚██╗██║            ║");
            Console.WriteLine("║        ███████╗███████╗██║  ██║██████╔╝██║ ╚████║            ║");
            Console.WriteLine("║        ╚══════╝╚══════╝╚═╝  ╚═╝╚═════╝ ╚═╝  ╚═══╝            ║");
            Console.WriteLine("║                                                              ║");
            Console.WriteLine("║              ██╗     ██╗███╗   ██╗███████╗                   ║");
            Console.WriteLine("║              ██║     ██║████╗  ██║██╔════╝                   ║");
            Console.WriteLine("║              ██║     ██║██╔██╗ ██║███████╗                   ║");
            Console.WriteLine("║              ██║     ██║██║╚██╗██═██║ ═══╝                   ║");
            Console.WriteLine("║              ███████╗██ ██nn ████ ███████╗                   ║");
            Console.WriteLine("║              ╚══════╝╚═══╝        ╚══════╝                   ║");
            Console.WriteLine("║                                                              ║");
            Console.WriteLine("║              SERVEUR E-LEARNING | Version 1.0                ║");
            Console.WriteLine("║              Port TCP : 8888                                 ║");
            Console.WriteLine("║                                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
