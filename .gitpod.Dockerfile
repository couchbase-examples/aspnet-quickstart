FROM couchbase:latest

RUN echo "* soft nproc 20000\n"\
"* hard nproc 20000\n"\
"* soft nofile 200000\n"\
"* hard nofile 200000\n" >> /etc/security/limits.conf

RUN apt-get update && export DEBIAN_FRONTEND=noninteractive &&\ 
 apt-get install -y wget &&\
 apt-get install -y software-properties-common sudo &&\
 apt-get update &&\
 wget https://packages.microsoft.com/config/ubuntu/20.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb &&\
 dpkg -i packages-microsoft-prod.deb &&\ 
 apt-add-repository https://packages.microsoft.com/ubuntu/20.04/prod &&\
 apt-get install -y apt-transport-https &&\
 apt-add-repository ppa:teejee2008/ppa\
 apt-get update\
 apt-get install ukuu\
 apt-get install -y dotnet-sdk-5.0 sudo 

 RUN addgroup --gid 33333 gitpod && \
     useradd --no-log-init --create-home --home-dir /home/gitpod --shell /bin/bash --uid 33333 --gid 33333 gitpod && \
     usermod -a -G gitpod,couchbase,sudo gitpod && \
     echo 'gitpod ALL=(ALL) NOPASSWD:ALL'>> /etc/sudoers

COPY startcb.sh /opt/couchbase/bin/startcb.sh
USER gitpod
