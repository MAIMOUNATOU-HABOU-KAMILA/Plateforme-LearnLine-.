using System;
using LearnLineClient.Shared;

namespace LearnLineClient.Services
{
    /// <summary>
    /// Service d'état global de l'application
    /// Gère la session de l'utilisateur connecté et les données partagées
    /// </summary>
    public class AppStateService
    {
        // ═══════════════════════════════════════════════════════════════
        // PROPRIÉTÉS DE SESSION
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Utilisateur actuellement connecté
        /// </summary>
        public User? CurrentUser { get; set; }

        /// <summary>
        /// Session active de l'utilisateur
        /// </summary>
        public UserSession? CurrentSession { get; set; }

        /// <summary>
        /// Indique si l'utilisateur est connecté
        /// </summary>
        public bool IsLoggedIn => CurrentUser != null && CurrentSession != null;

        // ═══════════════════════════════════════════════════════════════
        // ÉVÉNEMENTS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Événement déclenché lors d'un changement d'état
        /// Permet aux composants Blazor de se mettre à jour automatiquement
        /// </summary>
        public event Action? OnChange;

        /// <summary>
        /// Notifie tous les composants abonnés qu'un changement d'état a eu lieu
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

        // ═══════════════════════════════════════════════════════════════
        // MÉTHODES DE GESTION DE SESSION
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Définit l'utilisateur connecté et crée une session
        /// </summary>
        public void SetLoggedInUser(User user)
        {
            CurrentUser = user;
            CurrentSession = new UserSession
            {
                UserId = user.UserId,
                UserName = user.Name,
                LoginTime = DateTime.Now,
                IsActive = true
            };

            Console.WriteLine($"[APP STATE] ✅ Utilisateur connecté: {user.Name}");
            NotifyStateChanged();
        }

        /// <summary>
        /// Déconnecte l'utilisateur et termine la session
        /// </summary>
        public void Logout()
        {
            if (CurrentSession != null)
            {
                CurrentSession.LogoutTime = DateTime.Now;
                CurrentSession.IsActive = false;
            }

            Console.WriteLine($"[APP STATE] 🚪 Utilisateur déconnecté: {CurrentUser?.Name}");

            CurrentUser = null;
            CurrentSession = null;

            NotifyStateChanged();
        }

        /// <summary>
        /// Obtient le nom de l'utilisateur connecté
        /// </summary>
        public string GetCurrentUserName()
        {
            return CurrentUser?.Name ?? "Invité";
        }

        /// <summary>
        /// Obtient l'ID de l'utilisateur connecté
        /// </summary>
        public string GetCurrentUserId()
        {
            return CurrentUser?.UserId ?? string.Empty;
        }
    }
}