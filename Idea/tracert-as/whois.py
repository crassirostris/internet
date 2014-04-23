import socket as sock
import re
import subprocess
import sys

OCTET_REGEX = r'(?:25[0-5]|2[0-4]\d|[0-1]?\d?\d)'
IP_REGEX = r'(%s\.){3}%s' % (OCTET_REGEX, OCTET_REGEX)

global PING_COMMAND, TTL_EXPIRED_REGEX
if sys.platform.startswith('win32'):
    PING_COMMAND = 'ping -i %d %s'
    TTL_EXPIRED_REGEX = r'Reply from (%s): TTL expired in transit\.' % IP_REGEX
else:
    PING_COMMAND = 'ping -t %d %s'
    TTL_EXPIRED_REGEX = r'From (%s) icmp_seq=\d+ Time to live exceeded' % IP_REGEX


def trace(addr):
    addresses = []
    current_ttl = 1
    while True:
        output = subprocess.getoutput(PING_COMMAND % (current_ttl, addr))
        current_addr = re.search(TTL_EXPIRED_REGEX, output)
        if not current_addr:
            break
        addresses.append(current_addr.group(1))
        print('Hop: %s' % addresses[-1])
        current_ttl += 1
    return addresses



class WhoisClient:
    rir_servers = [
        'whois.arin.net',
        'whois.ripe.net',
        'whois.afrinic.net',
        'whois.lacnic.net',
        'whois.apnic.net',
    ]

    def query_internal(self, addr, server):
        s = sock.socket(sock.AF_INET, sock.SOCK_STREAM)
        s.settimeout(3)
        try:
            s.connect((server, 43))
            s.recv(1024)
            s.sendall(addr.encode('utf-8') + b'\r\n')
            result = ""
            while True:
                temp = s.recv(16 * 1024).decode('utf-8')
                if not temp:
                    break
                result += temp
            #            if len(result) > 40:
            #                result = result[:37] + '...'
            return result
        except Exception as e:
            return 'Server %s rejected you with exception %s' % (server, e)
        finally:
            s.close()

    def query(self, addr):
        for serv in WhoisClient.rir_servers:
            info = self.query_internal(addr, serv)
            if info:
                print(info)

whois_client = WhoisClient()

while True:
    addr = input()
    route = trace(addr)
    for hop in route:
        print(whois_client.query(hop))
