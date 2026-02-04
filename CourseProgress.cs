// ============================================================
// FICHIER : Models/CourseProgress.cs
// ROLE    : Suit la progression d'un utilisateur dans un cours
// ============================================================
using System;

namespace LearnLineServer.Models
{
    // Représente la progression d'un utilisateur dans un cours spécifique
    public class CourseProgress
    {
        // Identifiant du cours concerné
        public string CourseId { get; set; }

        // Nom du cours
        public string CourseName { get; set; }

        // Identifiant du chapitre en cours
        public string CurrentChapterId { get; set; }

        // Pourcentage de progression dans le cours (0 à 100)
        public double ProgressPercent { get; set; }

        // Temps total passé sur ce cours en secondes
        public long TotalTimeSpentSeconds { get; set; }

        // Date de début du cours
        public DateTime StartedAt { get; set; }

        // Date de la dernière activité sur ce cours
        public DateTime LastActivityAt { get; set; }

        // Indique si le cours est terminé
        public bool IsCompleted { get; set; }

        // Constructeur par défaut
        public CourseProgress()
        {
            ProgressPercent = 0;
            TotalTimeSpentSeconds = 0;
            IsCompleted = false;
        }

        // Constructeur avec paramètres
        public CourseProgress(string courseId, string courseName)
        {
            CourseId = courseId;
            CourseName = courseName;
            ProgressPercent = 0;
            TotalTimeSpentSeconds = 0;
            StartedAt = DateTime.Now;
            LastActivityAt = DateTime.Now;
            IsCompleted = false;
        }
    }
}
