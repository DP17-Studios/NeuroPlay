from django.db import models
from api.models import PlayerProfile


class Game(models.Model):
    """
    Game configuration and metadata
    """
    name = models.CharField(max_length=100, unique=True)
    display_name = models.CharField(max_length=200)
    description = models.TextField()
    version = models.CharField(max_length=20)
    health_domain = models.CharField(max_length=100)
    
    # Configuration options stored as JSON
    default_config = models.JSONField(default=dict)
    
    # Unity bundle information
    unity_bundle_url = models.URLField(blank=True, null=True)
    unity_bundle_version = models.CharField(max_length=20, blank=True, null=True)
    
    # Analytics and tracking
    times_played = models.IntegerField(default=0)
    active = models.BooleanField(default=True)
    
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    def __str__(self):
        return self.display_name
    
    class Meta:
        ordering = ['name']


class PlayerGameConfig(models.Model):
    """
    Player-specific game configuration
    """
    player = models.ForeignKey(PlayerProfile, on_delete=models.CASCADE, related_name='game_configs')
    game = models.ForeignKey(Game, on_delete=models.CASCADE, related_name='player_configs')
    
    # Player-specific configuration overrides
    config_overrides = models.JSONField(default=dict)
    
    # Personalization and progress
    difficulty_level = models.CharField(
        max_length=10,
        choices=[('easy', 'Easy'), ('medium', 'Medium'), ('hard', 'Hard')],
        default='medium'
    )
    current_level = models.IntegerField(default=1)
    highest_score = models.IntegerField(default=0)
    times_played = models.IntegerField(default=0)
    total_playtime_minutes = models.FloatField(default=0)
    last_played = models.DateTimeField(null=True, blank=True)
    
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    def __str__(self):
        return f"{self.player.user.username} - {self.game.name} Config"
    
    class Meta:
        unique_together = ('player', 'game')
        ordering = ['player', 'game']