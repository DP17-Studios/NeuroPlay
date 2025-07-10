from django.urls import path
from .views import (
    PlayerStatsView, 
    GamePerformanceView,
    ADHDAnalyticsView,
    EmotionAnalyticsView,
    MemoryAnalyticsView,
    MotorSkillsAnalyticsView,
    ASDAnalyticsView
)

urlpatterns = [
    path('player-stats/<int:player_id>/', PlayerStatsView.as_view(), name='player-stats'),
    path('game-performance/<str:game_name>/<int:player_id>/', GamePerformanceView.as_view(), name='game-performance'),
    path('adhd-analytics/<int:player_id>/', ADHDAnalyticsView.as_view(), name='adhd-analytics'),
    path('emotion-analytics/<int:player_id>/', EmotionAnalyticsView.as_view(), name='emotion-analytics'),
    path('memory-analytics/<int:player_id>/', MemoryAnalyticsView.as_view(), name='memory-analytics'),
    path('motor-skills-analytics/<int:player_id>/', MotorSkillsAnalyticsView.as_view(), name='motor-skills-analytics'),
    path('asd-analytics/<int:player_id>/', ASDAnalyticsView.as_view(), name='asd-analytics'),
]