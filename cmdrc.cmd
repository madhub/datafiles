@echo off
REM https://superuser.com/questions/302194/automatically-executing-commands-when-a-command-prompt-is-opened/302553#302553

DOSKEY ls=dir
DOSKEY cd=cd $1$Tdir
DOSKEY clear=cls
DOSKEY k8s=kubectl
DOSKEY dk=docke
