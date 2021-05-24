FROM deniswsrosa/couchbase7.0.beta-gitpod

#Simple example on how to extend the image to install Java and maven
RUN wget https://packages.microsoft.com/config/ubuntu/20.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
 dpkg -i packages-microsoft-prod.deb \
 sudo apt-get update; \
 sudo apt-get install -y apt-transport-https && \
 sudo apt-get update && \
 sudo apt-get install -y dotnet-sdk-5.0
