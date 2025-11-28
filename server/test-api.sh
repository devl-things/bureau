#!/bin/bash

# Test script za Niles Chores API
# API se pokreće na http://localhost:5032

API_BASE="http://localhost:5032/api"

echo "🧪 Testing Niles Chores API"
echo "============================"
echo ""

# Test 1: Health check
echo "1️⃣  Testing health endpoint..."
curl -s "$API_BASE/../health" | jq '.' || echo "❌ Health check failed"
echo ""

# Test 2: Get prioritized chores for today
echo "2️⃣  Testing GET /api/chores (today's chores)..."
TODAY=$(date +%Y-%m-%d)
curl -s "$API_BASE/chores?date=$TODAY" | jq '.' || echo "❌ Failed to get chores"
echo ""

# Test 3: Get prioritized chores for specific date
echo "3️⃣  Testing GET /api/chores (specific date)..."
curl -s "$API_BASE/chores?date=2025-11-28" | jq '.' || echo "❌ Failed to get chores for date"
echo ""

# Test 4: Submit chores completion
echo "4️⃣  Testing POST /api/chores/submit..."
curl -s -X POST "$API_BASE/chores/submit" \
  -H "Content-Type: application/json" \
  -d '{
    "date": "'"$TODAY"'",
    "duration": 45,
    "note": "Test submission",
    "completedChoreIds": ["101", "102"]
  }' | jq '.' || echo "❌ Failed to submit chores"
echo ""

# Test 5: Get housekeeping logs
echo "5️⃣  Testing GET /api/housekeeping..."
curl -s "$API_BASE/housekeeping" | jq '.' || echo "❌ Failed to get housekeeping logs"
echo ""

# Test 6: Get prioritized chores via alternative endpoint
echo "6️⃣  Testing GET /api/housekeeping/prioritized-chores..."
curl -s "$API_BASE/housekeeping/prioritized-chores?date=$TODAY" | jq '.' || echo "❌ Failed to get prioritized chores"
echo ""

# Test 7: Get all chores (admin)
echo "7️⃣  Testing GET /api/chores/all..."
curl -s "$API_BASE/chores/all" | jq '.' || echo "❌ Failed to get all chores"
echo ""

# Test 8: Create a new chore
echo "8️⃣  Testing POST /api/chores (create)..."
CREATE_RESPONSE=$(curl -s -X POST "$API_BASE/chores" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Chore",
    "description": "Test description",
    "type": "Maintenance",
    "repeatEveryWeeks": 2
  }')
echo "$CREATE_RESPONSE" | jq '.' || echo "❌ Failed to create chore"
CHORE_ID=$(echo "$CREATE_RESPONSE" | jq -r '.id // empty')
echo ""

# Test 9: Get single chore
if [ -n "$CHORE_ID" ]; then
    echo "9️⃣  Testing GET /api/chores/$CHORE_ID..."
    curl -s "$API_BASE/chores/$CHORE_ID" | jq '.' || echo "❌ Failed to get chore"
    echo ""
fi

# Test 10: Update chore
if [ -n "$CHORE_ID" ]; then
    echo "🔟 Testing PATCH /api/chores/$CHORE_ID (update)..."
    curl -s -X PATCH "$API_BASE/chores/$CHORE_ID" \
      -H "Content-Type: application/json" \
      -d '{
        "title": "Updated Test Chore",
        "description": "Updated description",
        "type": "Extra",
        "repeatEveryWeeks": 3
      }' | jq '.' || echo "❌ Failed to update chore"
    echo ""
fi

# Test 11: Mark chore as important
if [ -n "$CHORE_ID" ]; then
    echo "1️⃣1️⃣  Testing POST /api/chores/$CHORE_ID/important..."
    curl -s -X POST "$API_BASE/chores/$CHORE_ID/important" \
      -H "Content-Type: application/json" \
      -d '{
        "date": "2025-12-15",
        "description": "This is an important chore that needs attention"
      }' | jq '.' || echo "❌ Failed to mark chore as important"
    echo ""
fi

# Test 12: Delete chore
if [ -n "$CHORE_ID" ]; then
    echo "1️⃣2️⃣  Testing DELETE /api/chores/$CHORE_ID..."
    curl -s -X DELETE "$API_BASE/chores/$CHORE_ID" -w "\nHTTP Status: %{http_code}\n" || echo "❌ Failed to delete chore"
    echo ""
fi

echo "✅ Testing complete!"
echo ""
echo "💡 Tip: Make sure API is running on http://localhost:5032"
echo "   Start it with: cd server/src/Niles.Chores.Api && dotnet run"

