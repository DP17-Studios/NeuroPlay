# BalanceBot - Motor Coordination Trainer

## Overview
BalanceBot is a robot balancing simulator designed for stroke rehabilitation and motor skill therapy. The game focuses on fine motor control using keyboard, mouse, or tilt sensors, with incremental physical tasks to improve coordination.

## Game Mechanics
- **Robot Balancing Simulator**: Control a robot that must maintain balance
- **Fine Motor Control**: Use precise movements to adjust robot position
- **Incremental Physical Tasks**: Gradually increasing difficulty of motor challenges
- **Multiple Control Options**: Support for keyboard, mouse, or tilt sensor input
- **Adaptive Difficulty**: Adjusts based on player's motor control capabilities

## Backend Features
- **Motion Data Smoothing and Scoring**: Analyzes precision of motor movements
- **Recovery Timeline Graphing**: Tracks improvement in motor skills over time
- **AI-Based Level Difficulty Prediction**: Uses reinforcement learning to optimize challenge level
- **Token Reward System**: Motivational system (with potential blockchain integration)

## AI Analysis
- Uses OpenAI Gym, stable-baselines3, and PyTorch for reinforcement learning
- Analyzes motion patterns to identify motor control issues
- Predicts optimal difficulty progression for rehabilitation
- Provides personalized exercise recommendations

## Files in this Directory
- `BalanceBotController.cs`: Main game controller script
- `PhysicsSimulator.cs`: Handles robot physics and balance mechanics
- `InputManager.cs`: Processes various input methods (keyboard, mouse, tilt)
- `TaskGenerator.cs`: Creates appropriate motor skill challenges
- `MotionAnalyzer.cs`: Records and analyzes player movement data
- `ProgressTracker.cs`: Monitors improvement in motor skills
- `UIManager.cs`: Manages game interface elements

## Integration with Backend
The game sends the following data to the backend:
- Motion control precision metrics
- Balance maintenance duration and quality
- Task completion success rates
- Input method effectiveness comparison
- Motor skill improvement trends
- Session duration and engagement levels