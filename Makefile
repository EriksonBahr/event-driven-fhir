up:
	docker-compose up -d

tye:
	tye run --dashboard

setup:
	dotnet tool install -g Microsoft.Tye --version "0.11.0-alpha.22111.1"