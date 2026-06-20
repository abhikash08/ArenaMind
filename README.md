The original game was taken from https://github.com/humbertodias/unity-small-fighter.git. 
# Unity 2D Fighter: ML-Agents & Finite State Machines

A 2D fighting game environment built in Unity to demonstrate the integration of custom Finite State Machine (FSM) architecture with Deep Reinforcement Learning (Unity ML-Agents).

This repository serves as a foundational teaching tool for game AI, separating the physical game logic from the neural network "brain" using a modular, event-driven state machine.

## Key Features

* **Modular Finite State Machine (FSM):** A lean, non-MonoBehaviour state machine (The "Puppet Master") that cleanly handles transitions between states (`Idle`, `Walking`, `Jumping`, `Attacking`).
* **Frame-Perfect Combat System:** Custom hitbox/hurtbox pooling system that reads attack data (start frames, active frames, damage, knockback) directly from ScriptableObjects/Action lists.
* **Deep Reinforcement Learning Integration:** Fully configured `FighterAgent` that feeds the neural network 11 floating-point observations and accepts discrete action branches.
* **Null-Safe Architecture:** Hardened code designed to prevent simulation crashes (like `UnityTimeOutException` or missing Input Systems) during thousands of accelerated training episodes.

## The AI Architecture

The agent is trained using Proximal Policy Optimization (PPO) via Unity ML-Agents.

### Observation Space (The Eyes)
The neural network observes the arena through 11 specific float variables per frame:
1.  **Self:** Health %, X Position, Y Position, X Velocity, Y Velocity.
2.  **Opponent:** Health %, X Position, Y Position, X Velocity, Y Velocity.
3.  **Arena:** Absolute distance between fighters.

### Action Space (The Muscles)
The agent controls the fighter using two simultaneous, discrete decision branches:
* **Branch 0 (Movement):** 0 = Idle, 1 = Walk Forward, 2 = Walk Backward, 3 = Jump
* **Branch 1 (Combat):** 0 = Do Nothing, 1 = Attack

### Reward System (The Teacher)
* **+0.1:** Land a successful hit (Carrot)
* **-0.1:** Take damage (Stick)
* **+1.0:** Knockout the opponent (Win Condition)
* **-1.0:** Get knocked out (Lose Condition)

## Setup & Installation

1.  **Unity Version:** Built using Unity 6 (6000.x LTS) / Unity 2022.3 LTS (adjust based on your actual version).
2.  **Dependencies:** * Unity ML-Agents Package (`com.unity.ml-agents`)
    * New Input System (`com.unity.inputsystem`)
3.  **To Play Manually:** Click on `Fighter 1`, locate the `New Fighter` script in the Inspector, and uncheck `isAI`.

## How to Train

1. Open your terminal/Anaconda prompt in the project directory.
2. Run the training commands
Press Play in the Unity Editor.

To view training metrics, open a new terminal tab and run:

Bash
tensorboard --logdir results
