import sys
import json
import os
from inference import dog_inference, script_dir

input_num = int(sys.stdin.readline().strip())
csv_file_path = os.path.join(script_dir, 'items.csv')
dog_inference.load_csv(csv_file_path)
dog_num, urls = dog_inference.classification(input_num)

dog_num = [str(num) for num in dog_num]
output_data = {'dog_num': dog_num, 'urls': urls}

output_json_path = os.path.join(script_dir, 'output_data.json')
with open(output_json_path, 'w', encoding='utf-8') as json_file:
    json.dump(output_data, json_file, ensure_ascii=False, indent=4)