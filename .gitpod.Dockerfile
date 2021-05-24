FROM deniswsrosa/couchbase7.0.beta-gitpod

#extend for .net
USER root

#example on how to extend the image to install .NET 
RUN wget https://packages.microsoft.com/config/ubuntu/20.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
 dpkg -i packages-microsoft-prod.deb \ 
 apt-get update && export DEBIAN_FRONTEND=noninteractive \ 
 apt-get install -y apt-transport-https \
 apt-get update \
 apt-get install -y dotnet-sdk-5.0
