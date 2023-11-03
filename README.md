# DualPCControl
DPС is a program that allows you to control two computers simultaneously on a **local network**. This program is needed for those who want to use their second PC or laptop as a second monitor, but at the same time share the use of PC resources. For example, your main PC is loaded with heavy software (a game or Visual Studio with a virtual machine) and you don't want to load this PC even more to open the browser. In this case, my program will help you.

![Client](https://github.com/VityazevEgor/DualPCControl/assets/55443477/ba6d9a27-5909-46a2-aaec-34227468fe93)
![Server](https://github.com/VityazevEgor/DualPCControl/assets/55443477/dd64565b-6edd-4baa-bc82-cd89f4e2af1d)

## Features
- Full mouse emulation (mouse button pressing, mouse wheel, mouse movement)
- Full keyboard emulation with support for keyboard shortcuts such as alt+c and others
- Syncing keyboard layouts
- Clipboard synchronization (text only)
- The ability to select any hotkey to launch the overlay (control over the second PC)
- The ability to add a client and a server to the startup (they will automatically connect to each other when the PC starts)

## How to start?
1. Download archive DPC_Server on PC, from which you want to control the second pc, and unzip it.
2. Launch **DPC_Server.exe** as admin. If the program asks you to install dotnet, then do it.
3. Select the button to activate the overlay (click on the text field, then press on the key you want to use), change the port that the program will listen to and enable autorun (optional).
4. Click on the "Run Server" button.
5. Run the console as an administrator and find out your local IP using the command **ipconfig**.
6. Download archive DPC_Client on your second PC and unzip it.
7. Launch **DPC_Client.exe** as admin. Enter the local IP address of the main computer, the server port, enable startup (optional) and click "connect".
8. Everything is ready. To start controlling the second PC from the main PC, press on the button that you selected at stage 3. If everything went well, then an inscription should appear in the upper-left corner of the screen, informing you that you control the second PC. To stop controlling the second PC, press on the button you selected again.

## Video demonstrating the operation of the program
[https://youtu.be/XqVBTOaltH0](https://youtu.be/XqVBTOaltH0)
