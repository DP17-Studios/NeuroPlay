from django.db import models
from django.contrib.auth.models import User


class PlayerProfile(models.Model):
    """
    Extended profile for players with additional attributes
    """
    user = models.OneToOneField(User, on_delete=models.CASCADE, related_name='player_profile')
    date_of_birth = models.DateField(null=True, blank=True)
    
    # Health domain information
    has_adhd = models.BooleanField(default=False)
    has_dementia = models.BooleanField(default=False)
    has_motor_issues = models.BooleanField(default=False)
    has_asd = models.BooleanField(default=False)
    has_depression = models.BooleanField(default=False)
    
    # Game progress tracking
    games_played = models.IntegerField(default=0)
    total_sessions = models.IntegerField(default=0)
    total_playtime_minutes = models.IntegerField(default=0)
    
    # Preferences
    preferred_difficulty = models.CharField(
        max_length=10,
        choices=[('easy', 'Easy'), ('medium', 'Medium'), ('hard', 'Hard')],
        default='medium'
    )
    
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    def __str__(self):
        return f"{self.user.username}'s Profile"


class GameSession(models.Model):
    """
    Records individual game play sessions
    """
    player = models.ForeignKey(PlayerProfile, on_delete=models.CASCADE, related_name='game_sessions')
    game_name = models.CharField(max_length=100)
    start_time = models.DateTimeField()
    end_time = models.DateTimeField(null=True, blank=True)
    duration_minutes = models.FloatField(null=True, blank=True)
    
    # Performance metrics
    score = models.IntegerField(default=0)
    difficulty_level = models.CharField(
        max_length=10,
        choices=[('easy', 'Easy'), ('medium', 'Medium'), ('hard', 'Hard')],
        default='medium'
    )
    completed = models.BooleanField(default=False)
    
    # Raw data storage (JSON format)
    session_data = models.JSONField(default=dict)
    
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    def __str__(self):
        return f"{self.player.user.username} - {self.game_name} - {self.start_time}"
    
    class Meta:
        ordering = ['-start_time']