import optuna
import argparse
import json
import subprocess
import yaml
import os

class Trainer:

    def __init__(self, env, training_config, run_id):
        self.env = env
        self.env_config_template = training_config["env_config"]
        self.env_config = training_config["env_config_path"]
        self.nn_config_template = training_config["nn_config"]
        self.nn_config = training_config["nn_config_path"]
        self.output_path = training_config["log_directory_path"] + "\\" + run_id
        self.run_id = run_id
    
    def optimize(self):
        self.study = optuna.create_study(direction='maximize')
        self.study.optimize(self.objective, n_trials=4, n_jobs=4)
        
        best_trial = {
            'number': self.study.best_trial.number,
            'params': self.study.best_trial.params
        }
        with open(self.output_path + "\\best_trial.json", 'w') as f:
            json.dump(best_trial, f, indent=4)
        
        print('Complete!')

    def objective(self, trial):

        with open(self.env_config, 'r') as f:
            env_config = json.load(f)
            for key, value in self.env_config_template.items():
                min_val, max_val = value[0], value[1]

                if isinstance(min_val, int) and isinstance(max_val, int):
                    env_config[key] = trial.suggest_int(key, min_val, max_val)
                else:
                    env_config[key] = trial.suggest_float(key, min_val, max_val)
        
        with open(self.nn_config, 'r') as f:
            nn_config = yaml.load(open(self.nn_config, 'r'), Loader=yaml.FullLoader)
            for key, value in self.nn_config_template.items():
                min_val, max_val = value[0], value[1]

                if isinstance(min_val, int) and isinstance(max_val, int):
                    nn_config[key] = trial.suggest_int(key, min_val, max_val)
                else:
                    nn_config[key] = trial.suggest_float(key, min_val, max_val)
    
        env_config_path = self.output_path + "\\" + str(trial.number) + "\env_config_" + str(trial.number) + ".json"
        directory_path = os.path.dirname(env_config_path)
        os.makedirs(directory_path, exist_ok=True)
        with open(env_config_path, 'w') as f:
            json.dump(env_config, f, indent=4)
        
        nn_config_path = self.output_path + "\\" + str(trial.number) + "\\nn_config_" + str(trial.number) + ".yaml"
        directory_path = os.path.dirname(nn_config_path)
        os.makedirs(directory_path, exist_ok=True)
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

def init_json(env_config_path, output_path):
    with open(env_config_path, 'r') as f:
        config = json.load(f)
        config["output_path"] = output_path

    with open(env_config_path, 'w') as f:
        json.dump(config, f, indent=4)

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
