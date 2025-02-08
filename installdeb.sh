#!/usr/bin/env bash

sudo dpkg --add-architecture i386
sudo apt-get update && sudo DEBIAN_FRONTEND=noninteractive apt install -y ca-certificates curl gnupg wine64 wine32
sudo rm /etc/apt/keyrings/nodesource.gpg
curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | sudo gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg
NODE_MAJOR=22
echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" | sudo tee /etc/apt/sources.list.d/nodesource.list
sudo apt update && sudo DEBIAN_FRONTEND=noninteractive apt install nodejs -y
sudo npm install -g npm@11.1.0
sudo npm install -g yarn
node -v
npm -v
yarn -v
winecfg
