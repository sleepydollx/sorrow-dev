import requests

response = requests.get("https://api.github.com/users/sleepydollx")

data = response.json()

print("Nama:", data["name"])
print("Lokasi:", data["location"])
print("Jumlah repo publik:", data["public_repos"])