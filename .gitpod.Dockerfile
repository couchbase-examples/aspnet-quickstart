FROM deniswsrosa/couchbase7.0.beta-gitpod

#extend for .net
USER root

#example on how to extend the image to install .NET 
RUN sudo apt-get update && export DEBIAN_FRONTEND=noninteractive \ 
 sudo apt-get install -y wget \
 sudo wget https://packages.microsoft.com/config/ubuntu/20.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
 sudo dpkg -i packages-microsoft-prod.deb \ 
 sudo apt-get install -y apt-transport-https \
 sudo apt-get update \
 sudo apt-get install -y dotnet-sdk-5.0
