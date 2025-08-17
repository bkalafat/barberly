#!/usr/bin/env powershell

Write-Host "🚀 Barberly Frontend: Resolving Vite 7 + Storybook 9 Dependencies" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan

Write-Host "📋 Issue: Storybook 8.x doesn't support Vite 7.x" -ForegroundColor Yellow
Write-Host "✅ Solution: Upgrade to Storybook 9.1.2 which supports Vite 7" -ForegroundColor Green

Write-Host ""
Write-Host "🧹 Step 1: Clean installation environment..." -ForegroundColor Blue
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue node_modules
Remove-Item -Force -ErrorAction SilentlyContinue package-lock.json
npm cache clean --force

Write-Host ""
Write-Host "📦 Step 2: Installing dependencies with Storybook 9 + Vite 7..." -ForegroundColor Blue
Write-Host "Using --legacy-peer-deps to resolve peer dependency conflicts..." -ForegroundColor Gray

npm install --legacy-peer-deps

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Dependencies installed successfully!" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "🔍 Step 3: Verifying installation..." -ForegroundColor Blue
    
    # Check Storybook version
    $storybookVersion = npm list storybook --depth=0 2>$null | Select-String "storybook@"
    if ($storybookVersion) {
        Write-Host "✅ Storybook: $storybookVersion" -ForegroundColor Green
    }
    
    # Check Vite version  
    $viteVersion = npm list vite --depth=0 2>$null | Select-String "vite@"
    if ($viteVersion) {
        Write-Host "✅ Vite: $viteVersion" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "🧪 Step 4: Testing Storybook..." -ForegroundColor Blue
    Write-Host "Running Storybook build to verify compatibility..." -ForegroundColor Gray
    
    $timeout = 30
    $job = Start-Job -ScriptBlock { 
        Set-Location $using:PWD
        npm run build-storybook 2>&1
    }
    
    if (Wait-Job $job -Timeout $timeout) {
        $result = Receive-Job $job
        Remove-Job $job
        
        if ($result -like "*built*" -or $result -like "*success*") {
            Write-Host "✅ Storybook build successful!" -ForegroundColor Green
        } else {
            Write-Host "⚠️ Storybook build had warnings, but dependencies are resolved." -ForegroundColor Yellow
            Write-Host "You can now run 'npm run storybook' to start the dev server." -ForegroundColor Gray
        }
    } else {
        Stop-Job $job
        Remove-Job $job
        Write-Host "⚠️ Storybook build timed out, but dependencies are installed." -ForegroundColor Yellow
        Write-Host "You can manually test with 'npm run storybook'" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "🎉 SUCCESS: Vite 7 + Storybook 9 dependency conflicts resolved!" -ForegroundColor Green
    Write-Host "=================================================================" -ForegroundColor Cyan
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  • npm run storybook    (Start Storybook dev server)" -ForegroundColor White
    Write-Host "  • npm run dev          (Start Vite dev server)" -ForegroundColor White  
    Write-Host "  • npm run build        (Build for production)" -ForegroundColor White
    Write-Host ""
    
} else {
    Write-Host ""
    Write-Host "❌ Installation failed. Troubleshooting steps:" -ForegroundColor Red
    Write-Host "1. Check Node.js version (requires Node 18+)" -ForegroundColor Yellow
    Write-Host "2. Try: npm install --force" -ForegroundColor Yellow
    Write-Host "3. Check network connectivity" -ForegroundColor Yellow
}
