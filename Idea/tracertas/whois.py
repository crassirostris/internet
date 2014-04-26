import socket as sock

class RirWhoisClient:
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
        for serv in RirWhoisClient.rir_servers:
            info = self.query_internal(addr, serv)
            if info:
                print(info)

whois_client = RirWhoisClient()

while True:
    addr = input()
    route = trace(addr)
    for hop in route:
        print(whois_client.query(hop))
