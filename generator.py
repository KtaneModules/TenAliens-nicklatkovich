from random import randint
import math


def cfmethod(A):
    m = 2**(len(hex(max(A))) * 4)
    count = [0] * m
    for a in A:
        count[a] += 1
    count[0] += 1

    owner = list(range(m))
    sets = [[i] for i in range(m)]

    total = 0
    for profit in reversed(range(m)):
        a = profit
        b = a ^ profit
        while a > b:
            if count[a] and count[b] and owner[a] != owner[b]:
                total += (count[a] + count[b] - 1) * profit
                count[a] = 1
                count[b] = 1

                small = owner[a]
                big = owner[b]
                if len(sets[small]) > len(sets[big]):
                    small, big = big, small
                for c in sets[small]:
                    owner[c] = big
                sets[big] += sets[small]
            a = (a - 1) & profit
            b = a ^ profit
    return total - sum(A)


class SaR:
    def __init__(self, A: list):
        self.unused_nodes = A.copy()
        self.used_nodes = []
        self.score = 0

    def transfer(self, a):
        assert a in self.unused_nodes
        self.unused_nodes.remove(a)
        self.used_nodes.append(a)

    def pull(self, a, b):
        assert a in self.used_nodes
        assert b in self.unused_nodes
        self.unused_nodes.remove(b)
        self.used_nodes.append(b)
        self.score += a


def twoToFive(sar: SaR):
    if 5 not in sar.unused_nodes: return True
    use = 3 + 5 * (sar.unused_nodes.count(2) - 1)
    not_use = 5 * sar.unused_nodes.count(2) + 2 * (sar.unused_nodes.count(5) - 1)
    return use >= not_use


def mymethod(A):
    sar = SaR(A)
    if 6 in sar.unused_nodes:
        sar.transfer(6)
        if 1 in sar.unused_nodes:
            while 1 in sar.unused_nodes: sar.pull(6, 1)
            while 6 in sar.unused_nodes: sar.pull(1, 6)
            if 4 in sar.unused_nodes:
                sar.pull(1, 4)
                while 3 in sar.unused_nodes: sar.pull(4, 3)
                if 2 in sar.unused_nodes:
                    sar.pull(4, 2)
                    while 5 in sar.unused_nodes: sar.pull(2, 5)
                    if 5 in sar.used_nodes:
                        while 2 in sar.unused_nodes: sar.pull(5, 2)
                    else:
                        while 2 in sar.unused_nodes: sar.pull(4, 2)
                while 4 in sar.unused_nodes:
                    if 3 in sar.used_nodes: sar.pull(3, 4)
                    elif 2 in sar.used_nodes: sar.pull(2, 4)
                    elif 1 in sar.used_nodes: sar.pull(1, 4)
                return sar.score
            if 2 in sar.unused_nodes:
                if twoToFive(sar):
                    sar.pull(1, 2)
                    while 5 in sar.unused_nodes: sar.pull(2, 5)
                    if 5 in sar.used_nodes:
                        while 2 in sar.unused_nodes: sar.pull(5, 2)
                    else:
                        while 2 in sar.unused_nodes: sar.pull(1, 2)
                    return sar.score
                sar.transfer(5)
                while 2 in sar.unused_nodes: sar.pull(5, 2)
                while 5 in sar.unused_nodes: sar.pull(2, 5)
            return sar.score
    if 5 in sar.unused_nodes and 2 in sar.unused_nodes:
        sar.transfer(5)
        while 2 in sar.unused_nodes: sar.pull(5, 2)
        while 5 in sar.unused_nodes: sar.pull(2, 5)
        if 4 not in sar.unused_nodes:
            while 1 in sar.unused_nodes: sar.pull(2, 1)
            return sar.score
        sar.pull(2, 4)
        while 3 in sar.unused_nodes: sar.pull(4, 3)
        while 1 in sar.unused_nodes: sar.pull(4, 1)
        if 3 in sar.used_nodes:
            while 4 in sar.unused_nodes: sar.pull(3, 4)
        else:
            while 4 in sar.unused_nodes: sar.pull(2, 4)
    if 4 in sar.unused_nodes:
        sar.transfer(4)
        while 3 in sar.unused_nodes: sar.pull(4, 3)
        while 2 in sar.unused_nodes: sar.pull(4, 2)
        while 1 in sar.unused_nodes: sar.pull(4, 1)
        while 4 in sar.unused_nodes:
            if 3 in sar.used_nodes: sar.pull(3, 4)
            elif 2 in sar.used_nodes: sar.pull(2, 4)
            elif 1 in sar.used_nodes: sar.pull(1, 4)
    return sar.score


for _ in range(1000):
    A = [randint(1, 6) for x in range(10)]
    cf = cfmethod(A)
    my = mymethod(A)
    if cf == my: continue
    print()
    print(A)
    print(cfmethod(A))
    print(mymethod(A))
    print()
