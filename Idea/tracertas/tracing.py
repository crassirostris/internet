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