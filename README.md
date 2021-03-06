# QDeploy

QDeploy (Quick and Dirty Deployment tool) is an application that helps you deploy applications to multiple remote servers. It is made up of a Client GUI and a Server written in WCF that runs as a Windows Service on TCP Port 6969.

QDeploy is intelligent enough to compare the destination files with the local files and find out what differences there are and only update the different or missing files. **The source always wins**; this means that your local copy is assumed to be always correct. If there are any extra files at the destination they won't be removed.

![Screenshot](https://cloud.githubusercontent.com/assets/8025435/5494448/c2e8cdfc-86f4-11e4-90b3-add831a536a3.PNG)

##Client
You can clone this repository and build the Windows Client yourself, or else you may [download the pre-compiled executable](https://qdeploy.blob.core.windows.net/installer/QDeploy%20Client.application). 

The client allows you to save and open deployment profiles, so if you have some common profiles that you keep using it would be easier for you if you save them for future use.

> **Pro Tip:** If you right click one of your saved files, you can select it to always open with the QDeploy Client executable. This way you only have to double click the QDeploy Profile (`*.qdj`) to open your deployment window.

##Server
The server is a WCF Windows Service. To install this service on your server, compile this solution from source. 

Deploy the `Release` folder of the `ServerService` solution to your server in a permanent location. In the `Release` folder you will find `QDeploy Server.exe`, which is the Windows Service. You can name this directory whatever you want.

To install the Windows Service:

1. Open a Visual Studio command prompt
2. Browse to the QDeploy Server Directory
3. Run: `InstallUtil QDeploy Server.exe`
4. Run: `services.msc`
5. Find the QDeploy Server and start it (it will restart automatically after reboot)

> **Pro Tip:** If you need to uninstall the service, you can uninstall it by using the following command: `InstallUtil /u QDeploy Server.exe`

## Security
**This application doesn't have any form of authentication or encryption**. You should run this server behind a firewall so it can be accessible only from your LAN. **NEVER EVER EVER** make the server accessible to the public internet. For remote deployments, do not open this application to the internet, but rather connect with a VPN; this way you will have the added security of encryption and authentication that VPN setups inherently offer. 

> **TL;DR:** YOU NEED TO IMPLEMENT YOUR OWN AUTHENTICATION AND ENCRYPTION

## About File Comparison
File comparison is done by comparing the MD5 Hash of all the files in your local deployment with the MD5 hashes of all the files on the destination deployment. If the hash doesn't match it means that the file has changed and is then uploaded to the destination server. 

## Stability
Please test this application on your staging environment before using on your production systems. I have not tested this extensively so take caution.

## Issues
This application is still in it's very early stages. It is very likely that you will encounter some bugs or would like some new features. Unfortunately I don't have as much time as I would like to keep developing this application but if there are any features you need or any bugs you find, do not hesitate to [submit an Issue on GitHub](../../issues) and I will see if I can implement/fix them. 

On the other hand, if you are a developer and would like to contribute you can fork and submit a [pull request](../../pulls) and implement the issues you need.
