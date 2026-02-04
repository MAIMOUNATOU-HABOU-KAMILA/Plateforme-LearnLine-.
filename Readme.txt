════════════════════════════════════════════════════════════════════════════════
                        PROJET LEARNLINE - README
                     Plateforme E-Learning Interactive:appler LearnLine 
════════════════════════════════════════════════════════════════════════════════
════════════════════════════════════════════════════════════════════════════════
DESCRIPTION DU PROJET
════════════════════════════════════════════════════════════════════════════════
LearnLine est une plateforme moderne d’e-learning basée sur une architecture client–serveur.
Elle permet à plusieurs utilisateurs de se connecter simultanément à un serveur central afin
 d’accéder à des cours en ligne, de suivre leur progression, de passer des tests et de communiquer
 entre eux grâce à un système de messagerie en temps réel.
Le projet repose sur les technologies .NET 8, Blazor pour l’interface client et SignalR pour la communication 
instantanée, tout en assurant une séparation claire entre la partie serveur et la partie client.
Cette architecture permet à plusieurs machines d’un même réseau local de se connecter à un seul serveur
 LearnLine via une connexion TCP sécurisée.
LearnLine a pour objectif d’offrir une expérience d’apprentissage interactive, connectée et centralisée,
 tout en assurant le suivi précis des activités et des performances des utilisateurs.
════════════════════════════════════════════════════════════════════════════════
ARCHITECTURE
════════════════════════════════════════════════════════════════════════════════

SERVEUR (LearnLineServer)
- Type : Console .NET Core 3.1  
- Port : 8888 (TCP)
- Base de données : SQLite
- Activation : Mot de passe "LearnLine"

CLIENT (LearnLineClient)
- Type : Application Web Blazor Server .NET 8.0
- Interface : Navigateur web et un server automatique ayant pour port: 5000
- Connexion : TCP au serveur

Les deux solutions sont SÉPARÉES pour permettre la connexion de plusieurs
machines différentes via réseau local.

════════════════════════════════════════════════════════════════════════════════
PRÉREQUIS
════════════════════════════════════════════════════════════════════════════════

✓ Visual Studio 2026 (avec workload "Développement ASP.NET et web")
✓ .NET Core 3.1 SDK
✓ .NET 8.0 SDK  
✓ Navigateur web moderne
✓ Windows 10/11, Linux ou macOS

Vérification : Ouvrir CMD et taper "dotnet --version"
Doit afficher 5.0.xxx ou supérieur

════════════════════════════════════════════════════════════════════════════════
DÉMARRAGE
════════════════════════════════════════════════════════════════════════════════

⚠️ TOUJOURS DÉMARRER LE SERVEUR EN PREMIER

SERVEUR :
1. Ouvrir LearnLineServer.sln dans Visual Studio
2. Appuyer sur F5
3. Entrer le mot de passe : LearnLine
4. Le serveur démarre sur le port 8888
5. Laisser la console ouverte

CLIENT :
1. Ouvrir LearnLineClient.sln dans une NOUVELLE instance VS
2. Appuyer sur F5  
3. Le navigateur s'ouvre
4. Cliquer sur "Inscription"
5. Entrer adresse serveur : 127.0.0.1 (même machine)
6. Se connecter au serveur
7. S'inscrire puis se connecter

════════════════════════════════════════════════════════════════════════════════
FONCTIONNALITÉS
════════════════════════════════════════════════════════════════════════════════

✓ Inscription et connexion sécurisée
✓ Catalogue de cours avec vidéos (YouTube ou locales)
✓ Lecteur vidéo intégré
✓ Suivi automatique de la progression
✓ Tests d'évaluation avec correction automatique
✓ Statistiques visuelles (graphiques, courbes)
✓ Chat en temps réel type WhatsApp
✓ Logs exhaustifs de toutes les activités
✓ Support multi-utilisateurs simultanés
✓ Connexion réseau local entre machines
✓ Commandes administrateur (DB, CLIENTS, STATS, EXIT)

════════════════════════════════════════════════════════════════════════════════
COMMANDES SERVEUR
════════════════════════════════════════════════════════════════════════════════

Une fois le serveur démarré, tapez :

DB      - Affiche TOUTE la base de données
CLIENTS - Liste des clients connectés avec infos
STATS   - Statistiques du serveur
HELP    - Liste des commandes
CLEAR   - Effacer l'écran
EXIT    - Arrêter le serveur (avec confirmation)

════════════════════════════════════════════════════════════════════════════════
RÉSEAU LOCAL
════════════════════════════════════════════════════════════════════════════════

Pour connecter plusieurs machines :

SERVEUR :
1. Trouver l'adresse IP (ipconfig sur Windows)
   Exemple : 192.168.1.100
2. Autoriser port 8888 dans le pare-feu
3. Démarrer le serveur normalement

CLIENTS :
1. Page Login > Adresse serveur : 192.168.1.100
2. Cliquer "Se connecter au serveur"
3. S'inscrire/Connecter normalement

════════════════════════════════════════════════════════════════════════════════
STRUCTURE DES FICHIERS
════════════════════════════════════════════════════════════════════════════════

LearnLine/
├── LearnLineServer/                    ← PROJET SERVEUR (séparé du client)
│   ├── LearnLineServer.csproj          ← Fichier projet .NET Framework 8.0
│   ├── Program.cs                      ← Point d'entrée du serveur
│   ├── Models/
│   │   ├── User.cs                     ← Modèle utilisateur
│   │   ├── Course.cs                   ← Modèle cours, chapitre, quiz, question
│   │   ├── CourseProgress.cs           ← Progression dans un cours
│   │   ├── TestResult.cs              ← Résultat d'un test
│   │   ├── UserSession.cs             ← Session de connexion
│   │   ├── ChatMessage.cs             ← Message de chat
│   │   └── RequestResponse.cs         ← Requêtes/Réponses TCP
│   ├── Services/
│   │   └── TcpServerService.cs        ← Serveur TCP port 8888
│   └── Data/
│       └── DatabaseService.cs         ← Base de données JSON
│
   


LearnLineClient/
│
├── 📄 LearnLineClient.csproj      → Fichier projet .NET 8.0
├── 📄 Program.cs                   → Point d'entrée
├── 📄 appsettings.json            → Configuration
├── 📄 App.razor                    → Composant racine Blazor
├── 📄 _Imports.razor              → Directives globales
├── 📄 README.md                    → Documentation projet
├── 📄 GUIDE_UTILISATION.md        → Guide complet d'utilisation
│
├── 📁 Shared/
│   ├── MainLayout.razor           → Layout avec menu
│   └── Models.cs                  → Tous les modèles de données
│
├── 📁 Services/
│   ├── TcpClientService.cs        → Client TCP (communication serveur)
│   └── AppStateService.cs         → Gestion état global
│
├── 📁 Pages/
│   ├── _Host.cshtml               → Page hôte HTML
│   ├── Index.razor                → Page d'accueil
│   ├── About.razor                → Page à propos
│   ├── Login.razor                → Page connexion
│   ├── Register.razor             → Page inscription
│   ├── Courses.razor              → Liste des cours
│   ├── CourseDetail.razor         → Détail d'un cours
│   ├── ChapterView.razor          → Vidéo + progression
│   ├── TestPage.razor             → Quiz/Test
│   ├── Chat.razor                 → Chat temps réel
│   └── Statistics.razor           → Statistiques + graphiques
│
└── 📁 wwwroot/
    ├── 📁 css/
    │   └── styles.css             → 500+ lignes de CSS
    └── 📁 images/
        
════════════════════════════════════════════════════════════════════════════════
BASE DE DONNÉES
════════════════════════════════════════════════════════════════════════════════

Type : SQLite
Fichier : LearnLineDB.db  
Emplacement : LearnLineServer/bin/Debug/netcoreapp3.1/

8 TABLES :
1. Users              - Utilisateurs inscrits
2. ConnectionSessions - Sessions de connexion (heures, durées, IP)
3. Courses            - Cours disponibles
4. UserProgress       - Progression de chaque utilisateur
5. Tests              - Questions de test
6. TestResults        - Résultats des tests passés
7. ChatMessages       - Messages échangés
8. ActivityLogs       - TOUS les logs d'activité

Pour voir la base : Taper "DB" dans la console serveur


════════════════════════════════════════════════════════════════════════════════
           PROJET : LearnLine (Clients/Server) en mode TCP
════════════════════════════════════════════════════════════════════════════════
