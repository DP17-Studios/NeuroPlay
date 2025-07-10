from django.urls import path, include
from rest_framework.routers import DefaultRouter
from .views import UserViewSet, PlayerProfileViewSet

router = DefaultRouter()
router.register(r'users', UserViewSet)
router.register(r'profiles', PlayerProfileViewSet)

urlpatterns = [
    path('', include(router.urls)),
]