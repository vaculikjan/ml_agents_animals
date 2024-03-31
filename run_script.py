import optuna
import argparse
import json
import subprocess
import yaml
import os
import re


class Trainer:

    def __init__(self, env, training_config, run_id):
        self.env = env
        self.env_config_template = training_config["env_config"]
        self.env_config = training_config["env_config_path"]
        self.nn_config_template = training_config["nn_config"]
        self.nn_config = training_config["nn_config_path"]
        self.output_path = os.path.join(training_config["log_directory_path"], run_id)
        self.run_id = run_id

        self.n_trials = training_config["number_of_trials"]
        self.n_jobs = training_config["parallel_trials"]

    def suggest_config_values(self, config, config_template, trial):
        for key, value in config_template.items():
            if isinstance(value, dict):
                if "type" in value and "val" in value:
                    if value["type"] == "range_int":
                        config[self.strip_animal_suffixes(key)] = trial.suggest_int(
                            key, value["val"][0], value["val"][1], step=value["step"]
                        )
                    elif value["type"] == "range_float":
                        config[self.strip_animal_suffixes(key)] = trial.suggest_float(
                            key, value["val"][0], value["val"][1], step=value["step"]
                        )
                    elif value["type"] == "list_float":
                        config[self.strip_animal_suffixes(key)] = [
                            trial.suggest_float(
                                f"{key}_{i}", value["val"][i][0], value["val"][i][1], step=value["step"]
                            )
                            for i in range(len(value["val"]))
                        ]
                    elif value["type"] == "categorical":
                        config[self.strip_animal_suffixes(key)] = (
                            trial.suggest_categorical(key, value["val"])
                        )
                    elif value["type"] == "bool":
                        config[self.strip_animal_suffixes(key)] = (
                            trial.suggest_categorical(key, value["val"])
                        )
                else:
                    self.suggest_config_values(config[key], value, trial)

    def strip_animal_suffixes(self, input_string):
        stripped_string = input_string.replace("_wolf", "").replace("_deer", "")
        return stripped_string

    def optimize(self):
        self.study = optuna.create_study(direction="maximize")
        self.study.optimize(self.objective, n_trials=self.n_trials, n_jobs=self.n_jobs)

        best_trial = {
            "number": self.study.best_trial.number,
            "params": self.study.best_trial.params,
        }
        with open(os.path.join(self.output_path, "best_trial.json"), "w") as f:
            json.dump(best_trial, f, indent=4)

        print("Complete!")

    def objective(self, trial):
        with open(self.env_config, "r") as f:
            env_config = json.load(f)
        self.suggest_config_values(env_config, self.env_config_template, trial)

        env_config_path = os.path.join(
            self.output_path, str(trial.number), f"env_config_{trial.number}.json"
        )
        os.makedirs(os.path.dirname(env_config_path), exist_ok=True)
        with open(env_config_path, "w") as f:
            json.dump(env_config, f, indent=4)

        with open(self.nn_config, "r") as f:
            nn_config = yaml.load(f, Loader=yaml.FullLoader)
        self.suggest_config_values(nn_config, self.nn_config_template, trial)

        nn_config_path = os.path.join(
            self.output_path, str(trial.number), f"nn_config_{trial.number}.yaml"
        )
        os.makedirs(os.path.dirname(nn_config_path), exist_ok=True)
        with open(nn_config_path, "w") as f:
            yaml.dump(nn_config, f, default_flow_style=False)

        run_command = [
            "mlagents-learn",
            nn_config_path,
            "--env=" + self.env,
            "--run-id=" + self.run_id + "_" + str(trial.number),
            "--no-graphics",
            "--force",
            "--base-port=" + str(5005 + trial.number),
            "--env-args",
            "-config",
            env_config_path,
        ]

        evaluation_score = self.run_training_script(run_command, trial.number)
        return -evaluation_score

    def run_training_script(self, run_command, trial_number):
        log_file_path = os.path.join(
            self.output_path, f"trial_{trial_number}_run_{trial_number}.log"
        )
        os.makedirs(os.path.dirname(log_file_path), exist_ok=True)

        with open(log_file_path, "w") as log_file:
            subprocess.run(run_command, stdout=log_file, stderr=log_file)

        evaluation_score = self.read_and_evaluate(self.output_path + f"/{trial_number}")
        return evaluation_score

    def read_and_evaluate(self, run_directory):
        filepath = os.path.join(run_directory, "dataOutput.json")
        with open(filepath, "r") as file:
            data = json.load(file)

        average_time_alive_deers = data["DeerLogData"]["AverageLifespan"]
        average_time_alive_wolves = data["WolfLogData"]["AverageLifespan"]

        balance_metric = (
            average_time_alive_deers
            + average_time_alive_deers
            - abs(average_time_alive_deers - average_time_alive_wolves)
        )

        return balance_metric


def main():
    parser = argparse.ArgumentParser(description="Training script for ML Agents.")

    parser.add_argument(
        "--env", required=True, help="Path to the Unity environment executable."
    )
    parser.add_argument("--config", required=True, help="Path to the json config file.")
    parser.add_argument("--run-id", required=True, help="Run ID for the training run.")

    args = parser.parse_args()

    with open(args.config, "r") as f:
        training_config = json.load(f)
        trainer = Trainer(args.env, training_config, args.run_id)
        trainer.optimize()


if __name__ == "__main__":
    main()
