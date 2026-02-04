// ============================================================
// FICHIER : Services/TcpServerService.cs
// ROLE    : Gère le serveur TCP qui écoute les connexions des clients
//           sur le port 8888. Chaque client se connecte ici via le réseau local.
// ============================================================
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LearnLineServer.Data;
using LearnLineServer.Models;

namespace LearnLineServer.Services
{
    // Service qui gère le serveur TCP et les connexions des clients
    public class TcpServerService
    {
        // Port sur lequel le serveur écoute les connexions (8888 comme demandé)
        private const int PORT = 8888;

        // Le listener TCP principal
        private TcpListener _tcpListener;

        // Référence vers le service de base de données
        private readonly DatabaseService _databaseService;

        // Liste des connexions actives (pour pouvoir envoyer des notifications)
        private readonly Dictionary<string, TcpClient> _activeConnections;

        // Dictionnaire pour lier un userId à une connexion TCP
        private readonly Dictionary<string, string> _userConnections; // userId -> connectionId

        // Objet de verrouillage pour la liste des connexions
        private readonly object _connectionLock = new object();

        // Indique si le serveur est en cours d'exécution
        private bool _isRunning;

        // Constructeur - reçoit le service de base de données
        public TcpServerService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _activeConnections = new Dictionary<string, TcpClient>();
            _userConnections = new Dictionary<string, string>();
            _isRunning = false;
        }

        // Démarrer le serveur TCP
        public async Task StartAsync()
        {
            _isRunning = true;

            // Créer le listener sur l'adresse locale, port 8888
            _tcpListener = new TcpListener(IPAddress.Any, PORT);
            _tcpListener.Start();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SERVEUR TCP] Le serveur écoute sur le port {PORT}");
            Console.WriteLine($"[SERVEUR TCP] Adresse IP locale : {GetLocalIpAddress()}");
            Console.WriteLine($"[SERVEUR TCP] Les clients doivent se connecter à cette IP sur le port {PORT}");
            Console.ResetColor();

            // Boucle principale : attendre les connexions des clients
            while (_isRunning)
            {
                try
                {
                    // Attendre qu'un client se connecte
                    TcpClient clientSocket = await _tcpListener.AcceptTcpClientAsync();

                    // Générer un ID unique pour cette connexion
                    string connectionId = Guid.NewGuid().ToString();

                    // Stocker la connexion dans la liste
                    lock (_connectionLock)
                    {
                        _activeConnections[connectionId] = clientSocket;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[CONNEXION] Nouveau client connecté | ID: {connectionId} | IP: {clientSocket.Client.RemoteEndPoint}");
                    Console.ResetColor();

                    // Gérer cette connexion dans un thread séparé (pour ne pas bloquer)
                    var handlerThread = new Thread(() => { HandleClientAsync(clientSocket, connectionId).Wait(); });
                    handlerThread.IsBackground = true;
                    handlerThread.Start();
                }
                catch (Exception ex)
                {
                    if (_isRunning) // Ne pas afficher l'erreur si on a intentionnellement arrêté
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[ERREUR] Erreur lors de l'acceptation de connexion: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
        }

        // Gérer la communication avec un client connecté
        private async Task HandleClientAsync(TcpClient client, string connectionId)
        {
            NetworkStream stream = null;
            StreamReader reader = null;

            try
            {
                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);

                while (_isRunning && client.Connected)
                {
                    try
                    {
                        string requestJson = await reader.ReadLineAsync();

                        if (string.IsNullOrWhiteSpace(requestJson))
                            continue;

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"[REQUÊTE REÇUE] Connexion {connectionId}");
                        Console.ResetColor();

                        string responseJson = ProcessRequest(requestJson, connectionId);

                        if (responseJson != null)
                        {
                            byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson + "\n");
                            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                            await stream.FlushAsync();

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"[RÉPONSE ENVOYÉE] Connexion {connectionId}");
                            Console.ResetColor();
                        }
                    }
                    catch (IOException)
                    {
                        // Client déconnecté
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[ERREUR] Erreur de traitement pour {connectionId}: {ex.Message}");
                        Console.ResetColor();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERREUR] Erreur avec le client {connectionId}: {ex.Message}");
                    Console.ResetColor();
                }
            }
            finally
            {
                // Nettoyer : fermer la connexion et mettre à jour la base de données
                lock (_connectionLock)
                {
                    _activeConnections.Remove(connectionId);

                    // Supprimer la correspondance userId -> connectionId
                    string userIdToRemove = null;
                    foreach (var kvp in _userConnections)
                    {
                        if (kvp.Value == connectionId)
                        {
                            userIdToRemove = kvp.Key;
                            break;
                        }
                    }
                    if (userIdToRemove != null)
                    {
                        _userConnections.Remove(userIdToRemove);
                        _databaseService.CloseSession(userIdToRemove);
                    }
                }

                reader?.Dispose();
                stream?.Dispose();
                client?.Close();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[DÉCONNEXION] Client {connectionId} s'est déconnecté");
                Console.ResetColor();
            }
        }
        // Traiter une requête reçue du client et retourner la réponse
        // Traiter une requête reçue du client et retourner la réponse
        private string ProcessRequest(string requestJson, string connectionId)
        {
            try
            {
                // DEBUG: Afficher le JSON reçu
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[DEBUG JSON REÇU] {requestJson}");
                Console.ResetColor();

                // Désérialiser comme dictionnaire pour extraire RequestType et Data
                var request = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson);

                if (request == null)
                {
                    return SerializeResponse(new GenericResponse { Success = false, Message = "Requête invalide" });
                }

                // Extraire le RequestType
                if (!request.ContainsKey("RequestType") && !request.ContainsKey("Action"))
                {
                    return SerializeResponse(new GenericResponse { Success = false, Message = "RequestType ou Action manquant" });
                }

                string requestType = request.ContainsKey("RequestType")
                    ? request["RequestType"].ToString()
                    : request["Action"].ToString();

                // Extraire l'objet Data (s'il existe)
                string dataJson = requestJson; // Par défaut, utiliser le JSON complet
                if (request.ContainsKey("Data") && request["Data"] != null)
                {
                    // Sérialiser uniquement l'objet Data
                    dataJson = JsonConvert.SerializeObject(request["Data"]);
                }

                // Normaliser (enlever underscores et convertir en minuscules)
                string normalizedType = requestType.Replace("_", "").ToLower();

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[REQUÊTE] Type: {requestType} -> Normalisé: {normalizedType} | Connexion: {connectionId}");
                Console.ResetColor();

                // Selon le type de requête, traiter différemment
                switch (normalizedType)
                {
                    case "register":
                        var registerReq = JsonConvert.DeserializeObject<RegisterRequest>(dataJson);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"[DEBUG REGISTER] Name: '{registerReq?.Name}' | Email: '{registerReq?.Email}' | Password: '{registerReq?.Password}'");
                        Console.ResetColor();
                        return HandleRegister(registerReq);

                    case "login":
                        var loginReq = JsonConvert.DeserializeObject<LoginRequest>(dataJson);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"[DEBUG LOGIN] Email: '{loginReq?.Email}' | Password: '{loginReq?.Password}'");
                        Console.ResetColor();
                        return HandleLogin(loginReq, connectionId);

                    case "getcourses":
                        return HandleGetCourses();

                    case "getcoursedetail":
                        return HandleGetCourseDetail(JsonConvert.DeserializeObject<GetCourseDetailRequest>(dataJson));

                    case "updateprogress":
                        return HandleUpdateProgress(JsonConvert.DeserializeObject<UpdateProgressRequest>(dataJson));

                    case "submittest":
                        return HandleSubmitTest(JsonConvert.DeserializeObject<SubmitTestRequest>(dataJson));

                    case "sendmessage":
                        return HandleSendMessage(JsonConvert.DeserializeObject<SendMessageRequest>(dataJson));

                    case "getmessages":
                        return HandleGetMessages(JsonConvert.DeserializeObject<GetMessagesRequest>(dataJson));

                    case "getconnectedusers":
                        return HandleGetConnectedUsers();

                    case "getstatistics":
                        return HandleGetStatistics(JsonConvert.DeserializeObject<GetStatisticsRequest>(dataJson));

                    case "setcurrentcourse":
                        return HandleSetCurrentCourse(JsonConvert.DeserializeObject<SetCurrentCourseRequest>(dataJson));

                    case "logout":
                        return HandleLogout(JsonConvert.DeserializeObject<LogoutRequest>(dataJson), connectionId);

                    default:
                        return SerializeResponse(new GenericResponse { Success = false, Message = $"Type de requête inconnu: {requestType}" });
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERREUR] Traitement requête: {ex.Message}");
                Console.WriteLine($"[ERREUR] Stack trace: {ex.StackTrace}");
                Console.ResetColor();
                return SerializeResponse(new GenericResponse { Success = false, Message = $"Erreur serveur: {ex.Message}" });
            }
        }
        // =============================================
        // HANDLERS POUR CHAQUE TYPE DE REQUÊTE
        // =============================================

        // Gérer l'inscription
        // Gérer l'inscription
        private string HandleRegister(RegisterRequest request)
        {
            // DEBUG: Afficher ce qui est reçu
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[DEBUG REGISTER] Name: '{request?.Name}' | Email: '{request?.Email}' | Password: '{request?.Password}'");
            Console.ResetColor();

            // Vérifier que tous les champs sont remplis
            if (string.IsNullOrWhiteSpace(request?.Name) ||
                string.IsNullOrWhiteSpace(request?.Email) ||
                string.IsNullOrWhiteSpace(request?.Password))
            {
                return SerializeResponse(new RegisterResponse
                {
                    Success = false,
                    Message = "Tous les champs sont obligatoires (Nom, Email, Mot de passe)"
                });
            }

            // Vérifier le format de l'email (vérification simple)
            if (!request.Email.Contains("@") || !request.Email.Contains("."))
            {
                return SerializeResponse(new RegisterResponse
                {
                    Success = false,
                    Message = "Format d'email invalide"
                });
            }

            // Vérifier la longueur du mot de passe
            if (request.Password.Length < 4)
            {
                return SerializeResponse(new RegisterResponse
                {
                    Success = false,
                    Message = "Le mot de passe doit avoir au moins 4 caractères"
                });
            }

            // Essayer de créer l'utilisateur
            var user = _databaseService.AddUser(request.Name, request.Email, request.Password);

            if (user == null)
            {
                // L'email existe déjà
                return SerializeResponse(new RegisterResponse
                {
                    Success = false,
                    Message = "Cet email est déjà utilisé. Veuillez choisir un autre email."
                });
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[INSCRIPTION] Nouvel utilisateur inscrit: {user.Name} ({user.Email}) | Date: {DateTime.Now:dd/MM/yyyy HH:mm}");
            Console.ResetColor();

            return SerializeResponse(new RegisterResponse
            {
                Success = true,
                Message = "Inscription réussie ! Vous pouvez maintenant vous connecter.",
                UserId = user.Id
            });
        }

        // Gérer la connexion (Login)
        // Gérer la connexion (Login)
        private string HandleLogin(LoginRequest request, string connectionId)
        {
            // DEBUG: Afficher ce qui est reçu
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[DEBUG LOGIN] Email: '{request?.Email}' | Password: '{request?.Password}'");
            Console.ResetColor();

            if (string.IsNullOrWhiteSpace(request?.Email) || string.IsNullOrWhiteSpace(request?.Password))
            {
                return SerializeResponse(new LoginResponse
                {
                    Success = false,
                    Message = "Email et mot de passe sont obligatoires"
                });
            }

            // Authentifier l'utilisateur
            var user = _databaseService.AuthenticateUser(request.Email, request.Password);

            if (user == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[LOGIN ÉCHOUÉ] Tentative de connexion avec email: {request.Email} | Heure: {DateTime.Now:HH:mm:ss}");
                Console.ResetColor();

                return SerializeResponse(new LoginResponse
                {
                    Success = false,
                    Message = "Email ou mot de passe incorrect"
                });
            }

            // Créer la session
            _databaseService.CreateSession(user.Id, user.Name, user.Email, connectionId);

            // Stocker la correspondance userId -> connectionId
            lock (_connectionLock)
            {
                _userConnections[user.Id] = connectionId;
            }

            return SerializeResponse(new LoginResponse
            {
                Success = true,
                Message = "Connexion réussie !",
                UserId = user.Id,
                UserName = user.Name,
                UserEmail = user.Email
            });
        }

        // Gérer la requête de liste des cours
        // Gérer la requête de liste des cours
        private string HandleGetCourses()
        {
            var courses = _databaseService.GetAllCourses();

            var response = new GetCoursesResponse
            {
                Success = true,
                Message = "Cours récupérés avec succès",
                Courses = courses
            };

            string json = SerializeResponse(response);

            // DEBUG: Afficher la réponse complète
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[DEBUG GET_COURSES] Nombre de cours: {courses?.Count ?? 0}");
            Console.WriteLine($"[DEBUG GET_COURSES] Réponse JSON: {json.Substring(0, Math.Min(500, json.Length))}...");
            Console.ResetColor();

            return json;
        }
        // Gérer la requête de détail d'un cours
        private string HandleGetCourseDetail(GetCourseDetailRequest request)
        {
            var course = _databaseService.GetCourseById(request.CourseId);
            if (course == null)
            {
                return SerializeResponse(new GetCourseDetailResponse
                {
                    Success = false,
                    Message = "Cours introuvable"
                });
            }

            return SerializeResponse(new GetCourseDetailResponse
            {
                Success = true,
                Message = "Détails du cours récupérés",
                Course = course
            });
        }

        // Gérer la mise à jour de progression
        private string HandleUpdateProgress(UpdateProgressRequest request)
        {
            _databaseService.UpdateCourseProgress(
                request.UserId,
                request.CourseId,
                request.CourseName,
                request.ChapterId,
                request.ProgressPercent,
                request.TimeSpentSeconds
            );

            // Aussi mettre à jour le cours actuel de la session
            _databaseService.SetCurrentCourse(request.UserId, request.CourseName);

            return SerializeResponse(new UpdateProgressResponse
            {
                Success = true,
                Message = "Progression mise à jour"
            });
        }

        // Gérer la soumission d'un test
        private string HandleSubmitTest(SubmitTestRequest request)
        {
            // Récupérer le cours et le quiz pour vérifier les réponses
            var course = _databaseService.GetCourseById(request.CourseId);
            if (course == null)
            {
                return SerializeResponse(new SubmitTestResponse
                {
                    Success = false,
                    Message = "Cours introuvable"
                });
            }

            // Trouver le chapitre et le quiz
            Quiz quiz = null;
            foreach (var chapter in course.Chapters)
            {
                if (chapter.Id == request.ChapterId && chapter.Quiz != null && chapter.Quiz.Id == request.QuizId)
                {
                    quiz = chapter.Quiz;
                    break;
                }
            }

            if (quiz == null)
            {
                return SerializeResponse(new SubmitTestResponse
                {
                    Success = false,
                    Message = "Quiz introuvable"
                });
            }

            // Calculer le score et préparer le feedback
            int scoreObtained = 0;
            int totalScore = quiz.Questions.Count;
            var feedback = new List<TestAnswerFeedback>();

            foreach (var question in quiz.Questions)
            {
                // Trouver la réponse de l'utilisateur pour cette question
                int userAnswer = -1;
                foreach (var ans in request.Answers)
                {
                    if (ans.QuestionId == question.Id)
                    {
                        userAnswer = ans.SelectedAnswer;
                        break;
                    }
                }

                bool isCorrect = (userAnswer == question.CorrectAnswerIndex);
                if (isCorrect) scoreObtained++;

                feedback.Add(new TestAnswerFeedback
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    UserAnswer = userAnswer,
                    CorrectAnswer = question.CorrectAnswerIndex,
                    IsCorrect = isCorrect,
                    Explanation = question.Explanation
                });
            }
            // Marquer les réponses comme correctes ou non
            foreach (var ans in request.Answers)
            {
                foreach (var question in quiz.Questions)
                {
                    if (ans.QuestionId == question.Id)
                    {
                        ans.IsCorrect = (ans.SelectedAnswer == question.CorrectAnswerIndex);
                        break;
                    }
                }
            }

            // Sauvegarder le résultat dans la base de données
            _databaseService.SaveTestResult(
                request.UserId,
                request.CourseId,
                request.CourseName,
                request.ChapterId,
                request.QuizId,
                request.Answers,
                scoreObtained,
                totalScore
            );

            double percentage = totalScore > 0 ? (double)scoreObtained / totalScore * 100 : 0;
            bool passed = percentage >= quiz.PassingScore;

            return SerializeResponse(new SubmitTestResponse
            {
                Success = true,
                Message = passed ? "Félicitations ! Vous avez réussi le test !" : $"Vous n'avez pas réussi. Score minimum requis : {quiz.PassingScore}%",
                ScoreObtained = scoreObtained,
                TotalScore = totalScore,
                PercentageScore = Math.Round(percentage, 2),
                Passed = passed,
                Feedback = feedback
            });
        }

        // Gérer l'envoi d'un message
        private string HandleSendMessage(SendMessageRequest request)
        {
            // Sauvegarder le message
            var message = _databaseService.SaveMessage(
                request.SenderId,
                request.SenderName,
                request.RecipientId,
                request.RecipientName,
                request.Content
            );

            // Envoyer une notification au destinataire s'il est connecté
            SendNotificationToUser(request.RecipientId, new NewMessageNotification { Message = message });

            return SerializeResponse(new SendMessageResponse
            {
                Success = true,
                Message = "Message envoyé",
                SentMessage = message
            });
        }

        // Gérer la requête d'historique de messages
        private string HandleGetMessages(GetMessagesRequest request)
        {
            var messages = _databaseService.GetMessagesBetween(request.UserId, request.OtherUserId);
            return SerializeResponse(new GetMessagesResponse
            {
                Success = true,
                Message = "Messages récupérés",
                Messages = messages
            });
        }

        // Gérer la requête des utilisateurs connectés
        private string HandleGetConnectedUsers()
        {
            var activeSessions = _databaseService.GetActiveSessions();
            var users = new List<ConnectedUserInfo>();

            foreach (var session in activeSessions)
            {
                users.Add(new ConnectedUserInfo
                {
                    UserId = session.UserId,
                    UserName = session.UserName,
                    UserEmail = session.UserEmail,
                    CurrentCourse = session.CurrentCourse ?? "Aucun cours actif",
                    LoginTime = session.LoginTime
                });
            }

            return SerializeResponse(new GetConnectedUsersResponse
            {
                Success = true,
                Message = "Utilisateurs connectés",
                Users = users
            });
        }

        // Gérer la requête de statistiques
        private string HandleGetStatistics(GetStatisticsRequest request)
        {
            var user = _databaseService.GetUserById(request.UserId);
            if (user == null)
            {
                return SerializeResponse(new GetStatisticsResponse
                {
                    Success = false,
                    Message = "Utilisateur introuvable"
                });
            }

            // Calculer les statistiques
            int totalCoursesStarted = user.CoursesProgress.Count;
            int totalCoursesCompleted = 0;
            long totalTimeSpent = 0;
            int totalTestsTaken = user.TestResults.Count;
            int totalTestsPassed = 0;
            double totalScore = 0;

            foreach (var progress in user.CoursesProgress)
            {
                if (progress.IsCompleted) totalCoursesCompleted++;
                totalTimeSpent += progress.TotalTimeSpentSeconds;
            }

            foreach (var result in user.TestResults)
            {
                if (result.PercentageScore >= 60) totalTestsPassed++;
                totalScore += result.PercentageScore;
            }

            double averageScore = totalTestsTaken > 0 ? totalScore / totalTestsTaken : 0;

            return SerializeResponse(new GetStatisticsResponse
            {
                Success = true,
                Message = "Statistiques récupérées",
                CoursesProgress = user.CoursesProgress,
                TestResults = user.TestResults,
                TotalCoursesStarted = totalCoursesStarted,
                TotalCoursesCompleted = totalCoursesCompleted,
                TotalTestsPassed = totalTestsPassed,
                TotalTestsTaken = totalTestsTaken,
                AverageTestScore = Math.Round(averageScore, 2),
                TotalTimeSpentSeconds = totalTimeSpent
            });
        }

        // Gérer la mise à jour du cours actuel
        private string HandleSetCurrentCourse(SetCurrentCourseRequest request)
        {
            _databaseService.SetCurrentCourse(request.UserId, request.CourseName);
            return SerializeResponse(new GenericResponse
            {
                Success = true,
                Message = "Cours actuel mis à jour"
            });
        }

        // Gérer la déconnexion
        private string HandleLogout(LogoutRequest request, string connectionId)
        {
            _databaseService.CloseSession(request.UserId);

            lock (_connectionLock)
            {
                _userConnections.Remove(request.UserId);
            }

            return SerializeResponse(new GenericResponse
            {
                Success = true,
                Message = "Déconnexion réussie"
            });
        }
        // =============================================
        // MÉTHODES UTILITAIRES
        // =============================================

        // Envoyer une notification à un utilisateur spécifique
        private void SendNotificationToUser(string userId, object notification)
        {
            lock (_connectionLock)
            {
                if (_userConnections.ContainsKey(userId) &&
                    _activeConnections.ContainsKey(_userConnections[userId]))
                {
                    try
                    {
                        var client = _activeConnections[_userConnections[userId]];
                        string json = "NOTIFICATION:" + SerializeResponse(notification);
                        byte[] bytes = Encoding.UTF8.GetBytes(json);
                        var notifStream = client.GetStream();
                        notifStream.Write(bytes, 0, bytes.Length);
                        notifStream.Flush();
                    }
                    catch (Exception)
                    {
                        // Si l'envoi échoue, on ignore (le client est probablement déconnecté)
                    }
                }
            }
        }

        // Sérialiser un objet en JSON
        private string SerializeResponse(object response)
        {
            return JsonConvert.SerializeObject(response);
        }

        // Obtenir l'adresse IP locale de la machine
        private string GetLocalIpAddress()
        {
            try
            {
                string hostName = Dns.GetHostName();
                var hostEntry = Dns.GetHostEntry(hostName);
                foreach (var ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip.ToString();
                }
            }
            catch (Exception) { }
            return "127.0.0.1";
        }

        // Arrêter le serveur TCP
        public void Stop()
        {
            _isRunning = false;
            _tcpListener?.Stop();

            // Fermer toutes les connexions actives
            lock (_connectionLock)
            {
                foreach (var conn in _activeConnections.Values)
                {
                    conn?.Close();
                }
                _activeConnections.Clear();
                _userConnections.Clear();
            }
        }
    }
}