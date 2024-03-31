from collections import defaultdict
from datetime import timedelta
import numpy as np
from datetime import datetime
import matplotlib.pyplot as plt
import argparse


parser = argparse.ArgumentParser(description="Training script for ML Agents.")

parser.add_argument("--log", required=True, help="Path to logfile.")
args = parser.parse_args()

with open(args.log, "r") as f:
    log_entries = f.read()

log_entries = log_entries.split("\n")

# Parse log entries
parsed_data = []
for entry in log_entries:
    parts = entry.split(" - ")
    if len(parts) != 3:
        continue  # Skip invalid entries
    animal, cause = parts[0].split(" died: ")
    time_alive = float(parts[1])
    timestamp_str = parts[2].split(": ", 1)[1]
    timestamp = datetime.strptime(timestamp_str, "%m/%d/%Y %I:%M:%S %p")
    parsed_data.append((animal, cause, time_alive, timestamp))

# Define the interval duration
interval_duration = timedelta(minutes=3)

# Group data by intervals and animal type
interval_data = defaultdict(lambda: defaultdict(list))
start_time = min(entry[3] for entry in parsed_data)  # Find the earliest timestamp
end_time = max(entry[3] for entry in parsed_data)  # Find the latest timestamp

current_interval_start = start_time
while current_interval_start <= end_time:
    current_interval_end = current_interval_start + interval_duration
    for animal, cause, time_alive, timestamp in parsed_data:
        if current_interval_start <= timestamp < current_interval_end:
            interval_data[current_interval_start][animal].append(time_alive)
    current_interval_start = current_interval_end

# Calculate average time alive for each interval and animal type
average_times = defaultdict(dict)
for interval_start, data in interval_data.items():
    for animal, times in data.items():
        average_times[interval_start][animal] = np.mean(times)

# Preparing data for plotting
plot_data = {"Wolf": [], "Deer": []}
interval_starts = list(sorted(average_times.keys()))
for interval_start in interval_starts:
    for animal in ["Wolf", "Deer"]:
        if animal in average_times[interval_start]:
            plot_data[animal].append(average_times[interval_start][animal])
        else:
            plot_data[animal].append(0)  # No data for this interval

# Convert interval_starts to a format more friendly for plotting (minutes since start)
time_since_start = [
    (interval_start - start_time).total_seconds() / 60
    for interval_start in interval_starts
]

# Sliding window analysis
sliding_window_duration = timedelta(minutes=3)
slide_step = timedelta(minutes=1)  # Slide by 1 minute for each step

# Initialize data structures for sliding window analysis
sliding_window_data = {"Wolf": [], "Deer": []}
sliding_window_starts = []

current_slide_start = start_time
while current_slide_start + sliding_window_duration <= end_time + slide_step:
    current_slide_end = current_slide_start + sliding_window_duration
    # Temporarily store times alive for animals in the current window
    temp_times_alive = defaultdict(list)
    for animal, cause, time_alive, timestamp in parsed_data:
        if current_slide_start <= timestamp < current_slide_end:
            temp_times_alive[animal].append(time_alive)
    # Calculate and store the average for the current window, if data is available
    for animal in ["Wolf", "Deer"]:
        if temp_times_alive[animal]:
            sliding_window_data[animal].append(np.mean(temp_times_alive[animal]))
        else:
            # If no data for this window, append 0 or keep the last value?
            sliding_window_data[animal].append(0)  # Append 0 for consistency
    sliding_window_starts.append(current_slide_start)
    current_slide_start += slide_step

# Convert sliding_window_starts to minutes since start for plotting
sliding_time_since_start = [
    (time_point - start_time).total_seconds() / 60
    for time_point in sliding_window_starts
]

# Plotting both separate intervals and sliding window results
plt.figure(figsize=(14, 8))

# Separate Intervals Plot
for animal, times in plot_data.items():
    plt.plot(
        time_since_start,
        times,
        label=f"{animal} Avg Time Alive (Separate Intervals)",
        marker="o",
    )

# Sliding Window Plot
for animal, times in sliding_window_data.items():
    plt.plot(
        sliding_time_since_start,
        times,
        label=f"{animal} Avg Time Alive (Sliding Window)",
        linestyle="--",
    )

plt.xlabel("Minutes since start")
plt.ylabel("Average Time Alive (hours)")
plt.title(
    "Average Time Alive by Animal Type Over Time (Separate Intervals vs. Sliding Window)"
)
plt.legend()
plt.show()
