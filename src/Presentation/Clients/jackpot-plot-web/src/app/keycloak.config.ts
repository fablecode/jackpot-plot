import {
  AutoRefreshTokenService,
  createInterceptorCondition, INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
  IncludeBearerTokenCondition,
  provideKeycloak, UserActivityService,
  withAutoRefreshToken
} from 'keycloak-angular';

const allUrlsCondition = createInterceptorCondition<IncludeBearerTokenCondition>({
  urlPattern: /.*/ // Match all URLs
});

export const provideKeycloakAngular = () =>
  provideKeycloak({
    config: {
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
        useValue: [allUrlsCondition] // 🔥 Add Bearer token to all HTTP requests
      }
    ]
  });
