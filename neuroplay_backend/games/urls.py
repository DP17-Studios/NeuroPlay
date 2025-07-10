from django.urls import path
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
]