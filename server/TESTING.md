# How to Test Niles Chores API

## 1. Starting the Backend API

```bash
cd server/src/Niles.Chores.Api
dotnet run
```

The API will start on **http://localhost:5032**

## 2. Testing API Endpoints

### Option A: Using the Test Script

```bash
cd server
./test-api.sh
```

**Note:** The script requires `jq` for JSON formatting. If you don't have `jq`, install it:
- macOS: `brew install jq`
- Linux: `sudo apt-get install jq` or `sudo yum install jq`

### Option B: Manual Testing with curl

#### Health check
```bash
curl http://localhost:5032/api/health
```

#### Get prioritized chores for today
```bash
curl "http://localhost:5032/api/chores?date=2025-11-28"
```

#### Get prioritized chores for a specific date
```bash
curl "http://localhost:5032/api/chores?date=2025-11-28"
```

#### Submit chore completion
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

#### Get housekeeping logs
```bash
curl "http://localhost:5032/api/housekeeping"
```

#### Get housekeeping log by ID
```bash
# First get the list to see IDs
curl "http://localhost:5032/api/housekeeping"
# Then use the obfuscated ID
curl "http://localhost:5032/api/housekeeping/{obfuscated-id}"
```

### Option C: Using HTTP File (Rider/VS Code)

Open `server/src/Niles.Chores.Api/Niles.Chores.Api.http` in your IDE and use the built-in HTTP client.

## 3. Testing Frontend Pages

### Public Chores Page
1. Start the backend API (step 1)
2. Open `server/public/chores/index.html` in a browser
3. The page will automatically try to connect to `/api/chores` and `/api/chores/submit`

**Note:** If you use the file:// protocol, the browser will block CORS requests. Use a local web server:

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

Then open: http://localhost:8000/index.html

### Admin Chores Page
1. Start the backend API
2. Open `server/internal/admin/chores/index.html` in a browser
3. The page will fetch all chores for admin management

**Note:** CRUD operations (create/edit/delete) are now fully implemented in the backend.

### Admin Housekeeping Page
1. Start the backend API
2. Open `server/internal/admin/housekeeping/index.html` in a browser
3. The page will fetch housekeeping logs

## 4. Debugging

### Check if API is Running
```bash
curl http://localhost:5032/api/health
```

Expected response:
```json
{"status":"ok"}
```

### Check CORS Issues
If you see CORS errors in the browser console, CORS support is already added in `Program.cs`:

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

### Check Backend Logs
The backend will output logs to the console. If you see errors, check:
- Whether all services are registered in DI
- Whether all endpoints are correctly mapped
- Whether the request body matches the DTO structure

## 5. Test Data

The backend uses in-memory storage, so data will reset on each restart.

### Creating a Test Housekeeping Log
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

Then check:
```bash
curl "http://localhost:5032/api/housekeeping"
```

### Creating a Test Chore
```bash
curl -X POST "http://localhost:5032/api/chores" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Chore",
    "description": "Test description",
    "type": "Maintenance",
    "repeatEveryWeeks": 2
  }'
```

### Marking a Chore as Important
```bash
curl -X POST "http://localhost:5032/api/chores/{id}/important" \
  -H "Content-Type: application/json" \
  -d '{
    "date": "2025-12-15",
    "description": "This is an important chore"
  }'
```

## 6. Troubleshooting

### API Won't Start
- Check if you're in the correct directory
- Check if all NuGet packages are installed: `dotnet restore`
- Check if the build passes: `dotnet build`

### 404 Errors
- Check if you're using the correct port (5032)
- Check if the endpoint path matches the controller route

### 400 Bad Request
- Check if the request body matches the DTO structure
- Check if all required fields are present
- Check the date format (YYYY-MM-DD)

### CORS Errors
- Use a local web server instead of the file:// protocol
- Or add CORS support to the backend (see Debugging section)
