// ============================================================
// FICHIER : Models/ChatMessage.cs
// ROLE    : Représente un message échangé entre deux utilisateurs
// ============================================================
using System;

namespace LearnLineServer.Models
{
    // Représente un message dans le système de chat
    public class ChatMessage
    {
        // Identifiant unique du message
        public string Id { get; set; }

        // Identifiant de l'utilisateur qui a envoyé le message
        public string SenderId { get; set; }

        // Nom de l'expéditeur (pour l'affichage)
        public string SenderName { get; set; }

        // Identifiant de l'utilisateur destinataire
        public string RecipientId { get; set; }

        // Nom du destinataire (pour l'affichage)
        public string RecipientName { get; set; }

        // Contenu du message
        public string Content { get; set; }

        // Date et heure d'envoi du message
        public DateTime SentAt { get; set; }

        // Indique si le message a été lu par le destinataire
        public bool IsRead { get; set; }

        // Constructeur par défaut
        public ChatMessage()
        {
            IsRead = false;
        }

        // Constructeur avec paramètres
        public ChatMessage(string id, string senderId, string senderName, string recipientId, string recipientName, string content)
        {
            Id = id;
            SenderId = senderId;
            SenderName = senderName;
            RecipientId = recipientId;
            RecipientName = recipientName;
            Content = content;
            SentAt = DateTime.Now;
            IsRead = false;
        }
    }
}
