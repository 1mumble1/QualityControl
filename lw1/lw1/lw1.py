import sys

def try_float(value):
    try:
        float(value)
        return True
    except ValueError:
        return False

def define_type_of_triangle(a, b, c):
    triangle_type = "unknown error"
    if not try_float(a) or not try_float(b) or not try_float(c):
        return triangle_type

    a, b, c = float(a), float(b), float(c)
    if a <= 0 or b <= 0 or c <= 0:
        return triangle_type

    if (a < b + c) and (b < a + c) and (c < a + b):
        triangle_type = "simple triangle"
    else:
        return "not triangle"

    if a == b or a == c or b == c:
        triangle_type = "isosceles triangle"

    if a == b == c:
        triangle_type = "equilateral triangle"

    return triangle_type

def main():
    if len(sys.argv) != 4:
        print("unknown error")
        return 0

    a, b, c = sys.argv[1:]
    triangle_type = define_type_of_triangle(a, b, c);
    print(triangle_type)

    return 0

if __name__ == "__main__":
    sys.exit(main());
