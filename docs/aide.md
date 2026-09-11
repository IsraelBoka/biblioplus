AIDES
Ce fichier declare les aides consultees pendant le projet. Les elements retenus ont ete relus, adaptes au sujet BiblioPlus et verifies dans le depot avant validation.

Taches realisees avec l'appui de Claude (IA)
La conception (modele de donnees, regles metier, architecture en couches) a ete faite a la main. L'IA a servi surtout a repliquer et etendre ces concepts, et a produire les parties repetitives ou techniques :

- Replication de l'architecture : generation du code repetitif a partir du modele fait main (entites, configurations EF, repositories, Unit of Work, DTOs) en suivant le meme patron.
- Seed : donnees de demonstration et comptes par defaut, garantis meme sur une base existante.
- UI / UX : vues Razor, mise en page (_Layout), systeme de design CSS, pages de connexion / inscription et espace adherent.
- Authentification : login par cookie, roles (Admin / Adherent), protection des controleurs, hachage des mots de passe (PBKDF2).
- Integration Google : envoi d'e-mails via SMTP Gmail (alertes emprunt / penalite / bienvenue), configuration par fichier .env.
