# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  config.vm.provider "virtualbox" do |vb|
    vb.memory = "2048"
    vb.cpus = 2
  end

  GITHUB_REPO = "https://github.com/KhrystynaKonepud/Medical_Project.git"
  BRANCH = "deploy-branch"

  # ============================================
  # UBUNTU 22.04 LTS
  # ============================================
  config.vm.define "ubuntu", primary: true do |ubuntu|
    ubuntu.vm.box = "ubuntu/jammy64"
    ubuntu.vm.hostname = "medical-ubuntu"
    ubuntu.vm.network "forwarded_port", guest: 5000, host: 5010
    
    ubuntu.vm.provision "shell", inline: <<-SHELL
      set -e
      echo "Deploying on Ubuntu 22.04..."
      
      export DEBIAN_FRONTEND=noninteractive
      apt-get update -qq
      
      # .NET SDK
      wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
      dpkg -i packages-microsoft-prod.deb
      apt-get update -qq
      apt-get install -y dotnet-sdk-8.0 git curl
      
      # Node.js
      curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
      apt-get install -y nodejs
      
      # Clone & Build
      cd /home/vagrant
      git clone -b #{BRANCH} #{GITHUB_REPO}
      cd Medical_Project
      
      # ВАЖЛИВО: Видаляємо папку tests (вона використовує .NET 9.0)
      echo "Removing tests folder (uses .NET 9.0)..."
      rm -rf tests
      
      # Тепер збірка працюватиме
      dotnet restore
      dotnet build -c Release
      
      if [ -d "ClientApp" ]; then
        cd ClientApp
        npm install --silent
        npm run build
        cd ..
      fi
      
      # Startup script
      cat > /home/vagrant/run-app.sh << 'EOF'
#!/bin/bash
cd /home/vagrant/Medical_Project
dotnet run --urls "http://0.0.0.0:5000"
EOF
      chmod +x /home/vagrant/run-app.sh
      
      echo "SUCCESS: Ubuntu deployment completed!"
      echo "Access: http://localhost:5010"
    SHELL
  end

  # ============================================
  # DEBIAN 12
  # ============================================
  config.vm.define "debian" do |debian|
    debian.vm.box = "debian/bookworm64"
    debian.vm.hostname = "medical-debian"
    debian.vm.network "forwarded_port", guest: 5000, host: 5011
    
    debian.vm.provision "shell", inline: <<-SHELL
      set -e
      echo "Deploying on Debian 12..."
      
      export DEBIAN_FRONTEND=noninteractive
      apt-get update -qq
      
      # .NET SDK
      wget -q https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb
      dpkg -i packages-microsoft-prod.deb
      apt-get update -qq
      apt-get install -y dotnet-sdk-8.0 git curl
      
      # Node.js
      curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
      apt-get install -y nodejs
      
      # Clone & Build
      cd /home/vagrant
      git clone -b #{BRANCH} #{GITHUB_REPO}
      cd Medical_Project
      
      # ВАЖЛИВО: Видаляємо папку tests (вона використовує .NET 9.0)
      echo "Removing tests folder (uses .NET 9.0)..."
      rm -rf tests
      
      # Тепер збірка працюватиме
      dotnet restore
      dotnet build -c Release
      
      if [ -d "ClientApp" ]; then
        cd ClientApp
        npm install --silent
        npm run build
        cd ..
      fi
      
      # Startup script
      cat > /home/vagrant/run-app.sh << 'EOF'
#!/bin/bash
cd /home/vagrant/Medical_Project
dotnet run --urls "http://0.0.0.0:5000"
EOF
      chmod +x /home/vagrant/run-app.sh
      
      echo "SUCCESS: Debian deployment completed!"
      echo "Access: http://localhost:5011"
    SHELL
  end
end