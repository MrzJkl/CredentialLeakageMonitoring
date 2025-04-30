import csv
import sys
import random
from faker import Faker

fake = Faker()

custom_domains = [
    "gmail.com", "yahoo.com", "outlook.com", "hotmail.com",
    "web.de", "gmx.de", "icloud.com", "aol.com",
    "mail.com", "protonmail.com", "t-online.de",
    "example.org", "mydomain.net", "company.co",
    "devmail.io", "techhub.ai", "demoapp.cloud",
    "notarealmail.com", "service-mail.com", "mailbox.org",
    "student.edu", "enterprise.biz", "coolname.app",
    "fastmail.net", "randomcorp.com", "digitalmail.space",
    "testlabs.org", "netbox.tech", "datenpost.de"
]

def generate_realistic_email():
    local_part = "fake+" + fake.user_name()
    domain = random.choice(custom_domains)
    return f"{local_part}@{domain}"

def generate_realistic_password():
    styles = [
        lambda: fake.word() + str(random.randint(1, 9999)),
        lambda: fake.first_name().lower() + str(random.randint(10, 99)),
        lambda: fake.color_name().capitalize() + str(random.randint(100, 999)),
        lambda: fake.month_name() + str(random.randint(1900, 2025)),
        lambda: fake.word().capitalize() + "!" + str(random.randint(10, 99)),
        lambda: fake.password(length=random.randint(6, 12), special_chars=False),
        lambda: fake.password(length=random.randint(8, 16), special_chars=True),
    ]
    return random.choice(styles)()

def generate_fake_data(count, filename="fake_users.csv"):
    with open(filename, mode="w", newline="") as csvfile:
        writer = csv.writer(csvfile)
        for _ in range(count):
            email = generate_realistic_email()
            password = generate_realistic_password()
            writer.writerow([email, password])
    print(f"{count} Datensätze wurden mit 'fake+' E-Mails und realistischen Passwörtern in '{filename}' gespeichert.")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Bitte gib die Anzahl der zu generierenden Datensätze als Parameter an.")
        sys.exit(1)

    try:
        number_of_users = int(sys.argv[1])
        generate_fake_data(number_of_users)
    except ValueError:
        print("Ungültige Zahl. Bitte gib eine ganze Zahl als Parameter ein.")
        sys.exit(1)
