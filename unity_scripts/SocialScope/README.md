# SocialScope - Autism Interaction Sim

## Overview
SocialScope is a social decision-making simulation designed for users with Autism Spectrum Disorder (ASD). The game focuses on gaze and facial expression-based choices within scenario-driven learning experiences to improve social interaction skills.

## Game Mechanics
- **Social Decision-Making Simulation**: Navigate realistic social scenarios
- **Gaze/Facial Expression-Based Choices**: Interpret and respond to social cues
- **Scenario-Driven Learning**: Progress through everyday social situations
- **Multiple Difficulty Levels**: Gradually increasing complexity of social interactions
- **Feedback System**: Immediate guidance on social responses

## Backend Features
- **Social Cue Response Analysis**: Evaluates appropriateness of social decisions
- **Behavioral Report Cards**: Summarizes progress in different social skill areas
- **Optional Webcam Gaze Tracking**: Analyzes attention to relevant social cues
- **Longitudinal Behavioral Improvement Tracking**: Measures progress over time

## AI Analysis
- Uses OpenCV, dlib, face-recognition, and flask-socketio for facial/gaze analysis
- Identifies patterns in social interaction approaches
- Detects improvement in recognition of social cues
- Provides personalized social skill development recommendations

## Files in this Directory
- `SocialScopeController.cs`: Main game controller script
- `ScenarioManager.cs`: Manages social scenarios and progression
- `CharacterBehavior.cs`: Controls NPC facial expressions and social cues
- `GazeTracker.cs`: Optional webcam integration for eye-tracking
- `SocialResponseAnalyzer.cs`: Evaluates appropriateness of player choices
- `FeedbackManager.cs`: Provides constructive feedback on social interactions
- `UIManager.cs`: Manages game interface elements

## Integration with Backend
The game sends the following data to the backend:
- Social scenario response choices
- Response time to social cues
- Gaze tracking data (if enabled)
- Social skill category performance metrics
- Engagement level with different scenario types
- Learning progression across social contexts