// ============================================================
// FICHIER : Data/DatabaseService.cs
// ROLE    : Gère le stockage et la récupération des données via un fichier JSON
//           Tout ce qui se passe est enregistré ici (utilisateurs, sessions,
//           cours suivis, scores, messages, etc.)
// ============================================================
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using LearnLineServer.Models;

namespace LearnLineServer.Data
{
    // Classe qui représente la structure complète de la base de données
    public class DatabaseSchema
    {
        // Liste de tous les utilisateurs inscrits
        public List<User> Users { get; set; }

        // Liste de toutes les sessions (actives et inactives)
        public List<UserSession> Sessions { get; set; }

        // Liste de tous les messages échangés
        public List<ChatMessage> Messages { get; set; }

        // Liste des cours disponibles sur la plateforme
        public List<Course> Courses { get; set; }

        // Date de la dernière mise à jour de la base de données
        public DateTime LastUpdatedAt { get; set; }

        // Constructeur par défaut - initialise toutes les listes
        public DatabaseSchema()
        {
            Users = new List<User>();
            Sessions = new List<UserSession>();
            Messages = new List<ChatMessage>();
            Courses = new List<Course>();
            LastUpdatedAt = DateTime.Now;
        }
    }

    // Service principal pour gérer la base de données
    public class DatabaseService
    {
        // Chemin du fichier JSON où sont stockées les données
        private readonly string _dbFilePath;

        // Instance actuelle de la base de données en mémoire
        private DatabaseSchema _database;

        // Objet de verrouillage pour éviter les conflits d'accès simultanés
        private readonly object _lockObj = new object();

        // Constructeur - initialise le chemin du fichier et charge les données
        public DatabaseService(string dbPath = "LearnLine_Database.json")
        {
            _dbFilePath = dbPath;
            LoadDatabase(); // Charger la base de données depuis le fichier
        }

        // =============================================
        // MÉTHODES DE CHARGEMENT ET SAUVEGARDE
        // =============================================

        // Charge la base de données depuis le fichier JSON
        private void LoadDatabase()
        {
            lock (_lockObj)
            {
                if (File.Exists(_dbFilePath))
                {
                    try
                    {
                        // Lire le fichier JSON et le désérialiser en objet
                        string json = File.ReadAllText(_dbFilePath);
                        _database = JsonConvert.DeserializeObject<DatabaseSchema>(json) ?? new DatabaseSchema();
                    }
                    catch (Exception)
                    {
                        // Si erreur de lecture, créer une nouvelle base de données
                        _database = new DatabaseSchema();
                    }
                }
                else
                {
                    // Si le fichier n'existe pas, créer une nouvelle base de données
                    _database = new DatabaseSchema();
                }

                // Si pas de cours, ajouter les cours par défaut
                if (_database.Courses == null || _database.Courses.Count == 0)
                {
                    _database.Courses = InitializeDefaultCourses();
                }
            }
        }

        // Sauvegarde la base de données dans le fichier JSON
        private void SaveDatabase()
        {
            lock (_lockObj)
            {
                _database.LastUpdatedAt = DateTime.Now;
                // Sérialiser en JSON avec indentation pour lisibilité
                string json = JsonConvert.SerializeObject(_database, Formatting.Indented);
                File.WriteAllText(_dbFilePath, json);
            }
        }

        // =============================================
        // GESTION DES UTILISATEURS
        // =============================================

        // Vérifier si un email existe déjà dans la base de données
        public bool EmailExists(string email)
        {
            return _database.Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        // Ajouter un nouvel utilisateur
        public User AddUser(string name, string email, string password)
        {
            // Vérifier si l'email existe déjà
            if (EmailExists(email))
                return null; // L'email existe déjà

            // Créer un nouvel identifiant unique
            string userId = Guid.NewGuid().ToString();

            // Créer l'utilisateur avec les données fournies
            var user = new User(userId, name, email, password);

            // Ajouter à la liste et sauvegarder
            _database.Users.Add(user);
            SaveDatabase();

            return user;
        }

        // Trouver un utilisateur par email et mot de passe (pour le login)
        public User AuthenticateUser(string email, string password)
        {
            return _database.Users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);
        }

        // Trouver un utilisateur par son ID
        public User GetUserById(string userId)
        {
            return _database.Users.FirstOrDefault(u => u.Id == userId);
        }

        // Obtenir la liste de tous les utilisateurs
        public List<User> GetAllUsers()
        {
            return _database.Users;
        }

        // =============================================
        // GESTION DES SESSIONS
        // =============================================

        // Créer une nouvelle session quand un utilisateur se connecte
        public UserSession CreateSession(string userId, string userName, string userEmail, string connectionId)
        {
            string sessionId = Guid.NewGuid().ToString();
            var session = new UserSession(sessionId, userId, userName, userEmail, connectionId);

            _database.Sessions.Add(session);
            SaveDatabase();

            // Afficher dans la console du serveur qui s'est connecté
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SESSION] Utilisateur connecté : {userName} ({userEmail}) | Heure : {DateTime.Now:HH:mm:ss} | Date : {DateTime.Now:dd/MM/yyyy}");
            Console.ResetColor();

            return session;
        }

        // Fermer une session quand un utilisateur se déconnecte
        public void CloseSession(string userId)
        {
            var session = _database.Sessions.LastOrDefault(s => s.UserId == userId && s.IsActive);
            if (session != null)
            {
                session.LogoutTime = DateTime.Now;
                session.IsActive = false;
                SaveDatabase();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[SESSION] Utilisateur déconnecté : {session.UserName} | Heure : {DateTime.Now:HH:mm:ss}");
                Console.ResetColor();
            }
        }

        // Fermer une session par ID de connexion TCP
        public void CloseSessionByConnectionId(string connectionId)
        {
            var session = _database.Sessions.LastOrDefault(s => s.ConnectionId == connectionId && s.IsActive);
            if (session != null)
            {
                session.LogoutTime = DateTime.Now;
                session.IsActive = false;
                SaveDatabase();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[SESSION] Utilisateur déconnecté (timeout) : {session.UserName} | Heure : {DateTime.Now:HH:mm:ss}");
                Console.ResetColor();
            }
        }

        // Mettre à jour le cours actuel d'un utilisateur
        public void SetCurrentCourse(string userId, string courseName)
        {
            var session = _database.Sessions.LastOrDefault(s => s.UserId == userId && s.IsActive);
            if (session != null)
            {
                session.CurrentCourse = courseName;
                SaveDatabase();

                // Enregistrer dans la console que l'utilisateur a commencé un cours
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[ACTIVITÉ] {session.UserName} suit maintenant le cours : {courseName} | Heure : {DateTime.Now:HH:mm:ss}");
                Console.ResetColor();
            }
        }

        // Obtenir toutes les sessions actives (utilisateurs connectés)
        public List<UserSession> GetActiveSessions()
        {
            return _database.Sessions.Where(s => s.IsActive).ToList();
        }

        // Obtenir toutes les sessions (historique complet)
        public List<UserSession> GetAllSessions()
        {
            return _database.Sessions;
        }

        // =============================================
        // GESTION DES COURS
        // =============================================

        // Obtenir la liste de tous les cours disponibles
        public List<Course> GetAllCourses()
        {
            return _database.Courses;
        }

        // Obtenir un cours par son ID
        public Course GetCourseById(string courseId)
        {
            return _database.Courses.FirstOrDefault(c => c.Id == courseId);
        }

        // =============================================
        // GESTION DE LA PROGRESSION
        // =============================================

        // Mettre à jour la progression d'un utilisateur dans un cours
        public void UpdateCourseProgress(string userId, string courseId, string courseName, string chapterId, double progressPercent, long timeSpentSeconds)
        {
            var user = GetUserById(userId);
            if (user == null) return;

            // Chercher si une progression existe déjà pour ce cours
            var progress = user.CoursesProgress.FirstOrDefault(p => p.CourseId == courseId);

            if (progress == null)
            {
                // Créer une nouvelle progression
                progress = new CourseProgress(courseId, courseName);
                user.CoursesProgress.Add(progress);
            }

            // Mettre à jour les données de progression
            progress.CurrentChapterId = chapterId;
            progress.ProgressPercent = progressPercent;
            progress.TotalTimeSpentSeconds += timeSpentSeconds;
            progress.LastActivityAt = DateTime.Now;

            // Si la progression est à 100%, marquer le cours comme terminé
            if (progressPercent >= 100)
            {
                progress.IsCompleted = true;
            }

            SaveDatabase();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[PROGRESSION] {user.Name} - Cours: {courseName} | Progression: {progressPercent}% | Chapitre: {chapterId}");
            Console.ResetColor();
        }

        // =============================================
        // GESTION DES TESTS
        // =============================================

        // Enregistrer le résultat d'un test
        public TestResult SaveTestResult(string userId, string courseId, string courseName, string chapterId, string quizId, List<UserAnswer> answers, int scoreObtained, int totalScore)
        {
            var user = GetUserById(userId);
            if (user == null) return null;

            // Calculer le pourcentage
            double percentage = totalScore > 0 ? (double)scoreObtained / totalScore * 100 : 0;

            var testResult = new TestResult
            {
                Id = Guid.NewGuid().ToString(),
                CourseId = courseId,
                CourseName = courseName,
                ChapterId = chapterId,
                ScoreObtained = scoreObtained,
                TotalScore = totalScore,
                PercentageScore = Math.Round(percentage, 2),
                TakenAt = DateTime.Now,
                Answers = answers
            };

            user.TestResults.Add(testResult);
            SaveDatabase();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[TEST] {user.Name} - Cours: {courseName} | Score: {scoreObtained}/{totalScore} ({percentage:F1}%) | Date: {DateTime.Now:dd/MM/yyyy HH:mm}");
            Console.ResetColor();

            return testResult;
        }

        // =============================================
        // GESTION DES MESSAGES (CHAT)
        // =============================================

        // Sauvegarder un nouveau message
        public ChatMessage SaveMessage(string senderId, string senderName, string recipientId, string recipientName, string content)
        {
            var message = new ChatMessage(
                Guid.NewGuid().ToString(),
                senderId,
                senderName,
                recipientId,
                recipientName,
                content
            );

            _database.Messages.Add(message);
            SaveDatabase();

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"[CHAT] {senderName} → {recipientName}: {content} | Heure: {DateTime.Now:HH:mm:ss}");
            Console.ResetColor();

            return message;
        }

        // Obtenir l'historique des messages entre deux utilisateurs
        public List<ChatMessage> GetMessagesBetween(string userId1, string userId2)
        {
            return _database.Messages
                .Where(m =>
                    (m.SenderId == userId1 && m.RecipientId == userId2) ||
                    (m.SenderId == userId2 && m.RecipientId == userId1))
                .OrderBy(m => m.SentAt)
                .ToList();
        }

        // Obtenir tous les messages
        public List<ChatMessage> GetAllMessages()
        {
            return _database.Messages;
        }

        // =============================================
        // AFFICHAGE DE LA BASE DE DONNÉES (commande "DB" dans le serveur)
        // =============================================

        // Afficher le contenu complet de la base de données dans la console du serveur
        public void DisplayDatabase()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("\n========================================================");
            Console.WriteLine("         BASE DE DONNÉES LearnLine - SNAPSHOT COMPLET      ");
            Console.WriteLine($"         Date: {DateTime.Now:dd/MM/yyyy} | Heure: {DateTime.Now:HH:mm:ss}          ");
            Console.WriteLine("========================================================\n");
            Console.ResetColor();

            // ---- UTILISATEURS ----
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("━━━ UTILISATEURS INSCRITS ━━━");
            Console.ResetColor();

            if (_database.Users.Count == 0)
            {
                Console.WriteLine("  (Aucun utilisateur inscrit)");
            }
            else
            {
                foreach (var user in _database.Users)
                {
                    Console.WriteLine($"  ID: {user.Id}");
                    Console.WriteLine($"  Nom: {user.Name}");
                    Console.WriteLine($"  Email: {user.Email}");
                    Console.WriteLine($"  Inscrit le: {user.RegisteredAt:dd/MM/yyyy HH:mm}");
                    Console.WriteLine($"  Cours suivis: {user.CoursesProgress.Count}");
                    Console.WriteLine($"  Tests passés: {user.TestResults.Count}");
                    Console.WriteLine("  ---");
                }
            }

            // ---- SESSIONS ----
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n━━━ SESSIONS (HISTORIQUE COMPLET) ━━━");
            Console.ResetColor();

            if (_database.Sessions.Count == 0)
            {
                Console.WriteLine("  (Aucune session enregistrée)");
            }
            else
            {
                foreach (var session in _database.Sessions)
                {
                    string status = session.IsActive ? "ACTIVE" : "FERMÉE";
                    Console.ForegroundColor = session.IsActive ? ConsoleColor.Green : ConsoleColor.Gray;
                    Console.WriteLine($"  [{status}] {session.UserName} ({session.UserEmail})");
                    Console.WriteLine($"    Login: {session.LoginTime:dd/MM/yyyy HH:mm:ss}");
                    if (session.LogoutTime.HasValue)
                        Console.WriteLine($"    Logout: {session.LogoutTime.Value:dd/MM/yyyy HH:mm:ss}");
                    if (!string.IsNullOrEmpty(session.CurrentCourse))
                        Console.WriteLine($"    Cours actuel: {session.CurrentCourse}");
                    Console.ResetColor();
                    Console.WriteLine("    ---");
                }
            }

            // ---- PROGRESSION DES COURS ----
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n━━━ PROGRESSION DES COURS ━━━");
            Console.ResetColor();

            bool hasProgress = false;
            foreach (var user in _database.Users)
            {
                if (user.CoursesProgress.Count > 0)
                {
                    hasProgress = true;
                    Console.WriteLine($"  Utilisateur: {user.Name}");
                    foreach (var progress in user.CoursesProgress)
                    {
                        Console.WriteLine($"    Cours: {progress.CourseName} | Progression: {progress.ProgressPercent}% | Temps: {progress.TotalTimeSpentSeconds / 60}min | Terminé: {(progress.IsCompleted ? "OUI" : "NON")}");
                    }
                    Console.WriteLine("    ---");
                }
            }
            if (!hasProgress) Console.WriteLine("  (Aucune progression enregistrée)");

            // ---- RÉSULTATS DES TESTS ----
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n━━━ RÉSULTATS DES TESTS ━━━");
            Console.ResetColor();

            bool hasTests = false;
            foreach (var user in _database.Users)
            {
                if (user.TestResults.Count > 0)
                {
                    hasTests = true;
                    Console.WriteLine($"  Utilisateur: {user.Name}");
                    foreach (var result in user.TestResults)
                    {
                        string passedStr = result.PercentageScore >= 60 ? "RÉUSSI ✓" : "ÉCHOUÉ ✗";
                        Console.WriteLine($"    Cours: {result.CourseName} | Score: {result.ScoreObtained}/{result.TotalScore} ({result.PercentageScore}%) | {passedStr} | Date: {result.TakenAt:dd/MM/yyyy HH:mm}");
                    }
                    Console.WriteLine("    ---");
                }
            }
            if (!hasTests) Console.WriteLine("  (Aucun résultat de test enregistré)");

            // ---- MESSAGES ----
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n━━━ MESSAGES (CHAT) ━━━");
            Console.ResetColor();

            if (_database.Messages.Count == 0)
            {
                Console.WriteLine("  (Aucun message échangé)");
            }
            else
            {
                foreach (var msg in _database.Messages.OrderBy(m => m.SentAt))
                {
                    Console.WriteLine($"  [{msg.SentAt:HH:mm:ss}] {msg.SenderName} → {msg.RecipientName}: {msg.Content}");
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("\n========================================================");
            Console.WriteLine("                  FIN DU SNAPSHOT                         ");
            Console.WriteLine("========================================================\n");
            Console.ResetColor();
        }

        // Afficher les clients actuellement connectés (commande "CLIENTS")
        public void DisplayConnectedClients()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      CLIENTS CONNECTÉS                                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            var activeSessions = GetActiveSessions();

            if (activeSessions.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("  (Aucun client connecté en ce moment)");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  Total connectés : {activeSessions.Count}");
                Console.ResetColor();
                Console.WriteLine();

                int index = 1;
                foreach (var session in activeSessions)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  [{index}] {session.UserName}");
                    Console.ResetColor();
                    Console.WriteLine($"      Email        : {session.UserEmail}");
                    Console.WriteLine($"      Connecté le  : {session.LoginTime:dd/MM/yyyy HH:mm:ss}");
                    Console.WriteLine($"      Cours actuel : {(string.IsNullOrEmpty(session.CurrentCourse) ? "Aucun" : session.CurrentCourse)}");
                    Console.WriteLine();
                    index++;
                }
            }

            Console.WriteLine();
        }

        // Afficher les statistiques générales du serveur (commande "STATS")
        public void DisplayServerStats()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      STATISTIQUES DU SERVEUR                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            int totalUtilisateurs = _database.Users.Count;
            int totalSessions = _database.Sessions.Count;
            int sessionsActives = _database.Sessions.Count(s => s.IsActive);
            int totalMessages = _database.Messages.Count;
            int totalCours = _database.Courses.Count;

            // Calculer les tests totaux et la moyenne des scores
            int totalTests = 0;
            double totalScore = 0;
            int totalTestsReussis = 0;
            foreach (var user in _database.Users)
            {
                foreach (var result in user.TestResults)
                {
                    totalTests++;
                    totalScore += result.PercentageScore;
                    if (result.PercentageScore >= 60) totalTestsReussis++;
                }
            }
            double moyenneScore = totalTests > 0 ? totalScore / totalTests : 0;

            // Utilisateur le plus actif (le plus de tests)
            string userLePlusActif = "Aucun";
            int maxTests = 0;
            foreach (var user in _database.Users)
            {
                if (user.TestResults.Count > maxTests)
                {
                    maxTests = user.TestResults.Count;
                    userLePlusActif = user.Name;
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ── Données générales ──");
            Console.ResetColor();
            Console.WriteLine($"    Utilisateurs inscrits   : {totalUtilisateurs}");
            Console.WriteLine($"    Sessions totales        : {totalSessions}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"    Sessions actives        : {sessionsActives}");
            Console.ResetColor();
            Console.WriteLine($"    Cours disponibles       : {totalCours}");
            Console.WriteLine($"    Messages échangés       : {totalMessages}");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ── Données des tests ──");
            Console.ResetColor();
            Console.WriteLine($"    Tests effectués         : {totalTests}");
            Console.WriteLine($"    Tests réussis           : {totalTestsReussis}");
            Console.WriteLine($"    Tests échoués           : {totalTests - totalTestsReussis}");
            Console.WriteLine($"    Moyenne des scores      : {moyenneScore:F1}%");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ── Utilisateur le plus actif ──");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"    {userLePlusActif} ({maxTests} test(s))");
            Console.ResetColor();
            Console.WriteLine();
        }

        // =============================================
        // INITIALISATION DES COURS PAR DÉFAUT
        // =============================================

        // *** VOUS POUVEZ MODIFIER CES COURS ICI ***
        // *** POUR AJOUTER VOS PROPRES VIDÉOS, CHANGEZ Les VideoUrl ***
        // *** POUR MODIFIER LES TESTS, CHANGEZ Les Questions et Options ***
        private List<Course> InitializeDefaultCourses()
        {
            return new List<Course>
            {
                new Course
                {
                    Id = "course_001",
                    Title = "Introduction à la Programmation C#",
                    Description = "Apprenez les fondamentaux de la programmation en C# avec une bonne introduction.",
                    Category = "Programmation",
                    // *** CHANGEZ CETTE IMAGE pour votre propre miniature de cours ***
                    ThumbnailUrl = "/images/1.png",
                    Level = "Débutant",
                    EstimatedDurationMinutes = 120,
                    Chapters = new List<Chapter>
                    {
                        new Chapter
                        {
                            Id = "ch_001_1",
                            Title = "Chapitre 1 : Introduction à la Programmation C#",
                            Description = "Découvrez les bases , que offre le Language C#.",
                            Order = 1,
                            // *** CHANGEZ CETTE URL pour votre propre vidéo YouTube ou vidéo hébergée ***
                            // Exemple: "https://www.youtube.com/embed/VOTRE_ID_VIDEO"
                            VideoUrl = "https://youtu.be/uHUkndqnHAg?si=KuxQVz9BBVCA_lSC",
                            VideoDurationSeconds = 600, // 10 minutes
                            Quiz = new Quiz
                            {
                                Id = "quiz_001_1",
                                Title = "Quiz :Introduction",
                                PassingScore = 60,
                                Questions = new List<Question>
                                {
                                    new Question
                                    {
                                        Id = "q1",
                                        QuestionText = "Cette vidéo explique comment installer les outils nécessaires pour commencer à programmer en C#.",
                                        Options = new List<string> { "string", "int", "float", "bool" },
                                        CorrectAnswerIndex = 1, // "int" est la bonne réponse
                                        Explanation = "introduction à la programmation en C# et installation de l’outil .NET."
                                    },
                                    new Question
                                    {
                                        Id = "q2",
                                        QuestionText = "Cette vidéo enseigne comment faire une recette de cuisine pas à pas.",
                                        Options = new List<string> { "string nom = \"Jean\";", "String nom = Jean;", "str nom = \"Jean\";", "int nom = \"Jean\";" },
                                        CorrectAnswerIndex = 0,
                                        Explanation = "La syntaxe correcte est : string nom = \"Jean\"; avec string en minuscule et la valeur entre guillemets."
                                    },
                                    new Question
                                    {
                                        Id = "q3",
                                        QuestionText = "Cette vidéo décrit le déroulement d’un match de football en direct.",
                                        Options = new List<string> { "int", "string", "bool", "double" },
                                        CorrectAnswerIndex = 2, // "bool"
                                        Explanation = "Le type 'bool' (booléen) stocke uniquement les valeurs true ou false."
                                    }
                                }
                            }
                        },
                        new Chapter
                        {
                            Id = "ch_001_2",
                            Title = "Chapitre 2 : Afficher du text",
                            Description = "Maîtrisez les aficgages des text en C#.",
                            Order = 2,
                            // *** CHANGEZ CETTE URL pour votre propre vidéo ***
                            VideoUrl = "https://youtu.be/T1ghHTJtdGQ?si=kde8Xy6QMjV8bWlg",
                            VideoDurationSeconds = 720,
                            Quiz = new Quiz
                            {
                                Id = "quiz_001_2",
                                Title = "Quiz :  Afficher du text",
                                PassingScore = 60,
                                Questions = new List<Question>
                                {
                                    new Question
                                    {
                                        Id = "q4",
                                        QuestionText = "Cette vidéo explique comment se connecter à Internet sans Wi-Fi.",
                                        Options = new List<string> { "for (int i = 0; i < 10; i++)", "for i = 0 to 10", "loop i from 0 to 10", "for (i = 0; i < 10;)" },
                                        CorrectAnswerIndex = 0,
                                        Explanation = "La syntaxe correcte d'une boucle for est : for (int i = 0; i < 10; i++)"
                                    },
                                    new Question
                                    {
                                        Id = "q5",
                                        QuestionText = "Cette vidéo montre comment afficher du texte à l’écran en C#.",
                                        Options = new List<string> { "stop", "break", "exit", "return" },
                                        CorrectAnswerIndex = 1, // "break"
                                        Explanation = "L'instruction 'break' permet de sortir immédiatement d'une boucle."
                                    },
                                    new Question
                                    {
                                        Id = "q6",
                                        QuestionText = "Cette vidéo parle de faire un dessin animé sur Photoshop.",
                                        Options = new List<string> { "elif", "else if", "elseif", "otherwise" },
                                        CorrectAnswerIndex = 1,
                                        Explanation = "En C#, on utilise 'else if' pour une condition alternative (pas elif comme en Python)."
                                    }
                                }
                            }
                        }
                    }
                },
                new Course
                {
                    Id = "course_002",
                    Title = "Type de donneesS",
                    Description = "Apprenez la structure d'un type de donnees",
                    Category = "Développement C#",
                    // *** CHANGEZ CETTE IMAGE ***
                    ThumbnailUrl = "/images/3.png",
                    Level = "Débutant",
                    EstimatedDurationMinutes = 180,
                    Chapters = new List<Chapter>
                    {
                        new Chapter
                        {
                            Id = "ch_002_1",
                            Title = "Chapitre 3 : Type de donnees",
                            Description = "Apprenez la structure d'un type de donnees.",
                            Order = 1,
                            // *** CHANGEZ CETTE URL pour votre propre vidéo ***
                            VideoUrl = "https://youtu.be/T4D2a2gbxYc?si=ocTToBOFsGbV5t_s",
                            VideoDurationSeconds = 900,
                            Quiz = new Quiz
                            {
                                Id = "quiz_002_1",
                                Title = "Quiz : Type de donnees",
                                PassingScore = 60,
                                Questions = new List<Question>
                                {
                                    new Question
                                    {
                                        Id = "q7",
                                        QuestionText = "Une variable en C# peut changer de valeur pendant l’exécution du programme.",
                                        Options = new List<string> { "<p>", "<h1>", "<title>", "<header>" },
                                        CorrectAnswerIndex = 1,
                                        Explanation = "La balise <h1> est utilisée pour le titre principal d'une page HTML."
                                    },
                                    new Question
                                    {
                                        Id = "q8",
                                        QuestionText = "En C#, un int peut stocker des nombres entiers",
                                        Options = new List<string> { "<link>", "<url>", "<a>", "<href>" },
                                        CorrectAnswerIndex = 2,
                                        Explanation = "La balise <a> avec l'attribut href crée un lien hypertexte."
                                    },
                                    new Question
                                    {
                                        Id = "q9",
                                        QuestionText = "Une variable n’est utilisée que pour la couleur d’un texte.",
                                        Options = new List<string> { "<img>", "<image>", "<pic>", "<photo>" },
                                        CorrectAnswerIndex = 0,
                                        Explanation = "La balise <img> avec l'attribut src est utilisée pour afficher des images."
                                    }
                                }
                            }
                        },
                        new Chapter
                        {
                            Id = "ch_002_2",
                            Title = "Chapitre 4 :Variables",
                            Description = "Maîtrisez les varibles que offre le Language C#.",
                            Order = 2,
                            // *** CHANGEZ CETTE URL ***
                            VideoUrl = "https://youtu.be/Ssu2rZUZY64?si=HnUFTF4sB2T9tI4p",
                            VideoDurationSeconds = 850,
                            Quiz = new Quiz
                            {
                                Id = "quiz_002_2",
                                Title = "Quiz : Variables",
                                PassingScore = 60,
                                Questions = new List<Question>
                                {
                                    new Question
                                    {
                                        Id = "q10",
                                        QuestionText = "En C#, un double ne peut contenir que du texte.",
                                        Options = new List<string> { "text-color: red;", "color: red;", "font-color: red;", "text: red;" },
                                        CorrectAnswerIndex = 1,
                                        Explanation = "La propriété CSS 'color' permet de changer la couleur du texte."
                                    },
                                    new Question
                                    {
                                        Id = "q11",
                                        QuestionText = "En C#, un string sert à stocker des images.",
                                        Options = new List<string> { "margin", "padding", "spacing", "border" },
                                        CorrectAnswerIndex = 1,
                                        Explanation = "La propriété 'padding' contrôle l'espacement interne (entre le contenu et la bordure)."
                                    },
                                    new Question
                                    {
                                        Id = "q12",
                                        QuestionText = "Une variable en C# peut changer de valeur pendant l’exécution du programme.",
                                        Options = new List<string> { "#maclasse", ".maclasse", "&maclasse", "*maclasse" },
                                        CorrectAnswerIndex = 1,
                                        Explanation = "Le symbole '.' suivi du nom de classe sélectionne les éléments par classe CSS."
                                    }
                                }
                            }
                        }
                    }
                },
                new Course
                {
                    Id = "course_003",
                    Title = "Introduction à  Operateurs ",
                    Description = "Découvrez les concepts fondamentaux de  Operateurs  en C#.",
                    Category = "Intelligence Artificielle",
                    // *** CHANGEZ CETTE IMAGE ***
                    ThumbnailUrl = "/images/2.png",
                    Level = "Intermédiaire",
                    EstimatedDurationMinutes = 240,
                    Chapters = new List<Chapter>
                    {
                        new Chapter
                        {
                            Id = "ch_003_1",
                            Title = "Chapitre 5 : Operateurs ",
                            Description = "Une introduction complète sur les Operateurs dans le Language C#.",
                            Order = 1,
                            // *** CHANGEZ CETTE URL ***
                            VideoUrl = "https://youtu.be/9zUUp3HOtxo?si=Ajrz71DDZ7FsCdwR",
                            VideoDurationSeconds = 1200,
                            Quiz = new Quiz
                            {
                                Id = "quiz_003_1",
                                Title = "Quiz : Fondamentaux de  Operateurs ",
                                PassingScore = 60,
                                Questions = new List<Question>
                                {
                                    new Question
                                    {
                                        Id = "q13",
                                        QuestionText = "Un opérateur ne sert qu’à envoyer des messages texte.",
                                        Options = new List<string> { "Information Artificielle", "Intelligence Artificielle", "Interaction Automatique", "Intégration Avancée" },
                                        CorrectAnswerIndex = 1,
                                        Explanation = "IA signifie Intelligence Artificielle - la capacité des machines à simuler l'intelligence humaine."
                                    },
                                    new Question
                                    {
                                        Id = "q14",
                                        QuestionText = "Un opérateur est un personnage dans un jeu vidéo.",
                                        Options = new List<string> { "Un programme qui suit des instructions fixes", "Un réseau de neurones qui apprend de données", "Un simple calculateur", "Un fichier texte" },
                                        CorrectAnswerIndex = 1,
                                        Explanation = "Le Machine Learning utilise des algorithmes qui apprennent automatiquement à partir de données."
                                    },
                                    new Question
                                    {
                                        Id = "q15",
                                        QuestionText = "Un opérateur en C# permet de combiner ou comparer des données.",
                                        Options = new List<string> { "Aucune différence", "L'IA est le concept général, le ML est une sous-catégorie", "Le ML est plus ancien que l'IA", "Ils sont des synonymes exacts" },
                                        CorrectAnswerIndex = 1,
                                        Explanation = "es opérateurs effectuent des calculs ou des comparaisons.."
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
