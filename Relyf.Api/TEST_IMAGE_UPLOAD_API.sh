#!/bin/bash

# Image Upload API Test Script
# Tests all upload methods and validates responses

set -e

BASE_URL="http://localhost:5000"
PROJECT_ID=1  # Change to existing project ID
ITEM_ID=1     # Change to existing item ID

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "?? Image Upload API Test Suite"
echo "=============================="
echo ""

# Create test image files
echo "?? Creating test files..."
echo "Test image data" > test.jpg
echo "Test PNG data" > test.png

# Create 1x1 red pixel PNG (base64)
BASE64_PNG="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg=="

echo ""
echo "=============================="
echo "TEST 1: File Upload (Multipart)"
echo "=============================="
echo "Uploading test.jpg via multipart/form-data..."

RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/images/upload" \
  -F "file=@test.jpg" \
  -F "ownerType=Project" \
  -F "ownerId=$PROJECT_ID" \
  -F "altText=Test upload via multipart")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "201" ]; then
  echo -e "${GREEN}? PASSED${NC} - Status: 201 Created"
  echo "Response: $BODY"
  IMAGE_ID=$(echo "$BODY" | grep -o '"imageId":[0-9]*' | grep -o '[0-9]*')
  IMAGE_URL=$(echo "$BODY" | grep -o '"/uploads/images/[^"]*"' | tr -d '"')
  echo "Image ID: $IMAGE_ID"
  echo "Image URL: $IMAGE_URL"
else
  echo -e "${RED}? FAILED${NC} - Expected 201, got $HTTP_CODE"
  echo "Response: $BODY"
fi

echo ""
echo "=============================="
echo "TEST 2: Base64 Upload"
echo "=============================="
echo "Uploading base64 PNG via JSON..."

RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/images" \
  -H "Content-Type: application/json" \
  -d "{
    \"ownerType\": \"Project\",
    \"ownerId\": $PROJECT_ID,
    \"source\": \"$BASE64_PNG\",
    \"altText\": \"Base64 test upload\"
  }")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "201" ]; then
  echo -e "${GREEN}? PASSED${NC} - Status: 201 Created"
  echo "Response: $BODY"
else
  echo -e "${RED}? FAILED${NC} - Expected 201, got $HTTP_CODE"
  echo "Response: $BODY"
fi

echo ""
echo "=============================="
echo "TEST 3: External URL Upload"
echo "=============================="
echo "Adding external URL via JSON..."

RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/images" \
  -H "Content-Type: application/json" \
  -d "{
    \"ownerType\": \"Project\",
    \"ownerId\": $PROJECT_ID,
    \"source\": \"url\",
    \"url\": \"https://via.placeholder.com/150\",
    \"altText\": \"External placeholder image\"
  }")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "201" ]; then
  echo -e "${GREEN}? PASSED${NC} - Status: 201 Created"
  echo "Response: $BODY"
else
  echo -e "${RED}? FAILED${NC} - Expected 201, got $HTTP_CODE"
  echo "Response: $BODY"
fi

echo ""
echo "=============================="
echo "TEST 4: List Images for Project"
echo "=============================="
echo "Getting all images for Project $PROJECT_ID..."

RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$BASE_URL/api/images/Project/$PROJECT_ID")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "200" ]; then
  echo -e "${GREEN}? PASSED${NC} - Status: 200 OK"
  echo "Response: $BODY"
  COUNT=$(echo "$BODY" | grep -o '"imageId"' | wc -l)
  echo "Total images: $COUNT"
else
  echo -e "${RED}? FAILED${NC} - Expected 200, got $HTTP_CODE"
  echo "Response: $BODY"
fi

echo ""
echo "=============================="
echo "TEST 5: Invalid File Type"
echo "=============================="
echo "Attempting to upload text file (should fail)..."

echo "Not an image" > test.txt

RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/images/upload" \
  -F "file=@test.txt" \
  -F "ownerType=Project" \
  -F "ownerId=$PROJECT_ID")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "400" ]; then
  echo -e "${GREEN}? PASSED${NC} - Correctly rejected with 400 Bad Request"
  echo "Error message: $BODY"
else
  echo -e "${RED}? FAILED${NC} - Expected 400, got $HTTP_CODE"
  echo "Response: $BODY"
fi

echo ""
echo "=============================="
echo "TEST 6: Missing File"
echo "=============================="
echo "Attempting upload without file (should fail)..."

RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/images/upload" \
  -F "ownerType=Project" \
  -F "ownerId=$PROJECT_ID")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "400" ]; then
  echo -e "${GREEN}? PASSED${NC} - Correctly rejected with 400 Bad Request"
  echo "Error message: $BODY"
else
  echo -e "${RED}? FAILED${NC} - Expected 400, got $HTTP_CODE"
  echo "Response: $BODY"
fi

echo ""
echo "=============================="
echo "TEST 7: Invalid Owner Type"
echo "=============================="
echo "Attempting upload with invalid ownerType (should fail)..."

RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/images/upload" \
  -F "file=@test.jpg" \
  -F "ownerType=InvalidType" \
  -F "ownerId=$PROJECT_ID")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "400" ]; then
  echo -e "${GREEN}? PASSED${NC} - Correctly rejected with 400 Bad Request"
  echo "Error message: $BODY"
else
  echo -e "${RED}? FAILED${NC} - Expected 400, got $HTTP_CODE"
  echo "Response: $BODY"
fi

echo ""
echo "=============================="
echo "TEST 8: Delete Image"
echo "=============================="

if [ -n "$IMAGE_ID" ]; then
  echo "Deleting image with ID $IMAGE_ID..."
  
  RESPONSE=$(curl -s -w "\n%{http_code}" -X DELETE "$BASE_URL/api/images/$IMAGE_ID")
  
  HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
  BODY=$(echo "$RESPONSE" | head -n-1)
  
  if [ "$HTTP_CODE" = "204" ]; then
    echo -e "${GREEN}? PASSED${NC} - Status: 204 No Content"
  else
    echo -e "${RED}? FAILED${NC} - Expected 204, got $HTTP_CODE"
    echo "Response: $BODY"
  fi
else
  echo -e "${YELLOW}??  SKIPPED${NC} - No image ID from previous tests"
fi

echo ""
echo "=============================="
echo "TEST 9: Delete Non-Existent Image"
echo "=============================="
echo "Attempting to delete non-existent image (should return 404)..."

RESPONSE=$(curl -s -w "\n%{http_code}" -X DELETE "$BASE_URL/api/images/999999")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "404" ]; then
  echo -e "${GREEN}? PASSED${NC} - Correctly returned 404 Not Found"
else
  echo -e "${RED}? FAILED${NC} - Expected 404, got $HTTP_CODE"
  echo "Response: $BODY"
fi

echo ""
echo "=============================="
echo "TEST 10: Cloudinary URL"
echo "=============================="
echo "Adding Cloudinary URL via JSON..."

RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/images" \
  -H "Content-Type: application/json" \
  -d "{
    \"ownerType\": \"Project\",
    \"ownerId\": $PROJECT_ID,
    \"source\": \"cloudinary\",
    \"url\": \"https://res.cloudinary.com/demo/image/upload/sample.jpg\",
    \"altText\": \"Cloudinary test image\"
  }")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "201" ]; then
  echo -e "${GREEN}? PASSED${NC} - Status: 201 Created"
  echo "Response: $BODY"
else
  echo -e "${RED}? FAILED${NC} - Expected 201, got $HTTP_CODE"
  echo "Response: $BODY"
fi

# Cleanup test files
echo ""
echo "?? Cleaning up test files..."
rm -f test.jpg test.png test.txt

echo ""
echo "=============================="
echo "? Test Suite Complete"
echo "=============================="
echo ""
echo "Summary:"
echo "- All core upload methods tested"
echo "- Validation checks verified"
echo "- Error handling confirmed"
echo "- CRUD operations validated"
echo ""
echo "Next steps:"
echo "1. Verify uploaded files in: {ProjectRoot}/uploads/images/"
echo "2. Test static file access: http://localhost:5000/uploads/images/{filename}"
echo "3. Integrate with frontend application"
echo ""
