# Feature Flags API

API REST de gestion de feature flags développée en .NET 10, déployée sur Scaleway via GitHub Actions.

## Stack

- .NET 10 Minimal APIs
- xUnit (tests unitaires et d'intégration)
- Docker
- GitHub Actions (CI/CD)
- Scaleway Container Registry

## Lancer le projet

### En local

```bash
dotnet run --project FeatureFlagsApi
```

### Avec Docker

```bash
docker compose up --build
```

## Endpoints

### Health

| Méthode | Route | Description |
|---|---|---|
| GET | /api/health | Statut de l'API |
| GET | /api/version | Version de l'API |

### Users

| Méthode | Route | Description |
|---|---|---|
| POST | /api/users | Créer un utilisateur |
| GET | /api/users | Lister les utilisateurs |
| GET | /api/users/:id | Récupérer un utilisateur |
| PATCH | /api/users/:id | Modifier un utilisateur |
| DELETE | /api/users/:id | Supprimer un utilisateur |

### Groups

| Méthode | Route | Description |
|---|---|---|
| POST | /api/groups | Créer un groupe |
| GET | /api/groups | Lister les groupes |
| GET | /api/groups/:id | Récupérer un groupe |
| PATCH | /api/groups/:id | Modifier un groupe |
| DELETE | /api/groups/:id | Supprimer un groupe |
| POST | /api/groups/:id/users/:userId | Ajouter un utilisateur |
| DELETE | /api/groups/:id/users/:userId | Retirer un utilisateur |
| GET | /api/groups/:id/users | Lister les utilisateurs du groupe |

### Environments

| Méthode | Route | Description |
|---|---|---|
| POST | /api/environments | Créer un environnement |
| GET | /api/environments | Lister les environnements |
| GET | /api/environments/:name | Récupérer un environnement |
| PATCH | /api/environments/:name | Modifier un environnement |
| DELETE | /api/environments/:name | Supprimer un environnement |

### Features

| Méthode | Route | Description |
|---|---|---|
| POST | /api/features | Créer une feature |
| GET | /api/features | Lister les features |
| GET | /api/features/:key | Récupérer une feature |
| PATCH | /api/features/:key | Modifier une feature |
| DELETE | /api/features/:key | Supprimer une feature |
| PATCH | /api/features/:key/enable | Activer une feature |
| PATCH | /api/features/:key/disable | Désactiver une feature |
| PUT | /api/features/:key/environments/:env/config | Configurer pour un environnement |
| GET | /api/features/:key/environments/:env/config | Récupérer la config |
| DELETE | /api/features/:key/environments/:env/config | Supprimer la config |
| GET | /api/features/:key/evaluate?userId=1&env=prod | Évaluer l'accès |

## Codes HTTP

| Code | Description |
|---|---|
| 200 | OK |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request |
| 404 | Not Found |
| 409 | Conflict |
| 422 | Unprocessable Entity |
| 500 | Internal Server Error |

## Logique d'évaluation

Une feature est accessible si :

- Elle est activée globalement
- Elle est activée dans l'environnement demandé
- ET l'utilisateur est explicitement autorisé
- OU l'utilisateur appartient à un groupe autorisé
- OU l'utilisateur fait partie du pourcentage de rollout

## Tests

```bash
dotnet test
```

## CI/CD

- Chaque PR vers `main` déclenche le build, le lint et les tests
- Chaque release (`v*.*.*`) déclenche le déploiement sur Scaleway