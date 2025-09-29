import { platformBrowser } from '@angular/platform-browser';
import { AppModule } from './app/app-module';
import { environment } from './environments/environment.prod';
import { enableProdMode } from '@angular/core';

platformBrowser().bootstrapModule(AppModule, {
  
})
  .catch(err => console.error(err));

  if (environment.production) {
    enableProdMode();
  }

