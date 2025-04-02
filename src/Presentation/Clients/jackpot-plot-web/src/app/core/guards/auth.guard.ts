import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import Keycloak from 'keycloak-js';

export const authGuard: CanActivateFn = async (_, state) => {
  const keycloak = inject(Keycloak);
  const authenticated = keycloak.authenticated;

  if (!authenticated) {
    await keycloak.login({ redirectUri: window.location.origin + state.url });
    return false;
  }

  return true;
};
