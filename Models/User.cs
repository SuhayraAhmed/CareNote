using Google.Cloud.Firestore;
using System;

namespace CareNote.Models 
{
    [FirestoreData] // FirestoreData attribut används för att mappa till Google Firestore
    public class User // Användarmodell som lagras i Firestore
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty]
        public string DisplayName { get; set; } = string.Empty;

        [FirestoreProperty]
        public string AuthProvider { get; set; } = "email";

        [FirestoreProperty]
        public bool IsActive { get; set; } = true;

        [FirestoreProperty]
        public string Role { get; set; } = "user";

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    }
} // Används i AuthController för autentisering och användarhantering
