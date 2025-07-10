from django.urls import path, include
from .views import (
    GameConfigView,
    GameDataUploadView,
    GameSessionStartView,
    GameSessionEndView,
    GameListView
)

urlpatterns = [
    path('list/', GameListView.as_view(), name='game-list'),
    path('config/<str:game_name>/', GameConfigView.as_view(), name='game-config'),
    path('start-session/', GameSessionStartView.as_view(), name='start-session'),
    path('end-session/<int:session_id>/', GameSessionEndView.as_view(), name='end-session'),
    path('upload-data/<int:session_id>/', GameDataUploadView.as_view(), name='upload-data'),
    
    # Game-specific URLs
    path('neurosprint/', include('games.neurosprint.urls')),
    # Additional game URLs will be added here as they are developed
    # path('emotionecho/', include('games.emotionecho.urls')),
    # path('memorymaze/', include('games.memorymaze.urls')),
    # path('balancebot/', include('games.balancebot.urls')),
    # path('socialscope/', include('games.socialscope.urls')),
]