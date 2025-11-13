#!/bin/bash

# AI Ideas API - Testing Script
# Replace {token} with actual JWT token and {apiUrl} with your API base URL

API_URL="http://localhost:5000"
TOKEN="your-jwt-token-here"
USER_ID="5"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}===== AI Ideas API Testing =====${NC}\n"

# 1. CREATE AI IDEA
echo -e "${GREEN}1. Creating AI Idea...${NC}"
curl -X POST "${API_URL}/api/aiideas" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Reusable Tote Bag",
    "tools": "Needle, thread, scissors, ruler",
    "steps": "1. Cut two pieces of fabric\n2. Place with right sides together\n3. Sew three sides\n4. Turn right side out\n5. Sew handles\n6. Fold and press",
    "safety": "Use sharp scissors carefully. Mind your fingers when cutting."
  }' \
  -w "\nStatus: %{http_code}\n\n"

# 2. CREATE SECOND AI IDEA
echo -e "${GREEN}2. Creating Second AI Idea...${NC}"
curl -X POST "${API_URL}/api/aiideas" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Cloth Napkins from Old Sheets",
    "tools": "Scissors, sewing machine, thread",
    "steps": "1. Cut 20x20 inch squares\n2. Fold edges inward\n3. Sew hems\n4. Press flat",
    "safety": "Keep hands away from sewing machine needle"
  }' \
  -w "\nStatus: %{http_code}\n\n"

# 3. GET SPECIFIC AI IDEA
echo -e "${GREEN}3. Getting Specific AI Idea (ID: 1)...${NC}"
curl -X GET "${API_URL}/api/aiideas/1" \
  -H "Authorization: Bearer ${TOKEN}" \
  -w "\nStatus: %{http_code}\n\n"

# 4. LIST USER'S AI IDEAS
echo -e "${GREEN}4. Listing User's AI Ideas (Paginated)...${NC}"
curl -X GET "${API_URL}/api/aiideas/user/${USER_ID}?skip=0&take=10" \
  -H "Authorization: Bearer ${TOKEN}" \
  -w "\nStatus: %{http_code}\n\n"

# 5. UPDATE AI IDEA
echo -e "${GREEN}5. Updating AI Idea (ID: 1)...${NC}"
curl -X PUT "${API_URL}/api/aiideas/1" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Premium Reusable Tote Bag",
    "tools": "Needle, thread, scissors, ruler, measuring tape",
    "steps": "1. Cut two pieces of heavy fabric\n2. Cut two handle strips\n3. Place with right sides together\n4. Sew three sides leaving top open\n5. Sew handles to inside\n6. Fold raw edges inward\n7. Top stitch to close\n8. Press flat",
    "safety": "Use sharp scissors carefully. Mind your fingers when cutting. Watch for needle when sewing."
  }' \
  -w "\nStatus: %{http_code}\n\n"

# 6. CREATE PROJECT FROM AI IDEA
echo -e "${GREEN}6. Creating Project from AI Idea...${NC}"
curl -X POST "${API_URL}/api/projects" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My Tote Bag Project",
    "description": "Building a reusable tote bag from sustainable materials",
    "ideaId": null,
    "aiIdeaId": 1
  }' \
  -w "\nStatus: %{http_code}\n\n"

# 7. GET PROJECT WITH AI IDEA
echo -e "${GREEN}7. Getting Project (ID: 1) with AI Idea...${NC}"
curl -X GET "${API_URL}/api/projects/1" \
  -H "Authorization: Bearer ${TOKEN}" \
  -w "\nStatus: %{http_code}\n\n"

# 8. DELETE AI IDEA
echo -e "${GREEN}8. Deleting AI Idea (ID: 2)...${NC}"
curl -X DELETE "${API_URL}/api/aiideas/2" \
  -H "Authorization: Bearer ${TOKEN}" \
  -w "\nStatus: %{http_code}\n\n"

# 9. TRY TO GET DELETED IDEA (SHOULD FAIL)
echo -e "${GREEN}9. Trying to Get Deleted Idea (ID: 2)...${NC}"
curl -X GET "${API_URL}/api/aiideas/2" \
  -H "Authorization: Bearer ${TOKEN}" \
  -w "\nStatus: %{http_code}\n\n"

# 10. LIST IDEAS AFTER DELETION
echo -e "${GREEN}10. Listing Ideas After Deletion...${NC}"
curl -X GET "${API_URL}/api/aiideas/user/${USER_ID}?skip=0&take=10" \
  -H "Authorization: Bearer ${TOKEN}" \
  -w "\nStatus: %{http_code}\n\n"

echo -e "${YELLOW}===== Testing Complete =====${NC}"
