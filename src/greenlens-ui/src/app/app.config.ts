import { APP_INITIALIZER, ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { ApiService } from './services/api.service';

import { routes } from './app.routes';

/**
 * Loads runtime config (API key) from the server before the app starts.
 */
function initializeApp(apiService: ApiService): () => Promise<void> {
  return () => apiService.loadConfig();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    provideAnimationsAsync(),
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [ApiService],
      multi: true,
    },
  ],
};
