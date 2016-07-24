FROM microsoft/dotnet:latest
COPY bin/Debug/netcoreapp1.0/publish/ /root/
WORKDIR /root
EXPOSE 5000/tcp
ENTRYPOINT dotnet /root/DotNetCoreChatBots.dll
