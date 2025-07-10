# Emotion Echo - NLP & Voice Game

## Overview
Emotion Echo is a dialogue-based game that uses natural language processing and voice recognition to monitor emotional health and detect signs of depression. The game features a branching story where the player's emotional state influences outcomes.

## Game Mechanics
- **Dialogue-Based Branching Story**: Interactive narrative with multiple paths
- **Speech Input**: Players use their voice to progress through the story
- **Emotional State Tracking**: Game adapts based on detected emotional patterns
- **Multiple Endings**: Story outcomes reflect player's emotional journey

## Backend Features
- **Sentiment Analysis**: Analyzes voice-to-text transcripts for emotional content
- **Session-Wise Mood Shift Graphs**: Tracks emotional changes during gameplay
- **NLP-Based Emotion Classification**: Categorizes speech into emotional states
- **Emotional Trend Dashboard**: Visualizes long-term emotional patterns for users/caregivers

## AI Analysis
- Uses transformers, nltk, speechrecognition, and librosa for audio/text processing
- Performs sentiment analysis on player responses
- Identifies emotional patterns that may indicate depression
- Provides insights on emotional health trends over time

## Files in this Directory
- `EmotionEchoController.cs`: Main game controller script
- `DialogueManager.cs`: Manages conversation flow and branching
- `VoiceRecognitionManager.cs`: Handles speech input and processing
- `EmotionAnalyzer.cs`: Local emotion detection from voice and text
- `StoryManager.cs`: Controls narrative progression and endings
- `UIManager.cs`: Manages game interface elements

## Integration with Backend
The game sends the following data to the backend:
- Voice recordings (converted to text)
- Detected emotional states during gameplay
- Conversation choices and pathways
- Session duration and engagement metrics
- Local emotion analysis results for server-side validation