import string
import struct
import re


def split_n(data, n):
    for i in range(int((len(data) + n - 1) / n)):
        yield data[i * n: i * n + n]

def to_base64(data):
    result = ''
    for chunk in split_n(data, 3):
        bytes_value = 0
        for i in range(3):
            bytes_value <<= 8
            if i < len(chunk):
                bytes_value |= chunk[i]
        for i in range(4):
            result += int2char((bytes_value >> ((3 - i) * 6)) & 0x3F)
    if len(data) % 3 == 1:
        result = result[:len(result) - 2] + '=='
    if len(data) % 3 == 2:
        result = result[:len(result) - 1] + '='
    return result


def char2int(ch):
    if ch in string.ascii_uppercase:
        return ord(ch) - ord('A')
    if ch in string.ascii_lowercase:
        return ord(ch) - ord('a') + 26
    if ch in string.digits:
        return ord(ch) - ord('0') + 52
    if ch == '+':
        return 62
    if ch == '/':
        return 63
    raise Exception('This is not Base64 string')

def int2char(x):
    if x < 26:
        return chr(x + ord('A'))
    elif x < 52:
        return chr(x - 26 + ord('a'))
    elif x < 62:
        return chr(x - 52 + ord('0'))
    elif x == 62:
        return '+'
    elif x == 63:
        return '/'

def num2bytes(x):
    return struct.pack("BBB", (x >> 16) & 0xFF, (x >> 8) & 0xFF, x & 0xFF)

def from_base64(s):
    if len(s) == 0:
        return b''
    if not re.match(r'[A-Za-z0-9]+={0,2}', s):
        raise Exception('Not valid Base64 string')
    data = b''
    cur_acc = 0
    for i in range(len(s)):
        if i > 0 and i % 4 == 0:
            data += num2bytes(cur_acc)
            cur_acc = 0
        if s[i] == '=':
            break
        cur_acc <<= 6
        cur_acc |= char2int(s[i])
    if s.endswith('=='):
        data += num2bytes(cur_acc << 12)
        data = data[:len(data) - 2]
    elif s.endswith('='):
        data += num2bytes(cur_acc << 6)
        data = data[:len(data) - 1]
    else:
        data += num2bytes(cur_acc)
    return data


while True:
    line = input()
    s = to_base64(line.encode('utf-8'))
    print(s)
    print(from_base64(s).decode('utf-8'))
data = b'Hello, world!'
s = to_base64(data)
print(s)
print(from_base64(s))
