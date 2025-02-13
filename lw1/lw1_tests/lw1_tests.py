import subprocess
import sys

script_path = "../lw1/lw1.py"

def run_triangle_definer(args):
    try:
        result = subprocess.run(
            ["python", script_path] + args,
            text=True,
            capture_output=True
        )

        return result.stdout.strip()
    
    except Exception as e:
        return "error " + e

def main():
    with open("test_cases.txt", 'r') as file:
        while True:
            test = file.readline().split()
            if not test:
                break

            test_case = test[:-2]
            output = run_triangle_definer(test_case)

            expected_output = ''
            for item in test[-2:]:
                expected_output += item
                if item != test[len(test) - 1]:
                    expected_output += ' '

            expected_output.strip()

            if expected_output == output:
                print("success")
            else:
                print("error")

    return 0

if __name__ == "__main__":
    sys.exit(main())