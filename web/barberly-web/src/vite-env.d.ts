/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL: string;
  readonly VITE_AUTH_CLIENT_ID: string;
  readonly VITE_AUTH_AUTHORITY: string;
  readonly VITE_AUTH_KNOWN_AUTHORITIES: string;
  readonly VITE_AUTH_REDIRECT_URI: string;
  readonly VITE_AUTH_POST_LOGOUT_REDIRECT_URI: string;
  readonly VITE_AUTH_SCOPES: string;
  readonly VITE_AUTH_SIGN_UP_SIGN_IN_POLICY: string;
  readonly VITE_AUTH_RESET_PASSWORD_POLICY: string;
  readonly VITE_AUTH_EDIT_PROFILE_POLICY: string;
  readonly VITE_APP_ENV: string;
  readonly VITE_APP_NAME: string;
  readonly VITE_APP_VERSION: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
