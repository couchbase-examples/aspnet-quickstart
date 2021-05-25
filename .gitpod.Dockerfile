FROM deniswsrosa/couchbase7.0.beta-gitpod

RUN apt-get update && export DEBIAN_FRONTEND=noninteractive &&\ 
 apt-get install -y wget &&\
 apt-get install -y apt-add-repository &&\ 
 wget https://packages.microsoft.com/config/ubuntu/20.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb &&\
 dpkg -i packages-microsoft-prod.deb &&\ 
 apt-add-repository https://packages.microsoft.com/ubuntu/20.04/prod &&\
 apt-get install -y apt-transport-https &&\
 apt-get install -y dotnet-sdk-5.0