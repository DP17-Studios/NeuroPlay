from django.db import models
from api.models import PlayerProfile, GameSession

class NeuroSprintSession(models.Model):
    """
    Detailed data for NeuroSprint game sessions
    """
    session = models.OneToOneField(GameSession, on_delete=models.CASCADE, related_name='neurosprint_data')
    
    # Attention metrics
    avg_attention_score = models.FloatField(default=0)
    attention_consistency = models.FloatField(default=0)  # Standard deviation of attention
    attention_drops = models.IntegerField(default=0)  # Number of significant attention drops
    
    # Reaction time metrics
    avg_reaction_time = models.FloatField(default=0)
    min_reaction_time = models.FloatField(default=0)
    max_reaction_time = models.FloatField(default=0)
    reaction_time_std = models.FloatField(default=0)  # Standard deviation
    
    # Obstacle metrics
    obstacles_avoided = models.IntegerField(default=0)
    obstacles_hit = models.IntegerField(default=0)
    obstacle_avoidance_rate = models.FloatField(default=0)  # Percentage of obstacles avoided
    
    # Distraction metrics
    distractions_ignored = models.IntegerField(default=0)
    distractions_triggered = models.IntegerField(default=0)
    distraction_resistance_rate = models.FloatField(default=0)  # Percentage of distractions ignored
    
    # Raw data storage
    reaction_times = models.JSONField(default=list)  # List of individual reaction times
    attention_scores = models.JSONField(default=list)  # List of attention scores over time
    
    # Analysis results
    adhd_indicators = models.JSONField(default=dict)  # Indicators of potential ADHD patterns
    
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    def __str__(self):
        return f"NeuroSprint Session - {self.session.player.user.username} - {self.session.start_time}"
    
    class Meta:
        ordering = ['-session__start_time']


class NeuroSprintProgress(models.Model):
    """
    Tracks player progress and metrics across multiple NeuroSprint sessions
    """
    player = models.OneToOneField(PlayerProfile, on_delete=models.CASCADE, related_name='neurosprint_progress')
    
    # Overall stats
    total_sessions = models.IntegerField(default=0)
    total_playtime_minutes = models.FloatField(default=0)
    highest_score = models.IntegerField(default=0)
    
    # Attention metrics
    avg_attention_score = models.FloatField(default=0)
    attention_trend = models.FloatField(default=0)  # Positive = improving, negative = declining
    
    # Reaction time metrics
    avg_reaction_time = models.FloatField(default=0)
    reaction_time_trend = models.FloatField(default=0)  # Negative = improving, positive = declining
    
    # Obstacle and distraction metrics
    overall_obstacle_avoidance_rate = models.FloatField(default=0)
    overall_distraction_resistance_rate = models.FloatField(default=0)
    
    # Progress metrics
    current_difficulty_level = models.CharField(
        max_length=10,
        choices=[('easy', 'Easy'), ('medium', 'Medium'), ('hard', 'Hard')],
        default='easy'
    )
    difficulty_progression = models.JSONField(default=list)  # History of difficulty changes
    
    # ADHD assessment
    adhd_likelihood_score = models.FloatField(default=0)  # 0-100 scale
    attention_consistency_score = models.FloatField(default=0)  # 0-100 scale
    
    # Last session reference
    last_session = models.ForeignKey(
        NeuroSprintSession, 
        on_delete=models.SET_NULL, 
        null=True, 
        blank=True,
        related_name='+'
    )
    
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    def __str__(self):
        return f"NeuroSprint Progress - {self.player.user.username}"


class NeuroSprintRecommendation(models.Model):
    """
    AI-generated recommendations for players based on their NeuroSprint performance
    """
    player = models.ForeignKey(PlayerProfile, on_delete=models.CASCADE, related_name='neurosprint_recommendations')
    
    # Recommendation details
    title = models.CharField(max_length=100)
    description = models.TextField()
    priority = models.IntegerField(default=0)  # Higher = more important
    
    # Recommendation type
    RECOMMENDATION_TYPES = [
        ('gameplay', 'Gameplay Adjustment'),
        ('difficulty', 'Difficulty Adjustment'),
        ('exercise', 'Attention Exercise'),
        ('schedule', 'Play Schedule'),
        ('general', 'General Advice'),
    ]
    recommendation_type = models.CharField(max_length=20, choices=RECOMMENDATION_TYPES, default='general')
    
    # Status tracking
    is_implemented = models.BooleanField(default=False)
    implementation_date = models.DateTimeField(null=True, blank=True)
    
    # Related session that triggered this recommendation
    triggering_session = models.ForeignKey(
        NeuroSprintSession, 
        on_delete=models.SET_NULL, 
        null=True, 
        blank=True,
        related_name='triggered_recommendations'
    )
    
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    def __str__(self):
        return f"{self.title} - {self.player.user.username}"
    
    class Meta:
        ordering = ['-priority', '-created_at']