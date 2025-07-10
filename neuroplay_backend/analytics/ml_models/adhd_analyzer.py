"""
ADHD analysis module for NeuroSprint game data
"""

import numpy as np
import pandas as pd
from sklearn.ensemble import IsolationForest
from sklearn.preprocessing import StandardScaler
import matplotlib.pyplot as plt
import io
import base64


class ADHDAnalyzer:
    """
    Analyzes gameplay data from NeuroSprint to detect patterns
    related to attention deficit and hyperactivity
    """
    
    def __init__(self):
        self.scaler = StandardScaler()
        self.anomaly_detector = IsolationForest(
            contamination=0.1,
            random_state=42
        )
    
    def preprocess_data(self, sessions_data):
        """
        Preprocess raw session data into features for analysis
        
        Args:
            sessions_data: List of session data dictionaries
            
        Returns:
            DataFrame with extracted features
        """
        features = []
        
        for session in sessions_data:
            # Extract session_data JSON
            data = session.get('session_data', {})
            
            # Skip sessions with missing data
            if not data:
                continue
                
            # Extract relevant metrics
            reaction_times = data.get('reaction_times', [])
            if not reaction_times:
                continue
                
            # Calculate features
            avg_reaction = np.mean(reaction_times)
            std_reaction = np.std(reaction_times)
            min_reaction = np.min(reaction_times)
            max_reaction = np.max(reaction_times)
            
            attention_score = data.get('attention_score', 0)
            attention_consistency = data.get('attention_consistency', 0)
            
            obstacles_avoided = data.get('obstacles_avoided', 0)
            obstacles_hit = data.get('obstacles_hit', 0)
            
            # Avoid division by zero
            total_obstacles = obstacles_avoided + obstacles_hit
            if total_obstacles > 0:
                hit_ratio = obstacles_hit / total_obstacles
            else:
                hit_ratio = 0
                
            distractions_ignored = data.get('distractions_ignored', 0)
            distractions_triggered = data.get('distractions_triggered', 0)
            
            # Avoid division by zero
            total_distractions = distractions_ignored + distractions_triggered
            if total_distractions > 0:
                distraction_ratio = distractions_triggered / total_distractions
            else:
                distraction_ratio = 0
                
            # Create feature vector
            feature = {
                'session_id': session.get('id'),
                'avg_reaction_time': avg_reaction,
                'std_reaction_time': std_reaction,
                'min_reaction_time': min_reaction,
                'max_reaction_time': max_reaction,
                'reaction_time_range': max_reaction - min_reaction,
                'attention_score': attention_score,
                'attention_consistency': attention_consistency,
                'hit_ratio': hit_ratio,
                'distraction_ratio': distraction_ratio,
                'score': session.get('score', 0),
                'duration_minutes': session.get('duration_minutes', 0),
            }
            
            features.append(feature)
            
        # Convert to DataFrame
        if not features:
            return pd.DataFrame()
            
        return pd.DataFrame(features)
    
    def detect_attention_anomalies(self, df):
        """
        Detect sessions with unusual attention patterns
        
        Args:
            df: DataFrame with preprocessed features
            
        Returns:
            DataFrame with anomaly scores
        """
        if df.empty:
            return df
            
        # Select numerical features for anomaly detection
        feature_cols = [
            'avg_reaction_time', 'std_reaction_time', 
            'reaction_time_range', 'attention_score',
            'attention_consistency', 'hit_ratio',
            'distraction_ratio'
        ]
        
        # Handle missing data
        df_features = df[feature_cols].fillna(0)
        
        # Scale features
        scaled_features = self.scaler.fit_transform(df_features)
        
        # Detect anomalies
        df['anomaly_score'] = self.anomaly_detector.fit_predict(scaled_features)
        
        # Convert to a more intuitive score (-1 = anomaly, 1 = normal)
        df['attention_pattern'] = df['anomaly_score'].apply(
            lambda x: 'Inconsistent' if x == -1 else 'Normal'
        )
        
        return df
    
    def analyze_sessions(self, sessions_data):
        """
        Analyze multiple game sessions for ADHD patterns
        
        Args:
            sessions_data: List of session data dictionaries
            
        Returns:
            Dictionary with analysis results
        """
        # Preprocess data
        df = self.preprocess_data(sessions_data)
        
        if df.empty:
            return {
                'error': 'Insufficient data for analysis',
                'recommendations': [
                    'Play more NeuroSprint sessions to generate data',
                    'Ensure gameplay data is being properly recorded'
                ]
            }
        
        # Detect anomalies
        df = self.detect_attention_anomalies(df)
        
        # Calculate metrics
        avg_reaction = df['avg_reaction_time'].mean()
        avg_attention = df['attention_score'].mean()
        attention_variability = df['attention_consistency'].mean()
        
        # Count anomalies
        anomaly_count = (df['anomaly_score'] == -1).sum()
        anomaly_percent = (anomaly_count / len(df)) * 100
        
        # Generate plots
        reaction_time_plot = self._plot_reaction_times(df)
        attention_plot = self._plot_attention_scores(df)
        
        # Generate insights
        insights = self._generate_insights(df, avg_reaction, avg_attention, attention_variability, anomaly_percent)
        
        # Prepare results
        results = {
            'metrics': {
                'average_reaction_time': avg_reaction,
                'average_attention_score': avg_attention,
                'attention_variability': attention_variability,
                'anomaly_percentage': anomaly_percent
            },
            'visualizations': {
                'reaction_time_plot': reaction_time_plot,
                'attention_plot': attention_plot
            },
            'insights': insights,
            'adhd_indicators': self._evaluate_adhd_indicators(df),
            'recommendations': self._generate_recommendations(df, avg_reaction, avg_attention, attention_variability)
        }
        
        return results
    
    def _plot_reaction_times(self, df):
        """Generate reaction time plot"""
        plt.figure(figsize=(10, 6))
        plt.plot(df['session_id'], df['avg_reaction_time'], 'o-', label='Average Reaction Time')
        plt.fill_between(
            df['session_id'],
            df['avg_reaction_time'] - df['std_reaction_time'],
            df['avg_reaction_time'] + df['std_reaction_time'],
            alpha=0.2
        )
        plt.title('Reaction Time Trends')
        plt.xlabel('Session')
        plt.ylabel('Reaction Time (seconds)')
        plt.legend()
        plt.grid(True, alpha=0.3)
        
        # Convert plot to base64 string
        buffer = io.BytesIO()
        plt.savefig(buffer, format='png')
        buffer.seek(0)
        image_png = buffer.getvalue()
        buffer.close()
        plt.close()
        
        return base64.b64encode(image_png).decode('utf-8')
    
    def _plot_attention_scores(self, df):
        """Generate attention score plot"""
        plt.figure(figsize=(10, 6))
        plt.plot(df['session_id'], df['attention_score'], 'o-', label='Attention Score')
        plt.title('Attention Score Trends')
        plt.xlabel('Session')
        plt.ylabel('Attention Score')
        plt.legend()
        plt.grid(True, alpha=0.3)
        
        # Add anomaly markers
        anomalies = df[df['anomaly_score'] == -1]
        if not anomalies.empty:
            plt.scatter(
                anomalies['session_id'],
                anomalies['attention_score'],
                color='red',
                s=100,
                label='Attention Anomaly',
                zorder=5
            )
        
        # Convert plot to base64 string
        buffer = io.BytesIO()
        plt.savefig(buffer, format='png')
        buffer.seek(0)
        image_png = buffer.getvalue()
        buffer.close()
        plt.close()
        
        return base64.b64encode(image_png).decode('utf-8')
    
    def _generate_insights(self, df, avg_reaction, avg_attention, attention_variability, anomaly_percent):
        """Generate insights based on the data"""
        insights = []
        
        # Reaction time insights
        if avg_reaction > 0.8:
            insights.append("Reaction times are slower than average, which may indicate attention challenges")
        elif avg_reaction < 0.4:
            insights.append("Reaction times are faster than average, showing good attentional alertness")
        
        # Attention score insights
        if avg_attention < 50:
            insights.append("Overall attention scores are low, suggesting difficulty maintaining focus")
        elif avg_attention > 80:
            insights.append("Overall attention scores are high, indicating good sustained attention")
        
        # Variability insights
        if attention_variability > 20:
            insights.append("High variability in attention suggests inconsistent focus, a common ADHD indicator")
        elif attention_variability < 10:
            insights.append("Low variability in attention suggests consistent focus throughout gameplay")
        
        # Anomaly insights
        if anomaly_percent > 30:
            insights.append("A high percentage of sessions show unusual attention patterns")
        elif anomaly_percent < 10:
            insights.append("Attention patterns are mostly consistent across sessions")
        
        return insights
    
    def _evaluate_adhd_indicators(self, df):
        """Evaluate potential ADHD indicators in the data"""
        indicators = {
            'high_reaction_variability': False,
            'attention_lapses': False,
            'distractibility': False,
            'inconsistent_performance': False
        }
        
        # High variability in reaction times
        if df['std_reaction_time'].mean() > 0.3:
            indicators['high_reaction_variability'] = True
        
        # Attention lapses (sudden drops in attention score)
        attention_diffs = df['attention_score'].diff().dropna()
        if (attention_diffs < -20).any():
            indicators['attention_lapses'] = True
        
        # Distractibility (high distraction ratio)
        if df['distraction_ratio'].mean() > 0.4:
            indicators['distractibility'] = True
        
        # Inconsistent performance (high score variability)
        if df['score'].std() / df['score'].mean() > 0.5:
            indicators['inconsistent_performance'] = True
        
        return indicators
    
    def _generate_recommendations(self, df, avg_reaction, avg_attention, attention_variability):
        """Generate personalized recommendations based on the analysis"""
        recommendations = []
        
        # Basic recommendations
        recommendations.append("Continue regular NeuroSprint sessions to track attention patterns over time")
        
        # Specific recommendations based on metrics
        if avg_reaction > 0.7:
            recommendations.append("Try shorter, more frequent gameplay sessions to improve reaction time")
        
        if avg_attention < 60:
            recommendations.append("Practice mindfulness exercises to improve sustained attention")
        
        if attention_variability > 15:
            recommendations.append("Work on consistency by gradually increasing session duration")
        
        if df['distraction_ratio'].mean() > 0.3:
            recommendations.append("Practice ignoring distractions in a controlled environment")
        
        return recommendations