import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import {provideRouter, withComponentInputBinding} from '@angular/router';

import { routes } from './app.routes';
import {provideHttpClient} from '@angular/common/http';
import {provideKeycloak} from 'keycloak-angular';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideKeycloak({
      config: {
        // url: 'http://keycloak:8080/auth',
        url: 'http://localhost:8085',
        realm: 'jackpotplot',
        clientId: 'jackpotplot-web',
      },
      initOptions: {
        onLoad: 'check-sso', // or 'login-required' based on your needs
        silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
      },
    })
  ]
};
