// Environment configuration for the application
export const env = {
  VITE_API_URL: import.meta.env.VITE_API_URL || 'http://localhost:5156',

  VITE_AUTH_CLIENT_ID:
    import.meta.env.VITE_AUTH_CLIENT_ID || '00000000-0000-0000-0000-000000000000',

  VITE_AUTH_AUTHORITY:
    import.meta.env.VITE_AUTH_AUTHORITY ||
    'https://barberly-dev.b2clogin.com/barberly-dev.onmicrosoft.com/B2C_1_susi',

  VITE_AUTH_KNOWN_AUTHORITIES:
    import.meta.env.VITE_AUTH_KNOWN_AUTHORITIES || 'barberly-dev.b2clogin.com',

  VITE_AUTH_REDIRECT_URI:
    import.meta.env.VITE_AUTH_REDIRECT_URI || 'http://localhost:5174/auth/callback',

  VITE_AUTH_POST_LOGOUT_REDIRECT_URI:
    import.meta.env.VITE_AUTH_POST_LOGOUT_REDIRECT_URI || 'http://localhost:5174',

  VITE_AUTH_SCOPES: import.meta.env.VITE_AUTH_SCOPES || 'openid profile email',

  VITE_AUTH_SIGN_UP_SIGN_IN_POLICY:
    import.meta.env.VITE_AUTH_SIGN_UP_SIGN_IN_POLICY || 'B2C_1_susi',

  VITE_AUTH_RESET_PASSWORD_POLICY:
    import.meta.env.VITE_AUTH_RESET_PASSWORD_POLICY || 'B2C_1_reset_password',

  VITE_AUTH_EDIT_PROFILE_POLICY:
    import.meta.env.VITE_AUTH_EDIT_PROFILE_POLICY || 'B2C_1_edit_profile',

  VITE_APP_ENV: import.meta.env.VITE_APP_ENV || 'development',

  VITE_APP_NAME: import.meta.env.VITE_APP_NAME || 'Barberly',

  VITE_APP_VERSION: import.meta.env.VITE_APP_VERSION || '1.0.0',
};

export const msalConfig = {
  auth: {
    clientId: env.VITE_AUTH_CLIENT_ID,
    authority: env.VITE_AUTH_AUTHORITY,
    knownAuthorities: [env.VITE_AUTH_KNOWN_AUTHORITIES],
    redirectUri: env.VITE_AUTH_REDIRECT_URI,
    postLogoutRedirectUri: env.VITE_AUTH_POST_LOGOUT_REDIRECT_URI,
  },
  cache: {
    cacheLocation: 'localStorage' as const,
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level: number, message: string, containsPii: boolean) => {
        if (containsPii) return;
        switch (level) {
          case 0: // LogLevel.Error
            console.error(`MSAL Error: ${message}`);
            break;
          case 1: // LogLevel.Warning
            console.warn(`MSAL Warning: ${message}`);
            break;
          case 2: // LogLevel.Info
            console.info(`MSAL Info: ${message}`);
            break;
          case 3: // LogLevel.Verbose
            console.debug(`MSAL Debug: ${message}`);
            break;
        }
      },
      logLevel: env.VITE_APP_ENV === 'development' ? 3 : 1,
    },
  },
};

export const apiConfig = {
  baseURL: env.VITE_API_URL,
};
