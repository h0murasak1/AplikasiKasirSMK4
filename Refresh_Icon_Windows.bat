@echo off
title Refresh Icon Cache Windows
echo Menghapus cache icon Windows yang lama agar icon aplikasi langsung terupdate...
taskkill /f /im explorer.exe >nul 2>&1
timeout /t 1 /nobreak >nul
del /f /q "%localappdata%\IconCache.db" >nul 2>&1
del /f /q "%localappdata%\Microsoft\Windows\Explorer\iconcache*" >nul 2>&1
start explorer.exe
echo Icon Cache Windows berhasil diperbarui!
echo Silakan buka kembali folder File Explorer.
