// ============================================================
// FICHIER : Models/User.cs
// ROLE    : Représente un utilisateur inscrit dans le système
// ============================================================
using System;
using System.Collections.Generic;

namespace LearnLineServer.Models
{
    // Classe représentant un utilisateur de la plateforme
    public class User
    {
        // Identifiant unique de l'utilisateur
        public string Id { get; set; }

        // Nom complet de l'utilisateur
        public string Name { get; set; }

        // Adresse email (utilisée comme identifiant de connexion)
        public string Email { get; set; }

        // Mot de passe (stocké en texte simple pour cette version - en production, utiliser un hash)
        public string Password { get; set; }

        // Date d'inscription de l'utilisateur
        public DateTime RegisteredAt { get; set; }

        // Liste des cours suivis par l'utilisateur avec leur progression
        public List<CourseProgress> CoursesProgress { get; set; }

        // Liste des résultats de tests passés par l'utilisateur
        public List<TestResult> TestResults { get; set; }

        // Constructeur par défaut nécessaire pour la désérialisation JSON
        public User()
        {
            CoursesProgress = new List<CourseProgress>();
            TestResults = new List<TestResult>();
        }

        // Constructeur complet pour créer un nouvel utilisateur
        public User(string id, string name, string email, string password)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            RegisteredAt = DateTime.Now;
            CoursesProgress = new List<CourseProgress>();
            TestResults = new List<TestResult>();
        }
    }
}
