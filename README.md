# QDeploy

QDeploy (Quick and Dirty Deployment tool) is an application that helps you deploy applications to multiple remote servers. It is made up of a Client GUI and a Server written in WCF that runs as a Windows Service on TCP Port 6969.

##Client
Youu may download and build the Windows Client yourself, or else you may download this pre-compiled executable.

##Server

## Security
This application doesn't have any form of authentication or encryption. You should run this server behind a firewall so it can be accessible only from your LAN. For remote deployments, do not open this application to the internet, but rather connect with a VPN; this way you will have the added security of encryption and authentication that VPN setups inherently offer.