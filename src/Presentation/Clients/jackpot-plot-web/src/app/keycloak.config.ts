import {
  AutoRefreshTokenService,
  createInterceptorCondition, INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
  IncludeBearerTokenCondition,
  provideKeycloak, UserActivityService,
  withAutoRefreshToken
} from 'keycloak-angular';


const localhostCondition = createInterceptorCondition<IncludeBearerTokenCondition>({
  urlPattern: /^(http:\/\/localhost:8085)(\/.*)?$/i // Match URLs starting with http://localhost:8181
});

export const provideKeycloakAngular = () =>
  provideKeycloak({
    config: {
      // url: 'http://keycloak:8080/auth',
      url: 'http://localhost:8085',
      realm: 'jackpotplot',
      clientId: 'jackpotplot-web',
    },
    initOptions: {
      onLoad: 'check-sso',
      silentCheckSsoRedirectUri: `${window.location.origin}/assets/silent-check-sso.html`,
      redirectUri: window.location.origin + '/'
    },
    features: [
      withAutoRefreshToken({
        onInactivityTimeout: 'logout',
        sessionTimeout: 60000
      })
    ],
    providers: [
      AutoRefreshTokenService,
      UserActivityService,
      {
        provide: INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
        useValue: ['*'] // 🔥 Add Bearer token to all HTTP requests
      }
    ]
  });
