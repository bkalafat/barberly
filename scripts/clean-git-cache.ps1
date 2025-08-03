# Git Cache Temizleme Komutları - Barberly

# Bu dosyaları çalıştır terminal'de:

# 1. Tüm build artifacts'ı git tracking'den çıkar
git rm -r --cached backend/src/*/obj/
git rm -r --cached backend/src/*/bin/
git rm --cached backend/src/*/*.cache

# 2. .gitignore'u yeniden uygula
git add .gitignore
git commit -m "fix: remove build artifacts from git tracking"

# 3. Gelecekte bu dosyalar ignore edilecek
git status

# Alternatif: Tek komutla tüm obj/ klasörlerini temizle
git rm -r --cached . 
git add .
git commit -m "fix: apply .gitignore to all files"
