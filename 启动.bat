@echo off
start ModifyBH.exe
node cert.js
anyproxy -i -r rule.js