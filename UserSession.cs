// ============================================================
// FICHIER : Models/UserSession.cs
// ROLE    : Suit les sessions actives des utilisateurs connectés
// ============================================================
using System;

namespace LearnLineServer.Models
{
    // Représente une session active d'un utilisateur connecté au serveur
    public class UserSession
    {
        // Identifiant unique de la session
        public string SessionId { get; set; }

        // Identifiant de l'utilisateur associé
        public string UserId { get; set; }

        // Nom de l'utilisateur (pour l'affichage)
        public string UserName { get; set; }

        // Email de l'utilisateur
        public string UserEmail { get; set; }

        // Identifiant de la connexion TCP au serveur
        public string ConnectionId { get; set; }

        // Date et heure de début de session (quand l'utilisateur a fait Login)
        public DateTime LoginTime { get; set; }

        // Date et heure de fin de session (quand l'utilisateur se déconnecte)
        public DateTime? LogoutTime { get; set; }

        // Le cours actuellement suivi par l'utilisateur (null si aucun)
        public string CurrentCourse { get; set; }

        // Indique si la session est encore active
        public bool IsActive { get; set; }

        // Constructeur par défaut
        public UserSession()
        {
            IsActive = true;
        }

        // Constructeur avec paramètres
        public UserSession(string sessionId, string userId, string userName, string userEmail, string connectionId)
        {
            SessionId = sessionId;
            UserId = userId;
            UserName = userName;
            UserEmail = userEmail;
            ConnectionId = connectionId;
            LoginTime = DateTime.Now;
            IsActive = true;
        }
    }
}
