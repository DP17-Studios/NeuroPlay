# Neuroplay

Neuroplay is a platform for serious games with AI-powered backend targeting various cognitive and health domains.

## Project Overview

Neuroplay consists of 5 serious games developed in Unity, each targeting different cognitive and health domains, integrated with a Python/Django backend for AI-driven analytics, performance tracking, and smart feedback.

## Games

### 1. NeuroSprint - Reaction & Focus Tracker
- **Domain:** ADHD, Attention Training
- **Features:** Endless runner format, obstacle dodging, dynamic difficulty adjustment
- **Backend:** Reaction time data logging, attention consistency scoring, leaderboards

### 2. Emotion Echo - NLP & Voice Game
- **Domain:** Emotional Health, Depression Monitoring
- **Features:** Dialogue-based branching story, speech input, emotional state-driven outcomes
- **Backend:** Sentiment analysis, mood shift graphs, emotion classification

### 3. Memory Maze - Dementia Cognitive Tracker
- **Domain:** Dementia, Memory Rehabilitation
- **Features:** Maze navigation, pattern/sequence memorization, timed challenges
- **Backend:** Memory recall accuracy trends, navigation efficiency mapping, error heatmaps

### 4. BalanceBot - Motor Coordination Trainer
- **Domain:** Stroke Rehab, Motor Skill Therapy
- **Features:** Robot balancing simulator, fine motor control, incremental physical tasks
- **Backend:** Motion data analysis, recovery timeline graphing, difficulty prediction

### 5. SocialScope - Autism Interaction Sim
- **Domain:** Autism Spectrum Disorder (ASD)
- **Features:** Social decision-making simulation, gaze/facial expression-based choices
- **Backend:** Social cue response analysis, behavioral reports, gaze tracking

## System Architecture

| Component            | Technology                                    |
|---------------------|---------------------------------------------|
| Game Engine          | Unity (C#)                                    |
| Backend API          | Django + Django REST Framework                |
| AI/ML Analysis       | Python (scikit-learn, transformers, etc.)     |
| Database             | PostgreSQL                                    |
| Realtime Comm        | WebSocket / REST API                          |
| Dashboard (optional) | Streamlit / React Frontend                    |
| Deployment           | Docker + AWS/GCP                              |

## User Flow

1. Player launches game in Unity
2. Unity sends gameplay stats/events to Django server
3. Django stores raw data and calls ML pipeline
4. AI models process data and store analytics
5. User accesses performance analytics via web portal

## Installation

```
# Clone the repository
git clone https://github.com/yourusername/neuroplay.git

# Navigate to the project directory
cd neuroplay

# Install dependencies
pip install -r requirements.txt
```

## Development Roadmap

- [ ] Finalize game design documents (GDD) for each game
- [ ] Create Django models and REST endpoints for data logging
- [ ] Develop Unity <-> Django communication module
- [ ] Build dashboards for players and caregivers
- [ ] Integrate AI models and validate with simulated data

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contact

For questions and support, please open an issue on the GitHub repository.