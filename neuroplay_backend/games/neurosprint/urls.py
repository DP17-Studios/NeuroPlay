from django.urls import path, include
from rest_framework.routers import DefaultRouter
from .views import (
    NeuroSprintSessionViewSet,
    NeuroSprintProgressViewSet,
    NeuroSprintRecommendationViewSet,
    ProcessGameSessionData
)

# Create a router and register our viewsets
router = DefaultRouter()
router.register(r'sessions', NeuroSprintSessionViewSet)
router.register(r'progress', NeuroSprintProgressViewSet)
router.register(r'recommendations', NeuroSprintRecommendationViewSet)

# Additional URL patterns
urlpatterns = [
    path('', include(router.urls)),
    path('process-session/<int:session_id>/', ProcessGameSessionData.as_view(), name='process-session'),
]