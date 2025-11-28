# Kako testirati Niles Chores API

## 1. Pokretanje Backend API-ja

```bash
cd server/src/Niles.Chores.Api
dotnet run
```

API će se pokrenuti na **http://localhost:5032**

## 2. Testiranje API endpointova

### Opcija A: Korištenje test skripte

```bash
cd server
./test-api.sh
```

**Napomena:** Skripta zahtijeva `jq` za formatiranje JSON-a. Ako nemaš `jq`, instaliraj ga:
- macOS: `brew install jq`
- Linux: `sudo apt-get install jq` ili `sudo yum install jq`

### Opcija B: Ručno testiranje s curl

#### Health check
```bash
curl http://localhost:5032/api/health
```

#### Dohvati prioritizirane chores za danas
```bash
curl "http://localhost:5032/api/chores?date=2025-11-28"
```

#### Dohvati prioritizirane chores za specifičan datum
```bash
curl "http://localhost:5032/api/chores?date=2025-11-28"
```

#### Pošalji completion chores
```bash
curl -X POST "http://localhost:5032/api/chores/submit" \
  -H "Content-Type: application/json" \
  -d '{
    "date": "2025-11-28",
    "duration": 45,
    "note": "Test submission",
    "completedChoreIds": ["101", "102"]
  }'
```

#### Dohvati housekeeping logove
```bash
curl "http://localhost:5032/api/housekeeping"
```

#### Dohvati housekeeping log po ID-u
```bash
# Prvo dohvati listu da vidiš ID-eve
curl "http://localhost:5032/api/housekeeping"
# Zatim koristi obfuscated ID
curl "http://localhost:5032/api/housekeeping/{obfuscated-id}"
```

### Opcija C: Korištenje HTTP fajla (Rider/VS Code)

Otvori `server/src/Niles.Chores.Api/Niles.Chores.Api.http` u IDE-u i koristi built-in HTTP client.

## 3. Testiranje Frontend stranica

### Public Chores stranica
1. Pokreni backend API (korak 1)
2. Otvori `server/public/chores/index.html` u browseru
3. Stranica će automatski pokušati spojiti se na `/api/chores` i `/api/chores/submit`

**Napomena:** Ako koristiš file:// protokol, browser će blokirati CORS zahtjeve. Koristi lokalni web server:

```bash
# Python 3
cd server/public/chores
python3 -m http.server 8000

# Node.js (http-server)
npx http-server server/public/chores -p 8000

# PHP
cd server/public/chores
php -S localhost:8000
```

Zatim otvori: http://localhost:8000/index.html

### Admin Chores stranica
1. Pokreni backend API
2. Otvori `server/internal/admin/chores/index.html` u browseru
3. Stranica će dohvatiti prioritizirane chores za danas

**Napomena:** CRUD operacije (create/edit/delete) trenutno nisu implementirane u backend-u, pa će se prikazati poruka da nisu podržane.

### Admin Housekeeping stranica
1. Pokreni backend API
2. Otvori `server/internal/admin/housekeeping/index.html` u browseru
3. Stranica će dohvatiti housekeeping logove

## 4. Debugging

### Provjeri da li API radi
```bash
curl http://localhost:5032/api/health
```

Očekivani odgovor:
```json
{"status":"ok"}
```

### Provjeri CORS probleme
Ako vidiš CORS greške u browser konzoli, dodaj CORS podršku u `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ...

app.UseCors();
```

### Provjeri backend logove
Backend će ispisivati logove u konzolu. Ako vidiš greške, provjeri:
- Da li su svi servisi registrirani u DI
- Da li su svi endpointi ispravno mapirani
- Da li request body odgovara DTO strukturi

## 5. Test podaci

Backend koristi in-memory storage, pa će se podaci resetirati pri svakom restartu.

### Kreiranje test housekeeping loga
```bash
curl -X POST "http://localhost:5032/api/chores/submit" \
  -H "Content-Type: application/json" \
  -d '{
    "date": "2025-11-28",
    "duration": 60,
    "note": "Test housekeeping session",
    "completedChoreIds": ["101", "102", "103"]
  }'
```

Zatim provjeri:
```bash
curl "http://localhost:5032/api/housekeeping"
```

## 6. Troubleshooting

### API se ne pokreće
- Provjeri da li si u ispravnom direktoriju
- Provjeri da li su svi NuGet paketi instalirani: `dotnet restore`
- Provjeri da li build prolazi: `dotnet build`

### 404 greške
- Provjeri da li koristiš ispravan port (5032)
- Provjeri da li endpoint path odgovara controller route-u

### 400 Bad Request
- Provjeri da li request body odgovara DTO strukturi
- Provjeri da li su svi required fieldovi prisutni
- Provjeri format datuma (YYYY-MM-DD)

### CORS greške
- Koristi lokalni web server umjesto file:// protokola
- Ili dodaj CORS podršku u backend (vidi Debugging sekciju)

