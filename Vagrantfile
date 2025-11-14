# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  config.vm.provider "virtualbox" do |vb|
    vb.memory = "2048"
    vb.cpus = 2
  end

  GITHUB_REPO = "https://github.com/KhrystynaKonepud/Medical_Project.git"
  BRANCH = "deploy-branch" # Переконайтеся, що гілка deploy-branch існує і містить актуальний код

  # --- КОНФІГУРАЦІЯ UBUNTU ---
  config.vm.define "ubuntu", primary: true do |ubuntu|
    ubuntu.vm.box = "ubuntu/jammy64"
    ubuntu.vm.hostname = "medical-ubuntu"
    # Port 5000 (Guest) -> 5010 (Host)
    ubuntu.vm.network "forwarded_port", guest: 5000, host: 5010 
    
    ubuntu.vm.provision "shell", inline: <<-SHELL
      set -e
      echo "Deploying on Ubuntu 22.04..."
      
      export DEBIAN_FRONTEND=noninteractive
      apt-get update -qq
      
      # Встановлення .NET 8.0 SDK
      wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
      dpkg -i packages-microsoft-prod.deb
      apt-get update -qq
      apt-get install -y dotnet-sdk-8.0 git curl
      
      # Встановлення Node.js
      curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
      apt-get install -y nodejs
      
      cd /home/vagrant
      # Клонування репозиторію
      git clone -b #{BRANCH} #{GITHUB_REPO}
      cd Medical_Project
      
      echo "Removing tests folder..."
      rm -rf tests
      
      # --- Writing appsettings.Development.json ---
      # Генеруємо конфігураційний файл, використовуючи SQLite для простоти
      cat > appsettings.Development.json << 'EOT'
{
  "DatabaseProvider": "Sqlite", 
  "ConnectionStrings": {
    "SqlServerConnection": "Data Source=medicalcenter.db",
    "PostgresConnection": "Host=localhost;Database=MedicalCenterDb;Username=postgres;Password=yourpassword",
    "SqliteConnection": "Data Source=medicalcenter.db"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID_HERE",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET_HERE"
    }
  },
  "Auth": {
    "DisableExternalIdP": false
  },
  "Jwt": {
    "Issuer": "https://localhost:7263",
    "Audience": "medical_api",
    "Key": "g2QKc4wY6Pq9TzL1sVf8rU3yN0xB5hR7mD2kJ8aC4vW6eZ1tP9qL3nF7yS2uH5b"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.Authentication": "Information"
    }
  },
  "AllowedHosts": "*"
}
EOT

      echo "--- Preparing and Building .NET App ---"
      dotnet restore Medical_center.csproj
      
      if [ -d "ClientApp" ]; then
        cd ClientApp
        npm install --silent
        npm run build
        cd ..
      fi
      
      # Остаточна збірка після ClientApp
      dotnet build Medical_center.csproj -c Release

      # --- RUN SCRIPT CREATION ---
      cat > /home/vagrant/run-app.sh << 'EOF'
#!/bin/bash
cd /home/vagrant/Medical_Project
# Запуск у фоновому режимі (nohup)
nohup dotnet run --project Medical_center.csproj --urls "http://0.0.0.0:5000" &
EOF
      chmod +x /home/vagrant/run-app.sh
      
      echo "--- STARTING APPLICATION (Ubuntu) ---"
      /home/vagrant/run-app.sh
      
      echo "SUCCESS: Ubuntu deployment completed!"
      echo "Access: http://localhost:5010"
    SHELL
  end

  # --- КОНФІГУРАЦІЯ DEBIAN ---
  config.vm.define "debian" do |debian|
    debian.vm.box = "debian/bookworm64"
    debian.vm.hostname = "medical-debian"
    # Port 5000 (Guest) -> 5011 (Host)
    debian.vm.network "forwarded_port", guest: 5000, host: 5011
    
    debian.vm.provision "shell", inline: <<-SHELL
      set -e
      echo "Deploying on Debian 12..."
      
      export DEBIAN_FRONTEND=noninteractive
      apt-get update -qq
      
      # Встановлення .NET 8.0 SDK
      wget -q https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb
      dpkg -i packages-microsoft-prod.deb
      apt-get update -qq
      apt-get install -y dotnet-sdk-8.0 git curl
      
      # Встановлення Node.js
      curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
      apt-get install -y nodejs
      
      cd /home/vagrant
      # Клонування репозиторію
      git clone -b #{BRANCH} #{GITHUB_REPO}
      cd Medical_Project
      
      echo "Removing tests folder..."
      rm -rf tests
      
      # --- Writing appsettings.Development.json ---
      # Генеруємо конфігураційний файл, використовуючи SQLite для простоти
      cat > appsettings.Development.json << 'EOT'
{
  "DatabaseProvider": "Sqlite", 
  "ConnectionStrings": {
    "SqlServerConnection": "Data Source=medicalcenter.db",
    "PostgresConnection": "Host=localhost;Database=MedicalCenterDb;Username=postgres;Password=yourpassword",
    "SqliteConnection": "Data Source=medicalcenter.db"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID_HERE",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET_HERE"
    }
  },
  "Auth": {
    "DisableExternalIdP": false
  },
  "Jwt": {
    "Issuer": "https://localhost:7263",
    "Audience": "medical_api",
    "Key": "g2QKc4wY6Pq9TzL1sVf8rU3yN0xB5hR7mD2kJ8aC4vW6eZ1tP9qL3nF7yS2uH5b"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.Authentication": "Information"
    }
  },
  "AllowedHosts": "*"
}
EOT

      echo "--- Preparing and Building .NET App ---"
      dotnet restore Medical_center.csproj

      if [ -d "ClientApp" ]; then
        cd ClientApp
        npm install --silent
        npm run build
        cd ..
      fi
      
      # Остаточна збірка після ClientApp
      dotnet build Medical_center.csproj -c Release
      
      # --- RUN SCRIPT CREATION ---
      cat > /home/vagrant/run-app.sh << 'EOF'
#!/bin/bash
cd /home/vagrant/Medical_Project
# Запуск у фоновому режимі (nohup)
nohup dotnet run --project Medical_center.csproj --urls "http://0.0.0.0:5000" &
EOF
      chmod +x /home/vagrant/run-app.sh
      
      echo "--- STARTING APPLICATION (Debian) ---"
      /home/vagrant/run-app.sh
      
      echo "SUCCESS: Debian deployment completed!"
      echo "Access: http://localhost:5011"
    SHELL
  end
end
