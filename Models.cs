using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LearnLineClient.Shared
{
    // ═══════════════════════════════════════════════════════════════════
    // MODÈLE UTILISATEUR
    // ═══════════════════════════════════════════════════════════════════
    public class User
    {
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public List<string> EnrolledCourses { get; set; } = new List<string>();
    }

    // ═══════════════════════════════════════════════════════════════════
    // MODÈLE SESSION UTILISATEUR
    // ═══════════════════════════════════════════════════════════════════
    public class UserSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; } = DateTime.Now;
        public DateTime? LogoutTime { get; set; }
        public bool IsActive { get; set; } = true;
        public class CoursesResponse
        {
            public List<Course> Courses { get; set; } = new();
        }
    }

    public class Course
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string ThumbnailUrl { get; set; } = string.Empty;

        public string Level { get; set; } = string.Empty;

        public int EstimatedDurationMinutes { get; set; }

        public List<Chapter> Chapters { get; set; } = new();
    }


    public class Chapter
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Order { get; set; }

        public string VideoUrl { get; set; } = string.Empty;

        public int VideoDurationSeconds { get; set; }

        public Quiz? Quiz { get; set; }
    }


    public class Quiz
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public int PassingScore { get; set; }

        public List<Question> Questions { get; set; } = new();
    }


    public class Question
    {
        public string Id { get; set; } = string.Empty;

        public string QuestionText { get; set; } = string.Empty;

        public List<string> Options { get; set; } = new();

        public int CorrectAnswerIndex { get; set; }

        public string Explanation { get; set; } = string.Empty;
    }
    // ═══════════════════════════════════════════════════════════════════
    // MODÈLE PROGRESSION COURS
    // ═══════════════════════════════════════════════════════════════════
    public class CourseProgress
    {
        public string ProgressId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string ChapterId { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public DateTime LastAccessed { get; set; } = DateTime.Now;
        public bool IsCompleted { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════
    // MODÈLE RÉSULTAT DE TEST
    // ═══════════════════════════════════════════════════════════════════
    public class TestResult
    {
        public string ResultId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string ChapterId { get; set; } = string.Empty;
        public string QuizId { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CompletedDate { get; set; } = DateTime.Now;
        public List<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }

    // ═══════════════════════════════════════════════════════════════════
    // MODÈLE RÉPONSE UTILISATEUR
    // ═══════════════════════════════════════════════════════════════════
    public class UserAnswer
    {
        public string QuestionId { get; set; } = string.Empty;
        public int SelectedAnswerIndex { get; set; }
        public bool IsCorrect { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════
    // MODÈLE MESSAGE CHAT
    // ═══════════════════════════════════════════════════════════════════
    public class ChatMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    // ═══════════════════════════════════════════════════════════════════
    // MODÈLE REQUÊTE TCP
    // ═══════════════════════════════════════════════════════════════════
    public class TcpRequest
    {
        public string RequestType { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════
    // MODÈLE RÉPONSE TCP 
    // ═══════════════════════════════════════════════════════════════════
    public class TcpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}