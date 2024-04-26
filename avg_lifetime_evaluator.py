import math
import os
from evaluator import Evaluator


class WeightedAverageLifeTime(Evaluator):

    def __init__(self, log_file, **kwargs):
        self.discount_factor = kwargs.get("discount_factor", 0.99)
        self.log_file = log_file
        pass

    def process_log_file(self, filepath, discount_factor):
        wolf_sum_value, wolf_count = 0.0, 0.0
        deer_sum_value, deer_count = 0.0, 0.0

        with open(filepath, "r") as file:
            log_entries = file.readlines()

        for entry in log_entries:
            parts = entry.strip().split(" - ")
            animal = parts[0].split()[0]  # Animal type
            time_alive = float(parts[1])  # Time alive extracted

            if animal == "Wolf":
                wolf_sum_value = wolf_sum_value * discount_factor + time_alive * (
                    1 - discount_factor
                )
                wolf_count = wolf_count * discount_factor + (1 - discount_factor)
            elif animal == "Deer":
                deer_sum_value = deer_sum_value * discount_factor + time_alive * (
                    1 - discount_factor
                )
                deer_count = deer_count * discount_factor + (1 - discount_factor)

        wolf_incremental_average = wolf_sum_value / wolf_count if wolf_count != 0 else 0
        deer_incremental_average = deer_sum_value / deer_count if deer_count != 0 else 0

        print(wolf_incremental_average, deer_incremental_average)
        return wolf_incremental_average, deer_incremental_average

    def evaluate(self) -> float:

        wolf_avg, deer_avg = self.process_log_file(self.log_file, self.discount_factor)

        balance_metric = deer_avg + wolf_avg - abs(deer_avg - wolf_avg)
        return balance_metric


def evaluate(log_file, **kwargs):
    if not os.path.exists(log_file):
            print("No data output file found.")
            return -math.inf
    evaluator = WeightedAverageLifeTime(log_file, **kwargs)
    return evaluator.evaluate()
