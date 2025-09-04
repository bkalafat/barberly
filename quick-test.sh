#!/bin/bash
# Quick commit script to test the fix

echo "Staging changes..."
git add .

echo "Committing the test fix..."
git commit -m "fix: update GetAvailability test to use dynamic barber ID

- Replace hardcoded GUID with API query to find actual barber
- Test now queries /api/v1/barbers to get real seeded data
- This fixes the 404 'Barber not found' error in CI
- Consistent with other tests that query for actual IDs"

echo "Pushing to trigger GitHub Actions..."
git push

echo "✅ Changes pushed! Check GitHub Actions at:"
echo "https://github.com/bkalafat/barberly/actions"
