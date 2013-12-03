OODANI
======
DANI (Dynamic Artificial Non-Intelligence) - OO Assignment Dec 2013
Source code also available from https://github.com/carlmurdit/OODANI

*** INSTALLATION AND OPERATION ***

DANI Server
===========

Built with C#, Visual Studio 2012, .NET Framework 4.5
Scripts courtesy of http://www.dailyscript.com
No installation required, run DANI CSharp\DANI Server\bin\Debug\DANI Server.exe. 
You may talk to DANI by typing in the "Talk to DANI" textbox.
Initially no vocabulary is loaded so click File, Import and choose a script from the "DANI CSharp\Scripts" folder to get more interesting answers.
To allow clients (Desktop or Android) to connect, you may need to open port 11000 (or another port of your choice) in your firewall.
Use File > Start Socket Server to start accepting connections. Choose a network interface accessible to the client(s), note it (you'll need to enter it on the clients).

DANI Client
===========
Built with C#, Visual Studio 2012, .NET Framework 4.5
No installation required, run "DANI CSharp\DANI Client\bin\Debug\DANI Client.exe.
At startup enter the IP address and port selected on the server.

DANI Android
============
Built with Eclipse Keplar R1 and Android Development Kit 22.3 targetting Android 4.4.
As text-to-speech is used, a device may be preferable to the emulator.
Before installing, in settings, Security, check the option "Unknown Sources - Allow installation of apps from sources other than the Play Store". Transfer (or email) "DANI Android\DANI\bin\DANI.apk" to the Android phone and tap it to install.
Enable wifi. 
Run the DANI app. At startup it will prompt for the IP address and port selected on the server.
I couldn't get it to connect over dit-wifi (server & phone ips were on different subnets) so used ad-hoc (Connectify creates an ad-hoc network on Win8). 

