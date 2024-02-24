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
        self.output_path = os.path.join(training_config["log_directory_path"], run_id)  # Use os.path.join for OS compatibility
        self.run_id = run_id

    def suggest_config_values(self, config, config_template, trial):
        for key, value in config_template.items():
            if isinstance(value, dict):  # Handle nested dictionaries
                if key not in config:
                    config[key] = {}
                self.suggest_config_values(config[key], value, trial)
            elif re.match('.*_reward_curve', key):
                # Generate 4 values within the specified range for reward curves
                config[key] = [trial.suggest_float(f"{key}_{i}", value[i][0], value[i][1]) for i in range(4)]
            elif isinstance(value, list) and len(value) == 2 and all(isinstance(v, (int, float)) for v in value):
                # Check if the key matches the pattern for reward curve
                if isinstance(value[0], int):
                    # Handle integer ranges
                    config[key] = trial.suggest_int(key, value[0], value[1])
                else:
                    # Handle float ranges
                    config[key] = trial.suggest_float(key, value[0], value[1])

    def optimize(self):
        self.study = optuna.create_study(direction='maximize')
        self.study.optimize(self.objective, n_trials=4, n_jobs=4)

        best_trial = {
            'number': self.study.best_trial.number,
            'params': self.study.best_trial.params
        }
        with open(os.path.join(self.output_path, "best_trial.json"), 'w') as f:
            json.dump(best_trial, f, indent=4)

        print('Complete!')

    def objective(self, trial):
        # Load and suggest values for environment configuration
        with open(self.env_config, 'r') as f:
            env_config = json.load(f)
        self.suggest_config_values(env_config, self.env_config_template, trial)

        # Save the modified environment configuration
        env_config_path = os.path.join(self.output_path, str(trial.number), f"env_config_{trial.number}.json")
        os.makedirs(os.path.dirname(env_config_path), exist_ok=True)
        with open(env_config_path, 'w') as f:
            json.dump(env_config, f, indent=4)

        # Load and suggest values for neural network configuration
        with open(self.nn_config, 'r') as f:
            nn_config = yaml.load(f, Loader=yaml.FullLoader)
        self.suggest_config_values(nn_config, self.nn_config_template, trial)

        # Save the modified neural network configuration
        nn_config_path = os.path.join(self.output_path, str(trial.number), f"nn_config_{trial.number}.yaml")
        os.makedirs(os.path.dirname(nn_config_path), exist_ok=True)
        with open(nn_config_path, 'w') as f:
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
            env_config_path
        ]

        evaluation_score = self.run_training_script(run_command, trial.number)
        return -evaluation_score

    def run_training_script(self, run_command, trial_number):
        log_file_path = os.path.join(self.output_path, f"trial_{trial_number}_run_{trial_number}.log")
        os.makedirs(os.path.dirname(log_file_path), exist_ok=True)

        with open(log_file_path, 'w') as log_file:
            subprocess.run(run_command, stdout=log_file, stderr=log_file)

        evaluation_score = 0.0  # Replace with the actual evaluation score
        return evaluation_score

def main():
    parser = argparse.ArgumentParser(description='Training script for ML Agents.')

    parser.add_argument('--env', required=True, help='Path to the Unity environment executable.')
    parser.add_argument('--config', required=True, help='Path to the json config file.')
    parser.add_argument('--run-id', required=True, help='Run ID for the training run.')

    args = parser.parse_args()

    with open(args.config, 'r') as f:
        training_config = json.load(f)
        trainer = Trainer(args.env, training_config, args.run_id)
        trainer.optimize()

if __name__ == '__main__':
    main()
