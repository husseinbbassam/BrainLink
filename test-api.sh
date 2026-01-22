#!/bin/bash

# Test script for BrainLink API
# This script demonstrates the ingestion and search functionality

API_URL="http://localhost:5000"

echo "======================================"
echo "BrainLink API Test Script"
echo "======================================"
echo ""

# Test 1: Ingest documents
echo "Test 1: Ingesting sample documents..."
echo ""

echo "Ingesting document 1: AI and Machine Learning"
curl -X POST "$API_URL/ingest" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Artificial intelligence and machine learning are transforming the technology industry. Deep learning models can now perform complex tasks like image recognition, natural language processing, and autonomous driving. Neural networks have become increasingly sophisticated, allowing computers to learn from vast amounts of data.",
    "fileName": "ai-ml-intro.txt",
    "metadata": {
      "topic": "AI",
      "author": "Tech Writer",
      "category": "Technology"
    }
  }'
echo ""
echo ""

echo "Ingesting document 2: Cloud Computing"
curl -X POST "$API_URL/ingest" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Cloud computing has revolutionized how businesses manage their IT infrastructure. Services like AWS, Azure, and Google Cloud provide scalable computing resources on demand. Companies can now deploy applications globally without managing physical servers.",
    "fileName": "cloud-computing.txt",
    "metadata": {
      "topic": "Cloud",
      "author": "Cloud Expert",
      "category": "Infrastructure"
    }
  }'
echo ""
echo ""

echo "Ingesting document 3: Cybersecurity"
curl -X POST "$API_URL/ingest" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Cybersecurity is critical in todays digital world. Organizations must protect their systems from various threats including malware, phishing attacks, and data breaches. Encryption, firewalls, and multi-factor authentication are essential security measures.",
    "fileName": "cybersecurity.txt",
    "metadata": {
      "topic": "Security",
      "author": "Security Expert",
      "category": "Security"
    }
  }'
echo ""
echo ""

sleep 2

# Test 2: Semantic Search
echo "======================================"
echo "Test 2: Semantic Search"
echo "======================================"
echo ""

echo "Query 1: 'deep learning neural networks'"
curl -s "$API_URL/search?query=deep%20learning%20neural%20networks&limit=3" | jq '.'
echo ""
echo ""

echo "Query 2: 'cloud infrastructure scalability'"
curl -s "$API_URL/search?query=cloud%20infrastructure%20scalability&limit=3" | jq '.'
echo ""
echo ""

echo "Query 3: 'data protection and encryption'"
curl -s "$API_URL/search?query=data%20protection%20and%20encryption&limit=3" | jq '.'
echo ""
echo ""

# Test 3: Hybrid Search
echo "======================================"
echo "Test 3: Hybrid Search (Vector + Keyword)"
echo "======================================"
echo ""

echo "Query: 'machine learning'"
curl -s "$API_URL/search/hybrid?query=machine%20learning&limit=3" | jq '.'
echo ""
echo ""

echo "======================================"
echo "Tests completed!"
echo "======================================"
