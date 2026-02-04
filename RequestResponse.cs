// ============================================================
// FICHIER : Models/RequestResponse.cs
// ROLE    : Définit les types de messages échangés via TCP
// ============================================================
using System;
using System.Collections.Generic;

namespace LearnLineServer.Models
{
    // =============================================
    // REQUÊTES envoyées du CLIENT vers le SERVEUR
    // =============================================

    // Classe de base pour toutes les requêtes
    public class BaseRequest
    {
        // Type de requête (Register, Login, GetCourses, etc.)
        public string RequestType { get; set; }
    }

    // Requête d'inscription - le client envoie ces données pour créer un compte
    public class RegisterRequest : BaseRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public RegisterRequest()
        {
            RequestType = "Register";
        }
    }

    // Requête de connexion - le client envoie email et mot de passe
    public class LoginRequest : BaseRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public LoginRequest()
        {
            RequestType = "Login";
        }
    }

    // Requête pour obtenir la liste des cours disponibles
    public class GetCoursesRequest : BaseRequest
    {
        public GetCoursesRequest()
        {
            RequestType = "GetCourses";
        }
    }

    // Requête pour obtenir les détails d'un cours spécifique
    public class GetCourseDetailRequest : BaseRequest
    {
        public string CourseId { get; set; }

        public GetCourseDetailRequest()
        {
            RequestType = "GetCourseDetail";
        }
    }

    // Requête pour mettre à jour la progression d'un cours
    public class UpdateProgressRequest : BaseRequest
    {
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string ChapterId { get; set; }
        public double ProgressPercent { get; set; }
        public long TimeSpentSeconds { get; set; }

        public UpdateProgressRequest()
        {
            RequestType = "UpdateProgress";
        }
    }

    // Requête pour soumettre les réponses d'un test
    public class SubmitTestRequest : BaseRequest
    {
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string ChapterId { get; set; }
        public string QuizId { get; set; }
        public List<UserAnswer> Answers { get; set; }

        public SubmitTestRequest()
        {
            RequestType = "SubmitTest";
            Answers = new List<UserAnswer>();
        }
    }

    // Requête pour envoyer un message de chat
    public class SendMessageRequest : BaseRequest
    {
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string RecipientId { get; set; }
        public string RecipientName { get; set; }
        public string Content { get; set; }

        public SendMessageRequest()
        {
            RequestType = "SendMessage";
        }
    }

    // Requête pour obtenir l'historique des messages avec un utilisateur
    public class GetMessagesRequest : BaseRequest
    {
        public string UserId { get; set; }
        public string OtherUserId { get; set; }

        public GetMessagesRequest()
        {
            RequestType = "GetMessages";
        }
    }

    // Requête pour obtenir la liste des utilisateurs connectés
    public class GetConnectedUsersRequest : BaseRequest
    {
        public GetConnectedUsersRequest()
        {
            RequestType = "GetConnectedUsers";
        }
    }

    // Requête pour obtenir les statistiques d'un utilisateur
    public class GetStatisticsRequest : BaseRequest
    {
        public string UserId { get; set; }

        public GetStatisticsRequest()
        {
            RequestType = "GetStatistics";
        }
    }

    // Requête pour mettre à jour le cours actuel de l'utilisateur
    public class SetCurrentCourseRequest : BaseRequest
    {
        public string UserId { get; set; }
        public string CourseName { get; set; }

        public SetCurrentCourseRequest()
        {
            RequestType = "SetCurrentCourse";
        }
    }

    // Requête de déconnexion
    public class LogoutRequest : BaseRequest
    {
        public string UserId { get; set; }

        public LogoutRequest()
        {
            RequestType = "Logout";
        }
    }

    // =============================================
    // RÉPONSES envoyées du SERVEUR vers le CLIENT
    // =============================================

    // Classe de base pour toutes les réponses
    public class BaseResponse
    {
        // Indique si la requête a été traitée avec succès
        public bool Success { get; set; }

        // Message de retour (succès ou erreur)
        public string Message { get; set; }
    }

    // Réponse d'inscription
    public class RegisterResponse : BaseResponse
    {
        public string UserId { get; set; }
    }

    // Réponse de connexion - contient les données de l'utilisateur si succès
    public class LoginResponse : BaseResponse
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
    }

    // Réponse avec la liste des cours
    public class GetCoursesResponse : BaseResponse
    {
        public List<Course> Courses { get; set; }

        public GetCoursesResponse()
        {
            Courses = new List<Course>();
        }
    }

    // Réponse avec les détails d'un cours
    public class GetCourseDetailResponse : BaseResponse
    {
        public Course Course { get; set; }
    }

    // Réponse de mise à jour de progression
    public class UpdateProgressResponse : BaseResponse { }

    // Réponse de soumission de test avec le score
    public class SubmitTestResponse : BaseResponse
    {
        public int ScoreObtained { get; set; }
        public int TotalScore { get; set; }
        public double PercentageScore { get; set; }
        public bool Passed { get; set; }
        public List<TestAnswerFeedback> Feedback { get; set; }

        public SubmitTestResponse()
        {
            Feedback = new List<TestAnswerFeedback>();
        }
    }

    // Feedback pour chaque réponse du test
    public class TestAnswerFeedback
    {
        public string QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int UserAnswer { get; set; }
        public int CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public string Explanation { get; set; }
    }

    // Réponse d'envoi de message
    public class SendMessageResponse : BaseResponse
    {
        public ChatMessage SentMessage { get; set; }
    }

    // Réponse avec l'historique des messages
    public class GetMessagesResponse : BaseResponse
    {
        public List<ChatMessage> Messages { get; set; }

        public GetMessagesResponse()
        {
            Messages = new List<ChatMessage>();
        }
    }

    // Réponse avec la liste des utilisateurs connectés
    public class GetConnectedUsersResponse : BaseResponse
    {
        public List<ConnectedUserInfo> Users { get; set; }

        public GetConnectedUsersResponse()
        {
            Users = new List<ConnectedUserInfo>();
        }
    }

    // Info simplifiée d'un utilisateur connecté
    public class ConnectedUserInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string CurrentCourse { get; set; }
        public DateTime LoginTime { get; set; }
    }

    // Réponse avec les statistiques de l'utilisateur
    public class GetStatisticsResponse : BaseResponse
    {
        public List<CourseProgress> CoursesProgress { get; set; }
        public List<TestResult> TestResults { get; set; }
        public int TotalCoursesStarted { get; set; }
        public int TotalCoursesCompleted { get; set; }
        public int TotalTestsPassed { get; set; }
        public int TotalTestsTaken { get; set; }
        public double AverageTestScore { get; set; }
        public long TotalTimeSpentSeconds { get; set; }

        public GetStatisticsResponse()
        {
            CoursesProgress = new List<CourseProgress>();
            TestResults = new List<TestResult>();
        }
    }

    // Réponse générique pour les autres requêtes
    public class GenericResponse : BaseResponse { }

    // Notification de nouveau message reçu (envoyée en "push" au destinataire)
    public class NewMessageNotification
    {
        public string NotificationType { get; set; }
        public ChatMessage Message { get; set; }

        public NewMessageNotification()
        {
            NotificationType = "NewMessage";
        }
    }
}
