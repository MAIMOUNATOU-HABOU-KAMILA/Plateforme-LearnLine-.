// ============================================================
// FICHIER : Models/Course.cs
// ROLE    : Définit la structure d'un cours avec ses chapitres et tests
// ============================================================
using System;
using System.Collections.Generic;

namespace LearnLineServer.Models
{
    // Représente un cours disponible sur la plateforme
    public class Course
    {
        // Identifiant unique du cours
        public string Id { get; set; }

        // Titre du cours
        public string Title { get; set; }

        // Description du cours
        public string Description { get; set; }

        // Catégorie du cours (ex: Programmation, Design, etc.)
        public string Category { get; set; }

        // URL de l'image miniature du cours
        // *** VOUS POUVEZ CHANGER CES IMAGES ICI - Remplacez les chemins par vos propres images ***
        public string ThumbnailUrl { get; set; }

        // Niveau de difficulté (Débutant, Intermédiaire, Avancé)
        public string Level { get; set; }

        // Durée estimée du cours en minutes
        public int EstimatedDurationMinutes { get; set; }

        // Liste des chapitres du cours
        public List<Chapter> Chapters { get; set; }

        // Constructeur par défaut
        public Course()
        {
            Chapters = new List<Chapter>();
        }
    }

    // Représente un chapitre dans un cours
    public class Chapter
    {
        // Identifiant unique du chapitre
        public string Id { get; set; }

        // Titre du chapitre
        public string Title { get; set; }

        // Description du chapitre
        public string Description { get; set; }

        // Ordre du chapitre dans le cours (1, 2, 3, etc.)
        public int Order { get; set; }

        // URL de la vidéo du chapitre
        // *** VOUS POUVEZ AJOUTER VOS PROPRES VIDÉOS ICI ***
        // Exemple YouTube: "https://www.youtube.com/embed/VOTRE_ID_VIDEO"
        // Exemple vidéo locale: vous pouvez héberger une vidéo et mettre son URL
        public string VideoUrl { get; set; }

        // Durée de la vidéo en secondes
        public int VideoDurationSeconds { get; set; }

        // Test associé à ce chapitre (null si pas de test)
        // *** VOUS POUVEZ MODIFIER LES TESTS ICI EN CHANGEANT Les questions et réponses ***
        public Quiz Quiz { get; set; }
    }

    // Représente un quiz/test associé à un chapitre
    public class Quiz
    {
        // Identifiant unique du quiz
        public string Id { get; set; }

        // Titre du quiz
        public string Title { get; set; }

        // Liste des questions du quiz
        // *** AJOUTER OU MODIFIER VOS QUESTIONS ICI ***
        public List<Question> Questions { get; set; }

        // Score minimum requis pour réussir le test (en pourcentage)
        public int PassingScore { get; set; }

        // Constructeur par défaut
        public Quiz()
        {
            Questions = new List<Question>();
            PassingScore = 60; // 60% par défaut pour réussir
        }
    }

    // Représente une question dans un quiz
    public class Question
    {
        // Identifiant unique de la question
        public string Id { get; set; }

        // Texte de la question
        public string QuestionText { get; set; }

        // Liste des options de réponse
        public List<string> Options { get; set; }

        // Index de la réponse correcte dans la liste des options (commence à 0)
        public int CorrectAnswerIndex { get; set; }

        // Explication de la réponse correcte
        public string Explanation { get; set; }

        // Constructeur par défaut
        public Question()
        {
            Options = new List<string>();
        }
    }
}
