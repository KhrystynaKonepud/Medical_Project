# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  config.vm.provider "virtualbox" do |vb|
    vb.memory = "2048"
    vb.cpus = 2
  end

  GITHUB_REPO = "https://github.com/KhrystynaKonepud/Medical_Project.git"
  BRANCH = "deploy-branch"

  config.vm.define "ubuntu", primary: true do |ubuntu|
    ubuntu.vm.box = "ubuntu/jammy64"
    ubuntu.vm.hostname = "medical-ubuntu"
    ubuntu.vm.network "forwarded_port", guest: 5000, host: 5010
    
    ubuntu.vm.provision "shell", inline: <<-SHELL
      set -e
      echo "Deploying on Ubuntu 22.04..."
      
      export DEBIAN_FRONTEND=noninteractive
      apt-get update -qq
      
      wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
      dpkg -i packages-microsoft-prod.deb
      apt-get update -qq
      apt-get install -y dotnet-sdk-8.0 git curl
      
      curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
      apt-get install -y nodejs
      
      cd /home/vagrant
      git clone -b #{BRANCH} #{GITHUB_REPO}
      cd Medical_Project
      
      echo "Removing tests folder..."
      rm -rf tests
      
      dotnet restore Medical_center.csproj
      dotnet build Medical_center.csproj -c Release
      
      if [ -d "ClientApp" ]; then
        cd ClientApp
        npm install --silent
        npm run build
        cd ..
      fi
      
      cat > /home/vagrant/run-app.sh << 'EOF'
#!/bin/bash
cd /home/vagrant/Medical_Project
dotnet run --project Medical_center.csproj --urls "http://0.0.0.0:5000"
EOF
      chmod +x /home/vagrant/run-app.sh
      
      echo "SUCCESS: Ubuntu deployment completed!"
      echo "Access: http://localhost:5010"
    SHELL
  end

  config.vm.define "debian" do |debian|
    debian.vm.box = "debian/bookworm64"
    debian.vm.hostname = "medical-debian"
    debian.vm.network "forwarded_port", guest: 5000, host: 5011
    
    debian.vm.provision "shell", inline: <<-SHELL
      set -e
      echo "Deploying on Debian 12..."
      
      export DEBIAN_FRONTEND=noninteractive
      apt-get update -qq
      
      wget -q https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb
      dpkg -i packages-microsoft-prod.deb
      apt-get update -qq
      apt-get install -y dotnet-sdk-8.0 git curl
      
      curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
      apt-get install -y nodejs
      
      cd /home/vagrant
      git clone -b #{BRANCH} #{GITHUB_REPO}
      cd Medical_Project
      
      echo "Removing tests folder..."
      rm -rf tests
      
      dotnet restore Medical_center.csproj
      dotnet build Medical_center.csproj -c Release
      
      if [ -d "ClientApp" ]; then
        cd ClientApp
        npm install --silent
        npm run build
        cd ..
      fi
      
      cat > /home/vagrant/run-app.sh << 'EOF'
#!/bin/bash
cd /home/vagrant/Medical_Project
dotnet run --project Medical_center.csproj --urls "http://0.0.0.0:5000"
EOF
      chmod +x /home/vagrant/run-app.sh
      
      echo "SUCCESS: Debian deployment completed!"
      echo "Access: http://localhost:5011"
    SHELL
  end
end
