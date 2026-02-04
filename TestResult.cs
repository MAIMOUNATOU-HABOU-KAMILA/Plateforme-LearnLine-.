// ============================================================
// FICHIER : Models/TestResult.cs
// ROLE    : Stocke les résultats d'un test passé par un utilisateur
// ============================================================
using System;
using System.Collections.Generic;

namespace LearnLineServer.Models
{
    // Représente le résultat d'un test pour un utilisateur
    public class TestResult
    {
        // Identifiant unique du résultat
        public string Id { get; set; }

        // Identifiant du cours lié à ce test
        public string CourseId { get; set; }

        // Nom du cours lié au test
        public string CourseName { get; set; }

        // Identifiant du chapitre lié au test
        public string ChapterId { get; set; }

        // Score obtenu sur le total
        public int ScoreObtained { get; set; }

        // Score total possible
        public int TotalScore { get; set; }

        // Pourcentage de réussite calculé automatiquement
        public double PercentageScore { get; set; }

        // Date et heure à laquelle le test a été passé
        public DateTime TakenAt { get; set; }

        // Liste des réponses données par l'utilisateur
        public List<UserAnswer> Answers { get; set; }

        // Constructeur par défaut
        public TestResult()
        {
            Answers = new List<UserAnswer>();
        }
    }

    // Représente une réponse donnée par l'utilisateur pour une question
    public class UserAnswer
    {
        // Identifiant de la question
        public string QuestionId { get; set; }

        // Réponse choisie par l'utilisateur (index de la réponse)
        public int SelectedAnswer { get; set; }

        // Indique si la réponse est correcte
        public bool IsCorrect { get; set; }
    }
}
