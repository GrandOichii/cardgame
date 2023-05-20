import math
from termcolor import colored

def size_config(data: list, target: int, min_delegate, max_delegate) -> list[int]:
    per_one = target / len(data)
    a = []

    for item in data:
        a += [[min_delegate(item), per_one, max_delegate(item)]]

    while True:
        # find closest to min
        closest = None
        min_sum = 0
        good = []
        bad = []
        for i, item in enumerate(a):
            diff = item[0] - item[1]
            if diff > 0:
                bad += [i]
                min_sum += diff
                if closest is None or diff < closest[1] - closest[0]:
                    closest = item
                continue
            good += [i]
        
        if closest is None:
            break

        # calculate distributions
        dis_total = min_sum / len(good)
        dis_closest = (closest[0] - closest[1]) / len(good)

        sub = min(dis_total, dis_closest)
        for i in good:
            a[i][1] -= sub
        sub = min(min_sum / len(bad), (closest[0] - closest[1]) / len(bad))
        for i in bad:
            a[i][1] += sub
            
        if dis_total < dis_closest:
            break

        if dis_total < 0.0001:
            break

    while True:
        # find the closest
        closest = None
        closest_diff = 0
        good = []
        bad = []
        max_sum = 0
        for i, item in enumerate(a):
            if item[2] == -1:
                good += [i]
                continue
            diff = item[1] - item[2]
            if diff > 0:
                bad += [i]
                max_sum += diff
                if closest is None or diff > closest_diff:
                    closest = item
                    closest_diff = diff
                continue
            good += [i]

        if closest is None:
            break
            
        # calculate distributions
        dis_total = max_sum / len(good)
        dis_closest = (closest[1] - closest[2]) / len(good)
        sub = min(dis_total, dis_closest)
        for i in good:
            a[i][1] += sub

        sub = min(max_sum / len(bad), (closest[1] - closest[2]) / len(bad))
        for i in bad:
            a[i][1] -= sub
            
        if dis_total < dis_closest:
            break
    
        if dis_total < 0.0001:
            break

    # works more or less
    # TODO configure the thresh
    return [math.floor(i[1]) for i in a]
    # return [math.floor(i[1]) if (i[1] - 1) < 0.01 else math.ceil(i[1]) for i in a]

def tuple_size_config(data: list, target: int):
    return size_config(data, target, lambda o: o[0], lambda o: o[1])

test_suite = [
    {
        'data': [
            (0, -1),
            (0, -1),
            (0, -1)
        ],
        'target': 100,
        'expected': [100/3, 100/3, 100/3]
    },
    {
        'data': [
            (0, 20),
            (0, -1),
            (0, -1)
        ],
        'target': 100,
        'expected': [20, 40, 40]
    },
    {
        'data': [
            (0, 20),
            (0, -1),
            (0, 20)
        ],
        'target': 100,
        'expected': [20, 60, 20]
    },
    {
        'data': [
            (0, 20),
            (30, -1)
        ],
        'target': 100,
        'expected': [20, 80]
    },
    {
        'data': [
            (0, 20),
            (60, -1)
        ],
        'target': 100,
        'expected': [20, 80]
    },
    {
        'data': [
            (0, 10),
            (0, 27),
            (0, 40),
            (0, 40),
        ],
        'target': 100,
        'expected': [10, 27, 31.5, 31.5]
    },
    {
        'data': [
            (60, -1),
            (0, -1),
        ],
        'target': 100,
        'expected': [60, 40]
    },
    {
        'data': [
            (40, -1),
            (0, -1),
            (0, -1),
            (0, -1),
        ],
        'target': 100,
        'expected': [40, 20, 20, 20]
    },
    {
        'data': [
            (70, -1),
            (16, -1),
            (0, -1),
            (0, -1),
        ],
        'target': 100,
        'expected': [70, 16, 7, 7]
    },
    {
        'data': [
            (70, -1),
            (16, -1),
            (0, 4),
            (0, -1),
        ],
        'target': 100,
        'expected': [71, 17, 4, 8]
    }
]

def equal(arr1, arr2):
    if len(arr1) != len(arr2):
        raise Exception('INVALID SIZE COMPARISON')
    for i in range(len(arr1)):
        if not math.isclose(arr1[i], arr2[i]):
            return False
    return True

def run_tests():
    for i, test in enumerate(test_suite):
        data = test['data']
        target = test['target']
        expected = test['expected']

        result = tuple_size_config(data, target)
        if not equal(result, expected):
            print(colored(f'Test {i} failed', 'red'))
            print(f'Expected:\t{expected}')
            print(f'Got:\t\t{result}')
            continue

        print(colored(f'Test {i} passed', 'green'))

# run_tests()